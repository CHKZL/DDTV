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
using static Core.RuntimeObject.Download.File;
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

            #region Public Method
            /// <summary>
            /// 获取avc编码HLS内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns></returns>

            public static HostClass GetHlsHost_avc(long RoomId)
            {
                HostClass m3u8 = _GetHost(RoomId, "http_hls", "fmp4", "avc");
                if (m3u8.Effective)
                {
                    EXTM3U eXTM3U = Tools.Linq.SerializedM3U8(Network.Download.File.GetFileToString($"{m3u8.host}{m3u8.base_url}{m3u8.uri_name}{m3u8.extra}"));
                    m3u8.SteramInfo = eXTM3U.SteramInfo;
                }
                return m3u8;
            }

            /// <summary>
            /// 获取hevc编码HLS内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns></returns>
            public static HostClass GetHlsHost_hevc(long RoomId)
            {
                return _GetHost(RoomId, "http_hls", "fmp4", "hevc");
            }

            /// <summary>
            /// 获取avc编码FLV内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns></returns>
            public static HostClass GetFlvHost_avc(long RoomId)
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
                var urlInfo = codec.url_info.FirstOrDefault(x => x.host.Contains("d1--cn")) ?? codec.url_info[new Random().Next(0, codec.url_info.Count - 1)];

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

            #region Public Class

            public class HostClass
            {
                /// <summary>
                /// 是否有有效Host信息
                /// </summary>
                public bool Effective { get; set; } = false;
                /// <summary>
                /// host地址
                /// </summary>
                public string host { get; set; }
                /// <summary>
                /// 路由信息
                /// </summary>
                public string base_url { get; set; }
                /// <summary>
                /// 目录文件名
                /// </summary>
                public string uri_name { get; set; }
                /// <summary>
                /// 参数
                /// </summary>
                public string extra { get; set; }
                /// <summary>
                /// M3U8头
                /// </summary>
                internal string SteramInfo { get; set; }
            }

            #endregion



        }

        public class File
        {

            #region Public Method

            /// <summary>
            /// 录制HLS_avc制式的MP4文件
            /// </summary>
            /// <param name="Card"></param>
            /// <returns></returns>
            public static async Task<bool> DlwnloadHls_avc_Mp4(RoomList.RoomCard Card)
            {
                bool Success = false;
                Card.DownInfo.IsDownload = true;
                Card.DownInfo.Status = RoomList.RoomCard.DownloadStatus.Standby;
                string Title = Tools.KeyCharacterReplacement.CheckFilenames(RoomList.GetTitle(RoomList.GetUid(Card.RoomId)));
                long RoomId = Card.RoomId;
                string DirName = $"{Config.Core._RecFileDirectory}{Card.RoomId}-{RoomList.GetNickname(RoomList.GetUid(Card.RoomId))}";
                bool Initialization = false;
                long CurrentLocation = 0;
                DateTime LastTime = DateTime.MinValue;
                if (!Directory.Exists(DirName))
                {
                    Directory.CreateDirectory(DirName);
                }
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                await Task.Run(() =>
                {
                    using (FileStream fs = new FileStream($"{DirName}/{Title}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4", FileMode.Append))
                    {
                        int HLS_Error_Count = 0;
                        HostClass m3u8 = new();
                        do
                        {
                            if (HLS_Error_Count > 3)
                            {
                                Log.Info(nameof(Download), $"获取HLS流失败，放弃该任务");

                                Success = false;
                                cts.Cancel();
                            }
                            m3u8 = GetHlsHost_avc(RoomId);
                            HLS_Error_Count++;
                            Thread.Sleep(1000 * 10);
                        } while (!m3u8.Effective);
                        string StarText = $"({DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")})开始录制任务:\n" +
                        $"直播间:{Card.RoomId}\n" +
                        $"UID:{Card.UID}\n" +
                        $"昵称:{Card.Name}\n" +
                        $"标题:{Card.Title.Value}";

                        Log.Info(nameof(Download), $"{StarText}");
                        Card.DownInfo.Status = RoomList.RoomCard.DownloadStatus.Downloading;
                        Card.DownInfo.StartTime = DateTime.Now;
                        while (true)
                        {
                            long DownloadSizeForThisCycle = 0;
                            try
                            {
                                if (m3u8.Effective)
                                {
                                    EXTM3U eXTM3U = !string.IsNullOrEmpty(m3u8.SteramInfo) ? Tools.Linq.SerializedM3U8(Network.Download.File.GetFileToString(m3u8.SteramInfo)) : Tools.Linq.SerializedM3U8(Network.Download.File.GetFileToString($"{m3u8.host}{m3u8.base_url}{m3u8.uri_name}?{m3u8.extra}"));
                                    if (eXTM3U.eXTINFs.Count < 1 || string.IsNullOrEmpty(m3u8.SteramInfo))
                                    {
                                        Thread.Sleep(500);
                                        m3u8 = GetHlsHost_avc(RoomId);
                                        eXTM3U = !string.IsNullOrEmpty(m3u8.SteramInfo) ? Tools.Linq.SerializedM3U8(Network.Download.File.GetFileToString(m3u8.SteramInfo)) : Tools.Linq.SerializedM3U8(Network.Download.File.GetFileToString($"{m3u8.host}{m3u8.base_url}{m3u8.uri_name}?{m3u8.extra}"));
                                    }
                                    if (!Initialization)
                                    {
                                        Initialization = true;
                                        DownloadSizeForThisCycle += WriteToFile(fs, $"{m3u8.host}{m3u8.base_url}{eXTM3U.Map_URI}?{m3u8.extra}");
                                    }
                                    foreach (var item in eXTM3U.eXTINFs)
                                    {
                                        long.TryParse(item.FileName, out long index);
                                        if (index > CurrentLocation)
                                        {
                                            DownloadSizeForThisCycle += WriteToFile(fs, $"{m3u8.host}{m3u8.base_url}{item.FileName}.{item.ExtensionName}?{m3u8.extra}");
                                            CurrentLocation = index;
                                        }
                                    }
                                    double mt = DateTime.Now.Subtract(LastTime).TotalMilliseconds;
                                    Card.DownInfo.RealTimeDownloadSpe = (DownloadSizeForThisCycle / mt) * 1000;
                                    LastTime = DateTime.Now;
                                    Card.DownInfo.DownloadSize = Card.DownInfo.DownloadSize += DownloadSizeForThisCycle;
                                    if (eXTM3U.IsEND)
                                        Success = true;
                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    Success = false;
                                }
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(500);
                                m3u8 = GetHlsHost_avc(RoomId);
                            }
                        }
                    }
                },token);
                DownloadCompletedReset(Card);
                return Success;
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
            private static void DownloadCompletedReset(RoomList.RoomCard roomCard)
            {

                roomCard.DownInfo.IsDownload = false;
                roomCard.DownInfo.DownloadSize = 0;
                roomCard.DownInfo.RealTimeDownloadSpe = 0;
                roomCard.DownInfo.Status = RoomList.RoomCard.DownloadStatus.DownloadComplete;
                roomCard.DownInfo.EndTime = DateTime.Now;
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
                fs.Write(InitialFragment, 0, InitialFragment.Length);
                return InitialFragment.Length;
            }
            #endregion

            #region Public Class
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
                /// M3U8头
                /// </summary>
                internal string SteramInfo { get; set; }
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
                #endregion

            }
        }
    }
}
