using HandyControl.Controls;
using LibVLCSharp.Shared;
using System;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.Tool.TranscodModule;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.Log;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// ClipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewClipWindow : GlowWindow
    {
        
        LibVLC vlcVideo;
        private static MediaPlayer _mediaPlayer;
        private static string _FilePath = @"";
        private static string OriginalFilePath = @"";
        private static bool Pause = true;
        private static DateTime DateTime = DateTime.Now;
        private static bool IsMouseDown = false;
        private static long FlieLen = 0;
        private static bool IsDrag = true;
        private static DateTime FlieSizeLen = DateTime.Now;

        private static DateTime Start = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, 0, 0, 0);
        private static DateTime End = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, (int)FlieLen / 3600000, (int)FlieLen % 3600000 / 60000, (int)FlieLen % 3600000 % 60000 / 1000);
        private bool IsClose = false;
        private int Conut = 0;

        /// <summary>
        /// 当前播放进度时长
        /// </summary>
        private static uint CurrentPlaybackTime = 0;
        /// <summary>
        /// 进度条总时长
        /// </summary>
        private static uint TotalTime = 0;


        public NewClipWindow(string FP)
        {
            FP = FP.Replace("\\","/");
            OriginalFilePath = FP;
            InitializeComponent();
            Core.Initialize("./plugins/vlc");
            vlcVideo = new LibVLC();
            _mediaPlayer = new MediaPlayer(vlcVideo);
            VideoView.MediaPlayer = _mediaPlayer;
            VideoView.Loaded += (sender, e) => VideoView.MediaPlayer = _mediaPlayer;
            TimeSlider.ValueChanged += TimeSlider_ValueChanged;//拖动播放条事件
            ClipSlider.ValueChanged += ClipSlider_ValueChanged;//拖动范围条事件
            VideoView.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;//播放器时间变化事件
            VideoView.MediaPlayer.EndReached += MediaPlayer_EndReached;//播放完成事件
            
            Start = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime((long)0 * 1000);
            End = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime((long)1 * 1000);
            CheckSstartTime.Dispatcher.Invoke(() => CheckSstartTime.Text = $"{Start.Hour}小时{Start.Minute}分{Start.Second}秒");
            CheckEndTime.Dispatcher.Invoke(() => CheckEndTime.Text = $"{End.Hour}小时{End.Minute}分{End.Second}秒");
            SetLoading(1);
            _FilePath = PrepareRawData(FP);
            SetLoading(0);
            SetFile();//加载文件
        }

        private string PrepareRawData(string FilePath)
        {
            string File =DDTV_Core.SystemAssembly.DownloadModule.Download.TmpPath+"";
            var tm = Transcod.CallFFMPEG(new TranscodClass()
            {
                AfterFilenameExtension = ".mp4",
                BeforeFilePath = FilePath,
                AfterFilePath = DDTV_Core.SystemAssembly.DownloadModule.Download.TmpPath+FilePath.Split('/')[FilePath.Split('/').Length-1].Replace(".mp4", $"_Clip_{new Random().Next(1000, 9999)}.mp4").Replace(".flv", $"_Clip_{new Random().Next(1000, 9999)}.mp4"),
            }, false);
            return tm.AfterFilePath;
        }

        /// <summary>
        /// 设置Loading界面提示内容
        /// </summary>
        /// <param name="type">0：隐藏   1：准备数据   2：切片中</param>
        private void SetLoading(int type)
        {
            switch (type)
            {
                case 0:
                    Loading.Dispatcher.Invoke(() =>
                    {
                        Loading.Visibility = System.Windows.Visibility.Collapsed;
                        LoadingData.Visibility = System.Windows.Visibility.Collapsed;
                        LoadingClip.Visibility = System.Windows.Visibility.Collapsed;
                    });
                    break;
                case 1:
                    Loading.Dispatcher.Invoke(() =>
                    {
                        Loading.Visibility = System.Windows.Visibility.Visible;
                        LoadingData.Visibility = System.Windows.Visibility.Visible;
                        LoadingClip.Visibility = System.Windows.Visibility.Collapsed;
                    });
                    break;
                case 2:
                    Loading.Dispatcher.Invoke(() =>
                    {
                        Loading.Visibility = System.Windows.Visibility.Visible;
                        LoadingData.Visibility = System.Windows.Visibility.Collapsed;
                        LoadingClip.Visibility = System.Windows.Visibility.Visible;
                    });
                    break;
            }
        }

        /// <summary>
        /// 拖动范围条触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClipSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<HandyControl.Data.DoubleRange> e)
        {
            Start = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime((long)e.NewValue.Start * 1000);
            End = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime((long)e.NewValue.End * 1000);
            CheckSstartTime.Dispatcher.Invoke(() => CheckSstartTime.Text = $"{Start.Hour}小时{Start.Minute}分{Start.Second}秒");
            CheckEndTime.Dispatcher.Invoke(() => CheckEndTime.Text = $"{End.Hour}小时{End.Minute}分{End.Second}秒");
            ClipTimeStamp.Text = $"剪辑标记范围:{ Start.ToString("HH:mm:ss")}  -  { End.ToString("HH:mm:ss")}";
        }

        private long TimeSlider_ValueChanged_Flag = 0;
        private void TimeSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            IsDrag = true;
            CurrentPlaybackTime = (uint)e.NewValue;
            if ((e.NewValue - e.OldValue > 1 || e.NewValue - e.OldValue < -1) && VideoView.MediaPlayer != null)
            {
                VideoView.Dispatcher.Invoke(() =>
                {
                    VideoView.MediaPlayer.Time = (long)e.NewValue * 1000;
                      FlvTimeStamp.Dispatcher.Invoke(() => FlvTimeStamp.Text = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime(VideoView.MediaPlayer.Time).ToString("HH:mm:ss"));
                });
            }
        }

        private void UpdateFileInfo()
        {
            try
            {
                VideoView.Dispatcher.Invoke(() =>
                {
                    if (VideoView.MediaPlayer != null)
                    {
                        VideoView.Dispatcher.Invoke(() =>
                        {
                            FlieSizeLen = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime(VideoView.MediaPlayer.Length);
                        });
                        string ClipST = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime((long)0 * 1000).ToString("HH:mm:ss") + "-" + FlieSizeLen.ToString("HH:mm:ss");
                        ClipSlider.Dispatcher.Invoke(() =>
                        {
                            ClipSlider.Minimum = 0;
                            ClipSlider.Maximum = VideoView.MediaPlayer.Length / 1000;
                            ClipSlider.ValueStart = 0;
                            ClipSlider.ValueEnd = VideoView.MediaPlayer.Length / 1000;
                        });
                        ClipTimeStamp.Dispatcher.Invoke(() => ClipTimeStamp.Text = ClipST);
                        TimeSlider.Dispatcher.Invoke(() =>
                        {
                            TimeSlider.Minimum = 0;
                            TimeSlider.Maximum = VideoView.MediaPlayer.Length / 1000;
                        });
                        FlvTimeStamp.Dispatcher.Invoke(() => FlvTimeStamp.Text = "00:00:00");
                    }
                });
            }
            catch (Exception)
            {
            }
        }

        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            Growl.WarningGlobal("拖拽的时间时间轴超过文件结束值，正在重置播放器，请稍等");
            SetFile();
        }

        private void ClipWindow_ClipCompleted(object? sender, EventArgs e)
        {
            Loading.Dispatcher.Invoke(() => Loading.Visibility = System.Windows.Visibility.Collapsed);
            MessageBox.Show("切片完成，切片文件储存在对应的录制文件夹中");
        }

        /// <summary>
        /// 播放器时间变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            if (!IsMouseDown && !IsClose)
            {
                if (!IsDrag)
                {
                    TimeSlider.Dispatcher.Invoke(() =>
                    {
                        TimeSlider.Value = e.Time / 1000;
                    });
                }
                FlvTimeStamp.Dispatcher.Invoke(() => FlvTimeStamp.Text = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime(VideoView.MediaPlayer.Time).ToString("HH:mm:ss"));
            }
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
                        VideoView.MediaPlayer.Play(new Media(vlcVideo, _FilePath));
                        int t1 = 0;
                        do
                        {
                            t1++;
                            if(t1>3000)
                            {
                                break;
                            }
                            Thread.Sleep(10);
                        } while (!VideoView.MediaPlayer.IsPlaying);
                        UpdateFileInfo();
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
                //UpdateFileLen();           
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
            
            DateTime Time = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime((long)CurrentPlaybackTime*1000);
            if(Time>=End)
            {
                MessageBox.Show("起始时间不能晚于结束时间");
                return;
            }
            Start = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, Time.Hour, Time.Minute, Time.Second);
            ClipSlider.ValueStart = Start.Hour*3600+Start.Minute*60+Start.Second;

            //CheckSstartTime.Dispatcher.Invoke(() => CheckSstartTime.Text = $"{Start.Hour}小时{Start.Minute}分{Start.Second}秒");

            //FlvTimeBar.Dispatcher.Invoke(new Action(() => {
            //    FlvTimeBar.Hotspots.Clear();
            //    FlvTimeBar.Hotspots.Add(new HandyControl.Data.DateTimeRange(Start, End));
            //}));

        }

        private void Button_SetEndTime_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DateTime Time = DDTV_Core.Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime((long)CurrentPlaybackTime * 1000);
            if (Time <= Start)
            {
                MessageBox.Show("结束时间不能早于起始时间");
                return;
            }
            End = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, Time.Hour, Time.Minute, Time.Second);
            ClipSlider.ValueEnd = End.Hour * 3600 + End.Minute * 60 + End.Second;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Pause = true;
            VideoView.MediaPlayer.SetPause(Pause);
            SetLoading(2);
            Task.Run(() =>
            {
                int S1 = (Start.Hour * 3600 + Start.Minute * 60 + Start.Second) * 1000;
                int E1 = (End.Hour * 3600 + End.Minute * 60 + End.Second) * 1000;
                if(S1-5000>0)
                {
                    S1 = S1 - 5000;
                }
                else
                {
                    S1 = 0;
                }
                if(S1+30>E1)
                {
                    MessageBox.Show("因为I帧差的原因，剪切长度至少要30秒，请重新选择");
                    SetLoading(0);
                    return;
                }
                Conut++;
                var tm = Transcod.CallFFMPEG(new TranscodClass()
                {
                    AfterFilenameExtension = ".mp4",
                    BeforeFilePath = _FilePath,
                    AfterFilePath = (OriginalFilePath
                    .Replace(OriginalFilePath.Split('/')[OriginalFilePath.Split('/').Length - 1], "")+OriginalFilePath.Split('/')[OriginalFilePath.Split('/').Length - 1])
                    .Replace(".mp4", $"_激光切片_{new Random().Next(1000, 9999)}.mp4")
                    .Replace(".flv", $"_激光切片_{new Random().Next(1000, 9999)}.mp4"),
                }, false, false, "-y -i {Before} -c copy -ss " + Start.ToString("HH:mm:ss") + " -to " + End.ToString("HH:mm:ss") + " {After} ");
                 SetLoading(0);
                Growl.SuccessGlobal($"切片完成:{tm.AfterFilePath}");
            });
        }

        private void GlowWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
            DDTV_Core.Tool.FileOperation.Del(_FilePath);
        }
    }
}
