using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BiliAccount
{
    /// <summary>
    /// HTTP封装类
    /// </summary>
    internal class Http
    {
        #region Public Methods

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

        /// <summary>
        /// 会返回cookies的Get方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="out_cookies">返回的cookies集合实例</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string GetBodyOutCookies(string url, out CookieCollection out_cookies, CookieCollection cookie = null,
            string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null)
        {
            string result = "";
            out_cookies = new CookieCollection();
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

                foreach (string i in rep.Headers.GetValues("Set-Cookie"))
                {
                    string[] tmp = i.Split(';');
                    string[] tmp2 = tmp[0].Split('=');

                    out_cookies.Add(new Cookie(tmp2[0], tmp2[1]) { Expires = DateTime.Parse(tmp[2].Split('=')[1]) });
                }
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
        }

        /// <summary>
        /// 会返回cookies的Get方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="out_cookies">返回的cookies集合实例</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static Bitmap GetPicOutCookies(string url, out CookieCollection out_cookies, CookieCollection cookie = null,
            string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null)
        {
            Bitmap result = null;
            out_cookies = new CookieCollection();
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

                result = new Bitmap(rep.GetResponseStream());

                if (rep.Headers.GetValues("Set-Cookie") != null && rep.Headers.GetValues("Set-Cookie").Length > 0)
                {
                    foreach (string i in rep.Headers.GetValues("Set-Cookie"))
                    {
                        out_cookies.Add(new Cookie(new Regex("^(?<=).*?(?==)").Match(i).Value, new Regex("(?<==).*?(?=; )").Match(i).Value) { Expires = DateTime.Parse(new Regex("(?<=Expires=).*?(?=;)").Match(i).Value), Domain = new Regex("(?<=Domain=).*?(?=;)").Match(i).Value, Path = new Regex("(?<=Path=).*?(?=;)").Match(i).Value });
                    }
                }
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
        }

        /// <summary>
        /// POST方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="data">要POST发送的文本</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="contenttype">数据类型</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string PostBody(string url, string data = "", CookieCollection cookie = null,
            string contenttype = "application/x-www-form-urlencoded;charset=utf-8", string referer = "", string user_agent = "",
            WebHeaderCollection specialheaders = null)
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);

                if (specialheaders != null) req.Headers = specialheaders;

                req.Method = "POST";

                if (cookie != null)
                {
                    req.CookieContainer = new CookieContainer(cookie.Count)
                    {
                        PerDomainCapacity = cookie.Count
                    };
                    req.CookieContainer.Add(cookie);
                }

                req.ContentType = contenttype;

                byte[] bdata = Encoding.UTF8.GetBytes(data);
                Stream sdata = req.GetRequestStream();
                sdata.Write(bdata, 0, bdata.Length);
                sdata.Close();

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

        /// <summary>
        /// 返回cookies的POST方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="out_cookies">返回的cookies集合实例</param>
        /// <param name="data">要POST发送的文本</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="contenttype">数据类型</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string PostBodyOutCookies(string url, out CookieCollection out_cookies, string data = "", CookieCollection cookie = null,
            string contenttype = "application/x-www-form-urlencoded;charset=utf-8", string referer = "", string user_agent = "",
            WebHeaderCollection specialheaders = null)
        {
            string result = "";
            out_cookies = new CookieCollection();
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);

                if (specialheaders != null) req.Headers = specialheaders;

                req.Method = "POST";

                if (cookie != null)
                {
                    req.CookieContainer = new CookieContainer(cookie.Count)
                    {
                        PerDomainCapacity = cookie.Count
                    };
                    req.CookieContainer.Add(cookie);
                }

                req.ContentType = contenttype;

                byte[] bdata = Encoding.UTF8.GetBytes(data);
                Stream sdata = req.GetRequestStream();
                sdata.Write(bdata, 0, bdata.Length);
                sdata.Close();

                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;

                rep = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(rep.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }

                if (rep.Headers.GetValues("Set-Cookie") != null && rep.Headers.GetValues("Set-Cookie").Length > 0)
                {
                    foreach (string i in rep.Headers.GetValues("Set-Cookie"))
                    {
                        out_cookies.Add(new Cookie(new Regex("^(?<=).*?(?==)").Match(i).Value, new Regex("(?<==).*?(?=; )").Match(i).Value) { Expires = DateTime.Parse(new Regex("(?<=Expires=).*?(?=;)").Match(i).Value), Domain = new Regex("(?<=Domain=).*?(?=;)").Match(i).Value, Path = new Regex("(?<=Path=).*?(?=;)").Match(i).Value });
                    }
                }
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
        }

        #endregion Public Methods
    }
}