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
    internal class HomePage
    {
        private static int RecCount = 0;
        private static int LiveBroadcastingCount = 0;
        private static int MonitoringCount = 0;
        public static void Update(MainWindow mainWindow)
        {
            bool IsUpdate = false;
            int _RecCount = 0;
            int _LiveBroadcastingCount = 0;
            int _MonitoringCount = Rooms.RoomInfo.Count;
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadingList.Count > 0)
                {
                    _RecCount++;
                }
                if(A1.Value.live_status==1)
                {
                    _LiveBroadcastingCount++;
                }
            }
            if(RecCount!= _RecCount|| LiveBroadcastingCount!= _LiveBroadcastingCount|| MonitoringCount!= _MonitoringCount )
            {
                RecCount = _RecCount;
                LiveBroadcastingCount = _LiveBroadcastingCount;
                MonitoringCount = _MonitoringCount ;
                IsUpdate=true;
            }
            if (IsUpdate)
            {
                mainWindow.MonitoringCount.Dispatcher.Invoke(() => mainWindow.MonitoringCount.Text= _MonitoringCount.ToString());
                mainWindow.LiveBroadcastingCount.Dispatcher.Invoke(() => mainWindow.LiveBroadcastingCount.Text = _LiveBroadcastingCount.ToString());
                mainWindow.RecCount.Dispatcher.Invoke(() => mainWindow.RecCount.Text = _RecCount.ToString());
            }
            
        }
    }
}
