using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly
{
    public class InitDDTV_Core
    { 
        public static string Ver = "3.0.1.1-dev";
        public static void Init()
        {
            Console.WriteLine($"========================\nDDTV_Core启动，当前Core版本:{Ver}\n========================");
            Log.Log.LogInit(Log.LogClass.LogType.Debug);
            ConfigModule.CoreConfig.ConfigInit();
            BilibiliModule.User.BilibiliUser.Init();
            RoomPatrolModule.RoomPatrol.Init();
            //BilibiliModule.API.RoomInfo.room_init(2299184);
            //string url = BilibiliModule.API.RoomInfo.playUrl(2299184, BilibiliModule.Rooms.RoomInfoClass.PlayQuality.OriginalPainting);
            //DownloadModule.Download.DownFLV_WebClient(url);
            //BilibiliModule.API.DanMu.send(BilibiliModule.Rooms.Rooms.GetValue(408490081, DataCacheModule.DataCacheClass.CacheType.room_id), "DDTV3.0弹幕发送测试");
        }
        public enum SatrtType
        {
            DDTV_Core=0,
            DDTV_GUI=1,
            DDTV_CLI=2,
            DDTV_Other=int.MaxValue
        }
    }
}
