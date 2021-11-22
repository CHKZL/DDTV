//using BilibiliLiveChatScript;
using DDTV_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core_Test
{
    internal class Program
    {
        private static void Main(string[] arg)
        {
            InitDDTV_Core.Core_Init();
            while(true)
            {
                Console.ReadKey();
            }
            //LiveChatListener liveChatListener = new LiveChatListener();
            //liveChatListener.Connect(725364);
            //liveChatListener.MessageReceived+=LiveChatListener_MessageReceived; 
        }

        //private static void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        //{
        //    switch (e)
        //    {
        //        case DanmuMessageEventArgs Danmu:
        //            Console.WriteLine($"收到弹幕信息:{Time.Operate.ConvertTimeStampToDateTime(Danmu.Timestamp)} {Danmu.UserName}({Danmu.UserId}):{Danmu.Message}");
        //            break;
        //        case SuperchatEventArg SuperchatEvent:
        //            Console.WriteLine($"收到Superchat信息:");
        //            break;
        //        case GuardBuyEventArgs GuardBuyEvent:
        //            Console.WriteLine($"收到舰组信息:{Time.Operate.ConvertTimeStampToDateTime(GuardBuyEvent.Timestamp)} {GuardBuyEvent.UserName}({GuardBuyEvent.UserId}):开通了{GuardBuyEvent.Number}个月的{GuardBuyEvent.GiftName}(单价{GuardBuyEvent.Price})");
        //            break;
        //        case SendGiftEventArgs sendGiftEventArgs:
        //            Console.WriteLine($"收到礼物:{Time.Operate.ConvertTimeStampToDateTime(sendGiftEventArgs.Timestamp)} {sendGiftEventArgs.UserName}({sendGiftEventArgs.UserId}):价值{sendGiftEventArgs.GiftPrice}的{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}");
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
  
}
