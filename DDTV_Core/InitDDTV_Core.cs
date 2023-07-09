using ConsoleTableExt;
using DDTV_Core.SystemAssembly.BilibiliModule.API;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;
using DDTV_Core.SystemAssembly.RoomPatrolModule;
using DDTV_Core.Tool;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass.Downloads;

namespace DDTV_Core
{
    public class InitDDTV_Core
    {
        /// <summary>
        /// Core的版本号
        /// </summary>
        public static string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string ClientAID = string.Empty;
        public static SatrtType InitType = SatrtType.DDTV_Core;
        public static string CompiledVersion = "2023-07-09 22:29:10";
        public static bool WhetherInitializationIsComplet = false;//是否初始化完成
        public static string UpdateNotice = string.Empty;
        public static bool IsDevDebug = false;

        /// <summary>
        /// 初始化COre
        /// </summary>
        /// <param name="satrtType">启动类型</param>
        /// <param name="EAU">是否启用自动更新和提示</param>
        public static void Core_Init(SatrtType satrtType = SatrtType.DDTV_Core, bool EAU = true)
        {
            string? DockerEnvironment = Environment.GetEnvironmentVariable("DDTV_Docker_Project");
            if (!string.IsNullOrEmpty(DockerEnvironment))
            {
                switch (DockerEnvironment)
                {
                    case "DDTV_CLI":
                        InitType = SatrtType.DDTV_CLI_Docker;
                        break;
                    case "DDTV_WEB_Server":
                        InitType = SatrtType.DDTV_WEB_Docker;
                        break;
                    default:
                        InitType = satrtType;
                        break;
                }
            }
            else
            {
                InitType = satrtType;
            }
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            Console.WriteLine($"========================\nDDTV_Core开始启动，当前版本:{InitType} {Ver}(编译时间:{CompiledVersion})\n========================");

            Log.LogInit(LogClass.LogType.Debug);//初始化日志系统，设置日志输出等级
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//设置使用的最低安全协议（B站目前为Tls12）
            ServicePointManager.DefaultConnectionLimit = 1024 * 1024 * 8;//连接队列上线
            ServicePointManager.Expect100Continue = false;//禁止查询服务端post状态防止卡死
            CoreConfig.ConfigInit(InitType);//初始化设置系统
            DDTV_Update.CheckUpdateProgram();//检查更新器是否有新版本
            Task.Run(() => Tool.DDcenter.Init(InitType));//初始化DDC采集系统
            if (InitType != SatrtType.DDTV_GUI && InitType != SatrtType.DDTV_DanMu)
            {
                SeKey();//启动控制台菜单监听
                BilibiliUserConfig.CheckAccount.CheckAccountChanged += CheckAccount_CheckAccountChanged;//注册登陆信息检查失效WebHook事件
                FileOperation.PathAlmostFull += FileOperation_PathAlmostFull;//注册硬盘空间不足WebHook事件
            }
            FileOperation.RemainingSpaceWarningDetection();//开始监控硬盘剩余空间
            if (!EAU)//如果带--no-update则关闭自动更新功能
                CoreConfig.AutoInsallUpdate = false;
            Console.WriteLine($"========================\nDDTV_Core启动完成\n========================");
            Console.WriteLine($"有任何问题欢迎加群：338182356 直接沟通");
            //if(UserInfo.LoginValidityVerification())
            //{
            //    Log.AddLog(nameof(InitDDTV_Core), LogClass.LogType.Error, "登陆已失效！请重新登陆！", false, null, false);
            //}
            switch (InitType)
            {
                case SatrtType.DDTV_Core:
                    ServerInteraction.CheckUpdates.Update("Core");
                    ServerInteraction.Dokidoki.Start("Core");
                    BilibiliUserConfig.CheckAccount.CheckLoginValidity();
                    break;
                case SatrtType.DDTV_CLI:
                    ServerInteraction.CheckUpdates.Update("CLI");
                    ServerInteraction.Dokidoki.Start("CLI");
                    BilibiliUserConfig.CheckAccount.CheckLoginValidity();
                    break;
                case SatrtType.DDTV_CLI_Docker:
                    ServerInteraction.CheckUpdates.Update("CLI");
                    ServerInteraction.Dokidoki.Start("CLI");
                    BilibiliUserConfig.CheckAccount.CheckLoginValidity();
                    break;
                case SatrtType.DDTV_WEB:
                    ServerInteraction.CheckUpdates.Update("WEB");
                    ServerInteraction.Dokidoki.Start("WEB");
                    ServerInteraction.UpdateNotice.Start("WEB");
                    BilibiliUserConfig.CheckAccount.CheckLoginValidity();
                    break;
                case SatrtType.DDTV_WEB_Docker:
                    ServerInteraction.CheckUpdates.Update("WEB");
                    ServerInteraction.Dokidoki.Start("WEB");
                    ServerInteraction.UpdateNotice.Start("WEB");
                    BilibiliUserConfig.CheckAccount.CheckLoginValidity();
                    break;
                case SatrtType.DDTV_GUI:
                    //ServerInteraction.CheckUpdates.Update("GUI");
                    ServerInteraction.Dokidoki.Start("GUI");
                    break;
                case SatrtType.DDTV_DanMu:
                    break;
                default:
                    //ServerInteraction.CheckUpdates.Update("Core");
                    ServerInteraction.Dokidoki.Start("Core");
                    break;
            }
            WhetherInitializationIsComplet = true;
        }


