using AngleSharp.Dom.Events;
using Core.LogModule;
using Core.Network.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.RoomInfo;

namespace Core.RuntimeObject
{
    public class DetectRoom
    {
        #region Private Properties
        private bool _state = false;
        private bool Initialization = false;
        #endregion

        #region public Properties
        public event EventHandler<RoomCardClass> LiveStart;
        public event EventHandler<RoomCardClass> LiveEnd;
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
        #endregion

        #region Private Method

        /// <summary>
        /// 房间轮询检查
        /// </summary>
        /// <returns></returns>
        private async Task RoomLoopDetection()
        {
            bool FirstTime = true;
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
                                LiveStart.Invoke(FirstTime ? true : false, Card);
                            }
                            if (Card.live_status_end_event && Card.live_status.Value != 1)
                            {
                                Card.live_status_end_event = false;
                                LiveEnd.Invoke(FirstTime ? true : false, Card);
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
