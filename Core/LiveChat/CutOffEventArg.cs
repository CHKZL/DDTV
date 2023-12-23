using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Core.LiveChat
{
    public class CutOffEventArg : MessageEventArgs
    {
        public string msg { get; set; }
        public int roomID { set; get; }

        internal CutOffEventArg(JsonObject obj) : base(obj)
        {
            msg = (string)obj["msg"];
            roomID = (int)obj["roomID"];
        }
    }
}
