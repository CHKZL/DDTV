using Core.Network.Methods;
using Masuit.Tools.Hardware;
using System.Text.Json.Serialization;
using static Core.Network.Methods.Room;
using static Core.Network.Methods.User;
using static Core.RuntimeObject.RoomList;

namespace Core.RuntimeObject
{
    public class _Room
    {
        private static Dictionary<(long, long), RoomCard> roomInfos = new Dictionary<(long Uid, long Room_Id), RoomCard>();
        internal static RoomCard GetCardForUID(long UID)
        {

        }
    }

    
    public class RoomList
    {
        #region private Properties
        
        #endregion

        #region Public Method
        public static List<(int id, long uid, long roomid, string name, string title, long downloadedSize, double downloadRate, string state, DateTime startTime)> GetOverview()
        {
            List<(int id, long uid, long roomid, string name, string title, long downloadedSize, double downloadRate, string state, DateTime startTime)> values = new();
            int i = 1;
            foreach (var item in roomInfos)
            {
                if (item.Value.DownInfo.IsDownload)
                {
                    values.Add((id: i, uid: item.UID, roomid: item.RoomId, name: item.Name, title: item.Title.Value, downloadedSize: item.DownInfo.DownloadSize, downloadRate: item.DownInfo.RealTimeDownloadSpe, state: item.DownInfo.Status.ToString(), startTime: item.DownInfo.StartTime));
                    i++;
                }
            }
            return values;
        }
        public static bool GetLiveStatus(long RoomId)
        {
            return _GetLiveStatus(RoomId);
        }

        public static string GetNickname(long Uid)
        {
            return _GetNickname(Uid);
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

        public static (int Total,int Download) GetTasksInDownloadCount()
        {
            return (roomInfos.Count, roomInfos.Count(roomCard => roomCard.DownInfo.IsDownload));
        }


        #endregion

        #region internal Method

        /// <summary>
        /// 更新相关uid列表的直播间状态
        /// </summary>
        /// <param name="UIDList">如果传null则是更新整个roomInfos状态</param>
        /// <returns></returns>
        internal static async Task BatchUpdateRoomStatusForLiveStream(List<long> UIDList = null)
        {
            int _PageSize = 1500;
            if (UIDList == null)
            {
                UIDList = new();
                foreach (var item in roomInfos)
                {
                    UIDList.Add(item.UID);
                }
            }
            var list = new List<List<long>>();
            for (int i = 0; i < UIDList.Count; i += _PageSize)
            {
                list.Add(UIDList.GetRange(i, Math.Min(_PageSize, UIDList.Count - i)));
            }
            foreach (var item in list)
            {
                await _BatchUpdateRoomStatusForLiveStream(item);
            }
        }

        internal static async Task _BatchUpdateRoomStatusForLiveStream(List<long> UIDList)
        {
            await Task.Run(() =>
            {
                UidsInfo_Class uidsInfo_Class = GetRoomList(UIDList);
                if (uidsInfo_Class.data != null && uidsInfo_Class.data.Count > 0)
                {
                    foreach (var item in uidsInfo_Class.data)
                    {
                        long.TryParse(item.Key, out long uid);
                        if (uid > 0)
                        {
                            int index = roomInfos.FindIndex(x => x.UID == uid);
                            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.UID == uid);
                            if (index != -1)
                            {
                                roomInfos[index] = ToRoomCard(item.Value,roomCard);
                            }
                        }
                    }
                }
            });
        }

        #endregion


        #region private Method

        private static bool _GetLiveStatus(long RoomId)
        {
            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.RoomId == RoomId);
            if (roomCard == null || roomCard.live_status.ExpirationTime < DateTime.Now)
            {
                RoomCard card = ToRoomCard(GetRoomInfo(RoomId),roomCard);
                if (card == null)
                    return false;
                else if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[roomInfos.FindIndex(x => x.RoomId == RoomId)] = card;
                return card.live_status.Value == 1 ? true : false;
            }
            else
                return roomCard.live_status.Value == 1 ? true : false;
        }

