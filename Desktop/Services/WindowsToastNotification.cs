using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Desktop.Services
{
    /// <summary>
    /// Windows原生通知通道（WinRT Toast）。
    /// 未打包的Win32程序没有包标识，直接调ToastNotificationManager会失败，
    /// 这里通过注册表登记AUMID（DisplayName/IconUri）获得通知身份，无需快捷方式和打包。
    /// 设置页"通知方式"选择系统通知时，HudNotification统一入口会路由到这里。
    /// </summary>
    public static class WindowsToastNotification
    {
        private const string Aumid = "DDTV.Desktop";
        private static bool _registered;

        /// <summary>
        /// 弹出Windows原生通知（进入通知中心）。任意线程可调用。
        /// </summary>
        public static void Show(string title, string message)
        {
            try
            {
                EnsureRegistered();

                var xml = new XmlDocument();
                xml.LoadXml(
                    "<toast><visual><binding template=\"ToastGeneric\">" +
                    $"<text>{Escape(title)}</text>" +
                    (message.Length > 0 ? $"<text>{Escape(message)}</text>" : "") +
                    "</binding></visual></toast>");

                ToastNotificationManager.CreateToastNotifier(Aumid).Show(new ToastNotification(xml));
            }
            catch (Exception ex)
            {
                Core.LogModule.Log.Error(nameof(WindowsToastNotification), "发送Windows原生通知失败", ex);
            }
        }

        /// <summary>登记AUMID到当前用户注册表（只做一次），让系统识别通知来源为DDTV。</summary>
        private static void EnsureRegistered()
        {
            if (_registered)
            {
                return;
            }

            using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\AppUserModelId\{Aumid}");
            key.SetValue("DisplayName", "DDTV");

            // 输出目录根部的DDTV.ico（csproj里None Update复制），不存在则跳过图标
            string iconPath = Path.Combine(AppContext.BaseDirectory, "DDTV.ico");
            if (File.Exists(iconPath))
            {
                key.SetValue("IconUri", iconPath);
            }

            _registered = true;
        }

        private static string Escape(string text) => text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
