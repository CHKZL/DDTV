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
        int _fontSize = 26;
        double _Opacity = 1;
        int _Speed = 1;
        public DanMuConfig(EventHandler<EventArgs> _)
        {
            InitializeComponent();
            eventHandler = _;
            FontSizeSlider.Value= GUIConfig.DanMuFontSize;
            FontOpacitySlider.Value = GUIConfig.DanMuFontOpacity;
            SpeedSlider.Value = GUIConfig.DanMuSpeed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GUIConfig.DanMuFontSize = _fontSize;
            GUIConfig.DanMuFontOpacity = _Opacity;
            GUIConfig.DanMuSpeed = _Speed;

            CoreConfig.SetValue(CoreConfigClass.Key.DanMuFontSize, _fontSize.ToString(), CoreConfigClass.Group.Play);
            CoreConfig.SetValue(CoreConfigClass.Key.DanMuFontOpacity, _Opacity.ToString(), CoreConfigClass.Group.Play);
            CoreConfig.SetValue(CoreConfigClass.Key.DanMuSpeed, _Speed.ToString(), CoreConfigClass.Group.Play);
            GUIConfig.DanMuSpeed = _Speed;
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
            if ((int)e.NewValue > 0)
            {
                _fontSize = (int)e.NewValue;
                FontSizeText.Text = "字体大小：" + _fontSize;
                if(PreviewText!=null)
                {
                    PreviewText.FontSize = _fontSize;
                }
              
            }
        }

        private void FontOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (double.Parse(String.Format("{0:F}", e.NewValue)) > 0.0)
            {
                _Opacity = double.Parse(String.Format("{0:F}", e.NewValue));
                FontOpacityText.Text = "透明度：" + _Opacity;
                if (PreviewText != null)
                {
                    PreviewText.Opacity = _Opacity;
                }
               
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((int)e.NewValue > 0)
            {
                _Speed = (int)e.NewValue;
                SpeedText.Text = "速度：" + _Speed;
            }
            else
            {
                _Speed = 1;
                SpeedText.Text = "速度：" + _Speed;
            }
        }
    }
}
