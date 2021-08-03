using Auxiliary.RequestMessge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.Room;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 修改房间自动录制配置
    {
        internal static string 修改房间录制配置(string mess)
        {
            RoomInfo Rec = new RoomInfo();
            try
            {
                Rec = JsonConvert.DeserializeObject<RoomInfo>(mess);
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Messge<RoomStatusInfo>>((int)ServerSendMessgeCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessge.封装消息.修改房间录制配置.修改录制配置(Rec.RoomId, Rec.RecStatus);
        }
        internal class RoomInfo
        {
            internal string RoomId { set; get; }
            internal bool RecStatus { set; get; }
        }
    }
}
