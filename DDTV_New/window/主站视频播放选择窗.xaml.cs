using Auxiliary;
using DDTV_New.Utility;
using Newtonsoft.Json.Linq;
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
using System.Windows.Shapes;

namespace DDTV_New.window
{
    /// <summary>
    /// 主站视频播放选择窗.xaml 的交互逻辑
    /// </summary>
    public partial class 主站视频播放选择窗 : Window
    {
        public static List<PlayW.MainWindow> playList1 = new List<PlayW.MainWindow>();
        public 主站视频播放选择窗()
        {
            InitializeComponent();
            this.Activated += 主站视频播放选择窗_Activated;
        }

        private void 主站视频播放选择窗_Activated(object sender, EventArgs e)
        {
            刷新视频列表();
        }

        private void B站视频列表刷新列表按钮开关点击事件(object sender, RoutedEventArgs e)
        {
            刷新视频列表();
        }
        public void 刷新视频列表()
        {
            主站列表.Items.Clear();
            foreach (var item in Auxiliary.BiliVideoInfo.VideoInfo.Info)
            {
                主站列表.Items.Add(item.BV+"\\|/"+item.title);
            }
        }

        private void B站视频列表清空列表按钮开关点击事件(object sender, RoutedEventArgs e)
        {

        }

        private void B站主站视频播放按钮开关点击事件(object sender, RoutedEventArgs e)
        {

        }

        private void 主站列表_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string person = 主站列表.SelectedItem.ToString();
                if (person.Contains("\\|/"))
                {
                    string BVID = person.Replace("\\|/", "⒆").Split('⒆')[0];
                    string cid = "";
                    string dlurl = "";
                    foreach (var item in BiliVideoInfo.VideoInfo.Info)
                    {
                        if (item.BV == BVID)
                        {
                            if (item.data.Count < 2)
                            {
                                cid = item.data[0].cid.ToString();
                                break;
                            }
                        }
                    }
                    JObject JO = JObject.Parse(MMPU.使用WC获取网络内容("https://api.bilibili.com/x/player/playurl?bvid=" + BVID + "&cid=" + cid + "&type=json"));
                    if (JO["code"].ToString() == "0")
                    {
                        dlurl = JO["data"]["durl"][0]["url"].ToString();
                        Downloader 下载对象 = new Downloader
                        {
                            DownIofo = new Downloader.DownIofoData() { 平台 = "主站视频", 房间_频道号 = "", 标题 = person.Replace("\\|/", "⒆").Split('⒆')[1], 事件GUID = Guid.NewGuid().ToString(), 下载地址 = dlurl, 备注 = "视频播放缓存", 是否保存 = false, 继承 = new Downloader.继承(),是否是播放任务=true }
                        };
                        //Downloader 下载对象 = Downloader.新建下载对象(平台, 唯一码, 标题, GUID, 下载地址, "视频播放缓存", false);

                        Task.Run(() =>
                        {
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                打开直播列表(下载对象);
                                MMPU.当前直播窗口数量++;
                               
                            }));
                        });
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("出现未知错误");
                return;
            }
        }
        public void 打开直播列表(Downloader DL)
        {

            if (DL != null)
            {
                DL.DownIofo.播放状态 = true;
                DL.DownIofo.是否是播放任务 = true;
                PlayW.MainWindow PlayWindow = new PlayW.MainWindow(DL, MMPU.默认音量, new SolidColorBrush(Color.FromArgb(0xFF, Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[1], 16), Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[2], 16), Convert.ToByte(MMPU.默认弹幕颜色.Split(',')[3], 16))), new SolidColorBrush(Color.FromArgb(0xFF, Convert.ToByte(MMPU.默认字幕颜色.Split(',')[1], 16), Convert.ToByte(MMPU.默认字幕颜色.Split(',')[2], 16), Convert.ToByte(MMPU.默认字幕颜色.Split(',')[3], 16))), MMPU.默认弹幕大小, MMPU.默认字幕大小, MMPU.播放器默认宽度, MMPU.播放器默认高度);
                PlayWindow.Closed += 播放窗口退出事件;
                PlayWindow.Show();
                //PlayWindow.BossKey += 老板键事件;
                playList1.Add(PlayWindow);




                // PlayW.MainWindow PlayWindow = new PlayW.MainWindow(DL, MMPU.默认音量, 弹幕颜色, 字幕颜色, MMPU.默认弹幕大小, MMPU.默认字幕大小, MMPU.PlayWindowW, MMPU.PlayWindowH);



                // MMPU.ClearMemory();
            }
            else
            {
                System.Windows.MessageBox.Show("Downloader结构体不能为Null,出现了未知的错误！");
                return;
            }

        }
        private void 播放窗口退出事件(object sender, EventArgs e)
        {
            NewThreadTask.Run(() =>
            {
                MMPU.当前直播窗口数量--;
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
            });
        }
    }
}
