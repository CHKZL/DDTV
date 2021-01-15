using Auxiliary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using static Auxiliary.bilibili;
using MessageBox = System.Windows.MessageBox;
using static Auxiliary.RoomInit;
using Clipboard = System.Windows.Clipboard;
using ReactiveUI;
using System.Reactive.Disposables;
using DDTV_New.Utility;
using System.Diagnostics;
using DDTV_New.window;
using System.Configuration;

namespace DDTV_New
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        public static SolidColorBrush 弹幕颜色 = new SolidColorBrush();
        public static SolidColorBrush 字幕颜色 = new SolidColorBrush();
        public static List<PlayW.MainWindow> playList1 = new List<PlayW.MainWindow>();
        public static int 播放器版本 = 1;
        //  public static List<硬解播放器.Form1> playList2 = new List<硬解播放器.Form1>();

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainWindow));
        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public MainWindow()
        {
            //DDTV主窗口启动
            InitializeComponent();
            this.Title = "DDTV2.0主窗口";
            //if (MMPU.系统内核版本 < 10)
            //{
            //    MessageBox.Show($"启动失败，无法在该系统环境下运行");
            //    InfoLog.InfoPrintf("不是WIN10，拒绝系统调用，调用关闭程序，退出", InfoLog.InfoClass.系统错误信息);
            //    DDTV关闭事件();
            //    return;
            //}
                ViewModel = new MainViewModel();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel, vm => vm.TanoshiNum, v => v.TanoshiNumLabel.Content)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, vm => vm.TabText, v => v.TabTextLabel.Content)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, vm => vm.PushNotification, v => v.动态推送1.Content)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, vm => vm.ServerDelayBilibiliText, v => v.国内服务器延迟.Content)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, vm => vm.serverVdbText, v => v.数据源服务器延迟.Content)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, vm => vm.ServerDelayYoutubeText, v => v.国外服务器延迟.Content)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, vm => vm.LatestDataUpdateTimeText, v => v.newtime.Content)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, vm => vm.Announcement, v => v.推送内容1.Text)
                    .DisposeWith(disposable);
            });

            _grids = new List<Grid>
            {
                home,
                top层,
                直播层,
                设置层,
                关于层,
                帮助层,
                插件层,
                工具层,
                AOE直播层,
            };

            切换界面(home);

            软件启动配置初始化();

            #region 命令行参数处理
            Dictionary<string, string> arguments;
            try
            {
                arguments = ArgumentParser.parse(Environment.GetCommandLineArgs());
                if (arguments.ContainsKey("m") ||
                    (arguments.ContainsKey("minimized") && arguments["minimized"] == "true"))
                {
                    minimizeWindow();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("命令行参数无效，请检查");
            }
            #endregion

            icon();
            MMPU.弹窗.IcoUpdate += A_IcoUpdate;

            ServicePointManager.ServerCertificateValidationCallback +=
#pragma warning disable CA5359 // Do Not Disable Certificate Validation
            delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true; // **** Always accept
            };
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
            ServicePointManager.DefaultConnectionLimit = 999;
            ServicePointManager.MaxServicePoints = 999;

            if (MMPU.是否第一次使用DDTV && string.IsNullOrEmpty(MMPU.Cookie))
            {
                // 启动初始化配置窗口
                FirstTimeSetupWindow w = new FirstTimeSetupWindow();
                this.Hide();
                w.ShowDialog();

                if (MMPU.是否第一次使用DDTV) // 非正常关闭窗口
                {
                    MessageBox.Show("未完成初始化，请重新启动程序");
                    Environment.Exit(-1);
                }
                NewThreadTask.Run(runOnLocalThread =>
                 {
                     bilibili.周期更新B站房间状态();
                 }, this);            
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    登陆B站账号.IsEnabled = false;
                    注销B站账号.IsEnabled = true;
                }
            }
            this.Show();
            NewThreadTask.Run(runOnLocalThread =>
             {
                 int i = 0;
                 while (true)
                 {
                     try
                     {
                         if (是否启动WSS连接组 && Vtbs不存在的直播间.Count > 5)
                         {
                             i++;
                             if (i > 1)
                             {
                                 break;
                             }
                             runOnLocalThread(() =>
                             {
                                 MessageBoxResult dr = MessageBox.Show("检测到监控列表中，非VTBS数据数据库房间的数量大于5个，可能会造成未知的错误，推荐关闭非VTBS的WSS连接方式或减少次类型的房间\r点击确定跳转关闭WSS连接窗口，点击取消本次忽略该提醒，下次启动DDTV前不会再次提示", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                                 if (dr == MessageBoxResult.OK)
                                 {
                                     InfoLog.InfoPrintf("推荐非VTBS连接房间数小于5，检测到目前数量大于5，大概率会造成连接错误，请注意。", InfoLog.InfoClass.系统错误信息);
                                     增加监控列表 A = new 增加监控列表(1);
                                     A.ShowDialog();
                                 }
                             });
                             
                         }
                     }
                     catch (Exception e)
                     {

                     }
                     Thread.Sleep(1000);
                 }

             }, this);
            new Task(() =>
            {

            }).Start();
        }

        /// <summary>
        /// 所有层的集合
        /// </summary>
        private List<Grid> _grids;

        public void 软件启动配置初始化()
        {
            if (!MMPU.配置文件初始化(0))
            {
                ;
            }
            if (string.IsNullOrEmpty(MMPU.Cookie))
            {
                if (!MMPU.是否第一次使用DDTV)
                {
                    MessageBox.Show("BILIBILI登陆信息已过期或丢失，推荐重新登陆");
                }
            }
            else
            {
                登陆B站账号.IsEnabled = false;
                注销B站账号.IsEnabled = true;
            }

            //公告加载线程
            NewThreadTask.Run(() =>
            {
                公告项目启动();
            });

            //NewThreadTask.Run(() =>
            //{
            //    MMPU.加载网络房间方法.更新网络房间缓存();
            //});

            //房间刷新线程
            NewThreadTask.Loop(runOnLocalThread =>
            {
                刷新房间列表UI(runOnLocalThread);
                runOnLocalThread(() => ViewModel.LatestDataUpdateTime = DateTime.Now);

                while (true)
                {
                    if (bilibili房间信息更新次数 > 0)
                    {
                        runOnLocalThread(() =>
                        {
                            try
                            {
                                首次更新.Visibility = Visibility.Collapsed;
                            }
                            catch (Exception) { }
                        });
                        break;
                    }
                    Thread.Sleep(100);
                }
            }, this, MMPU.直播列表刷新间隔 * 1000);

            //延迟测试
            int 超时次数 = 0;
            NewThreadTask.Loop(runOnLocalThread =>
            {
                double 国内 = MMPU.测试延迟("https://live.bilibili.com");
                if (国内 > 0)
                {
                    runOnLocalThread(() => ViewModel.ServerDelayBilibili = 国内);
                    MMPU.是否能连接阿B = true;
                }
                else
                {
                    runOnLocalThread(() => ViewModel.ServerDelayBilibili = -2.0);
                    MMPU.是否能连接阿B = false;
                }

                if (MMPU.连接404使能)
                {
                    double 国外 = MMPU.测试延迟("https://www.youtube.com");
                    if (国外 > 0)
                    {
                        runOnLocalThread(() => ViewModel.ServerDelayYoutube = 国外);
                        MMPU.是否能连接404 = true;
                    }
                    else
                    {
                        runOnLocalThread(() => ViewModel.ServerDelayYoutube = -2.0);
                        MMPU.是否能连接404 = false;
                    }
                }
                else
                {
                    MMPU.是否能连接404 = false;
                    runOnLocalThread(() => ViewModel.ServerDelayYoutube = -1.0);
                }
                {
                    double vdb延迟 = MMPU.测试延迟(Auxiliary.VTBS.API.VTBS服务器CDN.VTBS_Url+ "/v1/vtbs");
                    if (vdb延迟 > 0)
                    {
                        runOnLocalThread(() => ViewModel.serverVdb = vdb延迟);
                        超时次数 = 0;
                        //MMPU.数据源 = 0;
                    }
                    else
                    {
                        超时次数++;
                        if (超时次数 > 5)
                        {
                            runOnLocalThread(() => ViewModel.serverVdb = -2.0);
                            //MMPU.数据源 = 1;
                        }

                    }
                }
            }, this, 10000);
            //缩小功能
            {
                MMPU.缩小功能 = int.Parse(MMPU.getFiles("Zoom", "0"));
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
                    byte A2 = Convert.ToByte(MMPU.默认字幕颜色.Split(',')[1], 16);
                    byte A3 = Convert.ToByte(MMPU.默认字幕颜色.Split(',')[2], 16);
                    byte A4 = Convert.ToByte(MMPU.默认字幕颜色.Split(',')[3], 16);
                    SolidColorBrush S1 = new SolidColorBrush(Color.FromArgb(0xFF, A2, A3, A4));
                    字幕默认颜色.Foreground = S1;
                    字幕颜色 = S1;
                    byte B2 = Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[1], 16);
                    byte B3 = Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[2], 16);
                    byte B4 = Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[3], 16);
                    SolidColorBrush S2 = new SolidColorBrush(Color.FromArgb(0xFF, B2, B3, B4));
                    弹幕默认颜色.Foreground = S2;
                    弹幕颜色 = S2;
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
                    默认播放宽度.Text = MMPU.播放器默认宽度.ToString();
                    默认播放高度.Text = MMPU.播放器默认高度.ToString();
                }
                //播放缓冲时长
                {
                    播放缓冲时长.Text = MMPU.播放缓冲时长.ToString();
                }
                //并行直播数量
                {
                    并行直播数量.Text = MMPU.最大直播并行数量.ToString();
                }
                //转码使能按钮
                {
                    //MMPU.转码功能使能 = MMPU.getFiles("AutoTranscoding", "0") == "1" ? true : false;
                    if (MMPU.转码功能使能)
                    {
                        转码使能按钮.IsChecked = true;
                    }
                    else
                    {
                        转码使能按钮.IsChecked = false;
                    }
                }
                //录制弹幕按钮
                {
                    //MMPU.录制弹幕 = MMPU.getFiles("RecordDanmu", "0") == "1" ? true : false;
                    if (MMPU.录制弹幕)
                    {
                        录制弹幕使能按钮.IsChecked = true;
                    }
                    else
                    {
                        录制弹幕使能按钮.IsChecked = false;
                    }
                }
                //开机自启动按钮
                {
                    //MMPU.录制弹幕 = MMPU.getFiles("RecordDanmu", "0") == "1" ? true : false;
                    if (MMPU.开机自启动)
                    {
                        开机自启动按钮.IsChecked = true;
                    }
                    else
                    {
                        开机自启动按钮.IsChecked = false;
                    }
                }
            }
            //增加插件列表
            {
                PluginC.Items.Add(new
                {
                    编号 = "0",
                    名称 = "DDTV",
                    版本 = MMPU.版本号,
                    是否加载 = "强制",
                    说明 = "本软件的所有必须内容()",
                    备注 = ""
                });
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
                PluginC.Items.Add(new
                {
                    编号 = "4",
                    名称 = "弹幕录制工具",
                    版本 = "1.0.0.1",
                    是否加载 = "√",
                    说明 = "用于录制直播弹幕内容(工具箱内)",
                    备注 = ""
                });
                PluginC.Items.Add(new
                {
                    编号 = "5",
                    名称 = "BiliAccount",
                    版本 = "2.5.1.23",
                    是否加载 = "√",
                    说明 = "用于处理B站账号类的操作",
                    备注 = "基于MIT授权引用GITHUB @LeoChen98/BiliAccount"
                });
                PluginC.Items.Add(new
                {
                    编号 = "6",
                    名称 = "ffmpeg",
                    版本 = "4.2.2",
                    是否加载 = "√",
                    说明 = "用于压制和编码",
                    备注 = "需要在设置里启用"
                });
                PluginC.Items.Add(new
                {
                    编号 = "7",
                    名称 = "vtbs.moe",
                    版本 = "α",
                    是否加载 = "√",
                    说明 = "V圈大数据平台，提供DDTV运作所需的数据API接口",
                    备注 = ""
                });
                PluginC.Items.Add(new
                {
                    编号 = "8",
                    名称 = "DDTVLiveRec",
                    版本 = MMPU.版本号,
                    是否加载 = "√",
                    说明 = "提供录制核心功能",
                    备注 = ""
                });
            }
            //剪贴板监听
            Thread CPT = new Thread(() => 
            {
                string 剪贴板缓存 = "";
                while (true)
                {
                    while (MMPU.剪贴板监听)
                    {
                        try
                        {
                            string CP = Clipboard.GetText();
                            if (!CP.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains("http")
                            || !CP.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains("bilibili.com/video/")
                            || 剪贴板缓存 == CP) break; //不匹配或剪贴板无变化

                            剪贴板缓存 = CP;
                            string ABV = CP.Replace("bilibili.com/video/", "⒆").Split('⒆')[1].Split('?')[0];
                            if (!ABV.Contains("BV")) break; //无BV号

                            string AC = MMPU.使用WC获取网络内容("https://api.bilibili.com/x/player/pagelist?bvid=" + ABV + "&jsonp=jsonp");
                            JObject 标题 = JObject.Parse(MMPU.使用WC获取网络内容("https://api.bilibili.com/x/web-interface/view?bvid=" + ABV + "&jsonp=jsonp"));
                            JObject JO = JObject.Parse(AC);
                            BiliVideoInfo.VideoInfo.Root videolist = new BiliVideoInfo.VideoInfo.Root() { data = new List<BiliVideoInfo.VideoInfo.data>() };
                            if (JO["code"].ToString() != "0") break; //无必需信息

                            videolist.BV = ABV;
                            if (标题["code"].ToString() == "0")
                            {
                                videolist.title = 标题["data"]["title"].ToString();
                            }
                            for (int i = 0; i < JO["data"].Count(); i++)
                            {
                                JToken dataElem = JO["data"][i];
                                videolist.data.Add(new BiliVideoInfo.VideoInfo.data()
                                {
                                    cid = int.Parse(dataElem["cid"].ToString()),
                                    duration = int.Parse(dataElem["duration"].ToString()),
                                    from = dataElem["from"].ToString(),
                                    page = int.Parse(dataElem["page"].ToString()),
                                    part = dataElem["part"].ToString(),
                                    vid = dataElem["vid"].ToString(),
                                    weblink = dataElem["weblink"].ToString(),
                                    dimension = new BiliVideoInfo.VideoInfo.dimension()
                                    {
                                        height = int.Parse(dataElem["dimension"]["height"].ToString()),
                                        width = int.Parse(dataElem["dimension"]["width"].ToString()),
                                        rotate = int.Parse(dataElem["dimension"]["rotate"].ToString()),
                                    }
                                });
                            }
                            BiliVideoInfo.VideoInfo.Info.Add(videolist);
                        }
                        catch (Exception) { }
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(500);
                }
            });
            CPT.TrySetApartmentState(ApartmentState.STA);
            //CPT.Start();
            //CPT.Join();

            //版本号更新
            版本显示.Content = "版本: " + MMPU.版本号;
        }
        public void 公告项目启动()
        {
            //动态推送1
            NewThreadTask.Loop(runOnLocalThread =>
            {
                bool 动态推送1开关 = MMPU.TcpSend(
                        Server.RequestCode.GET_TOGGLE_DYNAMIC_NOTIFICATION, "{}", true,50)
                        == "1" ? true : false;

                if (动态推送1开关)
                {
                    string 动态推送内容 = MMPU.TcpSend(
                            Server.RequestCode.GET_DYNAMIC_NOTIFICATION, "{}", true, 50);
                    runOnLocalThread(() => ViewModel.PushNotification = 动态推送内容);
                }
            }, this, 3600 * 1000);
            NewThreadTask.Run(()=> {

                if (MMPU.启动模式 == 0)
                {
                    update.检查升级程序是否需要升级();
                }
            });
            //版本检查
            NewThreadTask.Run(() =>
            {
                string 服务器版本号 = MMPU.TcpSend(
                    Server.RequestCode.GET_LATEST_VERSION_NUMBER, "{}", true, 50);

                if (!string.IsNullOrEmpty(服务器版本号))
                {
                    bool 检测状态 = true;
                    foreach (var item in MMPU.不检测的版本号)
                    {
                        if (服务器版本号 == item)
                        {
                            检测状态 = false;
                        }
                    }
                    if (MMPU.版本号 != 服务器版本号 && 检测状态)
                    {
                        MessageBoxResult dr = MessageBox.Show(
                            "检测到版本更新,更新公告:\r\n"
                                + MMPU.TcpSend(Server.RequestCode.GET_UPDATE_ANNOUNCEMENT, "{}", true, 100)
                                + "\r\n\r\n点击确定启动自动更新，点击取消忽略",
                            "有新版本",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Question);

                        if (dr == MessageBoxResult.OK)
                        {
                            if (File.Exists("./update/DDTV_Update.exe"))
                            {
                                //启动外部程式
                                ProcessStartInfo startInfo = new ProcessStartInfo
                                {
                                    FileName = System.Windows.Forms.Application.StartupPath + "./update/DDTV_Update.exe",
                                    Arguments = "DDTV",
                                    WindowStyle = ProcessWindowStyle.Normal
                                };
                                try
                                {
                                    Process.Start(startInfo);
                                }
                                catch (Exception) { }

                                Environment.Exit(0);
                            }
                            else
                            {
                                MessageBox.Show("未找到自动更新程序，请到github或者群共享手动下载,点击确定跳转到github页面");
                                Process.Start(Server.PROJECT_ADDRESS);
                            }
                        }
                    }                   
                }
            });
            //推送内容1
            NewThreadTask.Run(runOnLocalThread =>
            {
                string 推送内容1text = MMPU.TcpSend(
                    Server.RequestCode.GET_PUSH_NOTIFICATION_1, "{}", true, 100);
                if (推送内容1text.Length > 0)
                {
                    runOnLocalThread(() => ViewModel.Announcement = 推送内容1text);
                }
            }, this);

        }
        private void 刷新房间列表UI(Action<Action> runOnLocalThread)
        {
            if (!MMPU.已经提示wss连接错误 && MMPU.wss连接错误的次数 > 3)
            {
                MMPU.已经提示wss连接错误 = !MMPU.已经提示wss连接错误;
                InfoLog.InfoPrintf($"网络状态不佳，多次尝试保持房间监控长连接失败，请关闭非VTBS数据来源房间监控，因为多次被阿B服务器拒绝连接，部分房间状态监控更新已停止", InfoLog.InfoClass.系统错误信息);
                runOnLocalThread(() => {
                    MMPU.弹窗.Add(30000,"网络连接失败/网络质量差", "网络状态不佳，多次尝试保持房间监控长连接失败，请关闭非VTBS数据来源房间监控，因为多次被阿B服务器拒绝连接，部分房间状态监控更新已停止。");
                    全局提示.Content = $"多次尝试连接阿B服务器均失败，网络状态不佳，部分房间监控已停止。点击我去关闭非VTBS数据源房间监控";
                    全局提示.Visibility = Visibility.Visible;
                    全局提示.MouseDown += 全局提示_MouseDown;
                });
            }
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
            foreach (var item in youtube.RoomList)
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
            string 检测房间状态变化用的字符串 = string.Empty;
            foreach (var item in 正在直播)
            {
                检测房间状态变化用的字符串 += i;
                检测房间状态变化用的字符串 += item.名称;
                检测房间状态变化用的字符串 += item.直播状态;
                检测房间状态变化用的字符串 += item.平台;
                检测房间状态变化用的字符串 += item.是否提醒;
                检测房间状态变化用的字符串 += item.是否录制视频;
                检测房间状态变化用的字符串 += item.房间号;
                检测房间状态变化用的字符串 += item.原名;
            }
            foreach (var item in 未直播)
            {
                检测房间状态变化用的字符串 += i;
                检测房间状态变化用的字符串 += item.名称;
                检测房间状态变化用的字符串 += item.直播状态;
                检测房间状态变化用的字符串 += item.平台;
                检测房间状态变化用的字符串 += item.是否提醒;
                检测房间状态变化用的字符串 += item.是否录制视频;
                检测房间状态变化用的字符串 += item.房间号;
                检测房间状态变化用的字符串 += item.原名;
            }
            if (MMPU.房间状态MD5值 != MMPU.GetMD5(检测房间状态变化用的字符串))
            {
                MMPU.房间状态MD5值 = MMPU.GetMD5(检测房间状态变化用的字符串);
                runOnLocalThread(() => LiveList.Items.Clear());
                foreach (var item in 正在直播)
                {
                    runOnLocalThread(() => LiveListAdd(i, item.名称, item.直播状态, item.平台, item.是否提醒, item.是否录制视频, item.房间号, item.原名));
                    i++;
                }
                foreach (var item in 未直播)
                {
                    runOnLocalThread(() => LiveListAdd(i, item.名称, item.直播状态, item.平台, item.是否提醒, item.是否录制视频, item.房间号, item.原名));
                    i++;
                }
            }
            int 单推人数 = 正在直播.Count + 未直播.Count;
            runOnLocalThread(() => ViewModel.TanoshiNum = 单推人数);
            runOnLocalThread(() => ViewModel.NowStreamingNum = 正在直播.Count);
            runOnLocalThread(() => ViewModel.NotStreamingNum = 未直播.Count);

            runOnLocalThread(() => 等待框.Visibility = Visibility.Collapsed);
            if (MMPU.是否提示一键导入)
            {
                MMPU.是否提示一键导入 = !MMPU.是否提示一键导入;
                if (正在直播.Count + 未直播.Count < 1)
                {
                    if (!MMPU.是否第一次使用DDTV)
                    {
                        MessageBox.Show("房间配置文件为空，没有监控中的房间，请手动添加或在设置界面登录后一键导入");
                    }
                 
                }
            }
        }

        private void 全局提示_MouseDown(object sender, MouseButtonEventArgs e)
        {
            增加监控列表 A = new 增加监控列表(1);
            A.ShowDialog();
        }

        public void LiveListAdd(int 编号, string 名称, bool 状态, string 平台, bool 直播提醒, bool 是否录制, string 唯一码, string 原名)
        {
            LiveList.Items.Add(new 
            { 
                编号 = 编号, 
                名称 = 名称, 
                状态 = 状态 ? "●直播中" : "○未直播", 
                平台 = 平台, 
                是否提醒 = 直播提醒 ? "√" : "", 
                是否录制 = 是否录制 ? "√" : "", 
                唯一码 = 唯一码, 
                原名 = 原名 
            });
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception) { }
        }
        private void 工具箱_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面(工具层);
        }
        private void 帮助_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面(帮助层);
        }
        private void DDNA列表_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面(AOE直播层);
        }
        private void 主站视频播放_按钮事件(object sender, MouseButtonEventArgs e)
        {
            //window.主站视频播放选择窗 MainBili = new window.主站视频播放选择窗();
            //MainBili.Show();
        }
        private void 关于_按钮事件(object sender, MouseButtonEventArgs e)
        {
            切换界面(关于层);
        }

        private void 关闭按钮_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("确定退出DDTV？如果要修改为缩小到后台，请修改设置界面缩小按钮功能", "退出", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                DDTV关闭事件();
            }
        }
        public static void DDTV关闭事件()
        {
            NewThreadTask.Run(() =>
            {
                try
                {
                    FileInfo[] files = new DirectoryInfo("./tmp/LiveCache/").GetFiles();
                    foreach (var item in files)
                    {
                        MMPU.文件删除委托("./tmp/LiveCache/" + item.Name,"DDTV关闭清空LiveCache缓存文件");
                    }
                }
                catch (Exception) { }
                Environment.Exit(0);
            });
        }
        NotifyIcon notifyIcon;
        private void 最小化按钮_Click(object sender, MouseButtonEventArgs e)
        {
            minimizeWindow();
        }

        private void minimizeWindow()
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
                Icon = DDTV_New.Properties.Resources.DDTV,//程序图标
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
            切换界面(home);
        }

        /// <summary>
        /// 切换界面
        /// </summary>
        /// <param name="grid">要显示的界面Grid对象</param>
        public void 切换界面(Grid grid)
        {
            foreach (Grid g in _grids)
            {
                if (grid == g)
                {
                    if (grid != home) top层.Visibility = Visibility.Visible;
                    g.Visibility = Visibility.Visible;
                }
                else g.Visibility = Visibility.Collapsed;
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
            切换界面(直播层);
        }
        private void 设置层_点击事件(object sender, MouseButtonEventArgs e)
        {
            切换界面(设置层);
        }
        private void 插件_点击事件(object sender, MouseButtonEventArgs e)
        {
            切换界面(插件层);
        }

        private void 直播表单击事件(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ListView LV = (System.Windows.Controls.ListView)sender;
                if (LV.Items.Count != 0)
                {
                    已选内容 = LV.SelectedItems[0].ToString();
                    选中内容1.Content = MMPU.获取livelist平台和唯一码.平台(已选内容) + "\r\n" + MMPU.获取livelist平台和唯一码.唯一码(已选内容) + "\r\n" + MMPU.获取livelist平台和唯一码.名称(已选内容);
                }
            }
            catch (Exception) { }
        }
        public static string 已选内容 = "";
        private void 添加直播列表按钮点击事件(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult dr = MessageBox.Show(
            "现在DDTV主数据源来自vdb.vtbs.moe，点击确定跳转vtbs网页提交新的数据\r\r提交后等待平台后台同步完成一键导入即可导入新的监控列表\r\rvtbs.moe是一个V圈的分布式DD大数据平台，可以查询直播人气、舰队、V圈宏观经济、直播状态、视频势数据、风云榜等数据\r\n\r\nDDTV客户端安装在后台也回把直播数据统计发送到vtbs，帮助vtbs.moe持续运行",
            "添加新的监控",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    Process.Start("https://submit.vtbs.moe/");
                }
                else
                {
                    ;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("未找到默认浏览器，请手动在浏览器输入网址submit.vtbs.moe浏览");
            }
            return;
            等待框.Visibility = Visibility.Visible;
            AddMonitoringList AML = new AddMonitoringList("添加新单推", "", "", "", "", false);
            AML.ShowDialog();
        }
        private void 修改直播列表按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(已选内容))
            {
                MessageBox.Show("未选择");
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
                if (string.IsNullOrEmpty(直播地址))
                {
                    return;
                }
                Process.Start(直播地址);
            }
            catch (Exception)
            {
                MessageBox.Show("打开这个直播发生错误，可能是由于无法连接目标网页");
            }
        }
        private void 直播表双击事件(object sender, MouseButtonEventArgs e)
        {
           
            NewThreadTask.Run(runOnLocalThread =>
            {
                InfoLog.InfoPrintf("用户双击直播列表事件触发", InfoLog.InfoClass.Debug);
                System.Windows.Controls.ListView LV = (System.Windows.Controls.ListView)sender;
                RoomInit.RoomInfo roomInfo = new RoomInfo();
                try
                {
                    runOnLocalThread(() => {
                        InfoLog.InfoPrintf($"用户双击直播列表：双击房间号{MMPU.获取livelist平台和唯一码.唯一码(LV.SelectedItems[0].ToString())}", InfoLog.InfoClass.Debug);
                    });
                    
                }
                catch (Exception){}
               
                
              
                string 平台 = string.Empty;
                runOnLocalThread(() =>
                {
                    try
                    {
                        string TEST1 = LV.SelectedItems[0].ToString();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("选中的列表位置信息为空");
                        return;
                    }
                    平台 = MMPU.获取livelist平台和唯一码.平台(LV.SelectedItems[0].ToString());
                });

                if (MMPU.当前直播窗口数量 >= MMPU.最大直播并行数量)
                {
                    if (平台 == "bilibili")
                    {
                        MessageBox.Show("达到了设置的最大直播窗口数量,新建直播窗口失败");
                        return;
                    }

                }
                try
                {
                    runOnLocalThread(() => {
                        等待框.Visibility = Visibility.Visible;
                    });
                    
                }
                catch (Exception) { }


                try
                {
                    if (平台 == "bilibili")
                    {
                        runOnLocalThread(() => {
                            roomInfo = bilibili.GetRoomInfo(MMPU.获取livelist平台和唯一码.唯一码(LV.SelectedItems[0].ToString()));
                        });
                        
                        if (!roomInfo.直播状态)
                        {
                            MessageBox.Show("该房间未直播");
                            try
                            {
                                runOnLocalThread(() =>
                                {
                                    等待框.Visibility = Visibility.Collapsed;
                                });
                            }
                            catch (Exception)
                            {
                            }
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    runOnLocalThread(() => {
                        等待框.Visibility = Visibility.Collapsed;
                    });
                    
                    return;
                }
                string 唯一码 = roomInfo.房间号;
                string 标题 = roomInfo.标题;
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
                                        item.标题 = roomInfo.标题;
                                        标题 = item.标题;
                                    }
                                }
                                string 下载地址 = string.Empty;
                                try
                                {
                                    下载地址 = bilibili.根据房间号获取房间信息.下载地址(唯一码);
                                    if (string.IsNullOrEmpty(下载地址))
                                    {
                                        MessageBox.Show("房间未开播或已加密");
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    MessageBox.Show("获取下载地址错误");
                                    return;
                                }
                                Downloader 下载对象 = new Downloader
                                {
                                    DownIofo = new Downloader.DownIofoData() { 平台 = 平台, 房间_频道号 = 唯一码, 标题 = 标题, 事件GUID = GUID, 下载地址 = 下载地址, 备注 = "视频播放缓存", 是否保存 = false, 继承 = new Downloader.继承() }
                                };
                                Task.Run(() =>
                                {
                                    this.Dispatcher.Invoke(new Action(delegate
                                    {
                                        runOnLocalThread(() => {
                                            打开直播列表(下载对象);
                                            try
                                            {
                                                等待框.Visibility = Visibility.Collapsed;
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        });
                                        
                                        MMPU.当前直播窗口数量++;
                                        

                                    }));
                                });
                                break;
                            }
                        case "youtube":
                            {
                                Process.Start("https://www.youtube.com/channel/" + 唯一码 + "/live");
                                break;
                            }
                        default:
                            MessageBox.Show("发现了与当前版本不支持的平台，请检查更新");
                            return;
                    }
                }
                catch (Exception ex)
                {
                    错误窗 ERR = new 错误窗("新建播放窗口发生意外错误，请重试", "新建播放窗口发生意外错误，请重试\r\n" + ex.ToString());
                    ERR.ShowDialog();
                    return;
                }
            },this);
           
        }
        public void 打开直播列表(Downloader DL)
        {
            //System.Diagnostics.Process p = new System.Diagnostics.Process();
            //p.StartInfo.FileName = @"D:\Program Files (x86)\Pure Codec\x64\PotPlayerMini64.exe";//需要启动的程序名       
            //p.StartInfo.Arguments = " \""+DL.DownIofo.下载地址+"\"";//启动参数       
            //p.Start();//启动       

            //return;


            if (DL != null)
            {
                DL.DownIofo.播放状态 = true;
                DL.DownIofo.是否是播放任务 = true;
                switch (播放器版本)
                {
                    case 1:
                        {
                            PlayW.MainWindow PlayWindow = new PlayW.MainWindow(DL, MMPU.默认音量, 弹幕颜色, 字幕颜色, MMPU.默认弹幕大小, MMPU.默认字幕大小, MMPU.播放器默认宽度, MMPU.播放器默认高度);
                            PlayWindow.Closed += 播放窗口退出事件;
                            PlayWindow.Show();
                            PlayWindow.BossKey += 老板键事件;
                            playList1.Add(PlayWindow);
                            break;
                        }
                    case 2:
                        {
                            //硬解播放器.Form1 PlayWindow = new 硬解播放器.Form1(DL, MMPU.默认音量,MMPU.PlayWindowW, MMPU.PlayWindowH);
                            //PlayWindow.Closed += 播放窗口退出事件;
                            //PlayWindow.Show();
                            //playList2.Add(PlayWindow);
                            break;
                        }
                }
                // PlayW.MainWindow PlayWindow = new PlayW.MainWindow(DL, MMPU.默认音量, 弹幕颜色, 字幕颜色, MMPU.默认弹幕大小, MMPU.默认字幕大小, MMPU.PlayWindowW, MMPU.PlayWindowH);



                // MMPU.ClearMemory();
            }
            else
            {
                MessageBox.Show("Downloader结构体不能为Null,出现了未知的错误！");
                return;
            }
        }

        private void 老板键事件(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            foreach (var item in playList1)
            {
                if (item.窗口是否打开)
                {
                    item.WindowState = WindowState.Minimized;
                }
            }
        }

        private void 播放窗口退出事件(object sender, EventArgs e)
        {
            NewThreadTask.Run(() =>
            {
                try
                {
                    MMPU.当前直播窗口数量--;
                    switch (播放器版本)
                    {
                        case 1:
                            {
                                PlayW.MainWindow p = (PlayW.MainWindow)sender;
                                playList1.Remove(p);
                                foreach (var item in MMPU.DownList)
                                {
                                    if (item.DownIofo.事件GUID == p.DD.DownIofo.事件GUID)
                                    {
                                        item.DownIofo.WC.CancelAsync();
                                        item.DownIofo.下载状态 = false;
                                        item.DownIofo.备注 = "播放窗口关闭，停止下载";
                                        item.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                        if (!item.DownIofo.是否保存)
                                        {
                                            MMPU.文件删除委托(p.DD.DownIofo.文件保存路径,"关闭播放窗口，删除LiveCache缓存文件");
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        case 2:
                            {
                                //硬解播放器.Form1 p = (硬解播放器.Form1)sender;
                                //playList2.Remove(p);
                                //foreach (var item in MMPU.DownList)
                                //{
                                //    if (item.DownIofo.事件GUID == p.DD.DownIofo.事件GUID)
                                //    {
                                //        item.DownIofo.WC.CancelAsync();
                                //        item.DownIofo.下载状态 = false;
                                //        item.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                //        if (item.DownIofo.是否保存)
                                //        {

                                //        }
                                //        else
                                //        {
                                //            MMPU.文件删除委托(p.DD.DownIofo.文件保存路径);
                                //        }
                                //        break;
                                //    }
                                //}
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf("关闭播放窗口出现错误:"+e.ToString(), InfoLog.InfoClass.Debug);
                }
            });
        }

        private void 直播列表删除按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(已选内容))
            {
                MessageBox.Show("未选择");
                return;
            }
            try
            {
                等待框.Visibility = Visibility.Visible;
            }
            catch (Exception) { }

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
            if (MMPU.获取livelist平台和唯一码.平台(已选内容) == "bilibili")
            {
                InitializeRoomList(int.Parse(MMPU.获取livelist平台和唯一码.唯一码(已选内容)), true, false);
            }
            else
            {
                InitializeRoomList(0, false, false);
            }
            //更新房间列表(MMPU.获取livelist平台和唯一码(已选内容, "平台"), MMPU.获取livelist平台和唯一码(已选内容, "唯一码"),0);
            MessageBox.Show("删除完成");
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
            if (MMPU.连接404使能)
            {
                if (B站更新刷新次数 == 0 || youtube更新刷新次数 == 0)
                {
                    MessageBox.Show("后台还未初始化完成，不能修改，请稍候再试");
                    return;
                }
            }
            else
            {
                if (B站更新刷新次数 == 0)
                {
                    MessageBox.Show("后台还未初始化完成，不能修改，请稍候再试");
                    return;
                }
            }
            InfoLog.InfoPrintf(已选内容, InfoLog.InfoClass.Debug);
            bool 是否改过 = false;
            if (string.IsNullOrEmpty(已选内容))
            {
                MessageBox.Show("未选择");
                return;
            }
            try
            {
                等待框.Visibility = Visibility.Visible;
            }
            catch (Exception) { }

            RoomBox RB = new RoomBox
            {
                data = new List<RoomCadr>()
            };
            while (bilibili房间主表长度 + youtube房间主表长度 != bilibili房间主表.Count() + youtube房间主表.Count() 
                && bilibili房间主表长度 + youtube房间主表长度 != 0)
            {
                Thread.Sleep(10);
            }
            List<RL> 房间表 = bilibili房间主表.Concat(youtube房间主表).ToList<RL>();
            int rlclen = bilibili房间主表.Count() + youtube房间主表.Count();
            for (int i = 0; i < rlclen; i++)
            {
                string 唯一码 = MMPU.获取livelist平台和唯一码.唯一码(已选内容);
                if (房间表[i].唯一码 == 唯一码)
                {
                    if (!是否改过)
                    {
                        是否改过 = true;

                        房间表.Remove(房间表[i]);
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
                    RB.data.Add(new RoomCadr
                    { 
                        LiveStatus = 房间表[i].直播状态,
                        Name = 房间表[i].名称, OfficialName = 房间表[i].原名,
                        RoomNumber = 房间表[i].唯一码,
                        VideoStatus = 房间表[i].是否录制,
                        Types = 房间表[i].平台,
                        RemindStatus = 房间表[i].是否提醒,
                        status = false 
                    });
                    if (RoomInit.根据唯一码获取直播状态(房间表[i].唯一码))
                    {
                        RB.data[RB.data.Count() - 1].LiveStatus = true;
                    }
                }
            }
            string JOO = JsonConvert.SerializeObject(RB);
            MMPU.储存文本(JOO, RoomConfigFile);
            InitializeRoomList(0, false, false);
            //更新房间列表(平台.SelectedItem.ToString(), 唯一码.Text,2);
            //MessageBox.Show("修改成功");
        }

        private void 录制按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(已选内容))
            {
                MessageBox.Show("未选择");
                return;
            }
            NewThreadTask.Run(runOnLocalThread =>
            {
                runOnLocalThread(() => {
                    try
                    {
                        等待框.Visibility = Visibility.Visible;
                    }
                    catch (Exception) { }
                });
               
                switch (MMPU.获取livelist平台和唯一码.平台(已选内容))
                {
                    case "bilibili":
                        {
                            if (!bilibili.根据房间号获取房间信息.是否正在直播(MMPU.获取livelist平台和唯一码.唯一码(已选内容),false))
                            {
                                MessageBox.Show("该房间当前未直播");
                                return;
                            }
                            break;
                        }
                    case "youtube":
                        {
                            MessageBox.Show("youtube暂不支持录制功能");
                            break;
                        }
                    //case "youtube":
                    //    {
                    //        break;
                    //    }
                    //case "tc":
                    //    {
                    //        break;
                    //    }
                    //case "douyu":
                    //    {
                    //        break;
                    //    }
                    //case "老鼠台":
                    //    {
                    //        break;
                    //    }
                    //case "AcFun":
                    //    {
                    //        break;
                    //    }
                    default:
                        MessageBox.Show("发现了与当前版本不支持的平台，请检查更新");
                        return;
                }
                string 唯一码 = MMPU.获取livelist平台和唯一码.唯一码(已选内容);
                foreach (var item in MMPU.DownList)
                {
                    if (item.DownIofo.房间_频道号 == 唯一码 && item.DownIofo.结束时间 == 0)
                    {
                        if (item.DownIofo.是否保存)
                        {
                            MessageBox.Show("该房间在下载列表中已存在!");
                            return;
                        }
                        break;
                    }
                }
                string GUID = Guid.NewGuid().ToString();
                string 标题 = bilibili.根据房间号获取房间信息.获取标题(MMPU.获取livelist平台和唯一码.唯一码(已选内容));
                string 下载地址 = string.Empty;
                try
                {
                    下载地址 = bilibili.根据房间号获取房间信息.下载地址(MMPU.获取livelist平台和唯一码.唯一码(已选内容));
                    if (string.IsNullOrEmpty(下载地址))
                    {
                        MessageBox.Show("房间未开播或已加密");
                        return;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("获取下载地址失败");
                    return;
                }
                Downloader 下载对象 = Downloader.新建下载对象(MMPU.获取livelist平台和唯一码.平台(已选内容), MMPU.获取livelist平台和唯一码.唯一码(已选内容), 标题, GUID, 下载地址, "手动下载任务", true, MMPU.获取livelist平台和唯一码.名称(已选内容) + "-" + MMPU.获取livelist平台和唯一码.原名(已选内容), false, null);
               if(下载对象==null)
                {
                    MessageBox.Show("已经存在该直播间的录制任务，本次下载请求已撤销");
                }
                //MessageBox.Show("下载任务添加完成");
               
                    runOnLocalThread(() => {
                        try
                        {
                            等待框.Visibility = Visibility.Collapsed;
                        }
                        catch (Exception)
                        { }
                    });
                
            }, this);

           
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
                MessageBox.Show("未选择");
                return;
            }
            switch (MMPU.获取livelist平台和唯一码.平台(已选内容))
            {
                case "bilibili":
                    Process.Start("https://live.bilibili.com/" + MMPU.获取livelist平台和唯一码.唯一码(已选内容));
                    break;
                case "youtube":
                    Process.Start("https://www.youtube.com/channel/" + MMPU.获取livelist平台和唯一码.唯一码(已选内容) + "/live");
                    break;
                default:
                    MessageBox.Show("发现了与当前版本不支持的平台，请检查更新");
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
            P_File_Folder.Dispose();
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
        private void 录制完成后转码开关点击事件(object sender, RoutedEventArgs e)
        {
            if (转码使能按钮.IsChecked == true)
            {
                MMPU.转码功能使能 = true;
                MMPU.setFiles("AutoTranscoding", "1");
            }
            else
            {
                MMPU.转码功能使能 = false;
                MMPU.setFiles("AutoTranscoding", "0");
            }
        }
        private void 剪贴板监听按钮开关点击事件(object sender, RoutedEventArgs e)
        {
            if (剪贴板监听按钮.IsChecked == true)
            {
                MMPU.剪贴板监听 = true;
                MMPU.setFiles("ClipboardMonitoring", "1");
            }
            else
            {
                MMPU.剪贴板监听 = false;
                MMPU.setFiles("ClipboardMonitoring", "0");
            }
        }
        private void 录制弹幕按钮开关点击事件(object sender, RoutedEventArgs e)
        {
            if (录制弹幕使能按钮.IsChecked == true)
            {
                MessageBoxResult dr = MessageBox.Show("该功能可能会导致房间监控失效，并且由于是因为阿B的服务器接口限制问题，暂时无法修复\r\n如果必须录制，推荐监控列表中就放目标房间，不要放置其他房间\r\n\r\n确定要进行弹幕录制吗？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (dr == MessageBoxResult.Yes)
                {
                    MMPU.录制弹幕 = true;
                    MMPU.setFiles("RecordDanmu", "1");
                }
                else if (dr == MessageBoxResult.No)
                {
                    录制弹幕使能按钮.IsChecked = false;
                    MMPU.录制弹幕 = false;
                    MMPU.setFiles("RecordDanmu", "0");
                }          
            }
            else
            {
                MMPU.录制弹幕 = false;
                MMPU.setFiles("RecordDanmu", "0");
            }
        }
        private void 修改最大直播并行数量确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            try
            {
                MMPU.最大直播并行数量 = int.Parse(并行直播数量.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("并行直播数量输入框内容不是数字");
                return;
            }
            MMPU.setFiles("PlayNum", 并行直播数量.Text);
            MessageBox.Show("修改成功");
        }
        private void 修改弹幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color))
                    {
                        SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                        弹幕默认颜色.Foreground = solidColorBrush;
                        MMPU.默认弹幕颜色 = solidColorBrush.Color.A.ToString("X2") + "," + solidColorBrush.Color.R.ToString("X2") + "," + solidColorBrush.Color.G.ToString("X2") + "," + solidColorBrush.Color.B.ToString("X2");
                        MMPU.setFiles("DanMuColor", MMPU.默认弹幕颜色);
                        弹幕颜色 = solidColorBrush;
                    }
                }
            }
        }
        private void 修改字幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color))
                    {
                        SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                        字幕默认颜色.Foreground = solidColorBrush;
                        MMPU.默认字幕颜色 = solidColorBrush.Color.A.ToString("X2") + "," + solidColorBrush.Color.R.ToString("X2") + "," + solidColorBrush.Color.G.ToString("X2") + "," + solidColorBrush.Color.B.ToString("X2");
                        MMPU.setFiles("ZiMuColor", MMPU.默认字幕颜色);
                        字幕颜色 = solidColorBrush;
                    }
                }
            }
        }
        private void 并行直播数量_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            检测输入框是否为数字((System.Windows.Controls.TextBox)sender, 30, 3);
        }
        private void 弹幕文字大小_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            检测输入框是否为数字((System.Windows.Controls.TextBox)sender, 30, 1);
        }
        private void 修改弹幕文字大小确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            MMPU.默认弹幕大小 = int.Parse(弹幕文字大小.Text);
            MMPU.setFiles("DanMuSize", 弹幕文字大小.Text);
            MessageBox.Show("修改成功");
        }
        private void 字幕文字大小_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            检测输入框是否为数字((System.Windows.Controls.TextBox)sender, 99, 1);
        }
        private void 分辨率大小_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //检测输入框是否为数字((System.Windows.Controls.TextBox)sender, 4000, 128);
        }
        private void 修改字幕文字大小确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            MMPU.默认字幕大小 = int.Parse(字幕文字大小.Text);
            MMPU.setFiles("ZiMuSize", 字幕文字大小.Text);
            MessageBox.Show("修改成功");
        }
        private void 修改播放器默认大小确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.Parse(默认播放宽度.Text) < 128 || int.Parse(默认播放宽度.Text) > 5000)
                {

                }
                if (int.Parse(默认播放高度.Text) < 128 || int.Parse(默认播放高度.Text) > 3000)
                {

                }
            }
            catch (Exception)
            {
                InfoLog.InfoPrintf("输入的播放器默认长宽数字不符合要求", InfoLog.InfoClass.系统错误信息);
                MessageBox.Show("输入的播放器默认长宽数字不符合要求");
                //throw;
            }
            MMPU.播放器默认宽度 = int.Parse(默认播放宽度.Text);
            MMPU.setFiles("PlayWindowW", 默认播放宽度.Text);
            MMPU.播放器默认高度 = int.Parse(默认播放高度.Text);
            MMPU.setFiles("PlayWindowH", 默认播放高度.Text);

            MessageBox.Show("修改成功");
        }
        private void 检测输入框是否为数字(System.Windows.Controls.TextBox A, int max, int min)
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
                    if (int.Parse(A.Text) < min)
                    {
                        A.Text = min.ToString();
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
            NewThreadTask.Run(runOnLocalThread =>
            {
                try
                {
                    外部API.正在直播数据 直播数据 = new 外部API.正在直播数据();
                    直播数据.更新正在直播数据();
                    runOnLocalThread(() =>
                    {
                        bilibiliAOE直播列表.Items.Clear();
                        youtubeAOE直播列表.Items.Clear();
                        tcAOE直播列表.Items.Clear();
                    });

                    foreach (var item in 直播数据.直播数据)
                    {
                        string 时间 = MMPU.将时间戳转换为日期类型(item.实际开始时间);
                        if (!string.IsNullOrEmpty(item.直播连接))
                        {
                            switch (item.频道类型)
                            {
                                case 4:
                                    runOnLocalThread(() => bilibiliAOE直播列表.Items.Add(new { 名称 = item.主播名称, 标题 = item.标题, 观看人数 = item.当前观众, 直播开始时间 = 时间, 直播URL = item.直播连接 }));
                                    break;
                                case 1:
                                    runOnLocalThread(() => youtubeAOE直播列表.Items.Add(new { 名称 = item.主播名称, 标题 = item.标题, 观看人数 = item.当前观众, 直播开始时间 = 时间, 直播URL = item.直播连接 }));
                                    break;
                                case 8:
                                    runOnLocalThread(() => tcAOE直播列表.Items.Add(new { 名称 = item.主播名称, 标题 = item.标题, 观看人数 = item.当前观众, 直播开始时间 = 时间, 直播URL = item.直播连接 }));
                                    break;
                            }
                        }
                    }
                    if (bilibiliAOE直播列表.Items.Count == 0)
                    {
                        runOnLocalThread(() => bilibiliAOE直播列表.Items.Add(new { 名称 = "", 标题 = "没有正在BILIBILI直播的VTB数据,这有可能是外部服务器或者bilibili拒绝提供数据", 观看人数 = "", 直播开始时间 = "", 直播URL = "" }));
                    }
                    if (youtubeAOE直播列表.Items.Count == 0)
                    {
                        runOnLocalThread(() => youtubeAOE直播列表.Items.Add(new { 名称 = "", 标题 = "当前没有在YouTuBe直播的VTB", 观看人数 = "", 直播开始时间 = "", 直播URL = "" }));
                    }
                    if (tcAOE直播列表.Items.Count == 0)
                    {
                        runOnLocalThread(() => tcAOE直播列表.Items.Add(new { 名称 = "", 标题 = "当前没有在TwitCasting直播的VTB", 观看人数 = "", 直播开始时间 = "", 直播URL = "" }));
                    }
                } catch (Exception) { }

                runOnLocalThread(() =>
                {
                    刷新AOE直播列表按钮.IsEnabled = true;
                    刷新AOE直播列表按钮.Content = "刷新列表";
                });
            }, this);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void 获取网络列表按钮点击事件(object sender, RoutedEventArgs e)
        {
            //window.NetRoomList NRL = new window.NetRoomList();
            //NRL.Show();
        }

        private void 登陆B站账号_Click(object sender, RoutedEventArgs e)
        {
            window.BiliLoginWindowQR BLW = new window.BiliLoginWindowQR();
            BLW.ShowDialog();
            //  Plugin.BilibiliAccount.BiliLogin();
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                登陆B站账号.IsEnabled = false;
                注销B站账号.IsEnabled = true;
            }
        }

        private void 注销B站账号_Click(object sender, RoutedEventArgs e)
        {
            MMPU.Cookie = "";
            MMPU.写ini配置文件("User", "Cookie", "", MMPU.BiliUserFile);
            MMPU.csrf = null;
            MMPU.写ini配置文件("User", "csrf", "", MMPU.BiliUserFile);
            登陆B站账号.IsEnabled = true;
            注销B站账号.IsEnabled = false;
        }

        private void 一键导入账号关注VTB和VUP_Click(object sender, RoutedEventArgs e)
        {
            增加监控列表 T = new 增加监控列表();
            T.ShowDialog();

            return;
            MessageBox.Show("点击确定开始导入，此期间请勿关闭DDTV...请稍后.....");

            等待框.Visibility = Visibility.Visible;

            AddList.导入VTBVUP((TEXT) =>
            {
                //增加房间提示信息.Content = $"导入成功！原有:{已经存在的数量}个，新增VTB/VUP数：{增加的数量}";
                MessageBox.Show(TEXT);
                等待框.Visibility = Visibility.Collapsed;
            }, this,false);


            //if (string.IsNullOrEmpty(MMPU.Cookie))
            //{
            //    MessageBox.Show("请先登录");
            //    return;
            //}
            //else if (MMPU.加载网络房间方法.列表缓存.Count < 1000)
            //{
            //    MessageBox.Show("=======================\n房间列表数据后台加载中，请30秒后再试\n=======================");
            //    if (!MMPU.加载网络房间方法.是否正在缓存)
            //    {
            //        NewThreadTask.Run(() => MMPU.加载网络房间方法.更新网络房间缓存());
            //    }
            //    return;
            //}
            //else
            //{
            //    MessageBox.Show("=======================\n点击确定开始导入，在此期间请勿操作\n=======================");
            //}
            //NewThreadTask.Run(runOnLocalThread =>
            //{
            //    runOnLocalThread(() =>
            //    {
            //        try
            //        {
            //            等待框.Visibility = Visibility.Visible;
            //        }
            //        catch (Exception) { }
            //    });

            //    int 增加的数量 = 0;
            //    int 已经存在的数量 = 0;
            //    RoomBox rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
            //    RoomBox RB = new RoomBox
            //    {
            //        data = new List<RoomCadr>()
            //    };
            //    if (rlc.data != null)
            //    {
            //        foreach (var item in rlc.data)
            //        {
            //            RB.data.Add(item);
            //        }
            //    }
            //    List<MMPU.加载网络房间方法.选中的网络房间> 符合条件的房间 = new List<MMPU.加载网络房间方法.选中的网络房间>();
            //    JObject BB = bilibili.根据UID获取关注列表(MMPU.UID);
            //    foreach (var 账号关注数据 in BB["data"])
            //    {
            //        foreach (var 网络房间数据 in MMPU.加载网络房间方法.列表缓存)
            //        {
            //            if (账号关注数据["UID"].ToString() == 网络房间数据.UID)
            //            {
            //                符合条件的房间.Add(new MMPU.加载网络房间方法.选中的网络房间()
            //                {
            //                    UID = 网络房间数据.UID,
            //                    名称 = 网络房间数据.名称,
            //                    官方名称 = 网络房间数据.官方名称,
            //                    平台 = 网络房间数据.平台,
            //                    房间号 = null,
            //                    编号 = 0
            //                });
            //                break;
            //            }
            //        }
            //    }
            //    foreach (var 符合条件的 in 符合条件的房间)
            //    {
            //        if (!string.IsNullOrEmpty(符合条件的.UID))
            //        {
            //            string 房间号 = 通过UID获取房间号(符合条件的.UID);

            //            符合条件的.房间号 = 房间号;
            //            bool 是否已经存在 = false;
            //            foreach (var item in bilibili.RoomList)
            //            {
            //                if (item.房间号 == 房间号)
            //                {
            //                    是否已经存在 = true;
            //                    break;
            //                }
            //            }
            //            if (!是否已经存在 && !string.IsNullOrEmpty(房间号.Trim('0')))
            //            {
            //                增加的数量++;
            //                RB.data.Add(new RoomCadr { Name = 符合条件的.名称, RoomNumber = 符合条件的.房间号, Types = 符合条件的.平台, RemindStatus = false, status = false, VideoStatus = false, OfficialName = 符合条件的.官方名称, LiveStatus = false });
            //            }
            //            else
            //            {
            //                已经存在的数量++;
            //            }
            //        }
            //        Thread.Sleep(300);
            //    }
            //    string JOO = JsonConvert.SerializeObject(RB);
            //    MMPU.储存文本(JOO, RoomConfigFile);
            //    InitializeRoomList(0, false, false);

            //    MessageBox.Show("导入完成，新导入" + 增加的数量 + "个,主窗口列表可能会有延迟，加载完成后会自动更新界面");
            //    runOnLocalThread(() =>
            //    {
            //        try
            //        {
            //            等待框.Visibility = Visibility.Collapsed;
            //        }
            //        catch (Exception) { }
            //    });
            //}, this);
            //NewThreadTask.Run(() =>
            //{

            //});
        }

        private void 播放本地视频文件按钮_Click(object sender, RoutedEventArgs e)
        {

        }

        private void 播放缓冲时长_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //  检测输入框是否为数字((System.Windows.Controls.TextBox)sender, 60, 1);
        }

        private void 修改播放缓冲时长确定按钮点击事件(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.Parse(播放缓冲时长.Text) > 2)
                {
                    MMPU.播放缓冲时长 = int.Parse(播放缓冲时长.Text);
                    MMPU.setFiles("BufferDuration", 播放缓冲时长.Text);
                    MessageBox.Show("修改成功");
                }
            }
            catch (Exception)
            {
                InfoLog.InfoPrintf("输入的播放缓冲时长数值不符合要求，最低2秒", InfoLog.InfoClass.系统错误信息);
                MessageBox.Show("输入的播放缓冲时长数值不符合要求，最低2秒");
                //  throw;
            }

        }

        private void 播放窗口排序按钮点击事件(object sender, RoutedEventArgs e)
        {
            double W = SystemParameters.WorkArea.Width;//得到屏幕工作区域宽度
            double H = SystemParameters.WorkArea.Height;//得到屏幕工作区域高度
            switch (playList1.Count)
            {
                case 1:
                    {
                        playList1[0].Width = W;
                        playList1[0].Height = H;
                        break;
                    }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("确定退出DDTV？如果要修改为缩小到后台，请修改设置界面缩小按钮功能", "退出", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                DDTV关闭事件();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void 非VTB数据添加设置按钮事件(object sender, RoutedEventArgs e)
        {
            增加监控列表 A = new 增加监控列表();
            A.ShowDialog();
        }

        private void 开机自启动按钮开关点击事件(object sender, RoutedEventArgs e)
        {
            if (开机自启动按钮.IsChecked == true)
            {
              
                MMPU.开机自启动 = true;
                MMPU.setFiles("BootUp", "1");
            }
            else
            {
               
                MMPU.开机自启动 = false;
                MMPU.setFiles("BootUp", "0");
            }
        }

        private void 修改房间信息按钮_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("修改房间配置文件请使用DDTV文件夹里的【DDTVRoomConfig.exe】房间配置修改工具进行修改\r或者直接修改[RoomListConfig.json]文件\r修改前请关闭DDTV本体");
        }
    }
}