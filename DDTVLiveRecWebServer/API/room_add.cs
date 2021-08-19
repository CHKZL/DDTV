using Auxiliary;
using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Room;
using static Auxiliary.RoomInit;

namespace DDTVLiveRecWebServer.API
{
    public class room_add
    {
        public static string Web(HttpContext context)
        {
            try
            {
                int B = context.Request.Form.Count();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomAddInfo>>((int)ServerSendMessageCode.鉴权失败, null, "请求的表单格式不正确！");
            }
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
                return ReturnInfoPackage.InfoPkak<Message<RoomAddInfo>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                string CA = context.Request.Form["RecStatus"];
                bool 是否录制 = false;
                bool.TryParse(CA, out 是否录制);
                return Auxiliary.RequestMessage.封装消息.房间_增加房间.增加(context.Request.Form["RoomId"], context.Request.Form["Name"], context.Request.Form["OfficialName"], 是否录制);
            }
        }
    }
}
