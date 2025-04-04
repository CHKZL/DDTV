﻿using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Pages;
using Desktop.Views.Windows;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using Notification.Wpf;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using static Core.RuntimeObject.Detect;
using static Core.Tools.DokiDoki;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
		/// <summary>
		/// 后台托盘     改为NotifyIcon控件实现，原代码撇了
		/// </summary>


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
        /// <summary>
        /// 更新目录房间列表录制中数量定时器
        /// </summary>
        private Timer IpvDetectionTimer;

        public static Config.RunConfig configViewModel { get; set; } = new();

        public static string P_Title = string.Empty;



        public MainWindow()
        {
            if (Application_Startup())
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
				InitializeNotifyIcon();
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
            InitializeNotifyIcon();
			//初始化确认窗口
			_contentDialogService.SetDialogHost(RootContentDialogPresenter);
            //初始化标题和远程模式标志以及检查远程和本地版本号一致性
            InitializeTitleMode();
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
            //更新目录房间列表录制中数量
            IpvDetectionTimer = new Timer(UpdateNumberRecordedRoomsInDirectoryRoomList, null, 1, 1000);
        }

        private void InitializeTitleMode()
        {
            Task.Run(() =>
            {
                if (!Config.Core_RunConfig._DesktopIP.Contains("//127.") && !Config.Core_RunConfig._DesktopIP.Contains("//0.") && !Config.Core_RunConfig._DesktopIP.Contains("localhost"))
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
                        if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                        {
                            doki = NetWork.Get.GetBody<DokiClass>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/dokidoki");
                        }
                        else
                        {
                            doki = Core.Tools.DokiDoki.GetDoki();
                        }
                        if (doki != null)
                        {
                            Dispatcher.Invoke(() =>
                            {

                                if (Core.Init.Ver != doki.Ver)
                                {
                                    MainWindow.SnackbarService.Show("远程版本不一致", $"检测到远程模式下远程版本与本地Desktop版本不一致！这可能会造成未知的问题，请尽快更新双端到最新版本！\n本地Desktop版本号:【{Core.Init.Ver}】|远程版本号:【{doki.Ver}】", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(5));
                                    this.Title = $"{doki.InitType}|本地 {Core.Init.Ver}|远程 {doki.Ver}| %%% |{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion}){(ToConnectToRemoteServer ? "【远程模式】" : "")}";
                                }
                                else
                                {
                                    this.Title = $"{doki.InitType}|{doki.Ver}| %%% |{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion}){(ToConnectToRemoteServer ? "【远程模式】" : "")}";
                                }
                                P_Title = this.Title;
                                //UI_TitleBar.Title = P_Title.Replace("%%%","正在初始化");
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
        

        /// <summary>
        /// 初始化托盘图标，原有托盘图标动作重写到NotifyIcon控件内
        /// </summary>
		private void InitializeNotifyIcon()
		{
			NotifyIcon notifyIconWindow = new NotifyIcon();
			StateChanged += MainWindow_StateChanged;
		}

		/// <summary>
		/// 窗口缩小事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (Config.Core_RunConfig._ZoomOutMode != 0 && this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
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
                    if (Core.Init.GetRunTime() < 90)
                    {
                        QrLogin qrLogin = new QrLogin();
                        qrLogin.ShowDialog();
                    }
                    else
                    {
                        MainWindow.SnackbarService.Show("登录态检查失败", $"检查账号信息的登陆状态有效性失败，该提示一般是由于登录态已过期造成的（如账号在其他地方登陆过多，异地登录等触发风控），也有可能是网络不稳定造成，如果频繁提示或者录制失败请尝试重新登陆，重新登陆请到设置-登录态管理中进行", ControlAppearance.Primary, new SymbolIcon(SymbolRegular.CloudError20), TimeSpan.FromSeconds(30));
                    }
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


        /// <summary>
        /// 更新目录房间列表录制中数量
        /// </summary>
        /// <param name="state"></param>
        public static void UpdateNumberRecordedRoomsInDirectoryRoomList(object state)
        {
            try
            {
                (int MonitoringCount, int LiveCount, int RecCount) count = new();

                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    count = NetWork.Post.PostBody<(int MonitoringCount, int LiveCount, int RecCount)>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/get_rooms/room_statistics").Result;
                }
                else
                {
                    count = Core.RuntimeObject._Room.Overview.GetRoomStatisticsOverview();
                }


                configViewModel.DataPageTitle = $"房间列表 ({count.RecCount})";
                configViewModel.OnPropertyChanged("DataPageTitle");

                configViewModel.ProgramTitle = P_Title.Replace("%%%",$"{count.RecCount}录制中|{count.LiveCount}开播中|{count.MonitoringCount}监控中");
                configViewModel.OnPropertyChanged("ProgramTitle");
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(UpdateNumberRecordedRoomsInDirectoryRoomList), "更新房间统计出现错误，错误堆栈已写文本记录文件", ex, false);
            }
        }
    }
}
