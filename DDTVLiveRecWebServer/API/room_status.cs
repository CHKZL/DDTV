using Auxiliary;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                return ReturnInfoPackage.InfoPkak<Messge>((int)ReturnInfoPackage.MessgeCode.鉴权失败, null);
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
                    return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功但出现了错误, new List<roominfo>() {new roominfo()
                    {
                        result=false
                    }}, "输入的直播间房间号不符合房间号规则(数字)");
                }
                var data = new List<RoomCadr>();
                foreach (var item in bilibili房间主表)
                {
                    if(item.唯一码== roomId.ToString())
                    {
                        data.Add(new RoomCadr
                        {
                            LiveStatus = item.直播状态,
                            Name = item.名称,
                            OfficialName = item.原名,
                            RoomNumber = item.唯一码,
                            VideoStatus = bool.Parse(context.Request.Form["RecStatus"]),
                            Types = item.平台,
                            RemindStatus = item.是否提醒,
                            status = false
                        });
                    }
                    else
                    {
                        data.Add(new RoomCadr
                        {
                            LiveStatus = item.直播状态,
                            Name = item.名称,
                            OfficialName = item.原名,
                            RoomNumber = item.唯一码,
                            VideoStatus = item.是否录制,
                            Types = item.平台,
                            RemindStatus = item.是否提醒,
                            status = false
                        });
                    }
                }
                //RoomInit.根据唯一码获取直播状态(房间表[i].唯一码)
                string JOO = JsonConvert.SerializeObject(new RoomBox() { data = data });
                MMPU.储存文本(JOO, RoomConfigFile,true);
                InitializeRoomList(0, false, false);
                return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, new List<roominfo>() {new roominfo()
                    {
                        result=true,
                        messge="修改设置完成"
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
