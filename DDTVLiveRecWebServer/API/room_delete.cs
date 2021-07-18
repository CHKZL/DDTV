using Auxiliary;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Auxiliary.RoomInit;

namespace DDTVLiveRecWebServer.API
{
    public class room_delete
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = true;
            foreach (var item in new List<string>() {
                context.Request.Form["RooId"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = false;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "room_delete", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>(鉴权结果, null);
            }
            else
            {
                int roomId = 0;
                try
                {
                    string roomDD = bilibili.根据房间号获取房间信息.获取真实房间号(context.Request.Form["RoomId"]);
                    if (!string.IsNullOrEmpty(roomDD))
                    {
                        roomId = int.Parse(roomDD);
                    }
                }
                catch (Exception)
                {
                    return ReturnInfoPackage.InfoPkak(鉴权结果, new List<roominfo>() {new roominfo()
                    {
                        result=false,
                        messge="输入的直播间房间号不符合房间号规则(数字)"
                    }});
                }
                var rlc2 = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                foreach (var item in rlc2.data)
                {
                    if (item.RoomNumber == roomId.ToString())
                    {
                        rlc2.data.Remove(item);
                        break;
                    }
                }
                string JOO = JsonConvert.SerializeObject(rlc2);
                MMPU.储存文本(JOO, RoomConfigFile);
                InitializeRoomList(roomId, true, false);
                return ReturnInfoPackage.InfoPkak(鉴权结果, new List<roominfo>() {new roominfo()
                    {
                        result=true,
                        messge="删除完成"
                    }});
            }
        }
        private class Messge : ReturnInfoPackage.Messge<roominfo>
        {
            public static new List<roominfo> Package { set; get; }
        }
        private class roominfo
        {
            public bool result { set; get; }
            public string messge { set; get; }
        }
    }
}
