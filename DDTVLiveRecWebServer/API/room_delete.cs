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
                var rlc2 = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                bool okn = false;
                foreach (var item in rlc2.data)
                {
                    if (item.RoomNumber == roomId.ToString())
                    {
                        okn = true;
                        rlc2.data.Remove(item);
                        break;
                    }
                }
                string JOO = JsonConvert.SerializeObject(rlc2);
                MMPU.储存文本(JOO, RoomConfigFile,true);
                InitializeRoomList(roomId, true, false);
                if(okn)
                {
                    return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, new List<roominfo>() {new roominfo()
                    {
                        result=true,
                        messge="删除完成"
                    }});
                }
                else
                {
                    return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功但出现了错误, new List<roominfo>() {new roominfo()
                    {
                        result=false
                    }}, "配置文件中没有该房间号");
                }
               
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
