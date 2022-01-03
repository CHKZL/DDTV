using DDTV_Core.SystemAssembly.ConfigModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu
{
    public class DanMu
    {
        /// <summary>
        /// 发送直播间弹幕
        /// </summary>
        /// <param name="roomId">房间号(长房间号)</param>
        /// <param name="Message">发送信息(不能超过20个字符)</param>
        public static void Send(string roomId, string Message)
        {
            CookieContainer CK = NetworkRequestModule.NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie);
            Dictionary<string, string> Params = new Dictionary<string, string>
                {
                    { "color", "16777215" },
                    { "fontsize", "25" },
                    { "mode", "1" },
                    { "msg", Message },
                    { "rnd", (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalSeconds.ToString() },
                    { "roomid", roomId },
                    { "csrf_token", BilibiliUserConfig.account.csrf },
                    { "csrf", BilibiliUserConfig.account.csrf }
                };
            JObject JO = (JObject)JsonConvert.DeserializeObject(NetworkRequestModule.Post.Post.SendRequest_SendDanmu("https://api.live.bilibili.com/msg/send", Params, CK));
            Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"弹幕发送成功");
        }
        /// <summary>
        /// 获取直播间弹幕连接(wss长连接)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        internal static DanMuClass.DanMuWssInfo getDanmuInfo(long uid)
        {
            DanMuClass.DanMuWssInfo danMuWssInfo = new DanMuClass.DanMuWssInfo()
            {
                uid=uid
            };
            string RoomId = Rooms.Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id);
            JObject JO = (JObject)JsonConvert.DeserializeObject(NetworkRequestModule.Get.Get.GetRequest("https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo?id="+RoomId,false));
            if (JO!=null&&JO.ContainsKey("code")&&JO["code"]!=null&&(int)JO["code"]==0)
            {
                if(JO.ContainsKey("data")&&JO["data"].Count()>0)
                {
                    danMuWssInfo.token=JO["data"]["token"].ToString();
                    if (JO["data"]["host_list"].Count()>0)
                    {
                        foreach (var item in JO["data"]["host_list"])
                        {
                            danMuWssInfo.host_list.Add(new DanMuClass.Host()
                            {
                                host=item["host"].ToString(),
                                port=(int)item["port"],
                                wss_port=(int)item["wss_port"],
                                ws_port=(int)item["ws_port"],
                            });
                        }
                        Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"获取【[UID]：{uid}】的直播间wss长连接服务器信息成功");
                    }
                }             
            }
            else
            {
                Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"获取【[UID]：{uid}】的直播间wss信息失败，返回的code值不为0或数据为空");
            }
            return danMuWssInfo;
        }

    }
}
