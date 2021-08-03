using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.Downloader;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 执行取消录制任务
    {
        public static string 取消录制任务(string GUID)
        {
            List<DownIofoData> Package = new List<DownIofoData>();
            foreach (var item in MMPU.DownList)
            {
                if (GUID == item.DownIofo.事件GUID)
                {
                    item.DownIofo.下载状态 = false;
                    item.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                    item.DownIofo.备注 = "用户取消下载";
                    item.DownIofo.WC.CancelAsync();
                    下载结束提醒("API请求取消该下载任务", item.DownIofo);
                    //鉴权结果.鉴权返回消息 = "删除成功";
                    Package.Add(item.DownIofo);
                    Package[Package.Count - 1].WC = null;
                }
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, Package);
        }
        private static void 下载结束提醒(string 提醒标题, DownIofoData item)
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
