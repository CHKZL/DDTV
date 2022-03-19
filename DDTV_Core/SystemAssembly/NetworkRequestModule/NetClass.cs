using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.NetworkRequestModule
{
    public class NetClass
    {
        public static Dictionary<DataCacheModule.DataCacheClass.CacheType, int> SelectAPI_Count = new();
        private static List<DataCacheModule.DataCacheClass.CacheType> cacheTypes = new();

        public static Dictionary<string, int> API_Usage_Count = new();
        private static List<string> API_Usage_cacheTypes = new();

        public static void SelectAPICountAdd(DataCacheModule.DataCacheClass.CacheType cacheType)
        {
            cacheTypes.Add(cacheType);
        }
        public static void SAPIEVT()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
            Task.Run(() => {
                while (true)
                {
                    if(cacheTypes.Count>0)
                    {
                        if (SelectAPI_Count.ContainsKey(cacheTypes[0]))
                        {
                            SelectAPI_Count[cacheTypes[0]] = SelectAPI_Count[cacheTypes[0]] + 1;
                        }
                        else
                        {
                            SelectAPI_Count.Add(cacheTypes[0], 1);
                        }
                        cacheTypes.RemoveAt(0);
                    }
                    Thread.Sleep(30);
                }
            });
            Task.Run(() => {
                while (true)
                {
                    if (API_Usage_cacheTypes.Count > 0)
                    {
                        if (API_Usage_Count.ContainsKey(API_Usage_cacheTypes[0]))
                        {
                            API_Usage_Count[API_Usage_cacheTypes[0]] = API_Usage_Count[API_Usage_cacheTypes[0]] + 1;
                        }
                        else
                        {
                            API_Usage_Count.Add(API_Usage_cacheTypes[0], 1);
                        }
                        API_Usage_cacheTypes.RemoveAt(0);
                    }
                    Thread.Sleep(30);
                }
            });
        }
        public static void API_Count(string URL)
        {
            API_Usage_cacheTypes.Add(URL.Split('?')[0]);
        }
        public static string UA()
        {
            return $"Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36 DDTV/{InitDDTV_Core.Ver}";
        }
        /// <summary>
        /// 将cookie字符串转换为CookieContainer对象
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static CookieContainer CookieContainerTransformation(string cookie)
        {
            CookieContainer CK = new CookieContainer { MaxCookieSize = 4096, PerDomainCapacity = 50 };

            string[] cook = cookie.Replace(" ", "").Split(';');
            for (int i = 0 ; i < cook.Length ; i++)
            {
                try
                {
                    if(!string.IsNullOrEmpty(cook[i]))
                    {
                        CK.Add(new Cookie(cook[i].Split('=')[0], cook[i].Split('=')[1].Replace(",", "%2C")) { Domain = "live.bilibili.com" });
                    }          
                }
                catch (Exception e)
                {
                    Log.Log.AddLog(nameof(NetClass), Log.LogClass.LogType.Error, "将cookie字符串转换为CookieContainer对象的过程中出现了未知错误！", true, e);
                }
            }
            return CK;
        }
        /// <summary>
        /// 文件大小单位转换
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string ConversionSize(double size)
        {
            if (size < 0)
            {
                return "未知";
            }
            if (size <= 1024)
            {
                return size.ToString("F2") + "B";
            }
            if (size <= 1048576)
            {
                return (size / 1024.0).ToString("F2") + "KB";
            }
            if (size <= 1073741824)
            {
                return (size / 1048576.0).ToString("F2") + "MB";
            }
            if (size <= 1099511627776)
            {
                return (size / 1073741824.0).ToString("F2") + "GB";
            }
            return (size / 1099511627776.0).ToString("F2") + "TB";
        }
    }
}
