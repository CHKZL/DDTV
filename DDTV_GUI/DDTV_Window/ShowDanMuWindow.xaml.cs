using DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu;
using DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.Tool.DanMuKu;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// ShowDanMuWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShowDanMuWindow : GlowWindow
    {
        private List<DanmuInfo> DanMuList = new List<DanmuInfo>();
        private RoomInfoClass.RoomInfo _roominfo = new RoomInfoClass.RoomInfo();
        private DanMu.UserLiveInfo userLiveInfo = new();//屏蔽信息
        private int WordLimit = 20;
        private DispatcherTimer timer;

        public ShowDanMuWindow(long UID)
        {
            InitializeComponent();
            ShowDanMuSwitch.IsChecked = GUIConfig.ShowDanMuSwitch;
            ShowGiftSwitch.IsChecked = GUIConfig.ShowGiftSwitch;
            ShowSCSwitch.IsChecked = GUIConfig.ShowSCSwitch;
            ShowGuardSwitch.IsChecked = GUIConfig.ShowGuardSwitch;
            Loaded += new RoutedEventHandler(Topping);
            if (Rooms.RoomInfo.TryGetValue(UID, out var roomInfo))
            {

                _roominfo.room_id = roomInfo.room_id;
                _roominfo.uid = roomInfo.uid;
                _roominfo.uname = roomInfo.uname;
                DanMuLog.ItemsSource = DanMuList;
                if (GUIConfig.DoesShieldTakeEffect)
                {
                    userLiveInfo = DanMu.GetShieldList(_roominfo.room_id);
                }

                if (userLiveInfo.data.user_level.level >= 20)
                {
                    WordLimit = 30;
                }
                else
                {
                    WordLimit = 20;
                }
                Add($"尝试和弹幕服务器连接(RoomID:{roomInfo.room_id})");
                Rec(_roominfo.uid != 0 ? _roominfo.uid : long.Parse(DDTV_Core.SystemAssembly.ConfigModule.BilibiliUserConfig.account.uid));
            }
            else
            {
                Add($"处理失败，该房间不在DDTV监控列表中");
                Add($"请先添加到监控列表");
            }
        }
        void Topping(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer1_Tick;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Topmost = true;
        }
        private void Rec(long Uid)
        {
            MainWindow.linkDMNum++;
            Task.Run(() =>
            {


                _roominfo = DanMuKuRec.StartRecDanmu(Uid, true);
                Dispatcher.Invoke(() =>
                {
                    this.Title = $"{_roominfo.uname} - DanMuChat";
                });

                _roominfo.roomWebSocket.LiveChatListener.DisposeSent += LiveChatListener_DisposeSent;
                _roominfo.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                if (_roominfo.roomWebSocket.LiveChatListener.m_ReceiveBuffer != null)
                {
                    Add($"成功和弹幕服务器连接");
                    Add($"等待新的弹幕信息...");
                }
            });

        }
        private void Add(string Text)
        {
            DanMuLog.Dispatcher.Invoke(() =>
            {
                DanMuList.Add(new DanmuInfo() { Text = Text });
                DanMuLog.Items.Refresh();
                if (DanMuList.Count > 50)
                {
                    DanMuList.RemoveAt(0);
                }
                DanMuLog.ScrollIntoView(DanMuLog.Items[DanMuLog.Items.Count - 1]);
            });
        }

        private void LiveChatListener_DisposeSent(object? sender, EventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            try
            {
                MainWindow.linkDMNum--;
                if (!liveChatListener.IsUserDispose)
                {
                    Add("与弹幕服务器断开连接，正在重连....");
                    Log.AddLog(nameof(DDTV_DanMu), LogClass.LogType.Info, $"{liveChatListener.TroomId}直播间弹幕连接中断，开始重连弹幕服务器");
                    Rec(liveChatListener.mid);
                }
            }
            catch (Exception)
            {
            }
        }

        private void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;

            string text = string.Empty;
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {
                        if (GUIConfig.ShowDanMuSwitch)
                            text = $"[弹幕]{Danmu.UserName}：{Danmu.Message}";
                    }
                    break;
                case SuperchatEventArg SuperchatEvent:
                    {
                        if (GUIConfig.ShowSCSwitch)
                            text = $"[SC](金额{SuperchatEvent.Price}){SuperchatEvent.UserName}：{SuperchatEvent.Message}";
                    }
                    break;
                case GuardBuyEventArgs GuardBuyEvent:
                    {
                        if (GUIConfig.ShowGuardSwitch)
                            text = $"[上舰]{GuardBuyEvent.UserName}：{GuardBuyEvent.Number}个月的{(GuardBuyEvent.GuardLevel == 1 ? "总督" : GuardBuyEvent.GuardLevel == 2 ? "提督" : "舰长")}";
                    }
                    break;
                case SendGiftEventArgs sendGiftEventArgs:
                    {
                        if (GUIConfig.ShowGiftSwitch)
                            text = $"[礼物]{sendGiftEventArgs.UserName}：送了{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}";
                    }
                    break;
                case WarningEventArg warningEventArg:
                    {
                        text = $"[管理员警告]警告内容: {warningEventArg.msg}";
                    }
                    break;
                case CutOffEventArg cutOffEventArg:
                    {
                        text = $"[直播被切断]系统消息: {cutOffEventArg.msg}";
                    }
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(text))
            {
                Add(text);
            }
        }


        private class DanmuInfo
        {
            public string Text { get; set; }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            if (_roominfo.roomWebSocket.LiveChatListener != null && _roominfo.roomWebSocket.LiveChatListener.startIn)
            {
                MainWindow.linkDMNum--;
                try
                {
                    Log.AddLog(nameof(PlayWindow), LogClass.LogType.Info, $"弹幕窗口关闭，断开弹幕连接", false);
                    _roominfo.roomWebSocket.LiveChatListener.startIn = false;
                    _roominfo.roomWebSocket.IsConnect = false;
                    _roominfo.roomWebSocket.LiveChatListener.IsUserDispose = true;
                    _roominfo.roomWebSocket.LiveChatListener.Dispose();
                }
                catch (Exception)
                {

                }
            }
        }

        private void DanMuInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string Massage = DanMuInput.Text;
                if (Massage.Length > WordLimit)
                {
                    Growl.Warning($"发送的弹幕长度尝过限制(您当前的直播等级只能发送{WordLimit}个字内的弹幕信息)");
                    return;
                }
                else if (Massage.Length < 1)
                {
                    Growl.Warning("发送的弹幕内容不能为空");
                    return;
                }
                DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu.DanMu.Send(Rooms.GetValue(_roominfo.uid, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.room_id), Massage);
                DanMuInput.Text = "";
            }
        }

        private void ShowDanMuSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Checked = (bool)ShowDanMuSwitch.IsChecked;
            GUIConfig.ShowDanMuSwitch = Checked;
            CoreConfig.SetValue(CoreConfigClass.Key.ShowDanMuSwitch, Checked.ToString(), CoreConfigClass.Group.GUI);
            Growl.Success((Checked ? "打开" : "关闭") + "弹幕窗弹幕信息");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Checked ? "打开" : "关闭") + "弹幕窗弹幕信息", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void ShowGiftSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Checked = (bool)ShowGiftSwitch.IsChecked;
            GUIConfig.ShowGiftSwitch = Checked;
            CoreConfig.SetValue(CoreConfigClass.Key.ShowGiftSwitch, Checked.ToString(), CoreConfigClass.Group.GUI);
            Growl.Success((Checked ? "打开" : "关闭") + "弹幕窗礼物信息");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Checked ? "打开" : "关闭") + "弹幕窗礼物信息", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void ShowSCSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Checked = (bool)ShowSCSwitch.IsChecked;
            GUIConfig.ShowSCSwitch = Checked;
            CoreConfig.SetValue(CoreConfigClass.Key.ShowSCSwitch, Checked.ToString(), CoreConfigClass.Group.GUI);
            Growl.Success((Checked ? "打开" : "关闭") + "弹幕窗SC信息");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Checked ? "打开" : "关闭") + "弹幕窗SC信息", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void ShowGuardSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool Checked = (bool)ShowGuardSwitch.IsChecked;
            GUIConfig.ShowGuardSwitch = Checked;
            CoreConfig.SetValue(CoreConfigClass.Key.ShowGuardSwitch, Checked.ToString(), CoreConfigClass.Group.GUI);
            Growl.Success((Checked ? "打开" : "关闭") + "弹幕窗大航海信息");
            Log.AddLog(nameof(MainWindow), LogClass.LogType.Debug, (Checked ? "打开" : "关闭") + "弹幕窗大航海信息", false, null, false);
            CoreConfigFile.WriteConfigFile(true);
        }

        private void TypeGridSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (TypeSetGrid.Visibility == Visibility.Visible)
            {
                TypeSetGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                TypeSetGrid.Visibility = Visibility.Visible;
            }
        }

        private void ThisWindowTopping_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ThisWindowTopping.IsChecked)
            {
                this.Topmost = true;
                timer.Start();
            }
            else
            {
                timer.Stop();
                this.Topmost = false;
            }
        }
    }
}
