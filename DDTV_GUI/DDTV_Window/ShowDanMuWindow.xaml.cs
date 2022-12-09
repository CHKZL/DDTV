using DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu;
using DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.Log;
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

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// ShowDanMuWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShowDanMuWindow : GlowWindow
    {
        private List<DanmuInfo> DanMuList = new List<DanmuInfo>();
        private RoomInfoClass.RoomInfo _roominfo = new RoomInfoClass.RoomInfo();
        public ShowDanMuWindow(long UID)
        {
            InitializeComponent();
            if (Rooms.RoomInfo.TryGetValue(UID, out var roomInfo))
            {
                _roominfo.room_id = roomInfo.room_id;
                _roominfo.uid = roomInfo.uid;
                _roominfo.uname = roomInfo.uname;
                DanMuLog.ItemsSource = DanMuList;
                Add($"尝试和弹幕服务器连接(RoomID:{roomInfo.room_id})");
                Rec(_roominfo.uid != 0 ? _roominfo.uid : long.Parse(DDTV_Core.SystemAssembly.ConfigModule.BilibiliUserConfig.account.uid));
            }
            else
            {
                Add($"处理失败，该房间不在DDTV监控列表中");
                Add($"请先添加到监控列表");
            }
        }
        private void Rec(long Uid)
        {
            MainWindow.linkDMNum++;
            Task.Run(() =>
            {


                _roominfo = DanMuRec.StartRecDanmu(Uid, true);
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
                        text = $"[弹幕]{Danmu.UserName}：{Danmu.Message}";

                    }
                    break;
                case SuperchatEventArg SuperchatEvent:
                    {
                        text = $"[SC](金额{SuperchatEvent.Price}){SuperchatEvent.UserName}：{SuperchatEvent.Message}";
                    }
                    break;
                case GuardBuyEventArgs GuardBuyEvent:
                    {
                        string Lv = GuardBuyEvent.GuardLevel == 1 ? "总督" : GuardBuyEvent.GuardLevel == 2 ? "提督" : "舰长";
                        text = $"[上舰]{GuardBuyEvent.UserName}：{GuardBuyEvent.Number}个月的{Lv}";
                    }
                    break;
                case SendGiftEventArgs sendGiftEventArgs:
                    {
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
                if (Massage.Length > 30)
                {
                    Growl.Warning("发送的弹幕长度尝过限制(30个字符)");
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
    }
}
