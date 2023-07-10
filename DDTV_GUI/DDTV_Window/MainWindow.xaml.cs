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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = HandyControl.Controls.MessageBox;
using DDTV_GUI.WPFControl;
using System.Reflection;
using DDTV_Core.SystemAssembly.Log;
using Microsoft.WindowsAPICodePack.Dialogs;
using DDTV_Core.SystemAssembly.BilibiliModule.API;
using DDTV_Core.Tool.TranscodModule;
using DDTV_Core.Tool;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Resources;
using System.Data;
using static DDTV_Core.SystemAssembly.BilibiliModule.Rooms.RoomInfoClass;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : GlowWindow
    {
        public static int linkDMNum = 0;
        public static double DefaultVolume = 0;//默认音量
        private Dialog LogInQRDialog;//登陆过期预留弹出窗口
        private Dialog ClipDialog;//切片窗口
        private Dialog OpenDanMuWindowDialog;//切片窗口
        private Dialog SearchDialog;//搜索弹出窗口

        private static bool HideIconState = false;
        public static event EventHandler<EventArgs> LoginDialogDispose;//登陆窗口登陆事件
        public static event EventHandler<EventArgs> CuttingDialogDispose;//切片窗口关闭事件
        public static event EventHandler<EventArgs> OpenDanMuWindowDialogDispose;//切片窗口关闭事件
        public static event EventHandler<EventArgs> SearchDialogDispose;//搜索窗口关闭事件
        public NotifyIcon notifyIcon = new();
        public static List<PlayWindow> playWindowsList = new();
        public static GuideMode guideMode = GuideMode.N;
        public static string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static DDTV_DanMu.MainWindow DanMuWindow = null;



        public enum GuideMode
        {
            N,
            W1_6,
            W1_8,
            W1_13,
        }


        public MainWindow()
        {
            InitializeComponent();//初始化主界面
            DDTV_GUI.App.Application_Startup();//检查是否重复启动
#if DEBUG
            string Title = $"DDTV——你的地表最强阿B播放器 {Ver}　(Dev版-编译时间:{InitDDTV_Core.CompiledVersion})";
#endif
#if !DEBUG
            string Title = $"DDTV——你的地表最强阿B播放器 {Ver}　({InitDDTV_Core.Ver})";
#endif
            this.Title = Title;//设置标题
            DDTV_ICO.Text = Title;//设置ICON名称
            InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_GUI); //初始化DDTV_Core          
            if (CoreConfig.GUI_FirstStart)//判断是否首次启动
            {
                InitialBoot IB = new InitialBoot();//初始化'初始化界面'
                IB.ShowDialog();//阻塞显示初始化界面
            }
            LibVLCSharp.Shared.Core.Initialize("./plugins/vlc");//初始化VLC播放器组件
            RoomPatrol.StartLive += RoomPatrol_StartLive;//注册开播提醒事件
            RoomPatrol.StartRec += RoomPatrol_StartRec;//注册开始录制提醒事件
            Download.DownloadCompleted += Download_DownloadCompleted;//注册录制完成提醒事件
            PlayWindow.PlayListExit += MainWindow_PlayListExit;//注册批量关闭播放窗口事件
            FileOperation.PathAlmostFull += FileOperation_PathAlmostFull;//硬盘空间不足事件
            Tool.ServerInteraction.Start();//更新看门狗程序
            Tool.ServerInteraction.CheckUpdates.NewUpdate += CheckUpdates_NewUpdate;//检查更新事件
            Tool.ServerInteraction.Notice.NewNotice += Notice_NewNotice;//更新首页说明事件
            BilibiliUserConfig.CheckAccount.CheckAccountChanged += CheckAccount_CheckAccountChanged;//注册登陆信息检查失效事件
            BilibiliUserConfig.CheckAccount.CheckLoginValidity();//启动账号有效性定时检查     
            InitMainUI();//初始化UI组件显示内容     
            UpdateInterface.Main.update(this);//定时更新界面数据
            UpdateInterface.Main.ActivationInterface = 0;//设置默认显示0号界面
            DelTmpFile();//检查临时文件夹，并删除里面的文件

            //GUI初始化启动完成



            //系统托盘提示
            //UpdateInterface.Notify.NotifyUpdate += Notify_NotifyUpdate;
            //TimedTask.CheckUpdate.Check();
            //TimedTask.DokiDoki.Check();
            //ClipWindow clipWindow = new ClipWindow();
            //clipWindow.Show();

            //Tool.Beep.MessageBeep((uint)Tool.Beep.Type.Information);

            //Sprite.Show(new DDTV_Sprite());




            ////测试房间卡片样式
            //Task.Run(() =>
            //{
            //    Thread.Sleep(5000);
            //    foreach (var item in Rooms.RoomInfo)
            //    {
            //        if (item.Value.live_status == 1)
            //        {
            //            AddRoomCard(item.Value.uid);
            //        }
            //    }
            //    foreach (var item in Rooms.RoomInfo)
            //    {
            //        if (item.Value.live_status != 1)
            //        {
            //            AddRoomCard(item.Value.uid);
            //        }
            //    }
            //});

        }

        //public void TEST_B()
        //{
        //    List<BindingData.LiveList> _ = new();
        //    foreach (var item in Rooms.RoomInfo)
        //    {
        //        BindingData.LiveList live = new(item.Value.uname,
        //            item.Value.live_status == 1 ? "直播中" : " 摸了",
        //            item.Value.IsRemind ? "     √" : "      ",
        //            item.Value.IsAutoRec ? "     √" : "      ",
        //            item.Value.room_id,
        //            item.Value.uid,
        //            item.Value.live_status,
        //            item.Value.IsRecDanmu ? "     √" : "      ",
        //            item.Value.title);
        //        _.Add(live);
        //    }
        //    TESTDG.ItemsSource = _;

        //}


        public static string GetPackageVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        private void AddRoomCard(long uid)
        {


            if (Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Grid grid = new Grid()
                    {
                        Margin = new Thickness(-6, -6, -6, -6)
                    };
                    var bitmapImage = new BitmapImage();
                    if (!string.IsNullOrEmpty(roomInfo.cover_from_user))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(roomInfo.cover_from_user); ;
                        bitmapImage.EndInit();
                    }
                    Image image = new Image()
                    {
                        Margin = new Thickness(0, -6, -12, 32),
                        Source = bitmapImage
                    };
                    image.Clip = new RectangleGeometry()
                    {
                        RadiusX = 10,
                        RadiusY = 10,
                        Rect = new Rect(0, 0, 198, 110)
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Margin = new Thickness(0, 110, 0, 28),
                        Text = roomInfo.uname,
                        Foreground = new SolidColorBrush(Colors.Black),
                        FontSize = 16
                    };
                    grid.Children.Add(image);
                    grid.Children.Add(textBlock);
                    if (roomInfo.live_status != 1)
                    {
                        Grid GBlack = new Grid()
                        {
                            Margin = new Thickness(0, 0, -12, 32),
                            Background = new SolidColorBrush(Colors.Black),
                            Opacity = 0.5
                        };
                        GBlack.Clip = new RectangleGeometry()
                        {
                            RadiusX = 10,
                            RadiusY = 10,
                            Rect = new Rect(0, 0, 198, 110)
                        };
                        grid.Children.Add(GBlack);

                    }
                    Image B = new Image()
                    {
                        Height = 24,
                        Width = 24,
                        Margin = new Thickness(-180, 130, 0, 0),
                        Source = GetImageSouce(Properties.Resources.Rec)
                    };
                    grid.Children.Add(B);
                    grid.Children.Add(new Image()
                    {
                        Height = 24,
                        Width = 24,
                        Margin = new Thickness(-128, 130, 0, 0),
                        Source = GetImageSouce(Properties.Resources.DanMu)
                    });
                    grid.Children.Add(new Image()
                    {
                        Height = 24,
                        Width = 24,
                        Margin = new Thickness(-76, 130, 0, 0),
                        Source = GetImageSouce(Properties.Resources.Remind)
                    });
                    Border border = new Border()
                    {
                        Width = 208,
                        Height = 166,
                        Margin = new Thickness(2, 2, 2, 2),
                        Padding = new Thickness(10, 10, 10, 10),
                        BorderThickness = new Thickness(1, 1, 1, 1),
                        BorderBrush = new SolidColorBrush(Colors.White),
                        Background = new SolidColorBrush(Colors.White),
                        CornerRadius = new CornerRadius(5)
                    };
                    //BorderThickness="1" BorderBrush="White" Background="White"
                    border.Child = grid;
                    Window_RoomCard.Children.Add(border);
                });
            }
        }
        private BitmapSource GetImageSouce(System.Drawing.Bitmap bitmap)
        {
            BitmapSource img;
            IntPtr hBitmap;

            hBitmap = bitmap.GetHbitmap();
            img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return img;
        }

        /// <summary>
        /// 删除临时文件
        /// </summary>
        private void DelTmpFile()
        {
            Task.Run(() =>
            {
                if (Directory.Exists(Download.TmpPath))
                {
                    DirectoryInfo root = new DirectoryInfo(Download.TmpPath);
                    foreach (FileInfo item in root.GetFiles())
                    {
                        DDTV_Core.Tool.FileOperation.Del(item.FullName);
                    }
                }
            });
        }


        private void FileOperation_PathAlmostFull(object? sender, string e)
        {
            Growl.WarningGlobal(e);
        }

        private void Notify_NotifyUpdate(object? sender, EventArgs e)
        {
            UpdateInterface.Notify.InfoC A = (UpdateInterface.Notify.InfoC)sender;
            notifyIcon.ShowBalloonTip(A.Title, A.Content, HandyControl.Data.NotifyIconInfoType.Info);
        }

        private void Notice_NewNotice(object? sender, EventArgs e)
        {
            string N = "";
            N = sender as string;
            Notice.Dispatcher.Invoke(() =>
            {
                Notice.Text = N;
            });
        }

        private void CheckUpdates_NewUpdate(object? sender, EventArgs e)
        {
            if (!Tool.ExamineFullScreen.IsForegroundFullScreen())
            {
                if (CoreConfig.AutoInsallUpdate)
                {
                    bool IsDL = false;
                    foreach (var A1 in Rooms.RoomInfo)
                    {
                        if (A1.Value.DownloadingList.Count > 0)
                        {
                            IsDL = true;
                        }
                    }
                    if (IsDL || playWindowsList.Count > 0)
                    {
                        Growl.InfoGlobal($"DDTV检测到更新，但是当前有观看/录制任务正在进行中，等待任务结束后空闲时间会自动更新");
                    }
                    else
                    {

                        Log.AddLog(nameof(MainWindow), LogClass.LogType.Info, $"触发自动更新", false, null, false);
                        Update(true);
                    }
                }
                else
                {
                    Growl.InfoGlobal($"DDTV检测到更新，请在[设置]界面中点击[更新DDTV]进行自动更新");
                }
            }
            else
            {
                Log.AddLog(nameof(MainWindow), LogClass.LogType.Info, $"正在运行全屏任务，跳过自动更新", false, null, false);
            }
        }

        private bool CheckRepeatedRun()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.MainModule.FileName == current.MainModule.FileName)
                    {
                        MessageBox.Show("已经有DDTV_GUI实例正在运行！");
                        return true;
                    }
                }
            }
            return false;
        }
        private void MainWindow_PlayListExit(object? sender, EventArgs e)
        {

            for (int i = 0; i < playWindowsList.Count;)
            {
                playWindowsList[0].Dispatcher.Invoke(() => playWindowsList[0].Close());

            }

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
            //RoomPatrol.IsOn = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                LoginQRDialog = new LoginQRDialog(LoginDialogDispose, "登陆信息失效，请使用哔哩哔哩手机客户端扫码登陆");
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

        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void InitMainUI()
        {
            DefaultVolume = GUIConfig.DefaultVolume;
            HideIconState = GUIConfig.HideIconState;

            DefaultFileNameTextBox.Text = Download.DownloadFileName;
            DefaultFolderNameTextBox.Text = Download.DownloadFolderName;
            RecPathTextBox.Text = Download.DownloadPath;
            TmpPathTextBox.Text = Download.TmpPath;
            TranscodToggle.IsChecked = Transcod.IsAutoTranscod;
            HideIcon.IsChecked = HideIconState;

            RecQualityComboBox.SelectedIndex = Download.RecQuality == 10000
                ? 0 : Download.RecQuality == 400
                ? 1 : Download.RecQuality == 250
                ? 2 : Download.RecQuality == 150
                ? 3 : 4;

            PlayQualityComboBox.SelectedIndex = GUIConfig.PlayQuality == 10000
                ? 0 : GUIConfig.PlayQuality == 400
                ? 1 : GUIConfig.PlayQuality == 250
                ? 2 : GUIConfig.PlayQuality == 150
                ? 3 : 4;

            {
                DanmuToggle.IsChecked = Download.IsRecDanmu;
                //GiftToggle.IsChecked = Download.IsRecGift;
                //GuardToggle.IsChecked = Download.IsRecGuard;
                //SCToggle.IsChecked = Download.IsRecSC;
                DanmuToAssButton.IsChecked = GUIConfig.IsXmlToAss;
                SetDanmakuFactoryParameter.Text = GUIConfig.DanmukuFactoryParameter;
                if (Download.IsRecDanmu)
                {
                    DanmuToAssText.Visibility = Visibility.Visible;
                    DanmuToAssButton.Visibility = Visibility.Visible;
                    if (GUIConfig.IsXmlToAss)
                    {
                        SetDanmakuFactoryParameter.Visibility = Visibility.Visible;
                        SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SetDanmakuFactoryParameter.Visibility = Visibility.Collapsed;
                        SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    DanmuToAssText.Visibility = Visibility.Collapsed;
                    DanmuToAssButton.Visibility = Visibility.Collapsed;
                    SetDanmakuFactoryParameter.Visibility = Visibility.Collapsed;
                    SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Collapsed;
                }

            }
            IsFlvSplitToggle.IsChecked = Download.IsFlvSplit;
            FlvSplitSizeComboBox.Visibility = Download.IsFlvSplit ? Visibility.Visible : Visibility.Collapsed;

            FlvSplitSizeComboBox.SelectedIndex = Download.FlvSplitSize == 10485760
                ? 8 : Download.FlvSplitSize == 8482560409
                ? 7 : Download.FlvSplitSize == 6335076761
                ? 6 : Download.FlvSplitSize == 2040109465
                ? 5 : Download.FlvSplitSize == 5368709120
                ? 4 : Download.FlvSplitSize == 4294967296
                ? 3 : Download.FlvSplitSize == 3221225472
                ? 2 : Download.FlvSplitSize == 2147483648
                ? 1 : 0;

            DoNotSleepWhileDownloadingIcon.IsChecked = Dokidoki.IsDoNotSleepState;
            //ForceCDNResolution.IsChecked = RoomInfo.ForceCDNResolution;
            //TranscodingCompleteAutoDeleteFiles.IsChecked = Transcod.TranscodingCompleteAutoDeleteFiles;
            DDcenterSwitch.IsChecked = DDcenter.DDcenterSwitch;
            SpaceIsInsufficientWarn.IsChecked = DDTV_Core.Tool.FileOperation.SpaceIsInsufficientWarn;
            ReplaceAPIText.Text = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.ReplaceAPI;
            SelectAPI_v1.IsChecked = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.APIVersion == 1 ? true : false;
            SelectAPI_v2.IsChecked = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.APIVersion == 2 ? true : false;
            AutoUpdateSwitch.IsChecked = CoreConfig.AutoInsallUpdate;
            ProxySwitch.IsChecked = CoreConfig.WhetherToEnableProxy;
            DevSwitch.IsChecked = CoreConfig.IsDev;

            //SelectDanMu_v1.IsChecked = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuSaveType == 1 ? true : false;
            //SelectDanMu_v2.IsChecked = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuSaveType == 2 ? true : false;
            HLSToggle.IsChecked = DDTV_Core.SystemAssembly.DownloadModule.Download.IsHls;

            WaitHLSTime.Value = DDTV_Core.SystemAssembly.DownloadModule.Download.WaitHLSTime;
            DoesShieldTakeEffectSwitch.IsChecked = GUIConfig.DoesShieldTakeEffect;
        }

        private void Download_DownloadCompleted(object? sender, EventArgs e)
        {
            DownloadClass.Downloads downloads = (DownloadClass.Downloads)sender;
            Growl.InfoGlobal($"{downloads.Name}的直播[{downloads.Title}]录制完成");
            System.Media.SystemSounds.Hand.Play();
        }

        private void RoomPatrol_StartRec(object? sender, EventArgs e)
        {
            RoomInfoClass.RoomInfo roomInfo = (RoomInfoClass.RoomInfo)sender;
            Growl.InfoGlobal($"开始录制{roomInfo.room_id}({roomInfo.uname})的直播");
            System.Media.SystemSounds.Hand.Play();
        }

        private void RoomPatrol_StartLive(object? sender, EventArgs e)
        {
            RoomInfoClass.RoomInfo roomInfo = (RoomInfoClass.RoomInfo)sender;
            Growl.InfoGlobal($"房间{roomInfo.room_id}({roomInfo.uname})开始直播了");
            System.Media.SystemSounds.Hand.Play();
        }

        private void SideMenu_SelectionChanged(object sender, HandyControl.Data.FunctionEventArgs<object> e)
        {
            SideMenu sideMenu = (SideMenu)sender;
            int Index = sideMenu.Items.IndexOf(e.Info);
            this.MainWindowTab.SelectedIndex = Index;
            if (Index >= 0)
                UpdateInterface.Main.ActivationInterface = Index;
            //TEST_B();

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
                    Growl.Warning($"该文件路径不存在，复制失败");
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
            if (GUIConfig.IsExitReminder)
            {
                MessageBoxResult dr = HandyControl.Controls.MessageBox.Show("警告！当前退出会导致未完成的任务数据丢失\n确认退出?", "退出", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    DelTmpFile();//删除临时文件
                    Application.Current.Shutdown();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                DelTmpFile();
                Application.Current.Shutdown();
                return true;
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
            if (HideIconState)
            {
                this.Visibility = Visibility.Visible;
                this.Activate();
                this.Focus();
                UpdateInterface.Main.ActivationInterface = UpdateInterface.Main.PreviousPage;
                Process process = DDTV_GUI.App.RuningInstance(false);
                DDTV_GUI.App.HandleRunningInstance(process);
            }


        }
        /// <summary>
        /// 窗口状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlowWindow_StateChanged(object sender, EventArgs e)
        {
            if (HideIconState)
            {
                if (WindowState == WindowState.Minimized)
                {
                    Growl.InfoGlobal("DDTV已最小化到系统托盘icon中,请双击托盘icon恢复到开始菜单");
                    UpdateInterface.Main.ActivationInterface = -1;
                    this.Visibility = Visibility.Hidden;
                }
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
                    if (RoomConfig.DeleteRoom(uid))
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
                if (Download.CancelDownload(Uid))
                {
                    Growl.Success($"已取消[{name}({roomid})]录制任务，界面显示有延迟，稍后会自动消失");
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
                    if (!YIsAutoRec && Rooms.GetValue(uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
                    {
                        Download.AddVideoDownloadTaskd(uid, true);
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
            Dialog.Show(new AddRoomDialog(false));
        }

        /// <summary>
        /// 监控列表_右键菜单_在DDTV中观看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_Play_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Play_Click());
        }
        private void Play_Click()
        {
            if (!File.Exists("./plugins/vlc/libvlc.dll"))
            {
                Growl.Warning($"缺少对应的播放器解码组件！");
                return;
            }
            int Index = 0;
            this.Dispatcher.Invoke(new Action(() =>
            {
                Index = LiveList.SelectedIndex;
            }));

            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                string name = UpdateInterface.Main.liveList[Index].Name;
                if (UpdateInterface.Main.liveList[Index].LiveState == 1)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        PlayWindow playWindow = new PlayWindow(uid, name);
                        playWindowsList.Add(playWindow);
                        playWindow.Closed += PlayWindow_Closed;
                        playWindow.Show();
                    }));


                }
                else
                {
                    Growl.Warning($"该房间未开播");
                }
            }
        }

        public static void PlayWindow_Closed(object? sender, EventArgs e)
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
            if (Download.DownloadFileName != Text)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    Download.DownloadFileName = Text;
                    CoreConfig.SetValue(CoreConfigClass.Key.DownloadFileName, Download.DownloadFileName, CoreConfigClass.Group.Download);
                    Growl.Success("默认文件名设置成功");
                    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, "默认文件名设置成功:" + Download.DownloadFileName, false, null, false);
                    CoreConfigFile.WriteConfigFile(true);
                }
                else
                {
                    DefaultFileNameTextBox.Text = Download.DownloadFileName;
                    Growl.Warning("默认文件名不能为空");
                }
            }
        }
        private void DefaultFolderNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string Text = DefaultFolderNameTextBox.Text.Trim();
            if (Download.DownloadFolderName != Text)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    Download.DownloadFolderName = Text;
                    CoreConfig.SetValue(CoreConfigClass.Key.DownloadFolderName, Download.DownloadFolderName, CoreConfigClass.Group.Download);
                    Growl.Success("默认日期文件夹名称格式设置成功");
                    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, "默认日期文件夹名称格式设置成功:" + Download.DownloadFolderName, false, null, false);
                    CoreConfigFile.WriteConfigFile(true);
                }
                else
                {
                    DefaultFolderNameTextBox.Text = Download.DownloadFolderName;
                    Growl.Warning("默认日期文件夹名称不能为空");
                }
            }
        }

        private void RecPathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string Text = RecPathTextBox.Text.Trim();
            if (Download.DownloadPath != Text)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    if (Directory.Exists(Text))
                    {
                        Download.DownloadPath = Text;
                        if (Download.DownloadPath.Substring(Download.DownloadPath.Length - 1, 1) != "/")
                            Download.DownloadPath = Download.DownloadPath + "/";
                        CoreConfig.SetValue(CoreConfigClass.Key.DownloadPath, Download.DownloadPath, CoreConfigClass.Group.Download);
                        Growl.Success("录制储存文件夹设置成功");
                        CoreConfigFile.WriteConfigFile(true);
                    }
                    else
                    {
                        Growl.Warning("设置的录制储存文件夹路径文件夹不存在，设置失败！");
                    }
                }
                else
                {
                    RecPathTextBox.Text = Download.DownloadPath;
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
                        Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, "临时文件文件夹设置成功：" + Download.TmpPath, false, null, false);
                        CoreConfigFile.WriteConfigFile(true);
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
            if (!File.Exists("./plugins/ffmpeg/ffmpeg.exe"))
            {
                Growl.Warning($"缺少对应的转码组件！");
                return;
            }
            bool IsTranscod = (bool)TranscodToggle.IsChecked ? true : false;
            DDTV_Core.Tool.TranscodModule.Transcod.IsAutoTranscod = IsTranscod;
            CoreConfig.SetValue(CoreConfigClass.Key.IsAutoTranscod, IsTranscod.ToString(), CoreConfigClass.Group.Core);
            Growl.Success((IsTranscod ? "打开" : "关闭") + "自动转码成功");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (IsTranscod ? "打开" : "关闭") + "自动转码成功", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
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
                string Message = "修改录制默认分辨率为" + (Download.RecQuality == 10000 ? "原画" : Download.RecQuality == 400 ? "蓝光" : Download.RecQuality == 250 ? "超清" : Download.RecQuality == 150 ? "高清" : "流畅");
                Growl.Success(Message);
                Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, Message, false, null, false);
                CoreConfigFile.WriteConfigFile(true);
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
                    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, "已" + (!YIsRecDanmu ? "打开" : "关闭") + $"[{name}({roomid})]的弹幕录制", false, null, false);
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
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "弹幕录制总开关", false, null, false);
            if (Is)
            {
                //GiftToggle.IsEnabled = true;
                //GuardToggle.IsEnabled = true;
                //SCToggle.IsEnabled = true;
                //SelectDanMu_v1.IsEnabled = false;
                // SelectDanMu_v2.IsEnabled = true;
            }
            else
            {
                //GiftToggle.IsEnabled = false;
                //GuardToggle.IsEnabled = false;
                //SCToggle.IsEnabled = false;
                //SelectDanMu_v1.IsEnabled = false;
                //SelectDanMu_v2.IsEnabled = false;
            }
            Download.IsRecGift = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecGift, Is.ToString(), CoreConfigClass.Group.Download);
            //Growl.Success((Is ? "打开" : "关闭") + "礼物录制");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "礼物录制", false, null, false);

            Download.IsRecGuard = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecGuard, Is.ToString(), CoreConfigClass.Group.Download);
            //Growl.Success((Is ? "打开" : "关闭") + "上舰录制");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "上舰录制", false, null, false);

            Download.IsRecSC = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecSC, Is.ToString(), CoreConfigClass.Group.Download);
            //Growl.Success((Is ? "打开" : "关闭") + "SC录制");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "SC录制", false, null, false);
            CoreConfigFile.WriteConfigFile(true);

            if (Download.IsRecDanmu)
            {
                DanmuToAssText.Visibility = Visibility.Visible;
                DanmuToAssButton.Visibility = Visibility.Visible;
                if (GUIConfig.IsXmlToAss)
                {
                    SetDanmakuFactoryParameter.Visibility = Visibility.Visible;
                    SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Visible;
                }
                else
                {
                    SetDanmakuFactoryParameter.Visibility = Visibility.Collapsed;
                    SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                DanmuToAssText.Visibility = Visibility.Collapsed;
                DanmuToAssButton.Visibility = Visibility.Collapsed;
                SetDanmakuFactoryParameter.Visibility = Visibility.Collapsed;
                SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Collapsed;
            }
        }

        //private void GiftToggle_Click(object sender, RoutedEventArgs e)
        //{
        //    bool Is = (bool)GiftToggle.IsChecked ? true : false;
        //    Download.IsRecGift = Is;
        //    CoreConfig.SetValue(CoreConfigClass.Key.IsRecGift, Is.ToString(), CoreConfigClass.Group.Download);
        //    Growl.Success((Is ? "打开" : "关闭") + "礼物录制");
        //    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "礼物录制", false, null, false);
        //}

        //private void GuardToggle_Click(object sender, RoutedEventArgs e)
        //{
        //    bool Is = (bool)GuardToggle.IsChecked ? true : false;
        //    Download.IsRecGuard = Is;
        //    CoreConfig.SetValue(CoreConfigClass.Key.IsRecGuard, Is.ToString(), CoreConfigClass.Group.Download);
        //    Growl.Success((Is ? "打开" : "关闭") + "上舰录制");
        //    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "上舰录制", false, null, false);
        //}

        //private void SCToggle_Click(object sender, RoutedEventArgs e)
        //{
        //    bool Is = (bool)SCToggle.IsChecked ? true : false;
        //    Download.IsRecSC = Is;
        //    CoreConfig.SetValue(CoreConfigClass.Key.IsRecSC, Is.ToString(), CoreConfigClass.Group.Download);
        //    Growl.Success((Is ? "打开" : "关闭") + "SC录制");
        //    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "SC录制", false, null, false);
        //}

        private void IsFlvSplitToggle_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)IsFlvSplitToggle.IsChecked ? true : false;
            Download.IsFlvSplit = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsFlvSplit, Is.ToString(), CoreConfigClass.Group.Download);
            Growl.Success((Is ? "打开" : "关闭") + "启用录制文件大小限制(自动分割)");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "启用录制文件大小限制(自动分割)", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
            if (Is)
            {
                FlvSplitSizeComboBox.Visibility = Visibility.Visible;
                TranscodToggle.IsEnabled = false;
                DanmuToggle.IsEnabled = false;
                //GiftToggle.IsEnabled= false;
                //GuardToggle.IsEnabled = false;
                //SCToggle.IsEnabled = false;
            }
            else
            {
                FlvSplitSizeComboBox.Visibility = Visibility.Collapsed;
                TranscodToggle.IsEnabled = true;
                DanmuToggle.IsEnabled = true;
                //GiftToggle.IsEnabled = true;
                //GuardToggle.IsEnabled = true;
                //SCToggle.IsEnabled = true;
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
                    case 9:
                        Download.FlvSplitSize = 10485760;
                        break;
                }
                CoreConfig.SetValue(CoreConfigClass.Key.FlvSplitSize, Download.FlvSplitSize.ToString(), CoreConfigClass.Group.Download);
                string Message = "修改文件大小限制为" + (Download.FlvSplitSize == 10485760 ? "10MB" : Download.FlvSplitSize == 8482560409 ? "7.9GB" : Download.FlvSplitSize == 6335076761 ? "5.9GB" : Download.FlvSplitSize == 2040109465 ? "1.9GB" : Download.FlvSplitSize == 5368709120 ? "5GB" : Download.FlvSplitSize == 4294967296 ? "4GB" : Download.FlvSplitSize == 3221225472 ? "3GB" : Download.FlvSplitSize == 2147483648 ? "2GB" : "1GB");
                Growl.Success(Message);
                Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, Message, false, null, false);
                CoreConfigFile.WriteConfigFile(true);
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
                        GUIConfig.PlayQuality = 10000;
                        break;
                    case 2:
                        GUIConfig.PlayQuality = 400;
                        break;
                    case 3:
                        GUIConfig.PlayQuality = 250;
                        break;
                    case 4:
                        GUIConfig.PlayQuality = 150;
                        break;
                    case 5:
                        GUIConfig.PlayQuality = 80;
                        break;
                }
                CoreConfig.SetValue(CoreConfigClass.Key.PlayQuality, GUIConfig.PlayQuality.ToString(), CoreConfigClass.Group.Play);
                string Message = "修改默认在线观看画质为" + (GUIConfig.PlayQuality == 10000 ? "原画" : GUIConfig.PlayQuality == 400 ? "蓝光" : GUIConfig.PlayQuality == 250 ? "超清" : GUIConfig.PlayQuality == 150 ? "高清" : "流畅");
                Growl.Success(Message);
                Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, Message, false, null, false);
                CoreConfigFile.WriteConfigFile(true);
            }
        }

        /// <summary>
        /// 下载窗口鼠标右键菜单_激光切片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecList_MenuItem_Clip_Click(object sender, RoutedEventArgs e)
        {
            int Index = RecList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.recList.Count > Index)
            {
                string filePath = UpdateInterface.Main.recList[Index].FilePath;
                long uid = UpdateInterface.Main.recList[Index].Uid;

                if (Rooms.RoomInfo.TryGetValue(uid, out var roomInfo))
                {
                    if (roomInfo.DownloadingList.Count > 0)
                    {
                        FileInfo[] fileInfos = new FileInfo[roomInfo.DownloadingList.Count];
                        for (int i = 0; i < roomInfo.DownloadingList.Count; i++)
                        {
                            if (roomInfo.DownloadingList[i].FlvSplit)
                            {
                                Growl.WarningGlobal("该任务已启动自动分P功能，无法使用激光切片");
                                return;
                            }
                            //if (roomInfo.DownloadingList[i].IsHLS)
                            //{
                            //    Growl.WarningGlobal("该任务为HLS流，无法使用激光切片，如需使用该功能请加群联系我魔改该功能");
                            //    return;
                            //}
                            fileInfos[i] = new FileInfo(roomInfo.DownloadingList[i].FileName);
                        }
                        if (fileInfos.Length == 1)
                        {
                            NewClipWindow newClipWindow = new(fileInfos[0].FullName);
                            newClipWindow.Show();
                        }
                        else
                        {
                            CuttingDialogDispose += MainWindow_CuttingDialogDispose;
                            SelectCuttingFileDialog selectCuttingFileDialog = new SelectCuttingFileDialog(fileInfos, CuttingDialogDispose, roomInfo);
                            ClipDialog = Dialog.Show(selectCuttingFileDialog);
                        }
                    }
                }
            }
        }

        private void MainWindow_CuttingDialogDispose(object? sender, EventArgs e)
        {
            ClipDialog.Dispatcher.BeginInvoke(new Action(() => ClipDialog.Close()));
        }


        /// <summary>
        /// 录制任务_右键菜单_打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecList_CopyFolderPath_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int Index = RecList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.recList.Count > Index)
            {
                string filePath = UpdateInterface.Main.recList[Index].FilePath;
                if (Directory.Exists(filePath))
                {
                    //Environment.CurrentDirectory
                    if (filePath.Length > 2 && filePath[..1] == ".")
                    {
                        filePath = Environment.CurrentDirectory + filePath.Substring(1, filePath.Length - 1);
                    }
                    //System.Diagnostics.Process.Start(filePath);
                    Clipboard.SetDataObject(filePath);
                    Growl.Success("已复制路径到粘贴板");
                }
                else
                {
                    Growl.Warning($"该文件路径不存在，复制失败");
                }
            }
        }

        private void DDTV_UPDATE_Button_Click(object sender, RoutedEventArgs e)
        {
            DDTV_Core.Tool.DDTV_Update.CheckUpdateProgram(true);
            MessageBoxResult dr = MessageBox.Show($"确定要开始更新DDTV吗？\n确定后会结束DDTV全部任务并退出DDTV开始更新", "更新DDTV", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"用户点击自动更新", false, null, false);
                Update(false);
            }
        }

        /// <summary>
        /// 更新DDTV
        /// </summary>
        public void Update(bool IsAuto)
        {
            if (File.Exists("./DDTV_Update.exe"))
            {
                Process process = new Process();
                process.StartInfo.FileName = "./DDTV_Update.exe";
                if (IsAuto)
                {
                    process.StartInfo.Arguments += " autoupdate";

                }
                if (CoreConfig.IsDev)
                {
                    process.StartInfo.Arguments += " dev";
                }
                process.Start();
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    Application.Current.Shutdown();
                }));

            }
            else
            {
                Growl.Error($"找不到自动更新脚本程序DDTV_Update.exe");
                Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"找不到自动更新脚本程序DDTV_Update.exe", false, null, false);
            }
        }

        /// <summary>
        /// 根据关注列表对比导入V
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_GetFollow(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show($"确定要导入关注列表中的VUP/VTB的房间信息么？\n该功能基于缓存VTBS的数据与登陆账号的关注列表交叉比对，可能会有缺少需要手动补充", "导入列表", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                if (File.Exists("./DDTV_Update.exe"))
                {
                    ImportVTBButton.Text = "导入中...请稍候...";
                    Task.Run(() =>
                    {
                        int AddConut = DDTV_Core.SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid)).Count;
                        Growl.Success($"成功导入{AddConut}个关注列表中的V到配置");
                        Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"成功导入{AddConut}个关注列表中的V到配置", false, null, false);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            ImportVTBButton.Text = "导入完成";
                        }));

                    });
                }
            }

        }

        private void HideIcon_Click(object sender, RoutedEventArgs e)
        {
            HideIconState = (bool)HideIcon.IsChecked ? true : false;

            CoreConfig.SetValue(CoreConfigClass.Key.HideIconState, HideIconState.ToString(), CoreConfigClass.Group.GUI);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void LiveList_Play_MouseDouble_Click(object sender, MouseButtonEventArgs e)
        {
            Task.Run(() => Play_Click());
        }

        private void DoNotSleepWhileDownloadingIcon_Click(object sender, RoutedEventArgs e)
        {
            DDTV_Core.Tool.Dokidoki.IsDoNotSleepState = (bool)DoNotSleepWhileDownloadingIcon.IsChecked ? true : false;

            CoreConfig.SetValue(CoreConfigClass.Key.DoNotSleepWhileDownloading, DDTV_Core.Tool.Dokidoki.IsDoNotSleepState.ToString(), CoreConfigClass.Group.Download);
            CoreConfigFile.WriteConfigFile(true);
        }

        /// <summary>
        /// 选择新的录制存放路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Select_Save_Path_Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;//设置为选择文件夹
            dialog.ShowDialog();
            string path = string.Empty;

            try
            {
                path = dialog.FileName;
            }
            catch (Exception)
            {
                return;
            }



            if (Download.DownloadPath != path)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (Directory.Exists(path))
                    {
                        RecPathTextBox.Text = path;
                        Download.DownloadPath = path;
                        CoreConfig.SetValue(CoreConfigClass.Key.DownloadPath, Download.DownloadPath, CoreConfigClass.Group.Download);
                        Growl.Success("录制储存文件夹设置成功");
                        Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, "录制储存文件夹设置成功：" + Download.DownloadPath, false, null, false);
                        CoreConfigFile.WriteConfigFile(true);
                    }
                    else
                    {
                        Growl.Warning("设置的录制储存文件夹路径文件夹不存在，设置失败！");
                    }
                }
                else
                {
                    RecPathTextBox.Text = Download.DownloadPath;
                    Growl.Warning("录制储存文件夹不能为空");
                }
            }
        }
        /// <summary>
        /// 选择新的缓存文件存放路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Select_Tmp_Path_Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;//设置为选择文件夹
            dialog.ShowDialog();

            string path = string.Empty;

            try
            {
                path = dialog.FileName;
            }
            catch (Exception)
            {
                return;
            }

            if (Download.TmpPath != path)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (Directory.Exists(path))
                    {
                        TmpPathTextBox.Text = path;
                        Download.TmpPath = path;
                        CoreConfig.SetValue(CoreConfigClass.Key.TmpPath, Download.TmpPath, CoreConfigClass.Group.Download);
                        Growl.Success("临时文件文件夹设置成功");
                        Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, "临时文件文件夹设置成功：" + Download.TmpPath, false, null, false);
                        CoreConfigFile.WriteConfigFile(true);
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

        //private void ForceCDNResolution_Click(object sender, RoutedEventArgs e)
        //{
        //    bool Is = (bool)ForceCDNResolution.IsChecked ? true : false;
        //    RoomInfo.ForceCDNResolution = Is;
        //    CoreConfig.SetValue(CoreConfigClass.Key.ForceCDNResolution, Is.ToString(), CoreConfigClass.Group.Download);
        //    Growl.Success((Is ? "打开" : "关闭") + "强制主CDN开关");
        //    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "强制主CDN开关", false, null, false);
        //    CoreConfigFile.WriteConfigFile(true);
        //}

        //private void TranscodingCompleteAutoDeleteFiles_Click(object sender, RoutedEventArgs e)
        //{
        //    bool Is = (bool)TranscodingCompleteAutoDeleteFiles.IsChecked ? true : false;
        //    Transcod.TranscodingCompleteAutoDeleteFiles = Is;
        //    CoreConfig.SetValue(CoreConfigClass.Key.TranscodingCompleteAutoDeleteFiles, Is.ToString(), CoreConfigClass.Group.Core);
        //    Growl.Success((Is ? "打开" : "关闭") + "转码完成后自动删除原始FLV文件");
        //    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "转码完成后自动删除原始FLV文件", false, null, false);
        //    CoreConfigFile.WriteConfigFile(true);
        //}

        private void DDcenterSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)DDcenterSwitch.IsChecked ? true : false;
            DDcenter.DDcenterSwitch = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.DDcenterSwitch, Is.ToString(), CoreConfigClass.Group.Core);
            Growl.Success((Is ? "打开" : "关闭") + "DDC数据采集开关");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "DDC数据采集开关", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private static Task ManualTranscodingTask = null;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool IsTranscoding = false;
        /// <summary>
        /// 手动修复/转码选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_TranscodingSelectFilesManual(object sender, RoutedEventArgs e)
        {
            if (!IsTranscoding)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok && File.Exists(dialog.FileName))
                {
                    FileInfo fileInfo = new FileInfo(dialog.FileName);

                    if (fileInfo.Extension.ToLower() != ".flv" && fileInfo.Extension.ToLower() != ".mp4")
                    {
                        MessageBox.Show("选择的文件不是DDTV录制的FVL或mp4文件！");
                        return;
                    }
                    bool IsMp4File = fileInfo.Extension.ToLower() == ".mp4" ? true : false;
                    ManualTranscodingProgress.Text = $"后台正在进行文件转码:{fileInfo.Name}";
                    IsTranscoding = true;
                    TranscodingSelectFilesManual.Content = "修复/转码中";
                    Task.Factory.StartNew(() =>
                    {
                        if (!Transcod.CallFFMPEG(new TranscodClass()
                        {
                            AfterFilenameExtension = ".mp4",
                            AfterFilePath = IsMp4File ? dialog.FileName.Replace("\\", "/").Replace(".mp4", "_fix.mp4") : dialog.FileName.Replace("\\", "/").Replace(".flv", "_fix.mp4"),
                            BeforeFilePath = dialog.FileName.Replace("\\", "/"),
                        }, false, true).IsTranscod)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                ManualTranscodingProgress.Text = $"手动文件修复/转码完成，输出文件到原始文件路径";
                                IsTranscoding = false;
                                TranscodingSelectFilesManual.Content = "选择文件";
                            }));
                        }

                    }, cancellationTokenSource.Token);
                }
            }
            //else
            //{
            //    IsTranscoding = false;
            //    cancellationTokenSource.Cancel();
            //    ManualTranscodingProgress.Text = $"手动已经取消转码操作";
            //    TranscodingSelectFilesManual.Content = "选择文件";
            //}
        }

        private void SpaceIsInsufficientWarn_Click(object sender, RoutedEventArgs e)
        {
            FileOperation.SpaceIsInsufficientWarn = (bool)SpaceIsInsufficientWarn.IsChecked ? true : false;

            CoreConfig.SetValue(CoreConfigClass.Key.SpaceIsInsufficientWarn, FileOperation.SpaceIsInsufficientWarn.ToString(), CoreConfigClass.Group.Core);
            CoreConfigFile.WriteConfigFile(true);
        }

        /// <summary>
        /// 直播监控列表强制刷新按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForceRefreshLiveBroadcastList_Click(object sender, RoutedEventArgs e)
        {
            List<BindingData.LiveList> _ = new();
            foreach (var item in Rooms.RoomInfo)
            {
                BindingData.LiveList live = new(item.Value.uname,
                    item.Value.live_status == 1 ? "直播中" : " 摸了",
                    item.Value.IsRemind ? "     √" : "      ",
                    item.Value.IsAutoRec ? "     √" : "      ",
                    item.Value.room_id,
                    item.Value.uid,
                    item.Value.live_status,
                    item.Value.IsRecDanmu ? "     √" : "      ",
                    item.Value.title);
                _.Add(live);
            }
            LiveList.ItemsSource = _;
            UpdateInterface.Main.ActivationInterface = 1;
        }

        private void SaveReplaceAPI_Click(object sender, RoutedEventArgs e)
        {
            //DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.ReplaceAPI
            MessageBoxResult dr = MessageBox.Show("确认后会修改API请求所使用的域名，如果是第三方API代理无法访问或者错误，DDTV的录制功能将会出现意料外的错误\r\n确认修改吗？", "确定修改API请求域名吗", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                string APIURL = ReplaceAPIText.Text.ToLower();
                if (string.IsNullOrEmpty(APIURL))
                {
                    APIURL = "https://api.live.bilibili.com";
                    ReplaceAPIText.Text = APIURL;
                }
                if (APIURL.Substring(0, 4) != "http" || APIURL.Substring(APIURL.Length - 1, 1) == "/" || APIURL.Substring(APIURL.Length - 1, 1) == @"\")
                {
                    MessageBox.Show("地址有误！\r\n请确保输入的API使用的域名为http地址并不以斜杠结尾\r\n如：https://api.live.bilibili.com");
                    return;
                }
                else
                {
                    CoreConfig.ReplaceAPI = APIURL;
                    CoreConfig.SetValue(CoreConfigClass.Key.ReplaceAPI, APIURL, CoreConfigClass.Group.Core);
                    Growl.Success($"修改API代理为{APIURL}");
                    Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"修改API代理为{APIURL}", false, null, false);
                    CoreConfigFile.WriteConfigFile(true);
                }
            }
        }

        private void SelectAPI_v1_Click(object sender, RoutedEventArgs e)
        {
            CoreConfig.APIVersion = 1;
            CoreConfig.SetValue(CoreConfigClass.Key.APIVersion, CoreConfig.APIVersion.ToString(), CoreConfigClass.Group.Core);
            Growl.Success($"修改API版本为v{CoreConfig.APIVersion}");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"修改API版本为v{CoreConfig.APIVersion}", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void SelectAPI_v2_Click(object sender, RoutedEventArgs e)
        {
            CoreConfig.APIVersion = 2;
            CoreConfig.SetValue(CoreConfigClass.Key.APIVersion, CoreConfig.APIVersion.ToString(), CoreConfigClass.Group.Core);
            Growl.Success($"修改API版本为v{CoreConfig.APIVersion}");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"修改API版本为v{CoreConfig.APIVersion}", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        /// <summary>
        /// 注销阿B账号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            //BilibiliUserConfig.account = new();
            BilibiliUserConfig.account.cookie = "";
            BilibiliUserConfig.WritUserFile();
            BilibiliUserConfig.CheckAccount.CheckLoginValidity();
        }

        /// <summary>
        /// 自动更新开关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoUpdateSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)AutoUpdateSwitch.IsChecked ? true : false;
            CoreConfig.AutoInsallUpdate = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.AutoInsallUpdate, Is.ToString(), CoreConfigClass.Group.Core);
            Growl.Success((Is ? "打开" : "关闭") + "DDTV自动更新");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "DDTV自动更新", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void ProxySwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)ProxySwitch.IsChecked ? true : false;
            CoreConfig.WhetherToEnableProxy = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.WhetherToEnableProxy, Is.ToString(), CoreConfigClass.Group.Core);
            Growl.Success((Is ? "使用" : "不使用") + "系统代理");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "使用" : "不使用") + "系统代理", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void DevSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)DevSwitch.IsChecked ? true : false;
            CoreConfig.IsDev = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsDev, Is.ToString(), CoreConfigClass.Group.Core);
            Growl.Success((Is ? "打开" : "关闭") + "开发版本更新监听");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "开发版本更新监听", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void SelectDanMu_v2_Click(object sender, RoutedEventArgs e)
        {
            CoreConfig.DanMuSaveType = 2;
            CoreConfig.SetValue(CoreConfigClass.Key.DanMuSaveType, CoreConfig.DanMuSaveType.ToString(), CoreConfigClass.Group.Core);
            Growl.Success($"修改弹幕用户信息储存类型为原始数据模式");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"修改弹幕用户信息储存类型为原始数据模式", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void SelectDanMu_v1_Click(object sender, RoutedEventArgs e)
        {
            CoreConfig.DanMuSaveType = 1;
            CoreConfig.SetValue(CoreConfigClass.Key.DanMuSaveType, CoreConfig.DanMuSaveType.ToString(), CoreConfigClass.Group.Core);
            Growl.Success($"修改弹幕用户信息储存类型为兼容模式");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"修改弹幕用户信息储存类型为兼容模式", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void DanMuWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (DanMuWindow == null)
            {
                OpenDanMuWindowDialogDispose += MainWindow_OpenDanMuWindowDialogDispose;
                OpenDanMuWindowDialog openDanMuWindowDialog = new OpenDanMuWindowDialog(OpenDanMuWindowDialogDispose);
                OpenDanMuWindowDialog = Dialog.Show(openDanMuWindowDialog);
            }
            else
            {
                try
                {
                    DanMuWindow.Close();
                }
                catch (Exception)
                { }
                DanMuWindow = null;
            }
        }

        private void MainWindow_OpenDanMuWindowDialogDispose(object? sender, EventArgs e)
        {
            OpenDanMuWindowDialog.Dispatcher.BeginInvoke(new Action(() => OpenDanMuWindowDialog.Close()));
        }

        /// <summary>
        /// 监控列表_右键菜单_打开临时监控窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_Peep_Click(object sender, RoutedEventArgs e)
        {
            Dialog.Show(new AddRoomDialog(true));
        }

        private void LiveList_MenuItem_ManualRec_Click(object sender, RoutedEventArgs e)
        {
            int Index = LiveList.SelectedIndex;
            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                string name = UpdateInterface.Main.liveList[Index].Name;
                int roomid = UpdateInterface.Main.liveList[Index].RoomId;
                string Title = UpdateInterface.Main.liveList[Index].Title;
                Log.AddLog(nameof(RoomPatrol), LogClass.LogType.Info, $"【手动录制】用户手动开始录制【{roomid}-{name}】的直播流-标题：[{Title}]");
                Download.AddVideoDownloadTaskd(uid, true);
                Growl.SuccessGlobal($"增加手动录制任务:【{roomid}-{name}】");
            }
        }

        /// <summary>
        /// 主窗口大小改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlowWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Width - 130 > 0)
            {
                Window_RoomCard.Width = this.Width - 134;
                Window_RoomCard.Groups = (int)Window_RoomCard.Width / 212;
            }
        }

        private void HLSToggle_Click(object sender, RoutedEventArgs e)
        {
            bool IsHLS = (bool)HLSToggle.IsChecked ? true : false;
            DDTV_Core.SystemAssembly.DownloadModule.Download.IsHls = IsHLS;
            CoreConfig.SetValue(CoreConfigClass.Key.IsHls, IsHLS.ToString(), CoreConfigClass.Group.Download);
            Growl.Success((IsHLS ? "打开" : "关闭") + "HLS优先");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (IsHLS ? "打开" : "关闭") + "HLS优先", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        /// <summary>
        /// HLS流等待时间设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitHLSTime_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {

            if (InitDDTV_Core.WhetherInitializationIsComplet && e.Info >= 10)
            {
                DDTV_Core.SystemAssembly.DownloadModule.Download.WaitHLSTime = (int)e.Info;
                CoreConfig.SetValue(CoreConfigClass.Key.WaitHLSTime, ((int)e.Info).ToString(), CoreConfigClass.Group.Download);
                Growl.Success($"设置HLS等待时间为{(int)e.Info}秒");
                Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, $"设置HLS等待时间为{(int)e.Info}秒", false, null, false);
                CoreConfigFile.WriteConfigFile(true);
            }
            else
            {
                Growl.Warning($"设置HLS等待时间最低需要等待10秒");
            }
        }

        private void OpenSeparateDanMuWindow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int Index = 0;
            this.Dispatcher.Invoke(new Action(() =>
            {
                Index = LiveList.SelectedIndex;
            }));

            if (Index > -1 && UpdateInterface.Main.liveList.Count > Index)
            {
                long uid = UpdateInterface.Main.liveList[Index].Uid;
                string name = UpdateInterface.Main.liveList[Index].Name;
                if (UpdateInterface.Main.liveList[Index].LiveState == 1)
                {
                    ShowDanMuWindow danMuShowWindow = new ShowDanMuWindow(uid);
                    danMuShowWindow.Show();
                }
                else
                {
                    Growl.Warning($"该房间未开播");
                }
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //NewClipWindow newClipWindow = new(@"E:\老D\PR项目\DDTV宣传\成品片段\压制完成.mp4");
            //newClipWindow.Show();
        }

        private void SetDanmakuFactoryParameterSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SetDanmakuFactoryParameter.Text))
            {
                Growl.WarningGlobal($"转ASS参数不能为空");
                return;
            }
            GUIConfig.DanmukuFactoryParameter = SetDanmakuFactoryParameter.Text;
            CoreConfigFile.WriteConfigFile(true);
        }

        private void DanmuToAssButton_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)DanmuToAssButton.IsChecked ? true : false;
            GUIConfig.IsXmlToAss = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.IsXmlToAss, Is.ToString(), CoreConfigClass.Group.GUI);
            Growl.Success((Is ? "打开" : "关闭") + "录制后转为ass文件");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "录制后转为ass文件", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
            if (Is)
            {
                SetDanmakuFactoryParameter.Visibility = Visibility.Visible;
                SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Visible;
            }
            else
            {
                SetDanmakuFactoryParameter.Visibility = Visibility.Collapsed;
                SetDanmakuFactoryParameterSaveButton.Visibility = Visibility.Collapsed;
            }
        }

        private void DoesShieldTakeEffectSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Is = (bool)DoesShieldTakeEffectSwitch.IsChecked ? true : false;
            GUIConfig.DoesShieldTakeEffect = Is;
            CoreConfig.SetValue(CoreConfigClass.Key.DoesShieldTakeEffect, Is.ToString(), CoreConfigClass.Group.Play);
            Growl.Success((Is ? "打开" : "关闭") + "云屏蔽和进房提示");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Is ? "打开" : "关闭") + "云屏蔽和进房提示", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        /// <summary>
        /// 触发监控列表的搜索框
        /// </summary>
        private void SearchLiveList()
        {
            SearchDialogDispose += MainWindow_SearchDialogDispose;
            SearchListDialog SearchListDialog;

            this.Dispatcher.Invoke(new Action(() =>
            {
                SearchListDialog = new SearchListDialog(SearchDialogDispose);
                SearchDialog = Dialog.Show(SearchListDialog);
            }));
        }

        /// <summary>
        /// 监控列表搜索事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                SearchLiveList();
            }
        }
        public static int SearchIndex = 0;//返回的索引结果
        /// <summary>
        /// 搜索结束事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SearchDialogDispose(object? sender, EventArgs e)
        {
            SearchDialog.Dispatcher.BeginInvoke(new Action(() => SearchDialog.Close()));

            LiveList.ScrollIntoView(LiveList.Items[SearchIndex]);
            LiveList.SelectedIndex = SearchIndex;

            Task.Run(() =>
            {
                Thread.Sleep(100);
                this.Dispatcher.Invoke(new Action(() =>
               {
                   LiveList.Focus();
               }));

            });
        }
        /// <summary>
        /// 右键菜单_搜索列表中的房间_事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LiveList_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            SearchLiveList();
        }

        private void AssFile_SelectFilesManual_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.DefaultExtension = "xml";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && File.Exists(dialog.FileName))
            {
                FileInfo fileInfo = new FileInfo(dialog.FileName);

                if (fileInfo.Extension.ToLower() != ".xml")
                {
                    MessageBox.Show("选择的文件不是DDTV录制的csv文件！");
                    return;
                }
                Task.Factory.StartNew(() =>
                {
                   DDTV_Core.Tool.DanMuKu.DanMuKuRec.CallDanmakuFactory(fileInfo.DirectoryName+"/", fileInfo.Name.Replace(".xml", "_fix.ass"), fileInfo.Name);
                }, cancellationTokenSource.Token);
            }

        }
    }
}
