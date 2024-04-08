using Core;
using Core.LogModule;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI
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

            //Task.Run(() =>
            //{
            //    CLI.Program.Main([""]);
            //});
            Config.ReadConfiguration();
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
                    Arguments = "--GUI",
                }
            };
            process.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    string A = Core.Network.Get.GetBody($"http://127.0.0.1:{Config.Core._Port}/api/init_inspect", false);
                    OperationQueue.pack<string>? OJ = JsonSerializer.Deserialize<OperationQueue.pack<string>>(A);
                    if (OJ != null && OJ.message.ToLower() == "ok")
                    {
                        this.Dispatcher.Invoke(() =>
                        {

                            WV2.Visibility = Visibility.Visible;
                            starttest.Visibility = Visibility.Collapsed;
                            this.WV2.Source = new Uri($"http://127.0.0.1:{Config.Core._Port}");
                        });
                        return;
                    }
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (process != null)
            {
                //process.Kill();
                //process.Close();
                //process = null;
            }
        }
    }
}