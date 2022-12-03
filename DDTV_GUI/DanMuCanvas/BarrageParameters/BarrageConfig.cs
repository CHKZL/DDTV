using DDTV_GUI.DanMuCanvas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DDTV_GUI.DanMuCanvas.BarrageParameters
{
    public class BarrageConfig
    {
        #region 相关数据
        /// <summary>
        /// 每次循环时字幕的显示（减小lengthList）的速度
        /// </summary>
        public decimal reduceSpeed;
        /// <summary>
        /// 单个弹幕的结束时间
        /// </summary>
        public double initFinishTime;
        /// <summary>
        /// 弹幕当前高度
        /// </summary>
        public int height=0;
        #endregion

        #region 运行时
        private Canvas canvas;
        #endregion

        #region 初始化
        public BarrageConfig(Canvas canvas)
        {
            //InitializeColors();
            this.canvas = canvas;
            reduceSpeed = decimal.Parse("0.5");
            initFinishTime = double.Parse("10");
        }

        #endregion

        #region 运行时

        /// <summary>
        /// 在Window界面上显示弹幕信息,速度和位置随机产生
        /// </summary>
        /// <param name="contentlist"></param>
        public void Barrage(MessageInformation contentlist,int Height,bool IsSubtitle=false)
        {
            Random random = new Random();
            //当前读取弹幕的位置
            //获取高度位置(置顶、三分之一、三分之二)
            int Max = (int)(Height - DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuFontSize);
            double inittop = new Random().Next(0, Max);// GetHeight(locations[i]);
                                                       //获取速度随机数
                                                       //double randomspeed = random.NextDouble();
                                                       //设置完成动画的时间
                                                       //实例化TextBlock和设置基本属性,并添加到Canvas中

            inittop = height;
            height += DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuFontSize;
            if(height>Max)
            {
                height = 0;
            }
            TextBlock textblock = new TextBlock();
            
            //加上昵称显示
            if (!string.IsNullOrEmpty(contentlist.nickName))
            {
                textblock.Text = $"{contentlist.nickName}:{contentlist.content}";
            }
            else
            {
                textblock.Text = contentlist.content;
            }
            textblock.FontSize = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuFontSize;
            textblock.FontWeight = System.Windows.FontWeights.Bold;
            if (IsSubtitle)
            {

                byte R = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SubtitleColor.Split(',')[0], 16);
                byte G = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SubtitleColor.Split(',')[1], 16);
                byte B = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.SubtitleColor.Split(',')[2], 16);
                textblock.Foreground = new SolidColorBrush(Color.FromRgb(R, G, B));
            }
            else
            {
                byte R = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuColor.Split(',')[0], 16);
                byte G = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuColor.Split(',')[1], 16);
                byte B = Convert.ToByte(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuColor.Split(',')[2], 16);
                textblock.Foreground = new SolidColorBrush(Color.FromRgb(R,G,B)); 
            }
            
            //这里设置了弹幕的高度
            Canvas.SetTop(textblock, inittop);
            canvas.Children.Add(textblock);
            //实例化动画
            DoubleAnimation animation = new DoubleAnimation();
            Timeline.SetDesiredFrameRate(animation, 60);  //如果有性能问题,这里可以设置帧数
                                                          //从右往左
            animation.From = canvas.ActualWidth;
            animation.To = 0- (DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuFontSize * textblock.Text.Length);
            animation.Duration = TimeSpan.FromSeconds(initFinishTime);
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
