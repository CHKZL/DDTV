using AngleSharp.Io;
using Core.LogModule;
using Core.RuntimeObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Desktop.NetWork
{
    public class Post
    {
        static int PostErrorCount = 0;
        static bool First = true;
        /// <summary>
        /// 同步POST方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="_dic">POST要发送的键值对</param>
        /// <param name="ExpandList">需要额外携带的LongList</param>
        /// <returns>请求返回体</returns>
        public static async Task<T> PostBody<T>(string url, Dictionary<string, string> _dic = null)
        {
            if (!string.IsNullOrEmpty(url) && url.Length > 5 && url.Substring(0, 4) != "http")
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
                        dic.Add(item.Key, item.Value.ToString());
                    }
                }
                string AuthenticationOriginalStr = string.Join(";", dic.Where(p => p.Key.ToLower() != "sig").OrderBy(p => p.Key).Select(p => $"{p.Key.ToLower()}={p.Value}"));
                string sig = Core.Tools.Encryption.SHA1_Encrypt(AuthenticationOriginalStr);
                dic.Add("sig", sig);
                dic.Remove("access_key_secret");
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 8);
                    var content = new FormUrlEncodedContent(dic);
                    var response = await client.PostAsync(url, content);
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    OperationQueue.pack<T> A = JsonConvert.DeserializeObject<OperationQueue.pack<T>>(responseString);
                    return A.data;
                }
            }
            catch (Exception ex)
            {
                PostErrorCount++;
                if (PostErrorCount > 30)
                {
                    Log.Warn(nameof(PostBody), $"触发DesktopTips={PostErrorCount}");
                    PostErrorCount = 0;
                }
                if (First)
                {
                    First = false;
                    Log.Warn(nameof(PostBody), $"发起Post请求出错,URL:[{url}]，错误堆栈：\r\n{ex.ToString()}", ex);
                }
                else
                {
                    Log.Warn(nameof(PostBody), $"发起Post请求出错,URL:[{url}]，错误堆栈：\r\n{ex.ToString()}", ex, false);
                }
                return default;
            }
        }
    }
}
