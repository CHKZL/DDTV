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

namespace DDTV_New
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }
        public string 内容 = "";
        public Color 字体颜色 = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        public Color 描边颜色= Colors.Black;

        protected override void OnRender(DrawingContext drawingContext)
        {
         
            var str = 内容;

            var formattedText = new FormattedText(str, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface
                (
                    new FontFamily("微软雅黑"),
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal
                ),
                30,
                Brushes.Black, 5);

            var geometry = formattedText.BuildGeometry(new Point(10, 10));
            if(drawingContext==null)
            {
                MessageBox.Show("UserControl绘制drawingContext出错！");
                return;
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
