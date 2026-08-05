using AngleSharp.Dom;
using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.Download.Basics;
using static Core.RuntimeObject.Download.Basics.HostClass.EXTM3U;

namespace Core.RuntimeObject.Download
{
    public class HLS
    {
        /// <summary>
        /// 录制HLS_avc制式的MP4文件
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        /// <param name="Reconnection">是否为重连</param>
        /// <returns>[TaskStatus]任务状态；[FileName]下载成功的文件名</returns>
        public static async Task<(DownloadTaskState hlsState, string FileName)> DlwnloadHls_avc_mp4(RoomCardClass card, bool Reconnection)
        {
            DownloadTaskState hlsState = DownloadTaskState.Default;
            string File = string.Empty;
            Stopwatch stopWatch = new Stopwatch();
            //使用LongRunning独立线程执行同步录制循环，避免长期独占线程池线程（每个录制房间1个）
            await Task.Factory.StartNew(() =>
            {
                InitializeDownload(card, RoomCardClass.TaskType.HLS_AVC);
                card.DownInfo.DownloadFileList.CurrentOperationVideoFile = string.Empty;
                long roomId = card.RoomId;


                //构建要传递给 ReplaceKeyword 的完整文件名模板
                //    这个模板包含了文件夹和文件名
                string fileTemplate = Path.Combine(
                    Config.Core_RunConfig._DefaultLiverFolderName,
                    Config.Core_RunConfig._DefaultDataFolderName,
                    Config.Core_RunConfig._DefaultFileName
                );
                // Path.Combine 会自动处理中间的分隔符，即使某些部分为空

                // 对这个模板应用关键词替换，生成处理后的文件名（含相对路径）
                string processedRelativePath = Core.Tools.KeyCharacterReplacement.ReplaceKeyword(fileTemplate, DateTime.Now, card.UID);
                // 将处理后的相对路径与根目录拼接，并添加后缀
                string finalFilePath = Path.Combine(Config.Core_RunConfig._RecFileDirectory, processedRelativePath) + "_original.mp4";



                //File = $"{Config.Core_RunConfig._RecFileDirectory}{Core.Tools.KeyCharacterReplacement.ReplaceKeyword($"{Config.Core_RunConfig._DefaultLiverFolderName}/{Core.Config.Core_RunConfig._DefaultDataFolderName}{(string.IsNullOrEmpty(Core.Config.Core_RunConfig._DefaultDataFolderName) ? "" : "/")}{Config.Core_RunConfig._DefaultFileName}", DateTime.Now, card.UID)}_original.mp4";

                File = finalFilePath.Replace("\\","/");
                CreateDirectoryIfNotExists(File.Substring(0, File.LastIndexOf('/')));
                Thread.Sleep(5);
                //本地Task下载的文件大小
                long DownloadFileSizeForThisTask = 0;
                using (FileStream fs = new FileStream(File, FileMode.Append))
                {

                    HostClass hostClass = new();
                    while (!GetHlsHost_avc(card, ref hostClass))
                    {
                        hlsState = HandleHlsError(card, hostClass);
                        if (!Reconnection && hlsState == DownloadTaskState.NoHLSStreamExists)//初次任务，等待HLS流生成，等待时间根据配置文件来
                        {
                            Thread.Sleep(Config.Core_RunConfig._HlsWaitingTime * 1000);
                        }
                        hlsState = HandleHlsError(card, hostClass);
                        switch (hlsState)
                        {
                            case DownloadTaskState.StopLive:
                                hlsState = CheckAndHandleFile(File, ref card);
                                return;
                            case DownloadTaskState.UserCancellation:
                                hlsState = CheckAndHandleFile(File, ref card);
                                return;
                            case DownloadTaskState.PaidLiveStream:
                                CheckAndHandleFile(File, ref card);
                                Log.Warn(nameof(HandleHlsError), $"[{card.Name}({card.RoomId})]直播间开播中，但直播间为收费直播间(大航海或者门票直播)，创建任务失败，跳过当前任务");
                                return;
                            case DownloadTaskState.NoHLSStreamExists:
                                CheckAndHandleFile(File, ref card);
                                Log.Info(nameof(HandleHlsError), $"[{card.Name}({card.RoomId})]直播间开播中，但没获取到HLS流，降级到FLV模式");
                                return;
                        }
                    }
                    //Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]开始监听重连");
                    List<(long size, DateTime time)> values = new();
                    bool InitialRequest = true;
                    long currentLocation = 0;
                    long StartLiveTime = card.live_time.Value;
                    string lastMapUri = string.Empty;

                    stopWatch.Start();
                    int RetryCount = 0;
                    long TrackWidth = 0;
                    long TrackHeight = 0;
                    long ReM3U8TimeCount = (long)stopWatch.Elapsed.TotalSeconds;
                    string originalTitle = card.Title.Value;
                    while (true)
                    {
                        //处理标题变更分割
                        if (Config.Core_RunConfig._SplitOnTitleChange && !string.IsNullOrEmpty(originalTitle) && originalTitle != card.Title.Value)
                        {
                            Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]检测到直播间标题变化[{originalTitle}]→[{card.Title.Value}]，进行切割处理");
                            hlsState = DownloadTaskState.Success;
                            return;
                        }

                        //处理大小限制分割
                        if (card.RoomCutAccordingToSize > 0 && DownloadFileSizeForThisTask > card.RoomCutAccordingToSize)
                        {
                            Log.Info(nameof(DlwnloadHls_avc_mp4), $"{card.Name}({card.RoomId})触发房间文件大小分割");
                            hlsState = DownloadTaskState.Success;
                            return;
                        }

                        if (card.RoomCutAccordingToSize == 0 && Config.Core_RunConfig._CutAccordingToSize > 0 && DownloadFileSizeForThisTask > Config.Core_RunConfig._CutAccordingToSize)
                        {
                            Log.Info(nameof(DlwnloadHls_avc_mp4), $"{card.Name}({card.RoomId})触发全局文件大小分割");
                            hlsState = DownloadTaskState.Success;
                            return;
                        }
                        //处理时间限制分割
                        if (card.RoomCutAccordingToTime > 0 && stopWatch.Elapsed.TotalSeconds > card.RoomCutAccordingToTime)
                        {
                            Log.Info(nameof(DlwnloadHls_avc_mp4), $"{card.Name}({card.RoomId})触发房间时间分割");
                            hlsState = DownloadTaskState.Success;
                            return;
                        }

                        if (card.RoomCutAccordingToTime == 0 && Config.Core_RunConfig._CutAccordingToTime > 0 && stopWatch.Elapsed.TotalSeconds > Config.Core_RunConfig._CutAccordingToTime)
                        {
                            Log.Info(nameof(DlwnloadHls_avc_mp4), $"{card.Name}({card.RoomId})触发全局时间分割");
                            hlsState = DownloadTaskState.Success;
                            return;
                        }

                        //本次循环下载的单体文件切片大小
                        long downloadSizeForThisCycle = 0;
                        try
                        {
                            if (ShouldFinalizeRecording(card, StartLiveTime))
                            {
                                hlsState = CheckAndHandleFile(File, ref card, card.live_time.Value != StartLiveTime ? true : false);
                                return;
                            }
                            //刷新Host信息，获取最新的直播流片段
                            bool isHlsHostAvailable = RefreshHlsHost_avc(card, ref hostClass);
                            if (!isHlsHostAvailable)
                            {
                                hlsState = HandleHostRefresh(card, ref hostClass);
                                switch (hlsState)
                                {
                                    case DownloadTaskState.StopLive:
                                        hlsState = CheckAndHandleFile(File, ref card);
                                        return;
                                    case DownloadTaskState.UserCancellation:
                                        hlsState = CheckAndHandleFile(File, ref card);
                                        return;
                                    case DownloadTaskState.Default:
                                        if (RetryCount > 5)
                                        {
                                            CheckAndHandleFile(File, ref card);
                                            hlsState = DownloadTaskState.NoHLSStreamExists;
                                        }
                                        RetryCount++;
                                        return;
                                }
                            }
                            else
                            {
                                if (InitialRequest)
                                {
                                    string DebugFile = string.Empty;
                                    //DebugFile = $"{hostClass.eXTM3U.eXTINFs[0].FileName+"_"+hostClass.eXTM3U.Map_URI.Replace(".m4s","_I.m4s")}";
                                    downloadSizeForThisCycle += WriteToFile(fs, $"{hostClass.host}{hostClass.base_url}{hostClass.eXTM3U.Map_URI}?{hostClass.extra}", DebugFile);
                                    lastMapUri = hostClass.eXTM3U.Map_URI;
                                }
                                try
                                {
                                    //if (stopWatch.Elapsed.TotalSeconds - ReM3U8TimeCount > 50)
                                    //{
                                    //    ReM3U8TimeCount = (long)stopWatch.Elapsed.TotalSeconds;
                                    //    GetHlsHost_avc(card, ref hostClass);
                                    //}

                                    string m4sUrl = $"{hostClass.host}{hostClass.base_url}{hostClass.eXTM3U.Map_URI}?{hostClass.extra}";
                                    byte[] m4sBytes = Network.Download.File.GetNetworkByte(m4sUrl, true, "https://www.bilibili.com/");
                                    //Log.Debug ("test", $"m4sUrl:{m4sUrl}");
                                    if (TryParseResolution(m4sBytes, out long temp_TrackWidth, out long temp_TrackHeight))
                                    {
                                        //Log.Debug("test", $"temp_TrackWidth:{temp_TrackWidth} temp_TrackHeight:{temp_TrackHeight} TrackWidth:{TrackWidth} TrackHeight:{TrackHeight}");
                                        if (InitialRequest)
                                        {
                                            TrackWidth = temp_TrackWidth;
                                            TrackHeight = temp_TrackHeight;
                                        }
                                        if (TrackWidth != 0 || TrackHeight != 0)
                                            if (temp_TrackWidth != 0 && temp_TrackHeight != 0)
                                                if (temp_TrackWidth != TrackWidth || temp_TrackHeight != TrackHeight)
                                                {
                                                    Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]检测到分辨率变化，进行切割处理");
                                                    hlsState = DownloadTaskState.Success;
                                                    return;
                                                }
                                    }
                                    else
                                    {
                                        //分辨率检测是可选增强逻辑，init segment 异常时仅跳过本轮检测，不影响录制
                                        string skipReason = m4sBytes == null
                                            ? "init segment下载失败(未获取到数据，可能为网络问题或CDN拒绝请求)"
                                            : m4sBytes.Length < 248
                                                ? $"init segment响应内容过短(仅{m4sBytes.Length}字节，可能为CDN错误页或响应被截断)"
                                                : "init segment内容不是合法的m4s文件(缺少ftyp文件头，可能为CDN错误页)";
                                        Log.Warn(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]本轮分辨率变化检测已跳过：{skipReason}，录制不受影响，下一轮将自动重试");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Warn(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]本轮分辨率变化检测已跳过：下载或解析init segment时发生异常({ex.Message})，录制不受影响", ex);
                                }

                                // 检测 init segment 是否变化（主播连麦/重新推流后 B站会发送新的 init segment）
                                if (!InitialRequest && !string.IsNullOrEmpty(lastMapUri) && hostClass.eXTM3U.Map_URI != lastMapUri)
                                {
                                    Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]检测到 init segment 变化（可能是主播连麦或重新推流），进行切割处理");
                                    hlsState = DownloadTaskState.Success;
                                    return;
                                }

                                if (currentLocation != 0 && Core.Config.Core_RunConfig._ReconnectAnchorReStream)
                                {
                                    //用于处理流悄悄切了，但是没有发现的情况
                                    bool FileNameTimeout = hostClass.eXTM3U.eXTINFs.All(extinf =>
                                    {
                                        if (long.TryParse(extinf.FileName, out long value))
                                        {
                                            return value != currentLocation;
                                        }
                                        return false;
                                    });
                                    if (FileNameTimeout)
                                    {
                                        //如果本次HLS获取到的内容前后都包含上一秒的切片，视为换了全新的切片队列
                                        hlsState = DownloadTaskState.AnchorReStream;
                                        return;
                                    }
                                }
                                foreach (var item in hostClass.eXTM3U.eXTINFs)
                                {
                                    if (long.TryParse(item.FileName, out long index) && (index > currentLocation || currentLocation == 0))
                                    {
                                        string DebugFile = string.Empty;
                                        //DebugFile = $"{item.FileName}_BP.m4s";
                                        //Log.Info("test",$"index:{index} currentLocation:{currentLocation}");
                                        downloadSizeForThisCycle += WriteToFile(fs, $"{hostClass.host}{hostClass.base_url}{item.FileName}.{item.ExtensionName}?{hostClass.extra}", DebugFile);
                                        currentLocation = index;
                                    }                                  
                                }
                                hostClass.eXTM3U.eXTINFs = new();
                                values.Add((downloadSizeForThisCycle, DateTime.Now));
                                //计算这个Task下载的文件大小
                                DownloadFileSizeForThisTask += downloadSizeForThisCycle;
                                //计算下载速度和任务大小
                                values = UpdateDownloadSpeed(values, card, downloadSizeForThisCycle);
                                if (hostClass.eXTM3U.IsEND)
                                {
                                    if (InitialRequest)
                                    {
                                        hlsState = CheckAndHandleFile(File, ref card);
                                        hlsState = DownloadTaskState.SuccessfulButNotStream;
                                        if (!card.DownInfo.Unmark && !card.DownInfo.IsCut)
                                        {
                                            CheckAndHandleFile(File, ref card);
                                            Thread.Sleep(1000 * 10);
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]录制任务收到END数据包，进行收尾处理");
                                        hlsState = DownloadTaskState.Success;
                                        if (!card.DownInfo.Unmark && !card.DownInfo.IsCut)
                                        {
                                            CheckAndHandleFile(File, ref card);
                                            Thread.Sleep(1000 * 10);
                                        }
                                        return;
                                    }
                                }
                                if (InitialRequest)
                                {
                                    //把当前写入文件写入记录
                                    string F_S = Config.Core_RunConfig._RecFileDirectory + (Config.Core_RunConfig._RecFileDirectory.EndsWith("/") || Config.Core_RunConfig._RecFileDirectory.EndsWith("\\") ? "" : "/") + fs.Name.Replace(new DirectoryInfo(Config.Core_RunConfig._RecFileDirectory).FullName, "").Replace("\\", "/");
                                    card.DownInfo.DownloadFileList.CurrentOperationVideoFile = F_S;
                                    Log.Debug("test", card.DownInfo.DownloadFileList.CurrentOperationVideoFile);
                                    //正式开始下载提示
                                    LogDownloadStart(card, "HLS");

                                    hlsState = DownloadTaskState.Recording;
                                }
                                InitialRequest = false;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]录制循环中出现未知错误，写入日志", e, true);
                            if (!card.DownInfo.Unmark && !card.DownInfo.IsCut)
                                Thread.Sleep(1000);
                            if (card.DownInfo.IsCut)
                                return;
                        }
                        if (!card.DownInfo.Unmark && !card.DownInfo.IsCut)
                            Thread.Sleep(1000);
                        if (card.DownInfo.IsCut)
                            return;
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            card.DownInfo.DownloadSize = 0;
            stopWatch.Stop();
            return (hlsState, File);
        }





        /// <summary>
        /// 从init segment(m4s)字节流中按固定偏移解析轨道分辨率(tkhd box，16.16定点数)
        /// </summary>
        /// <param name="m4sBytes">init segment字节内容</param>
        /// <param name="width">解析出的宽度</param>
        /// <param name="height">解析出的高度</param>
        /// <returns>是否解析成功；内容缺失、过短或不是合法m4s时返回false</returns>
        private static bool TryParseResolution(byte[] m4sBytes, out long width, out long height)
        {
            width = 0;
            height = 0;
            //分辨率字段位于固定偏移240~247，数组至少需要248字节
            if (m4sBytes == null || m4sBytes.Length < 248)
            {
                return false;
            }
            //校验m4s文件头(box size + "ftyp"标识)，防止把CDN错误页等内容当作init segment解析
            if (m4sBytes[4] != 'f' || m4sBytes[5] != 't' || m4sBytes[6] != 'y' || m4sBytes[7] != 'p')
            {
                return false;
            }
            width = (long)(m4sBytes[240] * 0x100 * 0x100 * 0x100 + m4sBytes[241] * 0x100 * 0x100 + m4sBytes[242] * 0x100 + m4sBytes[243]) / 65536;
            height = (long)(m4sBytes[244] * 0x100 * 0x100 * 0x100 + m4sBytes[245] * 0x100 * 0x100 + m4sBytes[246] * 0x100 + m4sBytes[247]) / 65536;
            return true;
        }

        /// <summary>
        /// 处理Host刷新
        /// </summary>
        /// <param name="card">房间信息</param>
        /// <param name="hostClass">HostClass实例</param>
        /// <returns>HLS错误计数</returns>
        private static DownloadTaskState HandleHostRefresh(RoomCardClass card, ref HostClass hostClass)
        {
            if (!GetHlsHost_avc(card, ref hostClass) && !RoomInfo.GetLiveStatus(card.RoomId))
            {
                Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]刷新Host时发现直播间已下播");
                return DownloadTaskState.StopLive;
            }
            if (card.DownInfo.Unmark)
            {
                return DownloadTaskState.UserCancellation;
            }
            Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]直播间未检测到直播流，3秒后重试");
            return DownloadTaskState.Default;
        }




