using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.DownloadModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.HLS
{
    public class HLS_Host
    {
        public static HLSHostClass Get_HLS_Host(ref RoomInfoClass.RoomInfo roomInfo, ref DownloadClass.Downloads downloads, bool IsNewTask = false)
        {
            HLSHostClass hLSHostClass = new HLSHostClass();
            string WebText = NetworkRequestModule.Get.Get.GetRequest($"{ConfigModule.CoreConfig.ReplaceAPI}/xlive/web-room/v2/index/getRoomPlayInfo?room_id={roomInfo.room_id}&protocol=0,1&format=0,1,2&codec=0,1&qn={(int)DownloadModule.Download.RecQuality}&platform=h5&ptype=8", false, "https://www.bilibili.com/");
            ApiClass.BilibiliApiResponse<ApiClass.RoomPlayInfo> response = null;
            int error = 0;
            int NoHlsSteam = 0;
            bool IsFlvSteam = false;
            while (true)
            {

                if (error > 5 || (IsFlvSteam && (NoHlsSteam > (IsNewTask ? 15 : 10))) || (!IsFlvSteam && NoHlsSteam > 35))
                {
                    Log.Log.AddLog(nameof(HLS_Host), Log.LogClass.LogType.Info, $"【{roomInfo.uname}({roomInfo.uid}:{roomInfo.room_id})】获取HLS地址时重试10次超时");
                    return hLSHostClass;
                }
                if (string.IsNullOrEmpty(WebText))
                {
                    error++;
                   
                    WebText = NetworkRequestModule.Get.Get.GetRequest($"{ConfigModule.CoreConfig.ReplaceAPI}/xlive/web-room/v2/index/getRoomPlayInfo?room_id={roomInfo.room_id}&protocol=0,1&format=0,1,2&codec=0,1&qn={(int)Download.RecQuality}&platform=h5&ptype=8", false, "https://www.bilibili.com/");
                }
                else
                {
                    response = JsonConvert.DeserializeObject<ApiClass.BilibiliApiResponse<ApiClass.RoomPlayInfo>>(WebText);
                }
                if (response != null)
                {
                    if (response.Data.LiveStatus != 1)
                    {
                        Log.Log.AddLog(nameof(HLS_Host), Log.LogClass.LogType.Info, $"获取【{roomInfo.uname}({roomInfo.uid}:{roomInfo.room_id})】的直播流时发现直播间已经下播，任务结束");
                        hLSHostClass.LiveStatus = false;
                        return hLSHostClass;
                    }
                    foreach (var Stream in response.Data.PlayurlInfo.Playurl.Streams)
                    {
                        if (Stream.ProtocolName == "http_hls")
                        {
                            foreach (var Format in Stream.Formats)
                            {
                                if (Format.FormatName.ToLower() == "fmp4")
                                {
                                    hLSHostClass.IsEffective = true;
                                    hLSHostClass.host = Format.Codecs[0].UrlInfos[0].Host;
                                    hLSHostClass.extra = Format.Codecs[0].UrlInfos[0].Extra.Replace("\u0026", "&");
                                    hLSHostClass.base_url = Format.Codecs[0].BaseUrl;
                                    hLSHostClass.base_file_name = hLSHostClass.base_url.Split('/')[hLSHostClass.base_url.Split('/').Length - 1];
                                    hLSHostClass.base_url = hLSHostClass.base_url.Replace(hLSHostClass.base_url.Split('/')[hLSHostClass.base_url.Split('/').Length - 1], "");
                                    hLSHostClass.ExtendedName = "m4s";
                                    roomInfo.Host = "[HLS] " + hLSHostClass.host;
                                    downloads.ExtendedName = "m4s";
                                    return hLSHostClass;
                                }
                            }
                        }
                    }
                    if (!IsFlvSteam)
                    {
                        string FlvFileUrl = RoomInfo.GetPlayUrl(roomInfo.uid, (RoomInfoClass.Quality)Download.RecQuality);
                        if (Tool.FileOperation.IsExistsNetFile(FlvFileUrl))
                        {
                            IsFlvSteam = true;
                        }
                    }

                    NoHlsSteam++;
                    Thread.Sleep(1000);
                }
                else
                {
                    Log.Log.AddLog(nameof(HLS_Host), Log.LogClass.LogType.Debug, $"【{roomInfo.uname}({roomInfo.uid}:{roomInfo.room_id})】获取HLS地址时网络超时，尝试重试");
                }
            }
            return hLSHostClass;
        }
        public class HLSHostClass
        {
            public bool IsEffective { get; set; } = false;
            public bool LiveStatus { get; set; } = true;
            public string host { get; set; }
            public string extra { get; set; }
            public string base_url { get; set; }
            public string base_file_name { get; set; }
            public string ExtendedName { get; set; }
        }

    }
}
