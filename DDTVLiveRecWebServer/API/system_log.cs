using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class system_log
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_log");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>((int)ReturnInfoPackage.MessgeCode.鉴权失败, "鉴权失败", null);
            }
            else
            {              
                return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, "请求成功", Auxiliary.InfoLog.logInfos);
            }
        }
        private class Messge : Auxiliary.InfoLog.LogInfo
        {
            public static List<Auxiliary.InfoLog.LogInfo> Package { set; get; }
        }
    }
}
