using DDTV_Core.SystemAssembly.BilibiliModule.API;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;
using Newtonsoft.Json;
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


namespace DDTV_Core.SystemAssembly.DownloadModule
{
    public class Download
    {
        public static string DownloadPath = CoreConfig.GetValue(CoreConfigClass.Key.DownloadPath, "./Rec/", CoreConfigClass.Group.Download);
        public static string DownloadDirectoryName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadDirectoryName, "{ROOMID}_{NAME}", CoreConfigClass.Group.Download);
        public static string DownloadFileName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadFileName, "{DATE}_{TIME}_{TITLE}", CoreConfigClass.Group.Download);
        public static string DownloadFolderName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadFolderName, "{yy}_{MM}_{dd}", CoreConfigClass.Group.Download);
        public static string TmpPath = CoreConfig.GetValue(CoreConfigClass.Key.TmpPath, "./tmp/", CoreConfigClass.Group.Download);
        public static int RecQuality = int.Parse(CoreConfig.GetValue(CoreConfigClass.Key.RecQuality, "10000", CoreConfigClass.Group.Download));
        public static bool IsRecDanmu = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecDanmu, "True", CoreConfigClass.Group.Download));
        public static bool IsRecGift = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecGift, "True", CoreConfigClass.Group.Download));
        public static bool IsRecGuard = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecGuard, "True", CoreConfigClass.Group.Download));
        public static bool IsRecSC = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsRecSC, "True", CoreConfigClass.Group.Download));
        public static bool IsFlvSplit = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsFlvSplit, "False", CoreConfigClass.Group.Download));
        public static long FlvSplitSize = long.Parse(CoreConfig.GetValue(CoreConfigClass.Key.FlvSplitSize, "1073741824", CoreConfigClass.Group.Download));

        /// <summary>
        /// 下载完成事件
        /// </summary>
        public static event EventHandler<EventArgs> DownloadCompleted;
        /// <summary>
        /// 增加下载任务
        /// </summary>
        public static void AddDownloadTaskd(long uid, bool IsNewTask = false, bool IsHLS = true)
        {
            Task.Run(() =>
            {
                if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    if (IsNewTask || !roomInfo.IsUserCancel)
                    {
                        roomInfo.IsUserCancel = false;
                        if(IsHLS)
                        {
                            AddDownLoad_HLS(uid, IsNewTask);
                        }
                        else
                        {
                            AddDownLoad_FLV(uid, IsNewTask);
                        }
                    }
                    else
                    {
                        if (!IsNewTask)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"增加下载任务失败，原因：用户取消下载，并且不是新任务");
                            if (IsHLS)
                            {
                                DownloadCompleteTaskd_HLS(uid, roomInfo.DownloadingList[roomInfo.DownloadingList.Count()-1]);
                            }
                            else
                            {
                                DownloadCompleteTaskd_FLV(uid, IsFlvSplit);
                            }
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"增加下载任务失败，原因：用户取消下载，本任务是新任务");
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
                    if (!roomInfo.IsUserCancel)
                    {
                        roomInfo.IsUserCancel = true;
                        foreach (var item in roomInfo.DownloadingList)
                        {
                            WebHook.SendHook(WebHook.HookType.CancelRec, uid);
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"取消【{roomInfo.uname}({roomInfo.room_id})】的下载任务");
                            item.Cancel();
                            return true;
                        }
                    }
                    else
                    {
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
        /// 下载任务结束处理(HLS任务)
        /// </summary>
        /// <param name="IsCancel">该任务是否已经取消</param>
        internal static void DownloadCompleteTaskd_HLS(long uid, DownloadClass.Downloads downloadClass, bool IsCancel = false)
        {
            try
            {
                if (!Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Warn, $"录制结束操作出现异常：该UID:{uid}不存在于本地监控列表中");
                }
                else
                {
                    Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"开始执行[{roomInfo.uname}({roomInfo.room_id})]直播间的下载任务结束处理任务");
                    if (DownloadPath.Substring(DownloadPath.Length - 1, 1) != "/")
                        DownloadPath = DownloadPath + "/";
                    string OkFileName = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadPath}" + $"{DownloadDirectoryName}" + $"/{DownloadFolderName}/" + $"{DownloadFileName}" + "_{R}.mp4");

                    //弹幕录制结束处理
                    if (bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)) && roomInfo.roomWebSocket.IsConnect)
                    {
                        roomInfo.roomWebSocket.IsConnect = false;
                        if (roomInfo.roomWebSocket.LiveChatListener != null)
                        {
                            try
                            {
                                roomInfo.roomWebSocket.LiveChatListener.IsUserDispose = true;
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】的直播已结束，LiveChat连接中断，储存相关数据");
                                roomInfo.roomWebSocket.LiveChatListener.startIn = false;
                                roomInfo.DanmuFile.TimeStopwatch.Stop();
                                roomInfo.roomWebSocket.LiveChatListener.Dispose();
                                BilibiliModule.API.DanMu.DanMuRec.SevaDanmuFile(roomInfo);
                            }
                            catch (Exception e)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"【{roomInfo.uname}({roomInfo.room_id})】结束LiveChat连接时发生未知错误", true, e);
                            }
                        }
                    }
                    else
                    {
                        if (!bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)))
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到IsAutoRec为false，将不会储存弹幕相关数据");
                        }
                        if (!roomInfo.roomWebSocket.IsConnect)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到WebSocket未连接，将不会储存弹幕相关数据");
                        }

                    }

                    //转码相关操作
                    //Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】直播结束。开始HLS转码。");
                    //if (downloadClass.HLSRecorded.Count() > 2)
                    //{
                    //    Tool.TranscodModule.TranscodClass transcodClass = new Tool.TranscodModule.TranscodClass()
                    //    {
                    //        AfterFilenameExtension = ".mp4",
                    //        BeforeFilePath = downloadClass.FilePath + downloadClass.HLSRecorded[0],
                    //        AfterFilePath = downloadClass.FilePath + Tool.FileOperation.CheckFilenames(downloadClass.Title)+$"_{DateTime.Now.ToString("HHmmss")}",
                    //        HLS_Files = downloadClass.HLSRecorded,
                    //    };
                    //    try
                    //    {
                    //        do
                    //        {
                    //            Thread.Sleep(100);
                    //            transcodClass.AfterFilePath = downloadClass.FilePath + Tool.FileOperation.CheckFilenames(downloadClass.Title) + $"_{DateTime.Now.ToString("HHmmss")}";
                    //        } while (File.Exists(transcodClass.AfterFilePath + transcodClass.AfterFilenameExtension));
                    //    }
                    //    catch (Exception)
                    //    {}
                    //    //var tm = Tool.TranscodModule.Transcod.CallFFMPEG_HLS(transcodClass);
                    //    var tm = Tool.HlsModule.Sun.HLS_SUN(transcodClass);
                    //    roomInfo.DownloadedFileInfo.Mp4File = new FileInfo(tm.AfterFilePath);
                    //    WebHook.SendHook(WebHook.HookType.TranscodingComplete, uid);
                    //}
                    //else
                    //{
                    //    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】合并返回的flv文件数量为0，放弃转码，保留原始数据或环境");
                    //}

                    if (!IsCancel)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"开始处理下播对象[{roomInfo.uname}({roomInfo.room_id})]（直播列表）→（历史列表）");
                        DownloadClass.Downloads downloads = new DownloadClass.Downloads();
                        DateTime StartTime = DateTime.MaxValue;
                        DateTime EndTime = DateTime.MinValue;
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载任务List长度为{roomInfo.DownloadingList.Count}");
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
                            downloads.FileName = OkFileName;
                            downloads.HLSRecorded = new List<string>();
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
                            roomInfo.DownloadedLog.Add(downloads);
                        }
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载任务添加到历史任务完成");


                        if (DownloadCompleted == null)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件为空！");
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件触发");
                            DownloadCompleted.Invoke(downloads, EventArgs.Empty);
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件结束");
                        }
                        #region 房间Shell命令
                        if (!string.IsNullOrEmpty(roomInfo.Shell))
                        {
                            Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间开始执行Shell命令:" + roomInfo.Shell, false, null, false);
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
                        WebHook.SendHook(WebHook.HookType.DownloadEndMissionSuccess, uid);
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
                    else
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"录制结束操作：UID:{uid}的任务触发IsCancel操作，跳过任务结束操作操作");
                    }
                    if (!IsCancel)
                    {
                        roomInfo.live_status = 0;
                    }
                    roomInfo.DownloadingList = new List<DownloadClass.Downloads>();
                    //任务结束流程完成
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"录制结束操作出现重大错误！详细Exception已写到TXT文本。", true, e, true);
            }
        }


        /// <summary>
        /// 下载任务结束处理(FLV任务)
        /// </summary>
        /// <param name="uid">用户UID</param>
        /// <param name="Split">任务是否切片(当自动切片使能时，FLV文件合并功能会跳过)</param>
        /// <param name="IsCancel">该任务是否已经取消</param>
        internal static void DownloadCompleteTaskd_FLV(long uid, bool Split = false, bool IsCancel = false)
        {
            try
            {
                bool IsSun = true, IsTranscod = Tool.TranscodModule.Transcod.IsAutoTranscod;
                if (IsCancel)
                {
                    IsSun = false;
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
                    string OkFileName = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadPath}" + $"{DownloadDirectoryName}" + $"/{DownloadFolderName}/" + $"{DownloadFileName}" + "_{R}.flv");

                    //弹幕录制结束处理
                    if (bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)) && roomInfo.roomWebSocket.IsConnect)
                    {
                        roomInfo.roomWebSocket.IsConnect = false;
                        if (roomInfo.roomWebSocket.LiveChatListener != null)
                        {
                            try
                            {
                                roomInfo.roomWebSocket.LiveChatListener.IsUserDispose = true;
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】的直播已结束，LiveChat连接中断，储存相关数据");
                                roomInfo.roomWebSocket.LiveChatListener.startIn = false;
                                roomInfo.DanmuFile.TimeStopwatch.Stop();
                                roomInfo.roomWebSocket.LiveChatListener.Dispose();
                                BilibiliModule.API.DanMu.DanMuRec.SevaDanmuFile(roomInfo);
                            }
                            catch (Exception e)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"【{roomInfo.uname}({roomInfo.room_id})】结束LiveChat连接时发生未知错误", true, e);
                            }
                        }
                    }
                    else
                    {
                        if (!bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)))
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到IsAutoRec为false，将不会储存弹幕相关数据");
                        }
                        if (!roomInfo.roomWebSocket.IsConnect)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到WebSocket未连接，将不会储存弹幕相关数据");
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
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】录制任务结束，开始检测并合并flv文件");
                        string SunFileName = Tool.FlvModule.Sum.FLV.FlvFileSum(roomInfo, OkFileName);
                        if (!string.IsNullOrEmpty(SunFileName))
                        {
                            roomInfo.DownloadedFileInfo.FlvFile = new FileInfo(SunFileName);
                            FileList.Add(SunFileName);
                        }
                    }
                    //转码
                    if (IsTranscod)
                    {
                        foreach (var DLIST in roomInfo.DownloadingList)
                        {
                            foreach (var item in DLIST.FlvFileList)
                            {
                                if (File.Exists(item) && !FileList.Contains(item))
                                //if (File.Exists(item))
                                {
                                    FileList.Add((string)item);
                                }
                            }
                        }
                        if (FileList.Count > 0)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】直播结束。开始转码，待转码文件数：{FileList.Count}");
                            foreach (var item in FileList)
                            {
                                if (!string.IsNullOrEmpty(item) && File.Exists(item))
                                {

                                    var tm = Tool.TranscodModule.Transcod.CallFFMPEG_FLV(new Tool.TranscodModule.TranscodClass()
                                    {
                                        AfterFilenameExtension = ".mp4",
                                        BeforeFilePath = item,
                                        AfterFilePath = item,
                                    });
                                    roomInfo.DownloadedFileInfo.Mp4File = new FileInfo(tm.AfterFilePath);
                                    WebHook.SendHook(WebHook.HookType.TranscodingComplete, uid);
                                }
                                else
                                {
                                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】转码文件：[{item}]不存在！");
                                }
                            }
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】转码任务已全部完成，转码文件数：${FileList.Count}");
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】合并返回的flv文件数量为0，放弃转码，保留原始数据或环境");
                        }
                    }
                    if (!IsCancel)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"开始处理下播对象[{roomInfo.uname}({roomInfo.room_id})]（直播列表）→（历史列表）");
                        DownloadClass.Downloads downloads = new DownloadClass.Downloads();
                        DateTime StartTime = DateTime.MaxValue;
                        DateTime EndTime = DateTime.MinValue;
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载任务List长度为{roomInfo.DownloadingList.Count}");
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
                            roomInfo.DownloadedLog.Add(downloads);
                        }
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载任务添加到历史任务完成");


                        if (DownloadCompleted == null)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件为空！");
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件触发");
                            DownloadCompleted.Invoke(downloads, EventArgs.Empty);
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件结束");
                        }
                        #region 房间Shell命令
                        if (!string.IsNullOrEmpty(roomInfo.Shell))
                        {
                            Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间开始执行Shell命令:" + roomInfo.Shell, false, null, false);
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
                        WebHook.SendHook(WebHook.HookType.DownloadEndMissionSuccess, uid);
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
                    else
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"录制结束操作：UID:{uid}的任务触发IsCancel操作，跳过任务结束操作操作");
                    }
                    if (!IsCancel)
                    {
                        roomInfo.live_status = 0;
                    }
                    roomInfo.DownloadingList = new List<DownloadClass.Downloads>();
                    //任务结束流程完成
                }
                else
                {
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Warn, $"录制结束操作出现异常：该UID:{uid}不存在于本地监控列表中");
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"录制结束操作出现重大错误！详细Exception已写到TXT文本。", true, e, true);
            }
        }
        private static void AddDownLoad_HLS(long uid, bool IsNewTask)
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
                    if (DL == null)
                    {
                        downloadClass.RoomId = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id);
                        downloadClass.Name = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname);
                        string WebText = DDTV_Core.SystemAssembly.NetworkRequestModule.Get.Get.GetRequest($"{DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.ReplaceAPI}/xlive/web-room/v2/index/getRoomPlayInfo?room_id={downloadClass.RoomId}&protocol=0,1&format=0,1,2&codec=0,1&qn={(int)RecQuality}&platform=h5&ptype=8");
                        ApiClass.BilibiliApiResponse<ApiClass.RoomPlayInfo> response = JsonConvert.DeserializeObject<ApiClass.BilibiliApiResponse<ApiClass.RoomPlayInfo>>(WebText);
                        if (response.Data.LiveStatus != 1)
                        {

                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"获取【{roomInfo.uname}({roomInfo.uid}:{roomInfo.room_id})】的直播流时发现直播间已经下播，任务结束");
                            return;
                        }
                        string host = "";
                        string base_url = "";
                        string extra = "";
                        foreach (var Stream in response.Data.PlayurlInfo.Playurl.Streams)
                        {
                            if (Stream.ProtocolName == "http_hls")
                            {
                                foreach (var Format in Stream.Formats)
                                {
                                    if (Format.FormatName.ToLower() == "fmp4")
                                    {
                                        host = Format.Codecs[0].UrlInfos[0].Host;
                                        extra = Format.Codecs[0].UrlInfos[0].Extra.Replace("\u0026", "&");
                                        base_url = Format.Codecs[0].BaseUrl;
                                        base_url = base_url.Replace(base_url.Split('/')[base_url.Split('/').Length - 1], "");
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(base_url) || string.IsNullOrEmpty(extra))
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.uid}:{roomInfo.room_id})】没有HLS直播流，降级到FLV");
                            AddDownloadTaskd(uid, IsNewTask, false);
                            return;
                        }
                        Rooms.RoomInfo[uid].DownloadingList.Add(downloadClass);
                        roomInfo.IsDownload = true;
                        downloadClass.Uid = uid;
                        downloadClass.IsDownloading = true;
                        downloadClass.Status = DownloadClass.Downloads.DownloadStatus.Standby;
                        roomInfo.CreationTime = DateTime.Now;
                        downloadClass.Title = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.title);
                        string StarText = $"\n({DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")})开始录制任务:\n=========HLS==========\n" +
                            $"直播间:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}\n" +
                            $"UID:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uid)}\n" +
                            $"昵称:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)}\n" +
                            $"标题:{downloadClass.Title}\n" +
                            $"=========HLS==========";
                        Console.WriteLine(StarText);
                        downloadClass.IsHLS = true;
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, StarText.Replace("\n", "　"), false, null, false);
                        if (DownloadPath.Substring(DownloadPath.Length - 1, 1) != "/")
                            DownloadPath = DownloadPath + "/";
                        string Path = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadPath}{DownloadDirectoryName}");
                        string FileName = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadFileName}" + "_{R}");
                        //执行下载任务
                        Path = Tool.FileOperation.CreateAll(Path + $"/{Tool.FileOperation.ReplaceKeyword(uid, DownloadFolderName)}/");
                        downloadClass.FilePath = Path;
                        downloadClass.FileName = Path;
                        downloadClass.FlvSplit = IsFlvSplit;
                        downloadClass.FlvSplitSize = FlvSplitSize;
                       
                        if(downloadClass.Download_m4s(downloadClass, roomInfo, Path, FileName, host, base_url, extra, downloadClass.HLSRecorded))
                        {
                            if (!downloadClass.GetCancelState())
                            {
                                DownloadCompleteTaskd_HLS(uid, downloadClass);
                            }
                            return;
                        }
                        else
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            AddDownloadTaskd(uid, false, true);
                        }

                    }
                    else
                    {
                        if (DL != null)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】已有录制任务，放弃新建HLS任务");
                            return;
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】检测不存在，请检查提交的用户uid");
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"新建下载任务发生意料外的错误！", true, e);
            }
        }

        /// <summary>
        /// 增加FLV下载任务具体实现
        /// </summary>
        /// <param name="uid">需要下载的房间uid号</param>
        private static void AddDownLoad_FLV(long uid, bool IsNewTask)
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
                        downloadClass.Url = BilibiliModule.API.RoomInfo.GetPlayUrl(uid, (RoomInfoClass.Quality)RecQuality);
                        downloadClass.IsDownloading = true;
                        if (string.IsNullOrEmpty(downloadClass.Url))
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"获取【{roomInfo.uname}({roomInfo.uid}:{roomInfo.room_id})】的直播流时返回空内容，有可能直播间已经下播，开始重试任务确定状态");
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            DownloadCompleteTaskd_FLV(uid, IsFlvSplit);
                            return;
                        }
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(downloadClass.Url);
                        if (!DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.WhetherToEnableProxy)
                        {
                            req.Proxy = null;
                        }
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
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"收到【{roomInfo.uname}({roomInfo.room_id})】的下载请求，等待数据流中...");
                        int Ok = IsOk(roomInfo, downloadClass.Url);
                        if (Ok == 0)
                        {
                            roomInfo.CreationTime = DateTime.Now;
                            downloadClass.Title = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.title);
                            string StarText = $"\n({DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")})开始录制任务:\n=========FLV==========\n" +
                                $"直播间:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id)}\n" +
                                $"UID:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uid)}\n" +
                                $"昵称:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)}\n" +
                                $"标题:{downloadClass.Title}\n" +
                                $"=========FLV==========";
                            Console.WriteLine(StarText);
                            downloadClass.IsHLS = false;
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, StarText.Replace("\n", "　"), false, null, false);
                            if (DownloadPath.Substring(DownloadPath.Length - 1, 1) != "/")
                                DownloadPath = DownloadPath + "/";
                            string Path = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadPath}{DownloadDirectoryName}");
                            string FileName = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadFileName}" + "_{R}");
                            //执行下载任务
                            downloadClass.FilePath = Path;
                            downloadClass.FlvSplit = IsFlvSplit;
                            downloadClass.FlvSplitSize = FlvSplitSize;
                            Tool.FileOperation.CreateAll(Path + $"/{Tool.FileOperation.ReplaceKeyword(uid, DownloadFolderName)}");
                            downloadClass.DownFLV_HttpWebRequest(downloadClass, req, Path, FileName, "flv", roomInfo);
                            //录制弹幕(是否是新任务 && 弹幕录制总开关 && 房间弹幕录制设置)
                            bool RoomIsRecDanmu = false;
                            bool.TryParse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsRecDanmu), out RoomIsRecDanmu);
                            if (IsNewTask && IsRecDanmu && RoomIsRecDanmu)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】弹幕录制请求已发出");
                                roomInfo.DanmuFile = new();
                                roomInfo.DanmuFile.FileName = Path + $"/{Tool.FileOperation.ReplaceKeyword(uid, DownloadFolderName)}/" + FileName;
                                BilibiliModule.API.DanMu.DanMuRec.Rec(uid);
                            }
                            else
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】FLV录制任务不进行弹幕录制，理由：是否为重连任务:{!IsNewTask},弹幕总开关:{IsRecDanmu},房间弹幕录制设置:{RoomIsRecDanmu}");
                            }
                        }
                        else if (Ok == 1)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】已下播，FLV录制任务取消");
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            DownloadCompleteTaskd_FLV(uid, IsFlvSplit);
                            return;
                        }
                        else if (Ok == -2)
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            DownloadCompleteTaskd_FLV(uid, IsFlvSplit);
                            return;
                        }
                        else
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            AddDownloadTaskd(uid, IsNewTask);
                        }
                    }
                    else
                    {
                        if (DL != null)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】已有录制任务，放弃新建FLV任务");
                            return;
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】检测不存在，请检查提交的用户uid");
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
            if (string.IsNullOrEmpty(Url))
            {
                return -1;
            }
            int conut = 0;
            while (true)
            {
                if (roomInfo.IsUserCancel)
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
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info_API, $"房间【{roomInfo.uname}({roomInfo.room_id})】已开播，但未监测到推流数据，3秒后重试");
                    }
                    if (conut > 6)
                    {
                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info_API, $"【{roomInfo.uname}({roomInfo.room_id})】请求网络文件超时，等待任务自动重置..");
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
