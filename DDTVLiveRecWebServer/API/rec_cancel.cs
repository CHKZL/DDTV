using Auxiliary;
using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using static Auxiliary.Downloader;
using static Auxiliary.RequestMessge.MessgeClass;

namespace DDTVLiveRecWebServer.API
{
    public class rec_cancel
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["GUID"],
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
                return ReturnInfoPackage.InfoPkak<Messge<DownIofoData>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.执行取消录制任务.取消录制任务(context.Request.Form["GUID"]);
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
    }
}
