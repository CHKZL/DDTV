using DDTV_Core.SystemAssembly.BilibiliModule.API.HLS;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public static bool IsHls = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsHls, "True", CoreConfigClass.Group.Download));
        public static int WaitHLSTime = int.Parse(CoreConfig.GetValue(CoreConfigClass.Key.WaitHLSTime, "15", CoreConfigClass.Group.Download));

        /// <summary>
        /// 下载完成事件
        /// </summary>
        public static event EventHandler<EventArgs> DownloadCompleted;
        /// <summary>
        /// 增加视频下载任务
        /// </summary>
        public static void AddVideoDownloadTaskd(long uid, bool IsNewTask = false, bool IsHLS = true)
        {
            Task.Run(() =>
            {
                if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    string is_sp = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.is_sp);
                    if (is_sp == "1")
                    {
                         AddVideoDownLoad_FLV(uid, IsNewTask);
                    }
                    else
                    {
                        if (roomInfo.IsUserCancel)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"增加下载任务失败，原因：用户取消下载。该任务是否为新任务：[{(IsNewTask ? "是" : "否")}】");
                            if (IsHLS)
                            {
                                VideoDownloadCompleteTaskd_HLS(uid, roomInfo.DownloadingList[roomInfo.DownloadingList.Count() - 1]);
                            }
                            else
                            {
                                VideoDownloadCompleteTaskd_FLV(uid, IsFlvSplit);
                            }
                        }
                        else
                        {
                            if (!IsNewTask)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"{roomInfo.uname}(房间号:{roomInfo.room_id},UID:{roomInfo.uid})因为服务器原因连接中断，尝试进行重连...");
                            }
                            roomInfo.IsUserCancel = false;
                            if (IsHLS && DDTV_Core.SystemAssembly.DownloadModule.Download.IsHls)
                            {
                                AddVideoDownLoad_HLS(uid, IsNewTask);
                            }
                            else
                            {
                                AddVideoDownLoad_FLV(uid, IsNewTask);
                            }
                        }
                    }
                }
                else
                {
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Warn, $"[UID:{uid}]增加下载任务失败，原因：roomInfo不存在");
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
        internal static void VideoDownloadCompleteTaskd_HLS(long uid, DownloadClass.Downloads downloadClass, bool IsCancel = false)
        {
            try
            {
                if (!Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
                {
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Warn, $"录制结束操作出现异常：该UID:{uid}不存在于本地监控列表中");
                }
                else
                {
                    roomInfo.IsUserCancel = false;
                    downloadClass.EndTime = DateTime.Now;
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"开始执行[{roomInfo.uname}({roomInfo.room_id})]直播间的下载任务结束处理任务");
                    if (DownloadPath.Substring(DownloadPath.Length - 1, 1) != "/")
                        DownloadPath = DownloadPath + "/";
                    string OkFileName = Tool.FileOperation.ReplaceKeyword(uid, $"{DownloadPath}" + $"{DownloadDirectoryName}" + $"/{DownloadFolderName}/" + $"{DownloadFileName}" + "_{R}.mp4");

                    //弹幕录制结束处理
                    if (bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsRecDanmu)) && roomInfo.roomWebSocket.IsConnect)
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
                                Tool.DanMuKu.DanMuKuRec.SevaDanmuFile(roomInfo);
                                if (IsRecDanmu && CoreConfig.IsXmlToAss)
                                {
                                    try
                                    {
                                        Tool.DanMuKu.DanMuKuRec.CallDanmakuFactory(downloadClass.FilePath, roomInfo.DownloadedFileInfo.DanMuFile.Name.Replace(".xml", "_fix.ass"), roomInfo.DownloadedFileInfo.DanMuFile.Name);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Warn, $"【{roomInfo.uname}({roomInfo.room_id})】转换xml弹幕文件为ass文件时发生错误", true, e);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Error, $"【{roomInfo.uname}({roomInfo.room_id})】结束LiveChat连接时发生未知错误", true, e);
                            }
                        }
                    }
                    else
                    {
                        if (!bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)) && IsRecDanmu)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到IsAutoRec或IsRecDanmu为false，将不会储存弹幕相关数据");
                        }
                        if (!roomInfo.roomWebSocket.IsConnect)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到WebSocket未连接，将不会储存弹幕相关数据");
                        }

                    }
                    //转码

                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】直播结束。检并修复可能存在的错误。");
                    foreach (var item in roomInfo.Files)
                    {
                        if (!item.IsTranscod && !string.IsNullOrEmpty(item.FilePath) && File.Exists(item.FilePath))
                        {
                            var tm = Tool.TranscodModule.Transcod.CallFFMPEG(new Tool.TranscodModule.TranscodClass()
                            {
                                AfterFilenameExtension = ".mp4",
                                BeforeFilePath = item.FilePath,
                                AfterFilePath = item.FilePath.Replace(".mp4", "_fix.mp4").Replace(".flv", "_fix.mp4"),
                            });
                            item.IsTranscod = true;
                            roomInfo.DownloadedFileInfo.AfterRepairFiles.Add(new FileInfo(tm.AfterFilePath));
                            WebHook.SendHook(WebHook.HookType.TranscodingComplete, uid);
                        }
                        else
                        {
                            if (!item.IsTranscod)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】Fix文件：[{item.FilePath}]不存在！");
                            }
                        }
                    }
                    Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】修复任务已全部完成");
                    if (!IsCancel)
                    {
                        //Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"开始处理下播对象[{roomInfo.uname}({roomInfo.room_id})]（直播列表）→（历史列表）");
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
                            downloads.FileName = item.FileName;
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
                        }
                        if (downloads.TotalDownloadCount > 100)
                        {
                            roomInfo.DownloadedLog.Add(downloads);
                        }
                        //Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载任务添加到历史任务完成");


                        if (DownloadCompleted != null)
                        {
                            // Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件触发");
                            DownloadCompleted.Invoke(new DownloadClass.Downloads() { Title = roomInfo.title, Name = roomInfo.uname }, EventArgs.Empty);
                            //Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件结束");
                        }
                        else
                        {
                            if (DDTV_Core.InitDDTV_Core.InitType == InitDDTV_Core.SatrtType.DDTV_GUI)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件为空！");
                            }
                        }
                        #region 房间Shell命令
                        if (!string.IsNullOrEmpty(roomInfo.Shell))
                        {
                            Task.Run(() =>
                            {
                                WebHook.SendHook(WebHook.HookType.RunShellComplete, roomInfo.uid);
                                try
                                {
                                    string Shell = Tool.FileOperation.ReplaceKeyword(uid, roomInfo.Shell);
                                    Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间开始执行Shell命令:" + Shell, false, null, false);
                                    string result = Tool.SystemResource.ExternalCommand.Shell(Shell);
                                    Console.WriteLine($"{roomInfo.uname}直播间的Shell命令执行完成，执行返回结果:\n{result}");
                                    Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间执行Shell命令结束，返回信息:{result}", false, null, false);
                                    roomInfo.DownloadedFileInfo.AfterRepairFiles.Clear();
                                    roomInfo.DownloadedFileInfo.BeforeRepairFiles.Clear();
                                }
                                catch (Exception e)
                                {
                                    Log.Log.AddLog("Shell", Log.LogClass.LogType.Error, $"{roomInfo.uname}({roomInfo.room_id})直播间执行Shell命令失败！详细堆栈信息已写入日志。", true, e, true);
                                    roomInfo.DownloadedFileInfo.AfterRepairFiles.Clear();
                                    roomInfo.DownloadedFileInfo.BeforeRepairFiles.Clear();
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
                        Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                        if (string.IsNullOrEmpty(roomInfo.Shell))
                        {
                            roomInfo.DownloadedFileInfo.AfterRepairFiles.Clear();
                            roomInfo.DownloadedFileInfo.BeforeRepairFiles.Clear();
                        }
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
        internal static void VideoDownloadCompleteTaskd_FLV(long uid, bool Split = false, bool IsCancel = false)
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
                    if (bool.Parse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsRecDanmu)) && roomInfo.roomWebSocket.IsConnect)
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
                                Tool.DanMuKu.DanMuKuRec.SevaDanmuFile(roomInfo);
                                if (IsRecDanmu && CoreConfig.IsXmlToAss)
                                {
                                    try
                                    {
                                        Tool.DanMuKu.DanMuKuRec.CallDanmakuFactory(roomInfo.DownloadingList[roomInfo.DownloadingList.Count - 1].FilePath, roomInfo.DownloadedFileInfo.DanMuFile.Name.Replace(".xml", "_fix.ass"), roomInfo.DownloadedFileInfo.DanMuFile.Name);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Warn, $"【{roomInfo.uname}({roomInfo.room_id})】转换xml弹幕文件为ass文件时发生错误", true, e);
                                    }
                                }
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
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.room_id})】的直播结束，检测到IsAutoRec或IsRecDanmu为false，将不会储存弹幕相关数据");
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
                            roomInfo.DownloadedFileInfo.BeforeRepairFiles.Add(new FileInfo(SunFileName));
                            FileList.Add(SunFileName);
                        }
                    }
                    //转码
                    if (IsTranscod)
                    {
                        foreach (var file in roomInfo.Files)
                        {
                            if (!file.IsTranscod && File.Exists(file.FilePath) && !FileList.Contains(file.FilePath))
                            {
                                file.IsTranscod = true;
                                FileList.Add(file.FilePath);
                            }
                        }
                        if (FileList.Count > 0)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"直播间【{roomInfo.uname}({roomInfo.room_id})】直播结束。开始转码，待转码文件数：{FileList.Count}");
                            foreach (var item in FileList)
                            {
                                if (!string.IsNullOrEmpty(item) && File.Exists(item))
                                {

                                    var tm = Tool.TranscodModule.Transcod.CallFFMPEG(new Tool.TranscodModule.TranscodClass()
                                    {
                                        AfterFilenameExtension = ".mp4",
                                        BeforeFilePath = item,
                                        AfterFilePath = item.Replace(".mp4", "_fix.mp4").Replace(".flv", "_fix.mp4"),
                                    });
                                    roomInfo.DownloadedFileInfo.AfterRepairFiles.Add(new FileInfo(tm.AfterFilePath));
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
                        //Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"开始处理下播对象[{roomInfo.uname}({roomInfo.room_id})]（直播列表）→（历史列表）");
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
                            downloads.Title = item.Title;
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
                        if (downloads.TotalDownloadCount > 100)
                        {
                            roomInfo.DownloadedLog.Add(downloads);
                        }

                        Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载任务添加到历史任务完成");


                        if (DownloadCompleted != null)
                        {
                            // Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件触发");
                            DownloadCompleted.Invoke(new DownloadClass.Downloads() { Title = roomInfo.title, Name = roomInfo.uname }, EventArgs.Empty);
                            //Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件结束");

                        }
                        else
                        {
                            if (DDTV_Core.InitDDTV_Core.InitType == InitDDTV_Core.SatrtType.DDTV_GUI)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Debug, $"[{roomInfo.uname}({roomInfo.room_id})]下载完成事件为空！");
                            }
                        }
                        #region 房间Shell命令
                        if (!string.IsNullOrEmpty(roomInfo.Shell))
                        {

                            Task.Run(() =>
                            {
                                WebHook.SendHook(WebHook.HookType.RunShellComplete, roomInfo.uid);
                                try
                                {
                                    string Shell = Tool.FileOperation.ReplaceKeyword(uid, roomInfo.Shell);
                                    Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间开始执行Shell命令:" + Shell, false, null, false);
                                    string result = Tool.SystemResource.ExternalCommand.Shell(Shell);
                                    Console.WriteLine($"{roomInfo.uname}直播间的Shell命令执行完成，执行返回结果:\n{result}");
                                    Log.Log.AddLog("Shell", Log.LogClass.LogType.Info, $"{roomInfo.uname}({roomInfo.room_id})直播间执行Shell命令结束，返回信息:{result}", false, null, false);
                                    roomInfo.DownloadedFileInfo.AfterRepairFiles.Clear();
                                    roomInfo.DownloadedFileInfo.BeforeRepairFiles.Clear();
                                }
                                catch (Exception e)
                                {
                                    Log.Log.AddLog("Shell", Log.LogClass.LogType.Error, $"{roomInfo.uname}({roomInfo.room_id})直播间执行Shell命令失败！详细堆栈信息已写入日志。", true, e, true);
                                    roomInfo.DownloadedFileInfo.AfterRepairFiles.Clear();
                                    roomInfo.DownloadedFileInfo.BeforeRepairFiles.Clear();
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
                        if (string.IsNullOrEmpty(roomInfo.Shell))
                        {
                            roomInfo.DownloadedFileInfo.AfterRepairFiles.Clear();
                            roomInfo.DownloadedFileInfo.BeforeRepairFiles.Clear();
                        }
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


        /// <summary>
        /// 增加HLS下载任务具体实现
        /// </summary>
        /// <param name="uid">需要下载的房间uid号</param>
        /// <param name="IsNewTask">是否为新任务</param>
        private static void AddVideoDownLoad_HLS(long uid, bool IsNewTask)
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
                        downloadClass.Uid = uid;
                        downloadClass.IsDownloading = true;
                        downloadClass.Status = DownloadClass.Downloads.DownloadStatus.Standby;
                        roomInfo.CreationTime = DateTime.Now;
                        Rooms.RoomInfo[uid].DownloadingList.Add(downloadClass);
                        downloadClass.RoomId = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id);
                        downloadClass.Name = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname);
                        HLS_Host.HLSHostClass hLSHostClass = HLS_Host.Get_HLS_Host(ref roomInfo, ref downloadClass, IsNewTask);
                        if (!hLSHostClass.LiveStatus)
                        {
                            VideoDownloadCompleteTaskd_HLS(uid, downloadClass);
                            return;
                        }
                        if (hLSHostClass.IsUserCancel)
                        {
                            VideoDownloadCompleteTaskd_HLS(uid, downloadClass, true);
                            return;
                        }
                        if (!hLSHostClass.IsEffective)
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.uid}:{roomInfo.room_id})】没有检测到HLS直播流，降级到FLV");
                            AddVideoDownloadTaskd(uid, IsNewTask, false);
                            return;
                        }
                        roomInfo.IsDownload = true;

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

                        //弹幕录制
                        bool.TryParse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsRecDanmu), out bool RoomIsRecDanmu);
                        if (IsNewTask && IsRecDanmu && RoomIsRecDanmu)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】弹幕录制请求已发出");
                            roomInfo.DanmuFile = new();
                            roomInfo.DanmuFile.FileName = Path + FileName;
                            Tool.DanMuKu.DanMuKuRec.Rec(uid);
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】HLS录制任务不进行弹幕录制，判断依据：是否为重连任务:[{(IsNewTask ? "否" : "是")}],弹幕总开关:[{(IsRecDanmu ? "是" : "否")}],房间弹幕录制设置:[{(RoomIsRecDanmu ? "是" : "否")}]");
                        }

                        switch (downloadClass.Download_HLS(ref downloadClass, ref roomInfo, Path, FileName, hLSHostClass, downloadClass.HLSRecorded, downloadClass.ExtendedName))
                        {
                            case -1:
                                Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                                AddVideoDownloadTaskd(uid, false, false);
                                break;
                            case 0:
                                if (!downloadClass.GetCancelState())
                                {
                                    VideoDownloadCompleteTaskd_HLS(uid, downloadClass);
                                }
                                return;
                            case 1:
                                Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                                AddVideoDownloadTaskd(uid, false, true);
                                break;
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
        /// <param name="IsNewTask">是否为新任务</param>
        private static void AddVideoDownLoad_FLV(long uid, bool IsNewTask)
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
                            VideoDownloadCompleteTaskd_FLV(uid, IsFlvSplit);
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
                            if (roomInfo.Host.Substring(0, 1) == "[")
                            {
                                if (roomInfo.Host.Substring(0, 4) == "[HLS]")
                                {
                                    roomInfo.Host.Replace("[HLS]", "[FLV]");
                                }
                            }
                            else
                            {
                                roomInfo.Host = "[FLV] " + roomInfo.Host;
                            }
                            roomInfo.CurrentMode = 0;

                            Console.WriteLine(StarText);
                            downloadClass.IsHLS = false;
                            downloadClass.ExtendedName = "flv";
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
                            downloadClass.DownFLV_HttpWebRequest(downloadClass, req, Path, FileName, downloadClass.ExtendedName, roomInfo);
                            //录制弹幕(是否是新任务 && 弹幕录制总开关 && 房间弹幕录制设置)
                            bool RoomIsRecDanmu = false;
                            bool.TryParse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.IsRecDanmu), out RoomIsRecDanmu);
                            if (IsNewTask && IsRecDanmu && RoomIsRecDanmu)
                            {
                                Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.room_id})】弹幕录制请求已发出");
                                roomInfo.DanmuFile = new();
                                roomInfo.DanmuFile.FileName = Path + $"/{Tool.FileOperation.ReplaceKeyword(uid, DownloadFolderName)}/" + FileName;
                                Tool.DanMuKu.DanMuKuRec.Rec(uid);
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
                            VideoDownloadCompleteTaskd_FLV(uid, IsFlvSplit);
                            return;
                        }
                        else if (Ok == -2)
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            VideoDownloadCompleteTaskd_FLV(uid, IsFlvSplit);
                            return;
                        }
                        else
                        {
                            Rooms.RoomInfo[uid].DownloadingList.Remove(downloadClass);
                            AddVideoDownloadTaskd(uid, IsNewTask);
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
        internal static int IsOk(RoomInfoClass.RoomInfo roomInfo, string Url)
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
                        if (DDTV_Core.InitDDTV_Core.IsDevDebug)
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info_API, $"房间【{roomInfo.uname}({roomInfo.room_id})】已开播，但未监测到推流数据，3秒后重试:({Url})");
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info_API, $"房间【{roomInfo.uname}({roomInfo.room_id})】已开播，但未监测到推流数据，3秒后重试");
                        }

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
