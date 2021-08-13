using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.System_Core;
using static Auxiliary.RequestMessage.System_Core.SystemInfo;

namespace DDTVLiveRecWebServer.API
{
    public class system_info
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_info");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Message<SystemInfo>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return Auxiliary.RequestMessage.封装消息.获取系统消息.系统消息();
            }
          
        }
    }
}
