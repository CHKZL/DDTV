using DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using System;
using System.Threading;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.WebSocket
{
    public class WebSocket
    {
        public static RoomInfoClass.RoomInfo ConnectRoomAsync(long uid)
        {
            while(!Rooms.Rooms.RoomInfo.ContainsKey(uid))
            {
               ConfigModule.RoomConfig.AddRoom(uid, "Temporary");
                Thread.Sleep(500);
            }
            if (Rooms.Rooms.RoomInfo.ContainsKey(uid))
            {
                if (!Rooms.Rooms.RoomInfo[uid].roomWebSocket.IsConnect)
                {
                    Rooms.Rooms.RoomInfo[uid].roomWebSocket.LiveChatListener=new LiveChatListener();
                    Rooms.Rooms.RoomInfo[uid].roomWebSocket.LiveChatListener.host =DanMu.DanMu.getDanmuInfo(uid);
                    Rooms.Rooms.RoomInfo[uid].roomWebSocket.LiveChatListener.Connect(Rooms.Rooms.RoomInfo[uid].room_id, uid);
                    Rooms.Rooms.RoomInfo[uid].roomWebSocket.IsConnect=!Rooms.Rooms.RoomInfo[uid].roomWebSocket.IsConnect;
                    return Rooms.Rooms.RoomInfo[uid];
                }
                else
                {
                    return Rooms.Rooms.RoomInfo[uid];
                    //
                }
            }
            return null;
        }
    }
}
