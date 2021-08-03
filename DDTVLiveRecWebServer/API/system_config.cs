using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.ReturnInfoPackage;
using static Auxiliary.RequestMessge.System_Core;

namespace DDTVLiveRecWebServer.API
{
    public class system_config
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context,"system_config");
            if (!鉴权结果.鉴权结果)
            {
                return InfoPkak<Messge<systemConfig>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.获取配置文件信息.配置文件信息();
            }
        }
 
    }
}
