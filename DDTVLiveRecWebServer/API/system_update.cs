using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.System_Core;

namespace DDTVLiveRecWebServer.API
{
    public class system_update
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_update");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Message<SystemUpdateInfo>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return Auxiliary.RequestMessage.封装消息.消息_获取检查更新信息.检查更新信息();
            }

        }
    }
}
