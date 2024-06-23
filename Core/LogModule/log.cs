using Core.Tools.ColorConsole;
using Masuit.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.LogModule.LogClass;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.LogModule
{
    public class Log
    {
        /// <summary>
        /// 日志等级(向下包含)
        /// </summary>
        private static LogClass.LogType LogLevel = LogClass.LogType.All;
        /// <summary>
        /// 日志记录
        /// </summary>
        //public static List<LogClass> LogList = new();
        /// <summary>
        /// 新增日志事件
        /// </summary>
        public static event EventHandler<EventArgs> LogAddEvent;

        public static List<LogClass> LogList = new List<LogClass>();

        private static ConsoleWriter console = new ConsoleWriter();


        private static void Log_LogAddEvent(object? sender, EventArgs e)
        {
            LogList.Insert(0, (LogClass)sender);
            while(LogList.Count>100)
            {
                LogList.RemoveAt(LogList.Count - 1);
            }
        }
        /// <summary>
        /// Log系统初始化
        /// </summary>
        /// <param name="log">日志等级</param>
        public static void LogInit(LogClass.LogType log = LogClass.LogType.Debug)
        {
            //Console.ForegroundColor = ConsoleColor.DarkYellow;
            LogLevel = log;
            Tools.Time.Config.Init();
            LogDB.Config.SQLiteInit(false);
             Log.LogAddEvent += Log_LogAddEvent;
#if DEBUG
            Info(nameof(Log), $"{Init.InitType}|{Init.Ver}【Dev】(编译时间:{Init.CompiledVersion})");
            Info(nameof(Log), "Log系统初始化完成（Dev模式）");
#else
            Info(nameof(Log),  $"{Init.InitType}|{Init.Ver} (编译时间:{Init.CompiledVersion})");
            Info(nameof(Log),  "Log系统初始化完成");
#endif
        }
        /// <summary>
        /// 调试信息
        /// </summary>
        /// <param name="Source">来源</param>
        /// <param name="Message">信息</param>
        /// <param name="IsDisplay">是否显示</param>
        public static void Debug(string Source, string Message, bool IsDisplay = true)
        {
            LogClass logClass = new()
            {
                RunningTime = Tools.Time.Operate.GetRunMilliseconds(),
                Message = Message,
                Source =Source,
                Time = DateTime.Now,
                Type = LogClass.LogType.Debug,
                IsDisplay = IsDisplay,
            };
            _LogPrint(logClass);
        }
        /// <summary>
        /// 一般信息
        /// </summary>
        /// <param name="Source">来源</param>
        /// <param name="Message">信息</param>
        /// <param name="IsDisplay">是否显示</param>
        public static void Info(string Source, string Message, bool IsDisplay = true)
        {
            LogClass logClass = new()
            {
                RunningTime = Tools.Time.Operate.GetRunMilliseconds(),
                Message = Message,
                Source = Source,
                Time = DateTime.Now,
                Type = LogClass.LogType.Info,
                IsDisplay = IsDisplay,
            };
            _LogPrint(logClass);
        }
        /// <summary>
        /// 警告信息
        /// </summary>
        /// <param name="Source">来源</param>
        /// <param name="Message">信息</param>
        /// <param name="exception">堆栈</param>
        /// <param name="IsDisplay">是否显示</param>
        public static void Warn(string Source, string Message,Exception? exception = null, bool IsDisplay = true)
        {
            LogClass logClass = new()
            {
                RunningTime = Tools.Time.Operate.GetRunMilliseconds(),
                Message = Message,
                Source = Source,
                Time = DateTime.Now,
                Type = LogClass.LogType.Warn,
                IsDisplay = IsDisplay,
            };
            _LogPrint(logClass);
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="Source">来源</param>
        /// <param name="Message">信息</param>
        /// <param name="exception">堆栈</param>
        /// <param name="IsDisplay">是否显示</param>
        public static void Error(string Source, string Message, Exception? exception = null, bool IsDisplay = true)
        {
            LogClass logClass = new()
            {
                RunningTime = Tools.Time.Operate.GetRunMilliseconds(),
                Message = Message,
                Source = Source,
                Time = DateTime.Now,
                Type = LogClass.LogType.Error,
                IsError = true,
                exception = exception,
                IsDisplay = IsDisplay,
            };
            _LogPrint(logClass);
        }
        /// <summary>
        /// 额外的日志信息
        /// </summary>
        /// <param name="logClass">日志类</param>
        public static void Exception(LogClass logClass)
        {
            _LogPrint(logClass);
        }

        private static async Task _LogPrint(LogClass logClass)
        {
            await Task.Run(() =>
            {
                if (logClass != null)
                {
                    lock (console)
                    {
                        if (logClass.IsError)
                        {
                            logClass.Message = logClass.Message + " ,详细信息已写入txt文本日志中";                      
                            string ErrorText = $"\n{logClass.Time}:[Error][{logClass.Source}][{logClass.RunningTime}]{logClass.Message}";
                            if (logClass.exception != null)
                            {
                                ErrorText += $"错误堆栈\n{logClass.exception.ToString()}";
                            }
                            Task.Run(() =>
                            {
                                lock(LogDB.streamWriter) 
                                { 
                                    LogDB.streamWriter.WriteLine(ErrorText);
                                    LogDB.streamWriter.Flush();
                                }
                                
                                //Thread.Sleep(100);
                            });

                        }
#if DEBUG
                        if (true)
#else
                        if (logClass.Type <= LogLevel && logClass.Type != LogClass.LogType.Info_Transcod && logClass.IsDisplay && ( Config.Core_RunConfig._DebugMode || logClass.Type< LogType.Debug))
#endif
                        {
                            LogList.Add(logClass);
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

                            LogAddEvent?.Invoke(logClass, EventArgs.Empty);

                        }
                        if (logClass.Type < LogClass.LogType.Trace)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    LogDB.Operate.AddDb(logClass);
                                }
                                catch (Exception) { }
                            });
                        }
                    }

                }
                else
                {
                    ;
                }
            });
        }
    }
}
