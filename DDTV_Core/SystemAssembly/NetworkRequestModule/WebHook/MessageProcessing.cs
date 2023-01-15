using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.BilibiliModule.Rooms.RoomInfoClass;
using static DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook.WebHook;

namespace DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook
{
    public class MessageProcessing
    {
        public static string Processing(HookType hookType, long uid, string id)
        {
            Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo);
            DownloadedFileInfo _d = new DownloadedFileInfo();
            if(roomInfo!=null)
            {
                foreach (var item in roomInfo.DownloadedFileInfo.AfterRepairFiles)
                {
                    _d.AfterRepairFiles.Add(item.FullName);
                }
                foreach (var item in roomInfo.DownloadedFileInfo.BeforeRepairFiles)
                {
                    _d.BeforeRepairFiles.Add(item.FullName);
                }
                _d.DanMuFile = roomInfo.DownloadedFileInfo.DanMuFile != null ? roomInfo.DownloadedFileInfo.DanMuFile.FullName : null;
                _d.SCFile = roomInfo.DownloadedFileInfo.SCFile != null ? roomInfo.DownloadedFileInfo.SCFile.FullName : null;
                _d.GiftFile = roomInfo.DownloadedFileInfo.GiftFile != null ? roomInfo.DownloadedFileInfo.GiftFile.FullName : null;
                _d.GuardFile = roomInfo.DownloadedFileInfo.GuardFile != null ? roomInfo.DownloadedFileInfo.GuardFile.FullName : null;
            }


