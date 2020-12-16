using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary.LiveChatScript
{
    class LiveEventArgs : MessageEventArgs
    {
        public int roomID { set; get; }

        internal LiveEventArgs(JObject obj) : base(obj)
        {
            roomID = (int)obj["roomid"];
        }
    }
}
