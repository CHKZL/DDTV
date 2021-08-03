using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class update
    {
        public static void 检查升级程序是否需要升级(string name)
        {
            try
            {
                int 未变化文件 = 0;
                int 更新文件 = 0;
                int 缺少文件 = 0;
                string ConfigUrl = $"https://{name}-update.oss-cn-zhangjiakou.aliyuncs.com/update/Config.json";
                string a = 返回网页内容_GET(ConfigUrl);
                更新配置文件 student = JsonConvert.DeserializeObject<更新配置文件>(a);
                string 参考路径 = "./update/";
                foreach (var item in student.data)
                {
                    if (!File.Exists(参考路径 + item.Name))
                    {
                        缺少文件++;
                        //Console.Write("\n□(进度:{2}/{3})检测到本地缺少的文件:{0}({1})开始下载...", item.Name, CountSize(item.size), itemnum, student.data.Count);
                        var wc = new WebClient();
                        通过WC更新自动更新文件(item.Url, 参考路径 + item.Name);//File.ReadAllText("T:/Untitled-1.json");//;
                        //Console.WriteLine("{0}下载成功", item.Name);
                    }
                    else
                    {
                        if (GetMD5HashFromFile(参考路径 + item.Name) != item.Md5)
                        {
                            更新文件++;
                            //Console.Write("\n◇(进度:{2}/{3}) {0}({1})和服务端比较有差异，需要更新，开始下载...", item.Name, CountSize(item.size), itemnum, student.data.Count);
                            var wc = new WebClient();
                            通过WC更新自动更新文件(item.Url, 参考路径 + item.Name);//File.ReadAllText("T:/Untitled-1.json");//;
                            //Console.WriteLine("{0}下载成功", item.Name);
                        }
                        else
                        {
                            未变化文件++;
                            //Console.WriteLine("\n√(进度:{2}/{3}) {0}({1})文件未更新，跳过...", item.Name, CountSize(item.size), itemnum, student.data.Count);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //  return true;
            }
        }
        public static void 通过WC更新自动更新文件(string URL, string File)
        {
            var wc = new WebClient();
            wc.Headers.Add("Referer: Update.ddtv.pro");
            wc.DownloadFile(URL, File);//File.ReadAllText("T:/Untitled-1.json");//;
        }
        public static string 返回网页内容_GET(string url)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Referer = "Update.ddtv.pro";
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {

                Console.WriteLine("对比文件:{0}失败，该文件不存在或权限不足{1}:", fileName, ex.Message);
                return "";
            }
        }
        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>
        public static string CountSize(long Size)
        {
            string m_strSize = "";
            long FactSize = 0;
            FactSize = Size;
            if (FactSize < 1024.00)
                m_strSize = FactSize.ToString("F2") + " Byte";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " K";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " M";
            else if (FactSize >= 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " G";
            return m_strSize;
        }
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="sFullName"></param>
        /// <returns></returns>
        public static long GetFileSize(string sFullName)
        {
            long lSize = 0;
            if (File.Exists(sFullName))
                lSize = new FileInfo(sFullName).Length;
            return lSize;
        }
        public class 更新配置文件
        {
            public string Ver { set; get; }
            public string Text { set; get; }
            public List<文件信息> data { set; get; }

        }
        public class 文件信息
        {
            public string Name { set; get; }
            public string Md5 { set; get; }
            public string Url { set; get; }
            public long size { set; get; }
        }
    }
}
