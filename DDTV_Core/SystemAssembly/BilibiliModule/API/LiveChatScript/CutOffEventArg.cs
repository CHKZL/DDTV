using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    public class CutOffEventArg : MessageEventArgs
    {
        public string msg { get; set; }
        public int roomID { set; get; }

        internal CutOffEventArg(JObject obj) : base(obj)
        {
            msg = (string)obj["msg"];
            roomID = (int)obj["roomID"];
        }
    }
}
