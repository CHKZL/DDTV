using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript;
using System.Net.WebSockets;
using DDTV_Core.SystemAssembly.Log;
using System.Net;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using System.Runtime.InteropServices;
using ConsoleTables;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass.Downloads;
using DDTV_Core.Tool;
using DDTV_Core.SystemAssembly.RoomPatrolModule;

namespace DDTV_Core
{
    public class InitDDTV_Core
    {
        /// <summary>
        /// Core的版本号
        /// </summary>
        public static string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static string ClientAID = string.Empty;
        public static SatrtType InitType= SatrtType.DDTV_Core;
        public static string CompiledVersion ="2022-06-04 01:34:08";

        /// <summary>
        /// 初始化COre
        /// </summary>
        public static void Core_Init(SatrtType satrtType = SatrtType.DDTV_Core)
        {
            InitType= satrtType;
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            Console.WriteLine($"========================\nDDTV_Core开始启动，当前版本:{Ver}(编译时间:{CompiledVersion})\n========================");
            Log.LogInit(LogClass.LogType.Debug);
            //TestVetInfo();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 1024*1024*8;
            ServicePointManager.Expect100Continue = false;
            CoreConfig.ConfigInit(satrtType);
            Tool.DDTV_Update.CheckUpdateProgram();
            Task.Run(() => Tool.DDcenter.Init(satrtType));
            if (satrtType != SatrtType.DDTV_GUI)
            {
                BilibiliUserConfig.CheckAccount.CheckAccountChanged += CheckAccount_CheckAccountChanged;//注册登陆信息检查失效事件
            }
            //var c = RuntimeInformation.RuntimeIdentifier;
            Console.WriteLine($"========================\nDDTV_Core启动完成\n========================");
            
            switch(satrtType)
            {
                case SatrtType.DDTV_Core:
                    ServerInteraction.CheckUpdates.Update("Core");
                    ServerInteraction.Dokidoki.Start("Core");
                    break;
                case SatrtType.DDTV_CLI:
                    ServerInteraction.CheckUpdates.Update("CLI");
                    ServerInteraction.Dokidoki.Start("CLI");
                    break;
                case SatrtType.DDTV_WEB:
                    ServerInteraction.CheckUpdates.Update("WEB");
                    ServerInteraction.Dokidoki.Start("WEB");
                    break;
                case SatrtType.DDTV_GUI:
                    ServerInteraction.CheckUpdates.Update("GUI");
                    ServerInteraction.Dokidoki.Start("GUI");
                    break;
                default:
                    ServerInteraction.CheckUpdates.Update("Core");
                    ServerInteraction.Dokidoki.Start("Core");
                    break;
            }
           
            if (satrtType!= SatrtType.DDTV_GUI)
            {
                SeKey();
            }
           



            //switch (satrtType)
            //{
            //    case SatrtType.DDTV_Core:
            //        Tool.DDTV_Update.ComparisonVersion("Core", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            //        break;
            //    case SatrtType.DDTV_GUI:
            //        Tool.DDTV_Update.ComparisonVersion("GUI", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            //        break;
            //    case SatrtType.DDTV_WEB:
            //        Tool.DDTV_Update.ComparisonVersion("WEB", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            //        break;
            //}


            #region 测试代码


            //SystemAssembly.ConfigModule.RoomConfig.AddRoom(473244363,"");

            //while (true)
            //{
            //    if (Console.ReadKey().Key.Equals(ConsoleKey.I))
            //    {
            //        Console.WriteLine($"请输入UID");
            //        long uid = long.Parse(Console.ReadLine());
            //        SystemAssembly.DownloadModule.Download.CancelDownload(uid);
            //    }
            //}
            //SystemAssembly.DownloadModule.Download.AddDownloadTaskd(408490081);
            //List<long> vs = new List<long>()
            //{
            //    17661166,
            //    1081765694,
            //    269415357,
            //    36576761,
            //    2096422,
            //    8041302
            //};
            //Task.Run((Action)(() =>
            //{
            //    foreach (var item in vs)
            //    {
            //        BilibiliModule.API.WebSocket.WebSocket.ConnectRoomAsync(item.Value.uid);
            //var roomInfo = SystemAssembly.BilibiliModule.API.WebSocket.WebSocket.ConnectRoomAsync(122459);
            //roomInfo.roomWebSocket.LiveChatListener.MessageReceived += LiveChatListener_MessageReceived;
            //        Thread.Sleep(1000);
            //    }
            //}));
            //while (true)
            //{
            //    int i = 1;
            //    foreach (var item in BilibiliModule.Rooms.Rooms.RoomInfo)
            //    {
            //        if (item.Value.roomWebSocket.IsConnect)
            //        {
            //            long Time = TimeModule.Time.Operate.GetRunMilliseconds()-item.Value.roomWebSocket.dokiTime;
            //            Console.WriteLine(Time>35000 ? "no■"+Time : "ko□"+Time+$" {i} {item.Value.room_id} {item.Value.roomWebSocket.LiveChatListener.host.host_list[0].host}");
            //            i++;
            //        }

            //    }
            //    Thread.Sleep(10000);
            //}

            //BilibiliModule.API.DanMu.getDanmuInfo(411812743);
            //var ttt = BilibiliModule.Rooms.Rooms.GetValue(125555553, DataCacheModule.DataCacheClass.CacheType.is_sp);
            //Console.WriteLine(BilibiliModule.Rooms.Rooms.AddRoom(494850406, "test"));
            //BilibiliModule.API.RoomInfo.room_init(2299184);
            //string url = BilibiliModule.API.RoomInfo.playUrl(2299184, BilibiliModule.Rooms.RoomInfoClass.PlayQuality.OriginalPainting);
            //DownloadModule.Download.DownFLV_WebClient(url);
            //BilibiliModule.API.DanMu.send(BilibiliModule.Rooms.Rooms.GetValue(408490081, DataCacheModule.DataCacheClass.CacheType.room_id), "DDTV3.0弹幕发送测试");
            #endregion

        }


