using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.RuntimeObject
{
    internal class Room
    {
        private static List<RoomInfo> roomInfos = new List<RoomInfo>();


        private class RoomInfo
        {

            /// <summary>
            /// 直播间房间号(直播间实际房间号)
            /// </summary>
            private int room_id { get; set; } = 0;
            /// <summary>
            /// 主播mid
            /// </summary>
            private long uid { get; set; } = 0;
            /// <summary>
            /// 描述(Local值)
            /// </summary>
            private string Description { get; set; } = "";
            /// <summary>
            /// 是否自动录制(Local值)
            /// </summary>
            private bool IsAutoRec { set; get; }
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            private bool IsRemind { set; get; }
            /// <summary>
            /// 是否录制弹幕(Local值)
            /// </summary>
            private bool IsRecDanmu { set; get; }
            /// <summary>
            /// 特殊标记(Local值)
            /// </summary>
            private bool Like { set; get; }
            /// <summary>
            /// 该房间录制完成后会执行的Shell命令
            /// </summary>
            private string Shell { set; get; } = "";
            /// <summary>
            /// 标题
            /// </summary>
            private string title = "";
            /// <summary>
            /// 主播简介
            /// </summary>
            private string description = "";
            /// <summary>
            /// 关注数
            /// </summary>
            private int attention = 0;
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            private int online = 0;
            /// <summary>
            /// 开播时间(未开播时为-62170012800,live_status为1时有效)
            /// </summary>
            private long live_time = 0;
            /// <summary>
            /// 直播状态(1为正在直播，2为轮播中)
            /// </summary>
            private int live_status = 0;
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            private int short_id = 0;
            /// <summary>
            /// 直播间分区id
            /// </summary>
            private int area = 0;
            /// <summary>
            /// 直播间分区名
            /// </summary>
            private string area_name = "";
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            private int area_v2_id = 0;
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            private string area_v2_name = "";
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            private string area_v2_parent_name = "";
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            private int area_v2_parent_id = 0;
            /// <summary>
            /// 用户名
            /// </summary>
            private string uname = "";
            /// <summary>
            /// 主播头像url
            /// </summary>
            private string face = "";
            /// <summary>
            /// 系统tag列表(以逗号分割)
            /// </summary>
            private string tag_name = "";
            /// <summary>
            /// 用户自定义tag列表(以逗号分割)
            /// </summary>
            private string tags = "";
            /// <summary>
            /// 直播封面图
            /// </summary>
            private string cover_from_user = "";
            /// <summary>
            /// 直播关键帧图
            /// </summary>
            private string keyframe = "";
            /// <summary>
            /// 直播间封禁信息
            /// </summary>
            private string lock_till = "";
            /// <summary>
            /// 直播间隐藏信息
            /// </summary>
            private string hidden_till = "";
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            private int broadcast_type = 0;
            /// <summary>
            /// 是否p2p
            /// </summary>
            private int need_p2p = 0;
            /// <summary>
            /// 是否隐藏
            /// </summary>
            private bool is_hidden = false;
            /// <summary>
            /// 是否锁定
            /// </summary>
            private bool is_locked = false;
            /// <summary>
            /// 是否竖屏
            /// </summary>
            private bool is_portrait = false;
            /// <summary>
            /// 是否加密
            /// </summary>
            private bool encrypted = false;
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted=true时才有意义)
            /// </summary>
            private bool pwd_verified = false;
            /// <summary>
            /// 未知
            /// </summary>
            private int room_shield = 0;
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            private int is_sp = 0;
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            private int special_type = 0;
            /// <summary>
            /// 是否是临时播放项目
            /// </summary>
            private bool IsTemporaryPlay = false;
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            private int roomStatus = 0;
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            private int roundStatus = 0;
            /// <summary>
            /// 直播间网页url
            /// </summary>
            private string url = "";

            /// <summary>
            /// 用户等级
            /// </summary>
            private int level = 0;
            /// <summary>
            /// 主播性别
            /// </summary>
            private string sex = "";
            /// <summary>
            /// 主播简介
            /// </summary>
            private string sign = "";
            /// <summary>
            /// 下载标识符
            /// </summary>
            private bool IsDownload = false;
            /// <summary>
            /// 播放标识符
            /// </summary>
            private bool IsPlayer = false;
            /// <summary>
            /// 是否正在被编辑
            /// </summary>
            private bool IsCliping = false;
            /// <summary>
            /// 该房间当前的任务时间
            /// </summary>
            private DateTime CreationTime = DateTime.Now;

            /// <summary>
            /// 当前Host地址
            /// </summary>
            private string Host = "";
            /// <summary>
            /// 当前模式（1:FLV 2:HLS）
            /// </summary>
            private int CurrentMode = 0;
        }
    }
}
