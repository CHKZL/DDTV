using Core;
using Desktop.Views.Pages;
using System.Diagnostics;
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
//#if !DEBUG
            process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    FileName = "./Server.exe",
                    Arguments = "--StartMode=Desktop",
                }
            };
            process.OutputDataReceived += (sender, args) => Debug.WriteLine($"{args.Data}"); // 打印标准输出
            process.ErrorDataReceived += (sender, args) => Debug.WriteLine($"{args.Data}"); // 打印错误输出
            process.Start();
            process.BeginOutputReadLine(); // 开始异步读取标准输出
            process.BeginErrorReadLine(); // 开始异步读取错误输出
//#endif
            var doki = Core.Tools.DokiDoki.GetDoki();
            this.Title = $"{doki.InitType}|{doki.Ver}|{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion})";
            UI_TitleBar.Title = this.Title;
            Loaded += (_, _) => RootNavigation.Navigate(typeof(DefaultPage));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (process != null)
            {
                try
                {
                    process.Close();
                    process.Kill();
                }
                catch (Exception) { }
                process = null;
            }
        }
    }
}
