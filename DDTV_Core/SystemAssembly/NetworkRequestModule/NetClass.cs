using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.NetworkRequestModule
{
    internal class NetClass
    {
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
                    CK.Add(new Cookie(cook[i].Split('=')[0], cook[i].Split('=')[1].Replace(",", "%2C")) { Domain = "live.bilibili.com" });
                }
                catch (Exception)
                {

                }
            }
            return CK;
        }
    }
}
