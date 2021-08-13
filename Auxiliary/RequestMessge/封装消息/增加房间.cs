using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Room;
using static Auxiliary.RoomInit;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 增加房间
    {
        public static string 增加(string RoomId,string Name,string OfficialName,bool RecStatus)
        {
            int roomId = 0;
            try
            {
                string roomDD = bilibili.根据房间号获取房间信息.获取真实房间号(RoomId);
                if (!string.IsNullOrEmpty(roomDD))
                {
                    roomId = int.Parse(roomDD);
                }
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功但出现了错误, new List<RoomAddInfo>() {new RoomAddInfo()
                    {
                        result=false,
                        message="输入的直播间房间号不符合房间号规则(数字)"
                    }}, "输入的直播间房间号不符合房间号规则(数字)");
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
                        return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功但出现了错误, new List<RoomAddInfo>() {new RoomAddInfo()
                            {
                                result=false
                            }}, "配置文件中已有该房间号存在");
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
            bool 是否录制 = RecStatus;
            RB.data.Add(new RoomCadr() { Name = Name, OfficialName = OfficialName, RoomNumber = roomId.ToString(), UID = UID, VideoStatus = 是否录制 });
            string JOO = JsonConvert.SerializeObject(RB);
            MMPU.储存文本(JOO, RoomConfigFile, true);

            bilibili.已连接的直播间状态.Add(new bilibili.直播间状态() { 房间号 = roomId });

            bilibili.RoomList.Add(new RoomInfo
            {
                房间号 = roomId.ToString(),
                标题 = "",
                是否录制弹幕 = false,
                是否录制视频 = 是否录制,
                UID = UID.ToString(),
                直播开始时间 = "",
                名称 = Name,
                直播状态 = false,
                原名 = OfficialName,
                是否提醒 = false,
                平台 = "bilibili"
            });
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, new List<RoomAddInfo>() {new RoomAddInfo()
                    {
                        result=true,
                        message=Name + "[" + roomId.ToString() + "]添加完成"
                    }});
        }
    }
}
