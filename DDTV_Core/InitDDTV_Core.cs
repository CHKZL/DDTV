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

namespace DDTV_Core
{
    public class InitDDTV_Core
    {
        /// <summary>
        /// Core的版本号
        /// </summary>
        public static string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name+"-"+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
        /// <summary>
        /// 初始化COre
        /// </summary>
        public static void Core_Init(SatrtType satrtType = SatrtType.DDTV_Core)
        {
            Console.WriteLine($"========================\nDDTV_Core启动，当前版本:{Ver}\n========================");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 512;
            ServicePointManager.Expect100Continue = false;
            Log.LogInit(LogClass.LogType.Debug);
            SystemAssembly.ConfigModule.CoreConfig.ConfigInit();
            SystemAssembly.NetworkRequestModule.NetClass.SAPIEVT();
            SystemAssembly.RoomPatrolModule.RoomPatrol.Init();
 
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
            //var roomInfo = SystemAssembly.BilibiliModule.API.WebSocket.WebSocket.ConnectRoomAsync(439605619);
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
            while (true)
            {
                if (Console.ReadKey().Key.Equals(ConsoleKey.I))
                {
                    Console.WriteLine($"请按对应的按键查看或修改配置：\n" +
                         $"a：查看下载中的任务情况\n" +
                         $"b：查看调用阿B的API次数\n" +
                         $"c：查看API查询次数");
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.A:
                            {
                                int i = 0;
                                Console.WriteLine($"下载中的任务:");
                                foreach (var A1 in SystemAssembly.BilibiliModule.Rooms.Rooms.RoomInfo)
                                {
                                    if (A1.Value.DownloadingList.Count > 0)
                                    {
                                        ulong FileSize = 0;
                                        foreach (var item in A1.Value.DownloadingList)
                                        {
                                            FileSize += (ulong)item.DownloadCount;
                                        }
                                        i++;
                                        Console.WriteLine($"{i}：{A1.Value.uid}  {A1.Value.room_id}  {A1.Value.uname}  {A1.Value.title}  {SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(FileSize)}");
                                    }
                                }
                                break;
                            }
                        case ConsoleKey.B:
                            {
                                Console.WriteLine("API使用统计:");
                                foreach (var item in SystemAssembly.NetworkRequestModule.NetClass.API_Usage_Count)
                                {
                                    Console.WriteLine($"{item.Value}次，来源：{item.Key}");
                                }
                                break;
                            }
                        case ConsoleKey.C:
                            {
                                Console.WriteLine("查询API统计:");
                                foreach (var item in SystemAssembly.NetworkRequestModule.NetClass.SelectAPI_Count)
                                {
                                    Console.WriteLine($"{item.Value}次，来源：{item.Key}");
                                }
                                break;
                            }
                    }
                }
            }
        }

        private static void LiveChatListener_MessageReceived(object? sender, MessageEventArgs e)
        {
            switch (e)
            {
                case DanmuMessageEventArgs Danmu:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[收到弹幕信息]{SystemAssembly.TimeModule.Time.Operate.ConvertTimeStampToDateTime(Danmu.Timestamp)} {Danmu.UserName}({Danmu.UserId}):{Danmu.Message}");
                    break;
                case SuperchatEventArg SuperchatEvent:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[收到Superchat信息]{SystemAssembly.TimeModule.Time.Operate.ConvertTimeStampToDateTime(SuperchatEvent.Timestamp)} {SuperchatEvent.UserName}({SuperchatEvent.UserId}):价值[{SuperchatEvent.Price}]的SC信息:【{SuperchatEvent.Message}】,翻译后:【{SuperchatEvent.messageTrans}】");
                    break;
                case GuardBuyEventArgs GuardBuyEvent:
                    Log.AddLog(nameof(LiveChatListener), LogClass.LogType.Info, $"[收到舰组信息]{SystemAssembly.TimeModule.Time.Operate.ConvertTimeStampToDateTime(GuardBuyEvent.Timestamp)} {GuardBuyEvent.UserName}({GuardBuyEvent.UserId}):开通了{GuardBuyEvent.Number}个月的{GuardBuyEvent.GiftName}(单价{GuardBuyEvent.Price})");
                    break;
                case SendGiftEventArgs sendGiftEventArgs:
                    Log.AddLog(nameof(LiveChatListener),LogClass.LogType.Info, $"[收到礼物]{SystemAssembly.TimeModule.Time.Operate.ConvertTimeStampToDateTime(sendGiftEventArgs.Timestamp)} {sendGiftEventArgs.UserName}({sendGiftEventArgs.UserId}):价值{sendGiftEventArgs.GiftPrice}的{sendGiftEventArgs.Amount}个{sendGiftEventArgs.GiftName}");
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
            DDTV_Core=0,
            DDTV_GUI=1,
            DDTV_CLI=2,
            DDTV_Other=int.MaxValue
        }
    }
}
