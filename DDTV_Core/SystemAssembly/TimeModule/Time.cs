using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.TimeModule
{
    public class Time
    {
        private static readonly Stopwatch RunningTimeStopwatch = new();
     
        public class Config
        {
            public static void Init()
            {
                RunningTimeStopwatch.Start();
            }
        }
        public class Operate
        {
            public static int GetRunSeconds()
            {
                return RunningTimeStopwatch.Elapsed.Seconds;
            }
            public static long GetRunMilliseconds()
            {
                return RunningTimeStopwatch.ElapsedMilliseconds;
            }
            public static int GetRunMinute()
            {
                return RunningTimeStopwatch.Elapsed.Minutes;
            }
            public static int GetRunHour()
            {
                return RunningTimeStopwatch.Elapsed.Hours;
            }
            public static int GetRunDay()
            {
                return RunningTimeStopwatch.Elapsed.Days;
            }
            /// <summary>
            /// 转换时间戳为C#时间
            /// </summary>
            /// <param name="timeStamp">时间戳 单位：毫秒</param>
            /// <returns>C#时间</returns>
            public static DateTime ConvertTimeStampToDateTime(long timeStamp)
            {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                DateTime dt = startTime.AddMilliseconds(timeStamp);
                return dt;
            }
        }  
    }
}
