using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.System_Core;

namespace DDTVLiveRecWebServer.API
{
    public class system_update
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_update");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<SystemUpdateInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.获取检查更新信息.检查更新信息();
            }

        }
    }
}
