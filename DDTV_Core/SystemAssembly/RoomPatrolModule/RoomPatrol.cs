using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;

namespace DDTV_Core.SystemAssembly.RoomPatrolModule
{
    public class RoomPatrol
    {
        /// <summary>
        /// 房间巡逻(房间状态监控)初始化
        /// </summary>
        public static void Init()
        {
            Rooms.UpdateRoomInfo();
            foreach (var item in Rooms.RoomInfo)
            {
                if (item.Value.live_status==1)
                {
                    Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"检测到【{item.Value.room_id}-{item.Value.uname}】开播-标题[{item.Value.title}]");
                    if (bool.Parse(Rooms.GetValue(item.Value.uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)))
                    {
                        //自动录制
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"根据配置开始自动录制【{item.Value.room_id}-{item.Value.uname}】的直播流");
                        DownloadModule.Download.AddDownloadTaskd(item.Value.uid,true);
                        
                    }
                }
            }
            Task.Run(() => 
            {
                long ETime = 0;
                while (true)
                {     
                    try
                    {
                        Thread.Sleep(2 * 1000);
                        Patrol();
                        Thread.Sleep(8 * 1000);
                    }
                    catch (Exception e)
                    {
                        if ((ETime + 300000) < TimeModule.Time.Operate.GetRunMilliseconds())
                        {
                            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Warn, $"房间巡逻出现错误，错误信息已写入日志文件，2秒后重试", true, e);
                        }
                        ETime = TimeModule.Time.Operate.GetRunMilliseconds();
                    }
                }
            });
        }
        /// <summary>
        /// 房间更新状态监测
        /// </summary>
        public static void Patrol()
        {

            Dictionary<long, int> keyValuePairs = new Dictionary<long, int>();
            foreach (var item in BilibiliModule.Rooms.Rooms.RoomInfo)
            {
                keyValuePairs.Add(item.Key, item.Value.live_status);
            }
            BilibiliModule.Rooms.Rooms.UpdateRoomInfo();
            foreach (var item in BilibiliModule.Rooms.Rooms.RoomInfo)
            {
                if(item.Value.live_status==1)
                {
                    if(keyValuePairs[item.Value.uid]!=1)
                    {
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"检测到【{item.Value.room_id}-{item.Value.uname}】开播-标题[{item.Value.title}]");
                        //开播了
                        if(item.Value.IsAutoRec)
                        {
                            //自动录制警告！
                            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"根据配置开始自动录制【{item.Value.room_id}-{item.Value.uname}】的直播流");
                            DownloadModule.Download.AddDownloadTaskd(item.Value.uid, true);
                        }
                        if (item.Value.IsRemind)
                        {
                            //开播提醒警告！
                            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"开播提醒:【{item.Value.room_id}-{item.Value.uname}】");
                        }
                    }
                }
            }
            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.TmpInfo, $"周期性更新房间信息成功");
        }
    }
}
