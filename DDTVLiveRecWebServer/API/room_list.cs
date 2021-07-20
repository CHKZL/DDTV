using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class room_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "room_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>(鉴权结果, null);
            }
            else
            {
                List<Auxiliary.RoomInit.RL> roomInfos = new List<Auxiliary.RoomInit.RL>();
                foreach (var item in Auxiliary.RoomInit.bilibili房间主表)
                {
                    roomInfos.Add(item);
                }
                return ReturnInfoPackage.InfoPkak(鉴权结果, roomInfos);
            }
        }
        private class Messge : Auxiliary.RoomInit.RL
        {
            public static List<Auxiliary.RoomInit.RL> Package { set; get; }
        }    
    }
}
