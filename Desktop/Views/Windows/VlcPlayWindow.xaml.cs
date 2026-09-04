using Core;
using Core.LiveChat;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Services;
using Desktop.Views.Windows.DanMuCanvas.BarrageParameters;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using static Core.Config;
using Key = System.Windows.Input.Key;
using MenuItem = Wpf.Ui.Controls.MenuItem;


namespace Desktop.Views.Windows
{
    /// <summary>
    /// VlcPlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VlcPlayWindow : FluentWindow
    {

        //LibVLC实例全进程共享一个：每个播放窗口各建一个实例意味着每个窗口都背一份完整的native libvlc
        //（解码器缓存、线程池，几十MB），而LibVLCSharp官方支持单实例挂多个MediaPlayer，这也是推荐用法。
        //共享实例随进程生命周期存在，窗口关闭时只释放自己的MediaPlayer/Media即可。
        private static LibVLC _sharedLibVLC;
        private static readonly object _sharedLibVLCLock = new();
        private static LibVLC SharedLibVLC
        {
            get
            {
                lock (_sharedLibVLCLock)
                {
                    //每个选项必须是独立的argv元素，拼成一个字符串libvlc只会按一个非法参数解析，导致缓存设置全部失效；
                    //但libvlc遇到不认识的选项会直接实例化失败，只能传当前VLC版本真实存在的选项（已核实--hls-segment-threads和--no-cert-verification不存在，不可传入）
                    return _sharedLibVLC ??= new LibVLC("--network-caching=3000", "--live-caching=3000");
                }
            }
        }
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        /// <summary>
        /// 窗口展示内容数据绑定源
        /// </summary>
        internal VlcPlayModels vlcPlayModels { get; private set; }
        /// <summary>
        /// 当前窗口弹幕开关状态
        /// </summary>
        private bool DanmaSwitch = false;
        /// <summary>
        /// 当前窗口的置顶状态
        /// </summary>
        private bool TopMostSwitch = false;
        /// <summary>
        /// 当前播放窗口所属的房间卡
        /// </summary>
        private RoomCardClass roomCard = new();
        /// <summary>
        /// 弹幕渲染实例
        /// </summary>
        private BarrageConfig barrageConfig;
        /// <summary>
        /// 弹幕发射轨道
        /// </summary>
        public DanMuOrbitInfo[] danMuOrbitInfos = new DanMuOrbitInfo[100];
        /// <summary>
        /// 当前窗口的清晰度
        /// </summary>
        public long CurrentWindowClarity = 10000;
        /// <summary>
        /// 宽高比是否初始化
        /// </summary>
        public bool InitializeAspectRatio = false;
        /// <summary>
        /// 用于跟踪当前是否为全屏状态
        /// </summary>
        public bool isFullScreen = false;

        // 播放/重连重入保护（0=空闲，1=进行中），防止EndReached、F5、切换清晰度并发触发PlaySteam
        private int _playReentryGuard = 0;
        // 连续播放失败计数：Play()对网络错误是异步上报的（EncounteredError），单次PlaySteam内的重试上限拦不住
        // “报错→重连→再报错”的跨调用无限循环，必须用跨调用的连续失败计数兜底；Playing事件或手动刷新(F5/切清晰度)时清零
        private int _consecutivePlayFailures = 0;
        /// <summary>
        /// 连续播放失败达到此次数后停止自动重连，等待用户手动刷新
        /// </summary>
        private const int MaxConsecutivePlayFailures = 3;
        public class DanMuOrbitInfo
        {
            public string Text { get; set; }
            public int Time { get; set; } = 0;
        }

        /// <summary>
        /// 打开窗口时由调用方预取的流地址（CardControl检测HLS可用性时已取到一次），
        /// 首次播放直接复用，避免开窗后重复发起一次取流请求拉长首屏时间；仅首次播放使用，重连仍重新取流
        /// </summary>
        private readonly string _presetStreamUrl;

        public VlcPlayWindow(long uid, string presetStreamUrl = null)
        {
            _presetStreamUrl = presetStreamUrl;
            InitializeComponent();
            vlcPlayModels = new();
            CurrentWindowClarity = Core_RunConfig._DefaultPlayResolution;
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

            this.Title = $"{roomCard.Name}({roomCard.RoomId}) - {roomCard.Title.Value}";
            Log.Info(nameof(VlcPlayWindow), $"房间号:[{roomCard.RoomId}],打开播放器");

            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(SharedLibVLC);

            videoView.MediaPlayer = _mediaPlayer;
            videoView.MediaPlayer.Playing += MediaPlayer_Playing;
            videoView.MediaPlayer.EndReached += MediaPlayer_EndReached;
            videoView.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            videoView.MediaPlayer.Volume = 30;

            Task.Run(() => InitVlcPlay(uid));
            Task.Run(() => SetClarityMenu());
            PlayWindowManager.Register(this);
            //观看时长心跳：配置开关打开期间，窗口存续即向B站上报观看行为(开关判断收口在管理器内)
            Core.RuntimeObject.WatchHeartbeatManager.Register(roomCard.RoomId, roomCard.UID, "VlcPlayWindow");
        }
        /// <summary>
        /// 初始化播放器和弹幕渲染Canvas
        /// </summary>
        /// <param name="uid"></param>
        public void InitVlcPlay(long uid)
        {
            //首次播放优先使用调用方预取的流地址（为空时PlaySteam内部会自行取流）
            PlaySteam(_presetStreamUrl);
            Dispatcher.Invoke(() =>
            {
                barrageConfig = new BarrageConfig(DanmaCanvas, this.Width);
            });
            if (Core_RunConfig._PlayWindowDanmaSwitch)
            {
                SetDanma();
            }
            Dispatcher.Invoke(() =>
            {
                DanmaCanvas.Opacity = Core_RunConfig._PlayWindowDanMuFontOpacity;
            });
        }

        /// <summary>
        /// 获取和设置分辨率选项
        /// </summary>
        private void SetClarityMenu()
        {
            List<long> DefinitionList = Core.RuntimeObject.Download.Basics.GetOptionalClarity(roomCard.RoomId, "http_hls", "fmp4", "avc");

            Dictionary<long, string> clarityMap = new Dictionary<long, string>
            {
                {30000, "杜比"},
                {20000, "4K"},
                {10000, "原画"},
                {400, "蓝光"},
                {250, "超清"},
                {150, "高清"},
                {80, "流畅"}
            };

            foreach (var clarity in clarityMap)
            {
                if (DefinitionList.Contains(clarity.Key))
                {
                    Dispatcher.Invoke(() =>
                    {
                        MenuItem childMenuItem = new MenuItem
                        {
                            Header = clarity.Value,
                            Tag = clarity.Key
                        };
                        childMenuItem.Click += ModifyResolutionRightClickMenuEvent_Click;
                        SwitchPlaybackClarity_Menu.Items.Add(childMenuItem);
                    });
                }
            }
        }


        private void ModifyResolutionRightClickMenuEvent_Click(object sender, RoutedEventArgs e)
        {
            MenuItem clickedMenuItem = sender as MenuItem;

            Dispatcher.Invoke(() =>
            {
                CurrentWindowClarity = (long)clickedMenuItem.Tag; // 获取被点击的菜单项的索引
            });

            //手动切换清晰度视为用户主动重试，清零连续失败计数
            Interlocked.Exchange(ref _consecutivePlayFailures, 0);
            vlcPlayModels.LoadingVisibility = Visibility.Visible;
            vlcPlayModels.OnPropertyChanged("LoadingVisibility");

            PlaySteam(null);
        }

        private void MediaPlayer_Playing(object? sender, EventArgs e)
        {
            //播放成功，连续失败计数清零
            Interlocked.Exchange(ref _consecutivePlayFailures, 0);
            Task.Run(() =>
            {
                Thread.Sleep(3000);
                vlcPlayModels.LoadingVisibility = Visibility.Collapsed;
                vlcPlayModels.OnPropertyChanged("LoadingVisibility");
                //初始化宽高比
                if (!InitializeAspectRatio)
                {
                    if (_mediaPlayer != null && _mediaPlayer.Media != null && _mediaPlayer.Media.Tracks.Length > 0)
                    {
                        try
                        {
                            var videoWidth = _mediaPlayer.Media.Tracks[0].Data.Video.Width;
                            var videoHeight = _mediaPlayer.Media.Tracks[0].Data.Video.Height;
                            if (videoHeight > videoWidth)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    this.Width = 450;
                                    this.Height = 800;
                                });

                            }
                        }
                        catch (Exception) { }
                        InitializeAspectRatio = true;
                    }


                }
            });
        }

        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            //连续失败已达上限时不再自动重连，等待用户手动刷新（F5/右键刷新会清零计数）
            if (Volatile.Read(ref _consecutivePlayFailures) > MaxConsecutivePlayFailures)
            {
                return;
            }
            vlcPlayModels.LoadingVisibility = Visibility.Visible;
            vlcPlayModels.OnPropertyChanged("LoadingVisibility");
            PlaySteam(null);
        }

        /// <summary>
        /// VLC自身报告连接/播放错误时触发重连；连续失败超过上限后停止自动重连，避免流地址持续失效时无限循环
        /// </summary>
        private void MediaPlayer_EncounteredError(object? sender, EventArgs e)
        {
            int failures = Interlocked.Increment(ref _consecutivePlayFailures);
            Log.Warn(nameof(VlcPlayWindow), $"房间号:[{roomCard.RoomId}]，VLC报告播放错误(连续第{failures}次)");
            if (failures > MaxConsecutivePlayFailures)
            {
                Log.Warn(nameof(VlcPlayWindow), $"房间号:[{roomCard.RoomId}]，连续播放失败已达上限({MaxConsecutivePlayFailures}次)，停止自动重连，等待手动刷新");
                vlcPlayModels.LoadingVisibility = Visibility.Collapsed;
                vlcPlayModels.OnPropertyChanged("LoadingVisibility");
                vlcPlayModels.MessageVisibility = Visibility.Visible;
                vlcPlayModels.OnPropertyChanged("MessageVisibility");
                vlcPlayModels.MessageText = "连接直播间失败，请右键刷新或按F5重试";
                vlcPlayModels.OnPropertyChanged("MessageText");
                return;
            }
            vlcPlayModels.LoadingVisibility = Visibility.Visible;
            vlcPlayModels.OnPropertyChanged("LoadingVisibility");
            PlaySteam(null);
        }

        private async void SetDanma()
        {
            if (DanmaSwitch)
            {
                return;
            }
            DanmaSwitch = true;
            await Task.Run(() =>
            {

                if (roomCard.DownInfo.LiveChatListener == null)
                {
                    roomCard.DownInfo.LiveChatListener = new Core.LiveChat.LiveChatListener(roomCard.RoomId);
                    roomCard.DownInfo.LiveChatListener.Connect();
                }
                if (!roomCard.DownInfo.LiveChatListener.State)
                {
                    roomCard.DownInfo.LiveChatListener.Connect();
                }
                roomCard.DownInfo.LiveChatListener.MessageReceived -= LiveChatListener_MessageReceived;
                roomCard.DownInfo.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                Core.RuntimeObject.Danmu.DanmaTriggerReconnect -= Instance_DanmaTriggerReconnect;
                Core.RuntimeObject.Danmu.DanmaTriggerReconnect += Instance_DanmaTriggerReconnect;
                roomCard.DownInfo.LiveChatListener.Register.Add("VlcPlayWindow");
            });
        }

        private void Instance_DanmaTriggerReconnect(object? sender, RoomCardClass e)
        {
            if (e.DownInfo.LiveChatListener != null)
            {
                e.DownInfo.LiveChatListener.MessageReceived -= LiveChatListener_MessageReceived;
                e.DownInfo.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
            }
        }


        private async void CloseDanma()
        {
            DanmaSwitch = false;
            await Task.Run(() =>
            {
                if (roomCard.DownInfo.LiveChatListener != null && roomCard.DownInfo.LiveChatListener.Register.Count > 0)
                {

                    roomCard.DownInfo.LiveChatListener.MessageReceived -= LiveChatListener_MessageReceived;
                    Core.RuntimeObject.Danmu.DanmaTriggerReconnect -= Instance_DanmaTriggerReconnect;

                    roomCard.DownInfo.LiveChatListener.Register.Remove("VlcPlayWindow");
                    if (roomCard.DownInfo.LiveChatListener.Register.Count == 0)
                    {

                        roomCard.DownInfo.LiveChatListener.Dispose();
                        roomCard.DownInfo.LiveChatListener = null;
                    }

                }
            });
        }

        private void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {
                        //屏蔽词走缓存（配置不变时不重新Split），避免每条弹幕都分配临时数组
                        if (Services.BarrageBlockWords.IsBlocked(Danmu.Message))
                        {
                            return;
                        }
                        AddDanmu(Danmu.Message, false, Danmu.UserId);
                        break;
                    }
                case MessageEventArgs messageEventArgs:
                    {
                        if (messageEventArgs.Command == "Reconnect")
                        {
                            RoomCardClass roomCardClass = new();
                            _Room.GetCardForRoomId(liveChatListener.RoomId, ref roomCardClass);
                            Core.RuntimeObject.Danmu.ReconnectRoomDanmaObjects(roomCardClass);
                        }
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
            //重入保护：EndReached、F5、切换清晰度可能同时触发，并发执行会竞争_mediaPlayer.Media导致native崩溃
            if (Interlocked.Exchange(ref _playReentryGuard, 1) == 1)
            {
                Log.Info(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}],已有播放/重连流程进行中，忽略本次触发");
                return;
            }
            try
            {
                Log.Info(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}],播放网络路径直播流");
                await Task.Run(async () =>
                {
                    if (_mediaPlayer == null)
                    {
                        //窗口已关闭
                        return;
                    }
                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Stop();
                    }
                    if (_mediaPlayer.Media != null)
                    {
                        //Media包装native的libvlc_media_t，必须显式Dispose，只置null会累积native句柄
                        var oldMedia = _mediaPlayer.Media;
                        _mediaPlayer.Media = null;
                        oldMedia.ClearSlaves();
                        oldMedia.Dispose();
                    }

                    if (!RoomInfo.GetLiveStatus(roomCard.RoomId))
                    {
                        Log.Info(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}]，主播已下播，停止获取流地址");
                        return;
                    }
                    if (string.IsNullOrEmpty(Url))
                    {
                        Url = GeUrl(CurrentWindowClarity);
                    }
                    try
                    {
                        //有限次数重试：原实现用从不检查token的CancellationToken做"假取消"且无重试上限，
                        //流地址持续失效时会无限并发重连并堆积native资源，这里改为最多3次、每次间隔2秒
                        const int maxAttempts = 3;
                        for (int attempt = 1; attempt <= maxAttempts; attempt++)
                        {
                            if (_mediaPlayer == null)
                            {
                                //窗口已关闭（共享LibVLC实例随进程存活，不能用其实例是否存在判断窗口状态）
                                return;
                            }
                            if (string.IsNullOrEmpty(Url))
                            {
                                vlcPlayModels.MessageVisibility = Visibility.Visible;
                                vlcPlayModels.OnPropertyChanged("MessageVisibility");
                                vlcPlayModels.MessageText = "直播间已下拨获取地址失败，如需更新请右键刷新";
                                vlcPlayModels.OnPropertyChanged("MessageText");
                                return;
                            }

                            //上一次尝试遗留的Media先释放再重建
                            if (_mediaPlayer.Media != null)
                            {
                                var staleMedia = _mediaPlayer.Media;
                                _mediaPlayer.Media = null;
                                staleMedia.ClearSlaves();
                                staleMedia.Dispose();
                            }
                            var media = new Media(SharedLibVLC, Url, FromType.FromLocation);
                            _mediaPlayer.Media = media;
                            if (_mediaPlayer.Play())
                            {
                                vlcPlayModels.MessageVisibility = Visibility.Collapsed;
                                vlcPlayModels.OnPropertyChanged("MessageVisibility");
                                return;
                            }

                            Log.Warn(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}]，VLC播放启动失败(第{attempt}/{maxAttempts}次)，源地址[{Url}]");
                            if (attempt < maxAttempts)
                            {
                                vlcPlayModels.MessageVisibility = Visibility.Visible;
                                vlcPlayModels.OnPropertyChanged("MessageVisibility");
                                vlcPlayModels.MessageText = $"连接直播间失败，正在重试({attempt}/{maxAttempts})";
                                vlcPlayModels.OnPropertyChanged("MessageText");
                                await Task.Delay(2000);
                            }
                        }
                        vlcPlayModels.MessageVisibility = Visibility.Visible;
                        vlcPlayModels.OnPropertyChanged("MessageVisibility");
                        vlcPlayModels.MessageText = "连接直播间失败，请右键刷新或按F5重试";
                        vlcPlayModels.OnPropertyChanged("MessageText");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}]，VLC连接源出现意外错误，源地址[{Url}]", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                //async void的异常会直接打到SynchronizationContext导致崩溃，必须在此消化（多为窗口关闭与播放流程竞争导致的对象已释放）
                Log.Error(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}]，播放流程出现异常（可能是窗口关闭与播放竞争）", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _playReentryGuard, 0);
            }

        }

        /// <summary>
        /// 获取直播流地址
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string GeUrl(long Definition)
        {
            string url = "";
            if (roomCard != null && (Core.RuntimeObject.Download.HLS.GetHlsAvcUrl(roomCard, Definition, out url)))
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
                //先退订播放事件，避免Disposed对象上残留回调
                _mediaPlayer.Playing -= MediaPlayer_Playing;
                _mediaPlayer.EndReached -= MediaPlayer_EndReached;
                _mediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                if (_mediaPlayer.Media != null)
                {
                    //Media包装native的libvlc_media_t，必须显式Dispose，只置null会泄漏native句柄
                    var oldMedia = _mediaPlayer.Media;
                    _mediaPlayer.Media = null;
                    oldMedia.ClearSlaves();
                    oldMedia.Dispose();
                }
                Log.Info(nameof(PlaySteam), $"房间号:[{roomCard.RoomId}],关闭播放器");
                //释放native资源，避免反复开关播放窗积累句柄和内存
                videoView.MediaPlayer = null;
                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }
            //共享LibVLC实例随进程生命周期存在，此处不Dispose（否则其他播放窗口会拿到已释放的实例）
            //无论弹幕开关状态如何都确保退订重连事件，防止静态事件残留引用导致窗口无法回收
            Core.RuntimeObject.Danmu.DanmaTriggerReconnect -= Instance_DanmaTriggerReconnect;
            if (DanmaSwitch)
            {
                CloseDanma();
            }
            PlayWindowManager.Unregister(this);
            //注销观看时长心跳引用(该房间引用清零后管理器自动停止心跳)
            Core.RuntimeObject.WatchHeartbeatManager.Unregister(roomCard.RoomId, "VlcPlayWindow");
        }

        private DateTime lastClickTime = DateTime.MinValue; // 上次点击的时间
        /// <summary>
        /// 是否正在拖动窗口。VLC视频画面和WPF弹幕层是两个独立窗口（LibVLCSharp空域限制），拖动时弹幕仍在覆盖窗口上
        /// 逐帧动画重绘会显著放大拖动卡顿，拖动期间隐藏弹幕层并丢弃新弹幕，拖动结束自动恢复
        /// （volatile：弹幕接收线程也会读取该标志提前丢弃弹幕）
        /// </summary>
        private volatile bool _isDragging = false;

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            DanmaCanvas.Visibility = Visibility.Collapsed;
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
                //DragMove在非左键按住等情况下会抛InvalidOperationException，吞掉即可
            }
            finally
            {
                _isDragging = false;
                DanmaCanvas.Visibility = Visibility.Visible;
            }
            DateTime now = DateTime.Now;
            // 检查是否为双击（两次点击间隔小于系统双击时间）
            if ((now - lastClickTime).TotalMilliseconds <= SystemInformation.DoubleClickTime)
            {
                ToggleFullScreen();
            }
            lastClickTime = now;
        }
        private void ToggleFullScreen()
        {
            if (!isFullScreen)
            {
                // 切换到全屏模式
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.ResizeMode = ResizeMode.NoResize;
                isFullScreen = true;
            }
            else
            {
                // 切换回窗口模式
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                this.ResizeMode = ResizeMode.CanResize;
                isFullScreen = false;
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int v = 0;
            if (_mediaPlayer != null)
            {
                videoView.Dispatcher.Invoke(() => v = _mediaPlayer.Volume);
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
            if (videoView != null && _mediaPlayer != null)
            {

                videoView.Dispatcher.Invoke(() =>
                {
                    _mediaPlayer.Volume = i;
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

        /// <summary>
        /// 关闭全部播放窗口（二次确认后执行）
        /// </summary>
        private async void CloseAllWindows_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "关闭确认",
                Content = $"确认要关闭全部{PlayWindowManager.WindowCount}个播放窗口吗？",
                PrimaryButtonText = "是",
                SecondaryButtonText = "否",
                IsCloseButtonEnabled = false,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var result = await messageBox.ShowDialogAsync();
            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                //各窗口的Closing事件会自行完成MediaPlayer/Media释放和PlayWindowManager注销
                PlayWindowManager.CloseAll();
            }
        }


        /// <summary>
        /// 待渲染的弹幕缓冲（弹幕接收线程写入，UI线程批量取走）
        /// </summary>
        private readonly List<(string Text, bool IsSubtitle)> _pendingDanmu = new();
        private readonly object _pendingDanmuLock = new();
        private bool _danmuFlushScheduled = false;
        /// <summary>
        /// 弹幕聚合刷新间隔（ms）：把洪峰期间的UI线程操作从"每条一次"降到约7次/秒
        /// </summary>
        private const int DanmuFlushIntervalMs = 150;
        /// <summary>
        /// 每批最多渲染的弹幕条数（即约80条/秒上限）。弹幕洪峰时宁可丢弃也不能把UI线程打满——
        /// UI线程是全进程共享的，被打满会导致所有窗口、拖动、菜单一起卡死
        /// </summary>
        private const int MaxDanmuPerFlush = 12;
        /// <summary>
        /// 缓冲积压上限：超过说明渲染已跟不上，直接丢弃新弹幕防止延迟和内存膨胀
        /// </summary>
        private const int MaxPendingDanmu = 200;

        private void AddDanmu(string DanmuText, bool IsSubtitle, long uid = 0)
        {
            //弹幕渲染器尚未初始化完成（窗口刚打开）或拖动窗口期间，直接丢弃该条弹幕，不进缓冲
            if (barrageConfig == null || _isDragging)
            {
                return;
            }
            lock (_pendingDanmuLock)
            {
                if (_pendingDanmu.Count >= MaxPendingDanmu)
                {
                    return;
                }
                _pendingDanmu.Add((DanmuText, IsSubtitle));
                if (_danmuFlushScheduled)
                {
                    return;
                }
                _danmuFlushScheduled = true;
            }
            //150ms后批量渲染一次，弹幕洪峰时UI更新从每秒数百次降到约7次
            _ = Task.Run(async () =>
            {
                await Task.Delay(DanmuFlushIntervalMs);
                try
                {
                    await Dispatcher.InvokeAsync(FlushPendingDanmu);
                }
                catch (Exception)
                {
                    //窗口已关闭等情况忽略
                }
            });
        }

        /// <summary>
        /// 将缓冲的弹幕批量渲染到弹幕层（仅在UI线程执行）。
        /// 轨道分配在此串行执行（多线程并发扫轨道的竞态由缓冲单线程化天然避免）
        /// </summary>
        private void FlushPendingDanmu()
        {
            List<(string Text, bool IsSubtitle)> batch;
            lock (_pendingDanmuLock)
            {
                batch = new List<(string Text, bool IsSubtitle)>(_pendingDanmu);
                _pendingDanmu.Clear();
                _danmuFlushScheduled = false;
            }
            try
            {
                if (barrageConfig == null)
                {
                    return;
                }
                int rendered = 0;
                foreach (var (text, isSubtitle) in batch)
                {
                    //单批渲染量上限：超出部分丢弃，保证洪峰时每批占用的UI线程时间有界
                    if (rendered >= MaxDanmuPerFlush)
                    {
                        return;
                    }
                    //拖动窗口期间丢弃弹幕：弹幕动画的重绘会放大VLC空域窗口的拖动卡顿
                    if (_isDragging)
                    {
                        return;
                    }
                    int Index = -1;
                    for (int i = 0; i < danMuOrbitInfos.Length; i++)
                    {
                        if (danMuOrbitInfos[i] == null)
                        {
                            danMuOrbitInfos[i] = new();
                        }
                        if (danMuOrbitInfos[i].Time < Init.GetRunTime())
                        {
                            Index = i;
                            break;
                        }
                    }
                    //弹幕洪峰轨道全满时丢弃本批剩余弹幕，避免覆盖在屏弹幕
                    if (Index < 0)
                    {
                        return;
                    }
                    danMuOrbitInfos[Index].Time = (int)(Init.GetRunTime() + 5);
                    //显示弹幕
                    barrageConfig.Barrage_Stroke(new DanMuCanvas.Models.MessageInformation() { content = text }, Index, isSubtitle);
                    rendered++;
                }
            }
            catch (Exception)
            {
                //批量渲染在Dispatcher上执行，异常必须在此处消化，避免Dispatcher未处理异常导致程序崩溃
            }
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
                    videoView.Dispatcher.Invoke(() => v = _mediaPlayer.Volume);
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
                //手动刷新视为用户主动重试，清零连续失败计数
                Interlocked.Exchange(ref _consecutivePlayFailures, 0);
                vlcPlayModels.LoadingVisibility = Visibility.Visible;
                vlcPlayModels.OnPropertyChanged("LoadingVisibility");
                PlaySteam(null);
            }
        }

        private void Send_Danma_Button_Click(object sender, RoutedEventArgs e)
        {
            string T = DanmaOnly_DanmaInput.Text;
            if (string.IsNullOrEmpty(T) && T.Length >40 /*Core.Config.Core_RunConfig._MaximumLengthDanmu*/)
            {
                return;
            }
            Danmu.SendDanmu(roomCard.RoomId.ToString(), T);
            DanmaOnly_DanmaInput.Clear();
        }

        private void DanmaOnly_DanmaInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox? textBox = sender as System.Windows.Controls.TextBox;
            int maxlen = Core.Config.Core_RunConfig._MaximumLengthDanmu;
            if (textBox != null && textBox.Text.Length > Core.Config.Core_RunConfig._MaximumLengthDanmu)
            {
                int selectionStart = textBox.SelectionStart;
                textBox.Text = textBox.Text.Substring(0, 20);
                textBox.SelectionStart = selectionStart > 20 ? 20 : selectionStart;
            }
        }

        private void F5_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //手动刷新视为用户主动重试，清零连续失败计数
            Interlocked.Exchange(ref _consecutivePlayFailures, 0);
            vlcPlayModels.LoadingVisibility = Visibility.Visible;
            vlcPlayModels.OnPropertyChanged("LoadingVisibility");
            PlaySteam(null);
        }

        private void MenuItem_Switch_Danma_Send_Click(object sender, RoutedEventArgs e)
        {
            if (DanmaBox.Visibility == Visibility.Collapsed)
            {
                DanmaBox.Visibility = Visibility.Visible;
            }
            else
            {
                DanmaBox.Visibility = Visibility.Collapsed;
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (barrageConfig != null)
                barrageConfig._width = this.Width;
        }

        private void MenuItem_Switch_Danma_Exhibition_Click(object sender, RoutedEventArgs e)
        {
            if (DanmaSwitch)
            {
                SetNotificatom("关闭弹幕显示", $"{roomCard.Name}({roomCard.RoomId})播放窗口的弹幕显示已关闭");
                CloseDanma();
            }
            else
            {
                SetNotificatom("打开弹幕显示", $"{roomCard.Name}({roomCard.RoomId})播放窗口的弹幕显示已打开");
                SetDanma();
            }
        }

        private void SetNotificatom(string Title, string Message = "'")
        {
            Dispatcher.Invoke(() =>
            {
                HudNotification.Notify("/// PLAYER", Message, Title, HudNotification.HudLevel.Success);
            });

        }

        private void MenuItem_TopMost_Click(object sender, RoutedEventArgs e)
        {
            if (TopMostSwitch)
            {
                this.Topmost = false;
                TopMostSwitch = false;
                SetNotificatom("撤销窗口置顶", $"{roomCard.Name}({roomCard.RoomId})窗口置顶已关闭");
            }
            else
            {
                this.Topmost = true;
                TopMostSwitch = true;
                SetNotificatom("打开窗口置顶", $"{roomCard.Name}({roomCard.RoomId})窗口置顶已打开");
            }
        }

        private void DanmaOnly_DanmaInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                string T = DanmaOnly_DanmaInput.Text;
                if (string.IsNullOrEmpty(T) && T.Length > 40)
                {
                    return;
                }
                Danmu.SendDanmu(roomCard.RoomId.ToString(), T);
                DanmaOnly_DanmaInput.Clear();
            }
        }

        private void MenuItem_OpenLiveUlr_Click(object sender, RoutedEventArgs e)
        {

            var psi = new ProcessStartInfo
            {
                FileName = "https://live.bilibili.com/" + roomCard.RoomId,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void MenuItem_DanmaOnly_Click(object sender, RoutedEventArgs e)
        {
            RoomCardClass roomCardClass = new();
            _Room.GetCardForUID(roomCard.UID, ref roomCardClass);
            Windows.DanmaOnlyWindow danmaOnlyWindow = new(roomCardClass);
            danmaOnlyWindow.Show();
        }

        private void MenuItem_Layout_3x3_Click(object sender, RoutedEventArgs e)
        {
            PlayWindowManager.Arrange(this, WindowLayoutMode.Grid3x3);
        }

        private void MenuItem_Layout_3x4_Click(object sender, RoutedEventArgs e)
        {
            PlayWindowManager.Arrange(this, WindowLayoutMode.Grid3x4);
        }

        private void MenuItem_Layout_MainSub_Click(object sender, RoutedEventArgs e)
        {
            PlayWindowManager.Arrange(this, WindowLayoutMode.MainAndSub);
        }

        private void PasteStreamAddress_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer?.Media == null || string.IsNullOrEmpty(_mediaPlayer.Media.Mrl))
            {
                Log.Info("VlcPlayWindow", "流地址复制失败：当前无有效流地址（Media未初始化或Mrl为空）");
                return;
            }

            string streamAddress = _mediaPlayer.Media.Mrl;

            try
            {
                System.Windows.Clipboard.SetText(streamAddress);
                Log.Info("VlcPlayWindow", $"流地址已复制到剪贴板：{streamAddress}");
            }
            catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException || ex is System.Threading.ThreadStateException)
            {
                // 特定处理剪贴板相关的异常（如COM异常、线程状态异常）
                Log.Warn("VlcPlayWindow", $"流地址复制到剪贴板失败（剪贴板访问错误）", ex, false);
            }
            catch (Exception ex)
            {
                Log.Warn("VlcPlayWindow", $"流地址复制到剪贴板失败（未知错误）", ex, false);
            }
        }
    }
}
