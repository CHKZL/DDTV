using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class DataCache
    {
        public static int BilibiliApiCount = 0;

        public static Dictionary<string, string> 缓存队列 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> 缓存创建时间 = new Dictionary<string, DateTime>();
        public static Dictionary<string, string> 通过UID获取房间号键值对 = new Dictionary<string, string>();
        public static Dictionary<string, string> 获取真实房间号键值对 = new Dictionary<string, string>();
        public static Dictionary<string, string> 获取标题键值对 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> 获取标题有效期 = new Dictionary<string, DateTime>();

        /// <summary>
        /// 读缓存
        /// </summary>
        /// <param name="key">查询的键名</param>
        /// <param name="ExTime">超时时间,0为永远超时</param>
        /// <param name="data">查询到的结果</param>
        /// <returns>该键名是否有对应的数据</returns>
        public static bool 读缓存(string key, double ExTime, out string data)
        {
            data = null;
            if (DataCache.缓存创建时间.TryGetValue(key, out DateTime Cache))
            {
                TimeSpan TS = DateTime.Now - Cache;
                if ((TS.TotalSeconds < ExTime || ExTime == 0 )&& DataCache.缓存队列.TryGetValue(key, out string CacheData))
                {
                    data = CacheData;
                    InfoLog.InfoPrintf("缓存命中,读取数据:" + key + "|" + CacheData, InfoLog.InfoClass.Debug);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 写缓存
        /// </summary>
        /// <param name="key">写入的键名</param>
        /// <param name="value">写入的值</param>
        /// <param name="TS"></param>
        public static void 写缓存(string key,string value)
        {
            缓存创建时间.Add(key,DateTime.Now);
            缓存队列.Add(key, value);
            InfoLog.InfoPrintf("缓存未命中,缓存数据:"+key+"|" + value, InfoLog.InfoClass.Debug);
            DataCache.BilibiliApiCount++;
        }
     
       
    }
}