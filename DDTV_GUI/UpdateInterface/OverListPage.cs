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
    internal class OverListPage
    {
        private static bool first = true;
        public static List<BindingData.OverList> Update(List<BindingData.OverList> overLists, MainWindow mainWindow)
        {
            bool IsUpdate = false;
            List<BindingData.OverList> _ = new();
            foreach (var A1 in Rooms.RoomInfo)
            {
                foreach (var item in A1.Value.DownloadedLog)
                {
                    BindingData.OverList rec = new(item.Name,item.RoomId, NetClass.ConversionSize(item.DownloadCount),item.StartTime.ToString("MM-dd HH:mm:ss"),item.EndTime.ToString("MM-dd HH:mm:ss"),item.Title,item.Uid,item.FilePath,item.FileName);
                    _.Add(rec);
                }
            }
            if (_.Count == 0)
            {
                _.Add(new BindingData.OverList("", "0", "", "", "", "无已完成任务",0,"",""));
            }
            if (overLists.Count != _.Count)
            {
                IsUpdate = true;
            }
            else
            {
                for (int i = 0 ; i < overLists.Count ; i++)
                {
                    if (overLists[i].Name != _[i].Name
                    || overLists[i].RoomId != _[i].RoomId
                    || overLists[i].DownSzie != _[i].DownSzie
                    || overLists[i].StartTime != _[i].StartTime
                    || overLists[i].Title != _[i].Title)
                    {
                        IsUpdate = true;
                    }
                }
            }
            if ((first || !Main.ContextMenuState.OverListPage) && IsUpdate)
            {
                overLists = _;
                mainWindow.OverList.Dispatcher.Invoke(() => mainWindow.OverList.ItemsSource = overLists);
            }
            first = false;
            return overLists;
        }
    }
}
