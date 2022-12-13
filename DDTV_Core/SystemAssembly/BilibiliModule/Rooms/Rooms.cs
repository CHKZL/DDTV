using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DDTV_Core.SystemAssembly.ConfigModule;
using System.Threading;
using ConsoleTables;
using ConsoleTableExt;

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
            try
            {
                int P = 1;
                int APageCpunt = RoomInfo.Count();
                int OKConut = 0;
                while (RoomInfo.Count() / P > 1500)
                {
                    P++;
                    APageCpunt = RoomInfo.Count() / P;
                }
                List<long> mids = new List<long>();
                foreach (var item in RoomInfo)
                {
                    mids.Add(item.Value.uid);
                    OKConut++;
                    if (OKConut >= APageCpunt)
                    {
                        API.RoomInfo.get_status_info_by_uids(mids);
                        mids = new List<long>();
                        OKConut = 0;
                        if (P != 1)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Rooms), Log.LogClass.LogType.Warn, $"获取房间状态更新时发生错误，错误信息已写入日志文本中，稍后将自动重试，如果重复出现该错误，请检查网络状态", true, e, true);
            }
        }

        /// <summary>
        /// 获取房间控制台简报信息
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="IsLive"></param>
        /// <returns></returns>
        public static void GetRoomInfoBrief(long UID, bool IsLive)
        {
            try
            {

                Thread.Sleep(50);
                if (RoomInfo.TryGetValue(UID, out RoomInfoClass.RoomInfo roomInfo))
                {
                    if (roomInfo.attention < 1)
                    {
                        int.TryParse(GetValue(UID, DataCacheModule.DataCacheClass.CacheType.attention), out int att);
                        roomInfo.attention = att;
                    }
                 
                    TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
                    TimeSpan ts2 = new TimeSpan(roomInfo.MonitoringSystem_Airtime.Ticks);
                    TimeSpan timeLoop = ts1.Subtract(ts2).Duration();
                    int ChangeFansnum = roomInfo.attention - roomInfo.MonitoringSystem_Attention;
                    var tableData = new List<List<object>>
                    {
                        new List<object> {"昵称",
                        "标题",
                        "房间号",
                        "UID",
                        "分区",
                        "粉丝数",
                        "类型",
                        "加密",
                        "隐藏",
                        "付费",
                        "mcdn",
                        "标记",
                        "时长",
                        "本次直播粉丝数" },
                        new List<object> {
                    roomInfo.uname,
                        roomInfo.title,
                        roomInfo.room_id,
                        roomInfo.uid,
                        roomInfo.area_v2_name,
                        roomInfo.attention,
                        roomInfo.broadcast_type == 0 ? "普通直播" : "手机直播",
                        roomInfo.encrypted ? "是" : "否",
                        roomInfo.is_hidden ? "是" : "否",
                        roomInfo.is_sp == 1 ? "是" : "否",
                        roomInfo.need_p2p == 1 ? "已启用" : "未启用",
                        roomInfo.special_type == 0 ? "普通直播间" : roomInfo.special_type == 1 ? "付费直播间" : "拜年祭直播间",
                        IsLive?"才开播": timeLoop.ToString(@"hh\:mm\:ss"),
                       IsLive?"开播不统计本数据": ChangeFansnum < 0 ? $"-{ChangeFansnum}" : $"+{ChangeFansnum}"
                    }
                    };
                    if(IsLive)
                    {
                        ConsoleTableBuilder.From(tableData).WithTitle("∧开播信息", ConsoleColor.White, ConsoleColor.DarkRed).ExportAndWriteLine();
                    }
                    else
                    {
                        ConsoleTableBuilder.From(tableData).WithTitle("∨下播信息", ConsoleColor.White, ConsoleColor.DarkGray).ExportAndWriteLine();
                        //
                    }
                }
            }
            catch (Exception)
            {
            }

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
                switch (cacheType)
                {
                    case DataCacheModule.DataCacheClass.CacheType.uname:
                        if (!string.IsNullOrEmpty(Value))
                        {
                            return Value;
                        }
                        break;
                    case DataCacheModule.DataCacheClass.CacheType.attention:
                        if (int.TryParse(Value, out int att))
                        {
                            if (att != 0)
                            {
                                return Value;
                            }
                        }
                        break;
                    default:
                        return Value;
                }

            }
            var roominfo = SelectAPI(uid, cacheType);
            if (roominfo != null)
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
        /// <summary>
        /// 查找对应每个值的缓存数据来源API(房间组件内部方法)
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public static RoomInfoClass.RoomInfo SelectAPI(long uid, DataCacheModule.DataCacheClass.CacheType cacheType)
        {
            NetworkRequestModule.NetClass.SelectAPICountAdd(cacheType);

            switch (cacheType)
            {
                case DataCacheModule.DataCacheClass.CacheType.area:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.area_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.area_v2_id:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.area_v2_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.area_v2_parent_id:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.area_v2_parent_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.broadcast_type:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.cover_from_user:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.face:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.hidden_till:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.keyframe:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.live_status:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.live_time:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.lock_till:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.room_id:
                    return API.UserInfo.info(uid);
                case DataCacheModule.DataCacheClass.CacheType.short_id:
                    return API.RoomInfo.get_info(uid);
                case DataCacheModule.DataCacheClass.CacheType.tag_name:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.tags:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
                case DataCacheModule.DataCacheClass.CacheType.uid:
                    return API.RoomInfo.get_status_info_by_uids(new List<long>() { uid })[0];
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
