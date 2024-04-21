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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Desktop.NetWork
{
    public class Post
    {
        /// <summary>
        /// 异步POST方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="dic">POST要发送的键值对</param>
        /// <returns>请求返回体</returns>
        public static async Task<T> AsyncPostBody<T>(string url, Dictionary<string, string> _dic =null)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "access_key_id", NetWork.Basics.access_key_id },
                { "access_key_secret", NetWork.Basics.access_key_secret },
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
            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(dic);
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            OperationQueue.pack<T> A =JsonConvert.DeserializeObject<OperationQueue.pack<T>>(responseString);
            return A.data;
        }
        /// <summary>
        /// 同步POST方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="dic">POST要发送的键值对</param>
        /// <returns>请求返回体</returns>
        public static T PostBody<T>(string url, Dictionary<string, string> _dic =null)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "access_key_id", NetWork.Basics.access_key_id },
                { "access_key_secret", NetWork.Basics.access_key_secret },
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
            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(dic);
            var response = client.PostAsync(url, content).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            OperationQueue.pack<T> A =JsonConvert.DeserializeObject<OperationQueue.pack<T>>(responseString);
            return A.data;
        }
    }
}
