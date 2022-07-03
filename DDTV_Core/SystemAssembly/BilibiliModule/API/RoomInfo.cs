using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.DataCacheModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API
{
    public class RoomInfo
    {
        public static bool ForceCDNResolution = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.ForceCDNResolution, "False", CoreConfigClass.Group.Download));

        /// <summary>
        /// 使用uids获取房间状态信息
        /// </summary>
        /// <param name="UIDList">需要更新的UID列表</param>
        /// <param name="query">需要返回信息的UID</param>
        /// <returns></returns>
        internal static RoomInfoClass.RoomInfo get_status_info_by_uids(List<long> UIDList, long query = 0)
        {
            if (UIDList.Count > 0)
            {
                string LT = "";
                LT = "{\"uids\":[" + UIDList[0];
                if (UIDList.Count > 0)
                {
                    for (int i = 1 ; i < UIDList.Count ; i++)
                    {
                        if (UIDList[i] != 0)
                        {
                            LT += "," + UIDList[i];
                        }
                    }
                }
                LT += "]}";
                string WebText = NetworkRequestModule.Post.Post.SendRequest_GetWebInfo_JsonClass("https://api.live.bilibili.com/room/v1/Room/get_status_info_by_uids", LT, "UTF-8");

                if (string.IsNullOrEmpty(WebText))
                {
                    Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"get_status_info_by_uids获取网络数据为空或超时");
                    Thread.Sleep(800);
                    return get_status_info_by_uids(UIDList, query);
                }
                JObject JO = (JObject)JsonConvert.DeserializeObject(WebText);
                if (JO != null && JO.ContainsKey("code") && JO["code"] != null && (int)JO["code"] == 0)
                {
                    if (JO.TryGetValue("data", out var RoomList))
                    {
                        if (RoomList.Count() > 0)
                        {
                            IList<JToken> obj = JObject.Parse(RoomList.ToString());
                            for (int i = 0 ; i < RoomList.Count() ; i++)
                            {
                                long uid = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomList[((JProperty)obj[i]).Name].ToString()).uid;
                                if (Rooms.Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
                                {
                                    if (((JProperty)obj[i]).Name != null && RoomList[((JProperty)obj[i]).Name] != null)
                                    {
                                        string name = RoomList[((JProperty)obj[i]).Name].ToString();
                                        Rooms.Rooms.RoomInfo[uid].area = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).area;
                                        Rooms.Rooms.RoomInfo[uid].area_name = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).area_name;
                                        Rooms.Rooms.RoomInfo[uid].area_v2_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).area_v2_id;
                                        Rooms.Rooms.RoomInfo[uid].area_v2_name = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).area_v2_name;
                                        Rooms.Rooms.RoomInfo[uid].area_v2_parent_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).area_v2_parent_id;
                                        Rooms.Rooms.RoomInfo[uid].area_v2_parent_name = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).area_v2_parent_name;
                                        Rooms.Rooms.RoomInfo[uid].broadcast_type = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).broadcast_type;
                                        Rooms.Rooms.RoomInfo[uid].cover_from_user = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).cover_from_user;
                                        Rooms.Rooms.RoomInfo[uid].face = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).face;
                                        Rooms.Rooms.RoomInfo[uid].hidden_till = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).hidden_till;
                                        Rooms.Rooms.RoomInfo[uid].keyframe = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).keyframe;
                                        Rooms.Rooms.RoomInfo[uid].live_status = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).live_status;
                                        Rooms.Rooms.RoomInfo[uid].live_time = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).live_time;
                                        Rooms.Rooms.RoomInfo[uid].lock_till = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).lock_till;
                                        Rooms.Rooms.RoomInfo[uid].online = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).online;
                                        Rooms.Rooms.RoomInfo[uid].room_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).room_id;
                                        Rooms.Rooms.RoomInfo[uid].short_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).short_id;
                                        Rooms.Rooms.RoomInfo[uid].tag_name = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).tag_name;
                                        Rooms.Rooms.RoomInfo[uid].title = Tool.FileOperation.CheckFilenames(JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).title);
                                        Rooms.Rooms.RoomInfo[uid].uname = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(name).uname;
                                    }
                                }
                                else
                                {
                                    Rooms.Rooms.RoomInfo.Add(uid, JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomList[((JProperty)obj[i]).Name].ToString()));
                                }
                                //开始更新该API对应的缓存信息
                                DataCache.SetCache(CacheType.area, uid.ToString(), Rooms.Rooms.RoomInfo[uid].area.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.area_name, uid.ToString(), Rooms.Rooms.RoomInfo[uid].area_name.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.area_v2_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].area_v2_id.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.area_v2_name, uid.ToString(), Rooms.Rooms.RoomInfo[uid].area_v2_name.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.area_v2_parent_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].area_v2_parent_id.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.area_v2_parent_name, uid.ToString(), Rooms.Rooms.RoomInfo[uid].area_v2_parent_name.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.broadcast_type, uid.ToString(), Rooms.Rooms.RoomInfo[uid].broadcast_type.ToString(), 60 * 1000);
                                DataCache.SetCache(CacheType.cover_from_user, uid.ToString(), Rooms.Rooms.RoomInfo[uid].cover_from_user.ToString(), 60 * 1000);
                                DataCache.SetCache(CacheType.face, uid.ToString(), Rooms.Rooms.RoomInfo[uid].face.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.hidden_till, uid.ToString(), Rooms.Rooms.RoomInfo[uid].hidden_till.ToString(), 60 * 1000);
                                DataCache.SetCache(CacheType.keyframe, uid.ToString(), Rooms.Rooms.RoomInfo[uid].keyframe.ToString(), 60 * 1000);
                                DataCache.SetCache(CacheType.live_status, uid.ToString(), Rooms.Rooms.RoomInfo[uid].live_status.ToString(), 0);
                                DataCache.SetCache(CacheType.live_time, uid.ToString(), Rooms.Rooms.RoomInfo[uid].live_time.ToString(), 0);
                                DataCache.SetCache(CacheType.lock_till, uid.ToString(), Rooms.Rooms.RoomInfo[uid].lock_till.ToString(), 60 * 1000);
                                DataCache.SetCache(CacheType.online, uid.ToString(), Rooms.Rooms.RoomInfo[uid].online.ToString(), 0);
                                DataCache.SetCache(CacheType.room_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].room_id.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.short_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].short_id.ToString(), int.MaxValue);
                                DataCache.SetCache(CacheType.tags, uid.ToString(), Rooms.Rooms.RoomInfo[uid].tags.ToString(), 60 * 1000);
                                DataCache.SetCache(CacheType.tag_name, uid.ToString(), Rooms.Rooms.RoomInfo[uid].tag_name.ToString(), 60 * 1000);
                                DataCache.SetCache(CacheType.title, uid.ToString(), Tool.FileOperation.CheckFilenames(Rooms.Rooms.RoomInfo[uid].title.ToString()), 0);
                                DataCache.SetCache(CacheType.uname, uid.ToString(), Rooms.Rooms.RoomInfo[uid].uname.ToString(), int.MaxValue);
                                if (UIDList.Count() == 1)
                                {
                                    //Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"调用更新房间状态API回调成功，已更新房间信息");
                                    return Rooms.Rooms.RoomInfo[uid];
                                }
                            }
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"更新房间开播状态失败，稍后重试（偶尔提示可以无视本消息）");
                            return null;
                        }
                    }
                }
            }
            //Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"收到的UIDList长度为0，调用更新房间状态API回调失败");
            return null;
        }
        /// <summary>
        /// 获取房间初始化信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        internal static RoomInfoClass.RoomInfo room_init(long uid)
        {
            string roomid = Rooms.Rooms.GetValue(uid, CacheType.room_id);
            string WebText = NetworkRequestModule.Get.Get.GetRequest("https://api.live.bilibili.com/room/v1/Room/room_init?id=" + roomid);
            
            if (string.IsNullOrEmpty(WebText))
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"room_init获取网络数据为空或超时");
                Thread.Sleep(800);
                return room_init(uid);
            }
            JObject JO = (JObject)JsonConvert.DeserializeObject(WebText);
            if (JO != null && JO.ContainsKey("code") && JO["code"] != null && (int)JO["code"] == 0)
            {
                if (JO.TryGetValue("data", out var RoomInit) && RoomInit != null)
                {
                    if (Rooms.Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
                    {
                        Rooms.Rooms.RoomInfo[uid].room_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).room_id;
                        Rooms.Rooms.RoomInfo[uid].short_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).short_id;
                        Rooms.Rooms.RoomInfo[uid].uid = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).uid;
                        Rooms.Rooms.RoomInfo[uid].need_p2p = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).need_p2p;
                        Rooms.Rooms.RoomInfo[uid].is_hidden = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).is_hidden;
                        Rooms.Rooms.RoomInfo[uid].is_locked = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).is_locked;
                        Rooms.Rooms.RoomInfo[uid].is_portrait = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).is_portrait;
                        Rooms.Rooms.RoomInfo[uid].live_status = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).live_status;
                        Rooms.Rooms.RoomInfo[uid].hidden_till = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).hidden_till;
                        Rooms.Rooms.RoomInfo[uid].lock_till = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).lock_till;
                        Rooms.Rooms.RoomInfo[uid].encrypted = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).encrypted;
                        Rooms.Rooms.RoomInfo[uid].pwd_verified = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).pwd_verified;
                        Rooms.Rooms.RoomInfo[uid].live_time = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).live_time;
                        Rooms.Rooms.RoomInfo[uid].room_shield = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).room_shield;
                        Rooms.Rooms.RoomInfo[uid].is_sp = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).is_sp;
                        Rooms.Rooms.RoomInfo[uid].special_type = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString()).special_type;
                        Rooms.Rooms.RoomInfo[uid].roomStatus = 1;
                    }
                    else
                    {
                        RoomInfoClass.RoomInfo info = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(RoomInit.ToString());
                        info.roomStatus = 1;
                        Rooms.Rooms.RoomInfo.Add(uid, info);
                    }
                    DataCache.SetCache(CacheType.room_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].room_id.ToString(), int.MaxValue);
                    DataCache.SetCache(CacheType.short_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].short_id.ToString(), int.MaxValue);
                    DataCache.SetCache(CacheType.uid, uid.ToString(), Rooms.Rooms.RoomInfo[uid].uid.ToString(), int.MaxValue);
                    DataCache.SetCache(CacheType.need_p2p, uid.ToString(), Rooms.Rooms.RoomInfo[uid].need_p2p.ToString(), 60 * 1000);
                    DataCache.SetCache(CacheType.is_hidden, uid.ToString(), Rooms.Rooms.RoomInfo[uid].is_hidden.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.is_locked, uid.ToString(), Rooms.Rooms.RoomInfo[uid].is_locked.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.is_portrait, uid.ToString(), Rooms.Rooms.RoomInfo[uid].is_portrait.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.live_status, uid.ToString(), Rooms.Rooms.RoomInfo[uid].live_status.ToString(), int.MaxValue);
                    DataCache.SetCache(CacheType.hidden_till, uid.ToString(), Rooms.Rooms.RoomInfo[uid].hidden_till.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.lock_till, uid.ToString(), Rooms.Rooms.RoomInfo[uid].lock_till.ToString(), 60 * 1000);
                    DataCache.SetCache(CacheType.encrypted, uid.ToString(), Rooms.Rooms.RoomInfo[uid].encrypted.ToString(), 60 * 1000);
                    DataCache.SetCache(CacheType.pwd_verified, uid.ToString(), Rooms.Rooms.RoomInfo[uid].pwd_verified.ToString(), 60 * 1000);
                    DataCache.SetCache(CacheType.live_time, uid.ToString(), Rooms.Rooms.RoomInfo[uid].live_time.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.room_shield, uid.ToString(), Rooms.Rooms.RoomInfo[uid].room_shield.ToString(), int.MaxValue);
                    DataCache.SetCache(CacheType.is_sp, uid.ToString(), Rooms.Rooms.RoomInfo[uid].is_sp.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.special_type, uid.ToString(), Rooms.Rooms.RoomInfo[uid].special_type.ToString(), 300 * 1000);
                    DataCache.SetCache(CacheType.roomStatus, uid.ToString(), "1", 300 * 1000);

                    Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"获取用户[{uid}]的直播房间初始化信息成功");
                    return Rooms.Rooms.RoomInfo[uid];
                }
            }
            else
            {
                if (JO != null && JO.ContainsKey("code") && JO["code"] != null && (int)JO["code"] == 60004)
                {
                    if (Rooms.Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
                    {
                        Rooms.Rooms.RoomInfo[uid].roomStatus = 0;

                    }
                    else
                    {
                        Rooms.Rooms.RoomInfo.Add(uid, new RoomInfoClass.RoomInfo() { uid = uid, roomStatus = 0 });
                    }
                    DataCache.SetCache(CacheType.room_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].room_id.ToString(), int.MaxValue);

                    Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"获取用户[{uid}]的直播房间初始化信息成功");
                    return Rooms.Rooms.RoomInfo[uid];
                }
            }
            return null;
        }
        /// <summary>
        /// 获取直播间信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static RoomInfoClass.RoomInfo get_info(long uid,long RoomId=0,bool IsAddedToForm=true)
        {
            string WebText=String.Empty;
            if (RoomId==0)
            {
                WebText = NetworkRequestModule.Get.Get.GetRequest($"https://api.live.bilibili.com/room/v1/Room/get_info?id={Rooms.Rooms.GetValue(uid, CacheType.room_id)}");
            }
            else
            {
                WebText = NetworkRequestModule.Get.Get.GetRequest($"https://api.live.bilibili.com/room/v1/Room/get_info?id={RoomId}");
            }
           
            if (string.IsNullOrEmpty(WebText))
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"get_info获取网络数据为空或超时");
                Thread.Sleep(800);
                return get_info(uid);
            }
            JObject JO = (JObject)JsonConvert.DeserializeObject(WebText);
            if (JO != null && JO.ContainsKey("code") && JO["code"] != null && (int)JO["code"] == 0)
            {
                if (JO.TryGetValue("data", out var RoomInit) && RoomInit != null)
                {

                    string ri = RoomInit.ToString();
                    ri = ri.Replace("\"live_time\"", "\"live_time_t\"").Replace("0000-00-00 00:00:00", "1970-01-01 08:00:01");
                    if (IsAddedToForm)
                    {
                        if (Rooms.Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
                        {
                            Rooms.Rooms.RoomInfo[uid].room_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).room_id;
                            Rooms.Rooms.RoomInfo[uid].short_id = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).short_id;
                            Rooms.Rooms.RoomInfo[uid].attention = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).attention;
                            Rooms.Rooms.RoomInfo[uid].online = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).online;
                            Rooms.Rooms.RoomInfo[uid].description = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).description;
                            Rooms.Rooms.RoomInfo[uid].live_status = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).live_status;
                            Rooms.Rooms.RoomInfo[uid].title = Tool.FileOperation.CheckFilenames(JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).title);
                            Rooms.Rooms.RoomInfo[uid].cover_from_user = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).user_cover;
                            Rooms.Rooms.RoomInfo[uid].keyframe = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).keyframe;
                            Rooms.Rooms.RoomInfo[uid].area_name = JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri).area_name;
                        }
                        else
                        {
                            Rooms.Rooms.RoomInfo.Add(uid, JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri));
                        }
                        DataCache.SetCache(CacheType.room_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].room_id.ToString(), int.MaxValue);
                        DataCache.SetCache(CacheType.short_id, uid.ToString(), Rooms.Rooms.RoomInfo[uid].short_id.ToString(), int.MaxValue);
                        DataCache.SetCache(CacheType.attention, uid.ToString(), Rooms.Rooms.RoomInfo[uid].attention.ToString(), 60 * 1000);
                        DataCache.SetCache(CacheType.online, uid.ToString(), Rooms.Rooms.RoomInfo[uid].online.ToString(), 5 * 1000);
                        DataCache.SetCache(CacheType.description, uid.ToString(), Rooms.Rooms.RoomInfo[uid].description.ToString(), 300 * 1000);
                        DataCache.SetCache(CacheType.live_status, uid.ToString(), Rooms.Rooms.RoomInfo[uid].live_status.ToString(), 0);
                        DataCache.SetCache(CacheType.title, uid.ToString(), Tool.FileOperation.CheckFilenames(Rooms.Rooms.RoomInfo[uid].title.ToString()), 0);
                        DataCache.SetCache(CacheType.cover_from_user, uid.ToString(), Rooms.Rooms.RoomInfo[uid].user_cover.ToString(), 60 * 1000);
                        DataCache.SetCache(CacheType.keyframe, uid.ToString(), Rooms.Rooms.RoomInfo[uid].keyframe.ToString(), 60 * 1000);
                        DataCache.SetCache(CacheType.area_name, uid.ToString(), Rooms.Rooms.RoomInfo[uid].area_name.ToString(), 60 * 1000);
                        return Rooms.Rooms.RoomInfo[uid];
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<RoomInfoClass.RoomInfo>(ri);
                    }
                    //Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"获取用户[{uid}]的直播房间get_info信息成功");
                    
                }
            }
            return null;
        }
        public static List<int> GetQuality(long uid)
        {
            string roomId = Rooms.Rooms.GetValue(uid, CacheType.room_id);
            List<int> _out=new List<int>();
            string WebText = NetworkRequestModule.Get.Get.GetRequest("https://api.live.bilibili.com/room/v1/Room/playUrl?cid=" + roomId + $"&qn=10000&platform=web");
            if (string.IsNullOrEmpty(WebText))
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"GetQuality获取网络数据为空或超时，开始重试");
                Thread.Sleep(800);
                return GetQuality(uid);
            }
            JObject JO = new JObject();
            try
            {
                JO = (JObject)JsonConvert.DeserializeObject(WebText);
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Error, $"GetQuality获取的数据解析失败，出现未知的错误", true, e);
                return GetQuality(uid);
            }
            try
            {
                if (JO != null && JO.ContainsKey("code") && JO["code"] != null)
                {
                    if ((int)JO["code"] == 0)
                    {
                        if (JO.TryGetValue("data", out var Roomdurl) && Roomdurl != null)
                        {
                            foreach (var item in Roomdurl["quality_description"])
                            {
                                int.TryParse(item["qn"].ToString(), out int c);
                                _out.Add(c);
                            }
                        }
                    }
                    else if ((int)JO["code"] == -400)
                    {
                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"GetQuality:因为参数错误，{roomId}房间直播视频流获取失败");
                    }
                    else if ((int)JO["code"] == 19002003)
                    {
                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"GetQuality:因为{roomId}房间信息不存在，获取直播视频流失败");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"GetQuality获取视频流地址失败，失败的json串:\n{JO.ToString()}", true, e);
            }
            return _out;
        }

        /// <summary>
        /// 获取房间直播视频流地址
        /// </summary>
        /// <param name="uid">用户mid</param>
        /// <param name="qn">画质</param>
        /// <returns></returns>
        public static string playUrl_Mandatory(long uid, RoomInfoClass.Quality qn, RoomInfoClass.Line line = RoomInfoClass.Line.PrincipalLine,bool IsPlay=false)
        {
            List<RoomInfoClass.Quality> NotQU = new List<RoomInfoClass.Quality>();
        PlayR: if (!GetQuality(uid).Contains((int)qn))
            {
                qn = RoomInfoClass.Quality.OriginalPainting;
            }
            string roomId = Rooms.Rooms.GetValue(uid, CacheType.room_id);
            string WebText = NetworkRequestModule.Get.Get.GetRequest($"https://api.live.bilibili.com/xlive/web-room/v2/index/getRoomPlayInfo?room_id={roomId}&protocol=0,1&format=0,1,2&codec=0,1&qn={(int)qn}&platform=web&ptype=8");

            if (string.IsNullOrEmpty(WebText))
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"playUrl获取网络数据为空或超时，开始重试");
                Thread.Sleep(800);
                return playUrl_Mandatory(uid, qn);
            }
            try
            {
                var BA = JsonConvert.DeserializeObject<ApiClass.BilibiliApiResponse<ApiClass.RoomPlayInfo>>(WebText);
                switch (BA.Code)
                {
                    case 0:
                        if (BA?.Data.LiveStatus == 1)
                        {
                            var url_data = BA?.Data?.PlayurlInfo?.Playurl?.Streams;
                            var url_http_stream_flv_avc =
                                url_data.FirstOrDefault(x => x.ProtocolName == "http_stream")?.Formats?.FirstOrDefault(x => x.FormatName == "flv")?.Codecs?.FirstOrDefault(x => x.CodecName == "avc");
                            foreach (var item in url_http_stream_flv_avc.UrlInfos)
                            {
                                if (!IsPlay && ForceCDNResolution)
                                {
                                    if (item.Host.Contains("d1--") || item.Host.Contains("c1--"))
                                    {
                                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Info, $"策略2(主):获取到CDN地址为{item.Host}的下载流");
                                        return item.Host + url_http_stream_flv_avc.BaseUrl + item.Extra;
                                    }
                                }
                                else
                                {
                                    if (!item.Host.Contains(".mcdn."))
                                    {
                                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Info, $"策略1(从):获取到CDN地址为{item.Host}的下载流");
                                        return item.Host + url_http_stream_flv_avc.BaseUrl + item.Extra;
                                    }
                                    else
                                    {
                                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Info, $"策略1(P2P):获取到CDN地址为{item.Host}的下载流，该地址为mcdn地址，跳过");
                                    }
                                }
                            }
                            Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"未获取到主CDN地址，2秒后重试(如果一直失败，那还是把强制主CDN设置给关了吧)");
