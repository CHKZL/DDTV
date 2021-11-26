using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.DataCacheModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API
{
    internal class UserInfo
    {
        /// <summary>
        /// 获取账号信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        internal static RoomInfoClass.RoomInfo info(long uid)
        {
            JObject JO = (JObject)JsonConvert.DeserializeObject(NetworkRequestModule.Get.Get.GetRequest("https://api.bilibili.com/x/space/acc/info?mid="+uid));
            if (JO!=null&&JO.ContainsKey("code")&&JO["code"]!=null&&(int)JO["code"]==0)
            {
                if (JO.TryGetValue("data", out var RoomInit)&&RoomInit!=null)
                {
                    RoomInfoClass.RoomInfo room=new RoomInfoClass.RoomInfo();
                    User.UserClass.UserInfo userClass = new()
                    {
                        face=RoomInit["face"].ToString(),
                        level=int.Parse(RoomInit["level"].ToString()),
                        liveStatus=int.Parse(RoomInit["live_room"]["liveStatus"].ToString()),
                        name=RoomInit["name"].ToString(),
                        roomid=int.Parse(RoomInit["live_room"]["roomid"].ToString()),
                        roomStatus=int.Parse(RoomInit["live_room"]["roomStatus"].ToString()),
                        sex=RoomInit["sex"].ToString(),
                        sign=RoomInit["sign"].ToString(),
                        title=RoomInit["live_room"]["title"].ToString(),
                        uid=uid,
                        url=RoomInit["live_room"]["url"].ToString(),
                        roundStatus=int.Parse(RoomInit["live_room"]["roundStatus"].ToString()),

                    };
                   
                    if (Rooms.Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
                    {
                        Rooms.Rooms.RoomInfo[uid].room_id =userClass.roomid;
                        Rooms.Rooms.RoomInfo[uid].level =userClass.level;
                        Rooms.Rooms.RoomInfo[uid].live_status =userClass.liveStatus;
                        Rooms.Rooms.RoomInfo[uid].uname =userClass.name;
                        Rooms.Rooms.RoomInfo[uid].face =userClass.face;
                        Rooms.Rooms.RoomInfo[uid].roomStatus =userClass.roomStatus;
                        Rooms.Rooms.RoomInfo[uid].sex =userClass.sex;
                        Rooms.Rooms.RoomInfo[uid].sign =userClass.sign;
                        Rooms.Rooms.RoomInfo[uid].title =userClass.title;
                        Rooms.Rooms.RoomInfo[uid].url =userClass.url;
                        Rooms.Rooms.RoomInfo[uid].roundStatus =userClass.roundStatus;
                        room=Rooms.Rooms.RoomInfo[uid];
                    }
                    else
                    {
                        RoomInfoClass.RoomInfo roomInfo1 = new()
                        {
                            uid=uid,
                            room_id=userClass.roomid,
                            level=userClass.level,
                            live_status=userClass.liveStatus,
                            uname=userClass.name,
                            face=userClass.face,
                            roomStatus=userClass.roomStatus,
                            sex=userClass.sex,
                            sign=userClass.sign,
                            title=userClass.title,
                            url=userClass.url,
                            roundStatus=userClass.roundStatus
                        };
                        Rooms.Rooms.RoomInfo.Add(uid, roomInfo1);
                        room=roomInfo1;
                    }
                    DataCache.SetCache(CacheType.room_id, uid.ToString(), userClass.roomid.ToString(), int.MaxValue);
                    DataCache.SetCache(CacheType.level, uid.ToString(), userClass.level.ToString(), 3600*1000);
                    DataCache.SetCache(CacheType.live_status, uid.ToString(), userClass.liveStatus.ToString(), 0);
                    DataCache.SetCache(CacheType.uname, uid.ToString(), userClass.name, int.MaxValue);
                    DataCache.SetCache(CacheType.face, uid.ToString(), userClass.face, int.MaxValue);
                    DataCache.SetCache(CacheType.roomStatus, uid.ToString(), userClass.roomStatus.ToString(), 0);
                    DataCache.SetCache(CacheType.sex, uid.ToString(), userClass.sex, int.MaxValue);
                    DataCache.SetCache(CacheType.sign, uid.ToString(), userClass.sign, int.MaxValue);
                    DataCache.SetCache(CacheType.title, uid.ToString(), userClass.title, 0);
                    DataCache.SetCache(CacheType.url, uid.ToString(), userClass.url, 1*1000);
                    DataCache.SetCache(CacheType.roundStatus, uid.ToString(), userClass.roundStatus.ToString(), 0);
                    Log.Log.AddLog(nameof(RoomInfo), Log.LogClass.LogType.Debug, $"获取用户[{uid}]的直播房间get_info信息成功");
                    return room;
                }
            }
            return null;
        }
    }
}
