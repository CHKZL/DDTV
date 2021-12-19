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
        string title = string.Empty;
        private static MediaPlayer _mediaPlayer;
        WPFControl.SendDanmuDialog sendDanmuDialog;
        private DispatcherTimer VolumeTimer = new DispatcherTimer();
        int VolumeGridTime = 3;
        public event EventHandler<EventArgs> SendDialogDispose;
        private bool IsOpenDanmu = false;
        
        public PlayWindow(long Uid)
        {
            uid = Uid;
            InitializeComponent();
            Core.Initialize("./plugins/vlc");
            vlcVideo = new LibVLC();
            _mediaPlayer = new MediaPlayer(vlcVideo);
            VideoView.MediaPlayer = _mediaPlayer;
            VideoView.Loaded += (sender, e) => VideoView.MediaPlayer = _mediaPlayer;
            VideoView.MediaPlayer.Playing += MediaPlayer_Playing;
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                Play(uid);
            });
            VolumeTimer.Interval = new TimeSpan(0, 0, 0, 1); //参数分别为：天，小时，分，秒。此方法有重载，可根据实际情况调用
            VolumeTimer.Tick += VolumeTimer_Tick;
            VolumeTimer.Start();
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
            if(VolumeGridTime<0)
            {
                volume.Dispatcher.Invoke(new Action(() => volume.Visibility=Visibility.Collapsed));
                volumeText.Dispatcher.Invoke(() => volumeText.Visibility=Visibility.Collapsed);
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
            string Ulr = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.playUrl(Uid, DDTV_Core.SystemAssembly.BilibiliModule.Rooms.RoomInfoClass.Quality.DefaultHighest);
            title = DDTV_Core.SystemAssembly.BilibiliModule.Rooms.Rooms.GetValue(Uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.title);
            roomId= int.Parse(DDTV_Core.SystemAssembly.BilibiliModule.Rooms.Rooms.GetValue(Uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.room_id));
            this.Dispatcher.Invoke(() =>
                this.Title = title
            );
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
                        VideoView.MediaPlayer.Play(new Media(vlcVideo, new Uri(Ulr)));
                        SetVolume(MainWindow.DefaultVolume);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            });
        }
        /// <summary>
        /// 固定长宽比
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double X = this.Width;
            double Y = this.Height;
            if (X / Y > 1.77)
            {
                X = (Y - 32) / 9 * 16;
                this.Width = X;
            }
            else
            {
                Y = X / 16 * 9;
                this.Height = Y + 32;
            }
        }
        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="i"></param>
        private void SetVolume(double i)
        {
            VideoView.Dispatcher.Invoke(() => VideoView.MediaPlayer.Volume = (int)i);
            volume.Dispatcher.Invoke(() => volume.Value = i);
            volumeText.Dispatcher.Invoke(() => volumeText.Text=i.ToString());
            //CoreConfig.GetValue(CoreConfigClass.Key.DefaultVolume, "50", CoreConfigClass.Group.Play)
            DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SetValue(DDTV_Core.SystemAssembly.ConfigModule.CoreConfigClass.Key.DefaultVolume, i.ToString("f0"), DDTV_Core.SystemAssembly.ConfigModule.CoreConfigClass.Group.Play);
            MainWindow.DefaultVolume=i;
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
            loading.Visibility = Visibility.Visible;
            Play(uid);
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
            Growl.Warning("该功能还未完成");
            return;
            IsOpenDanmu = !IsOpenDanmu;
            if (IsOpenDanmu)
            {
                var roomInfo = DDTV_Core.SystemAssembly.BilibiliModule.API.WebSocket.WebSocket.ConnectRoomAsync(roomId);
                roomInfo.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
            }
            else
            {
                if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    if (roomInfo.roomWebSocket.LiveChatListener != null)
                    {
                        try
                        {
                            roomInfo.roomWebSocket.LiveChatListener.startIn = false;
                            roomInfo.DanmuFile.TimeStopwatch.Stop();
                            roomInfo.roomWebSocket.LiveChatListener.Dispose();
                        }
                        catch (Exception)
                        {
                           
                        }
                    }
                }
            }
        }

        private void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        {
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    Console.WriteLine($"{Danmu.Message}");
                    break;
                case SuperchatEventArg SuperchatEvent:
                    Console.WriteLine($"{SuperchatEvent.UserName}({SuperchatEvent.UserId}):价值[{SuperchatEvent.Price}]的SC信息:【{SuperchatEvent.Message}】,翻译后:【{SuperchatEvent.messageTrans}】");
                    break;
                case GuardBuyEventArgs GuardBuyEvent:
                    Console.WriteLine($"{GuardBuyEvent.UserName}({GuardBuyEvent.UserId}):开通了{GuardBuyEvent.Number}个月的{GuardBuyEvent.GuardName}(单价{GuardBuyEvent.Price})");
                    break;
                case SendGiftEventArgs sendGiftEventArgs:
                    Console.WriteLine($"{sendGiftEventArgs.UserName}({sendGiftEventArgs.UserId}):价值{sendGiftEventArgs.GiftPrice}的{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}");
                    break;
                case EntryEffectEventArgs entryEffectEventArgs:
                    Console.WriteLine($"[舰长进入房间]舰长uid:{entryEffectEventArgs.uid},舰长头像{entryEffectEventArgs.face},欢迎信息:{entryEffectEventArgs.copy_writing}");
                    break;
                default:
                    break;
            }
        }

        private void MenuItem_WindowSorting_Click(object sender, RoutedEventArgs e)
        {
            Growl.Warning("该功能还未完成");
        }
    }
}
