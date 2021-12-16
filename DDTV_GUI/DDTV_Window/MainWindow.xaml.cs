using DDTV_Core;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_Core.SystemAssembly.RoomPatrolModule;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using MessageBox = HandyControl.Controls.MessageBox;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : GlowWindow
    {
        public event EventHandler<EventArgs> DisposeSent;
        public static double DefaultVolume = double.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DefaultVolume, "50", CoreConfigClass.Group.Play));
        public MainWindow()
        {
            InitializeComponent();
            string Title = $"{InitDDTV_Core.Ver}";
            this.Title = Title;
            DDTV_ICO.Text = Title;

            //初始化DDTV_Core
           
            InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_GUI);
            RoomPatrol.StartLive += RoomPatrol_StartLive;
            RoomPatrol.StartRec += RoomPatrol_StartRec;
            Download.DownloadCompleted += Download_DownloadCompleted;
            //定时更新界面数据
            UpdateInterface.Main.update(this);
            UpdateInterface.Main.ActivationInterface = 0;

            InitMainUI();
        }
        private void InitMainUI()
        {
            DefaultFileName.Text = Download.DefaultFileName;
        }

        private void Download_DownloadCompleted(object? sender, EventArgs e)
        {
            DownloadClass.Downloads downloads = (DownloadClass.Downloads)sender;
            Growl.InfoGlobal($"{downloads.Name}的直播[{downloads.Title}]录制完成");
        }

        private void RoomPatrol_StartRec(object? sender, EventArgs e)
        {
            RoomInfoClass.RoomInfo roomInfo = (RoomInfoClass.RoomInfo)sender;
            Growl.InfoGlobal($"开始录制{roomInfo.room_id}({roomInfo.uname})的直播");
        }

        private void RoomPatrol_StartLive(object? sender, EventArgs e)
        {
            RoomInfoClass.RoomInfo roomInfo = (RoomInfoClass.RoomInfo)sender;
            Growl.InfoGlobal($"房间{roomInfo.room_id}({roomInfo.uname})开始直播了");
        }

        private void SideMenu_SelectionChanged(object sender, HandyControl.Data.FunctionEventArgs<object> e)
        {
            SideMenu sideMenu = (SideMenu)sender;
            int Index = sideMenu.Items.IndexOf(e.Info);
            this.MainWindowTab.SelectedIndex = Index;
            UpdateInterface.Main.ActivationInterface = Index;
        }


        private void GlowWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Growl.InfoGlobal("测试:蒂蒂媞薇开播了，根据设置开始自动录制");
            Growl.WarningGlobal("测试:储存空间剩余空间不足，请及时清理");
            Growl.ErrorGlobal("测试:储存空间已满，无法正常运行");
            Growl.AskGlobal("测试:蒂蒂媞薇开播了，是否打开直播间？", isConfirmed =>
            {
                if (isConfirmed)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "https://live.bilibili.com/21446992",
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                else
                {
                    Growl.InfoGlobal("好吧，不看就算了");
                }
                return true;
            });
            Growl.AskGlobal("测试:蒂蒂媞薇开播了，是否打开直播间？", isConfirmed =>
            {
                if (isConfirmed)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "https://live.bilibili.com/21446992",
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                else
                {
                    Growl.InfoGlobal("好吧，不看就算了");
                }
                return true;
            });
        }

        /// <summary>
        /// 完成记录_右键菜单_打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverList_CopyFolderPath_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int Index = OverList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.overList.Count > Index)
            {
                string filePath = UpdateInterface.Main.overList[Index].FilePath;
                if(Directory.Exists(filePath))
                {
                    //Environment.CurrentDirectory
                    if (filePath.Length>2&&filePath[..1] ==".")
                    {
                        filePath = Environment.CurrentDirectory + filePath.Substring(1, filePath.Length-1);
                    }
                    Clipboard.SetDataObject(filePath);
                    Growl.Success("已复制路径到粘贴板");
                }
                else
                {
                    Growl.Warning($"该文件路径不存在，打开失败");
                }
            }
        }
        /// <summary>
        /// 完成记录_右键菜单_播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OverList_Play_MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    int Index = OverList.SelectedIndex;
        //    if (UpdateInterface.Main.overList.Count > Index)
        //    {
        //        string fileName = UpdateInterface.Main.overList[Index].FileName;
        //        if (File.Exists(fileName))
        //        {
        //            Process.Start(fileName);
        //        }
        //        else
        //        {
        //            Growl.Warning($"文件不存在，打开失败");
        //        }
        //    }
        //}
        /// <summary>
        /// 托盘右键退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ICO_MenuItem_EXIT_Click(object sender, RoutedEventArgs e)
        {
            APP_EXIT();
        }
        /// <summary>
        /// 关闭询问窗事件
        /// </summary>
        /// <returns></returns>
        private bool APP_EXIT()
        {
            MessageBoxResult dr = HandyControl.Controls.MessageBox.Show("警告！当前退出DDTV会导致未完成的任务数据丢失\n确认退出DDTV?", "退出", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 主窗口关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlowWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!APP_EXIT())
            {
                e.Cancel = true;
            }
        }
        /// <summary>
        /// 托盘ICO双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DDTV_ICO_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.Activate();
            this.Focus();
            UpdateInterface.Main.ActivationInterface = UpdateInterface.Main.PreviousPage;
        }
        /// <summary>
        /// 窗口状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlowWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                UpdateInterface.Main.ActivationInterface = -1;
                this.Visibility = Visibility.Hidden;
            }
        }
        /// <summary>
        /// 录制任务打开右键菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateInterface.Main.ContextMenuState.RecListPage = true;
        }
        /// <summary>
        /// 录制任务关闭右键菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecList_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            UpdateInterface.Main.ContextMenuState.RecListPage = false;
        }
        /// <summary>
        /// 监控列表打开右键菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateInterface.Main.ContextMenuState.LiveListPage = true;
        }
        /// <summary>
        /// 监控列表关闭右键菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            UpdateInterface.Main.ContextMenuState.LiveListPage = false;
        }
        /// <summary>
        /// 完成记录打开右键菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateInterface.Main.ContextMenuState.OverListPage = true;
        }
        /// <summary>
        /// 完成记录关闭右键菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverList_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            UpdateInterface.Main.ContextMenuState.OverListPage = false;
        }
        /// <summary>
        /// 监控列表_右键菜单_打开直播间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_OpenLiveRoomUrl_Click(object sender, RoutedEventArgs e)
        {
            int Index = LiveList.SelectedIndex;
            if(Index > -1 && UpdateInterface.Main.liveList.Count>Index)
            {
                int roomid = UpdateInterface.Main.liveList[Index].RoomId;
                var psi = new ProcessStartInfo
                {
                    FileName = "https://live.bilibili.com/"+ roomid,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
          
        }
        /// <summary>
        /// 监控列表_右键菜单_删除直播间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_DelRoom_Click(object sender, RoutedEventArgs e)
        {
            int Index = LiveList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                int roomid = UpdateInterface.Main.liveList[Index].RoomId;
                string name = UpdateInterface.Main.liveList[Index].Name;
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                MessageBoxResult dr = HandyControl.Controls.MessageBox.Show($"确定要删除房间[{name}({roomid})]吗？", "删除房间", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    if(DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.DeleteRoom(uid))
                    { 
                        Growl.Success($"删除[{name}({roomid})]成功");
                    }
                    else
                    {
                        Growl.Warning($"删除[{name}({roomid})]出现未知问题，删除失败");
                    }
                }
            }
        }
        /// <summary>
        /// 录制任务_右键菜单_打开直播间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecLive_MenuItem_OpenLiveRoomUrl_Click(object sender, RoutedEventArgs e)
        {
            int Index = RecList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.recList.Count > Index)
            {
                int roomid = UpdateInterface.Main.recList[Index].RoomId;
                var psi = new ProcessStartInfo
                {
                    FileName = "https://live.bilibili.com/" + roomid,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }
        /// <summary>
        /// 录制任务_右键菜单_取消录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecLive_MenuItem_Cancel_Click(object sender, RoutedEventArgs e)
        {
            int Index = RecList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.recList.Count > Index)
            {
                long Uid = UpdateInterface.Main.recList[Index].Uid;
                string name = UpdateInterface.Main.recList[Index].Name;
                int roomid = UpdateInterface.Main.recList[Index].RoomId;
                if (DDTV_Core.SystemAssembly.DownloadModule.Download.CancelDownload(Uid))
                {
                    Growl.Success($"已取消[{name}({roomid})]录制任务");
                }
                else
                {
                    Growl.Warning($"取消[{name}({roomid})]的录制任务出现未知问题，取消失败");
                }
            }
        }
        /// <summary>
        /// 监控列表_右键菜单_切换自动录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_SetIsRec_Click(object sender, RoutedEventArgs e)
        {
            int Index = LiveList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                string name = UpdateInterface.Main.liveList[Index].Name;
                int roomid = UpdateInterface.Main.liveList[Index].RoomId;
                bool YIsAutoRec = bool.Parse(Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.IsAutoRec));
                RoomConfigClass.RoomCard roomCard = new RoomConfigClass.RoomCard()
                {
                    UID = uid,
                    IsAutoRec=!YIsAutoRec
                };
                if (RoomConfig.ReviseRoom(roomCard, false, 2))
                {
                    Growl.Success("已" + (!YIsAutoRec ? "打开" : "关闭") + $"[{name}({roomid})]的开播自动录制");
                }
                else
                {
                    Growl.Warning($"修改[{name}({roomid})]的开播自动录制出现问题，修改失败");
                }
            }
        }
        /// <summary>
        /// 监控列表_右键菜单_切换开播提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_SetIsRemind_Click(object sender, RoutedEventArgs e)
        {
            int Index = LiveList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                string name = UpdateInterface.Main.liveList[Index].Name;
                int roomid = UpdateInterface.Main.liveList[Index].RoomId;
                bool YIsRemind = bool.Parse(Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.IsRemind));
                RoomConfigClass.RoomCard roomCard = new RoomConfigClass.RoomCard()
                {
                    UID = uid,
                    IsRemind = !YIsRemind
                };
                if (RoomConfig.ReviseRoom(roomCard, false, 3))
                {
                    Growl.Success("已"+ (!YIsRemind?"打开":"关闭")+$"[{name}({roomid})]的开播提醒");
                }
                else
                {
                    Growl.Warning($"修改[{name}({roomid})]的开播提醒出现问题，修改失败");
                }
            }
        }
        /// <summary>
        /// 监控列表_右键菜单_添加房间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_AddRoom_Click(object sender, RoutedEventArgs e)
        {
            Dialog.Show<WPFControl.AddRoomDialog>();
        }
        /// <summary>
        /// 监控列表_右键菜单_在DDTV中观看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_Play_Click(object sender, RoutedEventArgs e)
        {
            int Index = LiveList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                if(UpdateInterface.Main.liveList[Index].LiveState==1)
                {
                    PlayWindow playWindow = new PlayWindow(uid);
                    playWindow.Show();
                }
                else
                {
                    Growl.Warning($"该房间未开播");
                }          
            }
        }
    }
}
