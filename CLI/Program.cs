using Core;
using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject;
using static Core.RuntimeObject.RoomList.RoomCard;
using System.Runtime.Intrinsics.X86;
using ConsoleTableExt;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Core.LiveChat;
using static System.Net.Mime.MediaTypeNames;
using System;

namespace CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Service.CreateHostBuilder(new string[] { "" }).Build().Run();
        }
        public static void TEST()
        {
            Core.LiveChat.LiveChatListener liveChatListener = new Core.LiveChat.LiveChatListener(858235);
            liveChatListener.MessageReceived += LiveChatListener_MessageReceived;
            liveChatListener.Connect();
        }

        private static void LiveChatListener_MessageReceived(object? sender, Core.LiveChat.MessageEventArgs e)
        {
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    {
                        Log.Info(nameof(LiveChatListener_MessageReceived), $"[弹幕]{DateTime.Now.ToString("HH:mm:ss")} {Danmu.UserName}：{Danmu.Message}");
                    }
                    break;
                case SuperchatEventArg SuperchatEvent:
                    {
                        Log.Info(nameof(LiveChatListener_MessageReceived), $"[超级留言](金额{SuperchatEvent.Price}){DateTime.Now.ToString("HH:mm:ss")} {SuperchatEvent.UserName}：{SuperchatEvent.Message}");
                    }
                    break;
                case GuardBuyEventArgs GuardBuyEvent:
                    {
                        string Lv = GuardBuyEvent.GuardLevel == 1 ? "总督" : GuardBuyEvent.GuardLevel == 2 ? "提督" : "舰长";
                        Log.Info(nameof(LiveChatListener_MessageReceived), $"[上舰]{DateTime.Now.ToString("HH:mm:ss")} {GuardBuyEvent.UserName}：{GuardBuyEvent.Number}个月的{Lv}");
                    }
                    break;
                case SendGiftEventArgs sendGiftEventArgs:
                    {
                        Log.Info(nameof(LiveChatListener_MessageReceived), $"[礼物]{DateTime.Now.ToString("HH:mm:ss")} {sendGiftEventArgs.UserName}：送了{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}");
                    }
                    break;
                case WarningEventArg warningEventArg:
                    {
                        Log.Info(nameof(LiveChatListener_MessageReceived), $"[管理员警告]{DateTime.Now.ToString("HH:mm:ss")} 管理员警告！警告内容:{warningEventArg.msg}");
                    }
                    break;
                case CutOffEventArg cutOffEventArg:
                    {
                        Log.Info(nameof(LiveChatListener_MessageReceived), $"[直播被切断]{DateTime.Now.ToString("HH:mm:ss")} 直播被切断！系统消息:{cutOffEventArg.msg}");
                    }
                    break;
                default:
                    break;
            }
        }

        public class Service
        {
            public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<DDTVService>();
                });
            public class DDTVService : BackgroundService
            {
                protected override Task ExecuteAsync(CancellationToken stoppingToken)
                {
                    return Task.Run(async () =>
                    {
                        Core.Init.Start();//初始化必须执行的
                        if (!Account.AccountInformation.State)
                        {
                            await Login.QR();//如果没有登录态，需要执行扫码
                        }
                        while (!Account.AccountInformation.State)
                        {
                            Thread.Sleep(1000);//等待登陆
                        }
                        TerminalDisplay.SeKey();
                        //DetectRoom detectRoom = new();//实例化房间监听
                        //detectRoom.start();//启动房间监听
                        //detectRoom.LiveStart += Record.DetectRoom_LiveStart;//开播事件
                        //Log.Info(nameof(DetectRoom), $"注册开播事件");
                        //detectRoom.LiveEnd += Record.DetectRoom_LiveEnd;//下播事件
                        //Log.Info(nameof(DetectRoom), $"注册下播事件");
                        TEST();
#if DEBUG
                        Task.Run(() =>
                        {
                            Process currentProcess = null;
                            while (true)
                            {
                                currentProcess = Process.GetCurrentProcess();
                                long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
                                (int Total, int Download) = Core.RuntimeObject.RoomList.GetTasksInDownloadCount();
                                Log.Info("DokiDoki", $"总:{Total}|录制中:{Download}|使用内存:{Core.Tools.Linq.ConversionSize(totalBytesOfMemoryUsed, Core.Tools.Linq.ConversionSizeType.String)}|{Init.InitType}|{Init.Ver}【Dev】(编译时间:{Init.CompiledVersion})");
                                Thread.Sleep(60 * 1000);
                            }
                        });
# endif
                    });
                }

                public override Task StopAsync(CancellationToken stoppingToken)
                {
                    return Task.Run(() =>
                    {
                        Log.Warn(nameof(DDTVService), "收到SIGINT信号(一般是用户Ctrl+C)，主进程被系统结束");
                    });
                }
            }
        }
    }
}
