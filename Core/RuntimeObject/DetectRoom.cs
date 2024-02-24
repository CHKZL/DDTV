using AngleSharp.Dom.Events;
using Core.LiveChat;
using Core.LogModule;
using Core.Network.Methods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.RoomInfo;

namespace Core.RuntimeObject
{
    public class Detect
    {
        public static DetectRoom detectRoom = new();//实例化房间监听    


        public Detect()
        {
            detectRoom.LiveStart += DetectRoom_LiveStart;//开播事件
            Log.Info(nameof(DetectRoom), $"注册开播事件");
            detectRoom.LiveEnd += DetectRoom_LiveEnd;//下播事件
            Log.Info(nameof(DetectRoom), $"注册下播事件");
            Detect.detectRoom.start();//启动房间监听
        }
        /// <summary>
        /// 开播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static async void DetectRoom_LiveStart(Object? sender, RoomCardClass e)
        {

            List<TriggerType> triggerTypes = sender as List<TriggerType>;
            if (triggerTypes == null)
            {
                triggerTypes = new List<TriggerType>();
            }
            if (e.live_status.Value != 1)
            {
                Log.Warn(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})触发录制事件，但目前该房间检测到未开播，跳过本次录制任务");
                return;
            }


            bool Initialization = true;

            if (e.IsRemind && triggerTypes.Contains(TriggerType.RegularTasks))
            {
                //Log.Info(nameof(DetectRoom_LiveStart), $"检测到通知对象：{e.RoomId}({e.Name})开播");
                //这里应该是开播广播事件
            }

