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
    public class room_delete
    {
        public static string Web(HttpContext context)
        {
            try
            {
                int B = context.Request.Form.Count();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomDeleteInfo>>((int)ServerSendMessageCode.鉴权失败, null, "请求的表单格式不正确！");
            }
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["RoomId"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "room_delete", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomDeleteInfo>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return Auxiliary.RequestMessage.封装消息.删除房间.删除(context.Request.Form["RoomId"]);
            }
        }

    }
}
