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
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Windows.Controls;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// PlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PlayWindow : GlowWindow
    {

        LibVLC vlcVideo;
        bool IsClose = false;
        long uid = 0;
        int roomId = 0;
        //string title = string.Empty;
        private static MediaPlayer _mediaPlayer;
        WPFControl.SendDanmuDialog sendDanmuDialog;
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
        private List<int> QualityList= new();
        private int LastWidth;
        private int LastHeight;
        public WindowInfo windowInfo = new();
        private List<RunningBlock> DanmuBlock = new();
        public PlayWindow(long Uid)
        {
           
            InitializeComponent();
            Quality = MainWindow.PlayQuality;
            uid = Uid;
            SetMenuItemSwitchQuality();

            UpdateWindowInfo();
            Core.Initialize("./plugins/vlc");
            vlcVideo = new LibVLC();
            _mediaPlayer = new MediaPlayer(vlcVideo);
            VideoView.MediaPlayer = _mediaPlayer;
            VideoView.Loaded += (sender, e) => VideoView.MediaPlayer = _mediaPlayer;
            VideoView.MediaPlayer.Playing += MediaPlayer_Playing;
            VideoView.MediaPlayer.EndReached += MediaPlayer_EndReached;
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                Play(uid);
            });
            VolumeTimer.Interval = new TimeSpan(0, 0, 0, 1); //参数分别为：天，小时，分，秒。此方法有重载，可根据实际情况调用
            VolumeTimer.Tick += VolumeTimer_Tick;
            VolumeTimer.Start();
        }
        public void SetMenuItemSwitchQuality()
        {
            QualityList = RoomInfo.GetQuality(uid);
            if (QualityList == null)
            {
                QualityList.Add(10000);
            }
            if(QualityList.Contains(10000))
                qn10000.Visibility= Visibility.Visible;
            else
                qn10000.Visibility= Visibility.Collapsed;
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
        }

        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            if (Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
            {
                RefreshWindow();
            }
            else
            {
                Growl.Warning("该直播间直播已结束");
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
        private void VolumeTimer_Tick(object? sender, EventArgs e)
        {
            VolumeGridTime--;
            if (VolumeGridTime < 0)
            {
                volume.Dispatcher.Invoke(new Action(() => volume.Visibility = Visibility.Collapsed));
                volumeText.Dispatcher.Invoke(() => volumeText.Visibility = Visibility.Collapsed);
            }
        }

        //public class V
        //{
        //    public double Value{ get; set; } = 0;
        //}

        private void MediaPlayer_Playing(object? sender, EventArgs e)
        {
            loading.Dispatcher.Invoke(() => loading.Visibility = Visibility.Collapsed);
        }
        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="Uid"></param>
        private void Play(long Uid)
        {
            if (Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
            {
                if(!QualityList.Contains(Quality))
                {
                    Quality = 10000;
                    Growl.Warning("该直播间没有默认匹配的清晰度，当前直播间已为您切换到原画");
                }
                string Url = RoomInfo.playUrl(Uid, (RoomInfoClass.Quality)Quality, (RoomInfoClass.Line)Line);
                windowInfo.title = Rooms.GetValue(Uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.title);
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
                                VideoView.MediaPlayer.Play(new Media(vlcVideo, FileDirectory));
                                SetVolume(MainWindow.DefaultVolume);
                            }
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                    });
                });
            }
            else
            {
                Growl.Ask("该直播间已下播，是否关闭本窗口", isConfirmed =>
                {
                    if (isConfirmed)
                    {
                        this.Close();
                    }
                    return true;
                });
            }
        }
        public void StartDownload(string Url)
        {

            if (Download.TmpPath.Substring(Download.TmpPath.Length - 1, 1) != "/")
                Download.TmpPath = Download.TmpPath + "/";
            FileDirectory = DDTV_Core.Tool.FileOperation.CreateAll(Download.TmpPath) + Guid.NewGuid() + ".flv";
            OldFileDirectoryList.Add(FileDirectory);
            CancelDownload();

           R: if (Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
            {
                string U2 = RoomInfo.playUrl(uid, (RoomInfoClass.Quality)Quality, (RoomInfoClass.Line)Line);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                req.Method = "GET";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Accept = "*/*";
                req.UserAgent = NetClass.UA();
                req.Referer = "https://www.bilibili.com/";
                if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                {
                    req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie);
                }
                try
                {
                    httpWebResponse = (HttpWebResponse)req.GetResponse();
                }
                catch (Exception)
                {
                    Thread.Sleep(10000);
                    goto R;
                }
                stream = httpWebResponse.GetResponseStream();
                IsDownloadStart = true;
                FileStream fileStream = new FileStream(FileDirectory, FileMode.Create);
                Task.Run(() =>
                {
                    while (true)
                    {
                        int EndF = 0;
                        if (stream.CanRead)
                        {
                            try
                            {
                                EndF = stream.ReadByte();
                            }
                            catch (Exception e)
                            {
                                EndF = -1;
                            }
                        }
                        else
                        {
                            EndF = -1;
                        }
                        if (EndF != -1)
                        {
                            fileStream.Write(new byte[] { (byte)EndF }, 0, 1);
                        }
                        else
                        {
                            fileStream.Close();
                            fileStream.Dispose();
                            return;
                        }
                    }

                });
            }
            else
            {
                Growl.Ask("该直播间已下播，是否关闭本窗口", isConfirmed =>
                {
                    if (isConfirmed)
                    {
                        this.Close();
                    }
                    return true;
                });
            }
        }
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
            IsDownloadStart = false;
        }

        /// <summary>
        /// 固定长宽比
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowInfo();
            foreach (var item in DanmuBlock)
            {
                item.Visibility = Visibility.Hidden;
            }
            //double X = this.Width;
            //double Y = this.Height;
            //if (X / Y > 1.77)
            //{
            //    X =Y / 9 * 16;
            //    this.Width = X;
            //}
            //else
            //{
            //    Y = X / 16 * 9;
            //    this.Height = Y;
            //}
        }
        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="i"></param>
        private void SetVolume(double i)
        {
            VideoView.Dispatcher.Invoke(() => VideoView.MediaPlayer.Volume = (int)i);
            volume.Dispatcher.Invoke(() => volume.Value = i);
            volumeText.Dispatcher.Invoke(() => volumeText.Text = i.ToString());
            //CoreConfig.GetValue(CoreConfigClass.Key.DefaultVolume, "50", CoreConfigClass.Group.Play)
            CoreConfig.SetValue(CoreConfigClass.Key.DefaultVolume, i.ToString("f0"), CoreConfigClass.Group.Play);
            MainWindow.DefaultVolume = i;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //音量增加
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Up))
            {
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
        public void RefreshWindow()
        {
            loading.Dispatcher.Invoke(() => loading.Visibility = Visibility.Visible);

            Task.Run(() => Play(uid));
        }
        private void FullScreenSwitch()
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClose = true;
            CancelDownload();
            LiveChatDispose();
            foreach (var item in OldFileDirectoryList)
            {
                DDTV_Core.Tool.FileOperation.Del(item);
            }

            VideoView.Dispatcher.Invoke(() =>
               VideoView.MediaPlayer.Stop()
           );
            if (VideoView != null)
            {
                VideoView.Dispose();
            }
            if (vlcVideo != null)
            {
                vlcVideo.Dispose();
            }
        }
        private Dialog DG = null;
        private void Danmu_Send_Button_Click(object sender, RoutedEventArgs e)
        {
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
            SetVolume(volume.Value);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            volume.Dispatcher.Invoke(new Action(() => volume.Visibility = Visibility.Visible));
            volumeText.Dispatcher.Invoke(() => volumeText.Visibility = Visibility.Visible);
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
                this.DragMove();
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_FullScreenSwitch_Send_Button_Click(object sender, RoutedEventArgs e)
        {
            FullScreenSwitch();
        }

        private void MenuItem_RefreshWindow_Send_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshWindow();
        }

        private void MenuItem_SetMute_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in MainWindow.playWindowsList)
            {
                item.SetMute();
            }
        }

        private void MenuItem_OpenDamu_Click(object sender, RoutedEventArgs e)
        {
            
            IsOpenDanmu = !IsOpenDanmu;
            if (IsOpenDanmu)
            {
                Growl.Info($"启动{roomId}房间的弹幕连接,10秒后开始显示弹幕");
                var roomInfo = DDTV_Core.SystemAssembly.BilibiliModule.API.WebSocket.WebSocket.ConnectRoomAsync(uid);
                roomInfo.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
            }
            else
            {
                LiveChatDispose();
            }
        }
        public void LiveChatDispose()
        {
            
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                if (roomInfo.roomWebSocket.LiveChatListener != null&& roomInfo.roomWebSocket.LiveChatListener.startIn)
                {
                    Growl.Info($"关闭{roomId}房间的弹幕连接");
                    try
                    {
                        roomInfo.roomWebSocket.LiveChatListener.startIn = false;
                        roomInfo.roomWebSocket.IsConnect = false;
                        roomInfo.roomWebSocket.LiveChatListener.Dispose();
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
                    //Growl.Info("收到弹幕Danmu.Message");
                    AddDanmu(Danmu.Message);
                    
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

        private void MenuItem_WindowSorting_Click(object sender, RoutedEventArgs e)
        {
            WindowSorting();
        }
        public void WindowSorting()
        {
            Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
            double dpixRatio = currentGraphics.DpiX / 96;
            int ScreenHeight = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height / dpixRatio);
            int ScreenWidth = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width / dpixRatio);


            if (MainWindow.playWindowsList.Count == 1)
            {
                MainWindow.playWindowsList[0].FullScreenSwitch();
            }
            else if (MainWindow.playWindowsList.Count > 1 && MainWindow.playWindowsList.Count < 5)
            {
                List<int[]> windows_4 = new List<int[]>();
                windows_4.Add(new int[] { 0, 0 });
                windows_4.Add(new int[] { ScreenWidth / 2, 0 });
                windows_4.Add(new int[] { 0, ScreenHeight / 2 });
                windows_4.Add(new int[] { ScreenWidth / 2, ScreenHeight / 2 });
                for (int i = 0 ; i < 4 ; i++)
                {
                    if (i >= MainWindow.playWindowsList.Count)
                    {
                        break;
                    }
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo
                    {
                        Width = (ScreenWidth / 2),
                        Height = (ScreenHeight / 2),
                        X = windows_4[i][0],
                        Y = windows_4[i][1]
                    });
                }
            }
            else if (MainWindow.playWindowsList.Count > 4 && MainWindow.playWindowsList.Count < 10)
            {
                List<int[]> windows_9 = new List<int[]>();
                windows_9.Add(new int[] { 0, 0 });
                windows_9.Add(new int[] { (ScreenWidth / 3), 0 });
                windows_9.Add(new int[] { (ScreenWidth / 3) + (ScreenWidth / 3), 0 });
                windows_9.Add(new int[] { 0, (ScreenHeight / 3) });
                windows_9.Add(new int[] { (ScreenWidth / 3), ScreenHeight / 3 });
                windows_9.Add(new int[] { (ScreenWidth / 3) + (ScreenWidth / 3), ScreenHeight / 3 });
                windows_9.Add(new int[] { 0, (ScreenHeight / 3) + (ScreenHeight / 3) });
                windows_9.Add(new int[] { (ScreenWidth / 3), (ScreenHeight / 3) + (ScreenHeight / 3) });
                windows_9.Add(new int[] { (ScreenWidth / 3) + (ScreenWidth / 3), (ScreenHeight / 3) + (ScreenHeight / 3) });
                for (int i = 0 ; i < 9 ; i++)
                {
                    if (i >= MainWindow.playWindowsList.Count)
                    {
                        break;
                    }
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                    {
                        Width = (ScreenWidth / 3),
                        Height = (ScreenHeight / 3),
                        X = windows_9[i][0],
                        Y = windows_9[i][1]
                    });
                }
            }
            else if (MainWindow.playWindowsList.Count > 9)
            {
                List<int[]> windows_16 = new List<int[]>();
                windows_16.Add(new int[] { 0, 0 });
                windows_16.Add(new int[] { (ScreenWidth / 4), 0 });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4), 0 });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4) + (ScreenWidth / 4), 0 });
                windows_16.Add(new int[] { 0, (ScreenHeight / 4) });
                windows_16.Add(new int[] { (ScreenWidth / 4), ScreenHeight / 4 });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4), ScreenHeight / 4 });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4) + (ScreenWidth / 4), ScreenHeight / 4 });
                windows_16.Add(new int[] { 0, (ScreenHeight / 4) + (ScreenHeight / 4) });
                windows_16.Add(new int[] { (ScreenWidth / 4), (ScreenHeight / 4) + (ScreenHeight / 4) });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4), (ScreenHeight / 4) + (ScreenHeight / 4) });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4) + (ScreenWidth / 4), (ScreenHeight / 4) + (ScreenHeight / 4) });
                windows_16.Add(new int[] { 0, (ScreenHeight / 4) + (ScreenHeight / 4) + (ScreenHeight / 4) });
                windows_16.Add(new int[] { (ScreenWidth / 4), (ScreenHeight / 4) + (ScreenHeight / 4) + (ScreenHeight / 4) });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4), (ScreenHeight / 4) + (ScreenHeight / 4) + (ScreenHeight / 4) });
                windows_16.Add(new int[] { (ScreenWidth / 4) + (ScreenWidth / 4) + (ScreenWidth / 4), (ScreenHeight / 4) + (ScreenHeight / 4) + (ScreenHeight / 4) });
                for (int i = 0 ; i < 16 ; i++)
                {
                    if (i >= MainWindow.playWindowsList.Count)
                    {
                        break;
                    }
                    MainWindow.playWindowsList[i].SetWindowInfo(new WindowInfo()
                    {
                        Width = (ScreenWidth / 4),
                        Height = (ScreenHeight / 4),
                        X = windows_16[i][0],
                        Y = windows_16[i][1]
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
        public void UpdateWindowInfo()
        {
            try
            {
                windowInfo.X= this.Left;
                windowInfo.Y = this.Top;
                windowInfo.Width = this.Width;
                windowInfo.Height = this.Height;
                windowInfo.title = this.Title;
                this.Focus();
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
        private void MenuItem_CopyLiveRoomUrl_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject("https://live.bilibili.com/" + roomId);
            Growl.Success("已复制直播间Url到粘贴板");
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
                Growl.Success("当前正处于该线路");
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
                Growl.Success("当前正处于该清晰度");
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
            RefreshWindow();
        }

        private void MenuItem_ExitAll_Click(object sender, RoutedEventArgs e)
        {
            Growl.Ask("确定要关闭所有播放窗口么？", isConfirmed =>
            {
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

        private void AddDanmu(string DanmuText)
        {
            Task.Run(() =>
            {
                RunningBlock runningBlock=null;
                PlayGrid.Dispatcher.Invoke(new Action(() => {
                runningBlock = new();
                runningBlock.AutoReverse = true;
                runningBlock.Duration = new Duration(new TimeSpan(0, 0, 10));
                runningBlock.IsRunning = true;
                runningBlock.FontSize = 30;
                runningBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.PaleVioletRed);
                runningBlock.BorderBrush = null;
                double Top = new Random().Next(0, (int)windowInfo.Height - 50);
                runningBlock.Margin = new Thickness(-50, Top, -50, (int)windowInfo.Height - Top - 80);
                StackPanel stackPanel = new StackPanel();
                OutlineText outlineText = new OutlineText();
                outlineText.Text = DanmuText;
                outlineText.FontWeight = FontWeights.Bold;
                outlineText.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                outlineText.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                outlineText.FontSize = 24;
                outlineText.StrokeThickness = 1;
                stackPanel.Children.Add(outlineText);
                runningBlock.Content = stackPanel;
                DanmuBlock.Add(runningBlock);

                PlayGrid.Children.Add(runningBlock);

            }));



                runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Visible));
                Thread.Sleep(5);
                runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Collapsed));
                Thread.Sleep(10000);
                if(runningBlock.Visibility==Visibility.Collapsed)
                {
                    runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Visible));
                }
                Thread.Sleep(10000);
                runningBlock.Dispatcher.Invoke(new Action(() => runningBlock.Visibility = Visibility.Collapsed));
                if(DanmuBlock.Contains(runningBlock))
                {
                    DanmuBlock.Remove(runningBlock);
                    PlayGrid.Dispatcher.Invoke(new Action(() => PlayGrid.Children.Remove(runningBlock)));
                    
                }
                
            });
        }
    }
}
