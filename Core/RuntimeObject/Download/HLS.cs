using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.Download.Basics;

namespace Core.RuntimeObject.Download
{
    public class HLS
    {
        /// <summary>
        /// 录制HLS_avc制式的MP4文件
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        /// <param name="First">是否为初次任务</param>
        /// <returns>[TaskStatus]任务状态；[FileName]下载成功的文件名</returns>
        public static async Task<(DlwnloadTaskState hlsState, string FileName)> DlwnloadHls_avc_mp4(RoomCardClass card,bool First=false)
        {
            DlwnloadTaskState hlsState = DlwnloadTaskState.Recording;
            string File = string.Empty;
            await Task.Run(() =>
            {
                InitializeDownload(card);
                string title = Tools.KeyCharacterReplacement.CheckFilenames(RoomInfo.GetTitle(card.UID));
                long roomId = card.RoomId;
                File = $"{Config.Core._RecFileDirectory}{Core.Tools.KeyCharacterReplacement.ReplaceKeyword(card.UID, Core.Config.Core._DefaultFilePathNameFormat)}_original.mp4";
                CreateDirectoryIfNotExists(File.Substring(0, File.LastIndexOf('/')));
                Thread.Sleep(5);
                
                using (FileStream fs = new FileStream(File, FileMode.Append))
                {

                    HostClass hostClass = new();
                    GetFlvHost_avc(card);
                    while (!GetHlsHost_avc(card, ref hostClass))
                    {
                        hlsState = HandleHlsError(card, hostClass);
                        if (First && hlsState == DlwnloadTaskState.NoHLSStreamExists)//初次任务，等待HLS流生成，等待时间根据配置文件来
                        {
                            Thread.Sleep(Config.Core._HlsWaitingTime * 1000);
                        }
                        hlsState = HandleHlsError(card, hostClass);
                        switch (hlsState)
                        {
                            case DlwnloadTaskState.StopLive:
                                hlsState = CheckAndHandleFile(File, ref card);
                                return;
                            case DlwnloadTaskState.UserCancellation:
                                hlsState = CheckAndHandleFile(File, ref card);
                                return;
                            case DlwnloadTaskState.PaidLiveStream:
                                Log.Warn(nameof(HandleHlsError), $"[{card.Name}({card.RoomId})]直播间开播中，但直播间为收费直播间(大航海或者门票直播)，创建任务失败，跳过当前任务");
                                return;
                            case DlwnloadTaskState.NoHLSStreamExists:
                                Log.Info(nameof(HandleHlsError), $"[{card.Name}({card.RoomId})]直播间开播中，但没获取到HLS流，降级到FLV模式");
                                return;
                        }
                    }
                    //Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]开始监听重连");
                    List<(long size, DateTime time)> values = new();
                    bool InitialRequest = true;
                    long currentLocation = 0;
                    while (true)
                    {
                        long downloadSizeForThisCycle = 0;
                        try
                        {
                            if (card.DownInfo.Unmark)
                            {
                                hlsState = CheckAndHandleFile(File, ref card);
                                return;
                            }
                            //刷新Host信息，获取最新的直播流片段
                            bool isHlsHostAvailable = RefreshHlsHost_avc(card, ref hostClass);
                            if (!isHlsHostAvailable)
                            {
                                hlsState = HandleHostRefresh(card, ref hostClass);
                                switch (hlsState)
                                {
                                    case DlwnloadTaskState.StopLive:
                                        hlsState = CheckAndHandleFile(File, ref card);
                                        return;
                                    case DlwnloadTaskState.UserCancellation:
                                        hlsState = CheckAndHandleFile(File, ref card);
                                        return;
                                    case DlwnloadTaskState.Recording:
                                        break;
                                }
                            }
                            else
                            {
                                if (InitialRequest)
                                {
                                    downloadSizeForThisCycle += WriteToFile(fs, $"{hostClass.host}{hostClass.base_url}{hostClass.eXTM3U.Map_URI}?{hostClass.extra}");
                                }
                                foreach (var item in hostClass.eXTM3U.eXTINFs)
                                {
                                    if (long.TryParse(item.FileName, out long index) && index > currentLocation)
                                    {
                                        downloadSizeForThisCycle += WriteToFile(fs, $"{hostClass.host}{hostClass.base_url}{item.FileName}.{item.ExtensionName}?{hostClass.extra}");
                                        currentLocation = index;
                                    }
                                }
                                hostClass.eXTM3U.eXTINFs = new();
                                values.Add((downloadSizeForThisCycle, DateTime.Now));
                                values = UpdateDownloadSpeed(values, card, downloadSizeForThisCycle);
                                if (hostClass.eXTM3U.IsEND)
                                {
                                    if (InitialRequest)
                                    {
                                        hlsState = CheckAndHandleFile(File, ref card);
                                        hlsState = DlwnloadTaskState.SuccessfulButNotStream;
                                        if (!card.DownInfo.Unmark)
                                            Thread.Sleep(1000 * 10);
                                        return;
                                    }
                                    else
                                    {
                                        Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]录制任务收到END数据包，进行收尾处理");
                                        hlsState = DlwnloadTaskState.Success;
                                        if (!card.DownInfo.Unmark)
                                            Thread.Sleep(1000 * 10);
                                        return;
                                    }
                                }
                                if (InitialRequest)
                                {
                                    //正式开始下载提示
                                    LogDownloadStart(card,"HLS");
                                }
                                InitialRequest = false;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]录制循环中出现未知错误，写入日志", e, true);
                            if (!card.DownInfo.Unmark)
                                Thread.Sleep(1000);
                        }
                        if (!card.DownInfo.Unmark)
                            Thread.Sleep(2000);
                    }
                }
            });
            card.DownInfo.DownloadSize = 0;
            return (hlsState, File);
        }





        /// <summary>
        /// 处理Host刷新
        /// </summary>
        /// <param name="card">房间信息</param>
        /// <param name="hostClass">HostClass实例</param>
        /// <returns>HLS错误计数</returns>
        private static DlwnloadTaskState HandleHostRefresh(RoomCardClass card, ref HostClass hostClass)
        {
            if (!GetHlsHost_avc(card, ref hostClass) && !RoomInfo.GetLiveStatus(card.RoomId))
            {
                Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]刷新Host时发现直播间已下播");
                return DlwnloadTaskState.StopLive;
            }
            if (card.DownInfo.Unmark)
            {
                return DlwnloadTaskState.UserCancellation;
            }
            Log.Info(nameof(DlwnloadHls_avc_mp4), $"[{card.Name}({card.RoomId})]触发Host刷新");
            return DlwnloadTaskState.Recording;
        }




        /// <summary>
        /// 处理HLS错误
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        /// <param name="hostClass">主播类</param>
        /// <returns>当前HLS状态</returns>
        private static DlwnloadTaskState HandleHlsError(RoomCardClass card, HostClass hostClass)
        {
            if (!RoomInfo.GetLiveStatus(card.RoomId))
            {
                return DlwnloadTaskState.StopLive;
            }
            if(card.DownInfo.Unmark)
            {
                return DlwnloadTaskState.UserCancellation;
            }
            //是否为收费直播
            bool isPaidLiveStream = hostClass.all_special_types.Contains(1);
            if (isPaidLiveStream)
            {   
                card.DownInfo.Status = RoomCardClass.DownloadStatus.Special;            
                return DlwnloadTaskState.PaidLiveStream;
            }
            else
            {              
                card.DownInfo.Status = RoomCardClass.DownloadStatus.Standby;
                return DlwnloadTaskState.NoHLSStreamExists;
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


    }
}
