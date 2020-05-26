﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public static class Server
    {
        public static readonly string IP_ADDRESS = "39.98.207.17";
        public static readonly int PORT = 11433;
        public static readonly string PROJECT_ADDRESS = "https://github.com/CHKZL/DDTV2/releases/latest";
        public static class RequestCode
        {
            public static readonly int GET_NEW_MEMBER_LIST_CONTENT = 20001;
            public static readonly int GET_PUSH_NOTIFICATION_1 = 20005;
            public static readonly int GET_TOGGLE_DYNAMIC_NOTIFICATION = 20009;
            public static readonly int GET_DYNAMIC_NOTIFICATION = 20010;
            public static readonly int GET_LATEST_VERSION_NUMBER = 20011;
            public static readonly int GET_UPDATE_ANNOUNCEMENT = 20012;
            public static readonly int GET_LIVELSIT = 20016;
            public static readonly int GET_DDC_TIME_NUMBER = 30001;
        }
    }
}
