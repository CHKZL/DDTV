using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_GUI.DDTV_Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_GUI.UpdateInterface
{
    internal class RecListPage
    {
        private static bool first = true;
        public static List<BindingData.RecList> Update(List<BindingData.RecList> recList, MainWindow mainWindow)
        {
            bool IsUpdate = false;
            List<BindingData.RecList> _ = new();
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadingList.Count > 0)
                {
                    ulong FileSize = 0;
                    DateTime starttime = DateTime.MaxValue;
                    string FilePath = "";
                    string Spe = "";
                    foreach (var item in A1.Value.DownloadingList)
                    {
                        FilePath = item.FilePath;
                        if (item.StartTime < starttime)
                        {
                            starttime = item.StartTime;
                        }
                        FileSize += (ulong)item.TotalDownloadCount;
                        Spe = NetClass.ConversionSize(item.DownloadSpe, NetClass.ConversionSizeType.BitRate);
                    }
                    BindingData.RecList rec = new(A1.Value.uname, A1.Value.room_id, NetClass.ConversionSize(FileSize), starttime.ToString("MM-dd HH:mm:ss"), A1.Value.title,A1.Value.uid, FilePath, Spe);
                    _.Add(rec);
                }
            }
            if (_.Count == 0)
            {
                _.Add(new BindingData.RecList("", 0, "", "", "当前无下载任务",0,"","0bps"));
            }
            if (recList.Count != _.Count)
            {
                IsUpdate = true;
            }
            else
            {
                for (int i = 0 ; i < recList.Count ; i++)
                {
                    if (recList[i].Name != _[i].Name
                    || recList[i].RoomId != _[i].RoomId
                    || recList[i].DownSzie != _[i].DownSzie
                    || recList[i].StartTime != _[i].StartTime
                    || recList[i].Title != _[i].Title
                    || recList[i].DownloadSpe != _[i].DownloadSpe)
                    {
                        IsUpdate = true;
                    }
                }
            }
            if ((first || !Main.ContextMenuState.RecListPage) && IsUpdate)
            {
                recList = _;
                mainWindow.RecList.Dispatcher.Invoke(() => mainWindow.RecList.ItemsSource = recList);
            }
            first = false;
            return recList;
        }
    }
}
