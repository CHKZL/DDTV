using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Pages;
using Desktop.Views.Windows;
using Masuit.Tools.Win32;
using Microsoft.Extensions.DependencyInjection;
using Notification.Wpf;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using static Core.RuntimeObject.Detect;
using static Core.Tools.DokiDoki;
using static Server.WebAppServices.Api.get_system_resources;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        /// <summary>
        /// 后台托盘
        /// </summary>
        private NotifyIcon notifyIcon = null;
        /// <summary>
        /// 系统托盘通知
        /// </summary>
        public static NotificationManager notificationManager = new NotificationManager();
        /// <summary>
        /// 确认窗口
        /// </summary>
        public static IContentDialogService _contentDialogService = new ContentDialogService();
        /// <summary>
        /// 程序关闭标志
        /// </summary>
        public static bool IsProgrammaticClose = false;
        /// <summary>
        /// 底部提示框
        /// </summary>
        public static ISnackbarService SnackbarService;
        /// <summary>
        /// 是否连接远程服务器
        /// </summary>
        public static bool ToConnectToRemoteServer = false;

        public static Config.RunConfig configViewModel { get; set; } = new();


        public MainWindow()
        {
            if(Application_Startup())
            {
                Environment.Exit(-114514);
                return;
            }
            InitializeComponent();

            this.DataContext = configViewModel;
            try
            {
                Thread.Sleep(1000);
                //初始化各种page
                Init();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"UI初始化出现重大错误，错误堆栈{ex.ToString()}");
            }

        }

        /// <summary>
        /// 初始化各种页面内容
        /// </summary>
        public void Init()
        {
            //设置房间卡片列表页定时任务
            DataPage.Timer_DataPage = new Timer(DataPage.Refresher, null, 1, 1000);
            //设置登录失效事件（失效后弹出扫码框）
            DataSource.LoginStatus.LoginFailureEvent += LoginStatus_LoginFailureEvent;
            //设置登录态检测定时任务
            DataSource.LoginStatus.Timer_LoginStatus = new Timer(DataSource.LoginStatus.RefreshLoginStatus, null, 1000 * 10, 1000 * 60 * 30);
            //版本更新检测
            Core.Tools.ProgramUpdates.NewVersionAvailableEvent += ProgramUpdates_NewVersionAvailableEvent;
            //设置默认显示页
            Loaded += (_, _) => RootNavigation.Navigate(typeof(DefaultPage));
            //初始化底部提示框
            SnackbarService = Desktop.App._MainSnackbarServiceProvider.GetRequiredService<ISnackbarService>();
            SnackbarService.SetSnackbarPresenter(MainSnackbar);
            //初始化托盘
            notify();
            //初始化确认窗口
            _contentDialogService.SetDialogHost(RootContentDialogPresenter);
            //初始化标题和远程模式标志以及检查远程和本地版本号一致性
            InitializeTitleMode();
            //监听开播事件，用于开播提醒
            Detect.detectRoom.LiveStart += DetectRoom_LiveStart;
            //初始化VLC播放器组件
            LibVLCSharp.Shared.Core.Initialize("./plugins/vlc");

        }

        private void InitializeTitleMode()
        {
            Task.Run(() =>
            {
                if (!Config.Core_RunConfig._DesktopIP.Contains("//127.") && !Config.Core_RunConfig._DesktopIP.Contains("//0."))
                {
                    ToConnectToRemoteServer = true;
                }

                bool F = false;
                DokiClass doki = null;
                do
                {
                    if (!F)
                    {
                        F = true;
                    }
                    else
                    {
                        Thread.Sleep(8000);
                    }
                    try
                    {
                        doki = NetWork.Get.GetBody<DokiClass>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/dokidoki");
                        if (doki != null)
                        {
                            Dispatcher.Invoke(() =>
                            {

                                if (Core.Init.Ver != doki.Ver)
                                {
                                    MainWindow.SnackbarService.Show("远程版本不一致", $"检测到远程模式下远程版本与本地Desktop版本不一致！这可能会造成未知的问题，请尽快更新双端到最新版本！\n本地Desktop版本号:【{Core.Init.Ver}】|远程版本号:【{doki.Ver}】", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(5));
                                    this.Title = $"{doki.InitType}|本地 {Core.Init.Ver}|远程 {doki.Ver}|{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion}){(ToConnectToRemoteServer ? "【远程模式】" : "")}";
                                }
                                else
                                {
                                    this.Title = $"{doki.InitType}|{doki.Ver}|{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion}){(ToConnectToRemoteServer ? "【远程模式】" : "")}";
                                }
                                UI_TitleBar.Title = this.Title;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(InitializeTitleMode), "初始化标题失败", ex, false);
                    }

                } while (doki == null);
            });
        }

        public bool Application_Startup()
        {
            Process process = RuningInstance();
            if (process != null)
            {
                System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
                    "已经有DDTV的Desktop实例正在运行中" +
                   "\r点击‘是’强制启动一个新DDTV" +
                   "\r点击‘否’阻止打开新窗口和新DDTV" +
                   $"\r======参考信息======" +
                   $"\rId:{process.Id}" +
                   $"\rProcessName:{process.ProcessName}"
                   , "已有DDTV实例正在运行", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    this.Show();  // 显示窗口
                    this.WindowState = WindowState.Normal;  // 设置窗口状态为正常
                    System.Threading.Thread.Sleep(500);
                    return true;
                    
                }
            }
            return false;
        }
        public static Process RuningInstance(bool IsStart = true)
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] Processes = Process.GetProcessesByName(currentProcess.ProcessName);
                foreach (Process process in Processes)
                {
                    if (!IsStart || process.Id != currentProcess.Id)
                    {
                        string PA = Assembly.GetExecutingAssembly().Location.Replace("/", "\\");
                        string PB = currentProcess.MainModule.FileName;
                        string PAA = PA.Replace(PA.Split('.')[PA.Split('.').Length - 1], "");
                        string PBA = PB.Replace(PB.Split('.')[PB.Split('.').Length - 1], "");
                        if (PAA == PBA)
                        {
                            return process;
                        }
                    }
                }
            }
            catch (Exception) { }
            return null;
        }
        private void notify()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "DDTV";
            notifyIcon.Icon = new System.Drawing.Icon("DDTV.ico");
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += NotifyIcon_Click;
            StateChanged += MainWindow_StateChanged; ;
        }

        /// <summary>
        /// 窗口缩小事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (Config.Core_RunConfig._ZoomOutMode!=0 && this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }
        /// <summary>
        /// 双击托盘ICON事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void NotifyIcon_Click(object? sender, EventArgs e)
        {
            this.Show();  // 显示窗口
            this.WindowState = WindowState.Normal;  // 设置窗口状态为正常
        }



        /// <summary>
        /// 开播事件，触发开播提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DetectRoom_LiveStart(object? sender, (RoomCardClass Card, bool Danma_MessageReceived) LiveInvoke)
        {
            RoomCardClass roomCard = LiveInvoke.Card;
            List<TriggerType> triggerTypes = sender as List<TriggerType> ?? new List<TriggerType>();
            if (roomCard.IsRemind && triggerTypes.Contains(TriggerType.RegularTasks))
            {
                Dispatcher.Invoke(() =>
                {
                    notificationManager.Show(new NotificationContent
                    {
                        Title = "DDTV-开播提醒",
                        Message = $"【{roomCard.Name}】的直播开始啦",
                        Type = NotificationType.Information
                    });
                });

            }
        }

        /// <summary>
        /// 新版本检测事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramUpdates_NewVersionAvailableEvent(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MainWindow.SnackbarService.Show("检测到更新", $"检测到DDTV新版本：【{sender}】，{(ToConnectToRemoteServer ? "请更新远程服务端后，再到设置页面点击更新按钮进行更新" : "请到设置页面点击更新按钮进行更新")}", ControlAppearance.Primary, new SymbolIcon(SymbolRegular.DocumentHeaderArrowDown20), TimeSpan.FromSeconds(5));
            });
        }

        /// <summary>
        /// 登陆失效事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void LoginStatus_LoginFailureEvent(object? sender, EventArgs e)
        {
            if (!DataSource.LoginStatus.LoginWindowDisplayStatus)
            {
                DataSource.LoginStatus.LoginWindowDisplayStatus = true;
                Dispatcher.Invoke(() =>
                {
                    QrLogin qrLogin = new QrLogin();
                    qrLogin.ShowDialog();
                });

            }
        }

        /// <summary>
        /// 关闭后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            DataPage.Timer_DataPage?.Dispose();
            DataSource.LoginStatus.Timer_LoginStatus?.Dispose();
            Environment.Exit(-114514);
        }

        /// <summary>
        /// 关闭前确认关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsProgrammaticClose)
            {
                System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("确认要关闭DDTV吗？\r\n关闭后所有录制任务以及播放窗口均会结束。", "关闭确认", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
