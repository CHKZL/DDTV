using AngleSharp.Io;
using Core.LogModule;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Network
{
    public class Post
    {
        /// <summary>
        /// POST方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="dic">POST要发送的键值对</param>
        /// <param name="IsCookie">是否使用cookis</param>
        /// <param name="jsondate">POST要发送的文本信息</param>
        /// <param name="contenttype">数据类型</param>
        /// <param name="referer">Referer</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string PostBody(string url, Dictionary<string, string> dic, bool IsCookie = false, string jsondate = "", string contenttype = "application/x-www-form-urlencoded;charset=utf-8", string referer = "", WebHeaderCollection specialheaders = null, int maxAttempts = 3,CookieContainer cookieContainer =null)
        {
//# if DEBUG
//            Log.Debug(nameof(PostBody), $"发起Post请求，目标:{url}");
//# endif
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                req.ServicePoint.Expect100Continue = false;
                req.Method = "POST";
                if (string.IsNullOrEmpty(contenttype)) req.ContentType = contenttype;
                req.UserAgent = Config.Core_RunConfig._HTTP_UA;
                if (specialheaders != null) req.Headers = specialheaders;
                req.Timeout=8000;
                if (IsCookie && RuntimeObject.Account.AccountInformation!=null && RuntimeObject.Account.AccountInformation.State) req.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);

                byte[] bdata = Encoding.UTF8.GetBytes(jsondate);
                Stream sdata = req.GetRequestStream();
                sdata.Write(bdata, 0, bdata.Length);
                sdata.Close();

                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;

                #region 添加Post 参数  
                if (dic != null)
                {
                    StringBuilder builder = new StringBuilder();
                    int i = 0;
                    foreach (var item in dic)
                    {
                        if (item.Key.Length > 20)
                        {
                            if (i > 0)
                                builder.Append("&");
                            builder.AppendFormat("{0}", item.Key);
                            i++;
                        }
                        else
                        {
                            if (i > 0)
                                builder.Append("&");
                            builder.AppendFormat("{0}={1}", item.Key, item.Value);
                            i++;
                        }
                    }
                    byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
                    req.ContentLength = data.Length;
                    if (cookieContainer != null)
                    {
                        req.CookieContainer = cookieContainer;
                    }

                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                    }
                }
                #endregion


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
                        Log.Warn(nameof(PostBody), $"{ex.Status.ToString()}:{url}", null, false);

                        if (attempt == maxAttempts - 1) // 如果已经达到最大尝试次数，就将结果设为空字符串
                        {
                            Log.Warn(nameof(PostBody), $"重试{maxAttempts}次均失败:{url}");
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
                        Log.Error(nameof(PostBody), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
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
