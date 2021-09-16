using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.Config;
using static Auxiliary.RequestMessage.MessageClass;

namespace DDTVLiveRecWebServer.API
{
    public class config_auto_transcoding
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            List<string> KeyL = new List<string>()
            {
                "TranscodingStatus"
            }.Select(x => x.ToLower()).ToList();
            Dictionary<string, string> _ = UrlCode.UrlDecode(context, true);
            foreach (var item in KeyL)
            {
                if (_.ContainsKey(item))
                {
                    if (string.IsNullOrEmpty(_[item]))
                    {
                        鉴权预处理结果 = true;
                        break;
                    }
                }
                else
                {
                    鉴权预处理结果 = true;
                    break;
                }
            }
            //foreach (var item in new List<string>() {
            //    context.Request.Form["TranscodingStatus"]
            //})
            //{
            //    if (string.IsNullOrEmpty(item))
            //    {
            //        鉴权预处理结果 = true;
            //        break;
            //    }
            //};
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "config_auto_transcoding", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Message<AutoTranscoding>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                bool 转码设置 = false;
                bool.TryParse(context.Request.Form["transcoding_status"], out 转码设置);
                return Auxiliary.RequestMessage.封装消息.配置_自动转码.转码(转码设置);
            }
        }
    }
}
