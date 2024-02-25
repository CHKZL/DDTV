using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;

namespace Core.RuntimeObject.Download
{
    public class Basics
    {
        #region internal Method

        /// <summary>
        /// 初始化下载
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        internal static void InitializeDownload(RoomCardClass card)
        {
            card.DownInfo.Status = RoomCardClass.DownloadStatus.Standby;
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
        internal static void LogDownloadStart(RoomCardClass card)
        {
            string startText = $"开始录制任务：" +
            $"{card.Name}({card.RoomId})|" +
            $"UID：{card.UID}|" +
            $"标题：{card.Title.Value}";
            Log.Info(nameof(LogDownloadStart), $"{startText}");
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
                else if (hostClass.eXTM3U.eXTINFs.Count == 0 && RefreshHlsHostClass(roomCard, ref hostClass))
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
        internal static bool RefreshHlsHostClass(RoomCardClass roomCard, ref HostClass m3u8)
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

            #endregion
        }
    }
}
