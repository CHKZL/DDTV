using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary.LiveChatScript
{
    class SuperchatEventArg : MessageEventArgs
    {
        public string message { get; set; }
        public int roomID { set; get; }
        public int userId { set; get; }
        public long ts { set; get; }
        public int SCID { set; get; }
        public double price { set; get; }
        public int rate { set; get; }
        public JObject OriginalInformation { set; get; }


        internal SuperchatEventArg(JObject obj) : base(obj)
        {
            message = (string)obj["data"]["message"];
            roomID = (int)obj["roomID"];
            userId = (int)obj["data"]["uid"];
            ts = (long)obj["data"]["ts"];
            SCID = (int)obj["data"]["id"];
            price = (double)obj["data"]["price"];
            rate = (int)obj["data"]["rate"];
            OriginalInformation = obj;
        }
    }
}
