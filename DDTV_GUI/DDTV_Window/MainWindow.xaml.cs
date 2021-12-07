using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : GlowWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SideMenu_SelectionChanged(object sender, HandyControl.Data.FunctionEventArgs<object> e)
        {
            SideMenu sideMenu = (SideMenu)sender;
            int a = sideMenu.Items.IndexOf(e.Info);
            this.MainWindowTab.SelectedIndex = a;
        }


        private void GlowWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Growl.InfoGlobal("测试:蒂蒂媞薇开播了，根据设置开始自动录制");
            Growl.WarningGlobal("测试:储存空间剩余空间不足，请及时清理");
            Growl.ErrorGlobal("测试:储存空间已满，无法正常运行");
            Growl.AskGlobal("测试:蒂蒂媞薇开播了，是否打开直播间？", isConfirmed =>
            {
                if(isConfirmed)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "https://live.bilibili.com/21446992",
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                else
                {
                    Growl.InfoGlobal("好吧，不看就算了");
                }       
                return true;
            });
            Growl.AskGlobal("测试:蒂蒂媞薇开播了，是否打开直播间？", isConfirmed =>
            {
                if (isConfirmed)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "https://live.bilibili.com/21446992",
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                else
                {
                    Growl.InfoGlobal("好吧，不看就算了");
                }
                return true;
            });
        }
    }
}
