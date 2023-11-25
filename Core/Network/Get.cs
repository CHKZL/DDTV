using Core.RuntimeObject;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Network
{
    internal class Get
    {
        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="IscCookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        internal static string GetBody(string url, bool IscCookie = false, string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null, string ContentType = "application/x-www-form-urlencoded")
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                req.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                req.ServicePoint.Expect100Continue = false;
                req.Method = "GET";
                req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
                req.UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.69";
                req.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
                if (!string.IsNullOrEmpty(ContentType)) req.ContentType = ContentType;
                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;
                if (specialheaders != null) req.Headers = specialheaders;
                if (IscCookie) req.Headers.Add("Cookie", AccountUser.AccountInformation.strCookies);         
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
