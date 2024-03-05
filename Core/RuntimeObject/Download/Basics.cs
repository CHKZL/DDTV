using ConsoleTableExt;
using Core.LiveChat;
using Core.LogModule;
using Core.Network.Methods;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;
using static Core.RuntimeObject.Detect;

namespace Core.RuntimeObject.Download
{
    public class Basics
    {

        #region internal Method

        /// <summary>
        /// 处理录制任务
        /// </summary>
        /// <param name="roomCard"></param>
        /// <param name="triggerTypes"></param>
        /// <param name="liveChatListener"></param>
        /// <param name="isFirstTime"></param>
        internal static async Task HandleRecordingAsync(RoomCardClass roomCard, List<TriggerType> triggerTypes, Core.LiveChat.LiveChatListener liveChatListener, bool isFirstTime)
        {
            if (isFirstTime)
            {
                Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})触发开播事件,开始录制【触发类型:" + (triggerTypes.Contains(TriggerType.ManuallyTriggeringTasks) ? "手动触发" : "自动触发") + "】");

                if (roomCard.IsRecDanmu)
                {
                    liveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                    liveChatListener.DisposeSent += LiveChatListener_DisposeSent;
                    liveChatListener.Connect();
                }
            }
            else
            {
                Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})检测到录制任务重连，同步录制状态，尝试重连...");
            }
            //HLS下载逻辑
            var result = await HLS.DlwnloadHls_avc_mp4(roomCard, isFirstTime);
            Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})HLS录制进程中断，状态:{Enum.GetName(typeof(DlwnloadTaskState), result.hlsState)}");
            if (result.hlsState == DlwnloadTaskState.NoHLSStreamExists)
            {
                Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})降级到FLV模式进行录制");
                //FLV兜底逻辑
                result = await FLV.DlwnloadHls_avc_flv(roomCard);
            }
            if (roomCard.IsRecDanmu)
            {
                Danmu.SevaDanmu(liveChatListener, ref roomCard);
            }
            if (result.hlsState == DlwnloadTaskState.Success)
            {
                roomCard.DownInfo.DownloadFileList.VideoFile.Add(result.FileName);
            }
            if (result.hlsState == DlwnloadTaskState.Success && Core.Config.Core._AutomaticRepair)
            {
                Core.Tools.Transcode transcode = new Core.Tools.Transcode();
                try
                {
                    transcode.TranscodeAsync(result.FileName, result.FileName.Replace("_original.mp4", "_fix.mp4").Replace("_original.flv", "_fix.mp4"), roomCard.RoomId);
                    roomCard.DownInfo.DownloadFileList.VideoFile.Add(result.FileName.Replace("_original.mp4", "_fix.mp4").Replace("_original.flv", "_fix.mp4"));
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})完成录制任务后修复时出现意外错误，文件:{result.FileName}");
                }
            }
        }

        /// <summary>
        /// 下载完成重置房间卡状态
        /// </summary>
        /// <param name="roomCard"></param>
        internal static void DownloadCompletedReset(ref RoomCardClass roomCard)
        {
            Log.Info(nameof(DownloadCompletedReset), $"[{roomCard.Name}({roomCard.RoomId})]进行录制完成处理");

            roomCard.DownInfo.RealTimeDownloadSpe = 0;
            roomCard.DownInfo.DownloadSize = 0;
            roomCard.DownInfo.Status = roomCard.DownInfo.Unmark ? RoomCardClass.DownloadStatus.Cancel : RoomCardClass.DownloadStatus.DownloadComplete;
            roomCard.DownInfo.EndTime = DateTime.Now;
            _Room.SetRoomCardByUid(roomCard.UID, roomCard);
        }

        internal static void LiveChatListener_DisposeSent(object? sender, EventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            //Danmu.SevaDanmu(liveChatListener);
            if (!liveChatListener._Cancel)
            {
                if (liveChatListener._disposed)
                {
                    liveChatListener = new Core.LiveChat.LiveChatListener(liveChatListener.RoomId);
                    liveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                    liveChatListener.DisposeSent += LiveChatListener_DisposeSent;
                }
                Log.Info(nameof(LiveChatListener_DisposeSent), $"{liveChatListener.RoomId}({liveChatListener.Name})弹幕断开连接，但直播未结束，触发重连");
                liveChatListener.Connect();
            }
            else
            {
                Log.Info(nameof(LiveChatListener_DisposeSent), $"{liveChatListener.RoomId}({liveChatListener.Name})弹幕断开连接");
                liveChatListener.Cancel();
                liveChatListener = null;
            }
        }

        /// <summary>
        /// 初始化下载
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        internal static void InitializeDownload(RoomCardClass card,RoomCardClass.TaskType taskType)
        {
            card.DownInfo.Status = RoomCardClass.DownloadStatus.Standby;
            card.DownInfo.taskType = taskType;
            _Room.SetRoomCardByUid(card.UID, card);
        }

        /// <summary>
        /// 如果目录不存在，则创建目录
        /// </summary>
        /// <param name="dirName">目录名称</param>
        internal static void CreateDirectoryIfNotExists(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        /// <summary>
        /// 记录下载开始
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        internal static void LogDownloadStart(RoomCardClass card,string Type = "Auto")
        {
            var tableData = new List<List<object>>
            {
                new List<object> { "模式","名称", "房间号", "UID", "直播标题" },
                new List<object> { Type,card.Name, card.RoomId, card.UID, card.Title.Value }
            };
            ConsoleTableBuilder.From(tableData).WithTitle("开始录制任务").ExportAndWriteLine();
            string startText = $"[开始录制任务]：{Type}|" +
           $"{card.Name}({card.RoomId})|" +
           $"UID：{card.UID}|" +
           $"标题：{card.Title.Value}";
            Log.Info(nameof(LogDownloadStart), $"{startText}", false);
            card.DownInfo.Status = RoomCardClass.DownloadStatus.Downloading;
            card.DownInfo.StartTime = DateTime.Now;
        }

        /// <summary>
        /// 从网络写入文件
        /// </summary>
        /// <param name="fs">待写入的FileStream</param>
        /// <param name="url">待写入的文件原始网络路径</param>
        /// <returns>写入的文件byte数</returns>
        internal static long WriteToFile(FileStream fs, string url)
        {
            long len = 0;
            if (!Core.Init.IsDevDebug)
                len = Network.Download.File.GetFileToByte(fs, url, true, "https://www.bilibili.com/");

            return len;
        }


        /// <summary>
        /// 获取avc编码HLS内容
        /// </summary>
        /// <param name="RoomId"></param>
        /// <returns></returns>

        internal static bool GetHlsHost_avc(RoomCardClass roomCard, ref HostClass hostClass)
        {
            if (!RoomInfo.GetLiveStatus(roomCard.RoomId))
            {
                return false;
            }
            hostClass = _GetHost(roomCard.RoomId, "http_hls", "fmp4", "avc");
            if (hostClass.Effective)
            {
                string Inp = $"{hostClass.host}{hostClass.base_url}{hostClass.uri_name}{hostClass.extra}";
                string webref = Network.Download.File.GetFileToString(Inp, true);
                if (string.IsNullOrEmpty(webref))
                {
                    Log.Debug("test", $"获取网络文件为空，房间号:{roomCard.RoomId}");
                }
                hostClass = Tools.Linq.SerializedM3U8(webref, ref hostClass);
                if (hostClass.eXTM3U.eXTINFs.Count != 0)
                {
                    return true;
                }
                else if (hostClass.eXTM3U.eXTINFs.Count == 0 && RefreshHlsHost_avc(roomCard, ref hostClass))
                {
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 刷新HlsHost信息
        /// </summary>
        /// <param name="roomCard"></param>
        /// <param name="m3u8"></param>
        /// <returns></returns>
        internal static bool RefreshHlsHost_avc(RoomCardClass roomCard, ref HostClass m3u8)
        {
            string fileContent = string.Empty;
            if (!string.IsNullOrEmpty(m3u8.SteramInfo))
            {
                fileContent = m3u8.SteramInfo;
                m3u8.SteramInfo = string.Empty;
            }
            else if (!string.IsNullOrEmpty(m3u8.host) && !string.IsNullOrEmpty(m3u8.base_url) && !string.IsNullOrEmpty(m3u8.uri_name) && !string.IsNullOrEmpty(m3u8.extra))
            {
                fileContent = $"{m3u8.host}{m3u8.base_url}{m3u8.uri_name}{m3u8.extra}";
            }

            if (!string.IsNullOrEmpty(fileContent))
            {
                string webref = Network.Download.File.GetFileToString(fileContent, true);
                if (string.IsNullOrEmpty(webref))
                {
                    return false;
                }
                Tools.Linq.SerializedM3U8(webref, ref m3u8);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 获取avc编码FLV内容
        /// </summary>
        /// <param name="RoomId"></param>
        /// <returns></returns>
        internal static HostClass GetFlvHost_avc(RoomCardClass roomCard)
        {
            return _GetHost(roomCard.RoomId, "http_stream", "flv", "avc");
        }

        /// <summary>
        /// 检查并处理文件
        /// </summary>
        /// <param name="File">文件名</param>
        /// <returns>是否成功</returns>
        internal static DlwnloadTaskState CheckAndHandleFile(string File, ref RoomCardClass card)
        {
            const long FileSizeThreshold = 10 * 1024 * 1024; // 10MB
            bool fileExists = System.IO.File.Exists(File);
            if (fileExists)
            {
                System.IO.FileInfo fileInfo = new(File);
                if (fileInfo.Length > FileSizeThreshold)
                {
                    return DlwnloadTaskState.Success;
                }
                else
                {
                    Tools.FileOperations.Delete(File);
                }
            }
            //card.DownInfo.DownloadFileList.VideoFile.RemoveAt(card.DownInfo.DownloadFileList.VideoFile.Count - 1);
            return DlwnloadTaskState.SuccessfulButNotStream;
        }

        #endregion

        #region Private Method



        private static HostClass _GetHost(long RoomId, string protocol_name, string format_name, string codec_name)
        {
            HostClass hostClass = new();
            PlayInfo_Class playInfo = GetPlayInfo(RoomId);
            if (playInfo == null || playInfo.data.playurl_info == null || playInfo.data.playurl_info.playurl == null)
                return hostClass;
            hostClass.all_special_types = playInfo.data.all_special_types;
            PlayInfo_Class.Stream? stream = playInfo.data.playurl_info.playurl.stream.FirstOrDefault(x => x.protocol_name == protocol_name);
            if (stream == null)
                return hostClass;
            PlayInfo_Class.Format? format = stream.format.FirstOrDefault(x => x.format_name == format_name);
            if (format == null)
                return hostClass;
            PlayInfo_Class.Codec? codec = format.codec.FirstOrDefault(x => x.codec_name == codec_name);
            if (codec == null)
                return hostClass;
            var urlInfo = codec.url_info.FirstOrDefault(x => !x.host.Contains("smtcdns.net")) ?? codec.url_info[new Random().Next(0, codec.url_info.Count - 1)];

            hostClass = new()
            {
                Effective = true,
                host = urlInfo.host,
                base_url = codec.base_url.Replace($"{codec.base_url.Split('/')[codec.base_url.Split('/').Length - 1]}", ""),
                uri_name = codec.base_url.Split('/')[codec.base_url.Split('/').Length - 1],
                extra = urlInfo.extra
            };
            return hostClass;
        }

    

        private static void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {

                        liveChatListener.DanmuMessage.Danmu.Add(new Danmu.DanmuInfo
                        {
                            color = Danmu.MessageColor,
                            pool = 0,
                            size = 25,
                            timestamp = Danmu.Timestamp,
                            type = Danmu.MessageType,
                            time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            uid = Danmu.UserId,
                            Message = Danmu.Message,
                            Nickname = Danmu.UserName,
                            LV = Danmu.GuardLV
                        });
                        if (Init.IsDevDebug)
                        {
                            //Log.Info(nameof(LiveChatListener_MessageReceived),$"收到弹幕:{Danmu.UserName}[{Danmu.UserId}]:{Danmu.Message}");
                        }
                        break;
                    }
                case SuperchatEventArg SuperchatEvent:
                    {

                        liveChatListener.DanmuMessage.SuperChat.Add(new Danmu.SuperChatInfo()
                        {
                            Message = SuperchatEvent.Message,
                            MessageTrans = SuperchatEvent.messageTrans,
                            Price = SuperchatEvent.Price,
                            Time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = SuperchatEvent.Timestamp,
                            UserId = SuperchatEvent.UserId,
                            UserName = SuperchatEvent.UserName,
                            TimeLength = SuperchatEvent.TimeLength
                        });
                        if (Init.IsDevDebug)
                        {
                            //Log.Info(nameof(LiveChatListener_MessageReceived),$"收到SC:{SuperchatEvent.UserName}[{SuperchatEvent.UserId}]:{SuperchatEvent.Message}");
                        }
                        break;
                    }
                case GuardBuyEventArgs GuardBuyEvent:
                    {

                        liveChatListener.DanmuMessage.GuardBuy.Add(new Danmu.GuardBuyInfo()
                        {
                            GuardLevel = GuardBuyEvent.GuardLevel,
                            GuradName = GuardBuyEvent.GuardName,
                            Number = GuardBuyEvent.Number,
                            Price = GuardBuyEvent.Price,
                            Time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = GuardBuyEvent.Timestamp,
                            UserId = GuardBuyEvent.UserId,
                            UserName = GuardBuyEvent.UserName
                        });
                        if (Init.IsDevDebug)
                        {
                            // Log.Info(nameof(LiveChatListener_MessageReceived),$"收到大航海:{GuardBuyEvent.UserName}[{GuardBuyEvent.UserId}]:{GuardBuyEvent.GuardName}");
                        }
                        break;
                    }
                case SendGiftEventArgs sendGiftEventArgs:
                    {
                        liveChatListener.DanmuMessage.Gift.Add(new Danmu.GiftInfo()
                        {
                            Amount = sendGiftEventArgs.Amount,
                            GiftName = sendGiftEventArgs.GiftName,
                            Price = sendGiftEventArgs.GiftPrice,
                            Time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = sendGiftEventArgs.Timestamp,
                            UserId = sendGiftEventArgs.UserId,
                            UserName = sendGiftEventArgs.UserName
                        });
                        if (Init.IsDevDebug)
                        {
                            // Log.Info(nameof(LiveChatListener_MessageReceived),$"收到礼物:{sendGiftEventArgs.UserName}[{sendGiftEventArgs.UserId}]:{sendGiftEventArgs.GiftName} x {sendGiftEventArgs.Amount}个");
                        }
                        break;
                    }
                default:
                    break;
            }

        }


        #endregion

        #region Class

        internal class HostClass
        {
            /// <summary>
            /// 是否有有效Host信息
            /// </summary>
            internal bool Effective { get; set; } = false;
            /// <summary>
            /// host地址
            /// </summary>
            internal string host { get; set; }
            /// <summary>
            /// 路由信息
            /// </summary>
            internal string base_url { get; set; }
            /// <summary>
            /// 目录文件名
            /// </summary>
            internal string uri_name { get; set; }
            /// <summary>
            /// 参数
            /// </summary>
            internal string extra { get; set; }
            /// <summary>
            /// M3U8头
            /// </summary>
            internal string SteramInfo { get; set; }
            /// <summary>
            /// 房间付费类型
            /// </summary>
            internal List<long> all_special_types { get; set; } = new List<long>();
            internal EXTM3U eXTM3U { get; set; } = new();
            internal class EXTM3U
            {
                /// <summary>
                /// 版本号
                /// </summary>
                internal int Version { get; set; }
                /// <summary>
                /// 起始时间标记
                /// </summary>
                internal long TimeOffSet { get; set; }
                /// <summary>
                /// 初始标签
                /// </summary>
                internal long MediaSequence { get; set; }
                /// <summary>
                /// 最大时长标记
                /// </summary>
                internal double Targetduration { get; set; }
                /// <summary>
                /// 初始化片段（I帧）
                /// </summary>
                internal string Map_URI { get; set; }
                /// <summary>
                /// 结束标记
                /// </summary>
                internal bool IsEND { get; set; }
                /// <summary>
                /// 具体的分片信息列表
                /// </summary>
                internal List<EXTINF> eXTINFs { get; set; } = new();
                internal class EXTINF
                {
                    /// <summary>
                    /// 自定义标签
                    /// </summary>
                    internal string Aux { get; set; }
                    /// <summary>
                    /// 时长
                    /// </summary>
                    internal double Duration { get; set; }
                    /// <summary>
                    /// 文件名
                    /// </summary>
                    internal string FileName { get; set; }
                    /// <summary>
                    /// 拓展名
                    /// </summary>
                    internal string ExtensionName { get; set; }
                }
            }
        }

        public enum DlwnloadTaskState
        {
            /// <summary>
            /// 初始状态(该状态不应该被传递出去，使用前必须状态已变化)
            /// </summary>
            Recording,
            /// <summary>
            /// 成功
            /// </summary>
            Success,
            /// <summary>
            /// 成功(但该任务未推流，未生成文件)
            /// </summary>
            SuccessfulButNotStream,
            /// <summary>
            /// 已下播
            /// </summary>
            StopLive,
            /// <summary>
            /// 不存在HLS流
            /// </summary>
            NoHLSStreamExists,
            /// <summary>
            /// 用户取消
            /// </summary>
            UserCancellation,
            /// <summary>
            /// 当前为付费直播
            /// </summary>
            PaidLiveStream
        }

        #endregion
    }
}
