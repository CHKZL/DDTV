using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using ColorConsole;

namespace DDTV_Core.SystemAssembly.Log
{
    /// <summary>
    /// 核心日志信息处理
    /// </summary>
    public class Log
    {
       
        /// <summary>
        /// 日志等级(向下包含)
        /// </summary>
        private static LogClass.LogType LogLevel = LogClass.LogType.All;
        /// <summary>
        /// 本地日志的记录
        /// </summary>
        public static List<LogClass> LogList = new();
        /// <summary>
        /// 新增日志事件
        /// </summary>
        public static event EventHandler<EventArgs> LogAddEvent;
        /// <summary>
        /// 是否启用外部日志事件调用
        /// </summary>
        public static bool IsEvent = false;
        /// <summary>
        /// 需要写入数据库的日志队列
        /// </summary>
        private static List<LogClass> LogDBClasses = new List<LogClass>();
        /// <summary>
        /// 
        /// </summary>
        private static ConsoleWriter console = new ConsoleWriter();
        /// <summary>
        /// 删除过期日志类
        /// </summary>
        public static CleanUpExpirLog cleanUpExpirLog = new CleanUpExpirLog();


        /// <summary>
        /// Log系统初始化
        /// </summary>
        /// <param name="log">日志等级</param>
        public static void LogInit(LogClass.LogType log = LogClass.LogType.All)
        {
            //Console.ForegroundColor = ConsoleColor.DarkYellow;
            LogLevel = log;
            Tool.TimeModule.Time.Config.Init();
            LogDB.Config.SQLiteInit(false);

            AddLog("System", LogClass.LogType.Info, $"{InitDDTV_Core.InitType}|{InitDDTV_Core.Ver}({InitDDTV_Core.CompiledVersion})");
            AddLog(nameof(Log), LogClass.LogType.Info, "Log系统初始化完成");
            WeritDB();
            WriteErrorLogFile();
            PrintThread();//新增打印线程，把并行日志打印方法修改为串行，为之后控制台颜色区分做准备
            cleanUpExpirLog.Start();
        }
        /// <summary>
        /// 增加日志
        /// </summary>
        /// <param name="Source">日志来源(类名)</param>
        /// <param name="logType">日志类型</param>
        /// <param name="Message">日志内容</param>
        /// <param name="IsError">是否是错误(错误内容会出了数据库外另外写一份txt文本记录详细错误日志和堆栈)</param>
        /// <param name="exception">IsError为真时有效，错误日志的Exception信息</param>
        /// <param name="IsDisplay">该记录是否在终端打印</param>
        public static void AddLog(string Source, LogClass.LogType logType, string Message, bool IsError = false, Exception? exception = null,bool IsDisplay=true)
        {
            Task.Run(() =>
            {
                LogClass logClass = new()
                {
                    RunningTime = Tool.TimeModule.Time.Operate.GetRunMilliseconds(),
                    Message = Message,
                    Source = Source,
                    Time = DateTime.Now,
                    Type = logType,
                    IsError= IsError,
                    exception= exception,
                    IsDisplay=IsDisplay,
                };
                LogList.Add(logClass);
            });
        }

        /// <summary>
        /// 日志打印线程
        /// </summary>
        private static void PrintThread()
        {
            Task.Run(() => {
                while (true)
                {
                    try
                    {
                        while (LogList.Count > 0)
                        {
                            _LogPrint(LogList[0]);
                            LogList.Remove(LogList[0]);
                        }
                    }
                    catch (Exception) { }
                    if (LogList.Count > 10)
                    {
                        Thread.Sleep(5);
                    }
                    else
                    {
                        Thread.Sleep(30);
                    }
                }
            });
        }

