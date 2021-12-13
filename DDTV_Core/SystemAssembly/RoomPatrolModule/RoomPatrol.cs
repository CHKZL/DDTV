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
        /// 开始直播事件
        /// </summary>
        public static event EventHandler<EventArgs> StartLive;
        /// <summary>
        /// 开始录制事件
        /// </summary>
        public static event EventHandler<EventArgs> StartRec;
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
                    //Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"检测到【{item.Value.room_id}-{item.Value.uname}】开播-标题[{item.Value.title}]");
                    if (bool.Parse(Rooms.GetValue(item.Value.uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)))
                    {
                        //自动录制
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"检测到开播根据配置开始自动录制【{item.Value.room_id}-{item.Value.uname}】：[{item.Value.title}]");
                        DownloadModule.Download.AddDownloadTaskd(item.Value.uid,true);
                        //StartRec.Invoke(item.Value, EventArgs.Empty);
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"检测到【{item.Value.room_id}-{item.Value.uname}】开播-：[{item.Value.title}]，根据配置忽略");
                        //StartLive.Invoke(item.Value, EventArgs.Empty);
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
                        ETime = TimeModule.Time.Operate.GetRunMilliseconds();
                        Thread.Sleep(8 * 1000);
                    }
                    catch (Exception e)
                    {
                        if ((ETime + 40000) < TimeModule.Time.Operate.GetRunMilliseconds())
                        {
                            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Warn_RoomPatrol, $"房间巡逻出现错误，错误信息已写入日志文件，2秒后重试", true, e);
                        }                     
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
            foreach (var item in Rooms.RoomInfo)
            {
                keyValuePairs.Add(item.Key, item.Value.live_status);
            }
            Rooms.UpdateRoomInfo();
            foreach (var item in Rooms.RoomInfo)
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
                            StartRec.Invoke(item.Value, EventArgs.Empty);
                        }
                        if (item.Value.IsRemind)
                        {
                            //开播提醒警告！
                            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"开播提醒:【{item.Value.room_id}-{item.Value.uname}】");
                            StartLive.Invoke(item.Value, EventArgs.Empty);
                        }
                    }
                }
            }
            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.TmpInfo, $"周期性更新房间信息成功");
        }
    }
}
