using Auxiliary;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using static Auxiliary.Downloader;

namespace DDTVLiveRecWebServer.API
{
    public class rec_cancel
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["GUID"],
                context.Request.Form["RoomId"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
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
                        下载结束提醒("API请求取消该下载任务", item.DownIofo);
                        鉴权结果.鉴权返回消息 = "删除成功";
                        Package.Add(item.DownIofo);
                        Package[Package.Count - 1].WC = null;
                    }
                }
                return ReturnInfoPackage.InfoPkak(鉴权结果, Package);
            }
        }
        public static void 下载结束提醒(string 提醒标题, DownIofoData item)
        {
            try
            {
                if (MMPU.录制弹幕 && MMPU.弹幕录制种类 == 2)
                {
                    try
                    {
                        item.弹幕储存流.WriteLine("</i>");
                        item.弹幕储存流.Flush();//写入弹幕数据
                        Clear(false, item);
                    }
                    catch (Exception)
                    { }
                }
                else
                {
                    Clear(true, item);
                }
               
            }
            catch (Exception) { }
            InfoLog.InfoPrintf($"\r\n=============={提醒标题}================\r\n" +
                   $"主播名:{item.主播名称}" +
                   $"\r\n房间号:{item.房间_频道号}" +
                   $"\r\n标题:{item.标题}" +
                   $"\r\n开播时间:{MMPU.Unix转换为DateTime(item.开始时间.ToString())}" +
                   $"\r\n结束时间:{MMPU.Unix转换为DateTime(item.结束时间.ToString())}" +
                   $"\r\n保存路径:{item.文件保存路径}" +
                   $"\r\n下载任务类型:{(item.继承.是否为继承对象 ? "续下任务" : "新建下载任务")}" +
                   $"\r\n结束原因:{item.备注}" +
                   $"\r\n==============={提醒标题}===============\r\n", InfoLog.InfoClass.下载系统信息);
        }
        private class Messge : ReturnInfoPackage.Messge<Auxiliary.Downloader.DownIofoData>
        {
            public static new List<Auxiliary.Downloader.DownIofoData> Package { set; get; }
        }
    }
}
