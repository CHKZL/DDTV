using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.HTTP
{
    public class Get
    {
        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string GetBody(string url, CookieCollection cookie = null,
            string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null)
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);

                if (specialheaders != null) req.Headers = specialheaders;

                if (cookie != null)
                {
                    req.CookieContainer = new CookieContainer(cookie.Count)
                    {
                        PerDomainCapacity = cookie.Count
                    };
                    req.CookieContainer.Add(cookie);
                }

                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;

                rep = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(rep.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
            
        }
    }
}
