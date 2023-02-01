using DDTV_Core.SystemAssembly.ConfigModule;
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
    public partial class ColorPickerWindow
    {
        EventHandler<EventArgs> eventHandler;
        public ColorPickerWindow(EventHandler<EventArgs> _)
        {
            InitializeComponent();
            eventHandler = _;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush solidColorBrush = ColorPicker.SelectedBrush;

            if(DanMuRadio.IsChecked.Value)
            {
                GUIConfig.DanMuColor =
                    "0x" + Convert.ToString(solidColorBrush.Color.R, 16) + "," +
                    "0x" + Convert.ToString(solidColorBrush.Color.G, 16) + "," +
                    "0x" + Convert.ToString(solidColorBrush.Color.B, 16);
                CoreConfig.SetValue(CoreConfigClass.Key.DanmuColor, GUIConfig.DanMuColor, CoreConfigClass.Group.Play);
            }
            else if (SubtitleRadio.IsChecked.Value)
            {
                GUIConfig.SubtitleColor =
                    "0x" + Convert.ToString(solidColorBrush.Color.R, 16) + "," +
                    "0x" + Convert.ToString(solidColorBrush.Color.G, 16) + "," +
                    "0x" + Convert.ToString(solidColorBrush.Color.B, 16);
                CoreConfig.SetValue(CoreConfigClass.Key.SubtitleColor, GUIConfig.SubtitleColor, CoreConfigClass.Group.Play);
            }
            eventHandler.Invoke(this, EventArgs.Empty);
        }
    }
}