        /// <summary>
        /// 登陆状态失效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CheckAccount_CheckAccountChanged(object? sender, EventArgs e)
        {
            RoomPatrol.IsOn = false;
            Task.Run(() =>
            {
                while (true)
                {
                    switch (BilibiliUserConfig.account.loginStatus)
                    {
                        case BilibiliUserConfig.LoginStatus.NotLoggedIn:
                            Log.AddLog("Login", LogClass.LogType.Warn, "未登录", false, null, false);
                            break;
                        case BilibiliUserConfig.LoginStatus.LoggedIn:
                            Log.AddLog("Login", LogClass.LogType.Info, "登陆成功", false, null, false);
                            BilibiliUserConfig.CheckAccount.IsState = true;
                            RoomPatrol.IsOn = true;
                            return;
                        case BilibiliUserConfig.LoginStatus.LoginFailure:
                            Log.AddLog("Login", LogClass.LogType.Warn, "账号登陆失效！请进入[i]键菜单根据提示重新登陆", false, null, true);
                            break;
                        case BilibiliUserConfig.LoginStatus.LoggingIn:
                            Log.AddLog("Login", LogClass.LogType.Info, "等待登陆中...", false, null, false);
                            break;
                    }
                    Thread.Sleep(10 * 1000);
                }
            });
        }
        private static void TestVetInfo()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(90 * 1000);
                        Console.WriteLine("当前版本为开发预览测试版，请随时关注反馈群更新到最新版本。\n如遇见任何问题，请加群[338182356]反馈");
                    }
                    catch (Exception)
                    {

                    }
                }
            });
        }

        private static void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        {

            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[收到弹幕信息]{Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime(Danmu.Timestamp)} {Danmu.UserName}({Danmu.UserId}):{Danmu.Message}");
                    break;
                case SuperchatEventArg SuperchatEvent:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[收到Superchat信息]{Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime(SuperchatEvent.Timestamp)} {SuperchatEvent.UserName}({SuperchatEvent.UserId}):价值[{SuperchatEvent.Price}]的SC信息:【{SuperchatEvent.Message}】,翻译后:【{SuperchatEvent.messageTrans}】");
                    break;
                case GuardBuyEventArgs GuardBuyEvent:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[收到舰组信息]{Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime(GuardBuyEvent.Timestamp)} {GuardBuyEvent.UserName}({GuardBuyEvent.UserId}):开通了{GuardBuyEvent.Number}个月的{GuardBuyEvent.GuardName}(单价{GuardBuyEvent.Price})");
                    break;
                case SendGiftEventArgs sendGiftEventArgs:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[收到礼物]{Tool.TimeModule.Time.Operate.ConvertTimeStampToDateTime(sendGiftEventArgs.Timestamp)} {sendGiftEventArgs.UserName}({sendGiftEventArgs.UserId}):价值{sendGiftEventArgs.GiftPrice}的{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}");
                    break;
                case EntryEffectEventArgs entryEffectEventArgs:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[舰长进入房间]舰长uid:{entryEffectEventArgs.uid},舰长头像{entryEffectEventArgs.face},欢迎信息:{entryEffectEventArgs.copy_writing}");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Core的启动类型
        /// </summary>
        public enum SatrtType
        {
            DDTV_Core,
            DDTV_GUI,
            DDTV_CLI,
            DDTV_WEB,
            DDTV_Other = int.MaxValue
        }

        private static void SeKey()
        {
            Task.Run(() => {
                while (true)
                {
                    if (Console.ReadKey().Key.Equals(ConsoleKey.I))
                    {
                        Console.WriteLine($"请按对应的按键查看或修改配置：");
                        Console.WriteLine($"a：查看下载中的任务情况");
                        Console.WriteLine($"b：一键导入关注列表中的V(可能不全需要自己补一下)");
                        Console.WriteLine($"c：重登登陆");

                        switch (Console.ReadKey().Key)
                        {
                            case ConsoleKey.A:
                                {
                                    int i = 0;
                                    ConsoleTable tables = new ConsoleTable("序号","UID", "房间号", "昵称","直播标题","已下载大小","下载速率", "状态","开始时间", "是否录制弹幕信息");
                                    Console.WriteLine($"下载中的任务:");
                                    foreach (var A1 in Rooms.RoomInfo)
                                    {
                                        if (A1.Value.DownloadingList.Count > 0)
                                        {
                                            DateTime StartTime=DateTime.Now;
                                            DownloadStatus downloadStatus=DownloadStatus.Standby;
                                            ulong FileSize = 0;
                                            string spe = "";
                                            foreach (var item in A1.Value.DownloadingList)
                                            {
                                                StartTime = item.StartTime;
                                                downloadStatus = item.Status;
                                                FileSize += (ulong)item.TotalDownloadCount;
                                                spe = NetClass.ConversionSize(item.DownloadSpe, NetClass.ConversionSizeType.BitRate);
                                            }
                                            i++;
                                            tables.AddRow(i, A1.Value.uid, A1.Value.room_id, A1.Value.uname, A1.Value.title,  NetClass.ConversionSize(FileSize), spe, downloadStatus, StartTime.ToString("yyyy-MM-dd HH:mm:ss"),A1.Value.IsRecDanmu?"YES":"NO");
                                        }
                                    }
                                    tables.Write();
                                    break;
                                }
                            case ConsoleKey.B:
                                {
                                    Console.WriteLine("确定一键导入关注列表中的V吗？按'Y'导入，按任意键取消");
                                    switch (Console.ReadKey().Key)
                                    {
                                        case ConsoleKey.Y:
                                            SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid));
                                            break;
                                        case ConsoleKey.F12://隐藏操作
                                            //一键导入所有关注列表
                                            SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid), true);
                                            break;
                                        default:
                                            Console.WriteLine("放弃导入");
                                            break;
                                    }
                                    break;
                                }
                            case ConsoleKey.C:
                                {
                                  Console.WriteLine("按[Y]确认进入重新登陆流程，按其他键退出菜单");
                                    if(Console.ReadKey().Key.Equals(ConsoleKey.Y))
                                    {
                                        if (SystemAssembly.BilibiliModule.User.login.VerifyLogin.QRLogin(InitType))
                                        {
                                            //重新登陆完成的操作，已经在QRLogin方法里面进行日志和控制台消息的打印，这里就不进行再操作了
                                        }
                                        else
                                        {
                                            //这里如果为false说明InitType为GUI，走单独的UI登陆流程，控制台不打印二维码
                                        }
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            });
        }

      

    }
}
