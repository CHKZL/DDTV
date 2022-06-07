using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;

namespace DDTV_Core.SystemAssembly.RoomPatrolModule
{
    public class RoomPatrol
    {
        public static bool IsOn = true;
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
                    if (bool.Parse(Rooms.GetValue(item.Value.uid, DataCacheModule.DataCacheClass.CacheType.IsAutoRec)))
                    {
                        //自动录制
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"【●录制】根据配置开始自动录制【{item.Value.room_id}-{item.Value.uname}】的直播流-标题：[{item.Value.title}]");
                        DownloadModule.Download.AddDownloadTaskd(item.Value.uid,true);
                        //StartRec.Invoke(item.Value, EventArgs.Empty);
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"【◎忽略】检测到【{item.Value.room_id}-{item.Value.uname}】开播-标题：[{item.Value.title}]");
                       
                        //StartLive.Invoke(item.Value, EventArgs.Empty);
                    }
                    WebHook.SendHook(WebHook.HookType.StartLive, item.Value.uid);
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

                        if (IsOn)
                            Patrol();
                        ETime = Tool.TimeModule.Time.Operate.GetRunMilliseconds();
                        Thread.Sleep(8 * 1000);
                    }
                    catch (Exception e)
                    {
                        if ((ETime + 60000) < Tool.TimeModule.Time.Operate.GetRunMilliseconds())
                        {
                            Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Warn_RoomPatrol, $"房间巡逻出现错误，错误信息已写入日志文件，2秒后重试", true, e,false);
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
                if (item.Value.live_status == 1 && keyValuePairs[item.Value.uid] != 1)
                {
                    //Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"检测到【{item.Value.room_id}-{item.Value.uname}】开播-标题[{item.Value.title}]");
                    //开播了
                    if (item.Value.IsAutoRec)
                    {
                        //自动录制
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"【●录制】根据配置开始自动录制【{item.Value.room_id}-{item.Value.uname}】的直播流-标题：[{item.Value.title}]");
                        DownloadModule.Download.AddDownloadTaskd(item.Value.uid, true);

                        if (StartRec != null)
                        {
                            StartRec.Invoke(item.Value, EventArgs.Empty);
                        }

                    }
                    else
                    {
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"【◎忽略】检测到【{item.Value.room_id}-{item.Value.uname}】开播-标题：[{item.Value.title}]");
                    }
                    if (item.Value.IsRemind)
                    {
                        //开播提醒
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"开播提醒:【{item.Value.room_id}-{item.Value.uname}】开播-标题：[{item.Value.title}]");
                        if (StartLive != null)
                        {
                            StartLive.Invoke(item.Value, EventArgs.Empty);
                        }
                    }
                    WebHook.SendHook(WebHook.HookType.StartLive, item.Value.uid);
                }
                else if (item.Value.live_status == 0 && keyValuePairs[item.Value.uid] != 0)
                {
                    if (item.Value.IsAutoRec)
                    {
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"【■下播】设定的录制对象【{item.Value.room_id}-{item.Value.uname}】结束直播-标题：[{item.Value.title}]");
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.Info, $"【□下播】未设置录制任务的对象【{item.Value.room_id}-{item.Value.uname}】结束直播-标题：[{item.Value.title}]");
                    }
                    WebHook.SendHook(WebHook.HookType.StopLive, item.Value.uid);
                }
                Log.Log.AddLog(nameof(RoomPatrol), Log.LogClass.LogType.TmpInfo, $"周期性更新房间信息成功");
            }
        }
    }
}
