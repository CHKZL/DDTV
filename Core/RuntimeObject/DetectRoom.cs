using AngleSharp.Dom.Events;
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
        /// 手动触发一个直播间的录制(UID和房间号二选一)
        /// </summary>
        /// <param name="UID">触发的UID</param>
        /// <param name="RoomId">触发的房间号</param>
        public void ManuallyTriggerRecord(long UID = 0, long RoomId = 0)
        {
            if (UID != 0)
            {
                RoomCardClass Card = new();
                if (_Room.GetCardForUID(UID, ref Card))
                {
                    if (LiveStart != null)
                    {
                        LiveStart.Invoke(true, Card);
                        return;
                    }
                }
            }
            if (RoomId != 0)
            {
                RoomCardClass Card = new();
                if (_Room.GetCardFoRoomId(RoomId, ref Card))
                {
                    if (LiveStart != null)
                    {
                        LiveStart.Invoke(true, Card);
                        return;
                    }
                }
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
                                    LiveStart.Invoke(true, Card);
                            }
                            if (Card.live_status_end_event && Card.live_status.Value != 1)
                            {
                                Card.live_status_end_event = false;
                                 if (LiveEnd != null)
                                    LiveEnd.Invoke(true, Card);
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
