using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary.LiveChatScript
{
    public class DDTV_EvenntArg : MessageEventArgs
    {

        public int T1 { get; set; }
        public int roomID { set; get; }

        internal DDTV_EvenntArg(JObject obj) : base(obj)
        {
            T1 = (int)obj["T1"];
            roomID = (int)obj["roomID"];
        }
    }
}
