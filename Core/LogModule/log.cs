using Core.Tools.ColorConsole;
using Masuit.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Core.LogModule.LogClass;

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
        private static readonly object _logListLock = new object();

        /// <summary>
        /// 获取日志列表快照（加锁拷贝，调用方可安全遍历，不受消费者线程写入影响）
        /// </summary>
        public static LogClass[] GetLogListSnapshot()
        {
            lock (_logListLock)
            {
                return LogList.ToArray();
            }
        }

        private static ConsoleWriter console = new ConsoleWriter();

        /// <summary>
        /// 日志队列（单消费者模式，业务线程写入后立即返回，避免日志IO阻塞业务）
        /// </summary>
        private static readonly Channel<LogClass> _logChannel = Channel.CreateBounded<LogClass>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
        private static int _consumerStarted = 0;
        /// <summary>
        /// 因队列满被丢弃的日志条数
        /// </summary>
        private static long _droppedCount = 0;

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
        public static void Warn(string Source, string Message, Exception? exception = null, bool IsDisplay = true)
        {
            LogClass logClass = new()
            {
                RunningTime = Tools.Time.Operate.GetRunMilliseconds(),
                Message = Message,
                Source = Source,
                Time = DateTime.Now,
                Type = LogClass.LogType.Warn,
                Error = true,
                exception = exception,
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
                Error = true,
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

        /// <summary>
        /// 写入日志队列并立即返回，实际的控制台/文件/数据库输出由独立消费者线程批量处理
        /// </summary>
        private static void _LogPrint(LogClass logClass)
        {
            if (logClass == null)
                return;
            //惰性启动消费者线程（保证LogInit之前的日志也能入队）
            if (Interlocked.CompareExchange(ref _consumerStarted, 1, 0) == 0)
            {
                //Task.Factory.StartNew对async委托会返回Task<Task>，专用线程在第一个await即退出，LongRunning意图落空；Task.Run正确解包
                Task.Run(LogConsumerLoop);
            }
            if (!_logChannel.Writer.TryWrite(logClass))
            {
                Interlocked.Increment(ref _droppedCount);
            }
        }

        /// <summary>
        /// 日志消费者主循环：批量取出日志，统一做控制台输出、txt错误日志、SQLite批量写入和事件通知
        /// </summary>
        private static async Task LogConsumerLoop()
        {
            List<LogClass> batch = new(512);
            List<LogClass> dbBatch = new(512);
            while (await _logChannel.Reader.WaitToReadAsync())
            {
                try
                {
                    batch.Clear();
                    dbBatch.Clear();
                    while (batch.Count < 512 && _logChannel.Reader.TryRead(out LogClass? item))
                    {
                        if (item != null)
                            batch.Add(item);
                    }
                    bool hasErrorLog = false;
                    foreach (LogClass logClass in batch)
                    {
                        try
                        {
                            if (logClass.Error)
                            {
                                logClass.Message = logClass.Message + " ,详细信息已写入txt文本日志中";
                                if (LogDB.streamWriter != null)
                                {
                                    string ErrorText = $"\n{logClass.Time}:[{logClass.Type.ToString()}][{logClass.Source}][{logClass.RunningTime}]{logClass.Message}";
                                    if (logClass.exception != null)
                                    {
                                        ErrorText += $"错误堆栈\n{logClass.exception.ToString()}";
                                    }
                                    lock (LogDB.ErrorFileLock)
                                    {
                                        LogDB.streamWriter.WriteLine(ErrorText);
                                    }
                                    hasErrorLog = true;
                                }
                            }
#if DEBUG
                            if (true)
#else
                        if (logClass.Type <= LogLevel && logClass.Type != LogClass.LogType.Info_Transcod && logClass.IsDisplay && (Config.Core_RunConfig._DebugMode || logClass.Type < LogType.Debug))
#endif
                            {
                                lock (_logListLock)
                                {
                                    LogList.Insert(0, logClass);
                                    while (LogList.Count > 100)
                                    {
                                        LogList.RemoveAt(LogList.Count - 1);
                                    }
                                }
                                WriteConsole(logClass);
                                try
                                {
                                    LogAddEvent?.Invoke(logClass, EventArgs.Empty);
                                }
                                catch (Exception)
                                {
                                    //订阅者异常不能影响日志消费者线程
                                }
                            }
                            if (logClass.Type < LogClass.LogType.Trace)
                            {
                                dbBatch.Add(logClass);
                            }
                        }
                        catch (Exception)
                        {
                            //单条日志处理异常不影响整批
                        }
                    }
                    //错误文本日志每批统一Flush一次，避免每条日志强制落盘
                    if (hasErrorLog && LogDB.streamWriter != null)
                    {
                        lock (LogDB.ErrorFileLock)
                        {
                            LogDB.streamWriter.Flush();
                        }
                    }
                    //SQLite批量写入，一个事务代替每条日志一个事务
                    if (dbBatch.Count > 0)
                    {
                        try
                        {
                            LogDB.Operate.AddDbBatch(dbBatch);
                        }
                        catch (Exception) { }
                    }
                    long dropped = Interlocked.Exchange(ref _droppedCount, 0);
                    if (dropped > 0)
                    {
                        console.WriteLine($"[Log]日志产生速度超过消费速度，已丢弃{dropped}条", ConsoleColor.DarkYellow);
                    }
                }
                catch (Exception)
                {
                    //整批处理出现异常（如磁盘写失败）时跳过该批，保证消费者线程不退出、日志系统不静默死亡
                }
            }
        }

        /// <summary>
        /// 控制台彩色输出（仅由日志消费者线程调用，无需加锁）
        /// </summary>
        private static void WriteConsole(LogClass logClass)
        {
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
        }
    }
}
