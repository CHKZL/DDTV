using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using HandyControl.Controls;

namespace DDTV_GUI
{
    internal class TimedTask
    {
        public class CheckUpdate
        {
            private static bool IsOn = false;
            public static void Check()
            {
                if (!IsOn)
                {
                    IsOn = !IsOn;
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
                                keyValuePairs.Add("Ver", 1);
                                string WebText = Post("http://api.ddtv.pro/api/Ver", keyValuePairs);

                                //string WebText = Post("http://127.0.0.1/api/ver", keyValuePairs);
                                pack<VerClass> jo = JsonConvert.DeserializeObject<pack<VerClass>>(WebText);
                                if (jo != null)
                                {
                                    if (jo.data.Ver != DDTV_Window.MainWindow.Ver)
                                    {
                                        Growl.Ask($"检测到新版本:{jo.data.Ver}", isConfirmed =>
                                        {
                                            if (isConfirmed)
                                            {
                                                Growl.Info("这个功能还没写，请加群查看");
                                            }
                                            else
                                            {
                                                Growl.Info("半小时后再提醒");
                                            }
                                            return true;
                                        });
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }
                            Thread.Sleep(60 * 30 * 1000);
                            //Thread.Sleep(5* 1000);
                        }
                    });
                }
            }
            public class VerClass
            {
                public string Ver { get; set; }
            }
        }
        public class DokiDoki
        {
            private static bool IsOn = false;
            public static void Check()
            {
                if (!IsOn)
                {
                    IsOn = !IsOn;
                    int Conut = 0;
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                Dictionary<string, int> keyValuePairs = new()
                                {
                                    { "Conut", Conut }
                                };
                                string WebText = Post("http://api.ddtv.pro/api/Ver", keyValuePairs);
                                Conut++;
                            }
                            catch (Exception)
                            {

                            }
                            Thread.Sleep(60 * 60 * 1000);
                        }
                    });
                }
            }
        }
        private class pack<T>
        {
            public int code { get; set; }
            public string cmd { get; set; }
            public string massage { get; set; }
            public T data { get; set; }
        }
        private static string Post(string url, Dictionary<string, int> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = $"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36 DDTV/{DDTV_Window.MainWindow.Ver}";
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
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}
