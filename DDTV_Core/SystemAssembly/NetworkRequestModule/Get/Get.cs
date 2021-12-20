using DDTV_Core.SystemAssembly.ConfigModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.NetworkRequestModule.Get
{
    internal class Get
    {
        /// <summary>
        /// 通过get请求获取返回信息
        /// </summary>
        /// <param name="url">目标网页地址</param>
        /// <returns></returns>
        internal static string GetRequest(string url, bool IsCookie = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Error, $"GetRequest请求错误！\nURL请求为空！\nGetRequest请求错误！");
            }
            NetClass.API_Count(url);
            string result = "";
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.ServicePoint.Expect100Continue = false;
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            req.UserAgent = NetClass.UA();
            req.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
           
            if (IsCookie)
            {
                if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                {
                    req.Headers.Add("Cookie", BilibiliUserConfig.account.cookie);
                    //req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie);
                }
            }
            req.Timeout = 3000;
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (Stream stream = resp.GetResponseStream())
                    {
                        //获取响应内容
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
                Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Trace_Web, $"发起GetRequest请求完成");
            }
            catch (Exception)
            {
                Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug, $"GetRequest请求超时或错误");
                return null;
            }
            return result;
            //NetClass.API_Count(url);
            //try
            //{
            //    using (WebClient wc = new WebClient())
            //    {
            //        wc.Headers.Add("Accept: */*");
            //        wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            //        wc.Headers.Add("User-Agent: " + NetClass.UA());
            //        if (IsCookie)
            //        {
            //            if (url.Contains("bili"))
            //            {
            //                if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
            //                {
            //                    wc.Headers.Add("Cookie", BilibiliUserConfig.account.cookie);
            //                }
            //            }
            //        }
            //        byte[] roomHtml = wc.DownloadData(url);
            //        if (wc != null)
            //            wc.Dispose();
            //        return Encoding.UTF8.GetString(roomHtml);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Error, $"GetRequest发生未知错误", true, e);
            //    return null;
            //}

        }
    }
}
