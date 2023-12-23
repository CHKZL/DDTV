using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Core.LiveChat
{
    public class GuardBuyEventArgs : MessageEventArgs
    {
        /// <summary>
        /// 舰长等级：1-总督 2-提督 3-舰长
        /// </summary>
        public int GuardLevel { get; set; }

        public string GuardName { get; set; }

        /// <summary>
        /// 花费：单位金瓜子
        /// </summary>
        public int Price { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        //public string GiftId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Number { get; set; }
        public long Timestamp { get; set; }

        internal GuardBuyEventArgs(JsonObject obj) : base(obj)
        {
            GuardLevel = (int)obj["data"]["guard_level"];
            UserId = (int)obj["data"]["uid"];
            UserName = (string)obj["data"]["username"];
            GuardName = (string)obj["data"]["gift_name"];
            Price = (int)obj["data"]["price"];
            Number = (int)obj["data"]["num"];
            Timestamp = (long)obj["data"]["start_time"];
            //GiftId = obj["data"]["gift_id"].Value<string>();//舰长没有
        }
    }
}
