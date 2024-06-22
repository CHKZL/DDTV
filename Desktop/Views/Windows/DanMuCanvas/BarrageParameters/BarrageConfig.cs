using Core;
using Desktop.Views.Windows.DanMuCanvas.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;

namespace Desktop.Views.Windows.DanMuCanvas.BarrageParameters
{
    public class BarrageConfig
    {
        #region 相关数据
        /// <summary>
        /// 每次循环时字幕的显示（减小lengthList）的速度
        /// </summary>
        public decimal reduceSpeed;
        /// <summary>
        /// 弹幕当前高度
        /// </summary>
        public int height = 0;
        /// <summary>
        /// 当前窗口宽度，用于计算弹幕速度
        /// </summary>
        public double _width = 0;
        #endregion

        #region 运行时
        private Canvas canvas;
        #endregion

        #region 初始化
        public BarrageConfig(Canvas canvas,double width)
        {
            //InitializeColors();
            this.canvas = canvas;
            reduceSpeed = decimal.Parse("0.5");
            _width = width;
        }

        #endregion

        #region 运行时

        public void Barrage_Stroke(MessageInformation contentlist, int Index, bool IsSubtitle = false)
        {
            height = Index * Config.Core_RunConfig._PlayWindowDanmaFontSize;
            Grid grid = new Grid();
            grid.Resources.Add("Stroke", new SolidColorBrush(Colors.Black));
            for (int i = 0; i < 4; i++)
            {
                TextBlock strokeTextBlock = new TextBlock();
                if (File.Exists("./typeface.ttf"))
                {
                    strokeTextBlock.FontFamily = new FontFamily(new Uri("file:///" + System.IO.Path.GetFullPath("./")), "./#typeface");
                }
                strokeTextBlock.Margin = new Thickness(i == 0 ? -2 : 0, i == 1 ? -2 : 0, i == 2 ? -2 : 0, i == 3 ? -2 : 0);
                strokeTextBlock.Text = !string.IsNullOrEmpty(contentlist.nickName) ? $"{contentlist.nickName}:{contentlist.content}" : contentlist.content;
                strokeTextBlock.FontSize = Config.Core_RunConfig._PlayWindowDanmaFontSize;
                strokeTextBlock.FontWeight = System.Windows.FontWeights.Bold;
                strokeTextBlock.Foreground = (Brush)grid.Resources["Stroke"];
                grid.Children.Add(strokeTextBlock);
            }
            TextBlock textblock = new TextBlock();
            if(File.Exists("./typeface.ttf"))
            {
                textblock.FontFamily = new FontFamily(new Uri("file:///" + System.IO.Path.GetFullPath("./")), "./#typeface");
            }
            
            if (!string.IsNullOrEmpty(contentlist.nickName))
            {
                textblock.Text = $"{contentlist.nickName}:{contentlist.content}";
            }
            else
            {
                textblock.Text = contentlist.content;
            }
            textblock.FontSize = Config.Core_RunConfig._PlayWindowDanmaFontSize;
            textblock.FontWeight = System.Windows.FontWeights.Bold;
            if (IsSubtitle)
            {
                byte R = Convert.ToByte(Config.Core_RunConfig._PlayWindowSubtitleColor.Split(',')[0], 16);
                byte G = Convert.ToByte(Config.Core_RunConfig._PlayWindowSubtitleColor.Split(',')[1], 16);
                byte B = Convert.ToByte(Config.Core_RunConfig._PlayWindowSubtitleColor.Split(',')[2], 16);
                textblock.Foreground = new SolidColorBrush(Color.FromRgb(R, G, B));
            }
            else
            {
                byte R = Convert.ToByte(Config.Core_RunConfig._PlayWindowDanmaColor.Split(',')[0], 16);
                byte G = Convert.ToByte(Config.Core_RunConfig._PlayWindowDanmaColor.Split(',')[1], 16);
                byte B = Convert.ToByte(Config.Core_RunConfig._PlayWindowDanmaColor.Split(',')[2], 16);
                textblock.Foreground = new SolidColorBrush(Color.FromRgb(R, G, B));
            }

            grid.Children.Add(textblock);

            //这里设置了弹幕的高度
            Canvas.SetTop(grid, height);
            canvas.Children.Add(grid);

            //实例化动画
            DoubleAnimation animation = new DoubleAnimation();
            Timeline.SetDesiredFrameRate(animation, 60); //如果有性能问题,这里可以设置帧数
                                                         //从右往左
            animation.From = canvas.ActualWidth;
            animation.To = 0 - (Config.Core_RunConfig._PlayWindowDanmaFontSize * textblock.Text.Length);
            if (Config.Core_RunConfig._PlayDanmaSpeed_Dynamically)
            {
                animation.Duration = TimeSpan.FromSeconds(Config.Core_RunConfig._PlayWindowDanmaSpeed);
            }
            else
            {
                animation.Duration = TimeSpan.FromSeconds(_width / 800 * Config.Core_RunConfig._PlayWindowDanmaSpeed);
            }
            animation.AutoReverse = false;
            animation.Completed += (object sender, EventArgs e) =>
            {
                canvas.Children.Remove(grid);
            };

            //启动动画
            grid.BeginAnimation(Canvas.LeftProperty, animation);
        }



