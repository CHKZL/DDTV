using Auxiliary;
using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.Room;
using static Auxiliary.RoomInit;

namespace DDTVLiveRecWebServer.API
{
    public class room_status
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
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
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "room_status", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<RoomStatusInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                bool 录制状态 = false;
                bool.TryParse(context.Request.Form["RecStatus"], out 录制状态);
                return Auxiliary.RequestMessge.封装消息.修改房间录制配置.修改录制配置(context.Request.Form["RoomId"], 录制状态);
            }
        }
    }
}
