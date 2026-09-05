using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Desktop.Views.Windows
{
    /// <summary>
    /// 通知队列宿主：主屏顶部居中的透明窗口，内部竖向堆叠最多4条HudCapsule（锁屏通知式）。
    /// 新通知从顶部插入并把旧的往下推；超出上限时最旧一条渐隐让位；
    /// 每条胶囊到点/被点击后自行消失，其余顺势上滑。全部消失后窗口隐藏。
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

        // 胶囊槽位高度 = 胶囊64 + 间隔8；窗口高度预留"上限+1"个槽位用于溢出过渡动画
        private const double CapsuleSlot = 72;

        // 胶囊自身动画的固定段时长（秒）：全部元素就位1.196s，到点折叠0.46s；停留段按配置伸缩
        private const double EntranceEnd = 1.196;
        private const double Fold = 0.46;

        // 队列滑动：新胶囊插入时槽位从0撑开（旧的下移），移除时槽位收缩（旧的上滑）
        private static readonly TimeSpan SlotGrow = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan SlotShrink = TimeSpan.FromMilliseconds(280);

        private static HudNotification? _instance;

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
        /// 弹出一条通知胶囊。可从任意线程调用，内部自行切到UI线程。
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

        // ---------------- 队列管理 ----------------

        private void Play(string tag, string headline, string detail, HudLevel level, string? iconData)
        {
            // 上限与窗口高度跟随配置（多留1个槽位给溢出过渡动画）
            int maxVisible = Math.Clamp(Core.Config.Core_RunConfig._HudNotificationMaxCount, 1, 10);
            Height = 8 + (maxVisible + 1) * CapsuleSlot;

            PlaceTopCenter();
            if (!IsVisible)
            {
                Show();
            }

            var capsule = new HudCapsule();
            capsule.Configure(tag, headline, detail, AccentFor(level), iconData ?? IconFor(level));
            capsule.RemoveRequested += Capsule_RemoveRequested;

            // 槽位从0撑开，把已有胶囊顺势往下推
            var wrapper = new Border { Height = 0, ClipToBounds = true, Child = capsule };
            Stack.Children.Insert(0, wrapper);
            wrapper.BeginAnimation(HeightProperty, new DoubleAnimation(0, CapsuleSlot, SlotGrow)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            });

            // 停留时长 = 配置总时长 - 入场/折叠固定段
            double total = Math.Clamp(Core.Config.Core_RunConfig._HudNotificationDuration, 1, 30);
            capsule.BeginLifecycle(TimeSpan.FromSeconds(Math.Max(0, total - EntranceEnd - Fold)));

            // 超出上限：最旧的若干条（底部）缓慢渐隐让位
            var alive = Stack.Children.Cast<Border>()
                .Select(b => b.Child as HudCapsule)
                .Where(c => c != null && !c.IsRemoving)
                .Cast<HudCapsule>()
                .ToList();
            int excess = alive.Count - maxVisible;
            if (excess > 0)
            {
                foreach (var old in alive.Skip(alive.Count - excess))
                {
                    old.DismissGentle();
                }
            }
        }

        /// <summary>胶囊消失后收缩其槽位，其余胶囊顺势上滑；队列清空后隐藏窗口。</summary>
        private void Capsule_RemoveRequested(object? sender, EventArgs e)
        {
            if (sender is not HudCapsule capsule)
            {
                return;
            }
            capsule.RemoveRequested -= Capsule_RemoveRequested;

            var wrapper = Stack.Children.Cast<Border>().FirstOrDefault(b => b.Child == capsule);
            if (wrapper == null)
            {
                return;
            }

            var shrink = new DoubleAnimation(wrapper.Height, 0, SlotShrink)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
            };
            shrink.Completed += (_, _) =>
            {
                Stack.Children.Remove(wrapper);
                if (Stack.Children.Count == 0)
                {
                    Hide();
                }
            };
            wrapper.BeginAnimation(HeightProperty, shrink);
        }

        // ---------------- 级别映射与定位 ----------------

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