        /// <summary>
        /// 在Window界面上显示弹幕信息,速度和位置随机产生
        /// </summary>
        /// <param name="contentlist"></param>
        public void Barrage(MessageInformation contentlist, int Index, bool IsSubtitle = false)
        {
            height = Index * Config.Core_RunConfig._PlayWindowDanmaFontSize;
            TextBlock textblock = new TextBlock();
            List<TextBlock> Stroke = new List<TextBlock>();
            Stroke.Add(new TextBlock());
            Stroke.Add(new TextBlock());
            Stroke.Add(new TextBlock());
            Stroke.Add(new TextBlock());

            //加上昵称显示
            if (!string.IsNullOrEmpty(contentlist.nickName))
            {
                textblock.Text = $"{contentlist.nickName}:{contentlist.content}";
                foreach (var item in Stroke)
                {
                    item.Text = $"{contentlist.nickName}:{contentlist.content}";
                }
            }
            else
            {
                textblock.Text = contentlist.content;
                foreach (var item in Stroke)
                {
                    item.Text = $"{contentlist.nickName}:{contentlist.content}";
                }
            }
            textblock.FontSize = Config.Core_RunConfig._PlayWindowDanmaFontSize;
            textblock.FontWeight = System.Windows.FontWeights.Bold;
            foreach (var item in Stroke)
            {
                item.FontSize = Config.Core_RunConfig._PlayWindowDanmaFontSize;
                item.FontWeight = System.Windows.FontWeights.Bold;
                textblock.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
            if (IsSubtitle)
            {

                byte R = Convert.ToByte(Config.Core_RunConfig._PlayWindowSubtitleColor.Split(',')[0], 16);
                byte G = Convert.ToByte(Config.Core_RunConfig._PlayWindowSubtitleColor.Split(',')[1], 16);
                byte B = Convert.ToByte(Config.Core_RunConfig._PlayWindowSubtitleColor.Split(',')[2], 16);
                textblock.Foreground = new SolidColorBrush(Color.FromRgb(R, G, B));
            }
            else
            {
                byte R = Convert.ToByte(Config.Core_RunConfig._PlayWindowDanmaColor.Split(',')[0], 16);
                byte G = Convert.ToByte(Config.Core_RunConfig._PlayWindowDanmaColor.Split(',')[1], 16);
                byte B = Convert.ToByte(Config.Core_RunConfig._PlayWindowDanmaColor.Split(',')[2], 16);
                textblock.Foreground = new SolidColorBrush(Color.FromRgb(R, G, B));
            }

            //这里设置了弹幕的高度
            Canvas.SetTop(textblock, height);
            canvas.Children.Add(textblock);
            //实例化动画
            DoubleAnimation animation = new DoubleAnimation();
            Timeline.SetDesiredFrameRate(animation, 60);  //如果有性能问题,这里可以设置帧数
                                                          //从右往左
            animation.From = canvas.ActualWidth;
            animation.To = 0 - (Config.Core_RunConfig._PlayWindowDanmaFontSize * textblock.Text.Length);
            if (Config.Core_RunConfig._PlayDanmaSpeed_Dynamically)
            {
                animation.Duration = TimeSpan.FromSeconds(Config.Core_RunConfig._PlayWindowDanmaSpeed);
            }
            else
            {
                animation.Duration = TimeSpan.FromSeconds(_width / 800 * Config.Core_RunConfig._PlayWindowDanmaSpeed);
            }
            animation.AutoReverse = false;
            animation.Completed += (object sender, EventArgs e) =>
            {
                canvas.Children.Remove(textblock);
            };
            //启动动画
            textblock.BeginAnimation(Canvas.LeftProperty, animation);
        }


        #endregion
    }
}
