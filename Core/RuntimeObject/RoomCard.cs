using Core.Network.Methods;
using static Core.Network.Methods.Room;
using static Core.Network.Methods.User;

namespace Core.RuntimeObject
{
    public class RoomList
    {
        #region private Properties
        private static List<RoomCard> roomInfos = new List<RoomCard>();
        #endregion

        #region Public Method

        public static string GetNickname(long RoomId,long Uid)
        {
            return _GetNickname(RoomId, Uid);
        }

        public static long GetUid(long RoomId)
        {
            return _GetUid(RoomId);
        }

        public static long GetRoomId(long Uid)
        {
            return _GetRoomId(Uid);
        }

        public static string GetTitle(long Uid)
        {
            return _GetTitle(Uid);
        }



        #endregion

        #region private Method

        private static string _GetNickname(long RoomId, long Uid)
        {
            RoomCard? roomCard = RoomId != 0 ? roomInfos.FirstOrDefault(x => x.RoomId == RoomId) : roomInfos.FirstOrDefault(x => x.UID == Uid);
            if (roomCard == null || roomCard.RoomId < 0)
            {
                RoomCard card = ToRoomCard(GetUserInfo(Uid));
                if (card == null)
                    return "获取昵称失败";
                int index = RoomId != 0 ? roomInfos.FindIndex(x => x.RoomId == RoomId) : roomInfos.FindIndex(x => x.UID == Uid);
                if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[index] = card;
                return card.name;
            }
            else
                return roomCard.name;
        }

        private static long _GetUid(long RoomId)
        {
            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.RoomId == RoomId);
            if (roomCard == null || roomCard.UID < 0)
            {
                RoomCard card = ToRoomCard(GetRoomInfo(RoomId));
                if (card == null)
                    return -1;
                else if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[roomInfos.FindIndex(x => x.RoomId == RoomId)] = card;
                return roomCard.UID;
            }
            else
                return roomCard.UID;
        }

