using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    /// <summary>
    /// DDTV本地缓存系统
    /// </summary>
    public class DataCache
    {
        public static int CacheCount = 0;
        private static Dictionary<string, string> 缓存队列 = new Dictionary<string, string>();
        private static Dictionary<string, DateTime> 缓存创建时间 = new Dictionary<string, DateTime>();
        private static bool 缓存锁 = false;
        /// <summary>
        /// 读缓存
        /// </summary>
        /// <param name="key">查询的键名</param>
        /// <param name="ExTime">超时时间,0为永远有效</param>
        /// <param name="data">查询到的结果</param>
        /// <returns>该键名是否有对应的数据</returns>
        public static bool 读缓存(string key, double ExTime, out string data)
        {
            data = null;
            try
            {             
                if (DataCache.缓存创建时间.TryGetValue(key, out DateTime Cache))
                {
                    TimeSpan TS = DateTime.Now - Cache;
                    if ((TS.TotalSeconds < ExTime || ExTime == 0) && DataCache.缓存队列.TryGetValue(key, out string CacheData))
                    {
                        data = CacheData;
                        InfoLog.InfoPrintf("缓存命中,读取数据:" + key + "|" + CacheData, InfoLog.InfoClass.Debug);
                        return true;
                    }
                    else
                    {
                        InfoLog.InfoPrintf("命中缓存，但数据已过期，返回false:" + key, InfoLog.InfoClass.Debug);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                InfoLog.InfoPrintf($"读缓存异常:{ex}", InfoLog.InfoClass.Debug);
                return false;
            }
        }
        /// <summary>
        /// 写缓存
        /// </summary>
        /// <param name="key">写入的键名</param>
        /// <param name="value">写入的值</param>
        /// <param name="TS"></param>
        public static void 写缓存(string key,string value="")
        {
            try
            {
                if (缓存队列.ContainsKey(key))
                    缓存队列[key] = value;
                else
                    缓存队列.Add(key, value);
                if (缓存创建时间.ContainsKey(key))
                    缓存创建时间[key]= DateTime.Now;
                else
                    缓存创建时间.Add(key, DateTime.Now);     
                //InfoLog.InfoPrintf("缓存未命中,缓存数据:" + key + "|" + value, InfoLog.InfoClass.Debug);
                DataCache.CacheCount++;
            }
            catch (Exception ex)
            {
                InfoLog.InfoPrintf($"写缓存异常:{ex.ToString()}", InfoLog.InfoClass.Debug);
            }
        }  
    }
}