        /// <summary>
        /// 处理HLS错误
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        /// <param name="hostClass">主播类</param>
        /// <returns>当前HLS状态</returns>
        private static DownloadTaskState HandleHlsError(RoomCardClass card, HostClass hostClass)
        {
            if (!RoomInfo.GetLiveStatus(card.RoomId))
            {
                return DownloadTaskState.StopLive;
            }

            if (card.DownInfo.Unmark)
            {
                return DownloadTaskState.UserCancellation;
            }
            //
            bool isPaidLiveStream = hostClass.all_special_types.Contains(1);
            string url = string.Empty;

            //是否为收费直播
            if (isPaidLiveStream)
            {
                //测是否有门票
                if (!GetHlsAvcUrl(card, Core.Config.Core_RunConfig._DefaultResolution, out url))
                {
                    //没门票
                    card.DownInfo.Status = RoomCardClass.DownloadStatus.Special;
                    return DownloadTaskState.PaidLiveStream;
                }
                //有门票
                if (!string.IsNullOrEmpty(url))
                {
                    Log.Info(nameof(HandleHlsError), $"[{card.Name}({card.RoomId})]检测到收费直播，但是好像有门票，继续尝试录制");
                }
            }
            //是否有HLS流
            if (hostClass.Effective)
            {
                card.DownInfo.Status = RoomCardClass.DownloadStatus.Downloading;
                return DownloadTaskState.Default;
            }
            else
            {
                card.DownInfo.Status = RoomCardClass.DownloadStatus.Standby;
                return DownloadTaskState.NoHLSStreamExists;
            }
        }