        private static void _LogPrint(LogClass logClass)
        {
            if (logClass != null)
            {
                if (logClass.IsError)
                {
                    ErrorLogFileWrite(logClass);
                    logClass.Message = logClass.Message + " ,详细信息已写入txt文本日志中";
                }
                if (logClass.Type <= LogLevel && logClass.Type != LogClass.LogType.Info_Transcod && logClass.IsDisplay)
                {

                    string _ = $"{logClass.Time}:[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}][{logClass.Source}]{logClass.Message}";
                    console.Write($"{logClass.Time}:", ConsoleColor.White);

                    switch (logClass.Type)
                    {
                        case LogClass.LogType.Error:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.Red);
                            break;
                        case LogClass.LogType.Error_IsAboutToHappen:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.Red);
                            break;
                        case LogClass.LogType.Warn:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.Yellow);
                            break;
                        case LogClass.LogType.Warn_RoomPatrol:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.Yellow);
                            break;
                        case LogClass.LogType.Info:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.Green);
                            break;
                        case LogClass.LogType.Info_Transcod:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.Green);
                            break;
                        case LogClass.LogType.Info_API:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.Green);
                            break;
                        default:
                            console.Write($"[{Enum.GetName(typeof(LogClass.LogType), (int)logClass.Type)}]", ConsoleColor.DarkGray);
                            break;
                    }
                    console.Write($"[{logClass.Source}]", ConsoleColor.DarkGray);
                    console.WriteLine($"{logClass.Message}", ConsoleColor.White);
                    if (IsEvent)
                    {
                        LogAddEvent.Invoke(_, EventArgs.Empty);
                    }

                    //Console.WriteLine($"{_}\n");
                }
                if (logClass.Type < LogClass.LogType.Trace)
                {
                    LogDBClasses.Add(logClass);

                }
            }
        }


        /// <summary>
        /// 启用sqlite数据库写入
        /// </summary>
        private static void WeritDB()
        {
            Task.Run(() => {
            while (true)
                {
                    try
                    {
                        while(LogDBClasses.Count > 0)
                        {
                            if (LogDB.Operate.AddDb(LogDBClasses[0]))
                            {
                                LogDBClasses.RemoveAt(0);
                            }
                        }  
                    }
                    catch (Exception){}
                    Thread.Sleep(30);
                }
            });
        }
        private static List<LogClass> logClasses = new List<LogClass>();
        /// <summary>
        /// 增加错误日志
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Message"></param>
        public static void ErrorLogFileWrite(LogClass logClass)// string Source, string Message)
        {
            logClasses.Add(logClass);     
        }
        /// <summary>
        /// 写错误日志堆栈详情到本地文本
        /// </summary>
        private static void WriteErrorLogFile()
        {
            Task.Run(() => {
                while (true)
                {
                    try
                    {
                        if (logClasses != null && logClasses.Count > 0)
                        {
                            LogClass logClass = new LogClass()
                            {
                                exception= logClasses[0].exception,
                                IsError= logClasses[0].IsError,
                                Message= logClasses[0].Message,
                                RunningTime=logClasses[0].RunningTime,
                                Source=logClasses[0].Source,
                                Time=logClasses[0].Time,
                                Type= logClasses[0].Type,
                            };
                            logClasses.RemoveAt(0);
                            string ErrorText = $"\n\n{logClass.Time}:[Error][{logClass.Source}][{logClass.RunningTime}]{logClass.Message}";
                            if (logClass.exception != null)
                            {
                                ErrorText += $"错误堆栈\n{logClass.exception.ToString()}";
                            }
                            File.AppendAllText(LogDB.ErrorFilePath, ErrorText, Encoding.UTF8);
                            Thread.Sleep(100);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(10);
                    }
                }
            });
        }

        public class CleanUpExpirLog
        {
            public static int Interval = 24 * 60 * 60 * 1000;
            public static bool IsOn = false;
            public void Start()
            {
                if(!IsOn)
                {
                    IsOn = true;
                    Clean();
                }
            }
            private void Clean()
            {
               while(true)
                {
                    try
                    {
                        string[] list = Directory.GetFiles(@"Log");
                        int SqliteFileCount = 0;
                        foreach (string file in list)
                        {
                            if (file.Split('.').Length > 1 && file.Split('.')[1].ToLower() == "sqlite")
                            {
                                SqliteFileCount++;
                            }
                        }
                        if (SqliteFileCount > 10)
                        {
                            DateTime NowTime = DateTime.Now;
                            foreach (string file in list)
                            {
                                if (file.Split('_').Length > 1 && file.Split('-').Length > 5 && file.Split('T').Length > 1)
                                {
                                    DateTime FileTime = DateTime.Parse(file.Split('_')[1].Split('T')[0] + " " + file.Split('_')[1].Split('T')[1].Replace("-", ":").Substring(0, 8));
                                    TimeSpan timeSpan = FileTime.Subtract(NowTime).Duration();
                                    if (timeSpan.TotalDays > 14)
                                    {
                                        DDTV_Core.Tool.FileOperation.Del(file);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog(nameof(Log), LogClass.LogType.Error, "过期Log清理失败，写错误日志", true, e, true);
                    }
                    Thread.Sleep(Interval);
                }
            }
        }
       
    }
}
