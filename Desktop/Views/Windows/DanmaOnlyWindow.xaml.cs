using Core.LiveChat;
using Core.RuntimeObject;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        /// <summary>
        /// 当前窗口的置顶状态
        /// </summary>
        private bool TopMostSwitch = false;
        /// <summary>
        /// 窗口是否已开始关闭（用于阻止后台线程在退订之后再次订阅事件）
        /// </summary>
        private volatile bool _closed = false;
        /// <summary>
        /// 本窗口是否已完成事件订阅与Register登记（仅在UI线程读写）
        /// </summary>
        private bool _subscribed = false;
        /// <summary>
        /// 订阅弹幕事件并登记Register（仅在UI线程调用，保证与Closing中的退订串行）
        /// </summary>
        private void SubscribeLiveChat()
        {
            if (_subscribed || roomCard.DownInfo.LiveChatListener == null)
            {
                return;
            }
            _subscribed = true;
            roomCard.DownInfo.LiveChatListener.Register.Add("DanmaOnlyWindow");
            roomCard.DownInfo.LiveChatListener.MessageReceived -= LiveChatListener_MessageReceived;
            roomCard.DownInfo.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
        }
        public DanmaOnlyWindow(RoomCardClass Card)
        {
            InitializeComponent();
            roomCard = Card;
            if (roomCard != null && roomCard.RoomId > 0)
            {
                DanmaView.ItemsSource = DanmaCollection;
                UI_TitleBar.Title = $"{Card.Name}({Card.RoomId})";
                this.Title = UI_TitleBar.Title;
                DanmaCollection.Add(new DanmaOnly { Message = $"连接[{Card.Name}({Card.RoomId})]直播间弹幕长连" });

                //事件订阅必须在UI线程同步执行：Closing中的退订也在UI线程，二者串行才不会出现"退订先于订阅"导致窗口永远无法释放的竞态
                SubscribeLiveChat();
                Core.RuntimeObject.Danmu.DanmaTriggerReconnect -= Instance_DanmaTriggerReconnect;
                Core.RuntimeObject.Danmu.DanmaTriggerReconnect += Instance_DanmaTriggerReconnect;

                Task.Run(() =>
                {
                    if (roomCard.DownInfo.LiveChatListener == null)
                    {
                        roomCard.DownInfo.LiveChatListener = new Core.LiveChat.LiveChatListener(roomCard.RoomId);
                        roomCard.DownInfo.LiveChatListener.Connect();
                    }
                    else if (!roomCard.DownInfo.LiveChatListener.State)
                    {
                        roomCard.DownInfo.LiveChatListener.Connect();
                    }

                    Dispatcher.Invoke(() =>
                    {
                        if (_closed)
                        {
                            //窗口在连接期间已被关闭，若长连是刚刚才建立的且没有任何注册者，直接释放，避免留下无人使用的监听器
                            if (roomCard.DownInfo.LiveChatListener != null && roomCard.DownInfo.LiveChatListener.Register.Count == 0)
                            {
                                try
                                {
                                    roomCard.DownInfo.LiveChatListener.Dispose();
                                    roomCard.DownInfo.LiveChatListener = null;
                                }
                                catch (Exception)
                                { }
                            }
                            return;
                        }
                        if (roomCard.DownInfo.LiveChatListener != null)
                        {
                            //监听器可能在构造之后才创建，此处补订阅（_subscribed保证每个窗口实例只登记一次，
                            //不能用Register.Contains判断——同房间开多个弹幕窗时条目会互相干扰）
                            SubscribeLiveChat();
                            DanmaCollection.Add(new DanmaOnly { Message = $"等待直播间消息中..." });
                        }
                    });
                });
            }
            else
            {
                 MainWindow.SnackbarService.Show("单独弹幕窗口打开失败", "没有获取到对应的直播间信息，请再次手动打开弹幕窗口重试或联系开发者", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.CalendarError24), TimeSpan.FromSeconds(8));
                //this.Close();
                return;
            }
        }

        private void Instance_DanmaTriggerReconnect(object? sender, RoomCardClass e)
        {
            if (_closed)
            {
                return;
            }
            Dispatcher.Invoke(() =>
            {
                DanmaCollection.Add(new DanmaOnly { Message = $"弹幕重连中..." });
            });
            if (e.DownInfo.LiveChatListener != null)
            {
                e.DownInfo.LiveChatListener.MessageReceived -= LiveChatListener_MessageReceived;
                e.DownInfo.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
            }
        }

        private void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
            DanmaOnly msg = new();
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {
                        //屏蔽词走缓存（配置不变时不重新Split），避免每条弹幕都分配临时数组
                        if (Services.BarrageBlockWords.IsBlocked(Danmu.Message))
                        {
                            return;
                        }
                        msg.Message = $"{Danmu.UserName}：{Danmu.Message}";
                        break;
                    }
                case SuperchatEventArg SuperchatEvent:
                    {
                        msg.Message = $"{SuperchatEvent.UserName}：打赏SC[{SuperchatEvent.Price}]{SuperchatEvent.Message}";
                        break;
                    }
                case GuardBuyEventArgs GuardBuyEvent:
                    {
                        msg.Message = $"{GuardBuyEvent.UserName}：购买大航海[{(GuardBuyEvent.GuardLevel == 1 ? "总督" : GuardBuyEvent.GuardLevel == 2 ? "提督" : "舰长")}]一共[{GuardBuyEvent.Number}]个月";
                        break;
                    }
                case GuardRenewEventArgs guardRenewEvent:
                    {
                        msg.Message = $"{guardRenewEvent.UserName}：购买大航海[{(guardRenewEvent.GuardLevel == 1 ? "总督" : guardRenewEvent.GuardLevel == 2 ? "提督" : "舰长")}]一共[{guardRenewEvent.Number}]个月";
                        break;
                    }
                case SendGiftEventArgs sendGiftEventArgs:
                    {
                        //屏蔽词走缓存（配置不变时不重新Split），避免每条礼物消息都分配临时数组
                        if (Services.BarrageBlockWords.IsBlocked(sendGiftEventArgs.GiftName))
                        {
                            return;
                        }
                        msg.Message = $"{sendGiftEventArgs.UserName}：赠送[{sendGiftEventArgs.GiftName}]礼物[{sendGiftEventArgs.Amount}]个";
                        break;
                    }
                case MessageEventArgs messageEventArgs:
                        {
                            if (messageEventArgs.Command == "Reconnect")
                            {
                                RoomCardClass roomCardClass = new();
                                _Room.GetCardForRoomId(liveChatListener.RoomId, ref roomCardClass);
                                Core.RuntimeObject.Danmu.ReconnectRoomDanmaObjects(roomCardClass);
                            }
                            break;
                        }
                default:
                    break;
            }
            //未识别/不需要展示的消息类型（如进场消息INTERACT_WORD、Reconnect指令等）不会产生有效文本，
            //直接丢弃不入队——大直播间这类消息非常高频，入队会把弹幕窗口刷满空白行并让flush空转
            if (string.IsNullOrEmpty(msg.Message))
            {
                return;
            }
            //弹幕先进缓冲，200ms聚合一次批量刷新到UI，避免每条弹幕都同步Invoke阻塞弹幕接收线程
            lock (_pendingDanmaLock)
            {
                _pendingDanma.Add(msg);
                if (_danmaFlushScheduled)
                {
                    return;
                }
                _danmaFlushScheduled = true;
            }
            _ = Task.Run(async () =>
            {
                await Task.Delay(200);
                try
                {
                    await Dispatcher.InvokeAsync(FlushPendingDanma);
                }
                catch (Exception)
                {
                    //窗口已关闭等情况忽略
                }
            });
        }

        /// <summary>
        /// 待刷新的弹幕缓冲
        /// </summary>
        private readonly List<DanmaOnly> _pendingDanma = new();
        private readonly object _pendingDanmaLock = new();
        private bool _danmaFlushScheduled = false;

        /// <summary>
        /// 将缓冲的弹幕批量刷新到界面（仅在UI线程执行）
        /// </summary>
        private void FlushPendingDanma()
        {
            List<DanmaOnly> msgs;
            lock (_pendingDanmaLock)
            {
                msgs = new List<DanmaOnly>(_pendingDanma);
                _pendingDanma.Clear();
                _danmaFlushScheduled = false;
            }
            foreach (var m in msgs)
            {
                DanmaCollection.Add(m);
            }
            while (DanmaCollection.Count > 50)
            {
                DanmaCollection.RemoveAt(0);
            }
            if (DanmaView.Items.Count > 0)
            {
                DanmaView.SelectedItem = DanmaView.Items[DanmaView.Items.Count - 1];
                DanmaView.ScrollIntoView(DanmaView.SelectedItem);
            }
        }

        public class DanmaOnly
        {
            public string Message { get; set; } = "";
        }

        private void FluentWindow_Closing(object sender, CancelEventArgs e)
        {
            _closed = true;
            if (roomCard!=null && roomCard.DownInfo.LiveChatListener != null)
            {
                //无论是否还有其他注册者，都必须退订本窗口的消息事件，否则监听器存活期间会持续引用已关闭的窗口导致内存泄漏
                roomCard.DownInfo.LiveChatListener.MessageReceived -= LiveChatListener_MessageReceived;
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
            Core.RuntimeObject.Danmu.DanmaTriggerReconnect -= Instance_DanmaTriggerReconnect;
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
            //Danmu.ReconnectRoomDanmaObjects(roomCard);
            //return;
            string T = DanmaOnly_DanmaInput.Text;
            if (string.IsNullOrEmpty(T) || T.Length > 40/*Core.Config.Core_RunConfig._MaximumLengthDanmu*/)
            {
                SetNotificatom("弹幕过长或为空", $"输入的弹幕长度为0或者超过最大长度限制，目前限制长度为40", HudNotification.HudLevel.Notice);
                return;
            }
            Danmu.SendDanmu(roomCard.RoomId.ToString(), T);
            DanmaOnly_DanmaInput.Clear();
        }

        private void DanmaOnly_DanmaInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //直接进行一个屏蔽，检查个屁，超过直接报错
            //return;
            //检测输入框长度，超过长度则截断
            System.Windows.Controls.TextBox? textBox = sender as System.Windows.Controls.TextBox;
            int maxlen = Core.Config.Core_RunConfig._MaximumLengthDanmu;
            if (textBox != null && textBox.Text.Length > Core.Config.Core_RunConfig._MaximumLengthDanmu)
            {
                int selectionStart = textBox.SelectionStart;
                textBox.Text = textBox.Text.Substring(0, maxlen);
                textBox.SelectionStart = selectionStart > maxlen ? maxlen : selectionStart;
            }
        }

        private void DanmaOnly_DanmaInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                string T = DanmaOnly_DanmaInput.Text;
                if (string.IsNullOrEmpty(T) && T.Length > 40)
                {
                    return;
                }
                Danmu.SendDanmu(roomCard.RoomId.ToString(), T);
                DanmaOnly_DanmaInput.Clear();
            }
        }

        private void MenuItem_TopMost_Click(object sender, RoutedEventArgs e)
        {
            if (TopMostSwitch)
            {
                this.Topmost = false;
                TopMostSwitch = false;
                SetNotificatom("撤销窗口置顶", $"{roomCard.Name}({roomCard.RoomId})窗口置顶已关闭");
            }
            else
            {
                this.Topmost = true;
                TopMostSwitch = true;
                SetNotificatom("打开窗口置顶", $"{roomCard.Name}({roomCard.RoomId})窗口置顶已打开");
            }
        }
        private void SetNotificatom(string Title, string Message = "'", HudNotification.HudLevel level = HudNotification.HudLevel.Success)
        {
            Dispatcher.Invoke(() =>
            {
                HudNotification.Notify("/// DANMAKU", Message, Title, level);
            });

        }
    }
}
