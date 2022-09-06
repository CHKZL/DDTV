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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDTV_GUI.WPFControl
{
    /// <summary>
    /// AddRoomDialog.xaml 的交互逻辑
    /// </summary>
    public partial class OpenDanMuWindowDialog
    {
        EventHandler<EventArgs> eventHandler;
        public OpenDanMuWindowDialog(EventHandler<EventArgs> _)
        {
            InitializeComponent();
            eventHandler = _;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string _UID = UIDInputBox.Text;
            if (string.IsNullOrEmpty(_UID))
            {
                Growl.Warning($"UID不能为空");
                return;
            }
            if (long.TryParse(_UID, out long UID))
            {
                var RoomInfo = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.get_info(0, UID, false);
                if (RoomInfo != null)
                {
                    UID = RoomInfo.uid;
                    //DDTV_DanMu.MainWindow mainWindow = new DDTV_DanMu.MainWindow(UID, false);
                    DDTV_DanMu.MainWindow mainWindow = new DDTV_DanMu.MainWindow();
                    mainWindow.Show();
                    DDTV_Window.MainWindow.DanMuWindow = mainWindow;
                    eventHandler.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Growl.WarningGlobal($"该房间号不存在！");
                    return;
                }

            }
            else
            {
                Growl.WarningGlobal($"房间号不符合规范！");
                return;
            }
        }
    }
}
