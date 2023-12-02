using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;

namespace Core.RuntimeObject
{
    public class Download
    {
        public class Host
        {
            #region Private Properties

            #endregion

            #region Public Method
            public static (bool Effective, string host, string base_url, string extra) GetHlsHost_avc(long RoomId)
            {
                return _GetHlsHost_avc(RoomId);
            }
            public static (bool Effective, string host, string base_url, string extra) GetHlsHost_hevc(long RoomId)
            {
                return _GetHlsHost_hevc(RoomId);
            }
            public static (bool Effective, string host, string base_url, string extra) GetFlvHost_avc(long RoomId)
            {
                return _GetFlvHost_avc(RoomId);
            }

            #endregion

            #region Private Method

            /// <summary>
            /// 获取avc编码HLS内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns>结果是否有效,host,base_url,extra</returns>
            private static (bool Effective, string host, string base_url, string extra) _GetHlsHost_avc(long RoomId)
            {
                PlayInfo playInfo = GetPlayInfo(RoomId);
                if (playInfo == null || playInfo.data.playurl_info.playurl == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Stream? stream = playInfo.data.playurl_info.playurl.stream.FirstOrDefault(x => x.protocol_name == "http_hls");
                if (stream == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Format? format = stream.format.FirstOrDefault(x => x.format_name == "fmp4");
                if (format == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Codec? codec = format.codec.FirstOrDefault(x => x.codec_name == "avc");
                if (codec == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                var urlInfo = codec.url_info.FirstOrDefault(x => x.host.Contains("d1--cn")) ?? codec.url_info[new Random().Next(0, codec.url_info.Count - 1)];
                return (true, urlInfo.host, codec.base_url, urlInfo.extra);
            }

            /// <summary>
            /// 获取hevc编码HLS内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns>结果是否有效,host,base_url,extra</returns>
            private static (bool Effective, string host, string base_url, string extra) _GetHlsHost_hevc(long RoomId)
            {
                PlayInfo playInfo = GetPlayInfo(RoomId);
                if (playInfo == null || playInfo.data.playurl_info.playurl == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Stream? stream = playInfo.data.playurl_info.playurl.stream.FirstOrDefault(x => x.protocol_name == "http_hls");
                if (stream == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Format? format = stream.format.FirstOrDefault(x => x.format_name == "fmp4");
                if (format == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Codec? codec = format.codec.FirstOrDefault(x => x.codec_name == "hevc");
                if (codec == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                var urlInfo = codec.url_info.FirstOrDefault(x => x.host.Contains("d1--cn")) ?? codec.url_info[new Random().Next(0, codec.url_info.Count - 1)];
                return (true, urlInfo.host, codec.base_url, urlInfo.extra);
            }

            /// <summary>
            /// 获取avc编码FLV内容
            /// </summary>
            /// <param name="RoomId"></param>
            /// <returns>结果是否有效,host,base_url,extra</returns>
            private static (bool Effective, string host, string base_url, string extra) _GetFlvHost_avc(long RoomId)
            {
                PlayInfo playInfo = GetPlayInfo(RoomId);
                if (playInfo == null || playInfo.data.playurl_info.playurl == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Stream? stream = playInfo.data.playurl_info.playurl.stream.FirstOrDefault(x => x.protocol_name == "http_stream");
                if (stream == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Format? format = stream.format.FirstOrDefault(x => x.format_name == "flv");
                if (format == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                PlayInfo.Codec? codec = format.codec.FirstOrDefault(x => x.codec_name == "avc");
                if (codec == null)
                    return (false, string.Empty, string.Empty, string.Empty);
                var urlInfo = codec.url_info.FirstOrDefault(x => x.host.Contains("d1--cn")) ?? codec.url_info[new Random().Next(0, codec.url_info.Count - 1)];
                return (true, urlInfo.host, codec.base_url, urlInfo.extra);
            }


            #endregion

            #region Public Class

            #endregion



        }

        public class File
        {
            public static void DlwnloadHlsMp4(long RoomId,string DirName,string FileName)
            {
                while (true)
                {
                    (bool Effective, string host, string base_url, string extra) a = Host.GetHlsHost_avc(21756924);

                    string A = Network.Download.File.GetFileToString($"{a.host}{a.base_url}?{a.extra}");
                }
            }
        }

    }
}
