using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary.LiveChatScript
{
    public class WarningEventArg : MessageEventArgs
    {
        public string msg { get; set; }
        public int roomID { set; get; }

        internal WarningEventArg(JObject obj) : base(obj)
        {
            msg = (string)obj["msg"];
            roomID = (int)obj["roomID"];
        }
    }
}
