using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Desktop.Views.Windows
{
    /// <summary>
    /// 通用系统通知HUD：主屏顶部居中的深色胶囊（移植自EndfieldHud的视觉与动画编排），
    /// 弹簧入场 → 图标滑入 → 标题升起 → 明细浮现 → 停留 → 急收折叠，点击胶囊提前收起。
    /// 连续触发时不重建窗口，就地复位后重播整条时间线。
    /// 配色与图标按级别区分：Info=白 / Notice=琥珀 / Success=荧光黄绿 / Alert=红。
    /// 开关与展示时长由设置页配置（_SystemCardReminder / _HudNotificationDuration）控制。
    /// </summary>
    public partial class HudNotification : Window
    {
        /// <summary>通知级别，决定主色与默认图标。</summary>
        public enum HudLevel
        {
            Info,
            Notice,
            Success,
            Alert,
        }

        private static readonly TimeSpan DismissFade = TimeSpan.FromMilliseconds(140);

        private static readonly KeySpline Spring = new(0.3, 1.25, 0.45, 1);  // 入场轻回弹
        private static readonly KeySpline Decel = new(0.05, 0.7, 0.1, 1);    // 快出慢停（滑入/升起）
        private static readonly KeySpline Accel = new(0.6, 0, 0.85, 0.3);    // 慢起急收（折叠）

        // 级别主色（EndfieldHud调色板）
        private static readonly Color InfoColor = Color.FromRgb(0xF5, 0xF7, 0xFA);
        private static readonly Color NoticeColor = Color.FromRgb(0xFF, 0xB0, 0x20);
        private static readonly Color SuccessColor = Color.FromRgb(0xD8, 0xF3, 0x4E);
        private static readonly Color AlertColor = Color.FromRgb(0xFF, 0x52, 0x57);

        // 描边图标（24×24视口，Stroke线宽2，圆头）
        private const string LiveIcon = "M12 12 m-1.6 0 a1.6 1.6 0 1 0 3.2 0 a1.6 1.6 0 1 0 -3.2 0 M8.82 8.82 A4.5 4.5 0 0 0 8.82 15.18 M15.18 8.82 A4.5 4.5 0 0 1 15.18 15.18 M6.34 6.34 A8 8 0 0 0 6.34 17.66 M17.66 6.34 A8 8 0 0 1 17.66 17.66";
        private const string InfoIcon = "M12 12 m-8 0 a8 8 0 1 0 16 0 a8 8 0 1 0 -16 0 M12 11 L12 16 M12 7.8 L12 8.2";
        private const string NoticeIcon = "M12 4.5 L20.5 19.5 L3.5 19.5 Z M12 10 L12 14 M12 16.4 L12 16.9";
        private const string SuccessIcon = "M5.5 12.5 L10 17 L18.5 7";
        private const string AlertIcon = "M12 12 m-8 0 a8 8 0 1 0 16 0 a8 8 0 1 0 -16 0 M12 7.5 L12 13 M12 15.9 L12 16.4";

        // 动画固定段时长（秒）：全部元素就位 = 1.196s，折叠 = 0.46s；停留段按配置伸缩
        private const double EntranceEnd = 1.196;
        private const double Fold = 0.46;

        private static HudNotification? _instance;
        private Storyboard? _timeline;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private HudNotification()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 弹出通知胶囊。可从任意线程调用，内部自行切到UI线程。
        /// </summary>
        /// <param name="tag">左上角小标签，如 "/// LIVE ALERT"</param>
        /// <param name="headline">主标题</param>
        /// <param name="detail">右侧明细小字（为空则不显示）</param>
        /// <param name="level">级别，决定主色与默认图标</param>
        /// <param name="iconData">自定义描边图标路径数据，为空时按级别取默认图标</param>
        /// <param name="force">为true时忽略设置页的开关强制弹出（测试按钮用）</param>
        public static void Notify(string tag, string headline, string detail = "", HudLevel level = HudLevel.Info, string? iconData = null, bool force = false)
        {
            if (!force && !Core.Config.Core_RunConfig._SystemCardReminder)
            {
                return;
            }
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }
            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => Notify(tag, headline, detail, level, iconData, force)));
                return;
            }
            _instance ??= new HudNotification();
            _instance.Play(tag, headline, detail, level, iconData);
        }

        /// <summary>开播提醒：琥珀主色 + 开播信号图标，明细回退为房间号。</summary>
        public static void NotifyLiveReminder(string name, string title, long roomId, bool force = false)
        {
            string detail = string.IsNullOrWhiteSpace(title) ? $"房间号 {roomId}" : title;
            Notify("/// LIVE ALERT", $"【{name}】的直播开始啦", detail, HudLevel.Notice, LiveIcon, force);
        }

        // ---------------- 播放入口 ----------------

        private void Play(string tag, string headline, string detail, HudLevel level, string? iconData)
        {
            StopTimeline();
            ResetVisual();

            Color accent = AccentFor(level);
            TagText.Text = tag;
            HeadlineText.Text = headline;
            DetailText.Text = detail;
            DetailText.Visibility = detail.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            GlyphPath.Data = Geometry.Parse(iconData ?? IconFor(level));
            GlyphPath.Stroke = new SolidColorBrush(accent);
            GlyphTile.Background = new SolidColorBrush(Color.FromArgb(0x2B, accent.R, accent.G, accent.B));

            PlaceTopCenter();
            if (!IsVisible)
            {
                Show();
            }

            double totalSeconds = Math.Clamp(Core.Config.Core_RunConfig._HudNotificationDuration, 1, 30);
            _timeline = BuildTimeline(totalSeconds);
            _timeline.Completed += Timeline_Completed;
            _timeline.Begin(this, true);
        }

        private static Color AccentFor(HudLevel level) => level switch
        {
            HudLevel.Notice => NoticeColor,
            HudLevel.Success => SuccessColor,
            HudLevel.Alert => AlertColor,
            _ => InfoColor,
        };

        private static string IconFor(HudLevel level) => level switch
        {
            HudLevel.Notice => NoticeIcon,
            HudLevel.Success => SuccessIcon,
            HudLevel.Alert => AlertIcon,
            _ => InfoIcon,
        };

        private void Timeline_Completed(object? sender, EventArgs e)
        {
            StopTimeline();
            Hide();
        }

        private void StopTimeline()
        {
            if (_timeline != null)
            {
                _timeline.Completed -= Timeline_Completed;
                _timeline.Stop(this);
                _timeline = null;
            }
        }

        /// <summary>点击胶囊提前收起（保留当前胶囊位置淡出）。</summary>
        private void Capsule_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double current = Capsule.Opacity;
            StopTimeline();

            var fade = new DoubleAnimation(current, 0, DismissFade);
            fade.Completed += (_, _) =>
            {
                Capsule.BeginAnimation(OpacityProperty, null);
                Hide();
            };
            Capsule.BeginAnimation(OpacityProperty, fade);
        }

        // ---------------- 动画时间线（WPF移植版HudChoreographer） ----------------

        /// <summary>
        /// 整条时间线：入场/折叠段时长固定，停留段 = 配置总时长 - 固定段，随配置伸缩。
        /// 关键帧时间点沿用EndfieldHud的cue × 4.6s换算结果，多关键帧下每段必须显式KeySpline。
        /// </summary>
        private Storyboard BuildTimeline(double totalSeconds)
        {
            double holdEnd = EntranceEnd + Math.Max(0, totalSeconds - EntranceEnd - Fold); // 停留结束
            double close = holdEnd + Fold;   // 折叠完成

            var storyboard = new Storyboard();

            // 胶囊：0.85缩放回弹到1，停留后急收到0.92并淡出
            storyboard.Children.Add(Anim(nameof(Capsule), "Opacity",
                K(0, 0), K(0.46, 1, Spring), K(holdEnd, 1), K(close, 0, Accel)));
            storyboard.Children.Add(Anim(nameof(Capsule), "(UIElement.RenderTransform).(ScaleTransform.ScaleX)",
                K(0, 0.85), K(0.46, 1, Spring), K(holdEnd, 1), K(close, 0.92, Accel)));
            storyboard.Children.Add(Anim(nameof(Capsule), "(UIElement.RenderTransform).(ScaleTransform.ScaleY)",
                K(0, 0.85), K(0.46, 1, Spring), K(holdEnd, 1), K(close, 0.92, Accel)));

            // 图标块：从左滑入
            storyboard.Children.Add(Anim(nameof(GlyphTile), "Opacity",
                K(0, 0), K(0.138, 0), K(0.736, 1, Decel), K(holdEnd, 1), K(close, 0, Accel)));
            storyboard.Children.Add(Anim(nameof(GlyphTile), "(UIElement.RenderTransform).(TranslateTransform.X)",
                K(0, -18), K(0.138, -18), K(0.736, 0, Decel), K(holdEnd, 0), K(close, 0, Accel)));

            // 标题组：升起
            storyboard.Children.Add(Anim(nameof(HeadlineGroup), "Opacity",
                K(0, 0), K(0.276, 0), K(0.92, 1, Decel), K(holdEnd, 1), K(close, 0, Accel)));
            storyboard.Children.Add(Anim(nameof(HeadlineGroup), "(UIElement.RenderTransform).(TranslateTransform.Y)",
                K(0, 10), K(0.276, 10), K(0.92, 0, Decel), K(holdEnd, 0), K(close, 0, Accel)));

            // 明细：淡入到0.65
            storyboard.Children.Add(Anim(nameof(DetailText), "Opacity",
                K(0, 0), K(0.46, 0), K(EntranceEnd, 0.65, Decel), K(holdEnd, 0.65), K(close, 0, Accel)));

            return storyboard;
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

        // ---------------- 复位与定位 ----------------

        /// <summary>所有元素回到入场前状态，保证重复播报时画面干净。</summary>
        private void ResetVisual()
        {
            Capsule.Opacity = 0;
            Capsule.RenderTransform = new ScaleTransform(0.85, 0.85);

            GlyphTile.Opacity = 0;
            GlyphTile.RenderTransform = new TranslateTransform(-18, 0);

            HeadlineGroup.Opacity = 0;
            HeadlineGroup.RenderTransform = new TranslateTransform(0, 10);

            DetailText.Opacity = 0;
        }

        /// <summary>主显示器工作区顶部居中（顶部留8px呼吸位，与原版一致）。</summary>
        private void PlaceTopCenter()
        {
            var area = SystemParameters.WorkArea;
            Left = area.Left + (area.Width - Width) / 2;
            Top = area.Top + 8;
        }

        /// <summary>设置WS_EX_NOACTIVATE，弹出时不抢占当前焦点。</summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            if (IntPtr.Size == 8)
            {
                long exStyle = GetWindowLongPtr64(hwnd, GWL_EXSTYLE).ToInt64();
                SetWindowLongPtr64(hwnd, GWL_EXSTYLE, new IntPtr(exStyle | WS_EX_NOACTIVATE));
            }
            else
            {
                int exStyle = GetWindowLong32(hwnd, GWL_EXSTYLE);
                SetWindowLong32(hwnd, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
            }
        }
    }
}
