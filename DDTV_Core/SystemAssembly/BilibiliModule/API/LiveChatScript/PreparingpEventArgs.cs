using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    class PreparingpEventArgs : MessageEventArgs
    {
        public int roomID { set; get; }

        internal PreparingpEventArgs(JObject obj) : base(obj)
        {
            roomID = (int)obj["roomID"];
        }
    }
}
