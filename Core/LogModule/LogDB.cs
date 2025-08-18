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
                    lock (SQLiteConn)
                    {
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
                    Log.Error(nameof(LogDB), "日志数据库初始化失败");
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
                    lock (SQLiteConn)
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
                    Log.Error(nameof(LogDB), $"日志数据库写入出现未知错误:{e.ToString()}");
                    return false;
                }
            }
        }


    }
}
