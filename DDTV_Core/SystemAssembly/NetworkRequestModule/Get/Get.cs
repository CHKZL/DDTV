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
        internal static string GetRequest(string url)
        {
            string result = "";
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            req.UserAgent = NetClass.UA();
            if (url.Contains("bilibili"))
            {
                if (!string.IsNullOrEmpty(BilibiliModule.User.BilibiliUser.account.cookie))
                {
                    req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliModule.User.BilibiliUser.account.cookie);
                }
            }
             req.Timeout = 10000;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Trace, $"发起GetRequest请求完成");
            return result;
        }
    }
}
