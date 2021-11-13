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
        }  
    }
}
