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

namespace PlayW
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Downloader DD { set; get; }
        public bool 播放状态 = false;
        public bilibili.danmu DM = new bilibili.danmu();
        public bool 弹幕使能 = false;
        public bool 字幕使能=false;
        public bool 窗口是否打开 = false;

        public MainWindow(Downloader A, int 默认音量, SolidColorBrush 弹幕颜色, SolidColorBrush 字幕颜色, int 弹幕大小, int 字幕大小, int 宽度, int 高度)
        {
            InitializeComponent();
            this.Width = 宽度;

            this.Height = 高度;
            窗口是否打开 = true;
            音量.Value = 默认音量;
            this.Closed += 关闭窗口事件;
            DD = A;
            字幕.Foreground = 字幕颜色;
            弹幕.Foreground = 弹幕颜色;
            字幕.FontSize = 字幕大小;
            弹幕.FontSize = 弹幕大小;
            DD.DownIofo.文件保存路径 = AppDomain.CurrentDomain.BaseDirectory + "tmp\\LiveCache\\" + A.DownIofo.标题 + A.DownIofo.事件GUID + ".flv";
            this.Title = A.DownIofo.标题;
            设置框.Visibility = Visibility.Collapsed;
            关闭框.Visibility = Visibility.Collapsed;
            if (MMPU.第一次打开播放窗口)
            {
                设置框.Visibility = Visibility.Visible;
                关闭框.Visibility = Visibility.Visible;
                MMPU.第一次打开播放窗口 = false;
            }
            if (MMPU.versionMajor != 10)
            {
                弹幕使能 = false;
                弹幕开关.IsChecked = false;
                弹幕开关.Visibility = Visibility.Collapsed;
                字幕使能 = false;
                字幕开关.IsChecked = false;
                字幕开关.Visibility = Visibility.Collapsed;
                修改字幕颜色按钮.Visibility = Visibility.Collapsed;
                修改弹幕颜色按钮.Visibility = Visibility.Collapsed;
                非win10提示.Visibility = Visibility.Visible;
            }
            else
            {
                非win10提示.Visibility = Visibility.Collapsed;
            }
        }

        private void 关闭窗口事件(object sender, EventArgs e)
        {
            播放状态 = false;
            new Thread(new ThreadStart(delegate {
                try
                {
                    this.VlcControl.SourceProvider.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放
                }
                catch (Exception)
                {
                }
            })).Start();
            DD.DownIofo.播放状态 = false;
            窗口是否打开 = false;
        }
        public void 增加字幕(string A)
        {
            int 显示的字幕数 = 3;
            int Len = 0;
            string B = "";
            string C = "";
            this.Dispatcher.Invoke(new Action(delegate
            {
                B = 字幕.Text;
                Len = 字幕.Text.Split('\n').Length;
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
                字幕.Text = A + "\n" + C;
            }));
        }
        public void 增加弹幕(string A)
        {
            int 显示的弹幕数 = 30;
            int Len = 0;
            string B = "";
            string C = "";
            this.Dispatcher.Invoke(new Action(delegate
            {
                B = 弹幕.Text;
                Len = 弹幕.Text.Split('\n').Length;
            }));
            if (Len > 显示的弹幕数)
            {
                for (int i = 0; i < 显示的弹幕数; i++)
                {
                    if(i==0)
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
                弹幕.Text = A + "\n" + C;
            }));


        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string A = System.IO.Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "libvlc", /*IntPtr.Size == 4 ?*/ "win-x64" /*: "win-x64"*/);
            this.VlcControl.SourceProvider.CreatePlayer(new DirectoryInfo(A));
            this.Dispatcher.Invoke(new Action(delegate
            {
                提示框.Visibility = Visibility.Visible;
                提示文字.Content = "该房间已开播,检测直播流状态,等待推流数据中....";
            }));
            new Thread(new ThreadStart(delegate
            {
                MMPU.DownList.Add(DD);
                DD.Start("直播观看缓冲进行中");
                DD.DownIofo.备注 = "直播观看缓冲进行中";
                Thread.Sleep(2000);
                this.Dispatcher.Invoke(new Action(delegate
                {
                    提示框.Visibility = Visibility.Collapsed;
                }));
                try
                {
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                    }));

                    this.VlcControl.SourceProvider.MediaPlayer.Play(new Uri(DD.DownIofo.文件保存路径));
                    this.VlcControl.SourceProvider.MediaPlayer.EndReached += 播放到达结尾触发事件;
                    播放状态 = true;
                    if(DD.DownIofo.平台=="bilibili")
                    {
                        获取弹幕();
                    }
                }
                catch (Exception)
                {

                }
            })).Start();
         
        }
        private void 获取弹幕()
        {
            new Thread(new ThreadStart(delegate {
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
                                        增加弹幕(JA[i]["Name"].ToString() + "：" + JA[i]["Text"].ToString());
                                    }

                                }
                            }
                        }
                        Thread.Sleep(800);
                    }
                    catch (Exception)
                    {
                    }
                    Thread.Sleep(1000);
                }
            })).Start();
        }
        private void 播放到达结尾触发事件(object sender, Vlc.DotNet.Core.VlcMediaPlayerEndReachedEventArgs e)
        {
            if (播放状态)
            {
                刷新播放("直播源推流停止或卡顿，正在尝试重连");
            }
        }
        public void 刷新播放(string 提示内容)
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
                        if (bilibili.根据房间号获取房间信息.是否正在直播(DD.DownIofo.房间_频道号))
                        {
                           
                            new Thread(new ThreadStart(delegate
                            {
                                DD.DownIofo._wc.CancelAsync();
                                DD.DownIofo.备注 = "用户取消";
                                Downloader 下载对象 = Downloader.新建下载对象(DD.DownIofo.平台, DD.DownIofo.房间_频道号, bilibili.根据房间号获取房间信息.获取标题(DD.DownIofo.房间_频道号), Guid.NewGuid().ToString(), bilibili.根据房间号获取房间信息.下载地址(DD.DownIofo.房间_频道号), "播放缓冲重连", false);
                                MMPU.文件删除委托(DD.DownIofo.文件保存路径);
                                DD = 下载对象;
                                for (int i = 0; i < 3; i++)
                                {
                                    Thread.Sleep(1000);
                                    if (下载对象.DownIofo.已下载大小bit > 1000)
                                    {
                                        Thread.Sleep(1000);
                                        this.VlcControl.SourceProvider.MediaPlayer.Play(new Uri(下载对象.DownIofo.文件保存路径));
                                        this.Dispatcher.Invoke(new Action(delegate
                                        {
                                            提示框.Visibility = Visibility.Collapsed;
                                        }));
                                        return;
                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(new Action(delegate
                                        {
                                            提示文字.Content = "直播源推流停止或卡顿，正在尝试重连,第" + (i + 1) + "次失败/一共尝试3次";
                                            if (i == 2)
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
                            this.Dispatcher.Invoke(new Action(delegate
                            {                           
                                提示文字.Content = "该房间/频道 直播停止..";
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
                this.VlcControl.SourceProvider.MediaPlayer.Pause();
                播放状态 = !播放状态;
            }
            else
            {
                this.VlcControl.SourceProvider.MediaPlayer.Play();
                播放状态 = !播放状态;
            }
        }

        private void 音量_MouseMove(object sender, MouseEventArgs e)
        {

            this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
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
            HwndSource source = HwndSource.FromVisual(this) as HwndSource;
            if (source != null)
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
                        else
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
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 100;
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
            //音量降低
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Down))
            {
                if (音量.Value - 5 >= 0)
                {
                    音量.Value -= 5;
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 0;
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
            //全屏回车
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
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
            //F5刷新
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.F5))
            {
                new Task(() =>
                {
                    this.VlcControl.SourceProvider.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放
                    刷新播放("检测到F5按下,刷新中..");
                }).Start();
               
            }
        }
        private void VlcControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            //播放状态 = false;
            //new Task(() =>
            //{
            //    this.VlcControl.SourceProvider.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放

            //}).Start();
            //DD.DownIofo.播放状态 = false;
            this.VlcControl.Dispose();
            this.Close();
        }

        private void 置顶选择_Checked(object sender, RoutedEventArgs e)
        {
            if(置顶选择.IsChecked==true)
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
            if (e.Delta>0)
            {
                if (音量.Value + 5 <= 100)
                {
                    音量.Value += 5;
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 100;
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
            }
            else if (e.Delta<0)
            {
                if (音量.Value - 5 >= 0)
                {
                    音量.Value -= 5;
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
                }
                else
                {
                    音量.Value = 0;
                    this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)音量.Value;
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
            }
            else
            {
                字幕框.Visibility = Visibility.Collapsed;
                字幕使能 = false;
            }
        }

        private void 弹幕开关_Click(object sender, RoutedEventArgs e)
        {
            if (弹幕开关.IsChecked == true)
            {
                弹幕使能 = true;
                弹幕框.Visibility = Visibility.Visible;
            }
            else
            {
                弹幕使能 = false;
                弹幕框.Visibility = Visibility.Collapsed;
            }
        }

        private void 修改字幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
               
                字幕.Foreground = solidColorBrush;
            }
            首页焦点.Focus();
        }
        private void 修改弹幕颜色按钮点击事件(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                
                弹幕.Foreground = solidColorBrush;
            }
            首页焦点.Focus();
        }
    }
}
