using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.LogModule
{
    internal class LogDB
    {
        internal static SqliteConnection SQLiteConn = new();
        internal static string dbPath = string.Empty;
        internal static string ErrorFilePath = string.Empty;
        internal static StreamWriter streamWriter = default;
        /// <summary>数据库操作专用锁（SQLiteConn字段会被重新赋值，不能锁可变字段）</summary>
        internal static readonly object DbLock = new();
        /// <summary>错误文本日志写入专用锁（streamWriter字段会被重新赋值，不能锁可变字段）</summary>
        internal static readonly object ErrorFileLock = new();
        /// <summary>DB写入失败记录标志（0=未记录，1=已记录）：仅首次失败写错误txt，恢复后重置，避免与日志通道形成自激循环</summary>
        private static int _dbWriteErrorLogged = 0;

        /// <summary>
        /// 记录DB写入失败：直接写错误txt而不是走Log通道（Error日志会重新进入写库队列→再次失败→再产生Error，形成自激循环）
        /// </summary>
        private static void LogDbWriteError(string operation, Exception e)
        {
            if (System.Threading.Interlocked.Exchange(ref _dbWriteErrorLogged, 1) == 1)
                return;
            try
            {
                if (streamWriter != null)
                {
                    lock (ErrorFileLock)
                    {
                        streamWriter.WriteLine($"\n{DateTime.Now}:[Error][LogDB]{operation}写入日志数据库失败（后续失败将静默，恢复后重置）:{e}");
                        streamWriter.Flush();
                    }
                }
            }
            catch { }
        }

        /// <summary>DB写入成功后重置失败记录标志</summary>
        private static void ResetDbWriteErrorFlag()
        {
            System.Threading.Interlocked.Exchange(ref _dbWriteErrorLogged, 0);
        }




        public static class Config
        {
            #region 初始化数据库
            /// <summary>
            /// 初始化\加载Log数据库
            /// </summary>
            /// <param name="isLoadDb">T为加载,F为新建</param>
            /// <returns></returns>
            public static bool SQLiteInit(bool isLoadDb)
            {
                try
                {
                    if (isLoadDb)
                    {
                        if (string.IsNullOrEmpty(dbPath))
                        {
                            dbPath = GetDbFileName();
                        }
                    }
                    else
                    {
                        dbPath = GetDbFileName();
                    }
                    //连接实例日志数据库
                    lock (DbLock)
                    {
                        //重初始化前释放旧连接，避免旧连接持有的文件句柄泄漏
                        try { SQLiteConn?.Close(); SQLiteConn?.Dispose(); } catch { }
                        SQLiteConn = new SqliteConnection($"Data Source = {dbPath}");
                        SQLiteConn.Open();
                        if (!isLoadDb)
                        {
                            //创建表
                            new SqliteCommand($"create table Log(Source string,Type int,Message string,Time DateTime,RunningTime long)", SQLiteConn).ExecuteNonQuery();
                        }
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(LogDB), $"日志数据库初始化失败:{e}", e, false);
                    return false;
                }
            }
            #endregion
            /// <summary>
            /// 生成日志数据库文件名称和路径
            /// </summary>
            /// <returns></returns>
            private static string GetDbFileName()
            {
                Task.Run(() =>
                {
                    try
                    {
                        CleanOldLogFiles(Core.Config.Core_RunConfig._LogFileDirectory, Core.Config.Core_RunConfig._DurationLogStorage);
                    }
                    catch (Exception)
                    {}
                });
                string date = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss-fffff");
                ErrorFilePath = $"{Core.Config.Core_RunConfig._LogFileDirectory}/DDTVCoreErrorLog_{date}.txt";
                //重初始化前释放旧writer，避免旧句柄占用文件
                try { streamWriter?.Dispose(); } catch { }
                streamWriter = new StreamWriter(ErrorFilePath, true, Encoding.UTF8);
                return $"{Core.Config.Core_RunConfig._LogFileDirectory}/DDTVCoreLog_{date}.sqlite";
            }

            /// <summary>
            /// 清理超过指定时间的旧日志文件
            /// </summary>
            /// <param name="directoryPath">日志文件夹路径</param>
            /// <param name="maxAgeInSeconds">文件最大保留时间(秒，UTC时间戳)</param>
            public static void CleanOldLogFiles(string directoryPath, long maxAgeInSeconds)
            {
                try
                {
                    // 如果目录不存在，直接返回
                    if (!Directory.Exists(directoryPath))
                        return;
                    // 获取当前UTC时间
                    DateTime currentUtcTime = DateTime.UtcNow;
                    // 计算最早保留的时间点
                    DateTime cutoffTime = currentUtcTime.AddSeconds(-maxAgeInSeconds);
                    // 获取目录下所有日志文件
                    var files = Directory.GetFiles(directoryPath)
                        .Where(f => f.Contains("DDTVCoreLog_") || f.Contains("DDTVCoreErrorLog_"));
                    foreach (var file in files)
                    {
                        try
                        {
                            // 获取文件最后写入时间(UTC)
                            DateTime fileLastWriteTimeUtc = File.GetLastWriteTimeUtc(file);
                            // 如果文件时间早于截止时间，则删除
                            if (fileLastWriteTimeUtc < cutoffTime)
                            {
                                Tools.FileOperations.Delete(file,"日志文件超过储存时长，删除");
                            }
                        }
                        catch (Exception ex)
                        {
                            // 处理单个文件删除时的异常，避免影响其他文件
                            Log.Error(nameof(LogDB), $"删除超时日志错误{file}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 处理目录访问等全局异常
                    Log.Error(nameof(LogDB), $"清理旧日志文件时发生错误: {ex.Message}");
                }
            }
        }
        public class Operate
        {
            /// <summary>
            /// 批量增加日志数据库记录（单事务写入，由日志消费者线程调用，避免每条日志一次fsync）
            /// </summary>
            /// <param name="logClasses">日志列表</param>
            /// <returns></returns>
            public static bool AddDbBatch(List<LogClass> logClasses)
            {
                try
                {
                    if (SQLiteConn == null)
                    {
                        SQLiteConn = new();
                    }
                    lock (DbLock)
                    {
                        if (SQLiteConn.State != ConnectionState.Open || logClasses == null || logClasses.Count == 0)
                        {
                            return false;
                        }
                        using SqliteTransaction transaction = SQLiteConn.BeginTransaction();
                        using SqliteCommand cmd = new("insert into Log(Source, Type, Message, Time, RunningTime) values (@Source, @Type, @Message, @Time ,@RunningTime)", SQLiteConn, transaction);
                        SqliteParameter pSource = new("@Source", DbType.String);
                        SqliteParameter pType = new("@Type", DbType.String);
                        SqliteParameter pMessage = new("@Message", DbType.String);
                        SqliteParameter pTime = new("@Time", DbType.DateTime);
                        SqliteParameter pRunningTime = new("@RunningTime", DbType.Int64);
                        cmd.Parameters.AddRange(new[] { pSource, pType, pMessage, pTime, pRunningTime });
                        foreach (LogClass logClass in logClasses)
                        {
                            if (logClass == null || logClass.Source == null)
                                continue;
                            pSource.Value = logClass.Source;
                            pType.Value = logClass.Type;
                            pMessage.Value = logClass.Message;
                            pTime.Value = logClass.Time;
                            pRunningTime.Value = logClass.RunningTime;
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        ResetDbWriteErrorFlag();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    LogDbWriteError("批量写入", e);
                    return false;
                }
            }
            /// <summary>
            /// 增加日志数据库记录
            /// </summary>
            /// <param name="logClass"></param>
            /// <returns></returns>
            public static bool AddDb(LogClass logClass)
            {
                try
                {
                    if (SQLiteConn == null)
                    {
                        SQLiteConn = new();
                    }
                    lock (DbLock)
                    {
                        if (logClass != null && logClass.Source != null)
                        {
                            if (SQLiteConn.State == ConnectionState.Open)
                            {
                                string sqltext = $"insert into Log(Source, Type, Message, Time, RunningTime) values (@Source, @Type, @Message, @Time ,@RunningTime)";
                                SqliteCommand cmd = new(sqltext, SQLiteConn);
                                //构造参数
                                SqliteParameter[] pms = new SqliteParameter[]
                                {
                                    new SqliteParameter("@Source",DbType.String) {Value=logClass.Source },
                                    new SqliteParameter("@Type",DbType.String) {Value=logClass.Type },
                                    new SqliteParameter("@Message",DbType.String) {Value=logClass.Message },
                                    new SqliteParameter("@Time",DbType.DateTime) {Value=logClass.Time },
                                    new SqliteParameter("@RunningTime",DbType.Int64) {Value=logClass.RunningTime },
                                };
                                //将变量参数加到cmd
                                cmd.Parameters.AddRange(pms);
                                //执行
                                int i = cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                if (i > 0)
                                {
                                    ResetDbWriteErrorFlag();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogDbWriteError("单条写入", e);
                    return false;
                }
            }
        }


    }
}
