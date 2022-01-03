using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    public class ActivityBannerEventArgs : MessageEventArgs
    {
        public int id { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public string background { get; set; }
        public string jump_url { get; set; }
        public string title_color { get; set; }
        public int closeable { get; set; }
        public int banner_type { get; set; }
        public int weight { get; set; }
        public int add_banner { get; set; }

        internal ActivityBannerEventArgs(JObject obj) : base(obj)
        {
            id = (int)obj["data"]["id"];
            title = (string)obj["data"]["title"];
            cover = (string)obj["data"]["cover"];
            background = (string)obj["data"]["background"];
            jump_url = (string)obj["data"]["jump_url"];
            title_color = (string)obj["data"]["title_color"];
            closeable = (int)obj["data"]["closeable"];
            banner_type = (int)obj["data"]["banner_type"];
            weight = (int)obj["data"]["weight"];
            add_banner = (int)obj["data"]["add_banner"];
        }
    }
}
