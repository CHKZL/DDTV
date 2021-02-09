using Auxiliary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Auxiliary.LiveChatScript;
using LibVLCSharp.WPF;
using LibVLCSharp.Shared;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace PlayW
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Downloader DD { set; get; }
        private bool 播放状态 = false;
        private bilibili.danmu DM = new bilibili.danmu();
        private bool 弹幕使能 = false;
        private bool 字幕使能 = false;
        public bool 窗口是否打开 = false;
        private int 刷新次数 = 0;
        private bool 是否已经连接弹幕 = false;
        LiveChatListener listener = new LiveChatListener();
        private static LibVLC _libVLC;
        private static MediaPlayer _mediaPlayer;
        private SetWindow.窗口信息 窗口信息 = new SetWindow.窗口信息();


        /// <summary>
        /// 建立新的播放窗口实例
        /// </summary>
        /// <param name="DL">播放对象实例</param>
        /// <param name="默认音量">新建实例的默认音量</param>
        /// <param name="弹幕颜色">新建实例的默认弹幕颜色</param>
        /// <param name="字幕颜色">新建实例的默认字幕颜色</param>
        /// <param name="弹幕大小">新建实例的默认弹幕字体大小</param>
        /// <param name="字幕大小">新建实例的默认字幕字体大小</param>
        /// <param name="宽度">新建实例的默认窗口宽度</param>
        /// <param name="高度">新建实例的默认窗口高度</param>
        public MainWindow(Downloader DL, int 默认音量, SolidColorBrush 弹幕颜色, SolidColorBrush 字幕颜色, int 弹幕大小, int 字幕大小, int 宽度, int 高度)
        {
            InitializeComponent();
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            VLCV.Loaded += (sender, e) => VLCV.MediaPlayer = _mediaPlayer;
            this.Width = 宽度;
            this.Height = 高度;

            this.Title = DL.DownIofo.标题;
            窗口信息 = new SetWindow.窗口信息()
            {
                GUID = Guid.NewGuid().ToString(),
                X坐标 = this.Left,
                Y坐标 = this.Top,
                宽度 = this.Width,
                高度 = this.Height,
                标题 = this.Title
            };
            
            
            窗口是否打开 = true;
            音量.Value = 默认音量;
            this.Closed += 关闭窗口事件;
            DD = DL;
            弹幕框.Opacity = 0.5;
            字幕.Foreground = 字幕颜色;
            弹幕.Foreground = 弹幕颜色;
            // 字幕.FontSize = 字幕大小;
            字幕.字体大小 = 字幕大小;
            字幕.是否居中 = true;
            //弹幕.FontSize = 弹幕大小;
            弹幕.字体大小 = 弹幕大小;
            DD.DownIofo.文件保存路径 =   "./tmp/LiveCache/" + DL.DownIofo.标题 + DL.DownIofo.事件GUID + "_" + 刷新次数 + ".flv";
            DD.DownIofo.继承 = new Downloader.继承();
           
            设置框.Visibility = Visibility.Collapsed;
            关闭框.Visibility = Visibility.Collapsed;
            if (MMPU.第一次打开播放窗口)
            {
                设置框.Visibility = Visibility.Visible;
                关闭框.Visibility = Visibility.Visible;
                MMPU.第一次打开播放窗口 = false;
            }
            if (MMPU.系统内核版本 != 10)
            {  
                弹幕使能 = false;
                弹幕开关.IsChecked = false;
                弹幕开关.Visibility = Visibility.Collapsed;
                字幕使能 = false;
                字幕开关.IsChecked = false;
                字幕开关.Visibility = Visibility.Collapsed;
                修改字幕颜色按钮.Visibility = Visibility.Collapsed;
                修改弹幕颜色按钮.Visibility = Visibility.Collapsed;
                弹幕输入窗口.Visibility = Visibility.Collapsed;
                弹幕发送提示.Visibility = Visibility.Collapsed;
                弹幕高CPU使用率警告.Visibility = Visibility.Collapsed;
                发送弹幕按钮.Visibility = Visibility.Collapsed;
                弹幕透明度.Visibility = Visibility.Collapsed;
                字幕位置.Visibility = Visibility.Collapsed;
                非win10提示.Visibility = Visibility.Visible;
            }
            else
            {
                非win10提示.Visibility = Visibility.Collapsed;
            }

        }
        private void 更新窗口信息()
        {
            try
            {
                窗口信息.X坐标 = this.Left;
                窗口信息.Y坐标 = this.Top;
                窗口信息.宽度 = this.Width;
                窗口信息.高度 = this.Height;
                窗口信息.标题 = this.Title;
            }
            catch (Exception){}
        }
        public SetWindow.窗口信息 获取窗口信息()
        {
            更新窗口信息();
            SetWindow.窗口信息 SW = new SetWindow.窗口信息()
            {
                X坐标 = this.Left,
                Y坐标 = this.Top,
                宽度 = this.Width,
                高度 = this.Height,
                标题=窗口信息.标题,
                GUID=窗口信息.GUID     
            };
            return SW;
        }
        public bool 设置窗口信息(SetWindow.窗口信息 SW)
        {
            try
            {
                if (SW.X坐标 >= 0)
                {
                    this.Left = SW.X坐标;
                }
                if (SW.Y坐标 >= 0)
                {
                    this.Top = SW.Y坐标;
                }
                if (SW.宽度 > 0)
                {
                    this.Width = SW.宽度;
                }
                if (SW.高度 > 0)
                {
                    this.Height = SW.高度;
                }
                if (!string.IsNullOrEmpty(SW.标题))
                {
                    this.Title = SW.标题;
                }

                //if (this.Height != LastHeight)
                //{
                //    this.Width = this.Height * 1.75;
                //}
                if (this.Width != LastWidth)
                {
                    this.Height = this.Width / 1.75;
                }
                LastWidth = (int)this.Width;
                LastHeight = (int)this.Height;
                更新窗口信息();
                return true;
            }
            catch (Exception)
            {
                更新窗口信息();
                return false;
            }
        }
        
        private void 关闭窗口事件(object sender, EventArgs e)
        {
            播放状态 = false;
            new Task((() => 
            {
                try
                {
                    Play_STOP();
                    //listener.Close();
                    listener.Dispose();

                    try
                    {
                        VLCV.MediaPlayer.Dispose();
                    }
                    catch (Exception) { }

                    try
                    {
                        //不能回收这玩意，回收了最后启动的一个实例就会挂掉，奇了个怪
                        //_mediaPlayer.Dispose();
                    }
                    catch (Exception) { }
                    try
                    {
                        _libVLC.Dispose();
                    }
                    catch (Exception) { }
                    try
                    {
                        VLCV.Dispose();
                    }
                    catch (Exception) { }
                    //this.VlcControl.SourceProvider.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放
                }
                catch (Exception)
                {
                }
            })).Start();
          
            DD.DownIofo.播放状态 = false;
            窗口是否打开 = false;
        }
        #region 播放控件事件
        public void Play_STOP()
        {
            new Task((() =>
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    try
                    {
                        if (VLCV.MediaPlayer.IsPlaying)
                        {
                            VLCV.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放
                        }
                    }
                    catch (Exception)
                    {
                    }
                }));
            })).Start();
        }
        public void Play_SETVolume(int a)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                if (窗口是否打开 && VLCV.MediaPlayer.IsPlaying)
                {
                    VLCV.MediaPlayer.Volume = a;
                }
                首页焦点.Focus();
            }));
        }
        public void Play_Play()
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                if (窗口是否打开 && VLCV.MediaPlayer.IsPlaying)
                {
                    VLCV.MediaPlayer.Play();
                }
            }));
        }
        public void Play_Open(string A)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                try
                {
                    if (窗口是否打开&&!VLCV.MediaPlayer.IsPlaying)
                    {
                        using (var media = new Media(_libVLC, A))
                            do
                            {

                            } while (!VLCV.MediaPlayer.Play(media));
                        do
                        {
                            Play_SETVolume((int)音量.Value);
                        } while (!VLCV.MediaPlayer.IsPlaying);
                        
                    }
                }
                catch (Exception e)
                {
                    ;
                }
            })); 
        }
        #endregion
        private void 增加字幕(string A)
        {
            int 显示的字幕数 = 3;
            int Len = 0;
            string B = "";
            string C = "";
            this.Dispatcher.Invoke(new Action(delegate
            {
                B = 字幕.内容;
                Len = 字幕.内容.Split('\n').Length;
            }));
            if (Len > 显示的字幕数)
            {
                for (int i = 0; i < 显示的字幕数; i++)
                {
                    if (i == 0)
                    {
                        C = B.Split('\n')[i];
                    }
                    else
                    {
                        C = C + "\n" + B.Split('\n')[i];
                    }

                }
            }
            else
            {
                C = B;
            }
            this.Dispatcher.Invoke(new Action(delegate
            {
                字幕.内容 = A + "\n" + C;
                if (字幕.FontSize > 50)
                {
                    字幕.FontSize = 1;
                }
                else
                {
                    字幕.FontSize++;
                }
            }));
        }
        private void 增加弹幕(string A)
        {
            int 显示的弹幕数 = 30;
            int Len = 0;
            string B = "";
            string C = "";
            this.Dispatcher.Invoke(new Action(delegate
            {
                B = 弹幕.内容;
                Len = 弹幕.内容.Split('\n').Length;
            }));
            if (Len > 显示的弹幕数)
            {
                for (int i = 0; i < 显示的弹幕数; i++)
                {
                    if (i == 0)
                    {
                        C = B.Split('\n')[i];
                    }
                    else
                    {
                        C = C + "\n" + B.Split('\n')[i];
                    }

                }
            }
            else
            {
                C = B;
            }
            this.Dispatcher.Invoke(new Action(delegate
            {
                弹幕.内容 = A + "\n" + C;
                if (弹幕.FontSize > 50)
                {
                    弹幕.FontSize = 1;
                }
                else
                {
                    弹幕.FontSize++;
                }
            }));


        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //string A = System.IO.Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "libvlc", /*IntPtr.Size == 4 ?*/ "win-x64" /*: "win-x64"*/);
            //var options = new string[]
            //{
            //    " --avcodec-hw=dxva2",
            //};
            //// this.VlcControl.SourceProvider.CreatePlayer(new DirectoryInfo(A), options);
            ///
           

            this.Dispatcher.Invoke(new Action(delegate
            {
                提示框.Visibility = Visibility.Visible;
                提示文字.Content = "该房间已开播,检测直播流状态,等待推流数据中....";
            }));
            new Task((() => 
            {
              
                //MMPU.DownList.Add(DD);
                if (DD.DownIofo.是否是播放任务)
                {
                    DD.Start("缓冲视频中");
                    DD.DownIofo.备注 = "缓冲视频中";
                }
                else
                {
                    DD.Start("直播观看缓冲进行中");
                    DD.DownIofo.备注 = "直播观看缓冲进行中";
                }

                Thread.Sleep(MMPU.播放缓冲时长 * 1000);
                this.Dispatcher.Invoke(new Action(delegate
                {
                    提示框.Visibility = Visibility.Collapsed;
                }));
                try
                {
                    if(!窗口是否打开)
                    {
                        return;
                    }
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        try
                        {
                            Play_SETVolume((int)音量.Value);
                        }
                        catch (Exception)
                        {
                        }


                        // Aplayer.Open(DD.DownIofo.文件保存路径);
                        // this.VlcControl.SourceProvider.MediaPlayer.Play(new Uri(DD.DownIofo.文件保存路径));
                        this.VLCV.MediaPlayer.EndReached += 播放到达结尾触发事件; ;
                        Play_Open(DD.DownIofo.文件保存路径);
                        //Aplayer.OnStateChanged += 播放到达结尾触发事件;  
                    }));
                    播放状态 = true;
                    if (DD.DownIofo.平台 == "bilibili")
                    {
                        //获取弹幕();
                    }
                }
                catch (Exception ex)
                {
                    ;
                }
            })).Start();

        }
        private void Listener_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e)
                {
                    case DanmuMessageEventArgs danmu:
                        if (字幕使能)
                        {
                            if (danmu.Message.Contains("【") || danmu.Message.Contains("】"))
                            {
                                增加字幕(danmu.Message);
                            }
                        }
                        if (弹幕使能)
                        {
                            if (!danmu.Message.Contains("【") || !danmu.Message.Contains("】"))
                            {
                                增加弹幕(danmu.Message);
                            }

                        }
                        // Console.WriteLine("WebSocket收到弹幕数据:{0}:{1}", danmu.UserName, danmu.Message);
                        //Debug.Log("M:" + danmu.Message);
                        //mtext.text = "M:" + danmu.Message;
                        break;
                    case SendGiftEventArgs gift:
                        //Debug.Log("G:"+gift.GiftName);
                        //gtext.text = "G:" + gift.GiftName;
                        break;
                    case GuardBuyEventArgs guard:
                        // Debug.LogError("JZ:" + guard.GiftName);
                        break;
                    case WelcomeEventArgs Welcome:

                        break;
                    case ActivityBannerEventArgs activityBannerEventArgs:

                        break;

                    default:
                        // Debug.LogError("???" + e.JsonObject);
                        // Debug.LogError(LitJson.JsonMapper.ToJson(e.JsonObject));

                        break;
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void 获取弹幕()
        {
            new Task((() => 
            {
                while (true)
                {
                    try
                    {
                        if (DD.DownIofo.下载状态)
                        {
                            if (弹幕使能 || 字幕使能)
                            {
                                Newtonsoft.Json.Linq.JArray JA = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(DM.getDanmaku(DD.DownIofo.房间_频道号));
                                for (int i = 0; i < JA.Count; i++)
                                {
                                    //  Console.WriteLine(JA[i]["Time"].ToString() + ":" + JA[i]["Name"].ToString() + "(" + JA[i]["uid"].ToString() + "):" + JA[i]["Text"].ToString());
                                    if (字幕使能)
                                    {
                                        if (JA[i]["Text"].ToString().Contains("【") || JA[i]["Text"].ToString().Contains("】"))
                                        {
                                            增加字幕(JA[i]["Text"].ToString());
                                        }
                                    }
                                    if (弹幕使能)
                                    {
                                        if (!JA[i]["Text"].ToString().Contains("【") || !JA[i]["Text"].ToString().Contains("】"))
                                        {
                                            增加弹幕(/*JA[i]["Name"].ToString() + "：" +*/ JA[i]["Text"].ToString());
                                        }

                                    }

                                }
                            }
                        }
                        Thread.Sleep(800);
                    }
                    catch (Exception)
                    {
                    }
                    //Thread.Sleep(10);
                }
            })).Start();
        }
        private void 播放到达结尾触发事件(object sender, EventArgs e)// AxAPlayer3Lib._IPlayerEvents_OnStateChangedEvent e)
        {
            if (播放状态)
            {
                刷新播放("直播源推流停止或卡顿，正在尝试重连(或延长设置里“默认缓冲时长”的时间）", false);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="提示内容"></param>
        /// <param name="来源">真为手动，假为自动</param>
        public void 刷新播放(string 提示内容,bool 来源)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                提示框.Visibility = Visibility.Visible;
                提示文字.Content = 提示内容;
            }));
            switch (DD.DownIofo.平台)
            {
                case "bilibili":
                    {
                        if (bilibili.根据房间号获取房间信息.是否正在直播(DD.DownIofo.房间_频道号,true))
                        {

                            new Task((() => 
                            {
                                ///需要DEBUG：CancelAsync过后，刷新播放没有显示出来
                                DD.DownIofo.WC.CancelAsync();
                                DD.DownIofo.备注 = "播放刷新";
                                DD.DownIofo.下载状态 = false;
                                DD.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                Downloader 下载对象 = Downloader.新建下载对象(DD.DownIofo.平台, DD.DownIofo.房间_频道号, bilibili.根据房间号获取房间信息.获取标题(DD.DownIofo.房间_频道号), Guid.NewGuid().ToString(), bilibili.根据房间号获取房间信息.下载地址(DD.DownIofo.房间_频道号), "播放缓冲重连", false, DD.DownIofo.主播名称, false, null);
                                MMPU.文件删除委托(DD.DownIofo.文件保存路径, "刷新播放窗口，删除LiveCache过期的缓存文件");
                                DD = 下载对象;
                                for (int i = 0; i < MMPU.播放缓冲时长; i++)
                                {
                                    Thread.Sleep(1000);
                                    if (下载对象.DownIofo.已下载大小bit > 1000)
                                    {
                                        Thread.Sleep(MMPU.播放缓冲时长 * 1000);
                                        try
                                        {
                                            Play_Open(下载对象.DownIofo.文件保存路径);
                                            //this.VlcControl.SourceProvider.MediaPlayer.Play(new Uri(下载对象.DownIofo.文件保存路径));
                                        }
                                        catch (Exception)
                                        {

                                            return;
                                        }
                                        this.Dispatcher.Invoke(new Action(delegate
                                        {
                                            DD = 下载对象;
                                            提示框.Visibility = Visibility.Collapsed;
                                        }));
                                        return;
                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(new Action(delegate
                                        {
                                            提示文字.Content = "直播源推流停止或卡顿，正在尝试重连,第" + (i + 1) + "次失败/一共尝试" + MMPU.播放缓冲时长 + "次";
                                            if (i >= MMPU.播放缓冲时长 - 1)
                                            {
                                                提示文字.Content += "\n请尝试重开播放窗口";
                                                return;
                                            }
                                        }));

                                    }
                                }
                            })).Start();
                        }
                        else
                        {
                            InfoLog.InfoPrintf(DD.DownIofo.房间_频道号 + "房间:" + DD.DownIofo.主播名称 + " 下播放，录制完成", InfoLog.InfoClass.下载必要提示);
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                提示文字.Content = "======该房间/频道 直播停止，请关闭播放窗口======";
                                return;
                            }));
                        }
                        break;
                    }
                default:
                    System.Windows.MessageBox.Show("发现了与当前版本不支持的平台，请检查更新");
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        提示框.Visibility = Visibility.Collapsed;
                    }));
                    return;
            }

        }
        private void pause_Click(object sender, MouseButtonEventArgs e)
        {

            if (播放状态)
            {
                Play_STOP();
                //this.VlcControl.SourceProvider.MediaPlayer.Pause();
                播放状态 = !播放状态;
            }
            else
            {
                Play_Play();
                //this.VlcControl.SourceProvider.MediaPlayer.Play();
                播放状态 = !播放状态;
            }
        }

        private void 音量_MouseMove(object sender, MouseEventArgs e)
        {

            try
            {
                Play_SETVolume((int)音量.Value);
                //this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
            }
            catch (Exception)
            {

            }
        }
        private void 字幕位置_MouseMove(object sender, MouseEventArgs e)
        {
            if (字幕.字幕位置 != (int)this.Width / 100 * (int)字幕位置.Value)
            {
                字幕.字幕位置 = (int)this.Width / 100 * (int)字幕位置.Value;
                if (字幕.FontSize > 50)
                {
                    字幕.FontSize = 1;
                }
                else
                {
                    字幕.FontSize++;
                }
            }
        }
        private void 弹幕透明度_MouseMove(object sender, MouseEventArgs e)
        {
            弹幕框.Opacity = 弹幕透明度.Value;

        }

        private void 播放设置按钮点击事件(object sender, MouseButtonEventArgs e)
        {
            if (设置框.Visibility == Visibility.Visible)
            {
                关闭框.Visibility = Visibility.Collapsed;
                设置框.Visibility = Visibility.Collapsed;
                播放设置按钮.Opacity = 0.3;
            }
            else
            {
                关闭框.Visibility = Visibility.Visible;
                设置框.Visibility = Visibility.Visible;
                播放设置按钮.Opacity = 1.0;
            }
        }

        private int LastWidth;
        private int LastHeight;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (HwndSource.FromVisual(this) is HwndSource source)
            {
                source.AddHook(new HwndSourceHook(WinProc));
            }
        }
        public const Int32 WM_EXITSIZEMOVE = 0x0232;
        private IntPtr WinProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            IntPtr result = IntPtr.Zero;
            switch (msg)
            {
                case WM_EXITSIZEMOVE:
                    {
                        if (this.Height != LastHeight)
                        {
                            this.Width = this.Height * 1.75;
                        }
                        if (this.Width != LastWidth)
                        {
                            this.Height = this.Width / 1.75;
                        }
                        LastWidth = (int)this.Width;
                        LastHeight = (int)this.Height;
                        break;
                    }
            }
            return result;
        }
        public event EventHandler<EventArgs> BossKey;
        private void MainWindows_Keydown(object sender, KeyEventArgs e)
        {

            //老板键(缩小所有播放窗口)
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                BossKey?.Invoke(this, EventArgs.Empty);
            }
            //音量增加
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Up))
            {
                if (音量.Value + 5 <= 100)
                {
                    音量.Value += 5;
                    Play_SETVolume((int)音量.Value);
                    //this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 100;
                    Play_SETVolume((int)音量.Value);
                    //this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
            //音量降低
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Down))
            {
                if (音量.Value - 5 >= 0)
                {
                    音量.Value -= 5;
                    Play_SETVolume((int)音量.Value);
                    //  this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 0;
                    Play_SETVolume((int)音量.Value);
                    //  this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
            //全屏回车
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                设置全屏();
            }
            //F5刷新
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.F5))
            {
                new Task(() =>
                {
                    Play_STOP();
                    //this.VlcControl.SourceProvider.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放
                    刷新播放("检测到F5按下,刷新中..",true);
                }).Start();

            }
            首页焦点.Focus();
        }
        public static DateTime 全屏操作的时间 = DateTime.UtcNow;
        public void 设置全屏()
        {
            TimeSpan ts = DateTime.UtcNow.Subtract(全屏操作的时间);
            if(ts.TotalMilliseconds>500)
            {
                全屏操作的时间 = DateTime.UtcNow;
            }
            else
            {
                return;
            }
            
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            if (字幕.字幕位置 != (int)this.Width / 100 * (int)字幕位置.Value)
            {
                字幕.字幕位置 = (int)this.Width / 100 * (int)字幕位置.Value;
                if (字幕.FontSize > 50)
                {
                    字幕.FontSize = 1;
                }
                else
                {
                    字幕.FontSize++;
                }
            }
        }
        private void VlcControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
            }
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            关闭窗口();
            this.Close();
        }
        private void 关闭窗口()
        {
            try
            {
                播放状态 = false;
                new Task(() =>
                {
                    try
                    {
                        if (this.VLCV.MediaPlayer.IsPlaying)
                        {
                            this.VLCV.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放
                        }
                        this.VLCV.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }).Start();
                DD.DownIofo.播放状态 = false;
                DD.DownIofo.备注 = "播放窗口关闭";
                DD.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                if (是否已经连接弹幕)
                {
                    try
                    {
                        listener.Dispose();
                    }
                    catch (Exception)
                    {

                    }
                    是否已经连接弹幕 = false;
                }
            }
            catch (Exception C)
            {
                System.Windows.MessageBox.Show("发生了意外的错误，请把该错误内容发送给开发者，谢谢\n错误内容：\n" + C.ToString());
            }
           
        }

        private void 置顶选择_Checked(object sender, RoutedEventArgs e)
        {
            if (置顶选择.IsChecked == true)
            {
                this.Topmost = true;
            }
            else
            {
                this.Topmost = false;
            }
        }

        private void 鼠标滚轮事件(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (音量.Value + 5 <= 100)
                {
                    音量.Value += 5;
                    Play_SETVolume((int)音量.Value);
                    //this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 100;
                    Play_SETVolume((int)音量.Value);
                    // this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
            else if (e.Delta < 0)
            {
                if (音量.Value - 5 >= 0)
                {
                    音量.Value -= 5;
                    Play_SETVolume((int)音量.Value);
                    //  this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 0;
                    Play_SETVolume((int)音量.Value);
                    //  this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
            }
        }

        private void 字幕开关_Click(object sender, RoutedEventArgs e)
        {


            if (字幕开关.IsChecked == true)
            {
                字幕使能 = true;
                字幕框.Visibility = Visibility.Visible;
                if (!是否已经连接弹幕)
                {
                    try
                    {
                        new Task(() => {
                            listener.Connect(int.Parse(DD.DownIofo.房间_频道号));
                            listener.MessageReceived += Listener_MessageReceived;
                        }).Start();
                    }
                    catch (Exception)
                    {

                    }
                    是否已经连接弹幕 = true;
                }
            }
            else
            {
                字幕框.Visibility = Visibility.Collapsed;
                字幕使能 = false;
                if (!弹幕使能 && !字幕使能)
                {
                    try
                    {
                        listener.Dispose();
                        listener = new LiveChatListener();
                    }
                    catch (Exception)
                    {

                    }
                    是否已经连接弹幕 = false;
                }
            }
        }

        private void 弹幕开关_Click(object sender, RoutedEventArgs e)
        {
            if (弹幕开关.IsChecked == true)
            {
                弹幕使能 = true;
                弹幕框.Visibility = Visibility.Visible;
                if(!是否已经连接弹幕)
                {
                    try
                    {
                        new Task(()=> {
                            listener.Connect(int.Parse(DD.DownIofo.房间_频道号));
                            listener.MessageReceived += Listener_MessageReceived;
                        }).Start();
                    }
                    catch (Exception)
                    {

                    }
                    是否已经连接弹幕 = true;
                }
            }
            else
            {
                弹幕使能 = false;
                弹幕框.Visibility = Visibility.Collapsed;
                if (!弹幕使能 && !字幕使能)
                {
                    try
                    {
                        listener.Dispose();
                        listener = new LiveChatListener();
                    }
                    catch (Exception)
                    {

                    }
                    是否已经连接弹幕 = false;
                }
            }
        }

        private void 修改字幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                //SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                字幕.字体颜色 = Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
                sb.Dispose();
                //字幕.Foreground = solidColorBrush;
                if (弹幕.FontSize > 50)
                {
                    弹幕.FontSize = 1;
                }
                else
                {
                    弹幕.FontSize++;
                }
                sb.Dispose();
            }
            colorDialog.Dispose();
            首页焦点.Focus();
        }
        private void 修改弹幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                //SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                弹幕.字体颜色 = Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
                sb.Dispose();
                //弹幕.Foreground = solidColorBrush;
                if (弹幕.FontSize > 50)
                {
                    弹幕.FontSize = 1;
                }
                else
                {
                    弹幕.FontSize++;
                }
            }
            colorDialog.Dispose();
            首页焦点.Focus();
        }


        private void 发送弹幕按钮_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(弹幕输入窗口.Text) && 弹幕输入窗口.Text.Length < 20)
            {

                弹幕发送提示.Content = bilibili.danmu.发送弹幕(DD.DownIofo.房间_频道号, 弹幕输入窗口.Text);
                弹幕输入窗口.Text = "";
            }
            else
            {
                弹幕发送提示.Content = "发送内容不能为空或超过20个字符！";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            关闭窗口();
        }

        public delegate void 播放窗口自动排序委托(string playbool);
        public event 播放窗口自动排序委托 播放窗口自动排序事件; 
        private void 播放窗口自动排列按钮_Click(object sender, RoutedEventArgs e)
        {
            播放窗口自动排序事件(窗口信息.GUID);
        }
    }
}

