using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;

namespace DDTVLiveRecWebServer.API
{
    public class room_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "room_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<Auxiliary.RoomInit.RL>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.获取当前房间配置列表总览信息.当前房间配置列表总览信息();
            }
        }
    }
}