        private static long _GetRoomId(long Uid)
        {
            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.UID == Uid);
            if (roomCard == null || roomCard.RoomId < 0)
            {
                RoomCard card = ToRoomCard(GetUserInfo(Uid));
                if (card == null)
                    return -1;
                else if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[roomInfos.FindIndex(x => x.UID == Uid)] = card;
                return card.RoomId;
            }
            else
                return roomCard.RoomId;
        }

        private static string _GetTitle(long Uid)
        {
            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.UID == Uid);
            if (roomCard == null || string.IsNullOrEmpty(roomCard.title.Value) || roomCard.title.ExpirationTime < DateTime.Now)
            {
                RoomCard card = ToRoomCard(GetUserInfo(Uid));
                if (card == null)
                    return "";
                else if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[roomInfos.FindIndex(x => x.UID == Uid)] = card;
                return card.title.Value;
            }
            else
                return roomCard.title.Value;
        }

        private static RoomCard ToRoomCard(RoomInfo roomInfo)
        {
            if (roomInfo != null)
            {
                RoomCard card = new RoomCard()
                {
                    RoomId = roomInfo.data.room_id,
                    short_id = new() { Value = roomInfo.data.short_id, ExpirationTime = DateTime.MaxValue },
                    UID = roomInfo.data.uid,
                    need_p2p = new() { Value = roomInfo.data.need_p2p, ExpirationTime = DateTime.Now.AddMinutes(1) },
                    is_hidden = new() { Value = roomInfo.data.is_hidden, ExpirationTime = DateTime.Now.AddMinutes(1) },
                    is_locked = new() { Value = roomInfo.data.is_locked, ExpirationTime = DateTime.Now.AddMinutes(1) },
                    is_portrait = new() { Value = roomInfo.data.is_portrait, ExpirationTime = DateTime.Now.AddMinutes(1) },
                    live_status = new() { Value = roomInfo.data.live_status, ExpirationTime = DateTime.Now.AddSeconds(1) },
                    hidden_till = new() { Value = roomInfo.data.hidden_till, ExpirationTime = DateTime.Now.AddMinutes(1) },
                    lock_till = new() { Value = roomInfo.data.lock_till, ExpirationTime = DateTime.Now.AddMinutes(1) },
                    encrypted = new() { Value = roomInfo.data.encrypted, ExpirationTime = DateTime.Now.AddSeconds(30) },
                    pwd_verified = new() { Value = roomInfo.data.pwd_verified, ExpirationTime = DateTime.Now.AddSeconds(30) },
                    live_time = new() { Value = roomInfo.data.live_time, ExpirationTime = DateTime.Now.AddMinutes(1) },
                    room_shield = new() { Value = roomInfo.data.room_shield, ExpirationTime = DateTime.Now.AddMinutes(30) },
                    is_sp = new() { Value = roomInfo.data.is_sp, ExpirationTime = DateTime.Now.AddSeconds(30) },
                };
                return card;
            }
            else
            {
                return null;
            }
        }

        private static RoomCard ToRoomCard(UserInfo userInfo)
        {
            if (userInfo != null)
            {
                RoomCard card = new RoomCard()
                {
                    UID = userInfo.data.mid,
                    RoomId = userInfo.data.live_room.roomid,
                    name = userInfo.data.name,
                    url = new() { Value = $"https://live.bilibili.com/{userInfo.data.live_room.roomid}", ExpirationTime = DateTime.MaxValue },
                    roomStatus = new() { Value = userInfo.data.live_room.liveStatus, ExpirationTime = DateTime.Now.AddSeconds(5) },
                    title = new() { Value = userInfo.data.live_room.title, ExpirationTime = DateTime.Now.AddSeconds(30) },
                    cover_from_user = new() { Value = userInfo.data.live_room.cover, ExpirationTime = DateTime.Now.AddMinutes(10) },
                    face = new() { Value = userInfo.data.face, ExpirationTime = DateTime.MaxValue },
                    sex = new() { Value = userInfo.data.sex, ExpirationTime = DateTime.MaxValue },
                    sign = new() { Value = userInfo.data.sign, ExpirationTime = DateTime.MaxValue },
                    level = new() { Value = userInfo.data.level, ExpirationTime = DateTime.MaxValue },
                };
                return card;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region internal Class

        private class RoomCard
        {
            /// <summary>
            /// 昵称
            /// (Local值)
            /// </summary>
            internal string name { get; set; } = "";
            /// <summary>
            /// 描述
            /// (Local值)
            /// </summary>
            internal string Description { get; set; } = "";
            /// <summary>
            /// 直播间房间号(长号)
            /// (Local值)
            /// </summary>
            internal long RoomId { get; set; } = -1;
            /// <summary>
            /// 主播mid
            /// </summary>
            internal long UID { get; set; } = -1;
            /// <summary>
            /// 是否自动录制
            /// (Local值)
            /// </summary>
            internal bool IsAutoRec { set; get; } = false;
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            internal bool IsRemind { set; get; } = false;
            /// <summary>
            /// 是否录制弹幕
            /// (Local值)
            /// </summary>
            internal bool IsRecDanmu { set; get; } = false;
            /// <summary>
            /// 特殊标记(Local值)
            /// </summary>
            internal bool Like { set; get; } = false;
            /// <summary>
            /// 该房间录制完成后会执行的Shell命令
            /// (Local值)
            /// </summary>
            internal string Shell { set; get; } = "";
            /// <summary>
            /// 是否持久化储存，用于判断是否需要写到房间配置文件
            /// (Local值)
            /// </summary>
            internal bool IsPersisting { set; get; } = false;
            /// <summary>
            /// 标题
            /// </summary>
            internal ExpansionType<string> title = new ExpansionType<string> { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 主播简介
            /// </summary>
            internal ExpansionType<string> description = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 关注数
            /// </summary>
            internal ExpansionType<int> attention = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            internal ExpansionType<int> online = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 开播时间(未开播时为-62170012800,live_status为1时有效)
            /// </summary>
            internal ExpansionType<long> live_time = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播状态(1为正在直播，2为轮播中)
            /// </summary>
            internal ExpansionType<int> live_status = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            internal ExpansionType<int> short_id = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间分区id
            /// </summary>
            internal ExpansionType<int> area = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间分区名
            /// </summary>
            internal ExpansionType<string> area_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            internal ExpansionType<int> area_v2_id = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            internal ExpansionType<string> area_v2_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            internal ExpansionType<string> area_v2_parent_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            internal ExpansionType<int> area_v2_parent_id = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 主播头像url
            /// </summary>
            internal ExpansionType<string> face = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 系统tag列表(以逗号分割)
            /// </summary>
            internal ExpansionType<string> tag_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 用户自定义tag列表(以逗号分割)
            /// </summary>
            internal ExpansionType<string> tags = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播封面图
            /// </summary>
            internal ExpansionType<string> cover_from_user = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播关键帧图
            /// </summary>
            internal ExpansionType<string> keyframe = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间锁定时间戳
            /// </summary>
            internal ExpansionType<int> lock_till = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 隐藏时间戳
            /// </summary>
            internal ExpansionType<int> hidden_till = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            internal ExpansionType<int> broadcast_type = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 是否p2p
            /// </summary>
            internal ExpansionType<int> need_p2p = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 是否隐藏
            /// </summary>
            internal ExpansionType<bool> is_hidden = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 是否锁定
            /// </summary>
            internal ExpansionType<bool> is_locked = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 是否竖屏
            /// </summary>
            internal ExpansionType<bool> is_portrait = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 是否加密
            /// </summary>
            internal ExpansionType<bool> encrypted = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted=true时才有意义)
            /// </summary>
            internal ExpansionType<bool> pwd_verified = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 房间屏蔽列表应用状态
            /// </summary>
            internal ExpansionType<int> room_shield = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            internal ExpansionType<int> is_sp = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            internal ExpansionType<int> special_type = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            internal ExpansionType<int> roomStatus = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            internal ExpansionType<int> roundStatus = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间网页url
            /// </summary>
            internal ExpansionType<string> url = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 用户等级
            /// </summary>
            internal ExpansionType<int> level = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 主播性别
            /// </summary>
            internal ExpansionType<string> sex = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 主播简介
            /// </summary>
            internal ExpansionType<string> sign = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 下载标识符
            /// </summary>
            internal bool IsDownload = false;
            /// <summary>
            /// 当前Host地址
            /// </summary>
            internal ExpansionType<string> Host = new() { ExpirationTime = DateTime.UnixEpoch, Value = "" };
            /// <summary>
            /// 当前模式（1:FLV 2:HLS）
            /// </summary>
            internal int CurrentMode = 0;
        }
        internal class ExpansionType<T>
        {
            internal DateTime ExpirationTime;
            internal T Value;
        }

        #endregion


    }
}
