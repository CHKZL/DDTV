using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Desktop.Views.Windows
{
    /// <summary>
    /// 单条通知胶囊：负责自身的入场（弹簧展开）、停留、到点急收折叠，以及两种主动消失方式——
    /// 点击快速淡出（140ms）、队列溢出时缓慢渐隐+轻微缩小（500ms）。
    /// 生命周期结束统一抛RemoveRequested，由宿主窗口负责高度收缩与队列移除。
    /// </summary>
    public partial class HudCapsule : System.Windows.Controls.UserControl
    {
        private static readonly KeySpline Spring = new(0.3, 1.25, 0.45, 1);  // 入场轻回弹
        private static readonly KeySpline Decel = new(0.05, 0.7, 0.1, 1);    // 快出慢停（滑入/升起）
        private static readonly KeySpline Accel = new(0.6, 0, 0.85, 0.3);    // 慢起急收（折叠）

        private static readonly TimeSpan DismissFade = TimeSpan.FromMilliseconds(140);
        private static readonly TimeSpan OverflowFade = TimeSpan.FromMilliseconds(500);

        /// <summary>生命周期结束（到点折叠完 / 被点击 / 溢出渐隐完），请求宿主移除本胶囊。</summary>
        public event EventHandler? RemoveRequested;

        /// <summary>是否已在移除流程中（宿主统计队列上限时据此跳过）。</summary>
        public bool IsRemoving => _removeRequested;

        private bool _removeRequested;
        private DispatcherTimer? _holdTimer;
        private Storyboard? _storyboard;

        public HudCapsule()
        {
            InitializeComponent();
        }

        /// <summary>填充内容与主色。</summary>
        public void Configure(string tag, string headline, string detail, Color accent, string iconData)
        {
            TagText.Text = tag;
            HeadlineText.Text = headline;
            DetailText.Text = detail;
            DetailText.Visibility = detail.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            GlyphPath.Data = Geometry.Parse(iconData);
            GlyphPath.Stroke = new SolidColorBrush(accent);
            GlyphTile.Background = new SolidColorBrush(Color.FromArgb(0x2B, accent.R, accent.G, accent.B));
        }

        /// <summary>播放入场动画，完成后停留hold时长，然后急收折叠并请求移除。</summary>
        public void BeginLifecycle(TimeSpan hold)
        {
            var entrance = BuildEntrance();
            entrance.Completed += (_, _) =>
            {
                _holdTimer = new DispatcherTimer { Interval = hold };
                _holdTimer.Tick += (_, _) =>
                {
                    _holdTimer?.Stop();
                    Fold();
                };
                _holdTimer.Start();
            };
            _storyboard = entrance;
            entrance.Begin(this, true);
        }

        /// <summary>点击胶囊：快速淡出后请求移除。</summary>
        private void Capsule_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DismissQuick();
        }

        /// <summary>快速淡出（140ms），用于点击提前收起。</summary>
        public void DismissQuick()
        {
            if (_removeRequested)
            {
                return;
            }
            double current = Capsule.Opacity;
            StopAll();

            var fade = new DoubleAnimation(current, 0, DismissFade);
            fade.Completed += (_, _) => RequestRemove();
            Capsule.BeginAnimation(OpacityProperty, fade);
        }

        /// <summary>缓慢渐隐+轻微缩小（500ms），用于队列溢出时最旧一条让位。</summary>
        public void DismissGentle()
        {
            if (_removeRequested)
            {
                return;
            }
            StopAll();

            var fade = new DoubleAnimation(Capsule.Opacity, 0, OverflowFade)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
            };
            fade.Completed += (_, _) => RequestRemove();
            Capsule.BeginAnimation(OpacityProperty, fade);

            if (Capsule.RenderTransform is ScaleTransform scale)
            {
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(scale.ScaleX, 0.92, OverflowFade));
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(scale.ScaleY, 0.92, OverflowFade));
            }
        }

        // ---------------- 动画 ----------------

        /// <summary>到点急收折叠（0.46s），从当前实际值起跳，折叠完请求移除。</summary>
        private void Fold()
        {
            var fold = new Storyboard();
            double opacity = Capsule.Opacity;
            double scaleX = Capsule.RenderTransform is ScaleTransform st ? st.ScaleX : 1;

            fold.Children.Add(Anim(nameof(Capsule), "Opacity", K(0, opacity), K(0.46, 0, Accel)));
            fold.Children.Add(Anim(nameof(Capsule), "(UIElement.RenderTransform).(ScaleTransform.ScaleX)", K(0, scaleX), K(0.46, 0.92, Accel)));
            fold.Children.Add(Anim(nameof(Capsule), "(UIElement.RenderTransform).(ScaleTransform.ScaleY)", K(0, scaleX), K(0.46, 0.92, Accel)));
            fold.Completed += (_, _) => RequestRemove();
            _storyboard = fold;
            fold.Begin(this, true);
        }

        /// <summary>入场：胶囊回弹展开 → 图标滑入 → 标题升起 → 明细浮现（共约1.2s，关键帧沿用EndfieldHud编排）。</summary>
        private Storyboard BuildEntrance()
        {
            var entrance = new Storyboard();

            entrance.Children.Add(Anim(nameof(Capsule), "Opacity",
                K(0, 0), K(0.46, 1, Spring)));
            entrance.Children.Add(Anim(nameof(Capsule), "(UIElement.RenderTransform).(ScaleTransform.ScaleX)",
                K(0, 0.85), K(0.46, 1, Spring)));
            entrance.Children.Add(Anim(nameof(Capsule), "(UIElement.RenderTransform).(ScaleTransform.ScaleY)",
                K(0, 0.85), K(0.46, 1, Spring)));

            entrance.Children.Add(Anim(nameof(GlyphTile), "Opacity",
                K(0, 0), K(0.138, 0), K(0.736, 1, Decel)));
            entrance.Children.Add(Anim(nameof(GlyphTile), "(UIElement.RenderTransform).(TranslateTransform.X)",
                K(0, -18), K(0.138, -18), K(0.736, 0, Decel)));

            entrance.Children.Add(Anim(nameof(HeadlineGroup), "Opacity",
                K(0, 0), K(0.276, 0), K(0.92, 1, Decel)));
            entrance.Children.Add(Anim(nameof(HeadlineGroup), "(UIElement.RenderTransform).(TranslateTransform.Y)",
                K(0, 10), K(0.276, 10), K(0.92, 0, Decel)));

            entrance.Children.Add(Anim(nameof(DetailText), "Opacity",
                K(0, 0), K(0.46, 0), K(1.196, 0.65, Decel)));

            return entrance;
        }

        private static DoubleAnimationUsingKeyFrames Anim(string targetName, string property, params DoubleKeyFrame[] keys)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTargetName(animation, targetName);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            foreach (var key in keys)
            {
                animation.KeyFrames.Add(key);
            }
            return animation;
        }

        private static DoubleKeyFrame K(double seconds, double value, KeySpline? spline = null)
        {
            var keyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(seconds));
            return spline != null
                ? new SplineDoubleKeyFrame(value, keyTime, spline)
                : new LinearDoubleKeyFrame(value, keyTime);
        }

        // ---------------- 生命周期收尾 ----------------

        private void RequestRemove()
        {
            if (_removeRequested)
            {
                return;
            }
            _removeRequested = true;
            StopAll();
            RemoveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void StopAll()
        {
            _holdTimer?.Stop();
            _holdTimer = null;
            if (_storyboard != null)
            {
                _storyboard.Stop(this);
                _storyboard = null;
            }
        }
    }
}
