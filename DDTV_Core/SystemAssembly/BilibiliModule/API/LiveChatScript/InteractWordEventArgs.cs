using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Linq;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    public class InteractWordEventArgs : MessageEventArgs
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public int MsgType { set; get; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public int Timestamp { set; get; }
        /// <summary>
        /// UID
        /// </summary>
        public long Uid { set; get; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Uname { set; get; }

        internal InteractWordEventArgs(JObject obj) : base(obj)
        {
            MsgType = (int)obj["data"]["msg_type"];
            Timestamp = (int)obj["data"]["timestamp"];
            Uid = (long)obj["data"]["uid"];
            Uname = (string)obj["data"]["uname"];
        }
    }
}
