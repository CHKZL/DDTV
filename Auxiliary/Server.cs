using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public static class Server
    {
#if true
        public static readonly IPAddress IP_ADDRESS = MMPU.根据URL获取IP地址("pro.ddtv.pro");
#elif false
         public static readonly IPAddress IP_ADDRESS = MMPU.根据URL获取IP地址("api.ddtv.pro");
#elif false
        public static readonly IPAddress IP_ADDRESS = MMPU.根据URL获取IP地址("192.168.199.100");
#elif false
        public static readonly IPAddress IP_ADDRESS = MMPU.根据URL获取IP地址("127.0.0.1");
#endif
        public static readonly int PORT = 11433;
        public static readonly string PROJECT_ADDRESS = "https://github.com/CHKZL/DDTV/releases/latest";
        public static class RequestCode
        {
            public static readonly int GET_NEW_MEMBER_LIST_CONTENT = 20001;
            public static readonly int GET_PUSH_NOTIFICATION_1 = 20005;
            public static readonly int GET_TOGGLE_DYNAMIC_NOTIFICATION = 20009;
            public static readonly int GET_DYNAMIC_NOTIFICATION = 20010;
            public static readonly int GET_DDTV_LATEST_VERSION_NUMBER = 20011;
            public static readonly int GET_DDTV_UPDATE_ANNOUNCEMENT = 20012;
            public static readonly int GET_GET_DDTVLiveRec_LATEST_VERSION_NUMBER = 20013;
            public static readonly int GET_DDTVLiveRec_UPDATE_ANNOUNCEMENT = 20014;
            public static readonly int GET_LIVELSIT = 20016;
            public static readonly int GET_VTBSROOMLIST = 20017;
            public static readonly int GET_DDC_TIME_NUMBER = 30001;
            public static readonly int SET_DokiDoki_DDTV = 40001;
            public static readonly int SET_DokiDoki_DDTVLiveRec = 40002;
            public static readonly int GET_IP = 50001;
        }
    }
}
