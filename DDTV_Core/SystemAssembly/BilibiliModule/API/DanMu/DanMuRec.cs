using DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu
{
    public class DanMuRec
    {
        public static void Rec(long UID)
        {
            Task.Run(() =>
            {
                StartRecDanmu(UID);
            });
        }
        public static void StartRecDanmu(long UID)
        {
            RoomInfoClass.RoomInfo _ = WebSocket.WebSocket.ConnectRoomAsync(UID);
            _.DanmuFile.TimeStopwatch = new System.Diagnostics.Stopwatch();
            _.DanmuFile.TimeStopwatch.Start();
            _.roomWebSocket.LiveChatListener.DisposeSent += LiveChatListener_DisposeSent;
            _.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
        }

        private static void LiveChatListener_DisposeSent(object? sender, EventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            try
            {
                if (!liveChatListener.IsUserDispose)
                {
                    Log.Log.AddLog(nameof(DanMuRec), LogClass.LogType.Info, $"{liveChatListener.TroomId}直播间弹幕连接中断，检测到直播未停止且弹幕录制设置已打开，开始重连弹幕服务器");
                    Rec(liveChatListener.mid);
                }
                else
                {
                    Log.Log.AddLog(nameof(DanMuRec), LogClass.LogType.Info, $"{liveChatListener.TroomId}请求重连，但该房间的录制已经标记不再连接，取消重连");
                }
            }
            catch (Exception)
            {
            }   
        }

        private static void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            Rooms.Rooms.RoomInfo.TryGetValue(liveChatListener.mid, out RoomInfoClass.RoomInfo roomInfo);
            if (roomInfo != null)
            {
                switch (e)
                {
                    case DanmuMessageEventArgs Danmu:
                        roomInfo.DanmuFile.Danmu.Add(new DanMuClass.DanmuInfo
                        {
                            color = Danmu.MessageColor,
                            pool = 0,
                            size = 25,
                            timestamp = Danmu.Timestamp,
                            type = Danmu.MessageType,
                            time = roomInfo.DanmuFile.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            uid = Danmu.UserId,
                            Message = Danmu.Message,
                            Nickname=Danmu.UserName,
                            LV=Danmu.GuardLV
                        });
                        break;
                    case SuperchatEventArg SuperchatEvent:
                        roomInfo.DanmuFile.SuperChat.Add(new DanMuClass.SuperChatInfo()
                        {
                            Message = SuperchatEvent.Message,
                            MessageTrans = SuperchatEvent.messageTrans,
                            Price = SuperchatEvent.Price,
                            Time = roomInfo.DanmuFile.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = SuperchatEvent.Timestamp,
                            UserId = SuperchatEvent.UserId,
                            UserName = SuperchatEvent.UserName,
                            TimeLength = SuperchatEvent.TimeLength
                        });
                        break;
                    case GuardBuyEventArgs GuardBuyEvent:
                        roomInfo.DanmuFile.GuardBuy.Add(new DanMuClass.GuardBuyInfo()
                        {
                            GuardLevel = GuardBuyEvent.GuardLevel,
                            GuradName = GuardBuyEvent.GuardName,
                            Number = GuardBuyEvent.Number,
                            Price = GuardBuyEvent.Price,
                            Time = roomInfo.DanmuFile.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = GuardBuyEvent.Timestamp,
                            UserId = GuardBuyEvent.UserId,
                            UserName = GuardBuyEvent.UserName
                        });
                        break;
                    case SendGiftEventArgs sendGiftEventArgs:
                        roomInfo.DanmuFile.Gift.Add(new DanMuClass.GiftInfo()
                        {
                            Amount = sendGiftEventArgs.Amount,
                            GiftName = sendGiftEventArgs.GiftName,
                            Price = sendGiftEventArgs.GiftPrice,
                            Time = roomInfo.DanmuFile.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = sendGiftEventArgs.Timestamp,
                            UserId = sendGiftEventArgs.UserId,
                            UserName = sendGiftEventArgs.UserName
                        });
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 保存弹幕相关信息
        /// </summary>
        /// <param name="roomInfo"></param>
        public static void SevaDanmuFile(RoomInfoClass.RoomInfo roomInfo)
        {
            if (Download.IsRecDanmu)
            {
                switch (CoreConfig.DanMuSaveType)
                {
                    case 1:
                        WebHook.SendHook(WebHook.HookType.SaveDanmuComplete, roomInfo.uid);
                        roomInfo.DownloadedFileInfo.DanMuFile = SevaDanmu(roomInfo.DanmuFile.Danmu, roomInfo.DanmuFile.Gift, roomInfo.DanmuFile.GuardBuy, roomInfo.DanmuFile.SuperChat, roomInfo.DanmuFile.FileName, roomInfo.uname, roomInfo.room_id, Tool.TimeModule.Time.Operate.DateTimeToConvertTimeStamp(roomInfo.CreationTime), roomInfo.title);
                        break;
                    case 2:
                        WebHook.SendHook(WebHook.HookType.SaveDanmuComplete, roomInfo.uid);
                        roomInfo.DownloadedFileInfo.DanMuFile = SevaDanmu(roomInfo.DanmuFile.Danmu, roomInfo.DanmuFile.FileName, roomInfo.uname, roomInfo.room_id, Tool.TimeModule.Time.Operate.DateTimeToConvertTimeStamp(roomInfo.CreationTime));
                        roomInfo.DownloadedFileInfo.GiftFile = SevaGift(roomInfo.DanmuFile.Gift, roomInfo.DanmuFile.FileName);
                        roomInfo.DownloadedFileInfo.GuardFile = SevaGuardBuy(roomInfo.DanmuFile.GuardBuy, roomInfo.DanmuFile.FileName);
                        roomInfo.DownloadedFileInfo.SCFile = SevaSuperChat(roomInfo.DanmuFile.SuperChat, roomInfo.DanmuFile.FileName);

                        break;
                   
                }
            }



            //if (Download.IsRecGift)
            //{
            //    WebHook.SendHook(WebHook.HookType.SaveGiftComplete, roomInfo.uid);
            //    roomInfo.DownloadedFileInfo.GiftFile = SevaGift(roomInfo.DanmuFile.Gift, roomInfo.DanmuFile.FileName);
            //}
            //if (Download.IsRecGuard)
            //{
            //    WebHook.SendHook(WebHook.HookType.SaveGuardComplete, roomInfo.uid);
            //    roomInfo.DownloadedFileInfo.GuardFile = SevaGuardBuy(roomInfo.DanmuFile.GuardBuy, roomInfo.DanmuFile.FileName);
            //}
            //if (Download.IsRecSC)
            //{
            //    WebHook.SendHook(WebHook.HookType.SaveSCComplete, roomInfo.uid);
            //    roomInfo.DownloadedFileInfo.SCFile = SevaSuperChat(roomInfo.DanmuFile.SuperChat, roomInfo.DanmuFile.FileName);
            //}
        }

        /// <summary>
        /// 储存新格式弹幕信息到xml文件
        /// </summary>
        private static FileInfo SevaDanmu(List<DanMuClass.DanmuInfo> danmuInfo, List<DanMuClass.GiftInfo> GiftInfo, List<DanMuClass.GuardBuyInfo> guardBuyInfos, List<DanMuClass.SuperChatInfo> superChatInfos, string FileName, string Name, int roomId, long time,string title)
        {
            string XML = Properties.Resources.LiveChatRecordInfo;
            XML = XML.Replace("<-app->",  InitDDTV_Core.Ver);
            XML = XML.Replace("<-name->", Name);
            XML = XML.Replace("<-time->", time.ToString());
            XML = XML.Replace("<-roomid->", roomId.ToString());
            XML = XML.Replace("<-title->", title);
            string d = string.Empty;
            for (int i = 0; i < danmuInfo.Count; i++)
            {
                d += Properties.Resources.LiveChat_d
                    .Replace("<-p->", $"{danmuInfo[i].time:f4},{danmuInfo[i].type},{danmuInfo[i].size},{danmuInfo[i].color},{danmuInfo[i].timestamp / 1000},{danmuInfo[i].pool},{danmuInfo[i].uid},{i}")
                    .Replace("<-user->", danmuInfo[i].Nickname)
                    .Replace("<-text->", XMLEscape(danmuInfo[i].Message))
                    + "\r";
            }

            string sc = string.Empty;
            foreach (var item in superChatInfos)
            {
                sc += Properties.Resources.LiveChat_sc
                    .Replace("<-ts->", item.Time.ToString())
                    .Replace("<-user->", item.UserName)
                    .Replace("<-uid->", item.UserId.ToString())
                    .Replace("<-price->", item.Price.ToString())
                    .Replace("<-time->", item.Message.ToString())
                    .Replace("<-time->", item.TimeLength.ToString())
                    .Replace("<-text->", XMLEscape(item.Message))
                    +"\r";
            }

            string gift = string.Empty;
            foreach (var item in GiftInfo)
            {
                gift += Properties.Resources.LiveChat_gift
                    .Replace("<-ts->", item.Time.ToString())
                    .Replace("<-user->", item.UserName)
                    .Replace("<-uid->", item.UserId.ToString())
                    .Replace("<-giftname->", item.GiftName)
                    .Replace("<-giftcount->", item.Amount.ToString())
                    + "\r";
            }

            string guard = string.Empty;
            foreach (var item in guardBuyInfos)
            {
                guard += Properties.Resources.LiveChat_guard
                    .Replace("<-ts->", item.Time.ToString())
                    .Replace("<-user->", item.UserName)
                    .Replace("<-uid->", item.UserId.ToString())
                    .Replace("<-level->", item.GuardLevel == 1 ? "总督" : item.GuardLevel == 2 ? "提督" : item.GuardLevel == 3 ? "舰长" : item.GuardLevel.ToString())
                    .Replace("<-count->", item.Number.ToString())
                    + "\r";
            }
            XML = XML.Replace("<-LiveChat->", d + sc + gift + guard);
            File.WriteAllText(FileName + ".xml", XML);
            return new FileInfo(FileName + ".xml");
        }


        /// <summary>
        /// 储存原始弹幕信息到xml文件
        /// </summary>
        /// <param name="danmuInfo"></param>
        /// <param name="FileName"></param>
        /// <param name="Name"></param>
        /// <param name="roomId"></param>
        private static FileInfo SevaDanmu(List<DanMuClass.DanmuInfo> danmuInfo, string FileName, string Name, int roomId, long time)
        {
            string XML = string.Empty;

            XML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<i>" +
            "<chatserver>chat.bilibili.com</chatserver>" +
            "<chatid>0</chatid>" +
            "<mission>0</mission>" +
            "<maxlimit>2147483647</maxlimit>" +
            "<state>0</state>" +
            $"<app>{InitDDTV_Core.Ver}</app>" +
            $"<real_name>{Name}</real_name>" +
            $"<roomid>{roomId}</roomid>" +
            $"<time>{time}</time>" +
            $"<source>k-v</source>";

            int i = 1;

            foreach (var item in danmuInfo)
            {
                XML += $"<d p=\"{item.time:f4},{item.type},{item.size},{item.color},{item.timestamp / 1000},{item.pool},{item.uid},{i}\">{XMLEscape(item.Message)}</d>\r\n";
                i++;
            }

            XML += "</i>";
            File.WriteAllText(FileName + ".xml", XML);
            return new FileInfo(FileName + ".xml");
        }
        /// <summary>
        /// 对XML特殊字符进行转义
        /// </summary>
        /// <param name="Message">待转义消息</param>
        /// <returns></returns>
        private static string XMLEscape(string Message)
        {
            return Message.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;");
                //.Replace(" ", "&nbsp;")
                //.Replace("×", "&times;")
                //.Replace("÷", "&divde;");
        }
        /// <summary>
        /// 储存原始礼物信息到文件
        /// </summary>
        /// <param name="GiftInfo"></param>
        /// <param name="FileName"></param>
        public static FileInfo SevaGift(List<DanMuClass.GiftInfo> GiftInfo, string FileName)
        {
            string Gift = "视频时间,送礼人昵称,送礼人Uid,礼物名称,礼物数量,礼物单价,时间戳";
            foreach (var item in GiftInfo)
            {
                Gift += $"\r\n{item.Time},{item.UserName},{item.UserId},{item.GiftName},{item.Amount},{item.Price},{item.Timestamp}";
            }
            File.WriteAllText(FileName + "_礼物.csv", Gift, Encoding.UTF8);
            return new FileInfo(FileName + "_礼物.csv");
        }
        /// <summary>
        /// 储存原始舰队信息到文件
        /// </summary>
        /// <param name="guardBuyInfos"></param>
        /// <param name="FileName"></param>
        public static FileInfo SevaGuardBuy(List<DanMuClass.GuardBuyInfo> guardBuyInfos, string FileName)
        {
            string Gift = "视频时间,送礼人昵称,送礼人Uid,上舰类型,上舰时间,每月价格,时间戳";
            foreach (var item in guardBuyInfos)
            {
                string Level = item.GuardLevel == 1 ? "总督" : item.GuardLevel == 2 ? "提督" : item.GuardLevel == 3 ? "舰长" : item.GuardLevel.ToString();
                Gift += $"\r\n{item.Time},{item.UserName},{item.UserId},{Level},{item.Number},{item.Price},{item.Timestamp}";
            }
            File.WriteAllText(FileName + "_舰队.csv", Gift, Encoding.UTF8);
            return new FileInfo(FileName + "_舰队.csv");
        }
        /// <summary>
        /// 储存原始SC信息到文件
        /// </summary>
        /// <param name="superChatInfos"></param>
        /// <param name="FileName"></param>
        public static FileInfo SevaSuperChat(List<DanMuClass.SuperChatInfo> superChatInfos, string FileName)
        {
            string Gift = "视频时间,送礼人昵称,送礼人Uid,SC金额,消息原文,翻译消息,时间戳";
            foreach (var item in superChatInfos)
            {
                Gift += $"\r\n{item.Time},{item.UserName},{item.UserId},{item.Price},{item.Message},{item.MessageTrans},{item.Timestamp}";
            }
            File.WriteAllText(FileName + "_SC.csv", Gift, Encoding.UTF8);
            return new FileInfo(FileName + "_SC.csv");
        }
    }
}
