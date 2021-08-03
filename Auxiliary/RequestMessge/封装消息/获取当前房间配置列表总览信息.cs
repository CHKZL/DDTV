using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 获取当前房间配置列表总览信息
    {
        public static string 当前房间配置列表总览信息()
        {
            List<RoomInit.RL> roomInfos = new List<RoomInit.RL>();
            foreach (var item in RoomInit.bilibili房间主表)
            {
                roomInfos.Add(item);
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, roomInfos);
        }
    }
}