            string msg = JsonConvert.SerializeObject(new Message()
            {
                id = id,
                type = hookType,
                type_name = nameof(hookType),
                hook_time = DateTime.Now,
                uid = uid,
                user_info = uid == 0||roomInfo==null ? null : new user_info()
                {
                    attention = roomInfo.attention,
                    face = roomInfo.face,
                    name = roomInfo.uname,
                    sign = roomInfo.sign,
                    uid = roomInfo.uid,
                },
                room_Info = uid == 0||roomInfo==null ? null : new room_info()
                {
                    area = roomInfo.area,
                    attention = roomInfo.attention,
                    area_name = roomInfo.area_name,
                    area_v2_id = roomInfo.area_v2_id,
                    area_v2_name = roomInfo.area_v2_name,
                    area_v2_parent_id = roomInfo.area_v2_parent_id,
                    area_v2_parent_name = roomInfo.area_v2_parent_name,
                    uid = uid,
                    IsAutoRec = roomInfo.IsAutoRec,
                    broadcast_type = roomInfo.broadcast_type,
                    cover_from_user = roomInfo.cover_from_user,
                    description = roomInfo.description,
                    DownloadedFileInfo = _d,
                    encrypted = roomInfo.encrypted,
                    face = roomInfo.face,
                    hidden_till = roomInfo.hidden_till,
                    IsRecDanmu = roomInfo.IsRecDanmu,
                    IsRemind = roomInfo.IsRemind,
                    is_hidden = roomInfo.is_hidden,
                    is_locked = roomInfo.is_locked,
                    is_portrait = roomInfo.is_portrait,
                    is_sp = roomInfo.is_sp,
                    keyframe = roomInfo.keyframe,
                    level = roomInfo.level,
                    live_status = roomInfo.live_status,
                    live_time = roomInfo.live_time,
                    lock_till = roomInfo.lock_till,
                    need_p2p = roomInfo.need_p2p,
                    online = roomInfo.online,
                    pwd_verified = roomInfo.pwd_verified,
                    roomStatus = roomInfo.roomStatus,
                    room_id = roomInfo.room_id,
                    roundStatus = roomInfo.roundStatus,
                    sex = roomInfo.sex,
                    Shell = roomInfo.Shell,
                    short_id = roomInfo.short_id,
                    sign = roomInfo.sign,
                    special_type = roomInfo.special_type,
                    tags = roomInfo.tags,
                    tag_name = roomInfo.tag_name,
                    title = roomInfo.title,
                    uname = roomInfo.uname,
                    url = roomInfo.url,
                }
            });
            return msg;
        }
        public class Message
        {
            /// <summary>
            /// WebHook请求的唯一标识
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// 类型
            /// </summary>
            public HookType type { set; get; }
            /// <summary>
            /// 返回的HookType类型枚举名称
            /// </summary>
            public string type_name { set; get; }
            /// <summary>
            /// 用户UID
            /// </summary>
            public long uid { set; get; }
            /// <summary>
            /// 发起该WebHook请求的时间
            /// </summary>
            public DateTime hook_time { set; get; }
            /// <summary>
            /// 用户信息
            /// </summary>
            public user_info user_info { set; get; } = new user_info();
            /// <summary>
            /// 直播间信息
            /// </summary>
            public room_info room_Info { set; get; }=new room_info();
        }
        public class user_info
        {
            public string name { set; get; }
            public string face { set; get; }
            public long uid { get; set; }
            public string sign { set; get; }
            public int attention { set; get; }
        }
        public class room_info
        {
            /// <summary>
            /// 标题
            /// </summary>
            public string title { get; set; } = "";
            /// <summary>
            /// 主播简介
            /// </summary>
            public string description { get; set; } = "";
            /// <summary>
            /// 关注数
            /// </summary>
            public int attention { get; set; }
            /// <summary>
            /// 直播间房间号(直播间实际房间号)
            /// </summary>
            public int room_id { get; set; }
            /// <summary>
            /// 主播mid
            /// </summary>
            public long uid { get; set; }
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            public int online { get; set; }
            /// <summary>
            /// 开播时间(未开播时为-62170012800,live_status为1时有效)
            /// </summary>
            public long live_time { get; set; }
            /// <summary>
            /// 直播状态(1为正在直播，2为轮播中)
            /// </summary>
            public int live_status { get; set; }
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            public int short_id { get; set; }
            /// <summary>
            /// 直播间分区id
            /// </summary>
            public int area { get; set; }
            /// <summary>
            /// 直播间分区名
            /// </summary>
            public string area_name { get; set; } = "";
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            public int area_v2_id { get; set; }
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            public string area_v2_name { get; set; } = "";
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            public string area_v2_parent_name { get; set; } = "";
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            public int area_v2_parent_id { get; set; }
            /// <summary>
            /// 用户名
            /// </summary>
            public string uname { get; set; } = "";
            /// <summary>
            /// 主播头像url
            /// </summary>
            public string face { get; set; } = "";
            /// <summary>
            /// 系统tag列表(以逗号分割)
            /// </summary>
            public string tag_name { get; set; } = "";
            /// <summary>
            /// 用户自定义tag列表(以逗号分割)
            /// </summary>
            public string tags { get; set; } = "";
            /// <summary>
            /// 直播封面图
            /// </summary>
            public string cover_from_user { get; set; } = "";
            /// <summary>
            /// 直播关键帧图
            /// </summary>
            public string keyframe { get; set; } = "";
            /// <summary>
            /// 直播间封禁信息
            /// </summary>
            public string lock_till { get; set; } = "";
            /// <summary>
            /// 直播间隐藏信息
            /// </summary>
            public string hidden_till { get; set; } = "";
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            public int broadcast_type { get; set; }
            /// <summary>
            /// 是否p2p
            /// </summary>
            public int need_p2p { set; get; }
            /// <summary>
            /// 是否隐藏
            /// </summary>
            public bool is_hidden { set; get; }
            /// <summary>
            /// 是否锁定
            /// </summary>
            public bool is_locked { set; get; }
            /// <summary>
            /// 是否竖屏
            /// </summary>
            public bool is_portrait { set; get; }
            /// <summary>
            /// 是否加密
            /// </summary>
            public bool encrypted { set; get; }
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted=true时才有意义)
            /// </summary>
            public bool pwd_verified { set; get; }
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            public int is_sp { set; get; }
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            public int special_type { set; get; }
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            public int roomStatus { set; get; }
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            public int roundStatus { set; get; }
            /// <summary>
            /// 直播间网页url
            /// </summary>
            public string url { set; get; } = "";
            /// <summary>
            /// 是否自动录制(Local值)
            /// </summary>
            public bool IsAutoRec { set; get; }
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            public bool IsRemind { set; get; }
            /// <summary>
            /// 是否录制弹幕(Local值)
            /// </summary>
            public bool IsRecDanmu { set; get; }
            /// <summary>
            /// 用户等级
            /// </summary>
            public int level { set; get; }
            /// <summary>
            /// 主播性别
            /// </summary>
            public string sex { set; get; }
            /// <summary>
            /// 主播简介
            /// </summary>
            public string sign { set; get; }
            /// <summary>
            /// 该房间最近一次完成的下载任务的文件信息
            /// </summary>
            public DownloadedFileInfo DownloadedFileInfo { set; get; } = new DownloadedFileInfo();
            /// <summary>
            /// 该房间录制完成后会执行的Shell命令
            /// </summary>
            public string Shell { set; get; } = "";
        }
        public class DownloadedFileInfo
        {
              /// <summary>
            /// 修复后的文件完整路径List
            /// </summary>
            public List<string> AfterRepairFiles { set; get; } = new List<string>();
            /// <summary>
            /// 修复前的文件完整路径List
            /// </summary>
            public List<string> BeforeRepairFiles { set; get; } = new List<string>();
            /// <summary>
            /// 录制的弹幕文件
            /// </summary>
            public string DanMuFile { set; get; }
            /// <summary>
            /// 录制的SC记录文件
            /// </summary>
            public string SCFile { set; get; }
            /// <summary>
            /// 录制的大航海记录文件
            /// </summary>
            public string GuardFile { set; get; }
            /// <summary>
            /// 录制的礼物记录文件
            /// </summary>
            public string GiftFile { set; get; }
        }
    }
}
