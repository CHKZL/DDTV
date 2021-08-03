using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.Room;
using static Auxiliary.RoomInit;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 删除房间
    {
        public static string 删除(string RoomId)
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
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功但出现了错误, new List<RoomDeleteInfo>() {new RoomDeleteInfo()
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
            MMPU.储存文本(JOO, RoomConfigFile, true);
            InitializeRoomList(roomId, true, false);
            if (okn)
            {
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, new List<RoomDeleteInfo>() {new RoomDeleteInfo()
                    {
                        result=true,
                        messge="删除完成"
                    }});
            }
            else
            {
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功但出现了错误, new List<RoomDeleteInfo>() {new RoomDeleteInfo()
                    {
                        result=false
                    }}, "配置文件中没有该房间号");
            }

        }
    }
}
