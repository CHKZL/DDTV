using Core.LiveChat;
using Core.LogModule;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Desktop.Views.Windows
{
    /// <summary>
    /// DanmaOnlyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DanmaOnlyWindow : FluentWindow
    {
        RoomCardClass roomCard;
        public ObservableCollection<DanmaOnly> DanmaCollection = new();
        public DanmaOnlyWindow(RoomCardClass Card)
        {
            InitializeComponent();
            roomCard = Card;
            DanmaView.ItemsSource = DanmaCollection;
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    UI_TitleBar.Title = $"{Card.Name}({Card.RoomId})";
                    DanmaCollection.Add(new DanmaOnly { Message = $"连接[{Card.Name}({Card.RoomId})]直播间弹幕长连" });
                });

                if (roomCard.DownInfo.LiveChatListener == null)
                {
                    roomCard.DownInfo.LiveChatListener = new Core.LiveChat.LiveChatListener(roomCard.RoomId);
                    roomCard.DownInfo.LiveChatListener.Connect();
                }
                else if (!roomCard.DownInfo.LiveChatListener.State)
                {
                    roomCard.DownInfo.LiveChatListener.Connect();
                }
                roomCard.DownInfo.LiveChatListener.Register.Add("DanmaOnlyWindow");
                roomCard.DownInfo.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
                Dispatcher.Invoke(() =>
                {
                    DanmaCollection.Add(new DanmaOnly { Message = $"等待直播间消息中..." });
                });       
            });
        }

        private void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
            DanmaOnly msg = new();
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {
                        msg.Message = $"{Danmu.UserName}：{Danmu.Message}";
                        break;
                    }
                case SuperchatEventArg SuperchatEvent:
                    {
                        msg.Message = $"{SuperchatEvent.UserName}：打赏SC[{SuperchatEvent.Price}]";
                        break;
                    }
                case GuardBuyEventArgs GuardBuyEvent:
                    {
                        msg.Message = $"{GuardBuyEvent.UserName}：购买大航海[{(GuardBuyEvent.GuardLevel == 1 ? "总督" : GuardBuyEvent.GuardLevel == 2 ? "提督" : "舰长")}]一共[{GuardBuyEvent.Number}]个月";
                        break;
                    }
                case SendGiftEventArgs sendGiftEventArgs:
                    {
                        msg.Message = $"{sendGiftEventArgs.UserName}：赠送[{sendGiftEventArgs.GiftName}]礼物[{sendGiftEventArgs.Amount}]个";
                        break;
                    }
                default:
                    break;
            }
            Dispatcher.Invoke(() =>
            {
                DanmaCollection.Add(msg);

                while (DanmaCollection.Count > 50)
                {
                    DanmaCollection.RemoveAt(0);
                }
                DanmaView.SelectedItem = DanmaView.Items[DanmaView.Items.Count - 1];

                DanmaView.ScrollIntoView(DanmaView.SelectedItem);
            });
        }

        public class DanmaOnly
        {
            public string Message { get; set; } = "";
        }

        private void FluentWindow_Closing(object sender, CancelEventArgs e)
        {
            if (roomCard.DownInfo.LiveChatListener != null)
            {
                roomCard.DownInfo.LiveChatListener.Register.Remove("DanmaOnlyWindow");
                if (roomCard.DownInfo.LiveChatListener.Register.Count == 0)
                {
                    roomCard.DownInfo.LiveChatListener.DanmuMessage = null;
                    try
                    {
                        roomCard.DownInfo.LiveChatListener.Dispose();
                        roomCard.DownInfo.LiveChatListener = null;
                    }
                    catch (Exception)
                    { }

                }
            }
        }

        public DanmaOnly? SelectedDanma { get; set; }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDanma != null)
            {
                System.Windows.Clipboard.SetText(SelectedDanma.Message);
            }
        }

        private void DanmaView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            SelectedDanma = DanmaView.SelectedItem as DanmaOnly;
        }

        private void Send_Danma_Button_Click(object sender, RoutedEventArgs e)
        {
            string T = DanmaOnly_DanmaInput.Text;
            if(string.IsNullOrEmpty(T) && T.Length>20)
            {
                return;
            }
            Danmu.SendDanmu(roomCard.RoomId.ToString(), T);
            DanmaOnly_DanmaInput.Clear();
        }

        private void DanmaOnly_DanmaInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox? textBox = sender as System.Windows.Controls.TextBox;
            if (textBox!=null && textBox.Text.Length > 20)
            {
                int selectionStart = textBox.SelectionStart;
                textBox.Text = textBox.Text.Substring(0, 20);
                textBox.SelectionStart = selectionStart > 20 ? 20 : selectionStart;
            }
        }
    }
}
