using Auxiliary;
using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Room;
using static Auxiliary.RoomInit;

namespace DDTVLiveRecWebServer.API
{
    public class room_status
    {
        public static string Web(HttpContext context)
        {
            try
            {
                int B = context.Request.Form.Count();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomStatusInfo>>((int)ServerSendMessageCode.鉴权失败, null, "请求的表单格式不正确！");
            }
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
                return ReturnInfoPackage.InfoPkak<Message<RoomStatusInfo>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                bool 录制状态 = false;
                bool.TryParse(context.Request.Form["RecStatus"], out 录制状态);
                return Auxiliary.RequestMessage.封装消息.修改房间录制配置.修改录制配置(context.Request.Form["RoomId"], 录制状态);
            }
        }
    }
}
