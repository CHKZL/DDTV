using DDTV_Core.SystemAssembly.ConfigModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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
            Task.Run(() =>
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
                JObject JO = (JObject)JsonConvert.DeserializeObject(NetworkRequestModule.Post.Post.SendRequest_SendDanmu($"{DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.ReplaceAPI}/msg/send", Params, CK));
                Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"弹幕发送成功");
            });
        }

        /// <summary>
        /// 获取屏蔽词和屏蔽用户uid
        /// </summary>
        /// <returns></returns>
        public static ShieldInfo GetShieldList()
        {
            ShieldInfo shieldInfo = new ShieldInfo();
            string WebStr = NetworkRequestModule.Get.Get.GetRequest($"{CoreConfig.ReplaceAPI}/xlive/web-room/v1/index/getInfoByUser?room_id=6374209&from=0&not_mock_enter_effect=0");
            ShielWebInfo? shielWebInfo = JsonConvert.DeserializeObject<ShielWebInfo>(WebStr);
            if(shielWebInfo!=null)
            {
                foreach (var item in shielWebInfo.data.shield_info.shield_user_list)
                {
                    shieldInfo.uids.Add(item.uid);
                }
                foreach (var item in shielWebInfo.data.shield_info.keyword_list)
                {
                    shieldInfo.keyword_list.Add(item);
                }
            }
            return shieldInfo;
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
                uid = uid
            };
            string RoomId = Rooms.Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id);
            string data = "";
            do
            {
                Thread.Sleep(1000 * new Random().Next(1, 5));
                data = NetworkRequestModule.Get.Get.GetRequest($"{DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.ReplaceAPI}/xlive/web-room/v1/index/getDanmuInfo?id=" + RoomId, false);
                if (!string.IsNullOrEmpty(data))
                {
                    break;
                }
                Thread.Sleep(1000 * new Random().Next(7, 14));
            } while (string.IsNullOrEmpty(data));
            JObject JO = (JObject)JsonConvert.DeserializeObject(data);
            if (JO != null && JO.ContainsKey("code") && JO["code"] != null && (int)JO["code"] == 0)
            {
                if (JO.ContainsKey("data") && JO["data"].Count() > 0)
                {
                    danMuWssInfo.token = JO["data"]["token"].ToString();
                    if (JO["data"]["host_list"].Count() > 0)
                    {
                        foreach (var item in JO["data"]["host_list"])
                        {
                            danMuWssInfo.host_list.Add(new DanMuClass.Host()
                            {
                                host = item["host"].ToString(),
                                port = (int)item["port"],
                                wss_port = (int)item["wss_port"],
                                ws_port = (int)item["ws_port"],
                            });
                        }
                        Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"获取【[UID]：{uid}】的直播间弹幕信息长连接服务器信息成功");
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"获取【[UID]：{uid}】的直播间wss信息失败，返回的host_list长度为0或空");
                    }
                }
                else
                {
                    Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"获取【[UID]：{uid}】的直播间wss信息失败，返回的data长度为0或空");
                }
            }
            else
            {
                Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"获取【[UID]：{uid}】的直播间wss信息失败，返回的code值不为0或数据为空");
            }
            return danMuWssInfo;
        }

        /// <summary>
        /// 屏蔽信息
        /// </summary>
        public class ShieldInfo
        {
            /// <summary>
            /// 文字匹配规则
            /// </summary>
            public List<string> keyword_list { get; set; } = new List<string>();
            /// <summary>
            /// UID屏蔽
            /// </summary>
            public List<long> uids { get; set; } = new List<long>();
        }
        public class ShielWebInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public long code { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string message { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long ttl { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public Data data { get; set; } =  new();
            public class User_level
            {
                /// <summary>
                /// 
                /// </summary>
                public long level { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public long next_level { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public long color { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public string level_rank { get; set; }

            }
            public class Vip
            {
                /// <summary>
                /// 
                /// </summary>
                public long vip { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public string vip_time { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public long svip { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public string svip_time { get; set; }

            }
            public class Info
            {
                /// <summary>
                /// 
                /// </summary>
                public long uid { get; set; }

                /// <summary>
                /// 某米SYXM
                /// </summary>
                public string uname { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public string uface { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public long main_rank { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public long bili_vip { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public long mobile_verify { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public long identification { get; set; }

            }
            public class Shield_user_listItem
            {
                /// <summary>
                /// 
                /// </summary>
                public long uid { get; set; }

                /// <summary>
                /// 今天的俊糖欧了嘛
                /// </summary>
                public string uname { get; set; }

            }
            public class Shield_info
            {
                /// <summary>
                /// 
                /// </summary>
                public List<Shield_user_listItem> shield_user_list { get; set; } = new();

                /// <summary>
                /// 
                /// </summary>
                public List<string> keyword_list { get; set; } = new();
            }
            public class Data
            {
                /// <summary>
                /// 
                /// </summary>
                public User_level user_level { get; set; } = new();

                /// <summary>
                /// 
                /// </summary>
                public Vip vip { get; set; } = new();

                /// <summary>
                /// 
                /// </summary>
                public Info info { get; set; } = new();

                /// <summary>
                /// 
                /// </summary>
                public Shield_info shield_info { get; set; } = new();

            }
        }
    }
}
