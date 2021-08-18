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
    internal class 修改房间Like配置
    {

        internal static string 修改房间Liek(string mess)
        {
            RoomInfo Rec = new RoomInfo();
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                Rec.RoomId = JO["RoomId"].ToString();
                Rec.RecStatus = bool.Parse(JO["LikeStatus"].ToString());
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomLiekInfo>>((int)ServerSendMessageCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessage.封装消息.房间_修改liek状态.修改Like配置(Rec.RoomId, Rec.RecStatus);
        }
        internal class RoomInfo
        {
            internal string RoomId { set; get; }
            internal bool RecStatus { set; get; }
        }
    }
}
