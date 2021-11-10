using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace DDTV_Core.SystemModule.Log
{
    /// <summary>
    /// 核心日志信息处理
    /// </summary>
    public class Log
    {
        private static int LogLevel = (int)LogClass.LogType.All;
        private static readonly Stopwatch RunningTimeStopwatch = new();

        public static void AddLog(string Source,LogClass.LogType logType,string Message)
        {
            Task.Run(() => 
            {
                LogClass logClass = new()
                {
                    RunningTime = RunningTimeStopwatch.Elapsed.Seconds,
                    Message = Message,
                    Source = Source,
                    Time = DateTime.Now,
                    Type = logType
                };
                if ((int)logType < LogLevel)
                {
                    Console.WriteLine($"{logClass.Time}:[{Enum.GetName(typeof(LogClass.LogType), (int)logType)}][{Source}]{Message}");
                }
               if (!LogDB.Operate.AddDb(logClass))
                {
                    ErrorLogFileWrite("LogSystem","日志数据库写入失败");
                }
            }).Start();
        }
        public static void ErrorLogFileWrite(string Source,string Message)
        {
            string ErrorText = $"{DateTime.Now}:[Error][{Source}][{RunningTimeStopwatch.Elapsed.Seconds}]{Message}！";
            File.AppendAllText(LogDB.ErrorFilePath, ErrorText, Encoding.UTF8);
        }
        public static void LogInit(LogClass.LogType log = LogClass.LogType.All)
        {
            LogLevel= (int)log;
            RunningTimeStopwatch.Start();
        }
    }
}
