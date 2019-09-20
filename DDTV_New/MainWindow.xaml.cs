using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Data;
using Auxiliary;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using static Auxiliary.bilibili;
using static DDTV_New.RoomInit;
using PlayW;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DDTV_New
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static SolidColorBrush 弹幕颜色 = new SolidColorBrush();
        public static SolidColorBrush 字幕颜色 = new SolidColorBrush();
        public static List<PlayW.MainWindow> playList = new List<PlayW.MainWindow>();
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "DDTV2.0主窗口";
            home.Visibility = Visibility.Visible;
            top层.Visibility = Visibility.Collapsed;
            直播层.Visibility = Visibility.Collapsed;
            设置层.Visibility = Visibility.Collapsed;
            关于层.Visibility = Visibility.Collapsed;
            帮助层.Visibility = Visibility.Collapsed;
            插件层.Visibility = Visibility.Collapsed;
            工具层.Visibility = Visibility.Collapsed;
            软件启动配置初始化();
            icon();
            MMPU.弹窗.IcoUpdate += A_IcoUpdate;
         
        }

        public void 软件启动配置初始化()
        {
            #region 配置文件设置
           
            try
            {
                RoomConfigFile = MMPU.getFiles("RoomConfiguration");
            }
            catch (Exception)
            {
                MMPU.setFiles("RoomConfiguration", "./RoomListConfig.json");
                RoomConfigFile = "./RoomListConfig.json";
            }
            //播放窗口默认高度
            try
            {
                MMPU.PlayWindowH = int.Parse(MMPU.getFiles("PlayWindowH"));
            }
            catch (Exception)
            {
                MMPU.setFiles("PlayWindowH", "450");
                MMPU.PlayWindowH = 450;
            }
            //播放窗口默认宽度
            try
            {
                MMPU.PlayWindowW = int.Parse(MMPU.getFiles("PlayWindowW"));
            }
            catch (Exception)
            {
                MMPU.setFiles("PlayWindowW", "800");
                MMPU.PlayWindowH = 800;     
            }
            //直播缓存目录
            try
            {
                MMPU.直播缓存目录 = MMPU.getFiles("Livefile");
            }
            catch (Exception)
            {
                MMPU.setFiles("Livefile", "./tmp/LiveCache/");
                MMPU.直播缓存目录 = "./tmp/LiveCache/";
            }
            //直播缓存目录
            try
            {
                MMPU.下载储存目录 = MMPU.getFiles("file");
            }
            catch (Exception)
            {
                MMPU.setFiles("file", "./tmp/");
                MMPU.下载储存目录 = "./tmp/";
            }
            //直播更新时间
            try
            {
                MMPU.直播更新时间 = int.Parse(MMPU.getFiles("RoomTime"));
            }
            catch (Exception)
            {
                MMPU.setFiles("RoomTime", "40");
                MMPU.直播更新时间 = 40;
            }
            //默认音量
            try
            {
                MMPU.默认音量 = int.Parse(MMPU.getFiles("DefaultVolume"));     
            }
            catch (Exception)
            {
                MMPU.setFiles("DefaultVolume", "50");
                MMPU.默认音量 = 50;
            }
            //缩小功能
            try
            {
                MMPU.缩小功能 = int.Parse(MMPU.getFiles("Zoom"));
            }
            catch (Exception)
            {
                MMPU.setFiles("Zoom", "1");
                MMPU.缩小功能 = 1;
            }
            //最大直播并行数量
            try
            {
                MMPU.最大直播并行数量 = int.Parse(MMPU.getFiles("PlayNum"));
            }
            catch (Exception)
            {
                MMPU.setFiles("PlayNum", "5");
                MMPU.最大直播并行数量 = 5;
            }
            //默认弹幕颜色
            try
            {
                MMPU.默认弹幕颜色 = MMPU.getFiles("DanMuColor");
            }
            catch (Exception)
            {
                MMPU.setFiles("DanMuColor", "0xFF, 0x00, 0x00, 0x00");
                MMPU.默认弹幕颜色 = "0xFF, 0x00, 0x00, 0x00";
            }
            //默认字幕颜色
            try
            {
                MMPU.默认字幕颜色 = MMPU.getFiles("ZiMuColor");
            }
            catch (Exception)
            {
                MMPU.setFiles("ZiMuColor", "0xFF, 0x00, 0x00, 0x00");
                MMPU.默认字幕颜色 = "0xFF, 0x00, 0x00, 0x00";
            }
            //默认字幕大小
            try
            {
                MMPU.默认字幕大小 = int.Parse(MMPU.getFiles("ZiMuSize"));
            }
            catch (Exception)
            {
                MMPU.setFiles("ZiMuSize", "24");
                MMPU.默认字幕大小 = 24;
            }
            //默认弹幕大小
            try
            {
                MMPU.默认弹幕大小 = int.Parse(MMPU.getFiles("DanMuSize"));
            }
            catch (Exception)
            {
                MMPU.setFiles("DanMuSize", "20");
                MMPU.默认弹幕大小 = 20;
            }
            #endregion
            //初始化房间
            RoomInit.start();
            //公告加载线程
            new Thread(new ThreadStart(delegate {
                公告项目启动();
            })).Start();
            //房间刷新线程
            new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    刷新房间列表UI();
                    Thread.Sleep(5 * 1000);
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        newtime.Content = "数据更新时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }));
                    while (true)
                    {
                        if (房间信息更新次数 > 0)
                        {
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                首次更新.Visibility = Visibility.Collapsed;
                            }));
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }
            })).Start();
            //延迟测试
            new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    try
                    {
                        double 国内 = MMPU.测试延迟("https://live.bilibili.com");
                        string 国内延迟 = string.Empty;
                        string 国外延迟 = string.Empty;
                        if (国内 > 0)
                        {
                            国内延迟 = "国内服务器延迟(阿B):" + 国内.ToString().Split('.')[0] + "ms";
                            MMPU.是否能连接阿B = true;
                        }
                        else
                        {
                            国内延迟 = "国内服务器延迟(阿B): 连接超时";
                            MMPU.是否能连接阿B = false;
                        }

                        if (MMPU.连接404使能)
                        {
                            double 国外 = MMPU.测试延迟("https://www.google.com");
                            if (国外 > 0)
                            {
                                国外延迟 = "国内服务器延迟(404):" + 国外.ToString().Split('.')[0] + "ms";
                                MMPU.是否能连接404 = true;
                            }
                            else
                            {
                                国外延迟 = "国内服务器延迟(404): 连接超时";
                                MMPU.是否能连接404 = false;
                            }

                        }
                        else
                        {
                            MMPU.是否能连接404 = false;
                            国外延迟 = "";
                        }
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            国内服务器延迟.Content = 国内延迟;
                        }));
                        if (MMPU.连接404使能)
                        {
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                国外服务器延迟.Content = 国外延迟;
                            }));

                        }
                        else
                        {
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                国外服务器延迟.Content = "";
                            }));

                        }

                    }
                    catch (Exception) { }
                    Thread.Sleep(4000);
                }
            })).Start();
            //缩小功能
            {
                MMPU.缩小功能 = int.Parse(MMPU.getFiles("Zoom"));
                if (MMPU.缩小功能 == 1)
                {
                    缩小到任务栏选择按钮.IsChecked = true;
                }
                else
                {
                    隐藏到后台托盘选择按钮.IsChecked = true;
                }
            }
            //加载配置文件
            {
                默认音量值显示.Content = MMPU.默认音量;
                修改默认音量.Value = MMPU.默认音量;
                默认下载路径.Text = MMPU.下载储存目录;
                //读取字幕弹幕颜色
                {
                    SolidColorBrush S1 = new SolidColorBrush(Color.FromArgb(0xFF, Convert.ToByte(MMPU.默认字幕颜色.Split(',')[1], 16), Convert.ToByte(MMPU.默认字幕颜色.Split(',')[2], 16), Convert.ToByte(MMPU.默认字幕颜色.Split(',')[3], 16)));
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        字幕默认颜色.Foreground = S1;
                        字幕颜色 = S1;
                    }));
                    SolidColorBrush S2 = new SolidColorBrush(Color.FromArgb(0xFF, Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[1], 16), Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[2], 16), Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[3], 16)));
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        弹幕默认颜色.Foreground = S2;
                        弹幕颜色 = S2;
                    }));
                }
                //读取字幕弹幕字体大小
                {
                    字幕文字大小.Text = MMPU.默认字幕大小.ToString();
                    弹幕文字大小.Text = MMPU.默认弹幕大小.ToString();
                }
                //默认音量
                {
                    修改默认音量.Value = MMPU.默认音量;
                }
                //播放窗口默认大小
                {
                    默认播放宽度.Text = MMPU.PlayWindowW.ToString();
                    默认播放高度.Text = MMPU.PlayWindowH.ToString();
                }
            }
            //增加插件列表
            {
                PluginC.Items.Add(new
                {
                    编号 = "1",
                    名称 = "bilibili数据接口插件",
                    版本 = "1.0.1.1",
                    是否加载 = "√",
                    说明 = "获取和处理来自阿B的直播数据流",
                    备注 = ""
                });
                PluginC.Items.Add(new
                {
                    编号 = "2",
                    名称 = "播放插件",
                    版本 = "1.0.0.4",
                    是否加载 = "√",
                    说明 = "用于播放直播视频流",
                    备注 = ""
                });
                PluginC.Items.Add(new
                {
                    编号 = "3",
                    名称 = "DDNA直播统计插件",
                    版本 = "1.0.0.1",
                    是否加载 = "√",
                    说明 = "用于获取目前已知正在直播的vtb列表(工具箱内)",
                    备注 = ""
                });
            }

            //剪切板监听，用于播放主站视频

           // Thread T1 = new Thread(new ThreadStart(delegate
           //{
           //    while (true)
           //    {
           //        try
           //        {
           //            System.Windows.IDataObject iData = System.Windows.Clipboard.GetDataObject();
           //            string A = (string)iData.GetData(System.Windows.DataFormats.Text);
           //            if (!string.IsNullOrEmpty(A))
           //            {
           //                if (A.Substring(0, 4).ToLower() == "http" && A.Contains("www.bilibili.com/video/"))
           //                {
           //                    string C = A.Replace("www.bilibili.com/video/", "㈨").Split('㈨')[1].Split('/')[0].ToLower().Replace("av", "");
           //                    System.Windows.Clipboard.SetDataObject("");
           //                     // string AB = MMPU.获取网页数据_下载视频用("https://www.bilibili.com/video/av68188405");
           //                     string CIDstr = MMPU.获取网页数据_下载视频用("https://www.bilibili.com/widget/getPageList?aid=" + C, true);
           //                    JArray JO1 = (JArray)JsonConvert.DeserializeObject(CIDstr);
           //                    string CID = JO1[0]["cid"].ToString();
           //                    string 下载地址 = MMPU.获取网页数据_下载视频用("https://api.bilibili.com/x/player/playurl?avid=" + C + "&cid=" + "118184249" + "&otype=json&qn=116", false);
           //                    JObject JO2 = (JObject)JsonConvert.DeserializeObject(下载地址);
           //                    下载地址 = JO2["data"]["durl"][0]["url"].ToString();
           //                    Downloader 下载对象 = new Downloader
           //                    {
           //                        DownIofo = new Downloader.DownIofoData() { 平台 = "主站视频", 房间_频道号 = "主站视频", 标题 = "主站视频", 事件GUID = Guid.NewGuid().ToString(), 下载地址 = 下载地址, 备注 = "主站视频播放", 是否保存 = false }
           //                    };
           //                     //Downloader 下载对象 = Downloader.新建下载对象(平台, 唯一码, 标题, GUID, 下载地址, "视频播放缓存", false);

           //                     Task.Run(() =>
           //                    {
           //                        this.Dispatcher.Invoke(new Action(delegate
           //                        {
           //                            打开直播列表(下载对象);
           //                            MMPU.当前直播窗口数量++;
           //                            等待框.Visibility = Visibility.Collapsed;
           //                        }));
           //                    });
           //                }
           //            }
           //        }
           //        catch (Exception ex)
           //        {

           //        }
           //        Thread.Sleep(500);
           //    }
           //}));
           // T1.TrySetApartmentState(ApartmentState.STA);
           // T1.Start();

            this.Dispatcher.Invoke(new Action(delegate
            {
                版本显示.Content = "版本：" + MMPU.版本号;
            }));
            
        }
        /// <summary>
        /// 通过get方式返回内容
        /// </summary>
        /// <param name="url">目标网页地址</param>
        /// <param name="headername">http标头名</param>
        /// <param name="headervl">http标头值</param>
        /// <returns></returns>
        public static string get返回网页内容(string url)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        public void 公告项目启动()
        {
            //动态推送1
            new Thread(new ThreadStart(delegate
            {
                try
                {

                    while (true)
                    {
                        bool 动态推送1开关 = MMPU.TcpSend(20009, "{}", true) == "1" ? true : false;
                        if (动态推送1开关)
                        {
                            string 动态推送内容 = MMPU.TcpSend(20010, "{}", true);
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                动态推送1.Content = 动态推送内容;
                            }));
                        }
                        Thread.Sleep(300 * 1000);
                    }
                }
                catch (Exception) { }
            })).Start();
            //版本检查
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    string 服务器版本号 = MMPU.TcpSend(20011, "{}", true);
                    if (!string.IsNullOrEmpty(服务器版本号))
                    {
                        if (MMPU.版本号 != MMPU.TcpSend(20011, "{}", true))
                        {
                            MessageBoxResult dr = System.Windows.MessageBox.Show("检测到版本更新,更新公告:\n" + MMPU.TcpSend(20012, "{}", true)+ "\n\n点击确定跳转到补丁(大约1MB)下载网页，点击取消忽略", "有新版本", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                            if (dr == MessageBoxResult.OK)
                            {
                                System.Diagnostics.Process.Start("https://github.com/CHKZL/DDTV2/releases/latest");
                            }
                        }
                    }
                }
                catch (Exception) { }
            })).Start();
            //推送内容1
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    string 推送内容1text = MMPU.TcpSend(20005, "{}", true);
                    if (推送内容1text.Length < 25)
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            推送内容1.Content = 推送内容1text;
                        }));
                    }

                }
                catch (Exception) { }
            })).Start();
        }
        private void 刷新房间列表UI()
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                LiveList.Items.Clear();
            }));
            InitRoomList();
        }

        public void InitRoomList()
        {
            List<RoomInfo> 正在直播 = new List<RoomInfo>();
            List<RoomInfo> 未直播 = new List<RoomInfo>();
            foreach (var item in bilibili.RoomList)
            {
                if (item.直播状态)
                {
                    正在直播.Add(item);
                }
                else
                {
                    未直播.Add(item);
                }
            }
            int i = 1;
            foreach (var item in 正在直播)
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    LiveListAdd(i, item.名称, item.直播状态, "bilibili", item.是否提醒, item.是否录制视频, item.房间号, item.原名);
                    i++;
                }));

            }
            foreach (var item in 未直播)
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    LiveListAdd(i, item.名称, item.直播状态, "bilibili", item.是否提醒, item.是否录制视频, item.房间号, item.原名);
                    i++;
                }));

            }
            if (正在直播.Count == 0)
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    tabText.Content = "监控列表中没有直播中的单推对象";
                }));

            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    tabText.Content = "在监控列表中有" + 正在直播.Count + "个单推对象正在直播";
                }));

            }
            this.Dispatcher.Invoke(new Action(delegate
            {
                ppnum.Content = i - 1;
                等待框.Visibility = Visibility.Collapsed;
            }));

        }
        public void LiveListAdd(int 编号, string 名称, bool 状态, string 平台, bool 直播提醒, bool 是否录制, string 唯一码, string 原名)
        {
            LiveList.Items.Add(new { 编号 = 编号, 名称 = 名称, 状态 = 状态 ? "●直播中" : "○未直播", 平台 = 平台, 是否提醒 = 直播提醒 ? "√" : "", 是否录制 = 是否录制 ? "√" : "", 唯一码 = 唯一码, 原名 = 原名 });
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
            }
        }


        private void 工具箱_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("工具层");
        }
        private void 帮助_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("帮助层");
        }
        private void DDNA列表_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("DDNA列表");
        }
        private void 关于_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("关于层");
        }

        private void 关闭按钮_Click(object sender, MouseButtonEventArgs e)
        {
            DDTV关闭事件();
        }
        public static void DDTV关闭事件()
        {

            MessageBoxResult dr = System.Windows.MessageBox.Show("确定退出DDTV？", "退出", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        FileInfo[] files = new DirectoryInfo("./tmp/LiveCache/").GetFiles();
                        foreach (var item in files)
                        {
                            MMPU.文件删除委托("./tmp/LiveCache/" + item.Name);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    Environment.Exit(0);
                })).Start();
            }
        }
        NotifyIcon notifyIcon;
        private void 最小化按钮_Click(object sender, MouseButtonEventArgs e)
        {
            if (MMPU.缩小功能 == 1)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                this.Hide();
            }
        }

        private void A_IcoUpdate(object sender, EventArgs e)
        {
            MMPU.弹窗提示 A = (MMPU.弹窗提示)sender;
            this.notifyIcon.ShowBalloonTip(A.持续时间, A.标题, A.内容, ToolTipIcon.None);
        }

        private void icon()
        {
            this.notifyIcon = new NotifyIcon
            {
                BalloonTipText = "DDTV已启动", //设置程序启动时显示的文本
                Text = "DDTV",//最小化到托盘时，鼠标点击时显示的文本
                Icon = new System.Drawing.Icon("DDTV.ico"),//程序图标
                Visible = true
            };
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
            this.notifyIcon.ShowBalloonTip(1000);
        }
        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }
        private void 返回首页_点击事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("home");
        }
        private void 切换界面(string name)
        {
            home.Visibility = Visibility.Collapsed;
            top层.Visibility = Visibility.Collapsed;
            直播层.Visibility = Visibility.Collapsed;
            设置层.Visibility = Visibility.Collapsed;
            关于层.Visibility = Visibility.Collapsed;
            帮助层.Visibility = Visibility.Collapsed;
            插件层.Visibility = Visibility.Collapsed;
            工具层.Visibility = Visibility.Collapsed;
            AOE直播层.Visibility = Visibility.Collapsed;
            switch (name)
            {
                case "home":
                    home.Visibility = Visibility.Visible;
                    break;
                case "直播层":
                    top层.Visibility = Visibility.Visible;
                    直播层.Visibility = Visibility.Visible;
                    break;
                case "设置层":
                    top层.Visibility = Visibility.Visible;
                    设置层.Visibility = Visibility.Visible;
                    break;
                case "关于层":
                    top层.Visibility = Visibility.Visible;
                    关于层.Visibility = Visibility.Visible;
                    break;
                case "帮助层":
                    top层.Visibility = Visibility.Visible;
                    帮助层.Visibility = Visibility.Visible;
                    break;
                case "插件层":
                    top层.Visibility = Visibility.Visible;
                    插件层.Visibility = Visibility.Visible;
                    break;
                case "工具层":
                    top层.Visibility = Visibility.Visible;
                    工具层.Visibility = Visibility.Visible;
                    break;
                case "DDNA列表":
                    top层.Visibility = Visibility.Visible;
                    AOE直播层.Visibility = Visibility.Visible;
                    break;
            }

        }
        private void 默认音量修改事件(object sender, System.Windows.Input.MouseEventArgs e)
        {
            默认音量值显示.Content = (int)修改默认音量.Value;
            MMPU.默认音量 = (int)修改默认音量.Value;
            MMPU.修改默认音量设置(MMPU.默认音量);
        }
        private void 关注列表_点击事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("直播层");
        }
        private void 设置层_点击事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("设置层");
        }
        private void 插件_点击事件(object sender, MouseButtonEventArgs e)
        {
            切换界面("插件层");
        }

        private void 直播表单击事件(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ListView LV = (System.Windows.Controls.ListView)sender;
                if (LV.Items.Count != 0)
                {
                    已选内容 = LV.SelectedItems[0].ToString();
                    选中内容1.Content = MMPU.获取livelist平台和唯一码.平台(已选内容) + "\n" + MMPU.获取livelist平台和唯一码.唯一码(已选内容) + "\n" + MMPU.获取livelist平台和唯一码.名称(已选内容);
                }
                //Console.WriteLine("已选内容");
            }
            catch (Exception)
            {
            }
        }
        public static string 已选内容 = "";
        private void 添加直播列表按钮点击事件(object sender, RoutedEventArgs e)
        {
            等待框.Visibility = Visibility.Visible;
            AddMonitoringList AML = new AddMonitoringList("添加新单推", "", "", "", "", false);
            AML.ShowDialog();

        }
        private void 修改直播列表按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(已选内容))
            {
                System.Windows.MessageBox.Show("未选择");
                return;
            }
            等待框.Visibility = Visibility.Visible;
            AddMonitoringList AML = new AddMonitoringList("修改单推属性", MMPU.获取livelist平台和唯一码.名称(已选内容), MMPU.获取livelist平台和唯一码.原名(已选内容), MMPU.获取livelist平台和唯一码.平台(已选内容), MMPU.获取livelist平台和唯一码.唯一码(已选内容), MMPU.获取livelist平台和唯一码.状态(已选内容) == "●直播中" ? true : false);
            AML.ShowDialog();
        }

        private void AOE直播表双击事件(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Controls.ListView LV = (System.Windows.Controls.ListView)sender;
                string A = LV.SelectedItems[0].ToString();
                A = A.Replace("\"", "").Replace(" ", "").Replace("{", "").Replace("}", "");
                string[] B = A.Split(',');
                string 直播地址 = B[4].Replace("直播URL=", "");
                if(string.IsNullOrEmpty(直播地址))
                {
                    return;
                }
                System.Diagnostics.Process.Start(直播地址);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("打开这个直播发生错误，可能是由于无法连接目标网页");
            }
        }
        private void 直播表双击事件(object sender, MouseButtonEventArgs e)
        {
            if (MMPU.当前直播窗口数量 >= MMPU.最大直播并行数量)
            {
                System.Windows.MessageBox.Show("达到了设置的最大直播窗口数量,新建直播窗口失败");
                return;
            }
            等待框.Visibility = Visibility.Visible;
            System.Windows.Controls.ListView LV = (System.Windows.Controls.ListView)sender;
            try
            {
                if (!bilibili.GetRoomInfo(MMPU.获取livelist平台和唯一码.唯一码(LV.SelectedItems[0].ToString())).直播状态)
                {
                    System.Windows.MessageBox.Show("该房间未直播");
                    等待框.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            catch (Exception)
            {
                等待框.Visibility = Visibility.Collapsed;
                return;
            }
            string 平台 = MMPU.获取livelist平台和唯一码.平台(LV.SelectedItems[0].ToString());
            string 唯一码 = MMPU.获取livelist平台和唯一码.唯一码(LV.SelectedItems[0].ToString());
            string 标题 = "";
            try
            {
                switch (平台)
                {
                    case "bilibili":
                        {

                            string GUID = Guid.NewGuid().ToString();
                            foreach (var item in bilibili.RoomList)
                            {
                                if (item.房间号 == 唯一码)
                                {
                                    标题 = item.标题;
                                }
                            }
                            string 下载地址 = string.Empty;
                            try
                            {
                                下载地址 = bilibili.根据房间号获取房间信息.下载地址(唯一码);
                            }
                            catch (Exception)
                            {
                                System.Windows.MessageBox.Show("获取下载地址错误");
                                return;
                            }
                            Downloader 下载对象 = new Downloader
                            {
                                DownIofo = new Downloader.DownIofoData() { 平台 = 平台, 房间_频道号 = 唯一码, 标题 = 标题, 事件GUID = GUID, 下载地址 = 下载地址, 备注 = "视频播放缓存", 是否保存 = false }
                            };
                            //Downloader 下载对象 = Downloader.新建下载对象(平台, 唯一码, 标题, GUID, 下载地址, "视频播放缓存", false);

                            Task.Run(() =>
                            {
                                this.Dispatcher.Invoke(new Action(delegate
                                {
                                    打开直播列表(下载对象);
                                    MMPU.当前直播窗口数量++;
                                    等待框.Visibility = Visibility.Collapsed;
                                }));
                            });
                            break;
                        }
                    default:
                        System.Windows.MessageBox.Show("发现了与当前版本不支持的平台，请检查更新");
                        return;
                }
            }
            catch (Exception ex)
            {
                错误窗 ERR = new 错误窗("新建播放窗口发生意外错误，请重试", "新建播放窗口发生意外错误，请重试\n" + ex.ToString());
                ERR.ShowDialog();
                return;
            }
        }
        public void 打开直播列表(Downloader DL)
        {
            DL.DownIofo.播放状态 = true;
            DL.DownIofo.是否是播放任务 = true;
            PlayW.MainWindow PlayWindow = new PlayW.MainWindow(DL, MMPU.默认音量, 弹幕颜色, 字幕颜色,MMPU.默认弹幕大小,MMPU.默认字幕大小,MMPU.PlayWindowW,MMPU.PlayWindowH);
            PlayWindow.Closed += 播放窗口退出事件;
            PlayWindow.Show();
            PlayWindow.BossKey += 老板键事件;
            playList.Add(PlayWindow);
            MMPU.ClearMemory();
        }

        private void 老板键事件(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            foreach (var item in playList)
            {
                if(item.窗口是否打开)
                {
                    item.WindowState = WindowState.Minimized;
                }
            }
        }

        private void 播放窗口退出事件(object sender, EventArgs e)
        {
            try
            {
                new Thread(new ThreadStart(delegate {
                    MMPU.当前直播窗口数量--;
                    PlayW.MainWindow p = (PlayW.MainWindow)sender;
                    foreach (var item in MMPU.DownList)
                    {
                        if (item.DownIofo.事件GUID == p.DD.DownIofo.事件GUID)
                        {
                            item.DownIofo._wc.CancelAsync();
                            item.DownIofo.下载状态 = false;
                            item.DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                            if (item.DownIofo.是否保存)
                            {

                            }
                            else
                            {
                                MMPU.文件删除委托(p.DD.DownIofo.文件保存路径);
                            }
                            break;
                        }
                    }
                })).Start();
            }
            catch (Exception)
            {


            }
        }

        private void 直播列表删除按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(已选内容))
            {
                System.Windows.MessageBox.Show("未选择");
                return;
            }
            等待框.Visibility = Visibility.Visible;
            var rlc2 = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
            foreach (var item in rlc2.data)
            {
                if (item.RoomNumber == MMPU.获取livelist平台和唯一码.唯一码(已选内容))
                {
                    rlc2.data.Remove(item);
                    break;
                }
            }

            string JOO = JsonConvert.SerializeObject(rlc2);
            MMPU.储存文本(JOO, RoomConfigFile);
            InitializeRoomList();
            //更新房间列表(MMPU.获取livelist平台和唯一码(已选内容, "平台"), MMPU.获取livelist平台和唯一码(已选内容, "唯一码"),0);
            System.Windows.MessageBox.Show("删除完成");

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void 修改录制状态点击事件(object sender, RoutedEventArgs e)
        {
            修改列表设置(true);

        }

        private void 修改提醒状态点击事件(object sender, RoutedEventArgs e)
        {
            修改列表设置(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">T修改录制设置，F修改提醒设置</param>
        public void 修改列表设置(bool a)
        {
            Console.WriteLine(已选内容);
            bool 是否改过 = false;
            if (string.IsNullOrEmpty(已选内容))
            {
                System.Windows.MessageBox.Show("未选择");
                return;
            }
            //编号 = 1, 名称 = 智障爱, 状态 = ○未直播, 平台 = bilibili, 是否提醒 = √, 是否录制 = , 唯一码 = 1485080, 原名 = 
            等待框.Visibility = Visibility.Visible;
            RoomBox RB = new RoomBox
            {
                data = new List<RoomCadr>()
            };
            while (RoomInit.房间主表长度 != 房间主表.Count() && RoomInit.房间主表长度 != 0)
            {
                Thread.Sleep(10);
            }
            int rlclen = 房间主表.Count();
            for (int i = 0; i < rlclen; i++)
            {
                if (房间主表[i].唯一码 == MMPU.获取livelist平台和唯一码.唯一码(已选内容))
                {
                    if (!是否改过)
                    {
                        是否改过 = true;

                        房间主表.Remove(房间主表[i]);
                        rlclen--;
                        i--;
                        bool 是否录制 = MMPU.获取livelist平台和唯一码.是否录制(已选内容) == "√" ? true : false;
                        bool 是否提醒 = MMPU.获取livelist平台和唯一码.是否提醒(已选内容) == "√" ? true : false;
                        if (a)
                        {
                            是否录制 = !是否录制;
                            if (是否录制)
                            {
                                已选内容 = 已选内容.Replace("是否录制 = ", "是否录制 = √");
                            }
                            else
                            {
                                已选内容 = 已选内容.Replace("是否录制 = √", "是否录制 = ");
                            }
                        }
                        else
                        {
                            是否提醒 = !是否提醒;
                            if (是否提醒)
                            {
                                已选内容 = 已选内容.Replace("是否提醒 = ", "是否提醒 = √");
                            }
                            else
                            {
                                已选内容 = 已选内容.Replace("是否提醒 = √", "是否提醒 = ");
                            }
                        }

                        RB.data.Add(new RoomCadr
                        {
                            Name = MMPU.获取livelist平台和唯一码.名称(已选内容),
                            RoomNumber = MMPU.获取livelist平台和唯一码.唯一码(已选内容),
                            Types = MMPU.获取livelist平台和唯一码.平台(已选内容),
                            RemindStatus = 是否提醒,
                            status = MMPU.获取livelist平台和唯一码.状态(已选内容) == "●直播中" ? true : false,
                            VideoStatus = 是否录制,
                            OfficialName = MMPU.获取livelist平台和唯一码.原名(已选内容),
                            LiveStatus = MMPU.获取livelist平台和唯一码.状态(已选内容) == "●直播中" ? true : false
                        });
                    }
                }
                else
                {
                    RB.data.Add(new RoomCadr() { LiveStatus = 房间主表[i].直播状态, Name = 房间主表[i].名称, OfficialName = 房间主表[i].原名, RoomNumber = 房间主表[i].唯一码, VideoStatus = 房间主表[i].是否录制, Types = 房间主表[i].平台, RemindStatus = 房间主表[i].是否提醒, status = false });
                    if (RoomInit.根据唯一码获取直播状态(房间主表[i].唯一码))
                    {
                        RB.data[RB.data.Count() - 1].LiveStatus = true;
                    }
                }
            }
            string JOO = JsonConvert.SerializeObject(RB);
            MMPU.储存文本(JOO, RoomConfigFile);
            InitializeRoomList();
            //更新房间列表(平台.SelectedItem.ToString(), 唯一码.Text,2);
            //System.Windows.MessageBox.Show("修改成功");

        }

        private void 录制按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(已选内容))
            {
                System.Windows.MessageBox.Show("未选择");
                return;
            }
            new Thread(new ThreadStart(delegate {
                switch (MMPU.获取livelist平台和唯一码.平台(已选内容))
                {
                    case "bilibili":
                        if (!bilibili.根据房间号获取房间信息.是否正在直播(MMPU.获取livelist平台和唯一码.唯一码(已选内容)))
                        {
                            System.Windows.MessageBox.Show("该房间当前未直播");
                            return;
                        }
                        break;
                    default:
                        System.Windows.MessageBox.Show("发现了与当前版本不支持的平台，请检查更新");
                        return;
                }
                string GUID = Guid.NewGuid().ToString();
                string 标题 = bilibili.根据房间号获取房间信息.获取标题(MMPU.获取livelist平台和唯一码.唯一码(已选内容));
                string 下载地址 = string.Empty;
                try
                {
                    下载地址 = bilibili.根据房间号获取房间信息.下载地址(MMPU.获取livelist平台和唯一码.唯一码(已选内容));
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("获取下载地址失败");
                    return;
                }
                Downloader 下载对象 = Downloader.新建下载对象(MMPU.获取livelist平台和唯一码.平台(已选内容), MMPU.获取livelist平台和唯一码.唯一码(已选内容), 标题, GUID, 下载地址, "手动下载任务", true);

                System.Windows.MessageBox.Show("下载任务添加完成");
            })).Start();
        }

        private void 显示下载队列按钮点击事件(object sender, RoutedEventArgs e)
        {
            下载工具 A = new 下载工具();
            A.Show();
        }

        private void 跳转到网页按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(已选内容))
            {
                System.Windows.MessageBox.Show("未选择");
                return;
            }
            switch (MMPU.获取livelist平台和唯一码.平台(已选内容))
            {
                case "bilibili":
                    System.Diagnostics.Process.Start("https://live.bilibili.com/" + MMPU.获取livelist平台和唯一码.唯一码(已选内容));
                    break;
                default:
                    System.Windows.MessageBox.Show("发现了与当前版本不支持的平台，请检查更新");
                    return;
            }

        }

        private void Slider_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void 修改默认下载目录按钮事件(object sender, RoutedEventArgs e)
        {


            FolderBrowserDialog P_File_Folder = new FolderBrowserDialog();

            if (P_File_Folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MMPU.下载储存目录 = P_File_Folder.SelectedPath;
                默认下载路径.Text = MMPU.下载储存目录;
                MMPU.setFiles("file", MMPU.下载储存目录);

            }
        }

        private void 缩小_任务栏选择事件(object sender, RoutedEventArgs e)
        {
            MMPU.缩小功能 = 1;
            MMPU.setFiles("Zoom", "1");
        }

        private void 缩小_后台托盘选择事件(object sender, RoutedEventArgs e)
        {
            MMPU.缩小功能 = 0;
            MMPU.setFiles("Zoom", "0");
        }

        private void 连接404类使能开关点击事件(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine(MMPU.测试延迟("https://www.google.com"));
            if (连接404类使能开关.IsChecked == true)
            {
                MMPU.连接404使能 = true;
                MMPU.是否能连接404 = true;
            }
            else
            {

                MMPU.连接404使能 = false;
                MMPU.是否能连接404 = false;
            }
        }

        private void 修改最大直播并行数量确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            MMPU.最大直播并行数量 = int.Parse(并行直播数量.Text);
            MMPU.setFiles("PlayNum", 并行直播数量.Text);
            System.Windows.MessageBox.Show("修改成功");
        }
        private void 修改弹幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                弹幕默认颜色.Foreground = solidColorBrush;
                MMPU.默认弹幕颜色 = solidColorBrush.Color.A.ToString("X2") + "," + solidColorBrush.Color.R.ToString("X2") + "," + solidColorBrush.Color.G.ToString("X2") + "," + solidColorBrush.Color.B.ToString("X2");
                MMPU.setFiles("DanMuColor", MMPU.默认弹幕颜色);
                弹幕颜色 = solidColorBrush;
            }
        }
        private void 修改字幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                字幕默认颜色.Foreground = solidColorBrush;
                MMPU.默认字幕颜色 = solidColorBrush.Color.A.ToString("X2") + "," + solidColorBrush.Color.R.ToString("X2") + "," + solidColorBrush.Color.G.ToString("X2") + "," + solidColorBrush.Color.B.ToString("X2");
                MMPU.setFiles("ZiMuColor", MMPU.默认字幕颜色);
                字幕颜色 = solidColorBrush;
            }
        }
        private void 并行直播数量_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            检测输入框是否为数字((System.Windows.Controls.TextBox)sender,30);     
        }
        private void 弹幕文字大小_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            检测输入框是否为数字((System.Windows.Controls.TextBox)sender,30);
        }
        private void 修改弹幕文字大小确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            MMPU.默认弹幕大小 = int.Parse(弹幕文字大小.Text);
            MMPU.setFiles("DanMuSize", 弹幕文字大小.Text);
            System.Windows.MessageBox.Show("修改成功");
        }
        private void 字幕文字大小_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            检测输入框是否为数字((System.Windows.Controls.TextBox)sender,99);
        }
        private void 分辨率大小_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            检测输入框是否为数字((System.Windows.Controls.TextBox)sender,4000);
        }
        private void 修改字幕文字大小确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            MMPU.默认字幕大小 = int.Parse(字幕文字大小.Text);
            MMPU.setFiles("ZiMuSize", 字幕文字大小.Text);
            System.Windows.MessageBox.Show("修改成功");
        }
        private void 修改播放器默认大小确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            MMPU.PlayWindowW = int.Parse(默认播放宽度.Text);
            MMPU.setFiles("PlayWindowW", 默认播放宽度.Text);
            MMPU.PlayWindowH = int.Parse(默认播放高度.Text);
            MMPU.setFiles("PlayWindowH", 默认播放高度.Text);

            System.Windows.MessageBox.Show("修改成功");
        }
        private void 检测输入框是否为数字(System.Windows.Controls.TextBox A,int max)
        {
            if (!string.IsNullOrEmpty(A.Text))
            {
                try
                {
                    int.Parse(A.Text);
                    if (int.Parse(A.Text) >= max)
                    {
                        A.Text = (max - 1).ToString();
                    }  
                   
                }
                catch (Exception)
                {
                    A.Text = A.Text.Substring(0, A.Text.Length - 1);
                }
            }
        }

        private void 刷新AOE直播列表按钮点击事件_Click(object sender, RoutedEventArgs e)
        {
            刷新AOE直播列表按钮.IsEnabled = false;
            刷新AOE直播列表按钮.Content = "刷新中....";
            new Thread(new ThreadStart(delegate {
                try
                {
                    外部API.正在直播数据 直播数据 = new 外部API.正在直播数据();
                    直播数据.更新正在直播数据();
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        bilibiliAOE直播列表.Items.Clear();
                        youtubeAOE直播列表.Items.Clear();
                        tcAOE直播列表.Items.Clear();
                    }));
                    foreach (var item in 直播数据.直播数据)
                    {
                        string 时间 = MMPU.将时间戳转换为日期类型(item.实际开始时间);
                        if (!string.IsNullOrEmpty(item.直播连接))
                        {
                            switch (item.频道类型)
                            {
                                case 4:

                                    this.Dispatcher.Invoke(new Action(delegate
                                    {
                                        bilibiliAOE直播列表.Items.Add(new { 名称 = item.主播名称, 标题 = item.标题, 观看人数 = item.当前观众, 直播开始时间 = 时间, 直播URL = item.直播连接 });
                                    }));
                                    break;
                                case 1:
                                    this.Dispatcher.Invoke(new Action(delegate
                                    {
                                        youtubeAOE直播列表.Items.Add(new { 名称 = item.主播名称, 标题 = item.标题, 观看人数 = item.当前观众, 直播开始时间 = 时间, 直播URL = item.直播连接 });
                                    }));
                                    break;
                                case 8:
                                    this.Dispatcher.Invoke(new Action(delegate
                                    {
                                        tcAOE直播列表.Items.Add(new { 名称 = item.主播名称, 标题 = item.标题, 观看人数 = item.当前观众, 直播开始时间 = 时间, 直播URL = item.直播连接 });
                                    }));
                                    break;
                            }
                        }
                    }
                    if (bilibiliAOE直播列表.Items.Count == 0)
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            bilibiliAOE直播列表.Items.Add(new { 名称 = "", 标题 = "当前没有在BILIBILI直播的VTB", 观看人数 = "", 直播开始时间 = "", 直播URL = "" });
                        }));
                    }
                    if (youtubeAOE直播列表.Items.Count == 0)
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            youtubeAOE直播列表.Items.Add(new { 名称 = "", 标题 = "当前没有在YouTuBe直播的VTB", 观看人数 = "", 直播开始时间 = "", 直播URL = "" });
                        }));
                    }
                    if (tcAOE直播列表.Items.Count == 0)
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            tcAOE直播列表.Items.Add(new { 名称 = "", 标题 = "当前没有在TwitCasting直播的VTB", 观看人数 = "", 直播开始时间 = "", 直播URL = "" });
                        }));
                    }
                }
                catch (Exception)
                {
                    
                }
                this.Dispatcher.Invoke(new Action(delegate
                {
                    刷新AOE直播列表按钮.IsEnabled = true;
                    刷新AOE直播列表按钮.Content = "刷新列表";
                }));
            })).Start();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
    
}
