using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_GUI.DDTV_Window;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static int RecCompleteCount = 0;
        private static string SavePathSize = "";
        private static string TmpPathSize = "";
        public static void Update(MainWindow mainWindow)
        {
            bool IsUpdate = false;
            int _RecCount = 0;
            int _LiveBroadcastingCount = 0;
            int _RecCompleteCount = 0;
            int _MonitoringCount = Rooms.RoomInfo.Count;
            string _SavePathName = Download.DefaultPath.Split(':').Length>1?Download.DefaultPath.Split(':')[0]:Path.GetFullPath(Download.DefaultPath).Split(':')[0];
            string _SavePathSize = DDTV_Core.SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(DDTV_Core.Tool.FileOperation.GetHardDiskSpace(_SavePathName,2))+"/"+ DDTV_Core.SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(DDTV_Core.Tool.FileOperation.GetHardDiskSpace(_SavePathName, 1));
            string _TmpPathName = Download.DefaultPath.Split(':').Length > 1 ? Download.DefaultPath.Split(':')[0] : Path.GetFullPath(Download.DefaultPath).Split(':')[0];
            string _TmpPathSize = DDTV_Core.SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(DDTV_Core.Tool.FileOperation.GetHardDiskSpace(_TmpPathName, 2)) + "/" + DDTV_Core.SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(DDTV_Core.Tool.FileOperation.GetHardDiskSpace(_TmpPathName, 1));

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
                if(A1.Value.DownloadedLog.Count>0)
                {
                    _RecCompleteCount++;
                }
            }
            if (RecCount != _RecCount || LiveBroadcastingCount != _LiveBroadcastingCount || MonitoringCount != _MonitoringCount || RecCompleteCount != _RecCompleteCount || SavePathSize != _SavePathSize || TmpPathSize != _TmpPathSize)
            {
                RecCount = _RecCount;
                LiveBroadcastingCount = _LiveBroadcastingCount;
                MonitoringCount = _MonitoringCount;
                RecCompleteCount = _RecCompleteCount;
                SavePathSize = _SavePathSize;
                TmpPathSize = _TmpPathSize;
                IsUpdate = true;
            }
            if (IsUpdate)
            {
                mainWindow.MonitoringCount.Dispatcher.Invoke(() => mainWindow.MonitoringCount.Text= _MonitoringCount.ToString());
                mainWindow.LiveBroadcastingCount.Dispatcher.Invoke(() => mainWindow.LiveBroadcastingCount.Text = _LiveBroadcastingCount.ToString());
                mainWindow.RecCount.Dispatcher.Invoke(() => mainWindow.RecCount.Text = _RecCount.ToString());
                mainWindow.RecCompleteCount.Dispatcher.Invoke(() => mainWindow.RecCompleteCount.Text = _RecCompleteCount.ToString());
                mainWindow.SavePathSize.Dispatcher.Invoke(() => mainWindow.SavePathSize.Text = _SavePathSize);
                mainWindow.TmpPathSize.Dispatcher.Invoke(() => mainWindow.TmpPathSize.Text = _TmpPathSize);
            }
            
        }
    }
}
