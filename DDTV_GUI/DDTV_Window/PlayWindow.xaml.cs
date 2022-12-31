using HandyControl.Controls;
using LibVLCSharp.Shared;
using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dialog = HandyControl.Controls.Dialog;
using System.Windows.Threading;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript;
using System.Net;
using System.Text;
using DDTV_Core.SystemAssembly.ConfigModule;
using System.Collections.Generic;
using System.IO;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_Core.SystemAssembly.BilibiliModule.API;
using System.Drawing;
using System.Windows.Interop;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Windows.Controls;
using DDTV_Core.SystemAssembly.Log;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DDTV_GUI.DanMuCanvas.BarrageParameters;
using DDTV_GUI.WPFControl;
using System.Diagnostics;
using Point = System.Drawing.Point;
using Downloader;
using DDTV_Core.SystemAssembly.BilibiliModule.API.HLS;
using System.Windows.Media;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// PlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PlayWindow : GlowWindow
    {

        LibVLC vlcVideo;
        private static LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        bool IsClose = false;
        long uid = 0;
        string name = string.Empty;
        int roomId = 0;
        //string title = string.Empty;

        WPFControl.SendDanmuDialog sendDanmuDialog;
        public event EventHandler<EventArgs> DanMuConfigDialogDispose;
        private Dialog DanMuConfigDialog;//取色盘弹出窗口
        private DispatcherTimer VolumeTimer = new DispatcherTimer();
        int VolumeGridTime = 3;
        public event EventHandler<EventArgs> SendDialogDispose;
        public static event EventHandler<EventArgs> PlayListExit;//批量关闭播放窗口事件
        private bool IsOpenDanmu = false;
        private HttpWebResponse httpWebResponse;
        private Stream stream;
        private long TmpSize = 0;
        private bool IsDownloadStart = false;
        private string FileDirectory = string.Empty;
        private List<string> OldFileDirectoryList = new();
        private int Quality = 0;
        private int Line = 0;
        private List<int> QualityList = new();
        private int LastWidth;
        private int LastHeight;
        public WindowInfo windowInfo = new();
        private List<RunningBlock> DanmuBlock = new();
        private DispatcherTimer timer;
        private bool IsTopping = false;

        private double BlackListeningOriginalSize_Width = 0;//350
        private double BlackListeningOriginalSize_Height = 0;//200
        private bool IsBlackHear = false;//是否为黑听模式

        private ShowDanMuWindow showDanMuWindow = null;

        /// <summary>
        /// 屏幕渲染弹幕对象
        /// </summary>
        private BarrageConfig barrageConfig;

         private class SubtitlInfo
        {
            public string Text { get; set; }
        }

        private DownloadService downloader = new DownloadService();
        private DownloadConfiguration downloadOpt = new DownloadConfiguration()
        {
            BufferBlockSize = 4096, // 通常，主机最大支持8000字节，默认值为8000。
            ChunkCount = 1, // 要下载的文件分片数量，默认值为1
            //MaximumBytesPerSecond = 1024 * 1024, // 下载速度限制为1MB/s，默认值为零或无限制
            MaxTryAgainOnFailover = int.MaxValue, // 失败的最大次数
            Timeout = 2000, // 每个 stream reader  的超时（毫秒），默认值是1000
            RequestConfiguration = // 定制请求头文件
            {
                Accept = "*/*",
                //CookieContainer = NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie), // Add your cookies
                UserAgent = NetClass.UA(),
                ContentType = "application/x-www-form-urlencoded",
                Referer = "https://www.bilibili.com/"
            }
        };

    

        public List<string> ShieldDanMuText = new List<string>()
        {
            "老板大气"
        };


        public PlayWindow(long Uid, string Name, bool IsTemporaryPlay = false)
        {
            InitializeComponent();

           
            downloader = new DownloadService(downloadOpt);

            Task.Run(() =>
            {
                VideoView.Dispatcher.Invoke(() =>
                {
                    vlcVideo = new LibVLC();
                    _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(vlcVideo);
                    VideoView.MediaPlayer = _mediaPlayer;
                    VideoView.Loaded += (sender, e) => VideoView.MediaPlayer = _mediaPlayer;
                    VideoView.MediaPlayer.Playing += MediaPlayer_Playloading;
                    VideoView.MediaPlayer.EndReached += MediaPlayer_EndReached;
                    MainWindow.DefaultVolume = GUIConfig.DefaultVolume;
                    SetVolume(MainWindow.DefaultVolume);
                });
                Quality = GUIConfig.PlayQuality;
                uid = Uid;
                name = Name;
                SetMenuItemSwitchQuality();
                this.Dispatcher.Invoke(() =>
                {
                    UpdateWindowInfo();
                });

                
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                   
                    Play(uid);
                });
                VolumeTimer.Interval = new TimeSpan(0, 0, 0, 1); //参数分别为：天，小时，分，秒。此方法有重载，可根据实际情况调用
                VolumeTimer.Tick += VolumeTimer_Tick;
                VolumeTimer.Start();

                barrageConfig = new BarrageConfig(canvas);
                canvas.Dispatcher.Invoke(() =>
                {
                    canvas.Opacity = CoreConfig.DanMuFontOpacity;
                });
            });
            Loaded += new RoutedEventHandler(Topping);
            Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"启动播放窗口[UID:{Uid}]", false);
          
        }
        void Topping(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer1_Tick;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Topmost = true;
        }
        /// <summary>
        /// 设置右键菜单清晰度可选项
        /// </summary>
        public void SetMenuItemSwitchQuality()
        {
            QualityList = RoomInfo.GetQuality(uid);
            if (QualityList == null)
            {
                QualityList.Add(10000);
            }
            Dispatcher.Invoke(() =>
            {
                if (QualityList.Contains(10000))
                    qn10000.Visibility = Visibility.Visible;
                else
                    qn10000.Visibility = Visibility.Collapsed;
                if (QualityList.Contains(400))
                    qn400.Visibility = Visibility.Visible;
                else
                    qn400.Visibility = Visibility.Collapsed;
                if (QualityList.Contains(250))
                    qn250.Visibility = Visibility.Visible;
                else
                    qn250.Visibility = Visibility.Collapsed;
                if (QualityList.Contains(150))
                    qn150.Visibility = Visibility.Visible;
                else
                    qn150.Visibility = Visibility.Collapsed;
                if (QualityList.Contains(80))
                    qn80.Visibility = Visibility.Visible;
                else
                    qn80.Visibility = Visibility.Collapsed;
            });

        }

        /// <summary>
        /// 播放结束事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            if (Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
            {
                RefreshWindow();
            }
            else
            {
                EndGrid.Dispatcher.Invoke(() =>
                {
                    EndGrid.Visibility = Visibility.Visible;
                });

                Growl.WarningGlobal($"【{name}({roomId})】直播已结束");

                return;
            }
            Thread.Sleep(3000);
        }

        /// <summary>
        /// 设置静音
        /// </summary>
        public void SetMute()
        {
            SetVolume(0);
        }
        /// <summary>
        /// 定时隐藏音量显示UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeTimer_Tick(object? sender, EventArgs e)
        {
            VolumeGridTime--;
            if (VolumeGridTime < 0)
            {
                volume.Dispatcher.Invoke(new Action(() => volume.Visibility = Visibility.Collapsed));
                volumeText.Dispatcher.Invoke(() => volumeText.Visibility = Visibility.Collapsed);
                volume_Global_Check.Dispatcher.Invoke(() => volume_Global_Check.Visibility = Visibility.Collapsed);
            }
        }


        /// <summary>
        /// 隐藏加载界面事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaPlayer_Playloading(object? sender, EventArgs e)
        {
            loading.Dispatcher.Invoke(() => loading.Visibility = Visibility.Collapsed);
        }
        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="Uid"></param>
        private void Play(long Uid)
        {
            try
            {
                if (Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
                {
                    if (!QualityList.Contains(Quality))
                    {
                        Quality = 10000;
                        Growl.WarningGlobal($"{name}播间没有默认匹配的清晰度，已为您切换到原画");
                    }
                    string Url = RoomInfo.GetPlayUrl(Uid, (RoomInfoClass.Quality)Quality, (RoomInfoClass.Line)Line, true);
                    windowInfo.title = Rooms.GetValue(Uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.uname) + "-" + Rooms.GetValue(Uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.title);
                    roomId = int.Parse(Rooms.GetValue(Uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.room_id));
                    this.Dispatcher.Invoke(() =>
                        this.Title = windowInfo.title
                    );
                    StartDownload(Url);

                    Task.Run(() =>
                    {
                        Thread.Sleep(3000);
                        VideoView.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                if (!IsClose && VideoView.MediaPlayer != null)
                                {
                                    if (VideoView.MediaPlayer.IsPlaying)
                                    {
                                        VideoView.Dispatcher.Invoke(() => VideoView.MediaPlayer.Stop());
                                    }
                                    if (!IsClose)
                                    {
                                        if (!File.Exists(FileDirectory))
                                        {
                                             Growl.WarningGlobal($"{name}-直播间的直播流当前不可访问，该问题一般是由于B站服务或者网络代理造成的，请稍后再试");
                                            return;
                                        }
                                        else
                                        {
                                            VideoView.MediaPlayer.Play(new Media(vlcVideo, FileDirectory));
                                            //SetVolume(MainWindow.DefaultVolume);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Log.AddLog(nameof(PlayWindow), LogClass.LogType.Error, $"【{name}({roomId})】播放器初始化播放过程中出现未知错误，错误详情已写txt中", true, e, false);
                            }
                        });
                    });
                }
                else
                {
                    EndGrid.Dispatcher.Invoke(() =>
                    {
                        EndGrid.Visibility = Visibility.Visible;
                    });

                    Growl.WarningGlobal($"【{name}({roomId})】直播已结束");
                }
            }
            catch (Exception e)
            {
                Log.AddLog(nameof(PlayWindow), LogClass.LogType.Error, $"【{name}({roomId})】试图加载视频流至播放器，被系统阻止", true, e, false);
                Growl.WarningGlobal($"【{name}({roomId})】试图加载视频流至播放器，被系统阻止，请稍候再试");
            }
        }
        public void StartDownload(string Url)
        {
            if (!string.IsNullOrEmpty(FileDirectory))
            {
                DDTV_Core.Tool.FileOperation.Del(FileDirectory);
            }
            if (Download.TmpPath.Substring(Download.TmpPath.Length - 1, 1) != "/")
                Download.TmpPath = Download.TmpPath + "/";
            FileDirectory = DDTV_Core.Tool.FileOperation.CreateAll(Download.TmpPath) + Guid.NewGuid() + ".flv";
            OldFileDirectoryList.Add(FileDirectory);
            CancelDownload();

        R: if (Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
            {
                downloader.DownloadFileTaskAsync(Url, FileDirectory);
                //HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                //if (!DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.WhetherToEnableProxy)
                //{
                //    req.Proxy = null;
                //}
                //req.Method = "GET";
                //req.ContentType = "application/x-www-form-urlencoded";
                //req.Accept = "*/*";
                //req.UserAgent = NetClass.UA();
                //req.Referer = "https://www.bilibili.com/";
                //if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                //{
                //    req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie);
                //}
                //try
                //{
                //    httpWebResponse = (HttpWebResponse)req.GetResponse();
                //}
                //catch (Exception)
                //{
                //    Thread.Sleep(10000);
                //    goto R;
                //}
                //stream = httpWebResponse.GetResponseStream();
                //IsDownloadStart = true;
                //FileStream fileStream = new FileStream(FileDirectory, FileMode.Create);
                //Task.Run(() =>
                //{
                //    while (true)
                //    {
                //        int EndF = 0;
                //        if (stream.CanRead)
                //        {
                //            try
                //            {
                //                EndF = stream.ReadByte();
                //            }
                //            catch (Exception e)
                //            {
                //                EndF = -1;
                //            }
                //        }
                //        else
                //        {
                //            EndF = -1;
                //        }
                //        if (EndF != -1)
                //        {
                //            fileStream.Write(new byte[] { (byte)EndF }, 0, 1);
                //        }
                //        else
                //        {
                //            try
                //            {
                //                if (req != null) req.Abort();
                //            }
                //            catch (Exception) { }
                //            fileStream.Close();
                //            fileStream.Dispose();
                //            return;
                //        }
                //    }
                //});
            }
            else
            {
                EndGrid.Dispatcher.Invoke(() =>
                {
                    EndGrid.Visibility = Visibility.Visible;
                });

                Growl.WarningGlobal($"【{name}({roomId})】直播已结束");
            }
        }
        /// <summary>
        /// 取消任务并停止缓冲
        /// </summary>
        public void CancelDownload()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
            if (httpWebResponse != null)
            {
                httpWebResponse.Close();
                httpWebResponse.Dispose();
            }
          
            if(downloader.Status == DownloadStatus.Running)
            {
                downloader.CancelAsync();
            }
            IsDownloadStart = false;
        }

        /// <summary>
        /// 固定长宽比
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowInfo(false);
            if (DanmuBlock.Count > 0)
            {
                foreach (var item in DanmuBlock)
                {
                    item.Visibility = Visibility.Hidden;
                }
            }
            if (this.Width / 16.0 > this.Height / 9.0)
            {
                if (this.Width > LastWidth + 1)
                {
                    this.Height = this.Width / 1.7755;
                }
                else
                {
                    this.Width = this.Height * 1.7755;
                }
            }
            else
            {
                if (this.Height > LastHeight + 1)
                {
                    this.Width = this.Height * 1.7755;
                }
                else
                {
                    this.Height = this.Width / 1.7755;
                }
            }
            LastWidth = (int)this.Width;
            LastHeight = (int)this.Height;
        }
        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="i"></param>
        private void SetVolume(double i)
        {
            //if (VideoView.MediaPlayer.State == VLCState.Playing)
            {
                VideoView.Dispatcher.Invoke(() => VideoView.MediaPlayer.Volume = (int)i);
                volume.Dispatcher.Invoke(() => volume.Value = i);
                volumeText.Dispatcher.Invoke(() =>
                {
                    if (i.ToString().Split('.').Length > 1)
                    {
                        volumeText.Text = i.ToString().Split('.')[0];
                    }
                    else
                    {
                        volumeText.Text = i.ToString();
                    }
                });
                CoreConfig.SetValue(CoreConfigClass.Key.DefaultVolume, i.ToString("f0"), CoreConfigClass.Group.Play);
                GUIConfig.DefaultVolume = i;
                MainWindow.DefaultVolume = i;
            }

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }
        public void RefreshWindow()
        {
            loading.Dispatcher.Invoke(() => loading.Visibility = Visibility.Visible);

            Task.Run(() => Play(uid));
        }
        private void FullScreenSwitch()
        {
            if (this.WindowState == WindowState.Normal)
            {
                if (!IsBlackHear)
                {
                    this.WindowState = WindowState.Maximized;
                }
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        /// <summary>
        /// 关闭播放窗前触发，停止播放并回收播放器组件占用的内存，回收并清理临时文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClose = true;//记录关闭状态
            Task.Run(() =>
            {
                try
                {
                    Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"播放窗口【{name}({roomId})】触发Window_Closing", false);
                    CancelDownload();//取消任务并停止缓冲
                    LiveChatDispose();//直播间消息连接回收

                    if (showDanMuWindow != null)
                    {
                        //关闭播放窗时，如果播放窗口存在，一起关闭
                        showDanMuWindow.Dispatcher.Invoke(() => showDanMuWindow.Close());              
                    }

                    if (VideoView != null)
                    {
                        VideoView.Dispatcher.Invoke(() =>
                        {
                            if (VideoView.MediaPlayer != null && VideoView.MediaPlayer.IsPlaying)
                            {     
                                VideoView.MediaPlayer.Stop();//停止播放
                            }
                            else
                            {
                                Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"VideoView.MediaPlayer为Null！在播放窗口【{name}({roomId})】触发Window_Closing的时候", false);
                            }
                        }
                        );
                        VideoView.Dispatcher.Invoke(() =>
                            VideoView.Dispose()//结束播放器组件对象，并回收内存
                        );
                    }
                    foreach (var item in OldFileDirectoryList)
                    {
                        DDTV_Core.Tool.FileOperation.Del(item);//清理观看产生的临时文件
                    }
                    Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"播放窗口【{name}({roomId})】Window_Closing完成", false);
                }
                catch (Exception e)
                {
                    Log.AddLog(nameof(PlayWindow), LogClass.LogType.Warn, $"播放窗口【{name}({roomId})】Window_Closing时发生意外错误！", true, e, true);
                }
            });
        }
        private Dialog DG = null;

        /// <summary>
        /// 鼠标右键菜单_发送弹幕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Danmu_Send_Button_Click(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.MessageBox.Show("已优化弹幕发送窗口，请点击播放器右下角的弹幕发送组件开关填写弹幕");
            return;
            SendDialogDispose += PlayWindow_SendDialogDispose;
            sendDanmuDialog = new WPFControl.SendDanmuDialog(roomId, SendDialogDispose);
            DG = Dialog.Show(sendDanmuDialog);
        }

        private void PlayWindow_SendDialogDispose(object? sender, EventArgs e)
        {
            DG.Close();
        }

        private void volume_MouseMove(object sender, MouseEventArgs e)
        {
            if ((bool)volume_Global_Check.IsChecked)
            {
                foreach (var item in MainWindow.playWindowsList)
                {
                    item.SetVolume(volume.Value);
                }
            }
            else
            {
                SetVolume(volume.Value);
            }

        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            volume.Dispatcher.Invoke(new Action(() => volume.Visibility = Visibility.Visible));
            volumeText.Dispatcher.Invoke(() => volumeText.Visibility = Visibility.Visible);
            volume_Global_Check.Dispatcher.Invoke(() => volume_Global_Check.Visibility = Visibility.Visible);
            VolumeGridTime = 3;
            if (e.Delta > 0)
            {
                if (volume.Value + 5 <= 100)
                {
                    volume.Value += 5;

                    SetVolume(volume.Value);
                    //this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    volume.Value = 100;
                    SetVolume(volume.Value);
                    // this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
            else if (e.Delta < 0)
            {
                if (volume.Value - 5 >= 0)
                {
                    volume.Value -= 5;
                    SetVolume(volume.Value);
                    //  this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    volume.Value = 0;
                    SetVolume(volume.Value);
                    //  this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// 鼠标右键菜单_关闭本窗口事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 鼠标右键菜单_切换全屏事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_FullScreenSwitch_Send_Button_Click(object sender, RoutedEventArgs e)
        {
            FullScreenSwitch();
        }

        /// <summary>
        /// 鼠标右键菜单_刷新本窗口事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_RefreshWindow_Send_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshWindow();
        }

        /// <summary>
        /// 鼠标右键菜单_全局静音事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_SetMute_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in MainWindow.playWindowsList)
            {
                item.SetMute();
            }
        }

        /// <summary>
        /// 鼠标右键菜单_打开\关闭弹幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_OpenDamu_Click(object sender, RoutedEventArgs e)
        {

            //if (MainWindow.linkDMNum >= 3)
            //{
            //    Growl.InfoGlobal($"因为bilibili连接限制，最高只能打开3个房间的弹幕信息");
            //    return;
            //}

            IsOpenDanmu = !IsOpenDanmu;




            if (IsOpenDanmu)
            {
                Subtitle.Visibility = Visibility;
                Growl.InfoGlobal($"启动【{name}({roomId})】的弹幕连接");
                Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"启动【{name}({roomId})】的弹幕连接", false);

                canvas.Visibility = Visibility;

                Task.Run(() =>
                {
                    if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo RI))
                    {
                        if (RI.roomWebSocket.LiveChatListener == null || !RI.roomWebSocket.LiveChatListener.startIn)
                        {
                            MainWindow.linkDMNum++;
                            var T1 = DDTV_Core.SystemAssembly.BilibiliModule.API.WebSocket.WebSocket.ConnectRoomAsync(uid);
                            T1.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;

                        }
                        else
                        {
                            RI.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                        }
                    }

                });
            }
            else
            {
            
                canvas.Visibility = Visibility.Collapsed;
                Subtitle.Text = "";
                Subtitle.Visibility = Visibility.Collapsed;
                LiveChatDispose();
            }

        }
        /// <summary>
        /// 直播间消息连接回收
        /// </summary>
        public void LiveChatDispose()
        {

            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                if (roomInfo.roomWebSocket.LiveChatListener != null && roomInfo.roomWebSocket.LiveChatListener.startIn)
                {
                    MainWindow.linkDMNum--;
                    try
                    {
                        if (!roomInfo.IsDownload)
                        {
                            Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"播放窗口关闭，断开弹幕连接", false);
                            roomInfo.roomWebSocket.LiveChatListener.startIn = false;
                            roomInfo.roomWebSocket.IsConnect = false;
                            roomInfo.roomWebSocket.LiveChatListener.IsUserDispose = true;
                            roomInfo.roomWebSocket.LiveChatListener.Dispose();
                            Growl.InfoGlobal($"关闭【{name}({roomId})】的弹幕连接");
                        }
                        else
                        {
                            Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"播放窗口关闭，但是还有录制任务，不断开弹幕连接", false);
                        }
                        roomInfo.roomWebSocket.LiveChatListener.MessageReceived -= LiveChatListener_MessageReceived;

                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }


        private void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        {

            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    bool IsShiel = false;
                    foreach (var item in ShieldDanMuText)
                    {
                        if (Danmu.Message.Contains(item))
                        {
                            IsShiel = true;
                            break;
                        }
                    }
                    //Growl.Info("收到弹幕Danmu.Message");
                    if (!IsShiel)
                    {
                        if (Danmu.Message[..1] == "【")
                        {
                            AddDanmu(Danmu.Message, true);
                        }
                        else
                        {
                            AddDanmu(Danmu.Message, false);
                        }
                    }


                    //Console.WriteLine($"{Danmu.Message}");
                    break;
                    //case SuperchatEventArg SuperchatEvent:
                    //    Console.WriteLine($"{SuperchatEvent.UserName}({SuperchatEvent.UserId}):价值[{SuperchatEvent.Price}]的SC信息:【{SuperchatEvent.Message}】,翻译后:【{SuperchatEvent.messageTrans}】");
                    //    break;
                    //case GuardBuyEventArgs GuardBuyEvent:
                    //    Console.WriteLine($"{GuardBuyEvent.UserName}({GuardBuyEvent.UserId}):开通了{GuardBuyEvent.Number}个月的{GuardBuyEvent.GuardName}(单价{GuardBuyEvent.Price})");
                    //    break;
                    //case SendGiftEventArgs sendGiftEventArgs:
                    //    Console.WriteLine($"{sendGiftEventArgs.UserName}({sendGiftEventArgs.UserId}):价值{sendGiftEventArgs.GiftPrice}的{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}");
                    //    break;
                    //case EntryEffectEventArgs entryEffectEventArgs:
                    //    Console.WriteLine($"[舰长进入房间]舰长uid:{entryEffectEventArgs.uid},舰长头像{entryEffectEventArgs.face},欢迎信息:{entryEffectEventArgs.copy_writing}");
                    //    break;
                    //default:
                    //    break;
            }
        }

        /// <summary>
        /// 鼠标右键菜单_自动全屏平铺事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_WindowSorting_Click(object sender, RoutedEventArgs e)
        {

            //int screenheight = convert.toint32(systemparameters.primaryscreenheight / dpixratio);
            //int screenwidth = convert.toint32(systemparameters.primaryscreenwidth / dpixratio);


            //int ScreenHeight = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);
            //int ScreenWidth = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);

            //ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            //ScreenHeight= Screen.PrimaryScreen.Bounds.Height;


            Graphics currentgraphics = Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
            double dpixratio = currentgraphics.DpiX / 96;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            var screen = System.Windows.Forms.Screen.FromHandle(handle);

            double ScreenWidth = (double)screen.Bounds.Width / dpixratio;
            double ScreenHeight = (double)screen.Bounds.Height / dpixratio;


            if (MainWindow.playWindowsList.Count == 1)
            {
                MainWindow.playWindowsList[0].FullScreenSwitch();
            }
            else if (MainWindow.playWindowsList.Count > 1 && MainWindow.playWindowsList.Count < 5)
            {
                List<int[]> windows_4 = new List<int[]>();
                windows_4.Add(new int[] { 0, 0 });
                windows_4.Add(new int[] { (int)ScreenWidth / 2, 0 });
                windows_4.Add(new int[] { 0, (int)ScreenHeight / 2 });
                windows_4.Add(new int[] { (int)ScreenWidth / 2, (int)ScreenHeight / 2 });

                for (int i = 0; i < 4; i++)
                {
                    if (i >= MainWindow.playWindowsList.Count)
                    {
                        break;
                    }
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo
                    {
                        Width = (ScreenWidth / 2),
                        Height = (ScreenHeight / 2),
                        X = windows_4[i][0] + screen.Bounds.X,
                        Y = windows_4[i][1] + screen.Bounds.Y
                    });
                }
            }
            else if (MainWindow.playWindowsList.Count > 4 && MainWindow.playWindowsList.Count < 10)
            {
                List<int[]> windows_9 = new List<int[]>();
                windows_9.Add(new int[] { 0, 0 });
                windows_9.Add(new int[] { (int)(ScreenWidth / 3), 0 });
                windows_9.Add(new int[] { (int)(ScreenWidth / 3) + (int)(ScreenWidth / 3), 0 });
                windows_9.Add(new int[] { 0, (int)(ScreenHeight / 3) });
                windows_9.Add(new int[] { (int)(ScreenWidth / 3), (int)ScreenHeight / 3 });
                windows_9.Add(new int[] { (int)(ScreenWidth / 3) + (int)(ScreenWidth / 3), (int)ScreenHeight / 3 });
                windows_9.Add(new int[] { 0, (int)(ScreenHeight / 3) + (int)(ScreenHeight / 3) });
                windows_9.Add(new int[] { (int)(ScreenWidth / 3), (int)(ScreenHeight / 3) + (int)(ScreenHeight / 3) });
                windows_9.Add(new int[] { (int)(ScreenWidth / 3) + (int)(ScreenWidth / 3), (int)(ScreenHeight / 3) + (int)(ScreenHeight / 3) });
                for (int i = 0; i < 9; i++)
                {
                    if (i >= MainWindow.playWindowsList.Count)
                    {
                        break;
                    }
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                    {
                        Width = (ScreenWidth / 3),
                        Height = (ScreenHeight / 3),
                        X = windows_9[i][0] + screen.Bounds.X,
                        Y = windows_9[i][1] + screen.Bounds.Y
                    });
                }
            }
            else if (MainWindow.playWindowsList.Count > 9 && MainWindow.playWindowsList.Count < 25)
            {
                List<int[]> windows_16 = new List<int[]>();
                windows_16.Add(new int[] { 0, 0 });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4), 0 });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), 0 });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), 0 });
                windows_16.Add(new int[] { 0, (int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4), (int)ScreenHeight / 4 });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)ScreenHeight / 4 });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)ScreenHeight / 4 });
                windows_16.Add(new int[] { 0, (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { 0, (int)(ScreenHeight / 4) + (int)(int)(ScreenHeight / 4) + (int)(int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4), (int)(int)(ScreenHeight / 4) + (int)(int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
                windows_16.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
                for (int i = 0; i < 16; i++)
                {
                    if (i >= MainWindow.playWindowsList.Count)
                    {
                        break;
                    }
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                    {
                        Width = (ScreenWidth / 4),
                        Height = (ScreenHeight / 4),
                        X = windows_16[i][0] + screen.Bounds.X,
                        Y = windows_16[i][1] + screen.Bounds.Y
                    });
                }
            }
        }


        public bool SetWindowInfo(WindowInfo SW)
        {
            try
            {
                if (SW.X >= 0)
                {
                    this.Left = SW.X;
                }
                if (SW.Y >= 0)
                {
                    this.Top = SW.Y;
                }

                if (SW.Height > 0)
                {
                    this.Height = SW.Height;
                }
                if (SW.Width > 0)
                {
                    this.Width = SW.Width;
                }
                if (!string.IsNullOrEmpty(SW.title))
                {
                    this.Title = SW.title;
                }

                //if (this.Height != LastHeight)
                //{
                //    this.Width = this.Height * 1.75;
                //}
                if (this.Width != LastWidth)
                {
                    this.Height = this.Width / 1.77;
                }
                LastWidth = (int)this.Width;
                LastHeight = (int)this.Height;
                UpdateWindowInfo();
                return true;
            }
            catch (Exception)
            {
                UpdateWindowInfo();
                return false;
            }
        }
        public void UpdateWindowInfo(bool IsF = true)
        {
            try
            {
                windowInfo.X = this.Left;
                windowInfo.Y = this.Top;
                windowInfo.Width = this.Width;
                windowInfo.Height = this.Height;
                windowInfo.title = this.Title;
                if (IsF)
                {
                    this.Focus();
                }
            }
            catch (Exception) { }
        }
        public class WindowInfo
        {
            public double X { set; get; } = -1;
            public double Y { set; get; } = -1;
            public double Width { set; get; } = -1;
            public double Height { set; get; } = -1;
            public string title { set; get; } = null;
        }

        /// <summary>
        /// 鼠标右键菜单_在浏览器打开直播间事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_OpenLiveRoomUrl_Click(object sender, RoutedEventArgs e)
        {
            if (roomId == 0)
            {
                System.Windows.MessageBox.Show("参数初始化中，请播放窗口加载完成后再试");
            }
            string url = "https://live.bilibili.com/" + roomId;
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
            Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"在浏览器打开直播间[URL:{url}]", false);
            //string url = "https://live.bilibili.com/" + roomId;
            //System.Windows.Clipboard.SetDataObject(url);
            //Growl.SuccessGlobal("已复制直播间Url到粘贴板");
            //Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"制直播间Url到粘贴板[URL:{url}]", false);





        }

        /// <summary>
        /// 右键菜单_切换备线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_SwitchLine_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = e.Source as System.Windows.Controls.MenuItem;
            if (Line == int.Parse(menuItem.Tag.ToString()))
            {
                Growl.SuccessGlobal("当前正处于该线路");
                return;
            }
            switch (menuItem.Tag.ToString())
            {
                case "0":
                    Line = 0;
                    break;
                case "1":
                    Line = 1;
                    break;
                case "2":
                    Line = 2;
                    break;
                case "3":
                    Line = 3;
                    break;
            }
            Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"切换线路到[{Line}]", false);
            RefreshWindow();
        }
        /// <summary>
        /// 右键菜单_切换清晰度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_SwitchQuality_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = e.Source as System.Windows.Controls.MenuItem;
            if (Quality == int.Parse(menuItem.Tag.ToString()))
            {
                Growl.SuccessGlobal("当前正处于该清晰度");
                return;
            }
            switch (menuItem.Tag.ToString())
            {
                case "10000":
                    Quality = 10000;
                    break;
                case "400":
                    Quality = 400;
                    break;
                case "250":
                    Quality = 250;
                    break;
                case "150":
                    Quality = 150;
                    break;
                case "80":
                    Quality = 80;
                    break;
            }
            Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"切换清晰度到[{Quality}]", false);
            RefreshWindow();
        }

        /// <summary>
        /// 鼠标右键菜单_关闭全部播放窗口事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_ExitAll_Click(object sender, RoutedEventArgs e)
        {

            Growl.AskGlobal("确定要关闭所有播放窗口么？", isConfirmed =>
            {
                Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"关闭所有播放窗口", false);
                if (isConfirmed)
                {
                    if (PlayListExit != null)
                    {
                        PlayListExit.Invoke(null, EventArgs.Empty);
                    }
                }
                return true;
            });
        }

        private void AddDanmu(string DanmuText, bool IsSubtitle)
        {
            if (IsSubtitle)
            {
                byte R = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SubtitleColor.Split(',')[0], 16);
                byte G = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SubtitleColor.Split(',')[1], 16);
                byte B = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SubtitleColor.Split(',')[2], 16);
                Subtitle.Dispatcher.Invoke(new Action(() =>
                {
                    Subtitle.FontSize = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuFontSize;
                    Subtitle.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R, G, B));
                    Subtitle.Text = DanmuText;
                }));

            }
            else
            {
                Task.Run(() =>
            {
                //非UI线程调用UI组件
                System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    //显示弹幕
                    barrageConfig.Barrage(new DanMuCanvas.Models.MessageInformation() { content = DanmuText }, (int)this.Height, IsSubtitle);
                });
            });
            }



            //Task.Run(() =>
            //{
            //    RunningBlock runningBlock=null;
            //    PlayGrid.Dispatcher.Invoke(new Action(() => {
            //    runningBlock = new();
            //    runningBlock.AutoReverse = true;
            //    runningBlock.Duration = new Duration(new TimeSpan(0, 0, 10));
            //    runningBlock.IsRunning = true;
            //    runningBlock.FontSize = 30;
            //    runningBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.PaleVioletRed);
            //    runningBlock.BorderBrush = null;
            //    double Top = new Random().Next(0, (int)windowInfo.Height - 50);
            //    runningBlock.Margin = new Thickness(-50, Top, -50, (int)windowInfo.Height - Top - 80);
            //    StackPanel stackPanel = new StackPanel();
            //    OutlineText outlineText = new OutlineText();
            //    outlineText.Text = DanmuText;
            //    outlineText.FontWeight = FontWeights.Bold;
            //    outlineText.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            //    outlineText.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            //    outlineText.FontSize = 24;
            //    outlineText.StrokeThickness = 1;
            //    stackPanel.Children.Add(outlineText);
            //    runningBlock.Content = stackPanel;
            //    DanmuBlock.Add(runningBlock);

            //    PlayGrid.Children.Add(runningBlock);

            //}));



            //    runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Visible));
            //    Thread.Sleep(5);
            //    runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Collapsed));
            //    Thread.Sleep(10000);
            //    if(runningBlock.Visibility==Visibility.Collapsed)
            //    {
            //        runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Visible));
            //    }
            //    Thread.Sleep(10000);
            //    runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Collapsed));
            //    if(DanmuBlock.Contains(runningBlock))
            //    {
            //        DanmuBlock.Remove(runningBlock);
            //        PlayGrid.Dispatcher.Invoke(new Action(() => PlayGrid.Children.Remove(runningBlock)));

            //    }

            //});
        }

        /// <summary>
        /// 鼠标右键菜单_设置/锁定锁定置顶事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Topping_Click(object sender, RoutedEventArgs e)
        {
            IsTopping = !IsTopping;
            if (IsTopping)
            {
                this.Topmost = true;
                timer.Start();
            }
            else
            {
                timer.Stop();
                this.Topmost = false;
            }
        }

        /// <summary>
        /// 鼠标右键菜单_查看当前播放窗口详情事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayWindowInfo_Click(object sender, RoutedEventArgs e)
        {
            string text = "数据获取中，请稍候再查看";
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                text =
                    $"用户名:\r\n{roomInfo.uname}\r\n\r\n" +
                    $"UID:\r\n{roomInfo.uid}\r\n\r\n" +
                    $"标题:\r\n{roomInfo.title}\r\n\r\n" +
                    $"房间号:\r\n{roomInfo.room_id}\r\n\r\n" +
                    $"Host:\r\n{roomInfo.Host}\r\n\r\n";
            }
            HandyControl.Controls.MessageBox.Show(text);
        }

        /// <summary>
        /// 鼠标右键菜单_赛事模式全屏平铺事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_GuideMode_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = e.Source as System.Windows.Controls.MenuItem;
            switch (menuItem.Tag.ToString())
            {
                case "1_6":
                    Guide1_6Mode();
                    MainWindow.guideMode = MainWindow.GuideMode.W1_6;
                    break;
                case "1_8":
                    Guide1_8Mode();
                    MainWindow.guideMode = MainWindow.GuideMode.W1_8;
                    break;
                case "1_13":
                    Guide1_13Mode();
                    MainWindow.guideMode = MainWindow.GuideMode.W1_13;
                    break;
            }
        }

        /// <summary>
        /// 赛事模式_1-6
        /// </summary>
        private void Guide1_6Mode()
        {
            Graphics currentgraphics = Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
            double dpixratio = currentgraphics.DpiX / 96;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            var screen = System.Windows.Forms.Screen.FromHandle(handle);

            double ScreenWidth = (double)screen.Bounds.Width / dpixratio;
            double ScreenHeight = (double)screen.Bounds.Height / dpixratio;

            List<int[]> windows_1_6 = new List<int[]>();
            windows_1_6.Add(new int[] { 0, 0 });
            windows_1_6.Add(new int[] { (int)(ScreenWidth / 3) + (int)(ScreenWidth / 3), 0 });
            windows_1_6.Add(new int[] { (int)(ScreenWidth / 3) + (int)(ScreenWidth / 3), (int)ScreenHeight / 3 });
            windows_1_6.Add(new int[] { 0, (int)(ScreenHeight / 3) + (int)(ScreenHeight / 3) });
            windows_1_6.Add(new int[] { (int)(ScreenWidth / 3), (int)(ScreenHeight / 3) + (int)(ScreenHeight / 3) });
            windows_1_6.Add(new int[] { (int)(ScreenWidth / 3) + (int)(ScreenWidth / 3), (int)(ScreenHeight / 3) + (int)(ScreenHeight / 3) });

            MainWindow.playWindowsList[0].SetWindowInfo(new WindowInfo()
            {
                Width = (ScreenWidth / 3) * 2,
                Height = (ScreenHeight / 3) * 2,
                X = windows_1_6[0][0] + screen.Bounds.X,
                Y = windows_1_6[0][1] + screen.Bounds.Y
            });


            for (int i = 1; i < 6; i++)
            {
                if (i >= MainWindow.playWindowsList.Count)
                {
                    break;
                }
                MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                {
                    Width = (ScreenWidth / 3),
                    Height = (ScreenHeight / 3),
                    X = windows_1_6[i][0] + screen.Bounds.X,
                    Y = windows_1_6[i][1] + screen.Bounds.Y
                });
            }
        }

        /// <summary>
        /// 赛事模式_1-8
        /// </summary>
        private void Guide1_8Mode()
        {
            Graphics currentgraphics = Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
            double dpixratio = currentgraphics.DpiX / 96;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            var screen = System.Windows.Forms.Screen.FromHandle(handle);

            double ScreenWidth = (double)screen.Bounds.Width / dpixratio;
            double ScreenHeight = (double)screen.Bounds.Height / dpixratio;

            List<int[]> windows_1_8 = new List<int[]>();
            windows_1_8.Add(new int[] { 0, 0 });
            windows_1_8.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), 0 });
            windows_1_8.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)ScreenHeight / 4 });
            windows_1_8.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_8.Add(new int[] { 0, (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_8.Add(new int[] { (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_8.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_8.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });

            MainWindow.playWindowsList[0].SetWindowInfo(new WindowInfo()
            {
                Width = (ScreenWidth / 4) * 3,
                Height = (ScreenHeight / 4) * 3,
                X = windows_1_8[0][0] + screen.Bounds.X,
                Y = windows_1_8[0][1] + screen.Bounds.Y
            });


            for (int i = 1; i < 8; i++)
            {
                if (i >= MainWindow.playWindowsList.Count)
                {
                    break;
                }
                MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                {
                    Width = (ScreenWidth / 4),
                    Height = (ScreenHeight / 4),
                    X = windows_1_8[i][0] + screen.Bounds.X,
                    Y = windows_1_8[i][1] + screen.Bounds.Y
                });
            }
        }

        /// <summary>
        /// 赛事模式_1-13
        /// </summary>
        private void Guide1_13Mode()
        {
            Graphics currentgraphics = Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
            double dpixratio = currentgraphics.DpiX / 96;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            var screen = System.Windows.Forms.Screen.FromHandle(handle);

            double ScreenWidth = (double)screen.Bounds.Width / dpixratio;
            double ScreenHeight = (double)screen.Bounds.Height / dpixratio;

            List<int[]> windows_1_13 = new List<int[]>();
            windows_1_13.Add(new int[] { 0, 0 });
            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4), 0 });

            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), 0 });
            windows_1_13.Add(new int[] { 0, (int)(ScreenHeight / 4) });


            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)ScreenHeight / 4 });
            windows_1_13.Add(new int[] { 0, (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_13.Add(new int[] { 0, (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });
            windows_1_13.Add(new int[] { (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4) + (int)(ScreenWidth / 4), (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) + (int)(ScreenHeight / 4) });




            for (int i = 0; i < 13; i++)
            {
                if (i >= MainWindow.playWindowsList.Count)
                {
                    break;
                }

                if (i == 1)
                {
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                    {
                        Width = (ScreenWidth / 4) * 2,
                        Height = (ScreenHeight / 4) * 2,
                        X = windows_1_13[i][0] + screen.Bounds.X,
                        Y = windows_1_13[i][1] + screen.Bounds.Y
                    });
                }
                else
                {
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                    {
                        Width = (ScreenWidth / 4),
                        Height = (ScreenHeight / 4),
                        X = windows_1_13[i][0] + screen.Bounds.X,
                        Y = windows_1_13[i][1] + screen.Bounds.Y
                    });
                }
            }
        }

        /// <summary>
        /// 鼠标右键菜单_赛事模式设置为主窗口事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_SetGuideModeMianWindow_Click(object sender, RoutedEventArgs e)
        {
            int ti = MainWindow.playWindowsList.IndexOf(this);
            switch (MainWindow.guideMode)
            {
                case MainWindow.GuideMode.N:

                    break;
                case MainWindow.GuideMode.W1_6:
                    {
                        PlayWindow playWindow = MainWindow.playWindowsList[0];
                        MainWindow.playWindowsList[0] = MainWindow.playWindowsList[ti];
                        MainWindow.playWindowsList[ti] = playWindow;
                    }
                    break;
                case MainWindow.GuideMode.W1_8:
                    {
                        PlayWindow playWindow = MainWindow.playWindowsList[0];
                        MainWindow.playWindowsList[0] = MainWindow.playWindowsList[ti];
                        MainWindow.playWindowsList[ti] = playWindow;
                    }
                    break;
                case MainWindow.GuideMode.W1_13:
                    {
                        if (MainWindow.playWindowsList.Count > 1)
                        {
                            PlayWindow playWindow = MainWindow.playWindowsList[1];
                            MainWindow.playWindowsList[1] = MainWindow.playWindowsList[ti];
                            MainWindow.playWindowsList[ti] = playWindow;
                        }
                    }
                    break;
            }



            switch (MainWindow.guideMode)
            {
                case MainWindow.GuideMode.N:
                    break;
                case MainWindow.GuideMode.W1_6:
                    Guide1_6Mode();
                    break;
                case MainWindow.GuideMode.W1_8:
                    Guide1_8Mode();
                    break;
                case MainWindow.GuideMode.W1_13:
                    Guide1_13Mode();
                    break;
            }
        }

        private void MenuItem_DamuConfig_Click(object sender, RoutedEventArgs e)
        {
            DanMuConfigDialogDispose += PlayWindow_DanMuConfigDialogDispose;
            DanMuConfig danMuConfigWindow = new DanMuConfig(DanMuConfigDialogDispose);
            DanMuConfigDialog = Dialog.Show(danMuConfigWindow);
        }

        private void PlayWindow_DanMuConfigDialogDispose(object? sender, EventArgs e)
        {
            canvas.Opacity = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuFontOpacity;
            DanMuConfigDialog.Close();
        }

        int i = 0;
        double FullScreenMonitor_Top = 0;
        double FullScreenMonitor_Left = 0;
        /// <summary>
        /// 双击播放窗口全屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (i == 0)
            {
                FullScreenMonitor_Top = this.Top;
                FullScreenMonitor_Left = this.Left;
            }

            i += 1;
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 500),
                IsEnabled = true,  
            };
            timer.Tick += (sender1, e1) => { timer.IsEnabled = false; i = 0; };
            if (i == 2)
            {
                if( FullScreenMonitor_Top == this.Top && FullScreenMonitor_Left == this.Left)
                {
                     FullScreenSwitch();
                }
                timer.IsEnabled = false;
                i = 0;
            }
        }

        /// <summary>
        /// 播放窗口音频solo功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_SetMuteSolo_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in MainWindow.playWindowsList)
            {
                if (item != this)
                {
                    item.SetMute();
                }
            }
        }

        private void GlowWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //音量增加
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Up))
            {
                volume.Dispatcher.Invoke(new Action(() => volume.Visibility = Visibility.Visible));
                volumeText.Dispatcher.Invoke(() => volumeText.Visibility = Visibility.Visible);
                volume_Global_Check.Dispatcher.Invoke(() => volume_Global_Check.Visibility = Visibility.Visible);
                VolumeGridTime = 3;
                if (volume.Value + 5 <= 100)
                {
                    SetVolume(volume.Value + 5);
                }
                else
                {
                    SetVolume(100);
                }
            }
            //音量降低
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Down))
            {
                volume.Dispatcher.Invoke(new Action(() => volume.Visibility = Visibility.Visible));
                volumeText.Dispatcher.Invoke(() => volumeText.Visibility = Visibility.Visible);
                volume_Global_Check.Dispatcher.Invoke(() => volume_Global_Check.Visibility = Visibility.Visible);
                VolumeGridTime = 3;
                if (volume.Value - 5 >= 0)
                {
                    SetVolume(volume.Value - 5);
                }
                else
                {
                    SetVolume(0);
                }
            }
            //全屏回车
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                FullScreenSwitch();
            }
            //F5刷新
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.F5))
            {
                RefreshWindow();
            }
        }

        /// <summary>
        /// 右键菜单_黑听模式切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_OnlyPlayAudio_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItem_OnlyPlayAudio.Tag.ToString() == "0")
            {
                IsBlackHear = true;
                FullScreenSwitch();
                MenuItem_OnlyPlayAudio.Tag = "1";
                BlackListeningOriginalSize_Width = this.Width;
                BlackListeningOriginalSize_Height = this.Height;
                this.Width = 350;
                this.Height = 200;
                BlackListeningTips.Visibility = Visibility.Visible;
                MenuItem_OnlyPlayAudio.Header = "退出只播音频";
                MenuItem_SwitchQuality.Visibility = Visibility.Collapsed;
                MenuItem_FullScreenSwitch.Visibility = Visibility.Collapsed;
                MenuItem_WindowSorting.Visibility = Visibility.Collapsed;

            }
            else
            {
                IsBlackHear = false;
                MenuItem_OnlyPlayAudio.Tag = "0";
                this.Width = BlackListeningOriginalSize_Width;
                this.Height = BlackListeningOriginalSize_Height;
                BlackListeningTips.Visibility = Visibility.Collapsed;
                MenuItem_OnlyPlayAudio.Header = "只放音频";
                MenuItem_SwitchQuality.Visibility = Visibility.Visible;
                MenuItem_FullScreenSwitch.Visibility = Visibility.Visible;
                MenuItem_WindowSorting.Visibility = Visibility.Visible;

            }
        }

        private void DanMuGridSwitch_Click(object sender, RoutedEventArgs e)
        {
            if(DanMuSendGrid.Visibility== Visibility.Visible)
            {
                DanMuSendGrid.Visibility= Visibility.Collapsed;
            }
            else
            {
                DanMuSendGrid.Visibility = Visibility.Visible;
            }
        }

        private void DanMuInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string Massage = DanMuInput.Text;
                if (Massage.Length > 30)
                {
                    Growl.Warning("发送的弹幕长度尝过限制(30个字符)");
                    return;
                }
                else if (Massage.Length < 1)
                {
                    Growl.Warning("发送的弹幕内容不能为空");
                    return;
                }
                DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu.DanMu.Send(Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.room_id), Massage);
                Growl.Success("发送弹幕完成");
                DanMuInput.Text = "";
            }
        }

        private void MenuItem_OpenSeparateDanmuWindow_Click(object sender, RoutedEventArgs e)
        {
            ShowDanMuWindow danMuShowWindow = new ShowDanMuWindow(uid);
            showDanMuWindow = danMuShowWindow;
            danMuShowWindow.Show();
        }

        private void MenuItem_SwitchSubtitleDisplay_Click(object sender, RoutedEventArgs e)
        {
            if(!IsOpenDanmu)
            {
                 Growl.WarningGlobal("切换野生字幕显示状态失败！原因：未打开弹幕功能");
            }
            else
            {
                if(Subtitle.Visibility== Visibility.Visible)
                {
                    Subtitle.Visibility= Visibility.Collapsed;
                    Subtitle.Text = "";
                }
                else
                {
                    Subtitle.Visibility = Visibility.Visible;
                }
            }
        }

        private void playSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClose && VideoView.MediaPlayer != null)
            {
                if (VideoView.MediaPlayer.IsPlaying)
                {
                    playSwitch.Content = "暂停";
                    VideoView.MediaPlayer.Pause();
                }
                else
                {
                    playSwitch.Content = "播放";
                    VideoView.MediaPlayer.Play();
                }
            }
        }
    }
}
