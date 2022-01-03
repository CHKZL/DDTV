using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    public class EntryEffectEventArgs : MessageEventArgs
    {
        /// <summary>
        /// 舰长头像
        /// </summary>
        public string face { set; get; }
        /// <summary>
        /// 舰长uid
        /// </summary>
        public long uid { set; get; }
        /// <summary>
        /// 欢迎信息
        /// </summary>
        public string copy_writing { set; get; }
        /// <summary>
        /// 欢迎信息V2
        /// </summary>
        public string copy_writing_v2 { set; get; }

        internal EntryEffectEventArgs(JObject obj) : base(obj)
        {
            uid = (long)obj["data"]["uid"];
            face = (string)obj["data"]["face"];
            copy_writing = (string)obj["data"]["copy_writing"];
            copy_writing_v2 = (string)obj["data"]["copy_writing_v2"];
        }
    }
}
