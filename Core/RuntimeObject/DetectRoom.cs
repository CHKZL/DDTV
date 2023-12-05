using AngleSharp.Dom.Events;
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
            Core.RuntimeObject.RoomList.BatchUpdateRoomStatusForLiveStream([85997303, 1752208642, 399815233]);
            _state = true;
        }

        public void stop()
        {
            _state= false;
        }
        #endregion

        #region Private Method
        private async Task RoomLoopDetection()
        {
            while (_state)
            {
                await Task.Delay(Config.Core._DetectIntervalTime);
                List<RoomCard> test = roomInfos.Select(item => item.Clone()).ToList();
                await BatchUpdateRoomStatusForLiveStream(new List<long>() { 85997303, 1752208642, 399815233 });
                foreach (var item in roomInfos)
                {
                    RoomCard? oldCard = test.FirstOrDefault(x => x.UID == item.UID);
                    RoomCard? newCard = roomInfos.FirstOrDefault(x => x.UID == item.UID);
                    if (oldCard != null && newCard != null && oldCard.live_status.Value != newCard.live_status.Value)
                    {
                        if (newCard.live_status.Value == 1)
                        {
                            LiveStart.Invoke(null, newCard);
                        }
                        else
                        {
                            LiveEnd.Invoke(null, newCard);
                        }
                    }
                }
            }
        }
        #endregion

        #region Public Class

        #endregion


    }
}