        /// <summary>
        /// 更新下载速度
        /// </summary>
        /// <param name="values">下载值列表</param>
        /// <param name="card">房间卡片信息</param>
        /// <param name="downloadSizeForThisCycle">本周期下载大小</param>
        /// <returns>更新后的下载值列表</returns>
        private static List<(long size, DateTime time)> UpdateDownloadSpeed(List<(long size, DateTime time)> values, RoomCardClass card, long downloadSizeForThisCycle)
        {
            while (values.Count >= 10)
            {
                values.RemoveAt(0);
            }
            card.DownInfo.RealTimeDownloadSpe = (values.Sum(x => x.size) / DateTime.Now.Subtract(values[0].time).TotalMilliseconds) * 1000;
            card.DownInfo.DownloadSize += downloadSizeForThisCycle;
            return values;
        }

        /// <summary>
        /// 获取avc编码HLS的M3U8文件URL
        /// </summary>
        /// <param name="roomCard"></param>
        /// <param name="Definition">清晰度</param>
        /// <param name="Url"></param>
        /// <returns></returns>

        public static bool GetHlsAvcUrl(RoomCardClass roomCard, long Definition, out string Url)
        {
            Url = "";
            if (!RoomInfo.GetLiveStatus(roomCard.RoomId))
            {
                return false;
            }
            HostClass hostClass = _GetHost(roomCard.RoomId, "http_hls", "fmp4", "avc", Definition);
            if (hostClass.Effective)
            {
                Url = $"{hostClass.host}{hostClass.base_url}{hostClass.uri_name}{hostClass.extra}";
                return true;
            }
            return false;
        }

    }
}
