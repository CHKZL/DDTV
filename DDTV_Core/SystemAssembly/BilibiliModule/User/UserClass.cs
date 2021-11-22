using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.User
{
    internal class UserClass
    {
        public class UserInfo
        {
            /// <summary>
            /// UID
            /// </summary>
            public long uid { get; set; }
            /// <summary>
            /// 昵称
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 性别
            /// </summary>
            public string sex { get; set; }
            /// <summary>
            /// 头像
            /// </summary>
            public string face { set; get; }
            /// <summary>
            /// 用户签名
            /// </summary>
            public string sign { set; get; }
            /// <summary>
            /// 用户等级
            /// </summary>
            public int level { set; get; }
            /// <summary>
            /// 房间状态
            /// </summary>
            public int roomStatus { set; get; }
            /// <summary>
            /// 直播状态
            /// </summary>
            public int liveStatus { set; get; }
            /// <summary>
            /// 直播间地址
            /// </summary>
            public string url { set; get; }
            /// <summary>
            /// 房间号
            /// </summary>
            public int roomid { set; get; }
            /// <summary>
            /// 标题
            /// </summary>
            public string title { set; get; }
            /// <summary>
            /// 轮播状态
            /// </summary>
            public int roundStatus { set; get; }
        }
    }
}
