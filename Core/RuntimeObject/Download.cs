using Masuit.Tools;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using static Core.Network.Methods.Room;
using static Core.RuntimeObject.Download.Host;

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
                return _GetHost(RoomId, "http_hls", "fmp4", "avc");
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
                PlayInfo playInfo = GetPlayInfo(RoomId);
                if (playInfo == null || playInfo.data.playurl_info.playurl == null)
                    return hostClass;
                PlayInfo.Stream? stream = playInfo.data.playurl_info.playurl.stream.FirstOrDefault(x => x.protocol_name == protocol_name);
                if (stream == null)
                    return hostClass;
                PlayInfo.Format? format = stream.format.FirstOrDefault(x => x.format_name == format_name);
                if (format == null)
                    return hostClass;
                PlayInfo.Codec? codec = format.codec.FirstOrDefault(x => x.codec_name == codec_name);
                if (codec == null)
                    return hostClass;
                var urlInfo = codec.url_info.FirstOrDefault(x => x.host.Contains("d1--cn")) ?? codec.url_info[new Random().Next(0, codec.url_info.Count - 1)];

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
            }

            #endregion



        }

        public class File
        {

            #region Public Method

            #endregion

            #region Private Method


            /// <summary>
            /// 录制HLS_avc制式的MP4文件
            /// </summary>
            /// <param name="RoomId">房间号</param>
            /// <param name="DirName">保存的路径</param>
            /// <param name="Title">标题</param>
            public static void DlwnloadHls_avc_Mp4(long RoomId, string DirName, string Title)
            {
                bool Initialization = false;
                long CurrentLocation = 0;
                if (!Directory.Exists(DirName))
                {
                    Directory.CreateDirectory(DirName);
                }
                using (FileStream fs = new FileStream($"{DirName}/{Title}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4", FileMode.Append))
                {
                    while (true)
                    {
                        try
                        {
                            HostClass m3u8 = GetHlsHost_avc(RoomId);
                            if (m3u8.Effective)
                            {
                                EXTM3U eXTM3U = Tools.Linq.SerializedM3U8(Network.Download.File.GetFileToString($"{m3u8.host}{m3u8.base_url}{m3u8.uri_name}?{m3u8.extra}"));
                                if (!Initialization)
                                {
                                    Initialization = true;
                                    WriteToFile(fs, $"{m3u8.host}{m3u8.base_url}{eXTM3U.Map_URI}?{m3u8.extra}", eXTM3U.Map_URI);
                                }
                                foreach (var item in eXTM3U.eXTINFs)
                                {
                                    long.TryParse(item.FileName, out long index);
                                    if (index > CurrentLocation)
                                    {
                                        WriteToFile(fs, $"{m3u8.host}{m3u8.base_url}{item.FileName}.{item.ExtensionName}?{m3u8.extra}", item.FileName);
                                        CurrentLocation = index;
                                    }
                                }
                                if (eXTM3U.IsEND)
                                    break;
                                Thread.Sleep(1500);
                            }
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            }

            /// <summary>
            /// 文件写入
            /// </summary>
            /// <param name="fs"></param>
            /// <param name="url"></param>
            /// <param name="fileName"></param>
            private static void WriteToFile(FileStream fs, string url, string fileName)
            {
                byte[] InitialFragment = Network.Download.File.GetFileToByte(url);
                fs.Write(InitialFragment, 0, InitialFragment.Length);
                Console.WriteLine($"写入文件{fileName}");
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
