using HandyControl.Controls;
using LibVLCSharp.Shared;
using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

using System.Windows.Input;


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
        string title = string.Empty;
        private static MediaPlayer _mediaPlayer;
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
        }

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
            string Ulr = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.playUrl(Uid, DDTV_Core.SystemAssembly.BilibiliModule.Rooms.RoomInfoClass.PlayQuality.HighDefinition);
            title = DDTV_Core.SystemAssembly.BilibiliModule.Rooms.Rooms.GetValue(Uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.title);
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
        /// 播放窗口隐藏切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            PlayControl.Visibility = (bool)toggleButton.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            this.Focus();
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
            VideoView.Dispatcher.Invoke(() => volume.Value = i);
            VideoView.Dispatcher.Invoke(() => VideoView.MediaPlayer.Volume = (int)volume.Value);
            //CoreConfig.GetValue(CoreConfigClass.Key.DefaultVolume, "50", CoreConfigClass.Group.Play)
            DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SetValue(DDTV_Core.SystemAssembly.ConfigModule.CoreConfigClass.Key.DefaultVolume, i.ToString("f0"), DDTV_Core.SystemAssembly.ConfigModule.CoreConfigClass.Group.Play);
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

                if (this.WindowState == WindowState.Normal)
                {
                    this.WindowState = WindowState.Maximized;
                    this.WindowStyle = WindowStyle.None;
                }
                else if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                }
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
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
            else if (e.Delta < 0)
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
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Dispose();
            }
            if (vlcVideo != null)
            {
                vlcVideo.Dispose();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.MessageBox.Show("TEST");
        }
    }
}