        private static string _GetNickname(long Uid)
        {
            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.UID == Uid);
            if (roomCard == null || string.IsNullOrEmpty(roomCard.Name))
            {
                RoomCard card = ToRoomCard(GetUserInfo(Uid),roomCard);
                if (card == null)
                    return "获取昵称失败";
                else if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[roomInfos.FindIndex(x => x.UID == Uid)] = card;
                return card.Name;
            }
            else
                return roomCard.Name;
        }

        private static long _GetUid(long RoomId)
        {
            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.RoomId == RoomId);
            if (roomCard == null || roomCard.RoomId < 0)
            {
                RoomCard card = ToRoomCard(GetRoomInfo(RoomId),roomCard);
                if (card == null)
                    return -1;
                else if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[roomInfos.FindIndex(x => x.RoomId == RoomId)] = card;
                return card.UID;
            }
            else
                return roomCard.UID;
        }

        private static long _GetRoomId(long Uid)
        {
            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.UID == Uid);
            if (roomCard == null || roomCard.RoomId < 0)
            {
                RoomCard card = ToRoomCard(GetUserInfo(Uid),roomCard);
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
            if (roomCard == null || string.IsNullOrEmpty(roomCard.Title.Value) || roomCard.Title.ExpirationTime < DateTime.Now)
            {
                RoomCard card = ToRoomCard(GetUserInfo(Uid),roomCard);
                if (card == null)
                    return "";
                else if (roomCard == null)
                    roomInfos.Add(card);
                else
                    roomInfos[roomInfos.FindIndex(x => x.UID == Uid)] = card;
                return card.Title.Value;
            }
            else
                return roomCard.Title.Value;
        }

        private static RoomCard ToRoomCard(UidsInfo_Class.Data data, RoomCard OldCard)
        {
            if (data != null)
            {
                if (OldCard == null)
                {
                    RoomCard card = new RoomCard()
                    {
                        UID = data.uid,
                        Title = new() { Value = data.title, ExpirationTime = DateTime.Now.AddSeconds(10) },
                        RoomId = data.room_id,
                        live_time = new() { Value = data.live_time, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        live_status = new() { Value = data.live_status, ExpirationTime = DateTime.Now.AddSeconds(1) },
                        short_id = new() { Value = data.short_id, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        area = new() { Value = data.area, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        area_name = new() { Value = data.area_name, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        area_v2_id = new() { Value = data.area_v2_id, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        area_v2_name = new() { Value = data.area_v2_name, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        area_v2_parent_name = new() { Value = data.area_v2_parent_name, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        area_v2_parent_id = new() { Value = data.area_v2_parent_id, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        Name = data.uname,
                        face = new() { Value = data.face, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        tag_name = new() { Value = data.tag_name, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        tags = new() { Value = data.tags, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        cover_from_user = new() { Value = data.cover_from_user, ExpirationTime = DateTime.Now.AddSeconds(30) },
                        keyframe = new() { Value = data.keyframe, ExpirationTime = DateTime.Now.AddSeconds(30) },
                        lock_till = new() { Value = data.lock_till, ExpirationTime = DateTime.Now.AddSeconds(30) },
                        hidden_till = new() { Value = data.hidden_till, ExpirationTime = DateTime.Now.AddSeconds(30) },
                        broadcast_type = new() { Value = data.broadcast_type, ExpirationTime = DateTime.Now.AddSeconds(30) },
                    };
                    return card;
                }
                else
                {
                    OldCard.UID = data.uid;
                    OldCard.Title = new() { Value = data.title, ExpirationTime = DateTime.Now.AddSeconds(10) };
                    OldCard.RoomId = data.room_id;
                    OldCard.live_time = new() { Value = data.live_time, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.live_status = new() { Value = data.live_status, ExpirationTime = DateTime.Now.AddSeconds(1) };
                    OldCard.short_id = new() { Value = data.short_id, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.area = new() { Value = data.area, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.area_name = new() { Value = data.area_name, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.area_v2_id = new() { Value = data.area_v2_id, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.area_v2_name = new() { Value = data.area_v2_name, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.area_v2_parent_name = new() { Value = data.area_v2_parent_name, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.area_v2_parent_id = new() { Value = data.area_v2_parent_id, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.Name = data.uname;
                    OldCard.face = new() { Value = data.face, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.tag_name = new() { Value = data.tag_name, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.tags = new() { Value = data.tags, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.cover_from_user = new() { Value = data.cover_from_user, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    OldCard.keyframe = new() { Value = data.keyframe, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    OldCard.lock_till = new() { Value = data.lock_till, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    OldCard.hidden_till = new() { Value = data.hidden_till, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    OldCard.broadcast_type = new() { Value = data.broadcast_type, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    return OldCard;
                }

            }
            else
            {
                return null;
            }
        }

        private static RoomCard ToRoomCard(RoomInfo_Class roomInfo, RoomCard OldCard)
        {
            if (roomInfo != null)
            {
                if (OldCard == null)
                {
                    RoomCard card = new RoomCard()
                    {
                        UID = roomInfo.data.uid,
                        RoomId = roomInfo.data.room_id,
                        short_id = new() { Value = roomInfo.data.short_id, ExpirationTime = DateTime.MaxValue },
                        need_p2p = new() { Value = roomInfo.data.need_p2p, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        is_hidden = new() { Value = roomInfo.data.is_hidden, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        is_locked = new() { Value = roomInfo.data.is_locked, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        is_portrait = new() { Value = roomInfo.data.is_portrait, ExpirationTime = DateTime.Now.AddMinutes(1) },
                        live_status = new() { Value = roomInfo.data.live_status, ExpirationTime = DateTime.Now.AddSeconds(1) },
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
                    OldCard.UID = roomInfo.data.uid;
                    OldCard.RoomId = roomInfo.data.room_id;
                    OldCard.short_id = new() { Value = roomInfo.data.short_id, ExpirationTime = DateTime.MaxValue };
                    OldCard.need_p2p = new() { Value = roomInfo.data.need_p2p, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.is_hidden = new() { Value = roomInfo.data.is_hidden, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.is_locked = new() { Value = roomInfo.data.is_locked, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.is_portrait = new() { Value = roomInfo.data.is_portrait, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.live_status = new() { Value = roomInfo.data.live_status, ExpirationTime = DateTime.Now.AddSeconds(1) };
                    OldCard.encrypted = new() { Value = roomInfo.data.encrypted, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    OldCard.pwd_verified = new() { Value = roomInfo.data.pwd_verified, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    OldCard.live_time = new() { Value = roomInfo.data.live_time, ExpirationTime = DateTime.Now.AddMinutes(1) };
                    OldCard.room_shield = new() { Value = roomInfo.data.room_shield, ExpirationTime = DateTime.Now.AddMinutes(30) };
                    OldCard.is_sp = new() { Value = roomInfo.data.is_sp, ExpirationTime = DateTime.Now.AddSeconds(30) };
                    return OldCard;
                }
            }
            else
            {
                return null;
            }
        }

        private static RoomCard ToRoomCard(UserInfo userInfo, RoomCard OldCard)
        {
            if (userInfo != null)
            {
                if (OldCard == null)
                {
                    RoomCard card = new RoomCard()
                    {
                        UID = userInfo.data.mid,
                        RoomId = userInfo.data.live_room.roomid,
                        Name = userInfo.data.name,
                        url = new() { Value = $"https://live.bilibili.com/{userInfo.data.live_room.roomid}", ExpirationTime = DateTime.MaxValue },
                        roomStatus = new() { Value = userInfo.data.live_room.liveStatus, ExpirationTime = DateTime.Now.AddSeconds(5) },
                        Title = new() { Value = userInfo.data.live_room.title, ExpirationTime = DateTime.Now.AddSeconds(10) },
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
                    OldCard.UID = userInfo.data.mid;
                    OldCard.RoomId = userInfo.data.live_room.roomid;
                    OldCard.Name = userInfo.data.name;
                    OldCard.url = new() { Value = $"https://live.bilibili.com/{userInfo.data.live_room.roomid}", ExpirationTime = DateTime.MaxValue };
                    OldCard.roomStatus = new() { Value = userInfo.data.live_room.liveStatus, ExpirationTime = DateTime.Now.AddSeconds(5) };
                    OldCard.Title = new() { Value = userInfo.data.live_room.title, ExpirationTime = DateTime.Now.AddSeconds(10) };
                    OldCard.cover_from_user = new() { Value = userInfo.data.live_room.cover, ExpirationTime = DateTime.Now.AddMinutes(10) };
                    OldCard.face = new() { Value = userInfo.data.face, ExpirationTime = DateTime.MaxValue };
                    OldCard.sex = new() { Value = userInfo.data.sex, ExpirationTime = DateTime.MaxValue };
                    OldCard.sign = new() { Value = userInfo.data.sign, ExpirationTime = DateTime.MaxValue };
                    OldCard.level = new() { Value = userInfo.data.level, ExpirationTime = DateTime.MaxValue };
                    return OldCard;
                }
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region public Class


        public class RoomCard
        {
            [JsonPropertyName("name")]
            /// <summary>
            /// 昵称
            /// (Local值)
            /// </summary>
            public string Name { get; set; } = "";
            [JsonPropertyName("Description")]
            /// <summary>
            /// 描述
            /// (Local值)
            /// </summary>
            public string Description { get; set; } = "";
            [JsonPropertyName("RoomId")]
            /// <summary>
            /// 直播间房间号(长号)
            /// (Local值)
            /// </summary>
            public long RoomId { get; set; } = -1;
            [JsonPropertyName("UID")]
            /// <summary>
            /// 主播mid
            /// (Local值)
            /// </summary>
            public long UID { get; set; } = -1;
            [JsonPropertyName("IsAutoRec")]
            /// <summary>
            /// 是否自动录制
            /// (Local值)
            /// </summary>
            public bool IsAutoRec { set; get; } = false;
            [JsonPropertyName("IsRemind")]
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            public bool IsRemind { set; get; } = false;
            [JsonPropertyName("IsRecDanmu")]
            /// <summary>
            /// 是否录制弹幕
            /// (Local值)
            /// </summary>
            public bool IsRecDanmu { set; get; } = false;
            [JsonPropertyName("Like")]
            /// <summary>
            /// 特殊标记
            /// (Local值)
            /// </summary>
            public bool Like { set; get; } = false;
            [JsonPropertyName("Shell")]
            /// <summary>
            /// 该房间录制完成后会执行的Shell命令
            /// (Local值)
            /// </summary>
            public string Shell { set; get; } = "";
            [JsonPropertyName("IsPersisting")]
            /// <summary>
            /// 是否持久化储存，用于判断是否需要写到房间配置文件
            /// (Local值)
            /// </summary>
            public bool IsPersisting { set; get; } = false;
            /// <summary>
            /// 标题
            /// </summary>
            public ExpansionType<string> Title = new ExpansionType<string> { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 主播简介
            /// </summary>
            public ExpansionType<string> description = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 关注数
            /// </summary>
            public ExpansionType<int> attention = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            public ExpansionType<int> online = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 开播时间(未开播时为-62170012800,live_status为1时有效)
            /// </summary>
            public ExpansionType<long> live_time = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播状态(1为正在直播，2为轮播中)
            /// </summary>
            public ExpansionType<int> live_status = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            public ExpansionType<int> short_id = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间分区id
            /// </summary>
            public ExpansionType<int> area = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间分区名
            /// </summary>
            public ExpansionType<string> area_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            public ExpansionType<int> area_v2_id = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            public ExpansionType<string> area_v2_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            public ExpansionType<string> area_v2_parent_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            public ExpansionType<int> area_v2_parent_id = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 主播头像url
            /// </summary>
            public ExpansionType<string> face = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 系统tag列表(以逗号分割)
            /// </summary>
            public ExpansionType<string> tag_name = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 用户自定义tag列表(以逗号分割)
            /// </summary>
            public ExpansionType<string> tags = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播封面图
            /// </summary>
            public ExpansionType<string> cover_from_user = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播关键帧图
            /// </summary>
            public ExpansionType<string> keyframe = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播间锁定时间戳
            /// </summary>
            public ExpansionType<string> lock_till = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 隐藏时间戳
            /// </summary>
            public ExpansionType<string> hidden_till = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            public ExpansionType<int> broadcast_type = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 是否p2p
            /// </summary>
            public ExpansionType<int> need_p2p = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 是否隐藏
            /// </summary>
            public ExpansionType<bool> is_hidden = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 是否锁定
            /// </summary>
            public ExpansionType<bool> is_locked = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 是否竖屏
            /// </summary>
            public ExpansionType<bool> is_portrait = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 是否加密
            /// </summary>
            public ExpansionType<bool> encrypted = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted=true时才有意义)
            /// </summary>
            public ExpansionType<bool> pwd_verified = new() { ExpirationTime = DateTime.UnixEpoch, Value = false };
            /// <summary>
            /// 房间屏蔽列表应用状态
            /// </summary>
            public ExpansionType<int> room_shield = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            public ExpansionType<int> is_sp = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            public ExpansionType<int> special_type = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            public ExpansionType<int> roomStatus = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            public ExpansionType<int> roundStatus = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 直播间网页url
            /// </summary>
            public ExpansionType<string> url = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 用户等级
            /// </summary>
            public ExpansionType<int> level = new() { ExpirationTime = DateTime.UnixEpoch, Value = -1 };
            /// <summary>
            /// 主播性别
            /// </summary>
            public ExpansionType<string> sex = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
            /// <summary>
            /// 主播简介
            /// </summary>
            public ExpansionType<string> sign = new() { ExpirationTime = DateTime.UnixEpoch, Value = string.Empty };
       
            /// <summary>
            /// 当前Host地址
            /// </summary>
            public ExpansionType<string> Host = new() { ExpirationTime = DateTime.UnixEpoch, Value = "" };
            /// <summary>
            /// 当前模式（1:FLV 2:HLS）
            /// </summary>
            public int CurrentMode = 0;
            /// <summary>
            /// 运行时下载相关信息
            /// </summary>
            public DownloadInfo DownInfo = new();
            public class DownloadInfo
            {
                /// <summary>
                /// 当前是否在下载
                /// </summary>
                public bool IsDownload = false;
                /// <summary>
                /// 当前房间下载任务总大小
                /// </summary>
                public long DownloadSize = 0;
                /// <summary>
                /// 实时下载速度
                /// </summary>
                public double RealTimeDownloadSpe = 0;
                /// <summary>
                /// 任务状态
                /// </summary>
                public DownloadStatus Status = DownloadStatus.NewTask;
                /// <summary>
                /// 任务开始时间
                /// </summary>
                public DateTime StartTime= DateTime.UnixEpoch;
                /// <summary>
                /// 任务结束时间
                /// </summary>
                public DateTime EndTime= DateTime.UnixEpoch;
                public DownloadInfo Clone()
                {
                    return new DownloadInfo
                    {
                        IsDownload = this.IsDownload,
                        DownloadSize = this.DownloadSize,
                        RealTimeDownloadSpe = this.RealTimeDownloadSpe,
                        Status = this.Status,
                        StartTime = this.StartTime,
                        EndTime = this.EndTime,
                    };
                }
            }


            public RoomCard Clone()
            {
                return new RoomCard
                {
                    Name = this.Name,
                    Description = this.Description,
                    RoomId = this.RoomId,
                    UID = this.UID,
                    IsAutoRec = this.IsAutoRec,
                    IsRemind = this.IsRemind,
                    IsRecDanmu = this.IsRecDanmu,
                    Like = this.Like,
                    Shell = this.Shell,
                    IsPersisting = this.IsPersisting,
                    Title = this.Title.Clone(),
                    description = this.description.Clone(),
                    attention = this.attention.Clone(),
                    online = this.online.Clone(),
                    live_time = this.live_time.Clone(),
                    live_status = this.live_status.Clone(),
                    short_id = this.short_id.Clone(),
                    area = this.area.Clone(),
                    area_name = this.area_name.Clone(),
                    area_v2_id = this.area_v2_id.Clone(),
                    area_v2_name = this.area_v2_name.Clone(),
                    area_v2_parent_name = this.area_v2_parent_name.Clone(),
                    area_v2_parent_id = this.area_v2_parent_id.Clone(),
                    face = this.face.Clone(),
                    tag_name = this.tag_name.Clone(),
                    tags = this.tags.Clone(),
                    cover_from_user = this.cover_from_user.Clone(),
                    keyframe = this.keyframe.Clone(),
                    lock_till = this.lock_till.Clone(),
                    hidden_till = this.hidden_till.Clone(),
                    broadcast_type = this.broadcast_type.Clone(),
                    need_p2p = this.need_p2p.Clone(),
                    is_hidden = this.is_hidden.Clone(),
                    is_locked = this.is_locked.Clone(),
                    is_portrait = this.is_portrait.Clone(),
                    encrypted = this.encrypted.Clone(),
                    pwd_verified = this.pwd_verified.Clone(),
                    room_shield = this.room_shield.Clone(),
                    is_sp = this.is_sp.Clone(),
                    special_type = this.special_type.Clone(),
                    roomStatus = this.roomStatus.Clone(),
                    roundStatus = this.roundStatus.Clone(),
                    url = this.url.Clone(),
                    level = this.level.Clone(),
                    sex = this.sex.Clone(),
                    sign = this.sign.Clone(),
                    Host = this.Host.Clone(),
                    CurrentMode = this.CurrentMode,
                    DownInfo = this.DownInfo
                };
            }

            public class ExpansionType<T>
            {
                public DateTime ExpirationTime { set; get; }
                public T Value { set; get; }
                public ExpansionType<T> Clone()
                {
                    return new ExpansionType<T>
                    {
                        ExpirationTime = this.ExpirationTime,
                        Value = this.Value
                    };
                }
            }
             public enum DownloadStatus
            {
                /// <summary>
                /// 新任务
                /// </summary>
                NewTask,
                /// <summary>
                /// 已准备
                /// </summary>
                Standby,
                /// <summary>
                /// 下载中
                /// </summary>
                Downloading,
                /// <summary>
                /// 下载结束
                /// </summary>
                DownloadComplete,
                /// <summary>
                /// 取消下载
                /// </summary>
                Cancel,
            }
        }


        #endregion


    }
}
