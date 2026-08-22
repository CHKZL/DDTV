using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Wpf.Ui.Tray;

namespace Desktop
{
	public partial class NotifyIcon : UserControl
	{
		private static readonly int WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");
		private HwndSource? _hwndSource;
		private MyNotifyIconService? _notifyIconService;
        private System.Windows.Controls.ContextMenu _contextMenu;

		[DllImport("user32.dll")]
		private static extern int RegisterWindowMessage(string lpString);

		/// <summary>
		/// 构造函数，初始化控件并绑定加载与卸载事件。
		/// </summary>
		public NotifyIcon()
		{
			Loaded += NotifyIcon_Loaded;
			Unloaded += NotifyIcon_Unloaded;
		}

		/// <summary>
		/// 控件加载时初始化托盘图标服务、右键菜单，并注册窗口消息钩子。
		/// </summary>
		private void NotifyIcon_Loaded(object sender, RoutedEventArgs e)
		{
			var mainWindow = Application.Current.MainWindow;
			if (mainWindow != null && _hwndSource == null)
			{
				_hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(mainWindow).Handle);
				_hwndSource.AddHook(WndProc);

				// 构建右键菜单
				_contextMenu = new System.Windows.Controls.ContextMenu();
				var forceShowMenu = new Wpf.Ui.Controls.MenuItem { Header = "强制显示" };
				forceShowMenu.Click += rightClickForceShow;
				var exitMenu = new Wpf.Ui.Controls.MenuItem { Header = "退出" };
				exitMenu.Click += RightClickExit;
				_contextMenu.Items.Add(forceShowMenu);
				_contextMenu.Items.Add(new Separator());
				_contextMenu.Items.Add(exitMenu);

				// 初始化自定义 NotifyIconService
				_notifyIconService = new MyNotifyIconService
				{
					TooltipText = "DDTV",
					Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/DDTV.ico", UriKind.Absolute)),
					ContextMenu = _contextMenu
				};
				//注意：这里不能调用SetParentWindow，WPF-UI的NotifyIconService.SetParentWindow内部会订阅主窗口Closing事件并直接Dispose托盘图标，
				//而主窗口的FluentWindow_Closing会取消关闭(e.Cancel=true)等待用户确认，Closing事件即使被取消其余订阅者仍会被调用，
				//导致点击关闭按钮弹确认框时托盘图标即被注销；用户选"否"后窗口还在但图标已消失，再最小化到托盘就再也找不回窗口。
				//无参Register()内部会自动使用Application.Current.MainWindow作为父窗口，行为一致且没有这个自动注销的副作用。
				_notifyIconService.Register();
			}
		}

		/// <summary>
		/// 控件卸载时移除窗口消息钩子并注销托盘图标。
		/// </summary>
		private void NotifyIcon_Unloaded(object sender, RoutedEventArgs e)
		{
			if (_hwndSource != null)
			{
				_hwndSource.RemoveHook(WndProc);
				_hwndSource = null;
			}
			_notifyIconService?.Unregister();
		}

		/// <summary>
		/// 窗口消息钩子，监听 explorer.exe 重启后自动恢复托盘图标。
		/// </summary>
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_TASKBARCREATED)
			{
				Dispatcher.Invoke(() =>
				{
					_notifyIconService?.Register();
				});
			}
			return IntPtr.Zero;
		}

		/// <summary>
		/// 右键退出菜单项点击事件，调用主窗口的退出确认逻辑。
		/// </summary>
		private async void RightClickExit(object sender, RoutedEventArgs e)
		{
			MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
			if (mainWindow != null)
			{
				bool shouldExit = await mainWindow.ShowExitConfirmationAsync();
				if (shouldExit)
				{
					Environment.Exit(Core.Init.ExitCodes.FatalError);
				}
			}
		}

		/// <summary>
		/// 右键“强制显示”菜单项点击事件，显示并激活主窗口。
		/// </summary>
		private void rightClickForceShow(object sender, RoutedEventArgs e)
		{
			MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
			if (mainWindow != null)
			{
				mainWindow.Show();
				mainWindow.WindowState = WindowState.Normal;
				mainWindow.Left = (SystemParameters.PrimaryScreenWidth - mainWindow.ActualWidth) / 2;
				mainWindow.Top = (SystemParameters.PrimaryScreenHeight - mainWindow.ActualHeight) / 2;
				mainWindow.Activate();
			}
		}

		/// <summary>
		/// 自定义 NotifyIconService，重写左键点击事件以显示主窗口。
		/// </summary>
		private class MyNotifyIconService : NotifyIconService
		{
			protected override void OnLeftClick()
			{
				base.OnLeftClick();
				var mainWindow = Application.Current.MainWindow as MainWindow;
				if (mainWindow != null)
				{
					mainWindow.Show();
					mainWindow.WindowState = WindowState.Normal;
				}
			}
		}
	}

}
