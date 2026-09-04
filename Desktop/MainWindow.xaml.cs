using Core;
using Core.Account;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Pages;
using Desktop.Views.Windows;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using static Core.RuntimeObject.Detect;
using static Core.Tools.DokiDoki;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
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
        /// <summary>
        /// 房间统计刷新定时器（驱动共享缓存RoomStatistics，标题栏与DefaultPage统计面板共用）
        /// </summary>
        private System.Threading.Timer RoomStatisticsRefreshTimer;

        public static Config.RunConfig configViewModel { get; set; } = new();

        public static string P_Title = string.Empty;

        public MainWindow()
        {
            if (Application_Startup())
            {
                Environment.Exit(Core.Init.ExitCodes.FatalError);
                return;
            }
            InitializeComponent();

            this.DataContext = configViewModel;
            try
            {
                //初始化各种page
                Init();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"UI初始化出现重大错误，错误堆栈{ex.ToString()}");
            }
            Version dotnetVersion = Environment.Version;
        }

        /// <summary>
        /// 初始化各种页面内容
        /// </summary>
        public void Init()
        {
            //设置房间卡片列表页定时任务（首次10ms立即触发，之后每3秒；回调内有重入保护，上次未完成的tick会被跳过）
            DataPage.Timer_DataPage = new System.Threading.Timer(DataPage.Refresher, null, 10, 3000);
            //设置登录失效事件（失效后弹出扫码框）
            DataSource.LoginStatus.LoginFailureEvent += LoginStatus_LoginFailureEvent;
            //设置登录态检测定时任务
            DataSource.LoginStatus.Timer_LoginStatus = new System.Threading.Timer(DataSource.LoginStatus.RefreshLoginStatus, null, 1000 * 10, 1000 * 60 * 30);
            //版本更新检测
            Core.Tools.ProgramUpdates.NewVersionAvailableEvent += ProgramUpdates_NewVersionAvailableEvent;
            //设置默认显示页
            Loaded += (_, _) => RootNavigation.Navigate(typeof(DefaultPage));
            //初始化底部提示框
            SnackbarService = Desktop.App._MainSnackbarServiceProvider.GetRequiredService<ISnackbarService>();
            SnackbarService.SetSnackbarPresenter(MainSnackbar);
            //托盘图标由MainWindow.xaml中的notifyIcon实例提供，这里只需订阅一次最小化事件用于最小化到托盘
            StateChanged += MainWindow_StateChanged;
            //初始化确认窗口
            _contentDialogService.SetDialogHost(RootContentDialogPresenter);
            //初始化标题和远程模式标志以及检查远程和本地版本号一致性
            _ = InitializeTitleModeAsync();
            //监听开播事件，用于开播提醒
            Detect.detectRoom.LiveStart += DetectRoom_LiveStart;
            //初始化VLC播放器组件
            LibVLCSharp.Shared.Core.Initialize("./plugins/vlc");
            //初始化系统休眠设置
            if (Config.Core_RunConfig._PreventWindowsHibernation)
            {
                WindowsAPI.CloseWindowsHibernation();
            }
            else
            {
                WindowsAPI.OpenWindowsHibernation();
            }
            //房间统计刷新（每3秒驱动一次共享缓存；缓存有最小间隔和重入保护，数值变化时才通过事件通知UI更新）
            DataSource.RoomStatistics.Updated += RoomStatistics_Updated;
            RoomStatisticsRefreshTimer = new System.Threading.Timer(_ => _ = DataSource.RoomStatistics.RefreshAsync(), null, 1000, 3000);
            //恢复到前台时立即刷新一次统计数据和房间卡片
            Services.UiActivity.ForegroundRestored += UiActivity_ForegroundRestored;
        }

        /// <summary>
        /// 异步初始化标题和远程模式
        /// </summary>
        private async Task InitializeTitleModeAsync()
        {
            if (!Config.Core_RunConfig._DesktopIP.Contains("//127.") && !Config.Core_RunConfig._DesktopIP.Contains("//0.") && !Config.Core_RunConfig._DesktopIP.Contains("localhost"))
            {
                ToConnectToRemoteServer = true;
            }

            int retryCount = 0;
            const int maxRetries = 10;

            while (retryCount < maxRetries)
            {
                try
                {
                    DokiClass doki;
                    if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                    {
                        doki = await NetWork.Get.GetBodyAsync<DokiClass>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/dokidoki");
                    }
                    else
                    {
                        doki = Core.Tools.DokiDoki.GetDoki();
                    }

                    if (doki != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            if (Core.Init.Ver != doki.Ver)
                            {
                                MainWindow.SnackbarService.Show("远程版本不一致", $"检测到远程模式下远程版本与本地Desktop版本不一致！\n本地Desktop版本号:【{Core.Init.Ver}】|远程版本号:【{doki.Ver}】", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(5));
                                this.Title = $"{doki.InitType}|本地 {Core.Init.Ver}|远程 {doki.Ver}| %%% |{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion}){(ToConnectToRemoteServer ? "【远程模式】" : "")}$$$";
                            }
                            else
                            {
                                this.Title = $"{doki.InitType}|{doki.Ver}| %%% |{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion}){(ToConnectToRemoteServer ? "【远程模式】" : "")}$$$";
                            }
                            P_Title = this.Title;
                            //标题模板就绪后立即用当前统计缓存填充一次占位符，避免统计数字不变时标题占位符长期不替换
                            UpdateWindowTitle();
                        });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(InitializeTitleModeAsync), "初始化标题失败", ex, false);
                }

                retryCount++;
                if (retryCount < maxRetries)
                {
                    await Task.Delay(8000);
                }
            }
        }

        public bool Application_Startup()
        {
            Process process = RunningInstance();
            if (process != null)
            {
                System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
                    "已经有DDTV的Desktop实例正在运行中" +
                   "\r点击'是'强制启动一个新DDTV" +
                   "\r点击'否'阻止打开新窗口和新DDTV" +
                   $"\r======参考信息======" +
                   $"\rId:{process.Id}" +
                   $"\rProcessName:{process.ProcessName}"
                   , "已有DDTV实例正在运行", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    System.Threading.Thread.Sleep(500);
                    return true;
                }
            }
            return false;
        }

        public static Process RunningInstance(bool IsStart = true)
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

        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        /// <summary>
        /// 窗口缩小事件
        /// </summary>
        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (Config.Core_RunConfig._ZoomOutMode != 0 && this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                //进入后台模式：各UI轮询定时器跳过执行（UI不可见，刷新纯属浪费CPU和网络请求）
                Services.UiActivity.SetBackground(true);
            }
            else if (this.WindowState != WindowState.Minimized)
            {
                //窗口恢复显示（托盘左键/强制显示都会把状态置回Normal，必经此分支）
                Services.UiActivity.SetBackground(false);
            }
        }

        /// <summary>
        /// 开播事件，触发开播提醒
        /// </summary>
        private void DetectRoom_LiveStart(object? sender, (RoomCardClass Card, bool Danma_MessageReceived) LiveInvoke)
        {
            RoomCardClass roomCard = LiveInvoke.Card;
            List<TriggerType> triggerTypes = sender as List<TriggerType> ?? new List<TriggerType>();
            if (roomCard.IsRemind && triggerTypes.Contains(TriggerType.RegularTasks))
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Views.Windows.HudNotification.NotifyLiveReminder(roomCard.Name, roomCard.Title.Value, roomCard.RoomId);
                });
            }
        }

        /// <summary>
        /// 新版本检测事件
        /// </summary>
        private void ProgramUpdates_NewVersionAvailableEvent(object? sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                MainWindow.SnackbarService.Show("检测到更新", $"检测到DDTV新版本：【{sender}】，{(ToConnectToRemoteServer ? "请更新远程服务端后，再到设置页面点击更新按钮进行更新" : "请到设置页面点击更新按钮进行更新")}", ControlAppearance.Primary, new SymbolIcon(SymbolRegular.DocumentHeaderArrowDown20), TimeSpan.FromSeconds(5));
            });
        }

        /// <summary>
        /// 登陆失效事件
        /// </summary>
        private void LoginStatus_LoginFailureEvent(object? sender, EventArgs e)
        {
            if (!DataSource.LoginStatus.LoginWindowDisplayStatus)
            {
                DataSource.LoginStatus.LoginWindowDisplayStatus = true;
                Dispatcher.InvokeAsync(() =>
                {
                    if (Core.Init.GetRunTime() < 90)
                    {
                        QrLogin qrLogin = new QrLogin();
                        qrLogin.ShowDialog();
                    }
                    else
                    {
                        MainWindow.SnackbarService.Show("登录态检查失败", $"检查账号信息的登陆状态有效性失败，该提示一般是由于登录态已过期造成的，请尝试重新登陆", ControlAppearance.Primary, new SymbolIcon(SymbolRegular.CloudError20), TimeSpan.FromSeconds(30));
                    }
                });
            }
        }

        /// <summary>
        /// 关闭后事件
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            if (!IsProgrammaticClose)
            {
                DataPage.Timer_DataPage?.Dispose();
                DataSource.LoginStatus.Timer_LoginStatus?.Dispose();
                Environment.Exit(Core.Init.ExitCodes.FatalError);
            }
        }

        /// <summary>
        /// 关闭前确认关闭
        /// </summary>
        private async void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsProgrammaticClose)
            {
                e.Cancel = true;

                int action = Config.Core_RunConfig._CloseButtonAction;
                if (action == 0)
                {
                    (int choice, bool remember) = await ShowCloseActionDialogAsync();
                    if (choice != 0)
                    {
                        if (remember)
                        {
                            //勾选"不再提示"，记住用户本次的选择
                            Config.Core_RunConfig._CloseButtonAction = choice;
                        }
                        action = choice;
                    }
                }

                switch (action)
                {
                    case 1:
                        //最小化到托盘后台（与MainWindow_StateChanged的最小化到托盘路径保持一致）
                        this.Hide();
                        Services.UiActivity.SetBackground(true);
                        break;
                    case 2:
                        //有直播间正在录制时先弹退出确认框（显示录制中的直播间数量），确认后才真正退出
                        if (await Services.ExitService.ConfirmExitIfRecordingAsync(this))
                        {
                            Services.ExitService.Exit();
                        }
                        break;
                    //case 0：用户未做出有效选择，维持窗口现状
                }
            }
        }

        /// <summary>
        /// 弹出关闭按钮行为询问对话框（最小化到托盘/退出），支持勾选"不再提示"记住选择
        /// 返回(行为, 是否记住)：行为 1:最小化到托盘  2:退出  0:未做出有效选择
        /// </summary>
        private async Task<(int Action, bool Remember)> ShowCloseActionDialogAsync()
        {
            var rememberCheckBox = new System.Windows.Controls.CheckBox
            {
                Content = "记住我的选择，不再提示",
                Margin = new Thickness(0, 14, 0, 0)
            };
            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "关闭确认",
                Content = new System.Windows.Controls.StackPanel
                {
                    Children =
                    {
                        new System.Windows.Controls.TextBlock
                        {
                            Text = "点击关闭按钮时，您希望DDTV做什么？\r\n最小化到托盘：程序将继续在后台运行，录制任务不受影响\r\n退出：结束所有录制任务和播放窗口",
                            TextWrapping = TextWrapping.Wrap
                        },
                        rememberCheckBox
                    }
                },
                PrimaryButtonText = "最小化到托盘",
                SecondaryButtonText = "退出",
                IsCloseButtonEnabled = false,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var result = await messageBox.ShowDialogAsync();

            return result switch
            {
                Wpf.Ui.Controls.MessageBoxResult.Primary => (1, rememberCheckBox.IsChecked == true),
                Wpf.Ui.Controls.MessageBoxResult.Secondary => (2, rememberCheckBox.IsChecked == true),
                _ => (0, false)
            };
        }

        /// <summary>
        /// 统计数据变化事件：封送到UI线程更新标题（只有数值变化时才会触发，不再每秒刷新）
        /// </summary>
        private static void RoomStatistics_Updated()
        {
            _ = Application.Current.Dispatcher.InvokeAsync(UpdateWindowTitle);
        }

        /// <summary>
        /// 恢复到前台时立即拉取最新统计并刷新房间卡片，避免界面展示过期数据
        /// </summary>
        private void UiActivity_ForegroundRestored()
        {
            _ = DataSource.RoomStatistics.RefreshAsync(true);
            Views.Pages.DataPage.RequestImmediateRefresh();
        }

        /// <summary>
        /// 根据共享统计缓存更新窗口标题和导航标题（需在UI线程调用）
        /// </summary>
        public static void UpdateWindowTitle()
        {
            try
            {
                var count = DataSource.RoomStatistics.Current;

                configViewModel.DataPageTitle = $"房间列表 ({count.RecCount})";
                configViewModel.OnPropertyChanged("DataPageTitle");

                configViewModel.ProgramTitle = P_Title.Replace("%%%", $"{count.RecCount}录制中|{count.LiveCount}开播中|{count.MonitoringCount}监控中")
                    .Replace("$$$", $"{(!Core.RuntimeObject.Account.AccountInformation.State ? "【警告！登陆态已失效】" : "")}");
                configViewModel.OnPropertyChanged("ProgramTitle");
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(UpdateWindowTitle), "更新窗口标题出现错误", ex, false);
            }
        }
    }
}
