using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu
{
    public class DanMuClass
    {
        public class DanMuWssInfo
        {
            public long uid { set; get; }
            public string token { set; get; }
            public List<Host> host_list { set; get; } = new List<Host>();

        }
        public class Host
        {
            public string host { set; get; }
            public int port { set; get; }
            public int wss_port { set; get; }
            public int ws_port { set; get; }
        }
    }
}
