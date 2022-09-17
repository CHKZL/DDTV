using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass;

namespace DDTV_Core.SystemAssembly.BilibiliModule.Rooms
{
    public class RoomInfoClass
    {
        /// <summary>
        /// 直播质量
        /// </summary>
        public enum Quality
        {
            /// <summary>
            /// 流畅
            /// </summary>
            Fluency =80,
            /// <summary>
            /// 高清
            /// </summary>
            HighDefinition=150,
            /// <summary>
            /// 蓝光
            /// </summary>
            BluRay=400,
            /// <summary>
            /// 原画
            /// </summary>
            OriginalPainting=10000,
            /// <summary>
            /// 选择默认最高质量
            /// </summary>
            DefaultHighest=10000
        }
        /// <summary>
        /// 直播线路
        /// </summary>
        public enum Line
        {
            /// <summary>
            /// 主线
            /// </summary>
            PrincipalLine = 0,
            /// <summary>
            /// 备线1
            /// </summary>
            SpareLine1 = 1,
            /// <summary>
            /// 备线2
            /// </summary>
            SpareLine2 = 2,
            /// <summary>
            /// 备线3
            /// </summary>
            SpareLine3 = 3,
        }
        public class RoomInfo
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
            /// 未知
            /// </summary>
            public int room_shield { set; get; }
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            public int is_sp { set; get; }
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            public int special_type { set; get; }
            /// <summary>
            /// 是否是临时播放项目
            /// </summary>
            public bool IsTemporaryPlay { set; get; } = false;
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            public int roomStatus { set; get; }
            /// <summary>
            /// (废弃：请使用live_status)(该值为getRoomInfoOld接口冗余值)直播状态(1为正在直播，2为轮播中)
            /// </summary>
            internal int liveStatus { set; get; }
            /// <summary>
            /// (废弃：请使用cover_from_user(该值为getRoomInfoOld接口冗余值)直播封面图
            /// </summary>
            internal string user_cover { get; set; } = "";
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            public int roundStatus { set; get; }
            /// <summary>
            /// 直播间网页url
            /// </summary>
            public string url { set; get; } = "";
            /// <summary>
            /// 描述(Local值)
            /// </summary>
            public string Description { get; set; } = "";
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
            /// 特殊标记(Local值)
            /// </summary>
            public bool Like { set; get; }
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
            /// 房间的WS连接对象类
            /// </summary>
            public RoomWebSocket roomWebSocket { set; get; }= new RoomWebSocket();
            /// <summary>
            /// 下载标识符
            /// </summary>
            public bool IsDownload { set; get; } = false;
            /// <summary>
            /// 房间当前下载任务记录
            /// </summary>
            public List<Downloads> DownloadingList { set; get; } = new List<Downloads>();
            /// <summary>
            /// 是否被用户取消操作
            /// </summary>
            public bool IsUserCancel { set; get; }=false;
            /// <summary>
            /// 房间历史下载记录
            /// </summary>
            public List<Downloads> DownloadedLog { set; get; } = new List<Downloads>();
            /// <summary>
            /// 弹幕录制对象
            /// </summary>
            public API.DanMu.DanMuClass.DanmuMessage DanmuFile { set; get; } = new API.DanMu.DanMuClass.DanmuMessage();
            /// <summary>
            /// 是否正在被编辑
            /// </summary>
            public bool IsCliping { set; get; } = false;
            /// <summary>
            /// 该房间当前的任务时间
            /// </summary>
            public DateTime CreationTime { set; get; } = DateTime.Now; 
            /// <summary>
            /// 该房间最近一次完成的下载任务的文件信息
            /// </summary>
            public DownloadedFileInfo DownloadedFileInfo { set; get; }=new DownloadedFileInfo();
            /// <summary>
            /// 该房间录制完成后会执行的Shell命令
            /// </summary>
            public string Shell { set; get; } = "";
            /// <summary>
            /// 用于房间监控系统，记录的是监控系统检测到开始直播的时间
            /// </summary>
            public DateTime MonitoringSystem_Airtime = DateTime.Now;
            /// <summary>
            ///  用于房间监控系统，记录开播时的关注数
            /// </summary>
            public int MonitoringSystem_Attention = 0;
            /// <summary>
            /// 当前Host地址
            /// </summary>
            public string Host { set; get; } = "";
        }
        public class RoomWebSocket
        {
            /// <summary>
            /// 是否已连接
            /// </summary>
            public bool IsConnect { set; get; }
            public long dokiTime { set; get; }
            /// <summary>
            /// WbdScket服务器信息
            /// </summary>
            public API.LiveChatScript.LiveChatListener LiveChatListener { set; get; } = new API.LiveChatScript.LiveChatListener();
        }
        public class DownloadedFileInfo
        {

            public FileInfo FlvFile { set; get; }
            public FileInfo Mp4File { set; get; }
            public FileInfo DanMuFile { set; get; }
            public FileInfo SCFile { set; get; }
            public FileInfo GuardFile { set; get; }
            public FileInfo GiftFile { set; get; }
        }
    }
}
