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
                            InterruptibleSleep(Config.Core_RunConfig._HlsWaitingTime * 1000, card);//可被打断，取消/切割时不用干等
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
                    //上一轮init segment的解码配置签名(视频宽高/编码参数、音频采样率/声道/AudioSpecificConfig)。
                    //注意：不能按Map_URI文件名或init segment逐字节内容比较——部分CDN的m3u8中每个分片都有
                    //独立的、按秒递增命名的EXT-X-MAP，且init segment里的码率统计等字段会周期性抖动，
                    //按文件名或全字节比较都会误判为init segment变化，造成无限切割产生大量碎片文件
                    byte[] lastInitSignature = null;
                    //最后一次下载到新分片的时间，用于"无新分片"看门狗
                    DateTime lastSegmentTime = DateTime.Now;

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
                            //无新分片看门狗：主播异常断流(主播端崩溃/推流网络中断等)时，CDN常继续提供不含#EXT-X-ENDLIST的旧m3u8，
                            //此时流侧信号(ENDLIST/host刷新失败)都无法发现下播，会在这里无限空转。
                            //超过阈值没有下载到任何新分片时，主动确认房间是否真的还在开播。
                            //阈值取3倍分片时长，钳制在[60,300]秒：下限避免正常直播误触发，
                            //上限防止m3u8异常内容解析出巨大TARGETDURATION导致看门狗永不触发。
                            //注意TryParse会把"NaN"等字符串解析为NaN(double.TryParse认为合法)，
                            //NaN会让大小比较恒为false使看门狗失效，所以非有限值一律回退到60秒兜底
                            double targetDuration = hostClass.eXTM3U.Targetduration;
                            double noSegmentTimeout = double.IsFinite(targetDuration) && targetDuration > 0 ? Math.Clamp(targetDuration * 3, 60, 300) : 60;
                            if ((DateTime.Now - lastSegmentTime).TotalSeconds > noSegmentTimeout)
                            {
                                if (ConfirmStopLive(card.RoomId, card))
                                {
                                    Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]超过{noSegmentTimeout}秒没有新分片，且已确认直播间下播，进行收尾处理");
                                    hlsState = CheckAndHandleFile(File, ref card);
                                    return;
                                }
                                if (card.DownInfo.Unmark || card.DownInfo.IsCut)
                                {
                                    //确认期间用户取消/手动切割，交给下一轮 ShouldFinalizeRecording 处理
                                    continue;
                                }
                                if (IsDefinitelyLive(card.RoomId))
                                {
                                    //直播间明确仍在开播但长时间没有新分片，判定为断流，按主播重新推流处理，结束当前任务由外层重连重新拉流
                                    Log.Warn(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]直播间仍在开播但超过{noSegmentTimeout}秒没有新分片，判定为断流，结束当前任务准备重连");
                                    hlsState = CheckAndHandleFile(File, ref card, true);
                                    return;
                                }
                                //未能确认房间状态(接口故障)：不拆除当前任务，重置看门狗计时，下个周期再确认，
                                //避免API故障期间反复拆任务产生空文件和无效修复
                                Log.Warn(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]超过{noSegmentTimeout}秒没有新分片，但房间状态查询失败，无法确认是否下播，等待下个周期重试");
                                lastSegmentTime = DateTime.Now;
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
                                        //直播间还在开播但本次没刷出流，按日志所说等待后重试，连续多次失败才放弃
                                        RetryCount++;
                                        if (RetryCount > 5)
                                        {
                                            Log.Warn(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]连续{RetryCount}次刷新Host均未获取到有效直播流，放弃本次HLS任务");
                                            CheckAndHandleFile(File, ref card);
                                            hlsState = DownloadTaskState.NoHLSStreamExists;
                                            return;
                                        }
                                        Thread.Sleep(2000);//配合循环末尾的1秒等待，实现3秒后重试
                                        break;
                                }
                            }
                            else
                            {
                                //刷新成功，重置连续失败计数
                                RetryCount = 0;
                                if (InitialRequest)
                                {
                                    string DebugFile = string.Empty;
                                    //DebugFile = $"{hostClass.eXTM3U.eXTINFs[0].FileName+"_"+hostClass.eXTM3U.Map_URI.Replace(".m4s","_I.m4s")}";
                                    downloadSizeForThisCycle += WriteToFile(fs, $"{hostClass.host}{hostClass.base_url}{hostClass.eXTM3U.Map_URI}?{hostClass.extra}", DebugFile);
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
                                        //提取init segment解码配置签名并比较：只有宽高/编码参数/音频参数真正变化(主播连麦/重新推流)才切割
                                        byte[] initSignature = GetInitSegmentSignature(m4sBytes);
                                        if (initSignature != null)
                                        {
                                            if (lastInitSignature != null && !initSignature.SequenceEqual(lastInitSignature))
                                            {
                                                Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]检测到推流编码参数变化（可能是主播连麦或重新推流），进行切割处理");
                                                hlsState = DownloadTaskState.Success;
                                                return;
                                            }
                                            lastInitSignature = initSignature;
                                        }
                                    }
                                    else
                                    {
                                        //分辨率/init segment内容检测是可选增强逻辑，init segment 异常时仅跳过本轮检测，不影响录制
                                        string skipReason = m4sBytes == null
                                            ? "init segment下载失败(未获取到数据，可能为网络问题或CDN拒绝请求)"
                                            : m4sBytes.Length < 248
                                                ? $"init segment响应内容过短(仅{m4sBytes.Length}字节，可能为CDN错误页或响应被截断)"
                                                : "init segment内容不是合法的m4s文件(缺少ftyp文件头，可能为CDN错误页)";
                                        Log.Warn(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]本轮分辨率/init segment变化检测已跳过：{skipReason}，录制不受影响，下一轮将自动重试");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Warn(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]本轮分辨率/init segment变化检测已跳过：下载或解析init segment时发生异常({ex.Message})，录制不受影响", ex);
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
                                    //分片之间检查取消/切割，避免一周期多分片时取消要等整个周期
                                    if (card.DownInfo.Unmark || card.DownInfo.IsCut)
                                    {
                                        break;
                                    }
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
                                //下载到新分片则刷新看门狗计时
                                if (downloadSizeForThisCycle > 0)
                                {
                                    lastSegmentTime = DateTime.Now;
                                }
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
                                            InterruptibleSleep(1000 * 10, card);//可被打断，取消/切割时不用干等10秒
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
                                            InterruptibleSleep(1000 * 10, card);//可被打断，取消/切割时不用干等10秒
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
        /// 从init segment(m4s)提取解码配置签名：视频宽高+编码配置(avcC/hvcC/av1C)内容、音频声道数/采样率/AudioSpecificConfig。
        /// 只有这些影响分片拼接解码的字段变化才视为推流参数真正变化；
        /// B站CDN下发的init segment中码率统计等字段会周期性抖动，逐字节比较会误判
        /// </summary>
        /// <param name="m4sBytes">init segment字节内容</param>
        /// <returns>签名字节；内容缺失、过短或不是合法m4s时返回null</returns>
        private static byte[] GetInitSegmentSignature(byte[] m4sBytes)
        {
            if (m4sBytes == null || m4sBytes.Length < 248)
            {
                return null;
            }
            if (m4sBytes[4] != 'f' || m4sBytes[5] != 't' || m4sBytes[6] != 'y' || m4sBytes[7] != 'p')
            {
                return null;
            }
            using MemoryStream signature = new();
            //视频轨道宽高(tkhd固定偏移，与TryParseResolution一致)
            signature.Write(m4sBytes, 240, 8);
            foreach (var moov in EnumerateChildBoxes(m4sBytes, 0, m4sBytes.Length))
            {
                if (moov.type != "moov")
                {
                    continue;
                }
                foreach (var trak in EnumerateChildBoxes(m4sBytes, moov.contentStart, moov.end))
                {
                    if (trak.type != "trak")
                    {
                        continue;
                    }
                    //逐层下钻到stsd，取第一个sample entry
                    var mdia = FindChildBox(m4sBytes, trak.contentStart, trak.end, "mdia");
                    var minf = mdia == null ? null : FindChildBox(m4sBytes, mdia.Value.contentStart, mdia.Value.end, "minf");
                    var stbl = minf == null ? null : FindChildBox(m4sBytes, minf.Value.contentStart, minf.Value.end, "stbl");
                    var stsd = stbl == null ? null : FindChildBox(m4sBytes, stbl.Value.contentStart, stbl.Value.end, "stsd");
                    if (stsd == null)
                    {
                        continue;
                    }
                    //stsd内容: version/flags(4) + entry_count(4) + sample entry列表
                    int entryStart = stsd.Value.contentStart + 8;
                    if (entryStart + 8 > stsd.Value.end)
                    {
                        continue;
                    }
                    int entrySize = ReadBoxSize(m4sBytes, entryStart);
                    string entryType = Encoding.ASCII.GetString(m4sBytes, entryStart + 4, 4);
                    int entryContent = entryStart + 8;
                    int entryEnd = entryStart + entrySize;
                    if (entrySize < 8 || entryEnd > stsd.Value.end)
                    {
                        continue;
                    }
                    if (entryType is "avc1" or "avc2" or "avc3" or "avc4" or "hev1" or "hvc1" or "av01")
                    {
                        //VisualSampleEntry固定头部78字节，之后是编码配置等子box
                        foreach (var child in EnumerateChildBoxes(m4sBytes, entryContent + 78, entryEnd))
                        {
                            if (child.type is "avcC" or "hvcC" or "av1C")
                            {
                                signature.Write(m4sBytes, child.contentStart, child.end - child.contentStart);
                            }
                        }
                    }
                    else if (entryType == "mp4a")
                    {
                        //AudioSampleEntry固定头部: reserved(6)+data_reference_index(2)+reserved(8)+channelcount(2)+samplesize(2)+pre_defined(2)+reserved(2)+samplerate(4)
                        if (entryContent + 28 <= entryEnd)
                        {
                            signature.Write(m4sBytes, entryContent + 16, 2);//channelcount
                            signature.Write(m4sBytes, entryContent + 24, 4);//samplerate(16.16定点数)
                            foreach (var child in EnumerateChildBoxes(m4sBytes, entryContent + 28, entryEnd))
                            {
                                if (child.type == "esds")
                                {
                                    byte[] asc = FindAudioSpecificConfig(m4sBytes, child.contentStart, child.end);
                                    if (asc != null)
                                    {
                                        signature.Write(asc, 0, asc.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return signature.ToArray();
        }

        /// <summary>
        /// 在指定范围内查找第一个指定类型的MP4子box
        /// </summary>
        private static (int contentStart, int end)? FindChildBox(byte[] buf, int start, int end, string type)
        {
            foreach (var box in EnumerateChildBoxes(buf, start, end))
            {
                if (box.type == type)
                {
                    return (box.contentStart, box.end);
                }
            }
            return null;
        }

        /// <summary>
        /// 枚举指定范围内的MP4子box
        /// </summary>
        private static IEnumerable<(int contentStart, int end, string type)> EnumerateChildBoxes(byte[] buf, int start, int end)
        {
            int i = start;
            while (i + 8 <= end)
            {
                int size = ReadBoxSize(buf, i);
                if (size < 8 || i + size > end)
                {
                    yield break;
                }
                yield return (i + 8, i + size, Encoding.ASCII.GetString(buf, i + 4, 4));
                i += size;
            }
        }

        /// <summary>
        /// 读取MP4 box的size字段(大端32位)
        /// </summary>
        private static int ReadBoxSize(byte[] buf, int offset)
        {
            return (int)((uint)buf[offset] << 24 | (uint)buf[offset + 1] << 16 | (uint)buf[offset + 2] << 8 | buf[offset + 3]);
        }

        /// <summary>
        /// 从esds box内容中解析DecoderSpecificInfo(tag 0x05，即AudioSpecificConfig)。
        /// 注意DecoderConfigDescriptor里的maxBitrate/avgBitrate等码率统计字段会随推流抖动，
        /// 不能把esds整体直接参与"init segment是否变化"的比较
        /// </summary>
        private static byte[] FindAudioSpecificConfig(byte[] buf, int start, int end)
        {
            //esds内容: version/flags(4) + ES_Descriptor(tag 0x03)
            foreach (var esDesc in EnumerateDescriptors(buf, start + 4, end))
            {
                if (esDesc.tag != 0x03 || esDesc.length < 3)
                {
                    continue;
                }
                //ES_Descriptor: ES_ID(2) + flags(1) + 子描述符
                foreach (var decoderConfig in EnumerateDescriptors(buf, esDesc.payloadStart + 3, esDesc.payloadStart + esDesc.length))
                {
                    if (decoderConfig.tag != 0x04 || decoderConfig.length < 13)
                    {
                        continue;
                    }
                    //DecoderConfigDescriptor: objectType(1)+streamType(1)+bufferSizeDB(3)+maxBitrate(4)+avgBitrate(4) + 子描述符
                    foreach (var specific in EnumerateDescriptors(buf, decoderConfig.payloadStart + 13, decoderConfig.payloadStart + decoderConfig.length))
                    {
                        if (specific.tag == 0x05)
                        {
                            byte[] asc = new byte[specific.length];
                            Array.Copy(buf, specific.payloadStart, asc, 0, specific.length);
                            return asc;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 枚举MPEG-4描述符(tag + 可扩展length + payload)
        /// </summary>
        private static IEnumerable<(int tag, int payloadStart, int length)> EnumerateDescriptors(byte[] buf, int start, int end)
        {
            int i = start;
            while (i + 2 <= end)
            {
                int tag = buf[i++];
                int length = 0;
                int lengthBytes = 0;
                bool continues;
                do
                {
                    if (i >= end || ++lengthBytes > 4)
                    {
                        yield break;
                    }
                    byte b = buf[i++];
                    length = (length << 7) | (b & 0x7F);
                    continues = (b & 0x80) != 0;
                } while (continues);
                if (i + length > end)
                {
                    yield break;
                }
                yield return (tag, i, length);
                i += length;
            }
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
            //样本不足2个或窗口时间跨度太小时不更新速度：单样本的跨度≈0会算出天文数字(任务首个周期必现)，
            //且DateTime.Now分辨率仅~15.6ms，过小的跨度会把速度放大到脱离实际
            double spanMs = values.Count > 1 ? DateTime.Now.Subtract(values[0].time).TotalMilliseconds : 0;
            if (spanMs >= 500)
            {
                card.DownInfo.RealTimeDownloadSpe = (values.Sum(x => x.size) / spanMs) * 1000;
            }
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
