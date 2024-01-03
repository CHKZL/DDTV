using Core.LiveChat;
using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject;
using Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;

namespace CLI
{
    internal class Record
    {
        /// <summary>
        /// 开播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static async void DetectRoom_LiveStart(Object? sender, RoomList.RoomCard e)
        {
            //return;
            bool Initialization = true;
            if (e.IsRemind)
            {
                Log.Info(nameof(DetectRoom_LiveStart), $"检测到通知对象：{e.RoomId}({e.Name})开播");
            }
          
            if (e.IsAutoRec)
            {
                Core.LiveChat.LiveChatListener liveChatListener = new Core.LiveChat.LiveChatListener(e.RoomId);
                do
                {
                    if (Initialization)
                    {
                        Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})触发开播事件,开始录制");
                        Initialization = false;
                        if (e.IsRecDanmu)
                        {
                            liveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                            liveChatListener.DisposeSent += LiveChatListener_DisposeSent;
                            liveChatListener.Connect();
                        }
                    }
                    else
                    {
                        Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})录制进程退出，但检测到房间还在开播状态，尝试重连...");
                    }
                    var result = await Download.File.DlwnloadHls_avc_mp4(e);
                    if (e.IsRecDanmu)
                    {
                        Danmu.SevaDanmu(liveChatListener);
                    }
                    if (result.isSuccess)
                    {
                        Core.Tools.Transcode transcode = new Core.Tools.Transcode();
                        try
                        {
                            transcode.TranscodeAsync(result.FileName, result.FileName.Replace("_original.mp4", "_fix.mp4"), e.RoomId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})完成录制任务后修复时出现意外错误，文件:{result.FileName}");
                        } 
                    }
                }
                while (RoomList.GetLiveStatus(e.RoomId));
                if (e.IsRecDanmu)
                {
                    if(liveChatListener.State)
                    {
                        liveChatListener.Dispose();
                    }
                }
                Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})录制结束");
            }
        }

        private static void LiveChatListener_DisposeSent(object? sender, EventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            Danmu.SevaDanmu(liveChatListener);
        }


        /// <summary>
        /// 下播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void DetectRoom_LiveEnd(object? sender, RoomList.RoomCard e)
        {
            if (e.IsRemind)
            {
                Log.Info(nameof(DetectRoom_LiveEnd), $"{e.RoomId}({e.Name})下播");
            }

        }


        private static void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
            //switch (e)
            //{
            //    case DanmuMessageEventArgs Danmu:
            //        {
            //            Log.Info(nameof(LiveChatListener_MessageReceived), $"[弹幕]{DateTime.Now.ToString("HH:mm:ss")} {Danmu.UserName}：{Danmu.Message}");
            //        }
            //        break;
            //    case SuperchatEventArg SuperchatEvent:
            //        {
            //            Log.Info(nameof(LiveChatListener_MessageReceived), $"[超级留言](金额{SuperchatEvent.Price}){DateTime.Now.ToString("HH:mm:ss")} {SuperchatEvent.UserName}：{SuperchatEvent.Message}");
            //        }
            //        break;
            //    case GuardBuyEventArgs GuardBuyEvent:
            //        {
            //            string Lv = GuardBuyEvent.GuardLevel == 1 ? "总督" : GuardBuyEvent.GuardLevel == 2 ? "提督" : "舰长";
            //            Log.Info(nameof(LiveChatListener_MessageReceived), $"[上舰]{DateTime.Now.ToString("HH:mm:ss")} {GuardBuyEvent.UserName}：{GuardBuyEvent.Number}个月的{Lv}");
            //        }
            //        break;
            //    case SendGiftEventArgs sendGiftEventArgs:
            //        {
            //            Log.Info(nameof(LiveChatListener_MessageReceived), $"[礼物]{DateTime.Now.ToString("HH:mm:ss")} {sendGiftEventArgs.UserName}：送了{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}");
            //        }
            //        break;
            //    case WarningEventArg warningEventArg:
            //        {
            //            Log.Info(nameof(LiveChatListener_MessageReceived), $"[管理员警告]{DateTime.Now.ToString("HH:mm:ss")} 管理员警告！警告内容:{warningEventArg.msg}");
            //        }
            //        break;
            //    case CutOffEventArg cutOffEventArg:
            //        {
            //            Log.Info(nameof(LiveChatListener_MessageReceived), $"[直播被切断]{DateTime.Now.ToString("HH:mm:ss")} 直播被切断！系统消息:{cutOffEventArg.msg}");
            //        }
            //        break;
            //    default:
            //        break;
            //}

            LiveChatListener liveChatListener = (LiveChatListener)sender;
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {

                        liveChatListener.DanmuMessage.Danmu.Add(new Danmu.DanmuInfo
                        {
                            color = Danmu.MessageColor,
                            pool = 0,
                            size = 25,
                            timestamp = Danmu.Timestamp,
                            type = Danmu.MessageType,
                            time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            uid = Danmu.UserId,
                            Message = Danmu.Message,
                            Nickname = Danmu.UserName,
                            LV = Danmu.GuardLV
                        });
                        break;
                    }
                case SuperchatEventArg SuperchatEvent:
                    {

                        liveChatListener.DanmuMessage.SuperChat.Add(new Danmu.SuperChatInfo()
                        {
                            Message = SuperchatEvent.Message,
                            MessageTrans = SuperchatEvent.messageTrans,
                            Price = SuperchatEvent.Price,
                            Time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = SuperchatEvent.Timestamp,
                            UserId = SuperchatEvent.UserId,
                            UserName = SuperchatEvent.UserName,
                            TimeLength = SuperchatEvent.TimeLength
                        });
                        break;
                    }
                case GuardBuyEventArgs GuardBuyEvent:
                    {

                        liveChatListener.DanmuMessage.GuardBuy.Add(new Danmu.GuardBuyInfo()
                        {
                            GuardLevel = GuardBuyEvent.GuardLevel,
                            GuradName = GuardBuyEvent.GuardName,
                            Number = GuardBuyEvent.Number,
                            Price = GuardBuyEvent.Price,
                            Time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = GuardBuyEvent.Timestamp,
                            UserId = GuardBuyEvent.UserId,
                            UserName = GuardBuyEvent.UserName
                        });

                        break;
                    }
                case SendGiftEventArgs sendGiftEventArgs:
                    {
                        liveChatListener.DanmuMessage.Gift.Add(new Danmu.GiftInfo()
                        {
                            Amount = sendGiftEventArgs.Amount,
                            GiftName = sendGiftEventArgs.GiftName,
                            Price = sendGiftEventArgs.GiftPrice,
                            Time = liveChatListener.TimeStopwatch.ElapsedMilliseconds / 1000.00,
                            Timestamp = sendGiftEventArgs.Timestamp,
                            UserId = sendGiftEventArgs.UserId,
                            UserName = sendGiftEventArgs.UserName
                        });
                        break;
                    }
                default:
                    break;
            }

        }

    }
}
