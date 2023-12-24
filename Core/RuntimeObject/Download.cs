using Core.LogModule;
using Core.Network.Methods;
using Masuit.Tools;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using static Core.Network.Methods.Room;
using static Core.RuntimeObject.Download.Host;
using static Core.RuntimeObject.RoomList;

namespace Core.RuntimeObject
{
    public class Download
    {
        public class Host
        {
            #region Private Properties

            #endregion

            #region internal Method
            /// <summary>
            /// 获取avc编码HLS内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns></returns>

            internal static bool GetHlsHost_avc(RoomList.RoomCard roomCard, ref HostClass hostClass)
            {
                if (!RoomList.GetLiveStatus(roomCard.RoomId))
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
                    else if (hostClass.eXTM3U.eXTINFs.Count == 0 && RefreshHostClass(roomCard, ref hostClass))
                    {
                        return false;
                    }
                }
                return false;
            }

            internal static bool RefreshHostClass(RoomList.RoomCard roomCard, ref HostClass m3u8)
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
                    if(string.IsNullOrEmpty(webref))
                    {
                        return false;
                    }
                    Tools.Linq.SerializedM3U8(webref, ref m3u8);
                    return true;
                }
                return false;
            }


            /// <summary>
            /// 获取hevc编码HLS内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns></returns>
            internal static HostClass GetHlsHost_hevc(long RoomId)
            {
                return _GetHost(RoomId, "http_hls", "fmp4", "hevc");
            }

            /// <summary>
            /// 获取avc编码FLV内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns></returns>
            internal static HostClass GetFlvHost_avc(long RoomId)
            {
                return _GetHost(RoomId, "http_stream", "flv", "avc");
            }

            #endregion



            #region Private Method



            private static HostClass _GetHost(long RoomId, string protocol_name, string format_name, string codec_name)
            {
                HostClass hostClass = new();
                PlayInfo_Class playInfo = GetPlayInfo(RoomId);
                if (playInfo == null || playInfo.data.playurl_info == null || playInfo.data.playurl_info.playurl == null)
                    return hostClass;
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
                    extra = urlInfo.extra,
                };
                return hostClass;
            }


            #endregion

            #region internal Class

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

                #endregion

            }
        }

        public class File
        {

            #region Public Method

            /// <summary>
            /// 录制HLS_avc制式的MP4文件
            /// </summary>
            /// <param name="card">房间卡片信息</param>
            /// <returns>是否成功下载</returns>
            public static async Task<(bool isSuccess, string FileName)> DlwnloadHls_avc_mp4(RoomList.RoomCard card)
            {
                bool isSuccess = false;
                string File = string.Empty;
                await Task.Run(() =>
                {
                    // 初始化下载
                    InitializeDownload(card);
                    string title = Tools.KeyCharacterReplacement.CheckFilenames(RoomList.GetTitle(card.UID));
                    long roomId = card.RoomId;
                    string dirName = $"{Config.Core._RecFileDirectory}{card.RoomId}-{RoomList.GetNickname(card.UID)}";
                    bool isInitialized = false;
                    long currentLocation = 0;

                    // 如果目录不存在，则创建目录
                    CreateDirectoryIfNotExists(dirName);
                    File = $"{dirName}/{title}_{DateTime.Now:yyyyMMdd_HHmmss}_{new Random().Next(100, 999)}_original.mp4";
                    // 使用FileStream进行文件操作
                    using (FileStream fs = new FileStream(File, FileMode.Append))
                    {
                        int hlsErrorCount = 0;
                        HostClass hostClass = new();
                        while (!GetHlsHost_avc(card, ref hostClass))
                        {
                            // 处理HLS错误
                            hlsErrorCount = HandleHlsError(hlsErrorCount, card, roomId);
                            if (hlsErrorCount == -1)
                            {
                                System.IO.FileInfo fileInfo = new(File);
                                //文件大于3MB返回true，小于3MB当作无效录制，删除文件并返回false
                                if (fileInfo.Length > 3 * 1024 * 1024)
                                {
                                    isSuccess = true;
                                }
                                else
                                {
                                    Task.Run(() => System.IO.File.Delete(File));                             
                                }
                                return;
                            }
                        }

                        // 记录下载开始
                        LogDownloadStart(card);
                        List<(long size, DateTime time)> values = new();

                        while (true)
                        {
                            long downloadSizeForThisCycle = 0;
                            try
                            {
                                bool isHlsHostAvailable = RefreshHostClass(card, ref hostClass);
                                if (!isHlsHostAvailable)
                                {
                                    // 处理HLS片段错误
                                    hlsErrorCount = HandleHlsSegmentError(hlsErrorCount, card, roomId, ref hostClass);
                                    continue;
                                }
                                else
                                {
                                    if (!isInitialized)
                                    {
                                        isInitialized = true;
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
                                        isSuccess = true;
                                        return;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(2000);
                        }
                    }
                });
                return (DownloadCompletedReset(isSuccess, ref card), File);
            }

            /// <summary>
            /// 初始化下载
            /// </summary>
            /// <param name="card">房间卡片信息</param>
            private static void InitializeDownload(RoomList.RoomCard card)
            {
                card.DownInfo.IsDownload = true;
                card.DownInfo.Status = RoomList.RoomCard.DownloadStatus.Standby;
                _Room.SetRoomCardByUid(card.UID, card);
            }

            /// <summary>
            /// 如果目录不存在，则创建目录
            /// </summary>
            /// <param name="dirName">目录名称</param>
            private static void CreateDirectoryIfNotExists(string dirName)
            {
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
            }

            /// <summary>
            /// 处理HLS错误
            /// </summary>
            /// <param name="hlsErrorCount">HLS错误计数</param>
            /// <param name="card">房间卡片信息</param>
            /// <param name="roomId">房间ID</param>
            /// <returns>更新后的HLS错误计数,返回-1表示已下播</returns>
            private static int HandleHlsError(int hlsErrorCount, RoomList.RoomCard card, long roomId)
            {
                if (hlsErrorCount > 3)
                {
                    hlsErrorCount = 0;
                    Log.Info(nameof(HandleHlsError), $"[{card.Name}({card.RoomId})]获取HLS流失败，10秒后重试该直播间");
                    if (!RoomList.GetLiveStatus(card.RoomId))
                    {
                        return -1;
                    }
                    Thread.Sleep(1000 * 10);
                }
                hlsErrorCount++;
                Thread.Sleep(1000 * 10);
                return hlsErrorCount;
            }

            /// <summary>
            /// 记录下载开始
            /// </summary>
            /// <param name="card">房间卡片信息</param>
            private static void LogDownloadStart(RoomList.RoomCard card)
            {
                string startText = $"({DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")})开始录制任务：\n" +
                $"直播间：{card.RoomId}\n" +
                $"UID：{card.UID}\n" +
                $"昵称：{card.Name}\n" +
                $"标题：{card.Title.Value}";

                Log.Info(nameof(LogDownloadStart), $"{startText}");
                card.DownInfo.Status = RoomList.RoomCard.DownloadStatus.Downloading;
                card.DownInfo.StartTime = DateTime.Now;
            }

            /// <summary>
            /// 处理HLS片段错误
            /// </summary>
            /// <param name="hlsErrorCount">HLS错误计数</param>
            /// <param name="card">房间卡片信息</param>
            /// <param name="roomId">房间ID</param>
            /// <returns>更新后的HLS错误计数</returns>
            private static int HandleHlsSegmentError(int hlsErrorCount, RoomList.RoomCard card, long roomId, ref HostClass hostClass)
            {
                hlsErrorCount++;
                if (hlsErrorCount > 3)
                {
                    Log.Info(nameof(HandleHlsSegmentError), $"[{card.Name}({card.RoomId})]获取HLS片段失败，跳过这个片段等待下一个周期重试", false);
                    if (!RoomList.GetLiveStatus(card.RoomId))
                    {
                        return hlsErrorCount;
                    }
                    else
                    {
                        GetHlsHost_avc(card, ref hostClass);
                    }
                    Thread.Sleep(2000);
                }
                Thread.Sleep(1000);
                return hlsErrorCount;
            }

            /// <summary>
            /// 更新下载速度
            /// </summary>
            /// <param name="values">下载值列表</param>
            /// <param name="card">房间卡片信息</param>
            /// <param name="downloadSizeForThisCycle">本周期下载大小</param>
            /// <returns>更新后的下载值列表</returns>
            private static List<(long size, DateTime time)> UpdateDownloadSpeed(List<(long size, DateTime time)> values, RoomList.RoomCard card, long downloadSizeForThisCycle)
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
            /// byte长度转换为表示大小的字符串
            /// </summary>
            /// <param name="ByteLen"></param>
            /// <returns></returns>
            public static string ByteToSizeConversion(double ByteLen)
            {
                return Tools.Linq.ConversionSize(ByteLen, Tools.Linq.ConversionSizeType.BitRate);
            }

            #endregion

            #region Private Method

            /// <summary>
            /// 下载完成重置房间卡状态
            /// </summary>
            /// <param name="roomCard"></param>
            private static bool DownloadCompletedReset(bool NormalEnd, ref RoomList.RoomCard roomCard)
            {
                Log.Info(nameof(DownloadCompletedReset), $"[{roomCard.Name}({roomCard.RoomId})]进行录制完成处理");
                roomCard.DownInfo.IsDownload = false;
                roomCard.DownInfo.DownloadSize = 0;
                roomCard.DownInfo.RealTimeDownloadSpe = 0;
                roomCard.DownInfo.Status = RoomList.RoomCard.DownloadStatus.DownloadComplete;
                roomCard.DownInfo.EndTime = DateTime.Now;
                _Room.SetRoomCardByUid(roomCard.UID, roomCard);
                return NormalEnd;
            }


            /// <summary>
            /// 从网络写入文件
            /// </summary>
            /// <param name="fs">待写入的FileStream</param>
            /// <param name="url">待写入的文件原始网络路径</param>
            /// <returns>写入的文件byte数</returns>
            private static long WriteToFile(FileStream fs, string url)
            {
                byte[] InitialFragment = Network.Download.File.GetFileToByte(url, true, "https://www.bilibili.com/");
                if (InitialFragment != null)
                {
                    fs.Write(InitialFragment, 0, InitialFragment.Length);
                    int len = InitialFragment.Length;
                    InitialFragment = null;
                    return len;
                }
                else
                {
                    return 0;
                }
            }
            #endregion

            #region Public Class

            #endregion

        }
    }

}
