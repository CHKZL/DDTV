using Core;
using static Core.Tools.DokiDoki;

namespace Desktop.DataSource
{
    internal class LoginStatus
    {
        /// <summary>
        /// 登陆失效事件
        /// </summary>
        public static event EventHandler<EventArgs> LoginFailureEvent;
        /// <summary>
        /// 登陆窗展示状态
        /// </summary>
        public static bool LoginWindowDisplayStatus = false;
        public static Timer Timer_LoginStatus;
        public static void RefreshLoginStatus(object state)
        {
            Task.Run(() =>
            {
                bool status = false;
                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    if (!NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_status").Result)
                    {
                        status = true;
                    }
                }
                else
                {
                    if (!Core.RuntimeObject.Account.GetLoginStatus())
                    {
                        status = true;
                    }
                }
                if (status)
                {
                    LoginFailureEvent?.Invoke(null, new EventArgs());
                }
            });
        }
    }
}
