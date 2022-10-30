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
    public class Post
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
            NetClass.API_Count(url);
            string strURL = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

            if (!CoreConfig.IsBypass_SSL)
            {
                request.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }         
            if (!CoreConfig.WhetherToEnableProxy)
            {
                request.Proxy = null;
            }
            if (!CoreConfig.MandatoryUseIPv4)
            {
                try
                {
                    request.ServicePoint.BindIPEndPointDelegate = (servicePoint, remoteEndPoint, retryCount) =>
                    {
                        if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return new IPEndPoint(IPAddress.Any, 0);
                        }
                        else if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            return new IPEndPoint(IPAddress.IPv6Any, 0);
                        }
                        else
                        {
                            return null;
                        }
                    };
                }
                catch (Exception) { }
            }
            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";
            request.ContentType = "application/json;charset=" + encode.ToUpper();
            string paraUrlCoded = jsonParam;
            byte[] payload;
            payload = Encoding.GetEncoding(encode.ToUpper()).GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(payload, 0, payload.Length);
                writer.Close();
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                string StrDate = "";
                string strValue = "";
                using (Stream s = response.GetResponseStream())
                {
                    using (StreamReader Reader = new StreamReader(s, Encoding.GetEncoding(encode.ToUpper())))
                    {
                        while ((StrDate = Reader.ReadLine()) != null)
                        {
                            strValue += StrDate + "\r\n";
                        }
                    }
                }
                if(response!=null)
                {
                    response.Dispose();
                }
                Log.Log.AddLog(nameof(Post), Log.LogClass.LogType.Trace_Web, $"发起POST请求:SendRequest_GetWebInfo_JsonClass完成");
                Log.Log.AddLog(nameof(Post), Log.LogClass.LogType.Trace_Web, $"发起POST请求完成：{url}",false,null,false);
                try
                {
                    if (request != null) request.Abort();
                }
                catch (Exception){}
                return strValue;
            }
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
            NetClass.API_Count(url);
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            if (!CoreConfig.IsBypass_SSL)
            {
                req.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }
            if (!CoreConfig.WhetherToEnableProxy)
            {
                req.Proxy = null;
            }
           
            if (!CoreConfig.MandatoryUseIPv4)
            {
                try
                {
                    req.ServicePoint.BindIPEndPointDelegate = (servicePoint, remoteEndPoint, retryCount) =>
                    {
                        if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return new IPEndPoint(IPAddress.Any, 0);
                        }
                        else if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            Log.Log.AddLog(nameof(Post), Log.LogClass.LogType.Info_IP_Ver, $"使用IPv6发起请求");
                            return new IPEndPoint(IPAddress.IPv6Any, 0);
                        }
                        else
                        {
                            Log.Log.AddLog(nameof(Post), Log.LogClass.LogType.Info_IP_Ver, $"没有IPv4也没有IPv6！");
                            return null;
                        }
                    };
                }
                catch (Exception) { }
            }
            req.ServicePoint.Expect100Continue = false;
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
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream stream = resp.GetResponseStream())
                {
                    //获取响应内容  
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                    Log.Log.AddLog(nameof(Post), Log.LogClass.LogType.Trace_Web, $"发起POST请求:SendRequest_SendDanmu完成");
                    Log.Log.AddLog(nameof(Post), Log.LogClass.LogType.Trace_Web, $"发起POST请求完成：{url}", false, null, false);
                    if (resp != null)
                    {
                        resp.Dispose();
                    }
                    try
                    {
                        if (req != null) req.Abort();
                    }
                    catch (Exception) { }
                    return result;
                }
            }
        }
        public static string HttpPost(string url, Dictionary<string, string> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            if (!CoreConfig.IsBypass_SSL)
            {
                req.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }
            if (!CoreConfig.WhetherToEnableProxy)
            {
                req.Proxy = null;
            }
            if (!CoreConfig.MandatoryUseIPv4)
            {
                try
                {
                    req.ServicePoint.BindIPEndPointDelegate = (servicePoint, remoteEndPoint, retryCount) =>
                    {
                        if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return new IPEndPoint(IPAddress.Any, 0);
                        }
                        else if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            return new IPEndPoint(IPAddress.IPv6Any, 0);
                        }
                        else
                        {
                            return null;
                        }
                    };
                }
                catch (Exception) { }
            }

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = $"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
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
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion
            Log.Log.AddLog(nameof(Post), Log.LogClass.LogType.Trace_Web, $"发起POST请求完成：{url}", false, null, false);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            try
            {
                if (req != null) req.Abort();
            }
            catch (Exception) { }
            return result;
        }
    }
}
