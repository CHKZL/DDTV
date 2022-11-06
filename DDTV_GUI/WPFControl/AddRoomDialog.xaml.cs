using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_GUI.DDTV_Window;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDTV_GUI.WPFControl
{
    /// <summary>
    /// AddRoomDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddRoomDialog
    {
        private bool IsTMP;
        public AddRoomDialog(bool isTmp = false)
        {
            InitializeComponent();
            this.IsTMP=isTmp;
            if(IsTMP)
            {
                Title.Text = "打开临时播放窗口";
                Reminder.Text = "选择打开的数据类型：";
                RecCheck.Visibility= Visibility.Collapsed;
                DanMuCheck.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool UIDCK = UIDRadio.IsChecked.Value;
            bool RoomIdCK = RoomIdRadio.IsChecked.Value;
            string InputValue = UIDInputBox.Text;
            List<long> Uids=new List<long>();
            List<long> ErrorUids = new List<long>();
            if (string.IsNullOrEmpty(InputValue))
            {
                Growl.Warning($"输入内容不能为空");
                return;
            }

            if(RoomIdCK)
            {
                if (long.TryParse(InputValue, out long RoomId))
                {
                    if(RoomId<1)
                    {
                        Growl.WarningGlobal($"房间号不能为负数！");
                        return;
                    }
                    var Info = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.get_info(0, RoomId, false);
                    Uids.Add(Info.uid);     
                }
                else
                {
                    Growl.WarningGlobal($"该房间号不符合规范！");
                    return;
                }
            }
            else if (UIDCK)
            {
                string[] T = InputValue.Split(" ");
                foreach (var item in T)
                {
                    if (long.TryParse(item, out long UID))
                    {
                        if (UID > 0)
                        {
                            Uids.Add(UID);
                        }
                        else
                        {
                            ErrorUids.Add(UID);
                        }
                    }
                }
            }
            List<RoomInfoClass.RoomInfo> roomInfo = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.get_status_info_by_uids(Uids, true);

            if (IsTMP)
            {
                if (!Rooms.RoomInfo.ContainsKey((roomInfo[0].uid)))
                {
                    Rooms.RoomInfo.Add(roomInfo[0].uid, new RoomInfoClass.RoomInfo()
                    {
                        uid = roomInfo[0].uid,
                        IsTemporaryPlay = true,
                        IsAutoRec = false,
                        IsRecDanmu = false,
                        IsRemind = false,
                    });
                }
                int.TryParse(Rooms.GetValue(roomInfo[0].uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.room_id), out int Rid);
                string Name = Rooms.GetValue(roomInfo[0].uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.uname);
                if (IsTMP)
                {
                    DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.AddRoom(roomInfo[0].uid, "tmp", false, false, false, true);
                    UIDInputBox.Clear();
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        PlayWindow playWindow = new PlayWindow(roomInfo[0].uid, Name);

                        MainWindow.playWindowsList.Add(playWindow);
                        playWindow.Closed += MainWindow.PlayWindow_Closed;
                        playWindow.Show();
                    }));
                }

            }
            else
            {
                foreach (var item in roomInfo)
                {
                    var tmp = new DDTV_Core.SystemAssembly.ConfigModule.RoomConfigClass.RoomCard()
                    {
                        UID = item.uid,
                        IsAutoRec = (bool)RecCheck.IsChecked,
                        Description = "",
                        IsRecDanmu = (bool)DanMuCheck.IsChecked,
                        IsRemind = false,
                        IsTemporaryPlay = false,
                        Like = false,
                        RoomId = item.room_id,
                        Shell = "",
                        name = item.uname,
                    };
                    DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.ReviseRoom(tmp, true, 0);
                    if (!IsTMP)
                        Growl.SuccessGlobal($"添加成功");
                    UIDInputBox.Clear();
                    if (tmp.IsAutoRec && Rooms.GetValue(tmp.UID, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
                    {
                        Download.AddDownloadTaskd(tmp.UID, true);
                        Growl.SuccessGlobal($"添加的房间{tmp.name}({tmp.RoomId})正在直播，开始录制");
                    }
                }
            }
        }

        private void RecCheck_Click(object sender, RoutedEventArgs e)
        {
            if (RecCheck.IsChecked == false)
            {
                DanMuCheck.IsChecked = false;
                DanMuCheck.IsEnabled = false;
            }
            else
            {
                DanMuCheck.IsEnabled = true;
            }
        }
    }
}
