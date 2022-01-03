using HandyControl.Controls;
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

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// LogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogWindow : GlowWindow
    {
        public LogWindow()
        {
            InitializeComponent();
            DDTV_Core.SystemAssembly.Log.Log.LogAddEvent += Log_LogAddEvent;
            DDTV_Core.SystemAssembly.Log.Log.IsEvent = true;
        }

        private void Log_LogAddEvent(object? sender, EventArgs e)
        {
            string text = sender as string;
            Log.Dispatcher.Invoke(() => Log.AppendText(text+ "\n\r"));
            

        }

        private void GlowWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DDTV_Core.SystemAssembly.Log.Log.IsEvent = false;
        }
    }
}
