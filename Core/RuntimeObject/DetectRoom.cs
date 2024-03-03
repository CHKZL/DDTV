using AngleSharp.Dom.Events;
using Core.LiveChat;
using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject.Download;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.Download.HLS;
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
        /// <param name="roomCard"></param>
        internal static async void DetectRoom_LiveStart(Object? sender, RoomCardClass roomCard)
        {
            List<TriggerType> triggerTypes = sender as List<TriggerType> ?? new List<TriggerType>();

            if (roomCard.live_status.Value != 1)
            {
                Log.Warn(nameof(DetectRoom_LiveStart), $"{roomCard.RoomId}({roomCard.Name})触发录制事件，但目前该房间检测到未开播，跳过本次录制任务");
                return;
            }

            bool isFirstTime = true;

            if (roomCard.IsRemind && triggerTypes.Contains(TriggerType.RegularTasks))
            {
                // 这里应该是开播广播事件
            }

            if (roomCard.IsAutoRec || triggerTypes.Contains(TriggerType.ManuallyTriggeringTasks) || roomCard.AppointmentRecord)
            {
                if (roomCard.DownInfo.IsDownload)
                {
                    Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})触发录制事件，但目前该房间已有录制任务，跳过本次录制任务");
                    return;
                }

                roomCard.DownInfo.IsDownload = true;

                Core.LiveChat.LiveChatListener liveChatListener = new Core.LiveChat.LiveChatListener(roomCard.RoomId);
                try
                {
                    do
                    {
                        //核心下载函数
                        await Basics.HandleRecordingAsync(roomCard, triggerTypes, liveChatListener, isFirstTime);
                        isFirstTime = false;
                    }
                    while (RoomInfo.GetLiveStatus(roomCard.RoomId) && !roomCard.DownInfo.Unmark);

                    Basics.DownloadCompletedReset(ref roomCard);
                    Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})录制结束" + (roomCard.DownInfo.Unmark ? "【原因：用户取消】" : ""));
                    roomCard.DownInfo.Unmark = false;
                    roomCard.DownInfo.IsDownload = false;
                }
                finally
                {
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
