using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace DDTVLiveRecWebServer.API
{
    public class rec_cancel
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = true;
            foreach (var item in new List<string>() {
                context.Request.Form["GUID"],
                context.Request.Form["RoomId"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = false;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "rec_cancel", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>(鉴权结果, null);
            }
            else
            {
                List<Auxiliary.Downloader.DownIofoData> Package = new List<Auxiliary.Downloader.DownIofoData>();
                foreach (var item in Auxiliary.MMPU.DownList)
                {
                    if (context.Request.Form["GUID"] == item.DownIofo.事件GUID)
                    {
                        item.DownIofo.下载状态 = false;
                        item.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                        item.DownIofo.备注 = "用户取消下载";
                        item.DownIofo.WC.CancelAsync();
                        鉴权结果.鉴权返回消息 = "删除成功";
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
