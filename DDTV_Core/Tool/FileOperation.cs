using DDTV_Core.SystemAssembly.ConfigModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    public class FileOperation
    {
        private static DelEvent delEvent = new DelEvent();

        /// <summary>
        /// 判断网络路径的文件是否存在
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool IsExistsNetFile(string Url)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(Url));
                httpWebRequest.Accept = "*/*";
                httpWebRequest.UserAgent = SystemAssembly.NetworkRequestModule.NetClass.UA();
                httpWebRequest.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
                if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                {
                    httpWebRequest.Headers.Add("Cookie", BilibiliUserConfig.account.cookie);
                }
                httpWebRequest.Timeout = 5000;
                //返回响应状态是否是成功比较的布尔值
                var B = httpWebRequest.GetResponse;
                if (((HttpWebResponse)httpWebRequest.GetResponse()).StatusCode == HttpStatusCode.OK)
                {

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="File"></param>
        public static void Del(string File)
        {
            delEvent.AddFile(File);
        }
        internal class DelEvent
        {
            private List<string> DelFilelist = new();
            private bool IsDelEnable = false;
            internal void AddFile(string File)
            {
                DelFilelist.Add(File);
                if (!IsDelEnable)
                {
                    IsDelEnable = true;
                    Del();
                }
            }
            private void Del()
            {
                Task.Run(() => {
                    while (true)
                    {
                        for (int i = DelFilelist.Count - 1 ; i > -1 ; i--)
                        {
                            if (File.Exists(DelFilelist[i]))
                            {
                                try
                                {
                                    File.Delete(DelFilelist[i]);
                                    DelFilelist.RemoveAt(i);
                                }
                                catch (Exception) { }
                            }
                            else
                            {
                                try
                                {
                                    DelFilelist.RemoveAt(i);
                                }
                                catch (Exception) { }
                            }
                        }
                        Thread.Sleep(10000);
                    }
                });
            }
        }
    }
}
