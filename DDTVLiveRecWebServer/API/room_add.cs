using Auxiliary;
using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.Room;
using static Auxiliary.RoomInit;

namespace DDTVLiveRecWebServer.API
{
    public class room_add
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["Name"],
                context.Request.Form["OfficialName"],
                context.Request.Form["RoomId"],
                context.Request.Form["RecStatus"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "room_add", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<RoomAddInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                string CA = context.Request.Form["RecStatus"];
                bool 是否录制 = false;
                bool.TryParse(CA, out 是否录制);
                return Auxiliary.RequestMessge.封装消息.增加房间.增加(context.Request.Form["RoomId"], context.Request.Form["Name"], context.Request.Form["OfficialName"], 是否录制);
            }
        }
    }
}
