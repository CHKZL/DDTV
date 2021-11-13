using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.DataCacheModule
{
    public class DataCache
    {
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="CacheType">缓存类型</param>
        /// <param name="Key">键名</param>
        /// <param name="Value">值</param>
        /// <param name="ExTime">有效期(毫秒)</param>
        /// <returns>是否设置成功</returns>
        public static bool SetCache(DataCacheClass.CacheType CacheType, string Key, string Value, int ExTime)
        {
            if (DataCacheClass.Caches.TryGetValue((int)CacheType, out var Cache))
            {
                if (Cache.ContainsKey(Key))
                {
                    Cache[Key].Value = Value;
                    Cache[Key].ExTime = TimeModule.Time.Operate.GetRunMilliseconds()+ExTime;
                }
                else
                {
                    Cache.Add(Key, new DataCacheClass.Data()
                    {
                        Value= Value,
                        ExTime=TimeModule.Time.Operate.GetRunMilliseconds()+ExTime
                    });
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="CacheType">缓存类型</param>
        /// <param name="Key">键名</param>
        /// <param name="Value">获取到的值</param>
        /// <returns>该键值对是否有效</returns>
        public static bool GetCache(DataCacheClass.CacheType CacheType, string Key, out string Value)
        {
            Value = string.Empty;
            if (DataCacheClass.Caches.TryGetValue((int)CacheType, out var Cache))
            {
                if (Cache.ContainsKey(Key))
                {
                    if (Cache[Key].ExTime<TimeModule.Time.Operate.GetRunMilliseconds())
                    {
                        Value = Cache[Key].Value;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
