using Auxiliary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace 主站视频下载工具
{
    class Program
    {
        static void Main(string[] args)
        {
            MMPU.BiliUser配置文件初始化(1);
            GO();
            while (true)
            {
                Console.ReadKey();
            }
        }
        public static void GO()
        {
        A: Console.Write("输入视频连接:");
            string 视频URL = Console.ReadLine();
            if (string.IsNullOrEmpty(视频URL))
            {
                Console.WriteLine("输入的视频内容为空！");
                goto A;
            }
            string ABV = string.Empty;
            try
            {
                ABV = 视频URL.Replace("/video/", "⒆").Split('⒆')[1].Split('?')[0];
            }
            catch (Exception)
            {
                Console.WriteLine("输入的视频内容不正确，正确的格式应为:https://www.bilibili.com/video/XXXXXXXX 样式");
                goto A;
            }
            JObject JO = JObject.Parse(MMPU.使用WC获取网络内容("https://api.bilibili.com/x/web-interface/view?bvid=" + ABV + "&jsonp=jsonp"));
            BiliVideoInfo.VideoInfo.Root videolist = new BiliVideoInfo.VideoInfo.Root() { data = new List<BiliVideoInfo.VideoInfo.data>() };
            videolist.BV = JO["data"]["bvid"].ToString();
            foreach (var item in JO["data"]["pages"])
            {
                JToken dataElem = item;
                videolist.data.Add(new BiliVideoInfo.VideoInfo.data()
                {
                    cid = int.Parse(dataElem["cid"].ToString()),
                    duration = int.Parse(dataElem["duration"].ToString()),
                    from = dataElem["from"].ToString(),
                    page = int.Parse(dataElem["page"].ToString()),
                    part = dataElem["part"].ToString(),
                    vid = dataElem["vid"].ToString(),
                    weblink = dataElem["weblink"].ToString(),
                    dimension = new BiliVideoInfo.VideoInfo.dimension()
                    {
                        height = int.Parse(dataElem["dimension"]["height"].ToString()),
                        width = int.Parse(dataElem["dimension"]["width"].ToString()),
                        rotate = int.Parse(dataElem["dimension"]["rotate"].ToString()),
                    }
                });
            }
        B: Console.WriteLine($"有{videolist.data.Count}个分页");
            int i = 0;
            foreach (var item in videolist.data)
            {
                Console.WriteLine($"分页{i}:分辨率:{item.dimension.width}x{item.dimension.height},标题:{item.part},长度{item.duration}秒");

                i++;
            }
            Console.Write("请选择下载的分页:");
            string 页 = Console.ReadLine();
            int 下载页 = 0;
            if (string.IsNullOrEmpty(页))
            {
                Console.WriteLine("输入的下载页编号有误");
                goto B;
            }
            try
            {
                下载页 = int.Parse(页);
            }
            catch (Exception)
            {
                Console.WriteLine("输入的下载页编号有误");
                goto B;
            }
            JObject 下载对象 = JObject.Parse(MMPU.使用WC获取网络内容("https://api.bilibili.com/x/player/playurl?bvid=" + videolist.BV + "&cid=" + videolist.data[下载页].cid + "&type=json"));
            下载对象 = JObject.Parse(MMPU.使用WC获取网络内容("https://api.bilibili.com/x/player/playurl?qn=" + 下载对象["data"]["accept_quality"][0].ToString() + "&bvid=" + videolist.BV + "&cid=" + videolist.data[下载页].cid + "&type=json"));
            总大小 = 下载对象["data"]["durl"][0]["size"].ToString();
            总大小 = 转换下载大小数据格式(double.Parse(总大小));
            Console.WriteLine("开始下载");
            下载(下载对象["data"]["durl"][0]["url"].ToString(), videolist.data[下载页].part);
        }
        public static string 总大小 = "";
        public static void 下载(string url, string 文件名)
        {
            WebClient WC = new WebClient();
            WC.Headers.Add("Accept: */*");
            WC.Headers.Add("User-Agent: AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4305.2 Safari/537.36");
            WC.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            WC.DownloadFileCompleted += 下载完成事件;
            WC.DownloadProgressChanged += 下载过程中事件;
            // rq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            WC.Headers.Add("Accept-Encoding: gzip, deflate, br");
            WC.Headers.Add("Cache-Control: max-age=0");
            WC.Headers.Add("Sec-Fetch-Mode: navigate");
            WC.Headers.Add("Sec-Fetch-Site: none");
            WC.Headers.Add("Sec-Fetch-User: ?1");
            WC.Headers.Add("Upgrade-Insecure-Requests: 1");
            WC.Headers.Add("Cache-Control: max-age=0");
            WC.Headers.Add("Referer: https://www.bilibili.com/");
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                WC.Headers.Add("Cookie", MMPU.Cookie);
            }
            WC.DownloadFileTaskAsync(new Uri(url), "./录制/" + 文件名+".flv");
        }

        public static int GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt32(ts.TotalSeconds);
        }
        public static int 当前时间 = 0;
        private static void 下载过程中事件(object sender, DownloadProgressChangedEventArgs e)
        {
            if (当前时间 != GetTimeStamp())
            {
                当前时间 = GetTimeStamp();
                Console.Clear();
                var bytes = e.BytesReceived;
                string 已下载大小str = 转换下载大小数据格式(bytes);
                Console.WriteLine($"下载中:{已下载大小str}/{总大小}");
            }

        }

        private static void 下载完成事件(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.Clear();
            Console.WriteLine("任务下载完成");
            GO();
        }
        public static string 转换下载大小数据格式(double size)
        {
            if (size <= 1024)
            {
                return size.ToString("F2") + "B";
            }
            if (size <= 1048576)
            {
                return (size / 1024.0).ToString("F2") + "KB";
            }
            if (size <= 1073741824)
            {
                return (size / 1048576.0).ToString("F2") + "MB";
            }
            if (size <= 1099511627776)
            {
                return (size / 1073741824.0).ToString("F2") + "GB";
            }
            return (size / 1099511627776.0).ToString("F2") + "TB";
        }
    }
    public class BiliVideoInfo
    {
        public class VideoInfo
        {
            public static List<Root> Info = new List<Root>();
            public class Root
            {
                public int code { get; set; }

                public string message { get; set; }

                public int ttl { get; set; }
                public string title { set; get; }
                public string BV { set; get; }

                public List<data> data { get; set; }
            }

            public class data
            {
                /// <summary>
                /// 视频CIA，最小单位
                /// </summary>
                public int cid { get; set; }
                /// <summary>
                /// 分P
                /// </summary>
                public int page { get; set; }
                /// <summary>
                /// 来源类别
                /// </summary>
                public string from { get; set; }
                /// <summary>
                /// 分P名称
                /// </summary>
                public string part { get; set; }
                /// <summary>
                /// 长度,秒
                /// </summary>
                public int duration { get; set; }
                /// <summary>
                /// vid
                /// </summary>
                public string vid { get; set; }
                /// <summary>
                /// weblink
                /// </summary>
                public string weblink { get; set; }
                /// <summary>
                /// 分辨率
                /// </summary>
                public dimension dimension { get; set; }
            }

            public class dimension
            {
                /// <summary>
                /// 视频宽度
                /// </summary>
                public int width { get; set; }
                /// <summary>
                /// 视频高度
                /// </summary>
                public int height { get; set; }
                /// <summary>
                /// 视频旋转
                /// </summary>
                public int rotate { get; set; }
            }

        }
    }
}
