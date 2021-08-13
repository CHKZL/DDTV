using Auxiliary.RequestMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Room;
using static Auxiliary.WSServer.WSServer;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 增加配置中监听的房间
    {
        internal static string 增加房间(string mess)
        {
            RoomInfo Rec = new RoomInfo();
            try
            {  
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                Rec.Name = JO["Name"].ToString();
                Rec.OfficialName = JO["OfficialName"].ToString();
                Rec.RoomId = JO["RoomId"].ToString();
                Rec.RecStatus = bool.Parse(JO["RecStatus"].ToString());
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomAddInfo>>((int)ServerSendMessageCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessage.封装消息.增加房间.增加(Rec.RoomId, Rec.Name, Rec.OfficialName, Rec.RecStatus);
        }
        internal class RoomInfo
        {
            public string Name { set; get; }
            public string OfficialName { set; get; }
            public string RoomId { set; get; }
            public bool RecStatus { set; get; }
        }
    }
}
