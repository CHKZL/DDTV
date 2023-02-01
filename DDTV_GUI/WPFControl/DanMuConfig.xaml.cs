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
    public partial class DanMuConfig
    {
        public event EventHandler<EventArgs> ColorPickerDialogDispose;
        EventHandler<EventArgs> eventHandler;
        private Dialog ColorPickerDialog;//取色盘弹出窗口
        int fontSize = 26;
        double Opacity = 1;
        public DanMuConfig(EventHandler<EventArgs> _)
        {
            InitializeComponent();
            eventHandler = _;
            FontSizeSlider.Value= GUIConfig.DanMuFontSize;
            FontOpacitySlider.Value = GUIConfig.DanMuFontOpacity;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GUIConfig.DanMuFontSize = fontSize;
            GUIConfig.DanMuFontOpacity = Opacity;

            CoreConfig.SetValue(CoreConfigClass.Key.DanMuFontSize, fontSize.ToString(), CoreConfigClass.Group.Play);
            CoreConfig.SetValue(CoreConfigClass.Key.DanMuFontOpacity, Opacity.ToString(), CoreConfigClass.Group.Play);
            eventHandler.Invoke(this, EventArgs.Empty);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ColorPickerDialogDispose += DanMuConfig_ColorPickerDialogDispose;
            ColorPickerWindow colorPickerWindow = new ColorPickerWindow(ColorPickerDialogDispose);
            ColorPickerDialog = Dialog.Show(colorPickerWindow);
        }

        private void DanMuConfig_ColorPickerDialogDispose(object? sender, EventArgs e)
        {
            ColorPickerDialog.Close();
        }
        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (fontSize > 0)
            {
                fontSize = (int)e.NewValue;
                FontSizeText.Text = "字体大小：" + fontSize;
                if(PreviewText!=null)
                {
                    PreviewText.FontSize = fontSize;
                }
              
            }
        }

        private void FontOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Opacity > 0)
            {
                Opacity = double.Parse(String.Format("{0:F}", e.NewValue));
                FontOpacityText.Text = "透明度：" + Opacity;
                if (PreviewText != null)
                {
                    PreviewText.Opacity = Opacity;
                }
               
            }
        }
    }
}