#if DEBUG
                            string uu = "";
                            foreach (var item in url_http_stream_flv_avc.UrlInfos)
                            {
                                uu += item.Host + "\n";
                            }
                            Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"{uu}");
#endif
                            Thread.Sleep(3000);
                            return playUrl_Mandatory(uid, qn);
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"获取直播流地时发现直播间已经不再开播状态，放弃获取");
                            return null;
                        }
                        
                    case 1002002:
                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"因为参数错误，{roomId}房间直播视频流获取失败");
                        break;
                    case 60004:
                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"因为{roomId}房间信息不存在，获取直播视频流失败");
                        break;
                    default:
                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Error, $"playUrl获取的数据解析失败，出现未知的错误1，出现错误的字符串:{WebText}", true, null, false);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Error, $"playUrl获取的数据解析失败，出现未知的错误2，出现错误的字符串:{WebText}", true, e,false);
                return playUrl_Mandatory(uid, qn);
            }  
            return null;
        }


        /// <summary>
        /// 获取房间直播视频流地址
        /// </summary>
        /// <param name="uid">用户mid</param>
        /// <param name="qn">画质</param>
        /// <returns></returns>
        public static string playUrl(long uid, RoomInfoClass.Quality qn, RoomInfoClass.Line line = RoomInfoClass.Line.PrincipalLine)
        {
            List<RoomInfoClass.Quality> NotQU = new List<RoomInfoClass.Quality>();
            PlayR: if (!GetQuality(uid).Contains((int)qn))
            {
                qn = RoomInfoClass.Quality.OriginalPainting;
            }
            string roomId = Rooms.Rooms.GetValue(uid, CacheType.room_id);
            
            string WebText = NetworkRequestModule.Get.Get.GetRequest("https://api.live.bilibili.com/room/v1/Room/playUrl?cid=" + roomId + $"&qn={(int)qn}&platform=web");

            if (string.IsNullOrEmpty(WebText))
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"playUrl获取网络数据为空或超时，开始重试");
                Thread.Sleep(800);
                return playUrl(uid, qn);          
            }

            JObject JO =new JObject();
            try
            {
                JO = (JObject)JsonConvert.DeserializeObject(WebText);
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Error, $"playUrl获取的数据解析失败，出现未知的错误", true, e);
                return playUrl(uid, qn);
            }

            try
            {
                if (JO != null && JO.ContainsKey("code") && JO["code"] != null)
                {
                    if ((int)JO["code"] == 0)
                    {
                        if (JO.TryGetValue("data", out var Roomdurl) && Roomdurl != null)
                        {
                            if((int)Roomdurl["current_quality"]==0)
                            {
                                NotQU.Add(qn);
                                if(!NotQU.Contains(RoomInfoClass.Quality.OriginalPainting))
                                {
                                    qn = RoomInfoClass.Quality.OriginalPainting;
                                }
                                else if (!NotQU.Contains(RoomInfoClass.Quality.BluRay))
                                {
                                    qn = RoomInfoClass.Quality.BluRay;
                                }
                                else if (!NotQU.Contains(RoomInfoClass.Quality.HighDefinition))
                                {
                                    qn = RoomInfoClass.Quality.HighDefinition;
                                }
                                else if (!NotQU.Contains(RoomInfoClass.Quality.Fluency))
                                {
                                    qn = RoomInfoClass.Quality.Fluency;
                                }
                                else
                                {
                                    Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Error, $"获取用户[{uid}]的直播房间清晰度时所有清晰度均无数据源！这是一个来自BILIBILI的内部错误，DDTV无法解决",true,null,true);
                                }
                                goto PlayR;
                            }
                            string Url = Roomdurl["durl"][(int)line]["url"].ToString();
                            Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"获取用户[{uid}]的直播房间清晰度为[{qn}]的视频流地址成功");
                            return Url;

                        }
                    }
                    else if ((int)JO["code"] == -400)
                    {
                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"因为参数错误，{roomId}房间直播视频流获取失败");
                    }
                    else if ((int)JO["code"] == 19002003)
                    {
                        Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"因为{roomId}房间信息不存在，获取直播视频流失败");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Warn, $"获取视频流地址失败，失败的json串:\n{JO.ToString()}", true, e);
            }
            return null;
        }

    }
}