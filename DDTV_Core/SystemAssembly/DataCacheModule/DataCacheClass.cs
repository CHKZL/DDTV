using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.DataCacheModule
{
    public class DataCacheClass
    {
        internal static Dictionary<CacheType, Dictionary<string, Data>> Caches { get => caches; set => caches = value; }

        /// <summary>
        /// 内建缓存表
        /// </summary>
        private static Dictionary<CacheType, Dictionary<string, Data>> caches = new Dictionary<CacheType, Dictionary<string, Data>>(new Dictionary<CacheType, Dictionary<string, Data>> { })
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
            { CacheType.need_p2p, new Dictionary<string, Data>() },
            { CacheType.is_hidden, new Dictionary<string, Data>() },
            { CacheType.is_locked, new Dictionary<string, Data>() },
            { CacheType.is_portrait, new Dictionary<string, Data>() },
            { CacheType.encrypted, new Dictionary<string, Data>() },
            { CacheType.pwd_verified, new Dictionary<string, Data>() },
            { CacheType.room_shield, new Dictionary<string, Data>() },
            { CacheType.is_sp, new Dictionary<string, Data>() },
            { CacheType.special_type, new Dictionary<string, Data>() },
            { CacheType.roomStatus, new Dictionary<string, Data>() },
            { CacheType.url, new Dictionary<string, Data>() },
            { CacheType.description, new Dictionary<string, Data>() },
            { CacheType.Description, new Dictionary<string, Data>() },
            { CacheType.IsAutoRec, new Dictionary<string, Data>() },
            { CacheType.IsRemind, new Dictionary<string, Data>() },
            { CacheType.IsRecDanmu, new Dictionary<string, Data>() },
            { CacheType.Like, new Dictionary<string, Data>() },
            { CacheType.attention, new Dictionary<string, Data>() },
            { CacheType.level, new Dictionary<string, Data>() },
            { CacheType.sex, new Dictionary<string, Data>() },
            { CacheType.sign, new Dictionary<string, Data>() },
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
            room_id,
            /// <summary>
            /// 直播间分区id
            /// </summary>
            area,
            /// <summary>
            /// 直播间分区名
            /// </summary>
            area_name,
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            area_v2_id,
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            area_v2_name,
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            area_v2_parent_id,
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            area_v2_parent_name,
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            broadcast_type,
            /// <summary>
            /// 直播间封面url
            /// </summary>
            cover_from_user,
            /// <summary>
            /// 主播头像url
            /// </summary>
            face,
            /// <summary>
            /// 直播间隐藏信息
            /// </summary>
            hidden_till,
            /// <summary>
            /// 直播间关键帧url
            /// </summary>
            keyframe,
            /// <summary>
            /// 直播间开播状态(0：未开播，1：正在直播，2：轮播中)
            /// </summary>
            live_status,
            /// <summary>
            /// 直播持续时长
            /// </summary>
            live_time,
            /// <summary>
            /// 直播间封禁信息
            /// </summary>
            lock_till,
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            online,
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            short_id,
            /// <summary>
            /// 直播间标签
            /// </summary>
            tag_name,
            /// <summary>
            /// 直播间自定标签
            /// </summary>
            tags,
            /// <summary>
            /// 直播间标题
            /// </summary>
            title,
            /// <summary>
            /// 主播mid
            /// </summary>
            uid,
            /// <summary>
            /// 主播用户名
            /// </summary>
            uname,
            /// <summary>
            /// 是否P2P
            /// </summary>
            need_p2p,
            /// <summary>
            /// 是否隐藏
            /// </summary>
            is_hidden,
            /// <summary>
            /// 是否锁定
            /// </summary>
            is_locked,
            /// <summary>
            /// 是否竖屏
            /// </summary>
            is_portrait,
            /// <summary>
            /// 是否加密
            /// </summary>
            encrypted,
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted为true时有效)
            /// </summary>
            pwd_verified,
            /// <summary>
            /// 暂时不知道是用来干啥的值
            /// </summary>
            room_shield,
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            is_sp,
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            special_type,
            /// <summary>
            /// 房间是否存在(0:无房间 1:有房间)
            /// </summary>
            roomStatus,
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            roundStatus,
            /// <summary>
            /// 直播间网页url
            /// </summary>
            url,
            /// <summary>
            /// 描述(Local值)
            /// </summary>
            Description,
            /// <summary>
            /// 是否自动录制(Local值)
            /// </summary>
            IsAutoRec,
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            IsRemind,
            /// <summary>
            /// 是否录制弹幕(Local值)
            /// </summary>
            IsRecDanmu,
            /// <summary>
            /// 特殊标记(Local值)
            /// </summary>
            Like,
            /// <summary>
            /// 房间关注人数
            /// </summary>
            attention,
            /// <summary>
            /// 主播简介(网络值)
            /// </summary>
            description,
            /// <summary>
            /// 账号等级
            /// </summary>
            level,
            /// <summary>
            /// 主播性别
            /// </summary>
            sex,
            /// <summary>
            /// 主播签名
            /// </summary>
            sign,
            /// <summary>
            /// 其他临时内容
            /// </summary>
            Other = int.MaxValue
        }
    }
}