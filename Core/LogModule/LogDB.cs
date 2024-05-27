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
                string date = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss-fffff");
                ErrorFilePath = $"{Core.Config.Core_RunConfig._LogFileDirectory}/DDTVCoreErrorLog_{date}.txt";
                streamWriter = new StreamWriter(ErrorFilePath, true, Encoding.UTF8);
                return $"{Core.Config.Core_RunConfig._LogFileDirectory}/DDTVCoreLog_{date}.sqlite";
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
                    if (SQLiteConn==null)
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
                    Log.Error(nameof(LogDB), "日志数据库写入出现未知错误");
                    return false;
                }
            }
        }


    }
}
