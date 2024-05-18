using Core;
using Desktop.Models;
using Desktop.Views.Pages;
using Desktop.Views.Windows;
using Masuit.Tools.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private Process process = null;
        public MainWindow()
        {
            InitializeComponent();

            Thread.Sleep(1000);
            var doki = Core.Tools.DokiDoki.GetDoki();
            this.Title = $"{doki.InitType}|{doki.Ver}|{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion})";
            UI_TitleBar.Title = this.Title;
            Loaded += (_, _) => RootNavigation.Navigate(typeof(DefaultPage));
            DataPage.Timer_DataPage = new Timer(DataPage.Refresher, null, 1000, 1000);

            DataSource.LoginStatus.LoginFailureEvent += LoginStatus_LoginFailureEvent;
            DataSource.LoginStatus.Timer_LoginStatus = new Timer(DataSource.LoginStatus.RefreshLoginStatus, null, 1000, 5000);      
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
                QrLogin qrLogin = new QrLogin();
                qrLogin.ShowDialog();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DataPage.Timer_DataPage?.Dispose();
            DataSource.LoginStatus.Timer_LoginStatus?.Dispose();
            Environment.Exit(-114514);
        }
    }
}
