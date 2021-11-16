using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    internal class RoomConfigClass
    {
       public class RoomList
        {
            public List<RoomCard> data { set; get; } = new List<RoomCard>();
        }
        internal class RoomCard
        {
            /// <summary>
            /// 名称
            /// </summary>
            public string name { get; set; } = "";
            /// <summary>
            /// 描述
            /// </summary>
            public string Description { get; set; } = "";
            /// <summary>
            /// 房间号
            /// </summary>
            public int RoomId { get; set; }
            /// <summary>
            /// 用户UID(mid)
            /// </summary>
            public long UID { set; get; } = 0;
            /// <summary>
            /// 自动录制
            /// </summary>
            public bool IsAutoRec { get; set; }
            /// <summary>
            /// 开播提醒
            /// </summary>
            public bool IsRemind { get; set; }
            /// <summary>
            /// 优先标记
            /// </summary>
            public bool Like { get; set; }
            /// <summary>
            /// (抛弃)(请使用name)名称
            /// </summary>
            public string Name { set; get; } = "";
            /// <summary>
            /// (抛弃)(请使用)房间号
            /// </summary>
            public string RoomNumber { set; get; } = "";
            /// <summary>
            /// (抛弃)(请使用IsAutoRec)是否自动录制
            /// </summary>
            public bool VideoStatus { get; set; }
            /// <summary>
            /// (抛弃)(请使用IsRemind)是否开播提醒
            /// </summary>
            public bool RemindStatus { get; set; }
        }
    }
}
