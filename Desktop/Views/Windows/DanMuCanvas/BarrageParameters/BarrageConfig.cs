using Core;
using Desktop.Views.Windows.DanMuCanvas.Models;
using System.IO;
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
        /// <summary>
        /// 字体缓存（避免每条弹幕都访问磁盘和重复解析字体文件）
        /// </summary>
        private static FontFamily _cachedTypeface = null;
        private static DateTime _typefaceCheckTime = DateTime.MinValue;
        /// <summary>
        /// 获取自定义字体，检查结果缓存5秒
        /// </summary>
        private static FontFamily GetTypefaceFontFamily()
        {
            if ((DateTime.Now - _typefaceCheckTime).TotalSeconds > 5)
            {
                _typefaceCheckTime = DateTime.Now;
                _cachedTypeface = File.Exists("./typeface.ttf")
                    ? new FontFamily(new Uri("file:///" + System.IO.Path.GetFullPath("./")), "./#typeface")
                    : null;
            }
            return _cachedTypeface;
        }

        //弹幕画刷缓存：冻结后的画刷可跨元素共享且渲染更快，避免每条弹幕都Split配置字符串+分配新画刷
        //（弹幕渲染在UI线程串行执行，静态缓存无需加锁；配置在设置页修改后下次渲染自动重建）
        private static readonly SolidColorBrush _strokeBrush = CreateFrozenBrush(Colors.Black);
        private static SolidColorBrush _danmaBrush = null;
        private static string _danmaBrushKey = null;
        private static SolidColorBrush _subtitleBrush = null;
        private static string _subtitleBrushKey = null;

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// 按"RR,GG,BB"配置字符串获取缓存的冻结画刷，配置未变化时直接复用
        /// </summary>
        private static SolidColorBrush GetCachedBrush(string colorConfig, ref SolidColorBrush cache, ref string cacheKey)
        {
            if (cache != null && cacheKey == colorConfig)
            {
                return cache;
            }
            Color color;
            try
            {
                //配置格式"RR,GG,BB"（十六进制）；格式损坏时回退白色，而不是让弹幕因异常静默消失
                string[] parts = colorConfig.Split(',');
                if (parts.Length != 3)
                    throw new FormatException($"颜色配置[{colorConfig}]不是三段式");
                color = Color.FromRgb(Convert.ToByte(parts[0], 16), Convert.ToByte(parts[1], 16), Convert.ToByte(parts[2], 16));
            }
            catch (Exception)
            {
                color = Colors.White;
            }
            cache = CreateFrozenBrush(color);
            cacheKey = colorConfig;
            return cache;
        }

        private static SolidColorBrush GetDanmaBrush() =>
            GetCachedBrush(Config.Core_RunConfig._PlayWindowDanmaColor, ref _danmaBrush, ref _danmaBrushKey);

        private static SolidColorBrush GetSubtitleBrush() =>
            GetCachedBrush(Config.Core_RunConfig._PlayWindowSubtitleColor, ref _subtitleBrush, ref _subtitleBrushKey);
        #endregion

        #region 初始化
        public BarrageConfig(Canvas canvas, double width)
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
            FontFamily typeface = GetTypefaceFontFamily();
            string text = !string.IsNullOrEmpty(contentlist.nickName) ? $"{contentlist.nickName}:{contentlist.content}" : contentlist.content;

            //单元素描边弹幕：几何轮廓描边替代原4+1个TextBlock叠加，渲染树节点从5个降到1个
            OutlinedText danma = new OutlinedText
            {
                Text = text,
                TextFontSize = Config.Core_RunConfig._PlayWindowDanmaFontSize,
                TextFontFamily = typeface,
                Fill = IsSubtitle ? GetSubtitleBrush() : GetDanmaBrush(),
                Stroke = _strokeBrush,
                IsHitTestVisible = false
            };
            Canvas.SetTop(danma, height);
            Canvas.SetLeft(danma, 0);
            canvas.Children.Add(danma);

            StartScrollAnimation(danma, text.Length);
        }

        /// <summary>
        /// 弹幕滚动动画：驱动TranslateTransform而非Canvas.Left。
        /// Canvas.Left是布局属性，每帧动画都会让整个Canvas（在VLC窗口里是覆盖视频的透明分层窗口）全量重排+重绘，
        /// 是弹幕一多就卡的主因；RenderTransform动画由渲染线程直接合成，不触发布局，开销几乎为零
        /// </summary>
        private void StartScrollAnimation(FrameworkElement danma, int textLength)
        {
            double startX = canvas.ActualWidth;
            //终点取实际渲染宽度而不是按字号估算，避免等宽假设下长弹幕提前消失或短弹幕空跑
            double endX = 0 - (danma is OutlinedText outlined ? outlined.TextWidth : Config.Core_RunConfig._PlayWindowDanmaFontSize * textLength) - 10;

            TranslateTransform transform = new TranslateTransform(startX, 0);
            danma.RenderTransform = transform;

            DoubleAnimation animation = new DoubleAnimation();
            Timeline.SetDesiredFrameRate(animation, 30); //30fps对滚动弹幕肉眼无感，渲染开销比60fps减半
            animation.From = startX;
            animation.To = endX;
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
                canvas.Children.Remove(danma);
            };

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
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
                textblock.Foreground = GetSubtitleBrush();
            }
            else
            {
                textblock.Foreground = GetDanmaBrush();
            }

            textblock.IsHitTestVisible = false;
            //这里设置了弹幕的高度
            Canvas.SetTop(textblock, height);
            Canvas.SetLeft(textblock, 0);
            canvas.Children.Add(textblock);
            StartScrollAnimation(textblock, textblock.Text.Length);
        }


        #endregion
    }
}
