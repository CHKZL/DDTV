using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.User
{
    internal class UserClass
    {
        public class Live_room
        {
            /// <summary>
            /// 
            /// </summary>
            public int roomStatus { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int liveStatus { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string url { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int roomid { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int roundStatus { get; set; }
        }

        public class Data
        {
            /// <summary>
            /// 
            /// </summary>
            public long mid { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 女
            /// </summary>
            public string sex { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string face { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string sign { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int level { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Live_room live_room { get; set; }
        }

        public class UserInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int ttl { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Data data { get; set; }
        }
    }
    internal class follow
    {
        public class ListItem
        {
            /// <summary>
            /// 
            /// </summary>
            public long mid { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string uname { get; set; }
        }

        public class Data
        {
            /// <summary>
            /// 
            /// </summary>
            public List<ListItem> list { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int re_version { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int total { get; set; }
        }

        public class Root
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int ttl { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Data data { get; set; }
        }
    }
    internal class VtbsFile
    {
        public long mid { set; get; }
        public string name { set; get; }
        public string roomid { set; get; }
    }
}
