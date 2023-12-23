using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Core.LiveChat
{
    public class SuperchatEventArg : MessageEventArgs
    {
        public string Message { get; set; }
        public string messageTrans { get; set; }
        public int RoomID { set; get; }
        public long UserId { set; get; }
        public string UserName { set; get; }
        public long Ts { set; get; }
        public int SCID { set; get; }
        public double Price { set; get; }
        public int Rate { set; get; }
        public int TimeLength { set; get; }
        public long Timestamp { set; get; }
        public string face { set; get; }
        public JsonObject OriginalInformation { set; get; }


        internal SuperchatEventArg(JsonObject obj) : base(obj)
        {
            Message = (string)obj["data"]["message"];
            messageTrans = (string)obj["data"]["message_trans"];
            UserId = (long)obj["data"]["uid"];
            UserName = (string)obj["data"]["user_info"]["uname"];
            face = (string)obj["data"]["user_info"]["face"];
            Ts = (long)obj["data"]["ts"];
            SCID = (int)obj["data"]["id"];
            Price = (double)obj["data"]["price"];
            Rate = (int)obj["data"]["rate"];
            TimeLength = (int)obj["data"]["time"];
            Timestamp = (long)obj["data"]["start_time"];
            OriginalInformation = obj;
        }
    }
}
