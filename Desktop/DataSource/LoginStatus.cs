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
        public static System.Threading.Timer Timer_LoginStatus;

        /// <summary>
        /// 异步刷新登录状态（Timer回调直接使用async void）
        /// </summary>
        public static async void RefreshLoginStatus(object state)
        {
            try
            {
                bool status = false;
                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    var (success, loginStatus) = await NetWork.Post.TryPostBody<bool>(
                        $"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_status");
                    //请求失败不等于登录失效：网络抖动时直接跳过本轮，避免误触发登录失效流程
                    if (!success)
                        return;
                    if (!loginStatus)
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
            }
            catch (Exception ex)
            {
                Core.LogModule.Log.Warn(nameof(RefreshLoginStatus), $"刷新登录状态失败", ex, false);
            }
        }
    }
}
