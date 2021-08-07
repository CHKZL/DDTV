using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.Config;
using static Auxiliary.RequestMessge.MessgeClass;

namespace DDTVLiveRecWebServer.API
{
    public class config_auto_transcoding
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["TranscodingStatus"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "config_auto_transcoding", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<AutoTranscoding>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                bool 转码设置 = false;
                bool.TryParse(context.Request.Form["transcoding_status"], out 转码设置);
                return Auxiliary.RequestMessge.封装消息.修改配置_自动转码.转码(转码设置);
            }
        }
    }
}
