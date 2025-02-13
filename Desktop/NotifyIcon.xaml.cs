using Desktop.Views.Pages;
using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Pages;
using Desktop.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Desktop
{
	public partial class NotifyIcon 
	{
		public NotifyIcon()
		{
			InitializeComponent();
			initMenu();
		}
		private void initMenu()
		{
			exitMenu.Click += RightClickExit;
			forceShowMenu.Click += rightClickForceShow;
			 
		}
		private void RightClickExit(object sender, RoutedEventArgs e)
		{
			if (!MainWindow.IsProgrammaticClose)
			{
				System.Windows.MessageBoxResult result = MessageBox.Show("确认要关闭DDTV吗？\r\n关闭后所有录制任务以及播放窗口均会结束。", "关闭确认", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
				if (result == System.Windows.MessageBoxResult.Yes)
				{
					DataPage.Timer_DataPage?.Dispose();
					DataSource.LoginStatus.Timer_LoginStatus?.Dispose();
					Environment.Exit(-114514);
				}
			}
		}

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
		private void NotifyIcon_Click(object? sender, EventArgs e)
		{
			MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
			mainWindow.Show();  
			mainWindow.WindowState = WindowState.Normal; 
		}
	}
	
}
