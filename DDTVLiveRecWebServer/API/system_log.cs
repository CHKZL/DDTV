using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;

namespace DDTVLiveRecWebServer.API
{
    public class system_log
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_log");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<Auxiliary.InfoLog.LogInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {              
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, Auxiliary.InfoLog.logInfos);
            }
        }
    }
}
