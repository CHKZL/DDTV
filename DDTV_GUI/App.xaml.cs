using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace DDTV_GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// 该函数设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWlndow函数的说明部分</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        /// <summary>
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_SHOWNOMAL = 1;
        public static void HandleRunningInstance(Process instance)
        {
            try
            {
                ShowWindowAsync(instance.MainWindowHandle, SW_SHOWNOMAL);//显示
                SetForegroundWindow(instance.MainWindowHandle);//当到最前端
            }
            catch (Exception) { }

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

        public static void Application_Startup()
        {
            Process process = RuningInstance();
            if (process != null)
            {
                MessageBoxResult dr = HandyControl.Controls.MessageBox.Show("已经有DDTV_GUI实例正在运行中" +
                   "\r点击‘是’弹出正在运行的窗口" +
                   "\r点击‘否’强制启动一个新DDTV" +
                   "\r=========\r如果点击‘是’后没有自动弹出请检查程序是否缩小到了系统后台托盘中" +
                   "\r如果后台托盘中也没有应该上次退出后的程序还未关闭" +
                   $"\r======参考信息======" +
                   $"\rId:{process.Id}" +
                   $"\rProcessName:{process.ProcessName}",
                   "已有DDTV实例正在运行", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dr == MessageBoxResult.Yes)
                {
                    HandleRunningInstance(process);
                    System.Threading.Thread.Sleep(1000);
                    System.Environment.Exit(1);
                }
                else
                {

                }
                //MessageBox.Show("已经有DDTV_GUI实例正在运行中" +
                //    "\r点击确定弹出正在运行的窗口" +
                //    "\r如果没有自动弹出请检查程序是否缩小到了系统后台托盘中" +
                //    "\r如果后台托盘中也没有应该上次退出后的程序还未关闭" +
                //    $"\r======参考信息======" +
                //    $"\rId:{process.Id}" +
                //    $"\rProcessName:{process.ProcessName}");
                //HandleRunningInstance(process);
                //System.Threading.Thread.Sleep(1000);
                //System.Environment.Exit(1);
            }
        }
    }
}
