using AngleSharp.Io;
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
    internal class Post
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
        internal static string PostBody(string url, Dictionary<string, string> dic, bool IsCookie = false, string jsondate = "", string contenttype = "application/x-www-form-urlencoded;charset=utf-8", string referer = "", WebHeaderCollection specialheaders = null)
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                req.ServicePoint.Expect100Continue = false;
                req.Method = "POST";
                if (string.IsNullOrEmpty(contenttype)) req.ContentType = contenttype;
                req.UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.69";
                if (specialheaders != null) req.Headers = specialheaders;

                if (IsCookie) req.Headers.Add("Cookie", AccountUser.AccountInformation.strCookies);

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
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                    }
                }
                #endregion

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
