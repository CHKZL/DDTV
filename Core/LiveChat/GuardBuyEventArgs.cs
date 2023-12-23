using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Core.LiveChat
{
      public class SendGiftEventArgs : MessageEventArgs
    {
        public long UserId { get; set; }

        public string UserName { get; set; } 

        public int GuardLevel { get; set; }
        public string GiftAction { get; set; }
        /// <summary>
        /// 礼物编号
        /// </summary>
        public int GiftId { get; set; }
        /// <summary>
        /// 礼物类型
        /// </summary>
        public int GiftType { get; set; }
        /// <summary>
        /// 礼物名称
        /// </summary>
        public string GiftName { get; set; }
        /// <summary>
        /// 礼物价值
        /// </summary>
        public float GiftPrice { get; set; }

        public int TotalCoin { get; set; }
        /// <summary>
        /// 礼物数量
        /// </summary>
        public int Amount { get; set; }

        public bool IsGoldGift { get; set; }

        public string CoinType { get; set; }

        public long Timestamp { get; set; }

        public string AvatarUrl { get; set; }

        internal SendGiftEventArgs(JsonObject obj) : base(obj)
        {
            UserId = (long)obj["data"]["uid"];
            UserName = (string)obj["data"]["uname"];
            TotalCoin = (int)obj["data"]["total_coin"];
            Amount = (int)obj["data"]["num"];
            GiftName = (string)obj["data"]["giftName"];
            GiftId = (int)obj["data"]["giftId"];
            GiftType = (int)obj["data"]["giftType"];
            GiftPrice = (float)obj["data"]["price"];
            GuardLevel = (int)obj["data"]["guard_level"];
            GiftAction = (string)obj["data"]["action"];
            CoinType = (string)obj["data"]["coin_type"];
            IsGoldGift = (string)obj["data"]["coin_type"] == "gold";
            Timestamp = (long)obj["data"]["timestamp"];
            AvatarUrl = (string)obj["data"]["face"];
        }
    }
}
