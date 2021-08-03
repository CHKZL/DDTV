using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.System_Core;
using static Auxiliary.RequestMessge.System_Core.SystemInfo;

namespace DDTVLiveRecWebServer.API
{
    public class system_info
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_info");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<SystemInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
               return Auxiliary.RequestMessge.封装消息.获取系统消息.系统消息();
            }
          
        }
    }
}
