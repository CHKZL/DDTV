using Core.LogModule;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Core.LiveChat
{
    public class GuardRenewEventArgs : MessageEventArgs
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

        public long UserId { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Number { get; set; }
        public long Timestamp { get; set; }

        internal GuardRenewEventArgs(JsonObject obj) : base(obj)
        {
            try
            {
                GuardLevel = (int)obj["data"]["guard_info"]["guard_level"];
                UserId = (long)obj["data"]["sender_uinfo"]["uid"];
                UserName = (string)obj["data"]["sender_uinfo"]["base"]["name"];
                GuardName = (string)obj["data"]["guard_info"]["role_name"];
                Price = (int)obj["data"]["pay_info"]["price"];
                Number = (int)obj["data"]["pay_info"]["num"];
                Timestamp = (long)obj["data"]["guard_info"]["start_time"];
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(GuardRenewEventArgs), "转换大航海信息发生错误，可能阿B修改了数据结构，请检查数据格式", ex);
            }
            //GiftId = obj["data"]["gift_id"].Value<string>();//舰长没有
        }
    }
}
