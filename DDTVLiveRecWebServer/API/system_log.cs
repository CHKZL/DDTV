using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.InfoLog;
using static Auxiliary.RequestMessge.MessgeClass;

namespace DDTVLiveRecWebServer.API
{
    public class system_log
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            List<string> T = new List<string>();
            int 需求条数 = 0;
            if(!string.IsNullOrEmpty(context.Request.Form["count"]))
            {
                T.Add(context.Request.Form["count"]);
                int.TryParse(context.Request.Form["count"], out 需求条数);              
            }
            foreach (var item in T)
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_log", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<LogInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.获取系统日志.日志(需求条数);
            }
        }
    }
}
