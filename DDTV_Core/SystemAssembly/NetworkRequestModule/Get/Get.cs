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
    public class Get
    {

        /// <summary>
        /// 通过get请求获取返回信息
        /// </summary>
        /// <param name="url">请求目标网页地址</param>
        /// <param name="IsCookie">是否需要携带B站cookie</param>
        /// <param name="Referer">使用的referer（默认为空，如有必要请使用https://www.bilibili.com/）</param>
        /// <param name="ContentType">类型（默认使用application/x-www-form-urlencoded）</param>
        /// <param name="IsMandatoryIPv4">是否强制使用IPv4</param>
        /// <returns></returns>
        public static string GetRequest(string url, bool IsCookie = true, string Referer = "", string ContentType = "application/x-www-form-urlencoded", bool IsMandatoryIPv4 = false)
        {

            //Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug, $"发出网络请求:{url.Split('?')[0]}", false, null, false);
            if (string.IsNullOrEmpty(url))
            {
                Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Error, $"GetRequest请求错误！\nURL请求为空！\nGetRequest请求错误！");
            }
            NetClass.API_Count(url);
            string result = "";
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            if (!CoreConfig.WhetherToEnableProxy)
            {
                req.Proxy = null;
            }
            if (CoreConfig.MandatoryUseIPv4)
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
            req.ServicePoint.Expect100Continue = false;
            req.Method = "GET";
            req.ContentType = ContentType;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            req.UserAgent = NetClass.UA();
            req.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            if (!string.IsNullOrEmpty(Referer))
            {
                req.Referer = Referer;
            }
            if (IsCookie && !string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
            {
                req.Headers.Add("Cookie", BilibiliUserConfig.account.cookie);

            }
            req.Timeout = 8000;

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
                //Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug_Request, $"发起GetRequest请求完成:{url}", false, null, false);
            }
            catch (WebException e)
            {
                //Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug, $"GetRequest请求发生网络层错误:{e.Status},{IsCookie},{Referer},{IsMandatoryIPv4},{url}");
                 if(DDTV_Core.InitDDTV_Core.IsDevDebug)
                {
                    Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug, $"GetRequest请求发生网络层错误:{e.Status}({(int)e.Status})[{url}]", true, e, false);
                }
                else
                {
                    Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug_Request_Error, $"GetRequest请求发生网络层错误:{e.Status}", true, e, false);
                }
                
                return null;
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug_Request_Error, $"GetRequest请求超时或错误,{e.ToString()}");
                return null;
            }

            return result;
        }
        public static void GetFile_For_Update(string URL, string File)
        {
            var wc = new WebClient();
            wc.Headers.Add("Referer: Update.ddtv.pro");
            string P = Path.GetDirectoryName(File);
            if (!string.IsNullOrEmpty(P))
            {
                Directory.CreateDirectory(P);
            }
            wc.DownloadFile(URL, File);

        }
        public static FileInfo GetFile(string URL, string File)
        {
            var wc = new WebClient();
            wc.Headers.Add("Referer: https://www.bilibili.com/");
            wc.Headers.Add("ContentType: application/x-www-form-urlencoded");
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("UserAgent: " + NetClass.UA());
            string P = Path.GetDirectoryName(File);
            if (!string.IsNullOrEmpty(P))
            {
                Directory.CreateDirectory(P);
            }
            wc.DownloadFile(URL, File);
            return new FileInfo(File);
        }
        public static byte[] GetFile_Bytes(string URL, long Uid = 0)
        {
            try
            {
                var wc = new WebClient();
                wc.Headers.Add("Referer: https://www.bilibili.com/");
                wc.Headers.Add("ContentType: application/x-www-form-urlencoded");
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("UserAgent: " + NetClass.UA());
                return wc.DownloadData(URL);
            }
            catch (WebException e)
            {
                switch (e.Status)
                {
                    case WebExceptionStatus.Timeout:
                        return null;
                    case WebExceptionStatus.ProtocolError:
                        return null;
                    default:
                        Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug, (Uid != 0 ? $"请求UID为[{Uid}]的房间发生" : "") + $"GetFile_Bytes请求错误:{e.Status}({(int)e.Status})");
                        return null;
                }
            }
            catch (Exception e)
            {
                Log.Log.AddLog(nameof(Get), Log.LogClass.LogType.Debug, $"GetFile_Bytes请求发生意外错误，详情已写入txt", true, e, true);
                return null;
            }
        }
    }
}
