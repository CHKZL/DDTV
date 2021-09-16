using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.ServerClass;

namespace DDTVLiveRecWebServer.API
{
    public class rec_info
    {
        public static string Web(HttpContext context)
        {
            try
            {
                int B = context.Request.Form.Count();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<Auxiliary.Downloader.DownIofoData>>((int)ServerSendMessageCode.鉴权失败, null, "请求的表单格式不正确！");
            }
            bool 鉴权预处理结果 = false;
            List<string> KeyL = new List<string>()
            {
                "GUID"
            };
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
            //    context.Request.Form["GUID"]
            //}){
            //    if(string.IsNullOrEmpty(item))
            //    {
            //        鉴权预处理结果 = true;
            //        break;
            //    }
            //};
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "rec_info", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Message<Auxiliary.Downloader.DownIofoData>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return Auxiliary.RequestMessage.封装消息.下载_获取录制任务详情信息.录制任务详情信息(context.Request.Form["GUID"]);
            }
        }
    }
}
