using HandyControl.Controls;
using LibVLCSharp.Shared;
using System;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.Tool.TranscodModule;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// ClipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClipWindow : GlowWindow
    {
        
        LibVLC vlcVideo;
        private static MediaPlayer _mediaPlayer;
        private static string FilePath = @"";
        private static bool Pause = true;
        private static DateTime DateTime = DateTime.Now;
        private static bool IsMouseDown = false;
        private static long FlieLen = 0;
        private static DateTime FlieSizeLen = DateTime.Now;

        private static DateTime Start = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, 0, 0, 0);
        private static DateTime End = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, (int)FlieLen / 3600000, (int)FlieLen % 3600000 / 60000, (int)FlieLen % 3600000 % 60000 / 1000);
        private bool IsClose = false;
        RoomInfoClass.RoomInfo roomInfo=new();




        public ClipWindow(string _FilePath, RoomInfoClass.RoomInfo _roomInfo)
        {
            FilePath = _FilePath;
            roomInfo = _roomInfo;
            roomInfo.IsCliping = true;
            InitializeComponent();
            Core.Initialize("./plugins/vlc");
            vlcVideo = new LibVLC();
            _mediaPlayer = new MediaPlayer(vlcVideo);
            VideoView.MediaPlayer = _mediaPlayer;
            VideoView.Loaded += (sender, e) => VideoView.MediaPlayer = _mediaPlayer;
            FlvTimeBar.TimeChanged += FlvTimeBar_TimeChanged;
            DDTV_Core.Tool.FlvModule.FileFix.ClipCompleted += ClipWindow_ClipCompleted;
            VideoView.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            VideoView.MediaPlayer.EndReached += MediaPlayer_EndReached;
            SetFile();
        }

        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            Growl.Warning("拖拽的时间时间轴超过文件结束值，正在重置播放器，请稍等");
            SetFile();
        }

        private void ClipWindow_ClipCompleted(object? sender, EventArgs e)
        {
            loading.Dispatcher.Invoke(() => loading.Visibility = System.Windows.Visibility.Collapsed);
            MessageBox.Show("切片完成，切片文件储存在对应的录制文件夹中");
        }

        /// <summary>
        /// 更新文件长度
        /// </summary>
        private void UpdateFileLen()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    //if (VideoView.MediaPlayer != null)
                    {
                        VideoView.Dispatcher.Invoke(() => FlieLen = VideoView.MediaPlayer.Length);
                        FlieSizeLen = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, (int)FlieLen / 3600000, (int)FlieLen % 3600000 / 60000, (int)FlieLen % 3600000 % 60000 / 1000);
                    }
                    Thread.Sleep(1000);
                }
            });

        }
        /// <summary>
        /// 拖动时间轴事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            if (!IsMouseDown&& !IsClose)
            {
                FlvTimeBar.Dispatcher.Invoke(() =>
                {

                    FlvTimeBar.SelectedTime = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, (int)e.Time / 3600000, (int)e.Time % 3600000 / 60000, (int)e.Time % 3600000 % 60000 / 1000);
                });
            }
            //Console.WriteLine(e.Time);
        }

        private void SetFile()
        {
            Task.Run(() =>
            {
                
                Thread.Sleep(1000);
                VideoView.Dispatcher.Invoke(() =>
                {
                    if (VideoView.MediaPlayer != null&&!IsClose)
                    {
                        VideoView.MediaPlayer.Play(new Media(vlcVideo, FilePath));
                    }
                });
                Thread.Sleep(10);
                VideoView.Dispatcher.Invoke(() =>
                {
                    if (VideoView.MediaPlayer != null && !IsClose)
                    {     
                        VideoView.MediaPlayer.Pause();
                        FlieLen = VideoView.MediaPlayer.Length;
                        VideoView.MediaPlayer.Time =0;
                    }
                });
                UpdateFileLen();
                FlvTimeBar.Dispatcher.Invoke(() => {
                    Start = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, 0,0,0);
                    End = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, (int)FlieLen / 3600000, (int)FlieLen % 3600000 / 60000, (int)FlieLen % 3600000 % 60000 / 1000);
                    FlieSizeLen = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, (int)FlieLen / 3600000, (int)FlieLen % 3600000 / 60000, (int)FlieLen % 3600000 % 60000 / 1000);
                    CheckSstartTime.Dispatcher.Invoke(() => CheckSstartTime.Text = $"0小时0分0秒");
                    CheckEndTime.Dispatcher.Invoke(() => CheckEndTime.Text = $"{FlieSizeLen.Hour}小时{FlieSizeLen.Minute}分{FlieSizeLen.Second}秒");
                    FlvTimeBar.Hotspots.Add(new HandyControl.Data.DateTimeRange(Start,End));
                });
                
            });
        }
        private void FlvTimeBar_TimeChanged(object? sender, HandyControl.Data.FunctionEventArgs<System.DateTime> e)
        {
            double Time = e.Info.TimeOfDay.TotalSeconds;
            if(VideoView.MediaPlayer!=null)
            {
                VideoView.MediaPlayer.Time = (long)Time * 1000;
            }
           
        }

        private void Play_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Pause =! Pause;
            if (VideoView.MediaPlayer != null)
            {
                VideoView.MediaPlayer.SetPause(Pause);
            }
        }

        private void FlvTimeBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsMouseDown=true;
        }

        private void FlvTimeBar_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsMouseDown = false; ;
        }

        private void Button_SetSstartTime_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DateTime Time = FlvTimeBar.SelectedTime;
            Start = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, Time.Hour, Time.Minute, Time.Second);
            CheckSstartTime.Dispatcher.Invoke(() => CheckSstartTime.Text = $"{Start.Hour}小时{Start.Minute}分{Start.Second}秒");
           
            //FlvTimeBar.Dispatcher.Invoke(new Action(() => {
            //    FlvTimeBar.Hotspots.Clear();
            //    FlvTimeBar.Hotspots.Add(new HandyControl.Data.DateTimeRange(Start, End));
            //}));

        }

        private void Button_SetEndTime_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DateTime Time = FlvTimeBar.SelectedTime;
            int S1= Start.Hour*3600+ Start.Minute*60+ Start.Second;
            int E1= Time.Hour * 3600 + Time.Minute * 60 + Time.Second;
            if(E1<=S1)
            {
                MessageBox.Show("结束时间不能小于等于开始时间");
                return;
            }
            End = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, Time.Hour, Time.Minute, Time.Second);
            CheckEndTime.Dispatcher.Invoke(() => CheckEndTime.Text = $"{End.Hour}小时{End.Minute}分{End.Second}秒");
           
            //FlvTimeBar.Dispatcher.Invoke(new Action(() => {
            //    FlvTimeBar.Hotspots.Clear();
            //    FlvTimeBar.Hotspots.Add(new HandyControl.Data.DateTimeRange(Start, End));
            //}));
            

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Pause = true;
            VideoView.MediaPlayer.SetPause(Pause);
            loading.Dispatcher.Invoke(() => loading.Visibility=System.Windows.Visibility.Visible);
            Task.Run(() =>
            {
                uint S1 = ((uint)Start.Hour * 3600 + (uint)Start.Minute * 60 + (uint)Start.Second) * 1000;
                uint E1 = ((uint)End.Hour * 3600 + (uint)End.Minute * 60 + (uint)End.Second) * 1000;
                if(S1-5000>0)
                {
                    S1 = S1 - 5000;
                }
                else
                {
                    S1 = 0;
                }
                DDTV_Core.Tool.FlvModule.FileFix.Cutting(S1, E1, FilePath);
            });

        }

        private void GlowWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClose = true;
            roomInfo.IsCliping = false;
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
    }
}
