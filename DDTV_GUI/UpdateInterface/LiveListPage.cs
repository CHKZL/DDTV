using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_GUI.DDTV_Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_GUI.UpdateInterface
{
    internal class LiveListPage
    {
        private static bool first = true;
        public static List<BindingData.LiveList> Update(List<BindingData.LiveList> liveList, MainWindow mainWindow)
        {
            bool IsUpdate = false;
            List<BindingData.LiveList> _ = new();
            Dictionary<long, RoomInfoClass.RoomInfo> IsLive = new Dictionary<long, RoomInfoClass.RoomInfo>();
            Dictionary<long, RoomInfoClass.RoomInfo> NotLive = new Dictionary<long, RoomInfoClass.RoomInfo>();
            foreach (var item in Rooms.RoomInfo)
            {
                if(item.Value.live_status==1)
                {
                    IsLive.Add(item.Key, item.Value);
                }
                else
                {
                    NotLive.Add(item.Key, item.Value);
                }
            }
            foreach (var item in NotLive)
            {
                IsLive.Add(item.Key, item.Value);
            }
            Rooms.RoomInfo = IsLive;
            foreach (var item in Rooms.RoomInfo)
            {
                BindingData.LiveList live = new(item.Value.uname, item.Value.live_status == 1 ? "直播中" : "未直播", item.Value.IsRemind ? "√" : "×", item.Value.IsAutoRec ? "√" : "×", item.Value.room_id, item.Value.uid, item.Value.live_status);
                _.Add(live);
            }
            if (_.Count == 0)
            {
                _.Add(new BindingData.LiveList("无房间信息", "", "", "", 0, 0,0));
                IsUpdate = true;
            }
            if (liveList.Count != _.Count)
            {
                IsUpdate = true;
            }
            else
            {
                for (int i = 0 ; i < liveList.Count ; i++)
                {
                    if (liveList[i].Name != _[i].Name
                    || liveList[i].State != _[i].State
                    || liveList[i].IsRemind != _[i].IsRemind
                    || liveList[i].IsRec != _[i].IsRec
                    || liveList[i].RoomId != _[i].RoomId)
                    {
                        IsUpdate = true;
                    }
                }
            }
            if ((first || !Main.ContextMenuState.LiveListPage) && IsUpdate)
            {
                liveList = _;
                mainWindow.LiveList.Dispatcher.Invoke(() => mainWindow.LiveList.ItemsSource = liveList);
            }
            first = false;
            return liveList;
        }
    }
}
