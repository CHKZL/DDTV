using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.Rooms
{
    internal class RoomInfoClass
    {
        internal class RoomInfo
        {
            /// <summary>
            /// 标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// 房间号
            /// </summary>
            public int Room_id { get; set; }
            /// <summary>
            /// UID
            /// </summary>
            public long Uid { get; set; }
            /// <summary>
            /// 忘了是啥的参数
            /// </summary>
            public int Online { get; set; }
            /// <summary>
            /// 开播时间(live_status为1时有效)
            /// </summary>
            public int Live_time { get; set; }
            /// <summary>
            /// 直播状态(1为正在直播，2为轮播中)
            /// </summary>
            public int Live_status { get; set; }
            /// <summary>
            /// 不知道是啥参数
            /// </summary>
            public int Short_id { get; set; }
            /// <summary>
            /// 分区号
            /// </summary>
            public int Area { get; set; }
            /// <summary>
            /// 分区名称
            /// </summary>
            public string Area_name { get; set; }
            /// <summary>
            /// 二级子分区号
            /// </summary>
            public int Area_v2_id { get; set; }
            /// <summary>
            /// 二级子分区名称
            /// </summary>
            public string Area_v2_name { get; set; }
            /// <summary>
            /// 二级父分区名称
            /// </summary>
            public string Area_v2_parent_name { get; set; }
            /// <summary>
            /// 二级父分区号
            /// </summary>
            public int Area_v2_parent_id { get; set; }
            /// <summary>
            /// 用户名
            /// </summary>
            public string Uname { get; set; }
            /// <summary>
            /// 头像Url
            /// </summary>
            public string Face { get; set; }
            /// <summary>
            /// 系统tag列表(以逗号分割)
            /// </summary>
            public string Tag_name { get; set; }
            /// <summary>
            /// 用户自定义tag列表(以逗号分割)
            /// </summary>
            public string Tags { get; set; }
            /// <summary>
            /// 直播封面图
            /// </summary>
            public string Cover_from_user { get; set; }
            /// <summary>
            /// 直播关键帧图
            /// </summary>
            public string Keyframe { get; set; }
            /// <summary>
            /// 上锁时间
            /// </summary>
            public string Lock_till { get; set; }
            /// <summary>
            /// 隐藏时间
            /// </summary>
            public string Hidden_till { get; set; }
            /// <summary>
            /// 广播类型(不知道用来干啥的，常年为0)
            /// </summary>
            public int Broadcast_type { get; set; }

        }
    }
}
