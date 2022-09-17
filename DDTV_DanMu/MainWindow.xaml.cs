using ColorConsole;
using DDTV_Core;
using DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu;
using DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace DDTV_DanMu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DanmuInfo> DanMuList = new List<DanmuInfo>();
        private bool IsTransparency=false;
        private Timer Most;
        private DispatcherTimer timer;
        private static Window W;
        private class DanmuInfo
        {
            public string Text { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IsIndependent">是否为独立启动，如果是独立启动会唤起一个新的Core实例</param>
        public MainWindow()
        {
            long Uid = 0; bool IsIndependent = true;
            InitializeComponent();
            if (IsIndependent)
            {
                InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_DanMu);
                Log.AddLog(nameof(DDTV_DanMu), LogClass.LogType.Info, $"初始化DDTV_DanMu完成...........");
                Log.AddLog(nameof(DDTV_DanMu), LogClass.LogType.Info, $"正在连接房间，请稍候...........");
                Thread.Sleep(3000);
            }
            Rec(Uid != 0 ? Uid : long.Parse(DDTV_Core.SystemAssembly.ConfigModule.BilibiliUserConfig.account.uid));
            //Rec(8739477);
            Add("游戏如需全屏请请使用“无边框全屏”模式");
            Add("初始化完成，等待直播间数据");
            W = this;
            DanMuLog.ItemsSource = DanMuList;
            timer = new DispatcherTimer();
            Loaded += new RoutedEventHandler(Topping);
           
            //Most = new Timer(RefresherTopmost, null, 1000, 1000);
            LockSize();
        }

        //public static void RefresherTopmost(object state)
        //{
        //    W.Dispatcher.Invoke(new Action(() =>
        //    {
        //        W.Topmost = true;
        //        //Console.WriteLine("TOP");
        //    }));
        //}

        void Topping(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer1_Tick;
            timer.Start();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void Rec(long Uid)
        {
            RoomInfoClass.RoomInfo _ = DanMuRec.StartRecDanmu(Uid, true);
            _.roomWebSocket.LiveChatListener.DisposeSent += LiveChatListener_DisposeSent;
            _.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
            if(_.roomWebSocket.LiveChatListener.m_ReceiveBuffer!=null)
            {
                Add($"与弹幕服务器连接成功(Uid:{Uid})...");
            }
        }

        private void LiveChatListener_DisposeSent(object? sender, EventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            try
            {
                if (!liveChatListener.IsUserDispose)
                {
                    Add("与弹幕服务器断开连接，正在重连....");
                    Log.AddLog(nameof(DDTV_DanMu), LogClass.LogType.Info, $"{liveChatListener.TroomId}直播间弹幕连接中断，检测到直播未停止且弹幕录制设置已打开，开始重连弹幕服务器");
                    Rec(liveChatListener.mid);
                }
                else
                {
                    Log.AddLog(nameof(DDTV_DanMu), LogClass.LogType.Info, $"{liveChatListener.TroomId}请求重连，但该房间的录制已经标记不再连接，取消重连");
                }
            }
            catch (Exception)
            {
            }
        }
        private ConsoleWriter console = new ConsoleWriter();

        private void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        {
            LiveChatListener liveChatListener = (LiveChatListener)sender;
            Rooms.RoomInfo.TryGetValue(liveChatListener.mid, out RoomInfoClass.RoomInfo roomInfo);
            if (roomInfo != null)
            {
                string text = string.Empty;
                switch (e)
                {
                    case DanmuMessageEventArgs Danmu:
                        {
                            console.Write($"[弹幕]", ConsoleColor.Green);
                            console.Write($"{DateTime.Now.ToString("HH:mm:ss")}:", ConsoleColor.DarkGray);
                            console.Write($"{Danmu.UserName}：", ConsoleColor.Magenta);
                            console.WriteLine($"{Danmu.Message}", ConsoleColor.White);
                            text = $"[弹幕]{Danmu.UserName}：{Danmu.Message}";

                        }
                        break;
                    case SuperchatEventArg SuperchatEvent:
                        {
                            console.Write($"[超级留言]", ConsoleColor.Red);
                            console.Write($"(金额{SuperchatEvent.Price})", ConsoleColor.Red);
                            console.Write($"{DateTime.Now.ToString("HH:mm:ss")}:", ConsoleColor.DarkGray);
                            console.Write($"{SuperchatEvent.UserName}：", ConsoleColor.Magenta);
                            console.WriteLine($"{SuperchatEvent.Message}", ConsoleColor.White);
                            text = $"[超级留言](金额{SuperchatEvent.Price}){SuperchatEvent.UserName}：{SuperchatEvent.Message}";
                        }
                        break;
                    case GuardBuyEventArgs GuardBuyEvent:
                        {
                            string Lv = GuardBuyEvent.GuardLevel == 1 ? "总督" : GuardBuyEvent.GuardLevel == 2 ? "提督" : "舰长";
                            console.Write($"[上舰]", ConsoleColor.Red);
                            console.Write($"{DateTime.Now.ToString("HH:mm:ss")}:", ConsoleColor.DarkGray);
                            console.Write($"{GuardBuyEvent.UserName}：", ConsoleColor.Magenta);
                            console.WriteLine($"{GuardBuyEvent.Number}个月的{Lv}", ConsoleColor.White);
                            text = $"[上舰]{GuardBuyEvent.UserName}：{GuardBuyEvent.Number}个月的{Lv}";
                        }
                        break;
                    case SendGiftEventArgs sendGiftEventArgs:
                        {
                            console.Write($"[礼物]", ConsoleColor.Red);
                            console.Write($"{DateTime.Now.ToString("HH:mm:ss")}:", ConsoleColor.DarkGray);
                            console.Write($"{sendGiftEventArgs.UserName}：", ConsoleColor.Magenta);
                            console.WriteLine($"送了{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}", ConsoleColor.White);
                            text = $"[礼物]{sendGiftEventArgs.UserName}：送了{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}";
                        }
                        break;
                    case InteractWordEventArgs interactWordEventArgs:
                        {
                            switch (interactWordEventArgs.MsgType)
                            {
                                case 1:
                                    console.Write($"[进场]", ConsoleColor.Yellow);
                                    text = $"[进场]";
                                    break;
                                case 2:
                                    console.Write($"[关注]", ConsoleColor.Yellow);
                                    text = $"[关注]";
                                    break;
                                case 4:
                                    console.Write($"[特别关注]", ConsoleColor.Yellow);
                                    text = $"[特别关注]";
                                    break;
                            }

                            console.Write($"{DateTime.Now.ToString("HH:mm:ss")}:", ConsoleColor.DarkGray);
                            console.WriteLine($"{interactWordEventArgs.Uname}", ConsoleColor.Magenta);
                            text += $":{interactWordEventArgs.Uname}";
                        }
                        break;
                    case WarningEventArg warningEventArg:
                        {
                            console.Write($"[管理员警告]", ConsoleColor.Yellow);
                            console.Write($"{DateTime.Now.ToString("HH:mm:ss")}:", ConsoleColor.DarkGray);
                            console.WriteLine($"管理员警告！警告内容:{warningEventArg.msg}", ConsoleColor.White);
                            text = $"[管理员警告]警告内容: { warningEventArg.msg}";
                        }
                        break;
                    case CutOffEventArg cutOffEventArg:
                        {
                            console.Write($"[直播被切断]", ConsoleColor.Yellow);
                            console.Write($"{DateTime.Now.ToString("HH:mm:ss")}:", ConsoleColor.DarkGray);
                            console.WriteLine($"直播被切断！系统消息:{cutOffEventArg.msg}", ConsoleColor.White);
                            text = $"[直播被切断]系统消息: { cutOffEventArg.msg}";
                        }
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(text))
                {
                    //DanMuLog.Dispatcher.Invoke(() => DanMuLog.AppendText(text+"\r"));
                    //DanMuLog.Focus(); // RichTextBox获取焦点，有时也可以不用
                    //DanMuLog.CaretPosition = DanMuLog.Document.ContentEnd; // 获取RichTextBox内文档结尾的光标位置
                    //DanMuLog.ScrollToVerticalOffset(100);
                    Add(text);
                }
            }

        }
        private void Add(string Text)
        {
            DanMuList.Add(new DanmuInfo() { Text = Text });
            UpdateDanMuListScrollToEnd();
        }
        private void UpdateDanMuListScrollToEnd()
        {
            try
            {
                DanMuLog.Items.Refresh();
                Decorator decorator = (Decorator)VisualTreeHelper.GetChild(DanMuLog, 0);
                ScrollViewer scrollViewer = (ScrollViewer)decorator.Child;
                scrollViewer.ScrollToEnd();
            }
            catch (Exception)
            { }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception)
            {

            }
        }

        private void LockWindowSize_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if ((bool)checkBox.IsChecked)
            {
                LockSize();
            }
            else
            {
                UnlockSize();
            }
        }

        private void LockSize()
        {
            this.MaxHeight = this.Height;
            this.MaxWidth = this.Width;
            this.MinHeight = this.Height;
            this.MinWidth = this.Width;
            this.ResizeMode = ResizeMode.NoResize;
        }
        private void UnlockSize()
        {
            this.MaxHeight = int.MaxValue;
            this.MaxWidth = int.MaxValue;
            this.MinHeight = 200;
            this.MinWidth = 200;
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
        }
    }
}
