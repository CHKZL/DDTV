using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
            Task.Run(() =>
            {
                while (true)
                {
                    if (cacheTypes.Count > 0)
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
            Task.Run(() =>
            {
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


        public static string Get_w_rid_string(long uid, long timestamp, string salt)
        {
            string w_rid = GetMd5Hash("mid=" + uid + "&platform=web&token=&web_location=1550101&wts=" + timestamp + salt);
            return $"mid={uid}&token=&platform=web&web_location=1550101&w_rid={w_rid}&wts={timestamp}";
        }
        private static string GetMd5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i=0;i<data.Length;i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public static string Get_salt(string imgKey, string subKey)
        {
            var n = imgKey + subKey;
            var array = n.ToCharArray();
            var order = new int[] { 46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49,
                33, 9, 42, 19, 29, 28, 14, 39, 12,
                38, 41, 13, 37, 48, 7, 16,
                24,55 ,40 ,61 ,26 ,17 ,0 ,1 ,60 ,51 ,30 ,4 ,22 ,25 ,54 ,21 ,56 ,59 ,6 ,63 ,57 ,62 ,
                11 ,36 ,20 ,34 ,44 ,52 };
            var salt = new string(order.Select(i => array[i]).ToArray()).Substring(0, 32); // 按照特定顺序混淆并取前32位
            return salt;
        }
        public static void API_Count(string URL)
        {
            API_Usage_cacheTypes.Add(URL.Split('?')[0]);
        }
        public static string UA()
        {
            return $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36";
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
            for (int i = 0; i < cook.Length; i++)
            {
                try
                {
                    if (!string.IsNullOrEmpty(cook[i]))
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
        public static string ConversionSize(double size, ConversionSizeType conversionSizeType = ConversionSizeType.String)
        {

            if (conversionSizeType == ConversionSizeType.BitRate)
            {
                size = size * 8.0;
            }
            if (size < 0)
            {
                return "未知";
            }
            if (size <= 1024)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return size.ToString("F2") + "B";
                    case ConversionSizeType.BitRate:
                        return size.ToString("F2") + "bps";
                    case ConversionSizeType.DownloadSpe:
                        return size.ToString("F2") + "B/s";
                    default:
                        return size.ToString("F2") + "B";
                }
            }
            if (size <= 1048576)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return (size / 1024.0).ToString("F2") + "KB";
                    case ConversionSizeType.BitRate:
                        return (size / 1024.0).ToString("F2") + "Kbps";
                    case ConversionSizeType.DownloadSpe:
                        return (size / 1024.0).ToString("F2") + "KB/s";
                    default:
                        return (size / 1024.0).ToString("F2") + "KB";
                }
            }
            if (size <= 1073741824)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return (size / 1048576.0).ToString("F2") + "MB";
                    case ConversionSizeType.BitRate:
                        return (size / 1048576.0).ToString("F2") + "Mbps";
                    case ConversionSizeType.DownloadSpe:
                        return (size / 1048576.0).ToString("F2") + "MB/s";
                    default:
                        return (size / 1048576.0).ToString("F2") + "MB";
                }
            }
            if (size <= 1099511627776)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return (size / 1073741824.0).ToString("F2") + "GB";
                    case ConversionSizeType.BitRate:
                        return (size / 1073741824.0).ToString("F2") + "Gbps";
                    case ConversionSizeType.DownloadSpe:
                        return (size / 1073741824.0).ToString("F2") + "GB/s";
                    default:
                        return (size / 1073741824.0).ToString("F2") + "GB";
                }
            }
            switch (conversionSizeType)
            {
                case ConversionSizeType.String:
                    return (size / 1099511627776.0).ToString("F2") + "TB";
                case ConversionSizeType.BitRate:
                    return (size / 1099511627776.0).ToString("F2") + "Tbps";
                case ConversionSizeType.DownloadSpe:
                    return (size / 1099511627776.0).ToString("F2") + "TB/s";
                default:
                    return (size / 1099511627776.0).ToString("F2") + "TB";
            }
        }
        public enum ConversionSizeType
        {
            String,
            BitRate,
            DownloadSpe,
        }
    }
}
