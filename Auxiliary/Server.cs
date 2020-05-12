using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public static class Server
    {
        public static readonly string PROJECT_ADDRESS = "https://github.com/CHKZL/DDTV2/releases/latest";
        public static class RequestCode
        {
            public static readonly int GET_TOGGLE_PUSH_NOTIFICATION = 20009;
            public static readonly int GET_PUSH_NOTIFICATION = 20010;
            public static readonly int GET_LATEST_VERSION_NUMBER = 20011;
            public static readonly int GET_UPDATE_ANNOUNCEMENT = 20011;
        }
    }
}
