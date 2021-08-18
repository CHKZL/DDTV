using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 消息_获取当前房间配置列表总览信息
    {
        public static string 当前房间配置列表总览信息()
        {
            List<RoomInit.RL> roomInfos = new List<RoomInit.RL>();
            foreach (var item in RoomInit.bilibili房间主表)
            {
                roomInfos.Add(item);
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, roomInfos);
        }
    }
}
