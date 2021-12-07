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
        public static void AddDownloadTaskd(long uid,bool IsNewTask=false)
        {
            Task.Run(() =>
            {
                AddDownLoad(uid, IsNewTask);
            });
        }
        /// <summary>
        /// 取消下载任务
        /// </summary>
        /// <param name="uid"></param>
        public static void CancelDownload(long uid)
        {
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
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
        /// <param name="uid">用户UID</param>
        /// <param name="Split">任务是否切片(当自动切片使能时，自动转码和flv文件合并取消)</param>
        /// <param name="IsSun">是否合并和转码</param>
        /// <param name="IsTranscod">是否转码</param>
        public static void DownloadCompleteTaskd(long uid, bool Split = false, bool IsSun = true, bool IsTranscod = true)
        {
            List<string> FileList = new List<string>();
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                string OkFileName = $"./{DefaultPath}" +
                                                   $"/{DefaultDirectoryName.Replace("{ROOMID}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)).Replace("{NAME}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname))}" +
                                                   "/" +
                                                   $"{DefaultFileName.Replace("{DATE}", DateTime.Now.ToString("yyMMdd")).Replace("{TIME}", DateTime.Now.ToString("HH-mm-ss")).Replace("{TITLE}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.title)).Replace("_{R}", "") + ".flv"}";
                //弹幕录制结束处理
                if (bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec))&&roomInfo.roomWebSocket.IsConnect)
                {
                    roomInfo.roomWebSocket.IsConnect = false;
                    if (roomInfo.roomWebSocket.LiveChatListener != null)
                        try
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"{ Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)}({Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)})的直播已结束，LiveChat连接中断，储存相关数据");
                            roomInfo.roomWebSocket.LiveChatListener.startIn = false;
                            roomInfo.DanmuFile.TimeStopwatch.Stop();
                            roomInfo.roomWebSocket.LiveChatListener.Dispose();
                            BilibiliModule.API.DanMu.DanMuRec.SevaDanmuFile(roomInfo);
                        }
                        catch (Exception e)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"结束LiveChat连接时发生未知错误", true, e);
                        }

                }
                //当自动切片使能时，自动转码和flv文件合并取消
                if (Split)
                {
                    IsSun = false;
                }
                roomInfo.IsDownload = false;
                //合并flv文件
                if (IsSun)
                {
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}({Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)})录制任务结束，开始检测并合并flv文件");
                    string SunFileName = Tool.FlvModule.Sum.FlvFileSum(roomInfo, OkFileName);
                    if(!string.IsNullOrEmpty(SunFileName))
                    {
                        FileList.Add(SunFileName);
                    }
                    
                }
                if (IsTranscod)
                {
                    if (FileList.Count > 0)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}({Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)})合并flv文件任务结束，开始转码");
                        foreach (var item in FileList)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Tool.TranscodModule.Transcod.CallFFMPEG(new Tool.TranscodModule.TranscodClass()
                                {
                                    AfterFilenameExtension = ".mp4",
                                    BeforeFilePath = item,
                                    AfterFilePath = item,
                                });
                            }
                        }
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}({Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)})合并返回的flv文件数量为0，放弃转码，保留原始数据或环境");
                    }
                    
                }
               
                foreach (var item in roomInfo.DownloadingList)
                {
                    item.HttpWebRequest = null;
                    roomInfo.DownloadedLog.Add(item);
                }
                string EndText = $"\n录制任务完成:\n===================\n" +
                           $"直播间:{roomInfo.room_id}\n" +
                           $"UID:{roomInfo.uid}\n" +
                           $"昵称:{roomInfo.uname}\n" +
                           $"标题:{roomInfo.title}\n" +
                           $"储存路径:" + (roomInfo.DownloadingList.Count > 0 ? roomInfo.DownloadingList[0].FileName : string.Empty) +
                           $"\n===================";
                Console.WriteLine(EndText);
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, EndText.Replace("\n","　"), false, null, false);
                roomInfo.DownloadingList = new List<DownloadClass.Downloads>();
            }
        }
        /// <summary>
        /// 增加下载任务具体实现
        /// </summary>
        /// <param name="uid">需要下载的房间uid号</param>
        private static void AddDownLoad(long uid,bool IsNewTask)
        {
            while (!Rooms.RoomInfo.ContainsKey(uid))
            {
                RoomConfig.AddRoom(uid, "Temporary");
                Thread.Sleep(200);
            }
            try
            {
                if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    DownloadClass.Downloads downloadClass = new DownloadClass.Downloads();
                    Rooms.RoomInfo[uid].DownloadingList.Add(downloadClass);
                    downloadClass.RoomId = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id);
                    DownloadClass.Downloads DL = roomInfo.DownloadingList.Find(e => e.IsDownloading);
                    if (DL == null)// && Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.roomStatus) == "1")
                    {
                        roomInfo.IsDownload = true;
                        downloadClass.Uid = uid;
                        downloadClass.Url = BilibiliModule.API.RoomInfo.playUrl(uid, RoomInfoClass.PlayQuality.OriginalPainting);
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(downloadClass.Url);
                        req.Method = "GET";
                        req.ContentType = "application/x-www-form-urlencoded";
                        req.Accept = "*/*";
                        req.UserAgent = NetClass.UA();
                        req.Referer = "https://www.bilibili.com/";
                        if (downloadClass.Url.Contains("bili"))
                        {
                            if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                            {
                                req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie);
                            }
                        }
                        downloadClass.Status = DownloadClass.Downloads.DownloadStatus.Standby;
                        if (!IsNewTask)
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"收到直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}({Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)})的下载请求，等在数据流中...");
                        int Ok = IsOk(roomInfo, downloadClass.Url);
                        if (Ok == 0)
                        {
                            string StarText = $"\n开始录制任务:\n===================\n" +
                                $"直播间:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}\n" +
                                $"UID:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uid)}\n" +
                                $"昵称:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)}\n" +
                                $"标题:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.title)}\n" +
                                $"===================";
                            Console.WriteLine(StarText);
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, StarText.Replace("\n","　"),false,null,false);

                            string Path = $"./{DefaultPath}/{DefaultDirectoryName.Replace("{ROOMID}", downloadClass.RoomId).Replace("{NAME}", Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname))}";
                            string FileName = $"{DefaultFileName.Replace("{DATE}", DateTime.Now.ToString("yyMMdd")).Replace("{TIME}", DateTime.Now.ToString("HH-mm-ss")).Replace("{TITLE}", Rooms.GetValue(downloadClass.Uid, DataCacheModule.DataCacheClass.CacheType.title)).Replace("{R}", new Random().Next(1000, 9999).ToString())}";
                            //执行下载任务
                            downloadClass.DownFLV_HttpWebRequest(downloadClass, req,Path, FileName, "flv", roomInfo.FlvSplit);
                            //录制弹幕
                            if(IsNewTask && bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsRecDanmu)))
                            {
                                roomInfo.DanmuFile = new();
                                roomInfo.DanmuFile.FileName = Path + "/" + FileName;
                                BilibiliModule.API.DanMu.DanMuRec.Rec(uid);
                            }
                        }
                        else if (Ok == 1)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}已下播，录制任务取消");
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            DownloadCompleteTaskd(uid, roomInfo.FlvSplit);
                            return;
                        }
                        else
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            AddDownloadTaskd(uid, false);
                        }
                    }
                    else
                    {
                        if (DL != null)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}已有录制任务，放弃新建");
                            return;
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}检测不存在，请检查提交的用户uid");
                            return;
                        }
                    }
                }
                else
                {
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}所属的UID不在房间配置文件内，取消下载任务");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"新建下载任务发生意料外的错误！",true,e);
            }
        }
        /// <summary>
        /// 判断文件是否可以下载
        /// </summary>
        /// <param name="roomInfo">房间信息</param>
        /// <param name="Url">网络文件地址</param>
        /// <returns></returns>
        private static int IsOk(RoomInfoClass.RoomInfo roomInfo, string Url)
        {
            int conut = 0;
            while (true)
            {
                if (Rooms.GetValue(roomInfo.uid, DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
                {
                    if (Tool.FileOperation.IsExistsNetFile(Url))
                    {
                        return 0;
                    }
                    if (conut > 5)
                    {
                        return -1;
                    }
                    Thread.Sleep(3000);
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
