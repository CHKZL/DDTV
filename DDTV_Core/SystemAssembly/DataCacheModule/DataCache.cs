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
        public static bool SetCache(DataCacheClass.CacheType CacheType, string Key, string Value, long ExTime)
        {
            if (DataCacheClass.Caches.TryGetValue(CacheType, out var Cache))
            {
                if (Cache.ContainsKey(Key))
                {
                    Cache[Key].Value = Value;
                    Cache[Key].ExTime = Tool.TimeModule.Time.Operate.GetRunMilliseconds()+ExTime;
                    //Log.Log.AddLog(nameof(DataCache), Log.LogClass.LogType.Trace, $"更新缓存:Type为[{CacheType}]的数据键[{Key}设置数据[{Value}]缓存成功，该缓存有效期至UTC零点时间+[{Cache[Key].ExTime}]毫秒");
                }
                else
                {
                    Cache.Add(Key, new DataCacheClass.Data()
                    {
                        Value= Value,
                        ExTime= Tool.TimeModule.Time.Operate.GetRunMilliseconds()+ExTime
                    });
                    //Log.Log.AddLog(nameof(DataCache), Log.LogClass.LogType.Trace, $"增加缓存:Type为[{CacheType}]的数据键[{Key}设置数据[{Value}]缓存成功，该缓存有效期至UTC零点时间+[{Cache[Key].ExTime}]毫秒");
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
            if (DataCacheClass.Caches.TryGetValue(CacheType, out var Cache))
            {
                if (Cache.ContainsKey(Key))
                {
                    if (Cache[Key].ExTime> Tool.TimeModule.Time.Operate.GetRunMilliseconds())
                    {
                        Value = Cache[Key].Value;
                        //Log.Log.AddLog(nameof(DataCache), Log.LogClass.LogType.Trace, $"命中缓存:Type为[{CacheType}]的数据键[{Key}数据[{Value}]缓存读取成功，该缓存有效期至UTC零点时间+[{Cache[Key].ExTime}]毫秒");
                        return true;
                    }
                    else
                    {
                       // Log.Log.AddLog(nameof(DataCache), Log.LogClass.LogType.Trace, $"缓存未命中:Type为[{CacheType}]的数据键[{Key}数据[{Value}]缓存命中失败，加入数据池等待更新数据");
                        return false;
                    }
                }
            }
            //Log.Log.AddLog(nameof(DataCache), Log.LogClass.LogType.Trace, $"缓存未命中:Type为[{CacheType}]的数据键[{Key}数据[{Value}]缓存命中失败，加入数据池等待更新数据");
            return false;
        }
    }
}
