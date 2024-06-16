using Core;
using Core.LiveChat;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Windows.DanMuCanvas.BarrageParameters;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SharpCompress.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using static Core.Config;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using Key = System.Windows.Input.Key;

namespace Desktop.Views.Windows
{
    /// <summary>
    /// VlcPlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VlcPlayWindow : FluentWindow
    {

        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        internal VlcPlayModels vlcPlayModels { get; private set; }

        /// <summary>
        /// 弹幕渲染实例
        /// </summary>
        private BarrageConfig barrageConfig;
        /// <summary>
        /// 弹幕发射轨道
        /// </summary>
        public DanMuOrbitInfo[] danMuOrbitInfos = new DanMuOrbitInfo[100];
        public class DanMuOrbitInfo
        {
            public string Text { get; set; }
            public int Time { get; set; } = 0;
        }

        private RoomCardClass roomCard = new();
        public VlcPlayWindow(long uid)
        {
            InitializeComponent();
            vlcPlayModels = new();
            this.DataContext = vlcPlayModels;
            _Room.GetCardForUID(uid, ref roomCard);

            vlcPlayModels.VolumeVisibility = Visibility.Collapsed;
            vlcPlayModels.OnPropertyChanged("VolumeVisibility");


            if (roomCard == null || roomCard.live_status.Value != 1)
            {
                Log.Info(nameof(VlcPlayWindow), $"打开播放器失败，入参uid:{uid},因为{(roomCard == null ? "roomCard为空" : "已下播")}");
                vlcPlayModels.MessageVisibility = Visibility.Visible;
                vlcPlayModels.OnPropertyChanged("MessageVisibility");
                vlcPlayModels.MessageText = "该直播间未开播，播放失败";
                vlcPlayModels.OnPropertyChanged("MessageText");
                return;
            }
            Log.Info(nameof(VlcPlayWindow), $"房间号:[{roomCard.RoomId}],打开播放器");

            _libVLC = new LibVLC([$"--network-caching={new Random().Next(3000, 4000)}"]);
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            videoView.MediaPlayer = _mediaPlayer;
            videoView.MediaPlayer.Playing += MediaPlayer_Playing;
            videoView.MediaPlayer.EndReached += MediaPlayer_EndReached;
            videoView.MediaPlayer.Stopped += MediaPlayer_EndReached;
            videoView.MediaPlayer.Volume = 30;
            PlaySteam();
            barrageConfig = new BarrageConfig(DanmaCanvas, ((double)this.Width / 800) * Core_RunConfig._PlayWindowDanmaSpeed);
            SetDanma();

            DanmaCanvas.Opacity = Core_RunConfig._PlayWindowDanMuFontOpacity;


        }

        private void MediaPlayer_Playing(object? sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Thread.Sleep(3000);
                vlcPlayModels.LoadingVisibility = Visibility.Collapsed;
                vlcPlayModels.OnPropertyChanged("LoadingVisibility");
            });
        }

        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            vlcPlayModels.LoadingVisibility = Visibility.Visible;
            vlcPlayModels.OnPropertyChanged("LoadingVisibility");
            PlaySteam();
        }

        private async void SetDanma()
        {
            await Task.Run(() =>
            {
                if (roomCard.DownInfo.LiveChatListener == null)
                {
                    roomCard.DownInfo.LiveChatListener = new Core.LiveChat.LiveChatListener(roomCard.RoomId);
                }
                if (!roomCard.DownInfo.LiveChatListener.State)
                {
                    roomCard.DownInfo.LiveChatListener.Connect();
                }
                roomCard.DownInfo.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                roomCard.DownInfo.LiveChatListener.Register.Add("VlcPlayWindow");
            });
        }

        private void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {
                        AddDanmu(Danmu.Message, false, Danmu.UserId);
                        break;
                    }
            }
        }

        /// <summary>
        /// 播放网络路径直播流
        /// </summary>
        /// <param name="Url"></param>
        public async void PlaySteam(string Url = null)
        {
            Log.Info(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}],播放网络路径直播流");
            await Task.Run(() =>
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                if (_mediaPlayer.Media != null)
                {
                    _mediaPlayer.Media.ClearSlaves();
                    _mediaPlayer.Media = null;
                }
                if (!RoomInfo.GetLiveStatus(roomCard.RoomId))
                {
                    Log.Info(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}]，主播已下播，停止获取流地址");
                    return;
                }
                if (string.IsNullOrEmpty(Url))
                {
                    Url = GeUrl();
                }
                try
                {
                    bool completedInTime = false;

                    while (!completedInTime)
                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        Task task = Task.Run(() =>
                        {
                            var media = new LibVLCSharp.Shared.Media(_libVLC, Url, LibVLCSharp.Shared.FromType.FromLocation);
                            _mediaPlayer.Media = media;
                            _mediaPlayer.Play();
                        }, cts.Token);

                        if (!task.Wait(TimeSpan.FromSeconds(10)))
                        {
                            cts.Cancel();
                            Log.Warn(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}]，VLC连接源超时，进行重试，源地址[{Url}]");
                            vlcPlayModels.MessageVisibility = Visibility.Visible;
                            vlcPlayModels.OnPropertyChanged("MessageVisibility");
                            vlcPlayModels.MessageText = "连接直播间失败，开始重试";
                            vlcPlayModels.OnPropertyChanged("MessageText");
                        }
                        else
                        {
                            completedInTime = true;
                            vlcPlayModels.MessageVisibility = Visibility.Collapsed;
                            vlcPlayModels.OnPropertyChanged("MessageVisibility");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}]，VLC连接源出现意外错误，进行重试，源地址[{Url}]", ex);
                }
            });

        }

        /// <summary>
        /// 获取直播流地址
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string GeUrl()
        {
            string url = "";
            if (roomCard != null && (Core.RuntimeObject.Download.HLS.GetHlsAvcUrl(roomCard, out url) || Core.RuntimeObject.Download.FLV.GetFlvAvcUrl(roomCard, out url)))
            {
                Log.Info(nameof(GeUrl), $"房间号:[{roomCard.RoomId}]，获取到直播流地址:[{url}]");
                return url;
            }
            return "";
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_mediaPlayer != null)
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                if (_mediaPlayer.Media != null)
                {
                    _mediaPlayer.Media.ClearSlaves();
                    _mediaPlayer.Media = null;
                }
                Log.Info(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}],关闭播放器");
            }
            if (roomCard.DownInfo.LiveChatListener.Register.Count > 0)
            {
                roomCard.DownInfo.LiveChatListener.Register.Remove("VlcPlayWindow");
                if (roomCard.DownInfo.LiveChatListener.Register.Count == 0)
                {
                    if (roomCard.DownInfo.LiveChatListener != null)
                    {
                        roomCard.DownInfo.LiveChatListener.DanmuMessage = null;
                        roomCard.DownInfo.LiveChatListener.Dispose();
                    }
                }
            }
        }


        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int v = 0;
            if (videoView != null && videoView.MediaPlayer != null)
            {
                videoView.Dispatcher.Invoke(() => v = videoView.MediaPlayer.Volume);
            }
            if (e.Delta > 0)
            {
                if (v + 5 <= 100)
                {
                    SetVolume(v + 5);
                }
                else
                {
                    SetVolume(100);
                }
            }
            else if (e.Delta < 0)
            {
                if (v - 5 >= 0)
                {
                    SetVolume(v - 5);
                }
                else
                {
                    SetVolume(0);
                }
            }
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="i"></param>
        private void SetVolume(int i)
        {
            if (videoView != null && videoView.MediaPlayer != null)
            {

                videoView.Dispatcher.Invoke(() =>
                {
                    videoView.MediaPlayer.Volume = i;
                    vlcPlayModels.VolumeVisibility = Visibility.Visible;
                    vlcPlayModels.OnPropertyChanged("VolumeVisibility");
                    Task.Run(() =>
                    {
                        Thread.Sleep(2000);
                        vlcPlayModels.VolumeVisibility = Visibility.Collapsed;
                        vlcPlayModels.OnPropertyChanged("VolumeVisibility");
                    });
                    vlcPlayModels.Volume = i.ToString();
                    vlcPlayModels.OnPropertyChanged("Volume");
                });
            }
        }

        private void ExitWindow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void AddDanmu(string DanmuText, bool IsSubtitle, long uid = 0)
        {

            Task.Run(() =>
            {
                int Index = 0;
                for (int i = 0; i < danMuOrbitInfos.Length; i++)
                {
                    if (danMuOrbitInfos[i] == null)
                    {
                        danMuOrbitInfos[i] = new();
                    }
                    if (danMuOrbitInfos[i].Time < Init.GetRunTime() / 1000)
                    {
                        Index = i;
                        break;
                    }
                }

                danMuOrbitInfos[Index].Time = ((int)(Init.GetRunTime() / 1000) + 5);
                //非UI线程调用UI组件
                System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    //显示弹幕
                    barrageConfig.Barrage_Stroke(new DanMuCanvas.Models.MessageInformation() { content = DanmuText }, Index, IsSubtitle);
                });

            });

        }

        private void FullScreenSwitch()
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void FullScreenSwitch_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            FullScreenSwitch();
        }



        private void FluentWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Up) || e.KeyStates == Keyboard.GetKeyStates(Key.Down))
            {
                int v = 0;
                if (videoView != null && videoView.MediaPlayer != null)
                {
                    videoView.Dispatcher.Invoke(() => v = videoView.MediaPlayer.Volume);
                }
                //音量增加
                if (e.KeyStates == Keyboard.GetKeyStates(Key.Up))
                {
                    if (v + 5 <= 100)
                    {
                        SetVolume(v + 5);
                    }
                    else
                    {
                        SetVolume(100);
                    }
                }
                //音量降低
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.Down))
                {
                    if (v - 5 >= 0)
                    {
                        SetVolume(v - 5);
                    }
                    else
                    {
                        SetVolume(0);
                    }
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
                vlcPlayModels.LoadingVisibility = Visibility.Visible;
                vlcPlayModels.OnPropertyChanged("LoadingVisibility");
                PlaySteam();
            }
        }
    }
}
