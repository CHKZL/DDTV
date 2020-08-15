using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace PlayW
{
    /// <summary>
    /// 字幕描边控件.xaml 的交互逻辑
    /// </summary>
    public partial class 字幕描边控件 : UserControl
    {
        public 字幕描边控件()
        {
            InitializeComponent();
        }
        public string 内容 = "弹幕/字幕显示功能已启动";
        public Color 字体颜色 = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        public Color 描边颜色 = Colors.Black;
        public int 字体大小 = 12;
        public bool 是否居中 = false;
        public int 字幕位置 = 180;
        protected override void OnRender(DrawingContext drawingContext)
        {
            var str = 内容;

            var formattedText = new FormattedText(str, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface
                (
                    new FontFamily("微软雅黑"),
                    FontStyles.Normal,
                    FontWeights.Bold,
                    FontStretches.Normal
                ),
                字体大小,
                Brushes.Black, 5);


            var geometry = formattedText.BuildGeometry(new Point(10, 1));
            if (是否居中)
            {
              
                geometry = formattedText.BuildGeometry(new Point(字幕位置-50, 1)); 
            }
            

            drawingContext.DrawGeometry
            (
                new SolidColorBrush(字体颜色),
                new Pen(new SolidColorBrush(描边颜色), 1),
                geometry
            );

            base.OnRender(drawingContext);
        }
    }
}
