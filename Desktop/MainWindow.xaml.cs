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

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process process = new();
        public MainWindow()
        {
            InitializeComponent();
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
                    FileName = "./CLI.exe",
                    Arguments = "--Desktop",
                }
            };
            process.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (process != null)
            {
                process.Kill();
                process.Close();
                process = null;
            }
        }
    }
}