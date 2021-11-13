using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.Rooms
{
    internal class Rooms
    {
        internal static Dictionary<long, RoomInfoClass.RoomInfo> RoomInfo = new();
        /// <summary>
        /// 使用uids获取房间状态信息
        /// </summary>
        /// <param name="UIDList">需要更新的UID列表</param>
        /// <returns></returns>
        internal static void UpdateRoomsInfo(List<long> UIDList)
        {
            API.RoomInfo.get_status_info_by_uids(UIDList);
        }
    }
}
