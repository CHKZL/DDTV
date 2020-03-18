using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace Auxiliary
{
    public class MMPU
    {
        public static 弹窗提示 弹窗 = new 弹窗提示();
        public static List<Downloader> DownList = new List<Downloader>();
        public static string 直播缓存目录 ="";
        public static int 直播更新时间 = 40;
        public static string 下载储存目录 = "";
        public static string 版本号 = "2.0.2.1";
        public static string[] 不检测的版本号 = {"2.0.2.0b", "2.0.2.0c" , "2.0.2.0d" , "2.0.2.0e" , "2.0.2.0f", "2.0.2.0g", "2.0.2.0" };
        public static bool 第一次打开播放窗口 = true;
        public static int 默认音量= 0;
        public static int 缩小功能 = 1;
        public static bool 连接404使能 = false;
        public static bool 是否能连接404 = false;
        public static bool 是否能连接阿B = true;
        public static int 最大直播并行数量 = 0;
        public static int 当前直播窗口数量 = 0;
        public static string 默认弹幕颜色 = "";
        public static string 默认字幕颜色 = "";
        public static int 默认字幕大小 = 24;
        public static int 默认弹幕大小 = 20;
        public static int PlayWindowH = 0;
        public static int PlayWindowW = 0;
        public static int versionMajor = Environment.OSVersion.Version.Major;
        public static int 直播列表刷新间隔 = 0;
        public static string Cookie = "";
        public static string csrf = "";
        public static DateTime CookieEX = new DateTime();
        public static string UID = "";
        public static string BiliUserFile= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BiliUser.ini");
        public static int 播放缓冲时长 = 3;

        public static string 房间状态MD5值 = string.Empty;
        public static void 修改默认音量设置(int A)
        {
            默认音量 = A;
            MMPU.setFiles("DefaultVolume", A.ToString());
        }
        public static string 寻找下载列表键值(string str,string name)
        {
            str = str.Replace(" ", "").Replace("\"", "").Replace("{", "").Replace("}", "");
            foreach (var item in str.Split(','))
            {
                if(item.Split('=')[0]==name)
                {
                    return item.Split('=')[1];
                }
            }
            return null;
        }

        /// <summary>
        /// 将时间戳转换为日期类型，并格式化
        /// </summary>
        /// <param name="longDateTime"></param>
        /// <returns></returns>
        public static string 将时间戳转换为日期类型(long longDateTime)
        {
            //用来格式化long类型时间的,声明的变量
            long unixDate;
            DateTime start;
            DateTime date;
            //ENd

            unixDate = longDateTime;
            start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            date = start.AddMilliseconds(unixDate).ToLocalTime();

            return date.ToString("yyyy-MM-dd HH:mm:ss");

        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="name">值名称</param>
        /// <param name="V">默认值</param>
        public static string 读取exe默认配置文件(string name,string V)
        {
            string A1 = V;
            try
            {
                A1 = getFiles(name);
            }
            catch (Exception)
            {
                setFiles(name, V);
            }
            return A1;
        }

        public static string 读ini配置文件(string 节点, string 项目,string 路径)
        {

            return bilibili.BiliUser.Read(节点, 项目, null, 路径);
        }

        public static string 写ini配置文件(string 节点, string 项目,string 值, string 路径)
        {
            bilibili.BiliUser.Write(节点, 项目, 值, 路径);
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">目标网页地址</param>
        /// <param name="headername">http标头名</param>
        /// <param name="headervl">http标头值</param>
        /// <returns></returns>
        public static string 返回网页内容(string url)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        public static string 使用WC获取网络内容(string url)
        {
            var wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                wc.Headers.Add("Cookie", MMPU.Cookie);
            }
            byte[] roomHtml = wc.DownloadData(url);
            return Encoding.UTF8.GetString(roomHtml);
        }
        public static string 获取网页数据_下载视频用(string url,bool 解码)
        {
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
            rq.CookieContainer = new CookieContainer();
            rq.Method = "GET";
            rq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            rq.Headers.Add("Accept-Encoding: gzip, deflate, br");
            rq.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            rq.Headers.Add("Accept-Encoding: gzip, deflate, br");
            rq.Headers.Add("Cache-Control: max-age=0");
            rq.Headers.Add("Sec-Fetch-Mode: navigate");
            rq.Headers.Add("Sec-Fetch-Site: none");
            rq.Headers.Add("Sec-Fetch-User: ?1");
            rq.Headers.Add("Upgrade-Insecure-Requests: 1");
            rq.Headers.Add("Cache-Control: max-age=0");
            //rq.Host = "www.bilibili.com";
            rq.UserAgent = Ver.UA;
            if (解码)
            {
                HttpWebResponse webResponse = (System.Net.HttpWebResponse)rq.GetResponse();
                string contentype = webResponse.Headers["Content-Type"];
                string responseString = "";
                using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                {

                    using (var zipStream = new System.IO.Compression.GZipStream(streamReceive, System.IO.Compression.CompressionMode.Decompress))
                    {
                        Regex regex = new Regex("charset\\s*=\\s*[\\W]?\\s*([\\w-]+)", RegexOptions.IgnoreCase);
                        if (regex.IsMatch(contentype))
                        {
                            Encoding ending = Encoding.GetEncoding(regex.Match(contentype).Groups[1].Value.Trim());
                            using (StreamReader sr = new System.IO.StreamReader(zipStream, ending))
                            {
                                responseString = sr.ReadToEnd();
                            }
                        }
                    }
                }
                return responseString;
            }
            else
            {
                HttpWebResponse resp = (HttpWebResponse)rq.GetResponse();
                using (Stream stream = resp.GetResponseStream())
                {

                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    String responseString = reader.ReadToEnd();
                    return responseString;
                }
            }
        }
        public class 加载网络房间方法
        {
            public static List<列表加载缓存> 列表缓存 = new List<列表加载缓存>();
            public static void 更新网络房间缓存()
            {
                int A = 1;
                new Thread(new ThreadStart(delegate
                {
                    var wc = new WebClient();
                    wc.Headers.Add("Accept: */*");
                    wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
                    byte[] roomHtml = wc.DownloadData("https://gitee.com/SYXM/vdb/raw/master/json/list.json");
                    var result = JObject.Parse(Encoding.UTF8.GetString(roomHtml));

                    foreach (var item in result["vtbs"])
                    {
                        foreach (var x in item["accounts"])
                        {
                            if (x["platform"].ToString() == "bilibili")
                            {

                                string name = "";
                                if (item["name"].ToString().Contains("ch"))
                                {
                                    try
                                    {
                                        name = item["name"]["ch"].ToString();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                                else if (item["name"].ToString().Contains("cn"))
                                {
                                    try
                                    {
                                        name = item["name"]["cn"].ToString();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                                列表缓存.Add(new 列表加载缓存
                                {
                                    编号 = A,
                                    名称 = name,
                                    官方名称 = item["name"][item["name"]["default"].ToString()].ToString(),
                                    平台 = "bilibili",
                                    UID = x["id"].ToString(),
                                    类型 = x["type"].ToString()
                                });
                                A++;
                            }
                        }
                    }
                    //this.Dispatcher.Invoke(new Action(delegate
                    //{
                    //    选中内容展示.Content = "";
                    //    更新网络房间列表.IsEnabled = true;
                    //}));
                })).Start();

            }
            public class 列表加载缓存
            {
                public int 编号 { set; get; }
                public string 名称 { set; get; }
                public string 官方名称 { set; get; }
                public string 平台 { set; get; }
                public string UID { set; get; }
                public string 类型 { set; get; }
            }

            public class 选中的网络房间
            {
                public int 编号 { set; get; }
                public string 名称 { set; get; }
                public string 官方名称 { set; get; }
                public string UID { set; get; }
                public string 房间号 { set; get; }
                public string 平台 { set; get; }
            }
        }
       
        public static double 测试延迟(string Url)
        {
            try
            {
                WebClient wcl = new WebClient();
                Stopwatch spwatch = new Stopwatch();
                spwatch.Start();
                byte[] resultBytes = wcl.DownloadData(new Uri(Url));
                spwatch.Stop();
                return spwatch.Elapsed.TotalMilliseconds;
            }
            catch (Exception)
            {

                return -1;
            }
        }
        public class 弹窗提示
        {
            public string 标题 { set; get; } = "";
            public string 内容 { set; get; } = "";
            public int 持续时间 { set; get; } = 0;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="A">持续时间(毫秒)</param>
            /// <param name="B">标题</param>
            /// <param name="C">内容</param>
            public void Add(int A, string B, string C)
            {
                标题 = B;
                内容 = C;
                持续时间 = A;
                IcoUpdate?.Invoke(this, EventArgs.Empty);
            }
            public event EventHandler<EventArgs> IcoUpdate;
        }

        public class 判断网络路径是否存在
        {
            public static bool 是否判断 = false;
            public static bool 是否存在 = true;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="url">目标路径</param>
            /// <param name="P">平台</param>
            /// <returns></returns>
            public bool 判断(string url, string P)
            {
                try
                {
                    switch (P)
                    {
                        case "bilibili":
                            {

                                HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.CreateDefault(new Uri(url));
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.UserAgent = Ver.UA;
                                httpWebRequest.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
                                // httpWebRequest
                                httpWebRequest.Timeout = 3000;
                                //返回响应状态是否是成功比较的布尔值

                                if(((HttpWebResponse)httpWebRequest.GetResponse()).StatusCode==HttpStatusCode.OK)
                                {

                                }
                                return true;
                            }
                        case "主站视频":
                            return true;
                        default:
                            return false;
                    }
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }

 

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        public static void 文件删除委托(string file)
        {

            new Thread(new ThreadStart(delegate {
                int i = 0;
                try
                {
                    while (true)
                    {
                        if (i > 10)
                        {
                            return;
                        }
                        if (!文件是否正在被使用(file))
                        {
                            File.Delete(file);
                            return;
                        }
                        i++;
                        Thread.Sleep(100);
                    }
                }
                catch (Exception)
                {
                }
            })).Start();
          
        }
        public static bool 文件是否正在被使用(string fileName)
        {
            bool inUse = true;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,
                FileShare.None);
                fs.Close();
                inUse = false;
            }
            catch
            {

            }
            return inUse;//true表示正在使用,false没有使用
        }
        /// <summary>
        /// 获得13位的时间戳
        /// </summary>
        /// <returns></returns>
        public static long 获取时间戳()
        {
            DateTime time = DateTime.Now;
            return DateTime转换为Unix(time);

        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long DateTime转换为Unix(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
        /// <summary>    
        /// 时间戳转为C#格式时间    
        /// </summary>    
        /// <param name=”timeStamp”></param>    
        /// <returns></returns>    
        public static DateTime Unix转换为DateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        public static void 储存文本(string data, string CurDir)
        {
            //文件覆盖方式添加内容
            System.IO.StreamWriter file = new System.IO.StreamWriter(CurDir, false);
            //保存数据到文件
            file.Write(data);
            //关闭文件
            file.Close();
            //释放对象
            file.Dispose();
        }
        public class 获取livelist平台和唯一码
        {
            public static string 编号(string A)
            {
                return GETA(A, "编号");
            }
            public static string 名称(string A)
            {
                return GETA(A, "名称");
            }
            public static string 原名(string A)
            {
                return GETA(A, "原名");
            }
            public static string 状态(string A)
            {
                return GETA(A, "状态");
            }
            public static string 平台(string A)
            {
                return GETA(A, "平台");
            }
            public static string 是否提醒(string A)
            {
                return GETA(A, "是否提醒");
            }
            public static string 是否录制(string A)
            {
                return GETA(A, "是否录制");
            }
            public static string 唯一码(string A)
            {
                return GETA(A, "唯一码");
            }
            private static string GETA(string A, string name)
            {
                A = A.Replace("\"", "").Replace(" ", "").Replace("{", "").Replace("}", "");
                string[] B = A.Split(',');
                foreach (var item in B)
                {
                    if (item.Split('=')[0] == name)
                    {
                        return item.Split('=')[1];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        public static string getFiles(string name)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings[name].Value;

        }
        /// <summary>
        /// 修改和添加AppSettings中配置 如果相应的Key存在则修改 如不存在则添加
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">相应值</param>
        public static bool setFiles(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                {
                    config.AppSettings.Settings[key].Value = value;
                }
                else
                {
                    config.AppSettings.Settings.Add(key, value);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                return false;
            }
        }
        ///// <summary>
        ///// 保存配置
        ///// </summary>
        ///// <param name="files"></param>
        //public static void setFiles(string name, string Value)
        //{
        //    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //    config.AppSettings.Settings[name].Value = Value;
        //    config.Save(ConfigurationSaveMode.Modified);

        //}
        public static string TcpSend(int code, string msg, bool 是否需要回复)
        {
            try
            {
                string 回复内容 = "";
                Socket tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipaddress = IPAddress.Parse("39.98.207.17");
                EndPoint point = new IPEndPoint(ipaddress, 11433);
                tcpClient.Connect(point);//通过IP和端口号来定位一个所要连接的服务器端
                tcpClient.Send(Encoding.UTF8.GetBytes(JSON发送拼接(code, msg)));
                if (是否需要回复)
                {
                    byte[] buffer = new byte[1024 * 1024];
                    tcpClient.Receive(buffer);
                    string 收到的数据 = Encoding.UTF8.GetString(buffer).Trim('\0');
                    回复内容 = 收到的数据;
                }
                else
                {
                    回复内容 = null;
                }
                tcpClient.Close();
                return 回复内容;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>返回32位字符串</returns>
        public static string GetMD5(string inputString)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] encryptedBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(inputString));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                sb.AppendFormat("{0:x2}", encryptedBytes[i]);
            }
            return sb.ToString();
        }

        public static string JSON发送拼接(int code, string msg)
        {
            return "{\"code\":\"" + code + "\",\"msg\":\"" + msg.Replace("\"", "\\\"") + "\"}";
        }
        /// <summary>
        /// 指定Post地址和添加cookis使用Get 方式获取网页返回内容  
        /// </summary>  
        /// <param name="url">请求后台地址</param>  
        /// <param name="dic">Post表参数</param>
        /// <param name="cook">cookis</param>
        /// <returns></returns>
        public static string 返回网页内容(string url, Dictionary<string, string> dic, CookieContainer cook)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
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
            return result;
        }

        public class SQL中间件数据格式
        {
            public string SQL { set; get; }
            public List<SQL数据对> code { set; get; }
        }
        public class SQL数据对
        {
            public string name { set; get; }
            public string val { set; get; }
        }
    }
}
