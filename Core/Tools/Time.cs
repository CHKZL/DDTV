using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
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
                DateTime startTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.MinValue);
                DateTime dt = startTime.AddMilliseconds(timeStamp);
                return dt;
            }
            /// <summary>
            /// DateTime转时间戳
            /// </summary>
            /// <param name="timeStamp">时间戳 单位：毫秒</param>
            /// <returns>C#时间</returns>
            public static long DateTimeToConvertTimeStamp(DateTime dateTime)
            {
                DateTime UnixTimeStampStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return (long)(dateTime.ToUniversalTime() - UnixTimeStampStart).TotalSeconds;
            }
            /// <summary>
            /// 把秒转换为string时间字符串
            /// </summary>
            /// <param name="time"></param>
            /// <returns></returns>
            public static string SecondToString(uint time)
            {
                string H = "";
                string M = "";
                string S = "";
                if(time<0)
                {
                    return $"未知";
                }
                if (time < 60)
                {
                    if(time.ToString().Length==1)
                    {
                        S = "0" + time.ToString();
                    }
                    else
                    {
                        S = time.ToString();
                    }
                    return $"00:00:{S}";
                }
                if (time < 3600)
                {
                    if ((time % 60).ToString().Length == 1)
                    {
                        S = "0" + (time % 60).ToString();
                    }
                    else
                    {
                        S = (time % 60).ToString();
                    }
                    if ((time / 60).ToString().Length == 1)
                    {
                        M = "0" + (time / 60).ToString();
                    }
                    else
                    {
                        M = (time / 60).ToString();
                    }
                    return $"00:{M}:{S}";
                }
                if ((time % 60).ToString().Length == 1)
                {
                    S = "0" + (time % 60).ToString();
                }
                else
                {
                    S = (time % 60).ToString();
                }
                if ((time / 60).ToString().Length == 1)
                {
                    M = "0" + (time / 60).ToString();
                }
                else
                {
                    M = (time / 60).ToString();
                }
                if ((time / 3600).ToString().Length == 1)
                {
                    H = "0" + (time / 3600).ToString();
                }
                else
                {
                    H = (time / 3600).ToString();
                }
                return $"{H}:{M}:{S}";
            } /// <summary>
        }
    }
}
