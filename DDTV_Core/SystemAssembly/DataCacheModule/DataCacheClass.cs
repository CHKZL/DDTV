using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.DataCacheModule
{
    public class DataCacheClass
    {
        internal static Dictionary<CacheType, Dictionary<string, Data>> Caches { get => caches; set => caches=value; }

        /// <summary>
        /// 内建缓存表
        /// </summary>
        private static Dictionary<DataCacheClass.CacheType, Dictionary<string, Data>> caches = new Dictionary<DataCacheClass.CacheType, Dictionary<string, Data>>(new Dictionary<DataCacheClass.CacheType, Dictionary<string, Data>> { })
            {
                { CacheType.room_id, new Dictionary<string, Data>() },
                { CacheType.area, new Dictionary<string, Data>() },
                { CacheType.area_name, new Dictionary<string, Data>() },
                { CacheType.area_v2_id, new Dictionary<string, Data>() },
                { CacheType.area_v2_name, new Dictionary<string, Data>() },
                { CacheType.area_v2_parent_id, new Dictionary<string, Data>() },
                { CacheType.area_v2_parent_name, new Dictionary<string, Data>() },
                { CacheType.broadcast_type, new Dictionary<string, Data>() },
                { CacheType.cover_from_user, new Dictionary<string, Data>() },
                { CacheType.face, new Dictionary<string, Data>() },
                { CacheType.hidden_till, new Dictionary<string, Data>() },
                { CacheType.keyframe, new Dictionary<string, Data>() },
                { CacheType.live_status, new Dictionary<string, Data>() },
                { CacheType.live_time, new Dictionary<string, Data>() },
                { CacheType.lock_till, new Dictionary<string, Data>() },
                { CacheType.online, new Dictionary<string, Data>() },
                { CacheType.short_id, new Dictionary<string, Data>() },
                { CacheType.tag_name, new Dictionary<string, Data>() },
                { CacheType.tags, new Dictionary<string, Data>() },
                { CacheType.title, new Dictionary<string, Data>() },
                { CacheType.uid, new Dictionary<string, Data>() },
                { CacheType.uname, new Dictionary<string, Data>() },
                { CacheType.Other, new Dictionary<string, Data>() },
            };


        /// <summary>
        /// 缓存表子对象
        /// </summary>
        internal class Data
        {
            /// <summary>
            /// 缓存表子对象的值
            /// </summary>
            internal string Value { set; get; }
            /// <summary>
            /// 缓存表子对象的有效期
            /// </summary>
            internal long ExTime { set; get; }
        }
        public enum CacheType
        {
            /// <summary>
            /// 直播间房间号(直播间实际房间号)
            /// </summary>
            room_id = 1000,
            /// <summary>
            /// 直播间分区id
            /// </summary>
            area = 1001,
            /// <summary>
            /// 直播间分区名
            /// </summary>
            area_name = 1002,
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            area_v2_id = 1003,
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            area_v2_name = 1004,
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            area_v2_parent_id = 1005,
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            area_v2_parent_name = 1006,
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            broadcast_type = 1007,
            /// <summary>
            /// 直播间封面url
            /// </summary>
            cover_from_user = 1008,
            /// <summary>
            /// 主播头像url
            /// </summary>
            face = 1009,
            /// <summary>
            /// 直播间隐藏信息
            /// </summary>
            hidden_till = 1010,
            /// <summary>
            /// 直播间关键帧url
            /// </summary>
            keyframe = 1011,
            /// <summary>
            /// 直播间开播状态(0：未开播，1：正在直播，2：轮播中)
            /// </summary>
            live_status = 1012,
            /// <summary>
            /// 直播持续时长
            /// </summary>
            live_time = 1013,
            /// <summary>
            /// 直播间封禁信息
            /// </summary>
            lock_till = 1014,
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            online = 1015,
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            short_id = 1016,
            /// <summary>
            /// 直播间标签
            /// </summary>
            tag_name = 1017,
            /// <summary>
            /// 直播间自定标签
            /// </summary>
            tags = 1018,
            /// <summary>
            /// 直播间标题
            /// </summary>
            title = 1019,
            /// <summary>
            /// 主播mid
            /// </summary>
            uid = 1020,
            /// <summary>
            /// 主播用户名
            /// </summary>
            uname = 1021,
            /// <summary>
            /// 是否P2P
            /// </summary>
            need_p2p=1022,
            /// <summary>
            /// 是否隐藏
            /// </summary>
            is_hidden=1023,
            /// <summary>
            /// 是否锁定
            /// </summary>
            is_locked=1024,
            /// <summary>
            /// 是否竖屏
            /// </summary>
            is_portrait=1025,
            /// <summary>
            /// 是否加密
            /// </summary>
            encrypted=1026,
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted为true时有效)
            /// </summary>
            pwd_verified = 1027,
            /// <summary>
            /// 暂时不知道是用来干啥的值
            /// </summary>
            room_shield=1028,
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            is_sp = 1029,
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            special_type = 1030,
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            roomStatus = 1031,
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            roundStatus = 1032,
            /// <summary>
            /// 直播间网页url
            /// </summary>
            url = 1033,
            /// <summary>
            /// 描述(Local值)
            /// </summary>
            Description = 1034,
            /// <summary>
            /// 是否自动录制(Local值)
            /// </summary>
            IsAutoRec = 1035,
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            IsRemind = 1036,
            /// <summary>
            /// 特殊标记(Local值)
            /// </summary>
            Like = 1037,
            /// <summary>
            /// 其他临时内容
            /// </summary>
            Other = int.MaxValue
        }
    }
}