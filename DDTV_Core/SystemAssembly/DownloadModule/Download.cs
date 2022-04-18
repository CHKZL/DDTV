using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass.Downloads;

// Disable the warning.
//#pragma warning disable SYSLIB0014

namespace DDTV_Core.SystemAssembly.DownloadModule
{
    public class Download
    {
        public static string DownloadPath = CoreConfig.GetValue(CoreConfigClass.Key.DownloadPath, "./Rec/", CoreConfigClass.Group.Download);
        public static string DownloadDirectoryName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadDirectoryName, "{ROOMID}_{NAME}", CoreConfigClass.Group.Download);
        public static string DownloadFileName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadFileName, "{DATE}_{TIME}_{TITLE}", CoreConfigClass.Group.Download);
        public static string TmpPath = CoreConfig.GetValue(CoreConfigClass.Key.TmpPath, "./tmp/", CoreConfigClass.Group.Download);
        public static int RecQuality = int.Parse(CoreConfig.GetValue(CoreConfigClass.Key.RecQuality, "10000", CoreConfigClass.Group.Download));
        public static bool IsRecDanmu = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecDanmu, "true", CoreConfigClass.Group.Download));
        public static bool IsRecGift = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecGift, "true", CoreConfigClass.Group.Download));
        public static bool IsRecGuard = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecGuard, "true", CoreConfigClass.Group.Download));
        public static bool IsRecSC = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecSC, "true", CoreConfigClass.Group.Download));
        public static bool IsFlvSplit = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsFlvSplit, "false", CoreConfigClass.Group.Download));
        public static long FlvSplitSize = long.Parse(CoreConfig.GetValue(CoreConfigClass.Key.FlvSplitSize, "1073741824", CoreConfigClass.Group.Download));
       
        /// <summary>
        /// 下载完成事件
        /// </summary>
        public static event EventHandler<EventArgs> DownloadCompleted;
        /// <summary>
        /// 增加下载任务
        /// </summary>
        public static void AddDownloadTaskd(long uid, bool IsNewTask = false)
        {
            Task.Run(() =>
            {
                if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    if(IsNewTask || !roomInfo.IsUserCancel)
                    {
                        roomInfo.IsUserCancel = false;
                        AddDownLoad(uid, IsNewTask);
                    }
                    else
                    {
                        if(!IsNewTask)
                        {
                            DownloadCompleteTaskd(uid, IsFlvSplit);
                        }
                        roomInfo.IsUserCancel = false;
                    }
                }
                
            });
        }
        /// <summary>
        /// 取消下载任务
        /// </summary>
        /// <param name="uid"></param>
        public static bool CancelDownload(long uid)
        {
            try
            {
                if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    roomInfo.IsUserCancel = true;
                    foreach (var item in roomInfo.DownloadingList)
                    {
                        WebHook.SendHook(WebHook.HookType.CancelRec, uid);
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"取消【{ roomInfo.uname}({roomInfo.room_id})】的下载任务");
                        item.Cancel();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"[UID:{uid}]取消下载任务出现未知错误！", true, e);
                return false;
            }
        }
        /// <summary>
        /// 下载任务结束处理
        /// </summary>
        /// <param name="uid">用户UID</param>
        /// <param name="Split">任务是否切片(当自动切片使能时，自动转码和flv文件合并取消)</param>
        /// <param name="IsCancel">该任务是否已经取消</param>
        internal static void DownloadCompleteTaskd(long uid, bool Split = false, bool IsCancel = false)
        {
            bool IsSun = true, IsTranscod = Tool.TranscodModule.Transcod.IsAutoTranscod;
            if(IsCancel)
            {
                IsSun = false;
                IsTranscod = false;
                Split = false;
            }
            List<string> FileList = new List<string>();
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                WebHook.SendHook(WebHook.HookType.RecComplete, uid);
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"开始执行[{roomInfo.uname}({roomInfo.room_id})]直播间的下载任务结束处理任务");
                do
                {
                    Thread.Sleep(500);
                } while (roomInfo.IsCliping);
                roomInfo.IsUserCancel = false;
                if (DownloadPath.Substring(DownloadPath.Length - 1, 1) != "/")
                    DownloadPath = DownloadPath + "/";
                string OkFileName = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadPath}" + $"{DownloadDirectoryName}" + $"/{roomInfo.CreationTime.ToString("yy_MM_dd")}/" + $"{DownloadFileName}" + "_{R}.flv");

                //弹幕录制结束处理
                if (bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)) && roomInfo.roomWebSocket.IsConnect)
                {
                    roomInfo.roomWebSocket.IsConnect = false;
                    if (roomInfo.roomWebSocket.LiveChatListener != null)
                    {
                        try
                        {
                            roomInfo.roomWebSocket.LiveChatListener.IsUserDispose = true;
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{ roomInfo.uname}({roomInfo.room_id})】的直播已结束，LiveChat连接中断，储存相关数据");
                            roomInfo.roomWebSocket.LiveChatListener.startIn = false;
                            roomInfo.DanmuFile.TimeStopwatch.Stop();
                            roomInfo.roomWebSocket.LiveChatListener.Dispose();
                            BilibiliModule.API.DanMu.DanMuRec.SevaDanmuFile(roomInfo);
                        }
                        catch (Exception e)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"【{ roomInfo.uname}({roomInfo.room_id})】结束LiveChat连接时发生未知错误", true, e);
                        }
                    }
                }
                else
                {
                    if (!bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)))
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{ roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到IsAutoRec为false，将不会储存弹幕相关数据");
                    }
                    if(!roomInfo.roomWebSocket.IsConnect)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{ roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到WebSocket未连接，将不会储存弹幕相关数据");
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
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{ roomInfo.uname}({roomInfo.room_id})】录制任务结束，开始检测并合并flv文件");
                    string SunFileName = Tool.FlvModule.Sum.FlvFileSum(roomInfo, OkFileName);
                    if (!string.IsNullOrEmpty(SunFileName))
                    {
                        roomInfo.DownloadedFileInfo.FlvFile = new FileInfo(SunFileName);
                        FileList.Add(SunFileName);
                    }
                }
                //转码
                if (IsTranscod)
                {
                    if (FileList.Count > 0)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{ roomInfo.uname}({roomInfo.room_id})】合并flv文件任务结束，开始转码");
                        foreach (var item in FileList)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                var tm = Tool.TranscodModule.Transcod.CallFFMPEG(new Tool.TranscodModule.TranscodClass()
                                {
                                    AfterFilenameExtension = ".mp4",
                                    BeforeFilePath = item,
                                    AfterFilePath = item,
                                });
                                roomInfo.DownloadedFileInfo.Mp4File = new FileInfo(tm.AfterFilePath);
                                WebHook.SendHook(WebHook.HookType.TranscodingComplete, uid);
                            }
                        }
                    }
                    else
                    {
                        if (IsSun)
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{ roomInfo.uname}({roomInfo.room_id})】合并返回的flv文件数量为0，放弃转码，保留原始数据或环境");
                    }
                }
                if (!IsCancel)
                {
                    DownloadClass.Downloads downloads = new DownloadClass.Downloads();
                    DateTime StartTime = DateTime.MaxValue;
                    DateTime EndTime = DateTime.MinValue;
                    foreach (var item in roomInfo.DownloadingList)
                    {
                        item.HttpWebRequest = null;
                        downloads.Token = item.Token;
                        downloads.HttpWebRequest = null;
                        downloads.RoomId = item.RoomId;
                        downloads.Uid = item.Uid;
                        downloads.Name = item.Name;
                        downloads.IsDownloading = false;
                        downloads.Url = String.Empty;
                        downloads.FileName = String.Empty;
                        if (item.Title != downloads.Title)
                        {
                            downloads.Title = item.Title;
                        }
                        if (item.StartTime < StartTime)
                        {
                            downloads.StartTime = item.StartTime;
                        }
                        if (item.EndTime > EndTime)
                        {
                            downloads.EndTime = item.EndTime;
                        }
                        downloads.DownloadCount += item.DownloadCount;
                        downloads.TotalDownloadCount += item.TotalDownloadCount;
                        downloads.Status = DownloadStatus.DownloadComplete;
                        downloads.FilePath = item.FilePath;
                        if (FileList.Count > 0)
                        {
                            downloads.FileName = FileList[0];
                        }
                    }

                    roomInfo.DownloadedLog.Add(downloads);
                    if (DownloadCompleted != null)
                    {
                        DownloadCompleted.Invoke(downloads, EventArgs.Empty);
                    }

                    #region 房间Shell命令
                    if (!string.IsNullOrEmpty(roomInfo.Shell))
                    {
                        Console.WriteLine($"{roomInfo.uname}直播间录制完成，开始执行预设的Shell命令");
                        Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间开始执行Shell命令:" +roomInfo.Shell, false, null, false);
                        Task.Run(() =>
                        {
                            WebHook.SendHook(WebHook.HookType.RunShellComplete, roomInfo.uid);
                            try
                            {
                                string Shell = Tool.FileOperation.ReplaceKeyword(uid, roomInfo.Shell);
                                string result = Tool.SystemResource.ExternalCommand.Shell(Shell);
                                Console.WriteLine($"{roomInfo.uname}直播间的Shell命令执行完成，执行返回结果:\n{result}");
                                Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间执行Shell命令结束，返回信息:{result}", false, null, false);
                            }
                            catch (Exception e)
                            {
                                Log.Log.AddLog("Shell", Log.LogClass.LogType.Error, $"{roomInfo.uname}({roomInfo.room_id})直播间执行Shell命令失败！详细堆栈信息已写入日志。", true, e, true);
                            }
                        });
                    }
                    #endregion

                    string EndText = $"\n({DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")})录制任务完成:\n===================\n" +
                               $"直播间:{roomInfo.room_id}\n" +
                               $"UID:{roomInfo.uid}\n" +
                               $"昵称:{roomInfo.uname}\n" +
                               $"标题:{roomInfo.title}\n" +
                               $"储存路径:" + (roomInfo.DownloadingList.Count > 0 ? roomInfo.DownloadingList[0].FileName : string.Empty) +
                               $"\n===================";
                    Console.WriteLine(EndText);
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, EndText.Replace("\n", "　"), false, null, false);
                }
                roomInfo.DownloadingList = new List<DownloadClass.Downloads>();
            }
        }
        /// <summary>
        /// 增加下载任务具体实现
        /// </summary>
        /// <param name="uid">需要下载的房间uid号</param>
        private static void AddDownLoad(long uid, bool IsNewTask)
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
                    DownloadClass.Downloads DL = roomInfo.DownloadingList.Find(e => e.IsDownloading);
                    if (DL == null)// && Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.roomStatus) == "1")
                    {
                        
                        Rooms.RoomInfo[uid].DownloadingList.Add(downloadClass);
                        downloadClass.RoomId = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id);
                        downloadClass.Name = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname);
                        roomInfo.IsDownload = true;
                        downloadClass.Uid = uid;
                        downloadClass.Url = BilibiliModule.API.RoomInfo.playUrl(uid, (RoomInfoClass.Quality)RecQuality);
                        downloadClass.IsDownloading = true;
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
                        //if (IsNewTask)
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"收到【{ roomInfo.uname}({roomInfo.room_id})】的下载请求，等待数据流中...");
                        int Ok = IsOk(roomInfo, downloadClass.Url);
                        if (Ok == 0)
                        {
                            roomInfo.CreationTime = DateTime.Now;
                            downloadClass.Title = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.title);
                            string StarText = $"\n({DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")})开始录制任务:\n===================\n" +
                                $"直播间:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}\n" +
                                $"UID:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uid)}\n" +
                                $"昵称:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)}\n" +
                                $"标题:{downloadClass.Title}\n" +
                                $"===================";
                            Console.WriteLine(StarText);
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, StarText.Replace("\n", "　"), false, null, false);
                            if (DownloadPath.Substring(DownloadPath.Length - 1, 1) != "/")
                                DownloadPath = DownloadPath + "/";
                            string Path = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadPath}{DownloadDirectoryName}");
                            string FileName = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadFileName}" + "_{R}");
                            //执行下载任务
                            downloadClass.FilePath = Path;
                            downloadClass.FlvSplit = IsFlvSplit;
                            downloadClass.FlvSplitSize = FlvSplitSize;
                            Tool.FileOperation.CreateAll(Path+ $"/{roomInfo.CreationTime.ToString("yy_MM_dd")}");
                            downloadClass.DownFLV_HttpWebRequest(downloadClass, req, Path, FileName, "flv", roomInfo);
                            //录制弹幕(是否是新任务 && 弹幕录制总开关 && 房间弹幕录制设置)
                            if (IsNewTask && IsRecDanmu && bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsRecDanmu)))
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{ roomInfo.uname}({roomInfo.room_id})】弹幕录制请求已发出");
                                roomInfo.DanmuFile = new();
                                roomInfo.DanmuFile.FileName = Path + $"/{roomInfo.CreationTime.ToString("yy_MM_dd")}/" + FileName;
                                BilibiliModule.API.DanMu.DanMuRec.Rec(uid);
                            }
                            else
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{ roomInfo.uname}({roomInfo.room_id})】录制任务不进行弹幕录制，理由：IsNewTask:{IsNewTask},IsRecDanmu:{IsRecDanmu}");
                            }
                        }
                        else if (Ok == 1)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{ roomInfo.uname}({roomInfo.room_id})】已下播，录制任务取消");
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            DownloadCompleteTaskd(uid, IsFlvSplit);
                            return;
                        }
                        else if (Ok == -2)
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            DownloadCompleteTaskd(uid, IsFlvSplit);
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
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{ roomInfo.uname}({roomInfo.room_id})】已有录制任务，放弃新建");
                            return;
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{ roomInfo.uname}({roomInfo.room_id})】检测不存在，请检查提交的用户uid");
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
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"新建下载任务发生意料外的错误！", true, e);
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
                if(roomInfo.IsUserCancel)
                {
                    roomInfo.IsUserCancel = !roomInfo.IsUserCancel;
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"检测到用户取消请求，放弃继续监听网络文件状态请求");
                    return -2;
                }
                if (Rooms.GetValue(roomInfo.uid, DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
                {
                    if (Tool.FileOperation.IsExistsNetFile(Url))
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"已确认房间【{roomInfo.uname}({roomInfo.room_id})】的推流数据，转交信息给下载进程");
                        return 0;
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"房间【{roomInfo.uname}({roomInfo.room_id})】已开播，但未监测到推流数据，3秒后重试");
                    }
                    if (conut > 6)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】请求网络文件超时，等待任务自动重置..");
                        return -1;
                    }
                    Thread.Sleep(5000);
                }
                else
                {
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"请求的网络路径绑定的房间号已经下播，放弃请求");
                    return 1;
                }
                conut++;
            }
        }
    }
}
