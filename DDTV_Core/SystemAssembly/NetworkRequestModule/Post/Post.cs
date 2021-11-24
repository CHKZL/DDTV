using DDTV_Core.SystemAssembly.ConfigModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.NetworkRequestModule.Post
{
    internal class Post
    {
        /// <summary>
        /// 通过Post请求发送网络请求获取Json类返回信息(该方法应该是为批量获取房间信息特殊准备的)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonParam"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string SendRequest_GetWebInfo_JsonClass(string url, string jsonParam, string encode)
        {
            string strURL = url;
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=" + encode.ToUpper();
            string paraUrlCoded = jsonParam;
            byte[] payload;
            payload = Encoding.GetEncoding(encode.ToUpper()).GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.GetEncoding(encode.ToUpper()));
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Trace_Web, $"发起POST请求:SendRequest_GetWebInfo_JsonClass完成");
            return strValue;
        }
        /// <summary>
        /// 以Post方式发送带参数和CookieContainer对象的http请求(该方法应该是为发送弹幕而特殊准备的)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dic"></param>
        /// <param name="cook"></param>
        /// <returns></returns>
        public static string SendRequest_SendDanmu(string url, Dictionary<string, string> dic, CookieContainer cook)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = NetClass.UA();
            if (url.Contains("bilibili"))
            {
                if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                {
                    req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliUserConfig.account.cookie);
                }
            }
            #region 添加Post 参数  
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
            req.CookieContainer = cook;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Trace_Web, $"发起POST请求:SendRequest_SendDanmu完成");
            return result;
        }
    }
}
