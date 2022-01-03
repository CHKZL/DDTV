using HandyControl.Controls;
using HandyControl.Interactivity;
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
    /// SendDanmuDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SendDanmuDialog
    {
        int RoomId = 0;
        EventHandler<EventArgs> eventHandler;
        public SendDanmuDialog(int roomId, EventHandler<EventArgs> _)
        {
            RoomId = roomId;
            eventHandler = _;
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Massage = MassageInputBox.Text;
            if(Massage.Length>20)
            {
                Growl.Warning("发送的弹幕长度尝过限制(20个字符)");
                return;
            }
            else if(Massage.Length<1)
            {
                Growl.Warning("发送的弹幕内容不能为空");
                return;
            }
            DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu.DanMu.Send(RoomId.ToString(), Massage);
            eventHandler.Invoke(this, EventArgs.Empty);
            //DDTV_Window.SendDanmuDialog.SendDialogDispose.Invoke(null, EventArgs.Empty);
        }
    }
}
