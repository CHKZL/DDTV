using Auxiliary;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                RoomBox rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                RoomBox RB = new RoomBox
                {
                    data = new List<RoomCadr>()
                };
                if (rlc.data != null)
                {
                    foreach (var item in rlc.data)
                    {
                        if (item.RoomNumber == roomId.ToString())
                        {
                            return ReturnInfoPackage.InfoPkak(鉴权结果, new List<roominfo>() {new roominfo()
                            {
                                result=false,
                                messge="配置文件中已有该房间号存在"
                            }});
                        }
                        RB.data.Add(item);
                    }
                }
                long UID = 0;
                if (DataCache.读缓存(DataCache.缓存头.通过房间号获取UID + roomId, 0, out string GETUID))
                {
                    try
                    {
                        UID = long.Parse(GETUID);
                    }
                    catch (Exception) { }
                }
                if (UID < 1)
                {
                    try
                    {
                        UID = long.Parse(bilibili.根据房间号获取房间信息.通过房间号获取UID(roomId.ToString()));
                    }
                    catch (Exception) { }
                }
                string CA = context.Request.Form["RecStatus"];
                bool 是否录制 = bool.Parse(CA);
                RB.data.Add(new RoomCadr() { Name = context.Request.Form["Name"], OfficialName = context.Request.Form["OfficialName"], RoomNumber = roomId.ToString(), UID = UID,VideoStatus= 是否录制 });
                string JOO = JsonConvert.SerializeObject(RB);
                MMPU.储存文本(JOO, RoomConfigFile,true);

                bilibili.已连接的直播间状态.Add(new bilibili.直播间状态() { 房间号 = roomId });
               
                bilibili.RoomList.Add(new RoomInfo
                {
                    房间号 = roomId.ToString(),
                    标题 = "",
                    是否录制弹幕 = false,
                    是否录制视频 = 是否录制,
                    UID = UID.ToString(),
                    直播开始时间 = "",
                    名称 = context.Request.Form["Name"],
                    直播状态 = false,
                    原名 = context.Request.Form["OfficialName"],
                    是否提醒 = false,
                    平台 = "bilibili"
                });
                return ReturnInfoPackage.InfoPkak(鉴权结果, new List<roominfo>() {new roominfo()
                    {
                        result=true,
                        messge=context.Request.Form["Name"] + "[" + roomId.ToString() + "]添加完成"
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
