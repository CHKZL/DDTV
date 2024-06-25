using AngleSharp.Dom;
using Core.LogModule;
using Core.RuntimeObject;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Desktop.NetWork
{
    public class Get
    {
        static int GetErrorCount = 0;
        static bool First = true;
        public static T GetBody<T>(string url, Dictionary<string, string> _dic = null)
        {
            if (!string.IsNullOrEmpty(url) && url.Substring(0, 4) != "http")
            {
                url = "http://" + url;
            }
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "access_key_id", Core.Config.Core_RunConfig._DesktopAccessKeyId },
                    { "access_key_secret", Core.Config.Core_RunConfig._DesktopAccessKeySecret },
                    { "time", DateTimeOffset.Now.ToUnixTimeSeconds().ToString()}
                };
                if (_dic != null)
                {
                    foreach (var item in _dic)
                    {
                        dic.Add(item.Key, item.Value);
                    }
                }
                string AuthenticationOriginalStr = string.Join(";", dic.Where(p => p.Key.ToLower() != "sig").OrderBy(p => p.Key).Select(p => $"{p.Key.ToLower()}={p.Value}"));
                string sig = Core.Tools.Encryption.SHA1_Encrypt(AuthenticationOriginalStr);
                dic.Add("sig", sig);
                dic.Remove("access_key_secret");
                string Parameter = string.Empty;
                foreach (var item in dic)
                {
                    Parameter += $"{item.Key}={item.Value}&";
                }

                using (HttpClient _httpClient = new HttpClient())
                {
                    _httpClient.Timeout = new TimeSpan(0,0,8);
                    HttpResponseMessage response = _httpClient.GetAsync($"{url}?{Parameter}").Result;
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    OperationQueue.pack<T> A = JsonConvert.DeserializeObject<OperationQueue.pack<T>>(responseBody);
                    return A.data;
                }
            }
            catch (Exception ex)
            {
                GetErrorCount++;
                if (GetErrorCount > 30)
                {     
                    Log.Warn(nameof(GetBody), $"触发DesktopTips={GetErrorCount}");
                    GetErrorCount = 0;
                }
                if (First)
                {
                    First = false;
                    Log.Warn(nameof(GetBody), $"发起Get请求出错,URL:[{url}]，错误堆栈：\r\n{ex.ToString()}", ex);
                }
                else
                {
                    Log.Warn(nameof(GetBody), $"发起Get请求出错,URL:[{url}]，错误堆栈：\r\n{ex.ToString()}", ex, false);
                }
                
                return default;
            }
        }
    }
}
