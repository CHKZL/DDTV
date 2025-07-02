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
		private ContextMenu _contextMenu;

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
				_contextMenu = new ContextMenu();
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
				_notifyIconService.SetParentWindow(mainWindow);
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
		/// 右键退出菜单项点击事件，弹出确认对话框并处理退出逻辑。
		/// </summary>
		private async void RightClickExit(object sender, RoutedEventArgs e)
		{
			var messageBox = new Wpf.Ui.Controls.MessageBox
			{
				Title = "关闭确认",
				Content = "确认要关闭DDTV吗？\r\n关闭后所有录制任务以及播放窗口均会结束。",
				PrimaryButtonText = "是",
				SecondaryButtonText = "否",
				IsCloseButtonEnabled = false 
				/*注意：IsCloseButtonEnabled 属性仅在 Wpf.Ui 4.0.3里可用，
				如果确定要更新的话可以把mainwindow里面的 FluentWindow_Closing也给换成这个退出，统一风格（
				如果不合并把pr关掉我改回去*/
			};

			var result = await messageBox.ShowDialogAsync();

			if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
			{
				Desktop.Views.Pages.DataPage.Timer_DataPage?.Dispose();
				Environment.Exit(-114514);
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
