using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
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
