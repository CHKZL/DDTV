using AngleSharp.Dom.Events;
using Core.LogModule;
using Core.Network.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.RoomList;

namespace Core.RuntimeObject
{
    public class DetectRoom
    {
        #region Private Properties
        private bool _state = false;
        private bool Initialization = false;
        #endregion

        #region public Properties
        public event EventHandler<RoomList.RoomCard> LiveStart;
        public event EventHandler<RoomList.RoomCard> LiveEnd;
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
            while (true)
            {
                try
                {
                    while (_state)
                    {
                        var oldList = _Room.GetCardListDeepClone();
                        await BatchUpdateRoomStatusForLiveStream();
                        var NewList = _Room.GetCardListDeepClone();
                        foreach (var item in oldList)
                        {
                            RoomCard oldCard = oldList.FirstOrDefault(x => x.Value.UID == item.Value.UID).Value;
                            RoomCard newCard = NewList.FirstOrDefault(x => x.Value.UID == item.Value.UID).Value;
                            if (oldCard != null && newCard != null && oldCard.live_status.Value != newCard.live_status.Value)
                            {
                                if (newCard.live_status.Value == 1)
                                {
                                    LiveStart.Invoke(null,newCard);
                                }
                                else if (oldCard.live_status.Value != -1)
                                {
                                    LiveEnd.Invoke(null, newCard);
                                }
                            }
                            oldCard = null;
                            newCard = null;
                        }
                        oldList = null;
                        NewList = null;
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
