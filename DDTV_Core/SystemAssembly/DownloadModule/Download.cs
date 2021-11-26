using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Disable the warning.
//#pragma warning disable SYSLIB0014

namespace DDTV_Core.SystemAssembly.DownloadModule
{
    public class Download
    {
        public static string DefaultPath = CoreConfig.GetValue(CoreConfigClass.Key.DownloadPath, "Rec", CoreConfigClass.Group.Download);
        public static string DefaultDirectoryName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadDirectoryName, "{ROOMID}_{NAME}", CoreConfigClass.Group.Download);
        public static string DefaultFileName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadFileName, "{DATE}_{TIME}_{TITLE}_{R}", CoreConfigClass.Group.Download);
        /// <summary>
        /// 增加下载任务
        /// </summary>
        public static void AddDownloadTaskd(long uid)
        {
            Task.Run(() =>
            {
                AddDownLoad(uid);
            });
        }
        public static void CancelDownload(long uid)
        {
            if (BilibiliModule.Rooms.Rooms.RoomInfo.TryGetValue(uid, out BilibiliModule.Rooms.RoomInfoClass.RoomInfo roomInfo))
            {
                foreach (var item in roomInfo.DownloadingList)
                {
                    item.Cancel();
                }
            }
        }
        /// <summary>
        /// 下载任务结束处理
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="IsSun">是否合并</param>
        public static void DownloadCompleteTaskd(long uid, bool Split = false , bool IsSun = true, bool IsTranscod = false)
        {
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                //当自动切片使能时，自动转码和flv文件合并取消
                if(Split)
                {
                    IsSun = false;
                    IsTranscod = false;
                }
                else
                {
                    IsSun = true;
                    IsTranscod = true;
                }
                roomInfo.IsDownload=false;
                if (IsSun)
                {
                    string SunFileName = Tool.FlvModule.Sum.FlvFileSum(roomInfo, $"./{DefaultPath}" +
                                                   $"/{DefaultDirectoryName.Replace("{ROOMID}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)).Replace("{NAME}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname))}" +
                                                   "/" +
                                                   $"{DefaultFileName.Replace("{DATE}", DateTime.Now.ToString("yyMMdd")).Replace("{TIME}", DateTime.Now.ToString("HH-mm-ss")).Replace("{TITLE}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.title)).Replace("_{R}", "") + ".flv"}");
                    Tool.TranscodModule.Transcod.CallFFMPEG(new Tool.TranscodModule.TranscodClass()
                    {
                        AfterFilenameExtension=".mp4",
                        BeforeFilePath= SunFileName,
                        AfterFilePath= SunFileName,
                    });
                }

                foreach (var item in roomInfo.DownloadingList)
                {
                    item.HttpWebRequest=null;
                    roomInfo.DownloadedLog.Add(item);
                }
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"\n录制任务完成:\n===================\n" +
                           $"直播间:{roomInfo.room_id}\n" +
                           $"UID:{roomInfo.uid}\n" +
                           $"昵称:{roomInfo.uname}\n" +
                           $"标题:{roomInfo.title}\n" +
                           $"储存路径:{roomInfo.DownloadingList[0].File}" +
                           $"===================");
                roomInfo.DownloadingList=new List<DownloadClass.Downloads>();
            }
        }
        /// <summary>
        /// 增加下载任务具体实现
        /// </summary>
        private static void AddDownLoad(long uid)
        {
            while (!Rooms.RoomInfo.ContainsKey(uid))
            {
                RoomConfig.AddRoom(uid, "Temporary");
                Thread.Sleep(200);
            }
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                DownloadClass.Downloads downloadClass = new DownloadClass.Downloads();
                Rooms.RoomInfo[uid].DownloadingList.Add(downloadClass);
                downloadClass.RoomId =Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id);
                DownloadClass.Downloads DL = roomInfo.DownloadingList.Find(e => e.IsDownloading);
                if (DL==null&&Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.roomStatus)=="1")
                {
                    roomInfo.IsDownload=true;
                    downloadClass.Uid = uid;
                    downloadClass.Url = BilibiliModule.API.RoomInfo.playUrl(uid, RoomInfoClass.PlayQuality.OriginalPainting);
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(downloadClass.Url);
                    req.Method = "GET";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Accept="*/*";
                    req.UserAgent = NetClass.UA();
                    req.Referer="https://www.bilibili.com/";
                    if (downloadClass.Url.Contains("bili"))
                    {
                        if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                        {
                            req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie);
                        }
                    }
                    downloadClass.Status= DownloadClass.Downloads.DownloadStatus.Standby;
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"收到直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}({Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)})的下载请求，等在数据流中...");
                    int Ok = IsOk(roomInfo, downloadClass.Url);
                    if (Ok==0)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"\n开始录制任务:\n===================\n" +
                            $"直播间:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}\n" +
                            $"UID:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uid)}\n" +
                            $"昵称:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)}\n" +
                            $"标题:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.title)}\n" +
                            $"===================");
                        downloadClass.DownFLV_HttpWebRequest(
                                               req,
                                               $"./{DefaultPath}" +
                                               $"/{DefaultDirectoryName.Replace("{ROOMID}", downloadClass.RoomId).Replace("{NAME}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname))}",
                                               $"{DefaultFileName.Replace("{DATE}", DateTime.Now.ToString("yyMMdd")).Replace("{TIME}", DateTime.Now.ToString("HH-mm-ss")).Replace("{TITLE}", Rooms.GetValue(downloadClass.Uid, DataCacheModule.DataCacheClass.CacheType.title)).Replace("{R}", new Random().Next(1000, 9999).ToString())}", "flv"
                                               , roomInfo.FlvCuttinge);
                    }
                    else if(Ok==1)
                    {
                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}已下播，录制任务取消");
                        Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                        DownloadCompleteTaskd(uid, !roomInfo.FlvCuttinge);
                        return;
                    }
                    else
                    {
                        AddDownLoad(uid);
                    }
                }
                else
                {
                    if (DL!=null)
                    {
                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}已有录制任务，放弃新建");
                        return;
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}检测不存在，请检查提交的用户uid");
                        return;
                    }
                }
            }
            else
            {
                Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}所属的UID不在房间配置文件内，取消下载任务");
                return;
            }
        }
        private static int IsOk(RoomInfoClass.RoomInfo roomInfo, string Url)
        {
            int conut = 0;
            while (true)
            {
                if (roomInfo.live_status==1)
                {
                    if (Tool.FileOperation.IsExistsNetFile(Url))
                    {
                        return 0;
                    }
                    if (conut>5)
                    {
                        return -1;
                    }
                    Thread.Sleep(1000);
                }
                else
                {
                    return 1;
                }
                conut++;
            }
        }
    }
}
