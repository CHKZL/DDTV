using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary.LiveChatScript
{
    public class WelcomeEventArgs : MessageEventArgs
    {
        public int UserId { get; set; }

        public string UserName { get; set; }


        public int svip { get; set; }

        public int vip { get; set; }

        //public int mock_effect { get; set; }

        internal WelcomeEventArgs(JObject obj) : base(obj)
        {
            UserId = (int)obj["data"]["uid"];
            UserName = (string)obj["data"]["uname"];
            svip = (int)obj["data"]["svip"];
            vip = (int)obj["data"]["vip"];
            //mock_effect = (int)obj["data"]["mock_effect"];
        }
    }
}
