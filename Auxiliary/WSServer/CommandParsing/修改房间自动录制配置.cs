using Auxiliary.RequestMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Room;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 修改房间自动录制配置
    {
        internal static string 修改房间录制配置(string mess)
        {
            RoomInfo Rec = new RoomInfo();
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                Rec.RoomId = JO["RoomId"].ToString();
                Rec.RecStatus = bool.Parse(JO["RecStatus"].ToString());
                bool 是否全部房间 = false;
                bool.TryParse(JO["AllRoom"].ToString(), out 是否全部房间);
                Rec.AllRoom = 是否全部房间;
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomStatusInfo>>((int)ServerSendMessageCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessage.封装消息.房间_修改房间录制配置.修改录制配置(Rec.RoomId, Rec.RecStatus, Rec.AllRoom);
        }
        internal class RoomInfo
        {
            internal string RoomId { set; get; }
            internal bool RecStatus { set; get; }
            internal bool AllRoom { set; get; } = false;
        }
    }
}
