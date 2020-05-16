using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Linq;

namespace Auxiliary.LiveChatScript
{
    public class DanmuMessageEventArgs:MessageEventArgs
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public int GuardLV { get; set; }

        public int MessageColor { get; set; }

        public string UserTitile { get; set; }


        internal DanmuMessageEventArgs(JObject obj) : base(obj)
        {
            UserId = (int)obj["info"][2][0];
            UserName = (string)obj["info"][2][1];
            Message = (string)obj["info"][1];
            GuardLV = (int)obj["info"][7];
            MessageColor = (int)obj["info"][0][3];
            UserTitile = (string)obj["info"][5][1];
        }
    }
}
