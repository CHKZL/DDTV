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
            string _UID = UIDInputBox.Text;
           
            if (string.IsNullOrEmpty(_UID))
            {
                Growl.Warning($"Uid/RoomID不能为空");
                return;
            }
            if (long.TryParse(_UID, out long UID))
            {
                if (RoomIdCK)
                {
                    var roomInfo = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.get_info(0, UID, false);
                    if(roomInfo != null)
                    {
                        UID= roomInfo.uid;
                    }
                    else
                    {
                        Growl.WarningGlobal($"该房间号或UID不存在！");
                        return;
                    }
                }

                if (UID <= 0)
                {
                    Growl.WarningGlobal($"房间号或UID不能为负数！");
                    return;
                }
                
                if(IsTMP && !Rooms.RoomInfo.ContainsKey(UID))
                {
                    Rooms.RoomInfo.Add(UID, new RoomInfoClass.RoomInfo()
                    {
                        uid = UID,
                        IsTemporaryPlay=true,
                        IsAutoRec=false,
                        IsRecDanmu=false,
                        IsRemind=false,
                    });
                }
                if (int.TryParse(Rooms.GetValue(UID, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.room_id), out int RoomId))
                {
                    string Name = Rooms.GetValue(UID, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.uname);
                    if (IsTMP)
                    {
                        DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.AddRoom(UID,"tmp",false,false,false,true);
                        UIDInputBox.Clear();
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            PlayWindow playWindow = new PlayWindow(UID, Name);

                            MainWindow.playWindowsList.Add(playWindow);
                            playWindow.Closed += MainWindow.PlayWindow_Closed;
                            playWindow.Show();
                        }));
                    }
                    else
                    {
                        var tmp = new DDTV_Core.SystemAssembly.ConfigModule.RoomConfigClass.RoomCard()
                        {
                            UID = UID,
                            IsAutoRec = (bool)RecCheck.IsChecked,
                            Description="",
                            IsRecDanmu= (bool)DanMuCheck.IsChecked,
                            IsRemind=false,
                            IsTemporaryPlay=false,
                            Like=false,
                            RoomId=RoomId,
                            Shell="",
                            name=Name,
                        };
                        DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.ReviseRoom(tmp,true,0);
                        Growl.SuccessGlobal($"添加成功");
                        UIDInputBox.Clear();
                        if (tmp.IsAutoRec && Rooms.GetValue(UID, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
                        {
                            Download.AddDownloadTaskd(UID, true);
                            Growl.SuccessGlobal($"添加的房间{tmp.name}({tmp.RoomId})正在直播，开始录制");
                        }
                    }
                }
                else
                {
                    Growl.WarningGlobal($"该房间号或UID不存在！");
                    return;
                }
            }
            else
            {
                Growl.WarningGlobal($"UID不符合规范！");
                return;
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
