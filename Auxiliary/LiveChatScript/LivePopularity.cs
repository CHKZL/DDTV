using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary.LiveChatScript
{
    public class LivePopularity : MessageEventArgs
    {
        public int LiveP { get; set; }
        public int roomID { set; get; }

        internal LivePopularity(JObject obj) : base(obj)
        {
            LiveP = (int)obj["LiveP"];
            roomID= (int)obj["roomID"];
        }
    }
}
