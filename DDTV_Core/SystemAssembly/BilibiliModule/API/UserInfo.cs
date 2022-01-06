using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.DataCacheModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API
{
    public class UserInfo
    {
        /// <summary>
        /// 获取账号信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        internal static RoomInfoClass.RoomInfo info(long uid)
        {
            string WebText = NetworkRequestModule.Get.Get.GetRequest("http://api.bilibili.com/x/space/acc/info?mid=" + uid);

            if (string.IsNullOrEmpty(WebText))
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"info获取网络数据为空或超时");
                Thread.Sleep(500);
                return info(uid);
            }

            JObject JO = (JObject)JsonConvert.DeserializeObject(WebText);
            if (JO != null && JO.ContainsKey("code") && JO["code"] != null && (int)JO["code"] == 0)
            {
                if (JO.TryGetValue("data", out var RoomInit) && RoomInit != null)
                {
                    RoomInfoClass.RoomInfo room = new RoomInfoClass.RoomInfo();
                    User.UserClass.Data data = new User.UserClass.Data();
                    try
                    {
                        data = JsonConvert.DeserializeObject<User.UserClass.Data>(RoomInit.ToString());
                    }
                    catch (Exception e)
                    {
                        Log.Log.AddLog(nameof(UserInfo), Log.LogClass.LogType.Warn, $"房间信息获取失败，错误json字符串内容:\n{RoomInit.ToString()}", true, e);
                        return null;
                    }


                    if (Rooms.Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
                    {
                        Rooms.Rooms.RoomInfo[uid].room_id = data.live_room.roomid;
                        Rooms.Rooms.RoomInfo[uid].level = data.level;
                        Rooms.Rooms.RoomInfo[uid].live_status = data.live_room.liveStatus;
                        Rooms.Rooms.RoomInfo[uid].uname = data.name;
                        Rooms.Rooms.RoomInfo[uid].face = data.face;
                        Rooms.Rooms.RoomInfo[uid].roomStatus = data.live_room.roomStatus;
                        Rooms.Rooms.RoomInfo[uid].sex = data.sex;
                        Rooms.Rooms.RoomInfo[uid].sign = data.sign;
                        Rooms.Rooms.RoomInfo[uid].title = Tool.FileOperation.CheckFilenames(data.live_room.title);
                        Rooms.Rooms.RoomInfo[uid].url = data.live_room.url;
                        Rooms.Rooms.RoomInfo[uid].roundStatus = data.live_room.roundStatus;
                        room = Rooms.Rooms.RoomInfo[uid];
                    }
                    else
                    {
                        RoomInfoClass.RoomInfo roomInfo1 = new()
                        {
                            uid = uid,
                            room_id = data.live_room.roomid,
                            level = data.level,
                            live_status = data.live_room.liveStatus,
                            uname = data.name,
                            face = data.face,
                            roomStatus = data.live_room.roomStatus,
                            sex = data.sex,
                            sign = data.sign,
                            title = Tool.FileOperation.CheckFilenames(data.live_room.title),
                            url = data.live_room.url,
                            roundStatus = data.live_room.roundStatus
                        };
                        Rooms.Rooms.RoomInfo.Add(uid, roomInfo1);
                        room = roomInfo1;
                    }
                    DataCache.SetCache(CacheType.room_id, uid.ToString(), data.live_room.roomid.ToString(), int.MaxValue);
                    DataCache.SetCache(CacheType.level, uid.ToString(), data.level.ToString(), 3600 * 1000);
                    DataCache.SetCache(CacheType.live_status, uid.ToString(), data.live_room.liveStatus.ToString(), 0);
                    DataCache.SetCache(CacheType.uname, uid.ToString(), data.name, int.MaxValue);
                    DataCache.SetCache(CacheType.face, uid.ToString(), data.face, int.MaxValue);
                    DataCache.SetCache(CacheType.roomStatus, uid.ToString(), data.live_room.roomStatus.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.sex, uid.ToString(), data.sex, int.MaxValue);
                    DataCache.SetCache(CacheType.sign, uid.ToString(), data.sign, int.MaxValue);
                    DataCache.SetCache(CacheType.title, uid.ToString(), data.live_room.title, 0);
                    DataCache.SetCache(CacheType.url, uid.ToString(), data.live_room.url, 1 * 1000);
                    DataCache.SetCache(CacheType.roundStatus, uid.ToString(), data.live_room.roundStatus.ToString(), 0);
                    //Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"获取用户[{uid}]的直播房间get_info信息成功");
                    return room;
                }
            }
            return null;
        }
        /// <summary>
        /// 检测登录是否有效
        /// </summary>
        /// <returns></returns>
        public static bool LoginValidityVerification()
        {
            if(string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
            {
                return false;
            }
            string WebText = NetworkRequestModule.Get.Get.GetRequest("https://api.bilibili.com/x/web-interface/nav");
            JObject root= (JObject)JsonConvert.DeserializeObject(WebText);
            if(root.TryGetValue("data",out var data))
            {
                try
                {
                    if (bool.TryParse(data["isLogin"].ToString(),out bool T))
                    {
                        if (T)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(UserInfo), Log.LogClass.LogType.Warn, $"请求账号nav信息isLogin值是出现未知问题,请求到的数据为:{WebText}");
                        return true;
                    }
                }
                catch (Exception)
                {
                    Log.Log.AddLog(nameof(UserInfo), Log.LogClass.LogType.Warn, $"请求账号nav信息出现解析错误,请求到的数据为:{WebText}");
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        internal class Root
        {
            /// <summary>
            /// 
            /// </summary>
            internal int code { get; set; }
            /// <summary>
            /// 消息
            /// </summary>
            internal string message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            internal int ttl { get; set; }
            /// <summary>
            /// 
            /// </summary>
            internal Data data { get; set; }
            internal class Data
            {
                /// <summary>
                /// 是否登陆
                /// </summary>
                internal string isLogin { get; set; }
            }
        }

        /// <summary>
        /// 添加关注列表中的V到本地房间配置文件
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static List<followClass> follow(long uid)
        {
            int AddCount = 0;
            List<User.follow.ListItem> FollowList = new();
            int pg = 0;
            int total = 0; 
            do
            {
                pg++;
                string WebText = NetworkRequestModule.Get.Get.GetRequest("https://api.bilibili.com/x/relation/followings?vmid=" + uid + "&pn=" + pg + "&ps=50&order=desc&jsonp=jsonp");
                User.follow.Root root = JsonConvert.DeserializeObject<User.follow.Root>(WebText);
                if (root.code == 0 && root.data.list.Count > 0)
                {
                    foreach (var item in root.data.list)
                    {
                        FollowList.Add(item);
                    }
                }
                total = root.data.total;
            } while (pg * 50 < total);
            List<followClass> followClasses = new List<followClass>();
            if (File.Exists("./Resources/vtbs.json"))
            {
                JArray keyValuePairs = (JArray)JsonConvert.DeserializeObject(File.ReadAllText("./Resources/vtbs.json"));
                List<User.VtbsFile> vtbsFiles = new();
                foreach (var item in keyValuePairs)
                {
                    long mid = 0;
                    int roomid = 0;
                    string name = "";
                    try
                    {
                        mid = long.Parse(item["mid"].ToString());
                        roomid = int.Parse(item["roomid"].ToString());
                        name = Tool.FileOperation.CheckFilenames(item["uname"].ToString());
                        if(FollowList.Any(e => e.mid == mid)&& mid!=0&&roomid!=0)
                        {
                            followClasses.Add(new followClass()
                            {
                                mid = mid,
                                name = name,
                                roomid = roomid
                            });
                            RoomConfig.AddRoom(mid, roomid, name);
                            AddCount++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                RoomConfigFile.WriteRoomConfigFile();
            }
            return followClasses;
        }
        public class followClass
        {
            public long mid;
            public int roomid;
            public string name;
        }
    }
}