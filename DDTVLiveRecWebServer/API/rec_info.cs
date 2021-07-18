using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class rec_info
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = true;
            foreach (var item in new List<string>() {
                context.Request.Form["GUID"]
            }){
                if(string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = false;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "rec_info", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>(鉴权结果, null);
            }
            else
            {
                List<Auxiliary.Downloader.DownIofoData> Package = new List<Auxiliary.Downloader.DownIofoData>();
                foreach (var item in Auxiliary.MMPU.DownList)
                {
                    if(context.Request.Form["GUID"]==item.DownIofo.事件GUID)
                    {
                         Package.Add(item.DownIofo);
                    }
                   
                }
                return ReturnInfoPackage.InfoPkak(鉴权结果, Package);
            }
        }
        private class Messge : ReturnInfoPackage.Messge<Auxiliary.Downloader.DownIofoData>
        {
            public static new List<Auxiliary.Downloader.DownIofoData> Package { set; get; }
        }
    }
}
