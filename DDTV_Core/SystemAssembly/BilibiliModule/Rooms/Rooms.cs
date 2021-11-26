using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DDTV_Core.SystemAssembly.ConfigModule;

namespace DDTV_Core.SystemAssembly.BilibiliModule.Rooms
{
    public class Rooms
    {
        public static Dictionary<long, RoomInfoClass.RoomInfo> RoomInfo = new();
        /// <summary>
        /// 通过API更新本地房间信息
        /// </summary>
        public static void UpdateRoomInfo()
        {
            List<long> mids = new List<long>();
            foreach (var item in Rooms.RoomInfo)
            {
                mids.Add(item.Value.uid);
            }
            API.RoomInfo.get_status_info_by_uids(mids);
        }

        /// <summary>
        /// 获取房间数据
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public static string GetValue(long uid, DataCacheModule.DataCacheClass.CacheType cacheType)
        {
            if (DataCacheModule.DataCache.GetCache(cacheType, uid.ToString(), out string Value))
            {
                return Value;
            }
            else
            {
                var roominfo = SelectAPI(uid, cacheType);
                if (roominfo!=null)
                {
                    string value = roominfo.GetType().GetProperty(Enum.GetName(typeof(DataCacheModule.DataCacheClass.CacheType), cacheType)).GetValue(roominfo, null).ToString();
                    Log.Log.AddLog(nameof(Rooms), Log.LogClass.LogType.TmpInfo, $"获取用户[{uid}]直播房间的[{cacheType}]信息成功:{value}");
                    return value;
                }
                else
                {
                    Log.Log.AddLog(nameof(Rooms), Log.LogClass.LogType.Error, "获取信息失败");
                    return null;
                }
            }
        }
        /// <summary>
        /// 查找对应每个值的缓存数据来源API(房间组件内部方法)
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        private static RoomInfoClass.RoomInfo SelectAPI(long uid, DataCacheModule.DataCacheClass.CacheType cacheType)
        {
            switch (cacheType)
            {
                case DataCacheModule.DataCacheClass.CacheType.area:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.area_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.area_v2_id:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.area_v2_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.area_v2_parent_id:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.area_v2_parent_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.broadcast_type:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.cover_from_user:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.face:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.hidden_till:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.keyframe:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.live_status:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.live_time:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.lock_till:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.room_id:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.short_id:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.tag_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.tags:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.uid:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid });
                case DataCacheModule.DataCacheClass.CacheType.uname:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.need_p2p:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.is_hidden:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.is_locked:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.is_portrait:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.encrypted:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.pwd_verified:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.room_shield:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.is_sp:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.special_type:
                    return API.RoomInfo.room_init(uid);
                case DataCacheModule.DataCacheClass.CacheType.roomStatus:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.attention:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.description:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.online:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.title:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.level:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.sex:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.url:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.sign:
                    return API.UserInfo.info(uid);
                default:
                    return null;
            }
        }
    }
}