        private static void FileOperation_PathAlmostFull(object? sender, string e)
        {
            Log.AddLog("HardDisk", LogClass.LogType.Error_IsAboutToHappen, e);
            WebHook.SendHook(WebHook.HookType.SpaceIsInsufficientWarn, 0);
        }

        public static string GetPackageVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        /// <summary>
        /// 登陆状态失效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CheckAccount_CheckAccountChanged(object? sender, EventArgs e)
        {
            //RoomPatrol.IsOn = false;
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
                            Log.AddLog("Login", LogClass.LogType.Warn, "账号登陆失效！请进入[i]键菜单根据提示重新登陆\n当前出于无登陆信息应急模式，该模式可能会导致触发阿B风控阻拦，请尽快登陆！！！", false, null, true);
                            WebHook.SendHook(WebHook.HookType.LoginFailure, 0);
                            break;
                        case BilibiliUserConfig.LoginStatus.LoggingIn:
                            Log.AddLog("Login", LogClass.LogType.Info, "等待登陆中...", false, null, false);
                            break;
                    }
                    Thread.Sleep(30 * 1000);
                }
            });
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
            DDTV_CLI_Docker,
            DDTV_WEB_Docker,
            DDTV_DanMu,
            DDTV_Other = int.MaxValue
        }

        private static void SeKey()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (Console.ReadKey().Key.Equals(ConsoleKey.I))
                    {
                        Console.WriteLine($"请按对应的按键查看或修改配置：");
                        Console.WriteLine($"a：查看下载中的任务情况");
                        Console.WriteLine($"b：一键导入关注列表中的V(可能不全需要自己补一下)");
                        Console.WriteLine($"c：重登登陆");
                        Console.WriteLine($"q：退出DDTV");
                        Console.WriteLine($"z：控制台开发者监控模式开关(会显示所有上下播信息和一大堆调试信息)");
                        switch (Console.ReadKey().Key)
                        {
                            case ConsoleKey.A:
                                {
                                    int i = 0;
                                    //ConsoleTable tables = new ConsoleTable("序号", "UID", "房间号", "昵称", "直播标题", "已下载大小", "下载速率", "状态", "开始时间", "是否录制弹幕信息");
                                    var tableData = new List<List<object>> { new List<object> { "序号", "UID", "房间号", "昵称", "直播标题", "已下载大小", "下载速率", "状态", "开始时间", "是否录制弹幕信息", "录制模式" } };
                                    foreach (var A1 in Rooms.RoomInfo)
                                    {
                                        if (A1.Value.DownloadingList.Count > 0)
                                        {
                                            DateTime StartTime = DateTime.Now;
                                            DownloadStatus downloadStatus = DownloadStatus.Standby;
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

                                            tableData.Add(new List<object> { i, A1.Value.uid, A1.Value.room_id, A1.Value.uname, A1.Value.title, NetClass.ConversionSize(FileSize), spe, downloadStatus, StartTime.ToString("yyyy-MM-dd HH:mm:ss"), A1.Value.IsRecDanmu ? "YES" : "NO", A1.Value.CurrentMode == 0 ? "FLV" : "HLS" });



                                            //tables.AddRow(i, A1.Value.uid, A1.Value.room_id, A1.Value.uname, A1.Value.title, NetClass.ConversionSize(FileSize), spe, downloadStatus, StartTime.ToString("yyyy-MM-dd HH:mm:ss"), A1.Value.IsRecDanmu ? "YES" : "NO");
                                        }
                                    }
                                    ConsoleTableBuilder.From(tableData).WithTitle("下载中的任务列表").ExportAndWriteLine();

                                    //tables.Write();
                                    break;
                                }
                            case ConsoleKey.B:
                                {
                                    Console.WriteLine("确定一键导入关注列表中的V吗？按'Y'导入，按任意键取消");
                                    switch (Console.ReadKey().Key)
                                    {
                                        case ConsoleKey.Y://导入关注列表中的V
                                            {
                                                SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid));
                                                break;
                                            }
                                        //case ConsoleKey.W://隐藏操作，导入vtbs列表中的所有人         
                                        //    {
                                        //        Console.WriteLine("触发隐藏操作，按Y导入vtbs列表中的所有人，按其他键取消");
                                        //        if (Console.ReadKey().Key.Equals(ConsoleKey.Y))
                                        //        {
                                        //            //一键导入本地vtbs文件中的所有V
                                        //            SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid), false, true);
                                        //        }
                                        //        break;
                                        //    }
                                        //case ConsoleKey.Q://隐藏操作，导入关注列表中的所有人
                                        //    {
                                        //        Console.WriteLine("触发隐藏操作，按Y导入关注列表中的所有人，按其他键取消");
                                        //        //一键导入所有关注列表
                                        //        if (Console.ReadKey().Key.Equals(ConsoleKey.Y))
                                        //        {
                                        //            SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid), true);
                                        //        }
                                        //        break;
                                        //    }
                                        default:
                                            Console.WriteLine("放弃导入");
                                            break;
                                    }
                                    break;
                                }
                            case ConsoleKey.C:
                                {
                                    Console.WriteLine("按[Y]确认进入重新登陆流程，按其他键退出菜单");
                                    if (Console.ReadKey().Key.Equals(ConsoleKey.Y))
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
                            case ConsoleKey.Q:
                                {
                                    Console.WriteLine("确定要退出DDTV么？YES/NO");
                                    Console.WriteLine(":");
                                    Console.WriteLine();
                                    string? Cons = Console.ReadLine();
                                    if (!string.IsNullOrEmpty(Cons) && (Cons.ToLower() == "yes" || Cons.ToLower() == "y"))
                                    {
                                        Environment.Exit(0);
                                    }
                                    else
                                    {
                                        Console.WriteLine("已返回，DDTV正在运行中...");
                                    }
                                    break;
                                }
                            case ConsoleKey.Z:
                                {
                                    CoreConfig.ConsoleMonitorMode = !CoreConfig.ConsoleMonitorMode;
                                    IsDevDebug = CoreConfig.ConsoleMonitorMode;
                                    Console.WriteLine((CoreConfig.ConsoleMonitorMode ? "打开" : "关闭") + "控制台开发者监控模式开关" + (CoreConfig.ConsoleMonitorMode ? "(打开后控制台会输出每个在列表中的任务开始和结束相信信息和一大堆调试信息" : ""));
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
