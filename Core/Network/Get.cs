using AngleSharp.Dom;
using Core.LogModule;
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
    public class Get
    {
        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="IsCookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string GetBody(string url, bool IsCookie = false, string referer = "", WebHeaderCollection specialheaders = null, string ContentType = "application/x-www-form-urlencoded", int maxAttempts = 3)
        {
//#if DEBUG
//            Log.Debug(nameof(GetBody), $"发起Get请求，目标:{url}");
//#endif
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
                req.UserAgent = Config.Core_RunConfig._HTTP_UA;
                req.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
                req.Timeout=8000;
                if (!string.IsNullOrEmpty(ContentType)) req.ContentType = ContentType;
                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                if (specialheaders != null) req.Headers = specialheaders;
                if (IsCookie && RuntimeObject.Account.AccountInformation!=null && RuntimeObject.Account.AccountInformation.State) req.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    try
                    {
                        rep = (HttpWebResponse)req.GetResponse();
                        using (StreamReader reader = new StreamReader(rep.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }
                        if (!string.IsNullOrEmpty(result))
                        {
                            break; // 如果成功获取响应，就跳出循环
                        }
                    }
                    catch (WebException ex)
                    {
                        if (IsCookie)
                            Log.Warn(nameof(GetBody), $"{ex.Status.ToString()}:{url}", null, false);
                        if (attempt == maxAttempts - 1) // 如果已经达到最大尝试次数，就将结果设为空字符串
                        {
                            if (IsCookie)
                                Log.Warn(nameof(GetBody), $"重试{maxAttempts}次均失败:{url}");
                            result = string.Empty;
                        }
                        else
                        {
                            Thread.Sleep(300);
                            continue; // 如果没有达到最大尝试次数，就继续尝试
                        }
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(300);
                        if (IsCookie)
                            Log.Error(nameof(GetBody), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
                        continue; // 如果没有达到最大尝试次数，就继续尝试
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
    }
}