            if (e.IsAutoRec || triggerTypes.Contains(TriggerType.ManuallyTriggeringTasks) || e.AppointmentRecord)
            {

                if (e.DownInfo.IsDownload)
                {
                    Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})触发录制事件，但目前该房间已有录制任务，跳过本次录制任务");
                    return;
                }
                e.DownInfo.IsDownload = true;
                Core.LiveChat.LiveChatListener liveChatListener = new Core.LiveChat.LiveChatListener(e.RoomId);
                do
                {
                    if (Initialization)
                    {
                        Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})触发开播事件,开始录制【触发类型:" + (triggerTypes.Contains(TriggerType.ManuallyTriggeringTasks) ? "手动触发" : "自动触发") + "】");
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
                        Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})弹幕录制进程被服务器终止，但检测到房间还在开播状态，尝试重连...");
                    }

                    var result = await Download.File.DlwnloadHls_avc_mp4(e);
                    if (e.IsRecDanmu)
                    {

                        Danmu.SevaDanmu(liveChatListener, ref e);
                    }
                    if (result.isSuccess && Core.Config.Core._AutomaticRepair)
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
                //while (false) ;
                while (RoomInfo.GetLiveStatus(e.RoomId) || !e.DownInfo.Unmark);
                DownloadCompletedReset(ref e);
                Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})录制结束" + (e.DownInfo.Unmark ? "【原因：用户取消】" : ""));
                e.DownInfo.Unmark = false;
                e.DownInfo.IsDownload = false;

                if (e.IsRecDanmu)
                {
                    if (!liveChatListener._Cancel)
                    {
                        liveChatListener.Cancel();
                    }
                }
                if (liveChatListener != null)
                {
                    liveChatListener.DanmuMessage = null;
                    try
                    {
                        liveChatListener.Dispose();
                    }
                    catch (Exception)
                    { }
                }


            }

        }

        /// <summary>
        /// 下载完成重置房间卡状态
        /// </summary>
        /// <param name="roomCard"></param>
        private static void DownloadCompletedReset(ref RoomCardClass roomCard)
        {
            Log.Info(nameof(DownloadCompletedReset), $"[{roomCard.Name}({roomCard.RoomId})]进行录制完成处理");

            roomCard.DownInfo.RealTimeDownloadSpe = 0;
            roomCard.DownInfo.DownloadSize = 0;
            roomCard.DownInfo.Status = roomCard.DownInfo.Unmark ? RoomCardClass.DownloadStatus.Cancel : RoomCardClass.DownloadStatus.DownloadComplete;
            roomCard.DownInfo.EndTime = DateTime.Now;
            _Room.SetRoomCardByUid(roomCard.UID, roomCard);
        }

        private static void LiveChatListener_DisposeSent(object? sender, EventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            //Danmu.SevaDanmu(liveChatListener);
            if (!liveChatListener._Cancel)
            {
                if (liveChatListener._disposed)
                {
                    liveChatListener = new Core.LiveChat.LiveChatListener(liveChatListener.RoomId);
                    liveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                    liveChatListener.DisposeSent += LiveChatListener_DisposeSent;
                }
                Log.Info(nameof(LiveChatListener_DisposeSent), $"{liveChatListener.RoomId}({liveChatListener.Name})弹幕断开连接，但直播未结束，触发重连");
                liveChatListener.Connect();
            }
            else
            {
                Log.Info(nameof(LiveChatListener_DisposeSent), $"{liveChatListener.RoomId}({liveChatListener.Name})弹幕断开连接");
                liveChatListener.Cancel();
                liveChatListener = null;
            }
        }


        /// <summary>
        /// 下播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void DetectRoom_LiveEnd(object? sender, RoomCardClass e)
        {
            if (e.IsRemind)
            {
                Log.Info(nameof(DetectRoom_LiveEnd), $"{e.RoomId}({e.Name})下播");
            }

        }


        private static void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
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
                        if (Init.IsDevDebug)
                        {
                            //Log.Info(nameof(LiveChatListener_MessageReceived),$"收到弹幕:{Danmu.UserName}[{Danmu.UserId}]:{Danmu.Message}");
                        }
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
                        if (Init.IsDevDebug)
                        {
                            //Log.Info(nameof(LiveChatListener_MessageReceived),$"收到SC:{SuperchatEvent.UserName}[{SuperchatEvent.UserId}]:{SuperchatEvent.Message}");
                        }
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
                        if (Init.IsDevDebug)
                        {
                           // Log.Info(nameof(LiveChatListener_MessageReceived),$"收到大航海:{GuardBuyEvent.UserName}[{GuardBuyEvent.UserId}]:{GuardBuyEvent.GuardName}");
                        }
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
                        if (Init.IsDevDebug)
                        {
                           // Log.Info(nameof(LiveChatListener_MessageReceived),$"收到礼物:{sendGiftEventArgs.UserName}[{sendGiftEventArgs.UserId}]:{sendGiftEventArgs.GiftName} x {sendGiftEventArgs.Amount}个");
                        }
                        break;
                    }
                default:
                    break;
            }

        }
        public enum TriggerType
        {
            /// <summary>
            /// 普通任务
            /// </summary>
            RegularTasks,
            /// <summary>
            /// 切换状态触发任务
            /// </summary>
            SwitchingStatesTriggersTasks,
            /// <summary>
            /// 手动触发任务
            /// </summary>
            ManuallyTriggeringTasks,
        }
    }

    public class DetectRoom
    {
        #region Private Properties
        private bool _state = false;
        private bool Initialization = false;
        #endregion

        #region public Properties
        public event EventHandler<RoomCardClass> LiveStart;
        public event EventHandler<RoomCardClass> LiveEnd;
        public bool State { get { return _state; } }
        #endregion

        public DetectRoom()
        {

        }

        #region Public Method
        public void start()
        {
            if (!Initialization)
            {
                Log.Info(nameof(DetectRoom), $"房间状态监听已启动");
                RoomLoopDetection();
                Initialization = true;
            }
            _state = true;
        }

        public void stop()
        {
            _state = false;
        }

        /// <summary>
        /// 手动触发一个直播间的录制
        /// </summary>
        /// <param name="UID">触发的UID</param>
        public void ManuallyTriggerRecord(long UID = 0)
        {
            RoomCardClass Card = new();
            if (UID != 0 && _Room.GetCardForUID(UID, ref Card) && LiveStart != null)
            {
                if (RoomInfo.GetLiveStatus(Card.RoomId))
                {
                    LiveStart.Invoke(new List<Detect.TriggerType>() { Detect.TriggerType.ManuallyTriggeringTasks }, Card);
                }
                return;
            }
        }
        #endregion

        #region Private Method

        /// <summary>
        /// 房间轮询检查
        /// </summary>
        /// <returns></returns>
        private async Task RoomLoopDetection()
        {
            while (true)
            {
                try
                {
                    while (_state)
                    {
                        await BatchUpdateRoomStatusForLiveStream();
                        var List = _Room.GetCardListClone();
                        foreach (var item in List)
                        {
                            RoomCardClass Card = List.FirstOrDefault(x => x.Value.UID == item.Value.UID).Value;
                            if (Card == null)
                            {
                                continue;
                            }
                            if (Card.live_status_start_event && Card.live_status.Value == 1)
                            {
                                Card.live_status_start_event = false;
                                if (LiveStart != null)
                                    LiveStart.Invoke(new List<Detect.TriggerType>() { Detect.TriggerType.RegularTasks }, Card);
                            }
                            if (Card.live_status_end_event && Card.live_status.Value != 1)
                            {
                                Card.live_status_end_event = false;
                                if (LiveEnd != null)
                                    LiveEnd.Invoke(new List<Detect.TriggerType>() { Detect.TriggerType.RegularTasks }, Card);
                            }
                        }
                        await Task.Delay(Config.Core._DetectIntervalTime);
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(2000);
                }
                await Task.Delay(1000);
            }
        }
        #endregion

        #region Public Class

        #endregion


    }
}
