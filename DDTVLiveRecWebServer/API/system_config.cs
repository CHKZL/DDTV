using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.ReturnInfoPackage;
using static Auxiliary.RequestMessage.System_Core;

namespace DDTVLiveRecWebServer.API
{
    public class system_config
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context,"system_config");
            if (!鉴权结果.鉴权结果)
            {
                return InfoPkak<Message<systemConfig>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return Auxiliary.RequestMessage.封装消息.消息_获取配置文件信息.配置文件信息();
            }
        }
 
    }
}
