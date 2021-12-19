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
using ComboBox = HandyControl.Controls.ComboBox;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : GlowWindow
    {
        public static double DefaultVolume = 0;//默认音量
        private Dialog LogInQRDialog;//登陆过期预留弹出窗口
        event EventHandler<EventArgs> LoginDialogDispose;//登陆窗口登陆事件
        public static List<PlayWindow> playWindowsList = new();
        public static string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static int PlayQuality = int.Parse(CoreConfig.GetValue(CoreConfigClass.Key.PlayQuality, "250", CoreConfigClass.Group.Play));
        public MainWindow()
        {
            InitializeComponent();
            string Title = $"DDTV——你的地表最强B站录播机 {Ver}　({InitDDTV_Core.Ver})";
            this.Title = Title;
            DDTV_ICO.Text = Title;
            //初始化DDTV_Core          
            InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_GUI);

            DefaultVolume = double.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DefaultVolume, "50", CoreConfigClass.Group.Play));

            if (CoreConfig.FirstStart)
            {
                InitialBoot IB = new InitialBoot();
                IB.ShowDialog();
            }

            RoomPatrol.StartLive += RoomPatrol_StartLive;//注册开播提醒事件
            RoomPatrol.StartRec += RoomPatrol_StartRec;//注册开始录制提醒事件
            Download.DownloadCompleted += Download_DownloadCompleted;//注册录制完成提醒事件
            BilibiliUserConfig.CheckAccount.CheckAccountChanged += CheckAccount_CheckAccountChanged;//注册登陆信息检查失效事件
            BilibiliUserConfig.CheckAccount.CheckLoginValidity();
            

            InitMainUI();

            //定时更新界面数据
            UpdateInterface.Main.update(this);
            UpdateInterface.Main.ActivationInterface = 0;
            TimedTask.CheckUpdate.Check();
            TimedTask.DokiDoki.Check();
        }
        /// <summary>
        /// 登陆信息失效事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CheckAccount_CheckAccountChanged(object? sender, EventArgs e)
        {
            LoginDialogDispose += MainWindow_LoginDialogDispose;
            WPFControl.LoginQRDialog LoginQRDialog;
            RoomPatrol.IsOn = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                LoginQRDialog = new WPFControl.LoginQRDialog(LoginDialogDispose, "登陆信息失效，请使用哔哩哔哩手机客户端扫码登陆");
                LogInQRDialog = Dialog.Show(LoginQRDialog);
            }));


        }
        /// <summary>
        /// 登陆完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MainWindow_LoginDialogDispose(object? sender, EventArgs e)
        {
            LogInQRDialog.Dispatcher.BeginInvoke(new Action(() => LogInQRDialog.Close()));
            RoomPatrol.IsOn = true;
        }

        private void InitMainUI()
        {
            DefaultFileNameTextBox.Text = Download.DefaultFileName;
            RecPathTextBox.Text = Download.DefaultPath;
            TmpPathTextBox.Text = Download.TmpPath;
            TranscodToggle.IsChecked = DDTV_Core.Tool.TranscodModule.Transcod.IsAutoTranscod;

            RecQualityComboBox.SelectedIndex = Download.RecQuality == 10000 ? 0 : Download.RecQuality == 400 ? 1 : Download.RecQuality == 250 ? 2 : Download.RecQuality==150? 3:4;
            PlayQualityComboBox.SelectedIndex = PlayQuality == 10000 ? 0 : PlayQuality == 400 ? 1 : PlayQuality == 250 ? 2 : PlayQuality == 150 ? 3 : 4;
            DanmuToggle.IsChecked = Download.IsRecDanmu;
            GiftToggle.IsChecked = Download.IsRecGift;
            GuardToggle.IsChecked = Download.IsRecGuard;
            SCToggle.IsChecked = Download.IsRecSC;
            IsFlvSplitToggle.IsChecked = Download.IsFlvSplit;
            FlvSplitSizeComboBox.Visibility = Download.IsFlvSplit ? Visibility.Visible : Visibility.Collapsed;
            FlvSplitSizeComboBox.SelectedIndex = Download.FlvSplitSize == 8482560409 ? 7 : Download.FlvSplitSize == 6335076761 ? 6 : Download.FlvSplitSize == 2040109465 ? 5 : Download.FlvSplitSize == 5368709120 ? 4 : Download.FlvSplitSize == 4294967296 ? 3 : Download.FlvSplitSize == 3221225472 ? 2 : Download.FlvSplitSize == 2147483648 ? 1 : 0;
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
                if (Directory.Exists(filePath))
                {
                    //Environment.CurrentDirectory
                    if (filePath.Length > 2 && filePath[..1] == ".")
                    {
                        filePath = Environment.CurrentDirectory + filePath.Substring(1, filePath.Length - 1);
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
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                int roomid = UpdateInterface.Main.liveList[Index].RoomId;
                var psi = new ProcessStartInfo
                {
                    FileName = "https://live.bilibili.com/" + roomid,
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
                    if (DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.DeleteRoom(uid))
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
                    IsAutoRec = !YIsAutoRec
                };
                if (RoomConfig.ReviseRoom(roomCard, false, 2))
                {
                    Growl.Success("已" + (!YIsAutoRec ? "打开" : "关闭") + $"[{name}({roomid})]的开播自动录制");
                    if(Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status)== "1")
                    {
                        Download.AddDownloadTaskd(uid, true);
                    }
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
                    Growl.Success("已" + (!YIsRemind ? "打开" : "关闭") + $"[{name}({roomid})]的开播提醒");
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
            if(!File.Exists("./plugins/vlc/libvlc.dll"))
            {
                Growl.Warning($"缺少对应的播放器解码组件！");
                return;
            }
            int Index = LiveList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                if (UpdateInterface.Main.liveList[Index].LiveState == 1)
                {
                    PlayWindow playWindow = new PlayWindow(uid);
                    playWindowsList.Add(playWindow);
                    playWindow.Closed += PlayWindow_Closed;
                    playWindow.Show();

                }
                else
                {
                    Growl.Warning($"该房间未开播");
                }
            }
        }

        private void PlayWindow_Closed(object? sender, EventArgs e)
        {
            playWindowsList.Remove(sender as PlayWindow);
        }

        /// <summary>
        /// log调试按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
        }

        private void DefaultFileNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string Text = DefaultFileNameTextBox.Text.Trim();
            if (Download.DefaultFileName != Text)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    Download.DefaultFileName = Text;
                    CoreConfig.SetValue(CoreConfigClass.Key.DownloadFileName, Download.DefaultFileName, CoreConfigClass.Group.Download);
                    Growl.Success("默认文件名设置成功");
                }
                else
                {
                    DefaultFileNameTextBox.Text = Download.DefaultFileName;
                    Growl.Warning("默认文件名不能为空");
                }
            }
        }

        private void RecPathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string Text = RecPathTextBox.Text.Trim();
            if (Download.DefaultPath != Text)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    if (Directory.Exists(Text))
                    {
                        Download.DefaultPath = Text;
                        if (Download.DefaultPath.Substring(Download.DefaultPath.Length - 1, 1) != "/")
                            Download.DefaultPath = Download.DefaultPath + "/";
                        CoreConfig.SetValue(CoreConfigClass.Key.DownloadPath, Download.DefaultPath, CoreConfigClass.Group.Download);
                        Growl.Success("录制储存文件夹设置成功");
                    }
                    else
                    {
                        Growl.Warning("设置的录制储存文件夹路径文件夹不存在，设置失败！");
                    }
                }
                else
                {
                    RecPathTextBox.Text = Download.DefaultPath;
                    Growl.Warning("录制储存文件夹不能为空");
                }
            }
        }

        private void TmpPathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string Text = TmpPathTextBox.Text.Trim();
            if (Download.TmpPath != Text)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    if (Directory.Exists(Text))
                    {
                        Download.TmpPath = Text;
                        if (Download.TmpPath.Substring(Download.TmpPath.Length - 1, 1) != "/")
                            Download.TmpPath = Download.TmpPath + "/";
                        CoreConfig.SetValue(CoreConfigClass.Key.TmpPath, Download.TmpPath, CoreConfigClass.Group.Download);
                        Growl.Success("临时文件文件夹设置成功");
                    }
                    else
                    {
                        Growl.Warning("设置的临时文件文件夹路径文件夹不存在，设置失败！");
                    }
                }
                else
                {
                    TmpPathTextBox.Text = Download.TmpPath;
                    Growl.Warning("临时文件文件夹不能为空");
                }
            }
        }

        private void TranscodToggle_Click(object sender, RoutedEventArgs e)
        {
            if(!File.Exists("./plugins/ffmpeg/ffmpeg.exe"))
            {
                Growl.Warning($"缺少对应的转码组件！");
                return;
            }
            bool IsTranscod = (bool)TranscodToggle.IsChecked ? true : false;
            DDTV_Core.Tool.TranscodModule.Transcod.IsAutoTranscod = IsTranscod;
            CoreConfig.SetValue(CoreConfigClass.Key.IsAutoTranscod, IsTranscod.ToString(), CoreConfigClass.Group.Core);
            Growl.Success((IsTranscod ? "打开" : "关闭") + "自动转码成功");
        }

        private void RecQualityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ComboBoxItem _ = e.AddedItems[0] as ComboBoxItem;
                int i = int.Parse(_.Tag.ToString());
                switch (i)
                {
                    case 1:
                        Download.RecQuality = 10000;
                        
                        break;
                    case 2:
                        Download.RecQuality = 400;
                        break;
                    case 3:
                        Download.RecQuality = 250;
                        break;
                    case 4:
                        Download.RecQuality = 150;
                        break;
                    case 5:
                        Download.RecQuality = 80;
                        break;
                }
                CoreConfig.SetValue(CoreConfigClass.Key.RecQuality, Download.RecQuality.ToString(), CoreConfigClass.Group.Download);
                Growl.Success("修改录制默认分辨率为"+ (Download.RecQuality == 10000 ? "原画" : Download.RecQuality == 400 ? "蓝光" : Download.RecQuality==250?"超清": Download.RecQuality == 150 ? "高清" : "流畅"));
            }
        }
        /// <summary>
        /// 监控列表_右键菜单_切换弹幕录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_SetIsDanmu_Click(object sender, RoutedEventArgs e)
        {
            int Index = LiveList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                string name = UpdateInterface.Main.liveList[Index].Name;
                int roomid = UpdateInterface.Main.liveList[Index].RoomId;
                bool YIsRecDanmu = bool.Parse(Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.IsRecDanmu));
                RoomConfigClass.RoomCard roomCard = new RoomConfigClass.RoomCard()
                {
                    UID = uid,
                    IsRecDanmu = !YIsRecDanmu
                };
                if (RoomConfig.ReviseRoom(roomCard, false, 6))
                {
                    Growl.Success("已" + (!YIsRecDanmu ? "打开" : "关闭") + $"[{name}({roomid})]的弹幕录制");
                }
                else
                {
                    Growl.Warning($"修改[{name}({roomid})]的弹幕录制出现问题，修改失败");
                }
            }
        }

        private void DanmuToggle_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)DanmuToggle.IsChecked ? true : false;
            Download.IsRecDanmu = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecDanmu, Is.ToString(), CoreConfigClass.Group.Download);
            Growl.Success((Is ? "打开" : "关闭") + "弹幕录制总开关");
            if(Is)
            {
                GiftToggle.IsEnabled = true;
                GuardToggle.IsEnabled = true;
                SCToggle.IsEnabled = true;
            }
            else
            {
                GiftToggle.IsEnabled = false;
                GuardToggle.IsEnabled = false;
                SCToggle.IsEnabled = false;
            }
        }

        private void GiftToggle_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)GiftToggle.IsChecked ? true : false;
            Download.IsRecGift = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecGift, Is.ToString(), CoreConfigClass.Group.Download);
            Growl.Success((Is ? "打开" : "关闭") + "礼物录制");
        }

        private void GuardToggle_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)GuardToggle.IsChecked ? true : false;
            Download.IsRecGuard = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecGuard, Is.ToString(), CoreConfigClass.Group.Download);
            Growl.Success((Is ? "打开" : "关闭") + "上舰录制");
        }

        private void SCToggle_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)SCToggle.IsChecked ? true : false;
            Download.IsRecSC = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecSC, Is.ToString(), CoreConfigClass.Group.Download);
            Growl.Success((Is ? "打开" : "关闭") + "SC录制");
        }

        private void IsFlvSplitToggle_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)IsFlvSplitToggle.IsChecked ? true : false;
            Download.IsFlvSplit = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsFlvSplit, Is.ToString(), CoreConfigClass.Group.Download);
            Growl.Success((Is ? "打开" : "关闭") + "启用录制文件大小限制(自动分割)");
            if(Is)
            {
                FlvSplitSizeComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                FlvSplitSizeComboBox.Visibility = Visibility.Collapsed;
            }
        }

        private void FlvSplitSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ComboBoxItem _ = e.AddedItems[0] as ComboBoxItem;
                int i = int.Parse(_.Tag.ToString());
                switch (i)
                {
                    case 1:
                        Download.FlvSplitSize = 1073741824;
                        break;
                    case 2:
                        Download.FlvSplitSize = 2147483648;
                        break;
                    case 3:
                        Download.FlvSplitSize = 3221225472;
                        break;
                    case 4:
                        Download.FlvSplitSize = 4294967296;
                        break;
                    case 5:
                        Download.FlvSplitSize = 5368709120;
                        break;
                    case 6:
                        Download.FlvSplitSize = 2040109465;
                        break;
                    case 7:
                        Download.FlvSplitSize = 6335076761;
                        break;
                    case 8:
                        Download.FlvSplitSize = 8482560409;
                        break;
                }
                CoreConfig.SetValue(CoreConfigClass.Key.FlvSplitSize, Download.FlvSplitSize.ToString(), CoreConfigClass.Group.Download);
                Growl.Success("修改文件大小限制为" + (Download.FlvSplitSize == 8482560409 ? "7.9GB" : Download.FlvSplitSize == 6335076761 ? "5.9GB" : Download.FlvSplitSize == 2040109465 ? "1.9GB" : Download.FlvSplitSize == 5368709120 ? "5GB" : Download.FlvSplitSize == 4294967296 ? "4GB" : Download.FlvSplitSize == 3221225472 ? "3GB" : Download.FlvSplitSize == 2147483648 ? "2GB" : "1GB"));
            }
        }

        private void PlayQualityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ComboBoxItem _ = e.AddedItems[0] as ComboBoxItem;
                int i = int.Parse(_.Tag.ToString());
                switch (i)
                {
                    case 1:
                        PlayQuality = 10000;
                        break;
                    case 2:
                        PlayQuality = 400;
                        break;
                    case 3:
                        PlayQuality = 250;
                        break;
                    case 4:
                        PlayQuality = 150;
                        break;
                    case 5:
                        PlayQuality = 80;
                        break;
                }
                CoreConfig.SetValue(CoreConfigClass.Key.PlayQuality, PlayQuality.ToString(), CoreConfigClass.Group.Play);
                Growl.Success("修改默认在线观看画质为" + (Download.RecQuality == 10000 ? "原画" : Download.RecQuality == 400 ? "蓝光" : Download.RecQuality == 250 ? "超清" : Download.RecQuality == 150 ? "高清" : "流畅"));
            }
        }
    }
}
