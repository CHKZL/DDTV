using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.IO.Path;

namespace Auxiliary
{
    public class Downloader
    {
       
        public DownIofoData DownIofo = new DownIofoData();
        public class DownIofoData
        {
            public WebClient _wc { set; get; }
            public bool 下载状态 { set; get; }
            public double 已下载大小bit { set; get; }
            public string 已下载大小str { set; get; }
            public string 文件保存路径 { set; get; }
            public string 事件GUID { set; get; }
            public string 备注 { set; get; }
            public int 开始时间 { set; get; }
            public int 结束时间 { set; get; }
            public string 房间_频道号 { set; get; }
            public string 平台 { set; get; }
            public bool 是否保存 { set; get; }
            public string 下载地址 { set; get; }
            public string 标题 { set; get; }
            public bool 播放状态 { set; get; }
            public bool 是否是播放任务 { set; get; }

        }
        public string Start(string 开始后显示的备注)
        {
            DownIofo._wc = new WebClient();
            DownIofo._wc.Headers.Add("Accept: */*");
            DownIofo._wc.Headers.Add("User-Agent: " + Ver.UA);
            DownIofo._wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            DownIofo._wc.DownloadFileCompleted += 下载完成事件;
            DownIofo._wc.DownloadProgressChanged += 下载过程中事件;
            DownIofo.下载状态 = true;
            
            if (!Directory.Exists(GetDirectoryName(DownIofo.文件保存路径)))
            {
                Directory.CreateDirectory(GetDirectoryName(DownIofo.文件保存路径));
            }
            // ReSharper restore AssignNullToNotNullAttribute
            DownIofo.备注 = "等待接收直播数据流";
            MMPU.判断网络路径是否存在 判断文件是否存在 = new MMPU.判断网络路径是否存在();
            while(true)
            {
                switch(DownIofo.平台)
                {
                    case "bilibili":
                        if (!bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                        {
                            DownIofo.下载状态 = false;
                            DownIofo.备注 = "该房间未直播";
                            return null;
                        }
                        break;
                    default:
                        DownIofo.下载状态 = false;
                        DownIofo.备注 = "不支持的平台";
                        return null;
                }
                
                if(判断文件是否存在.判断(DownIofo.下载地址,DownIofo.平台))
                {
                    break;
                }
                else
                {
                    Thread.Sleep(500);
                    if (bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                    {
                        DownIofo.下载地址 = bilibili.根据房间号获取房间信息.下载地址(DownIofo.房间_频道号);
                    }
                }
            }
            DownIofo.开始时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            DownIofo._wc.DownloadFileAsync(new Uri(DownIofo.下载地址), DownIofo.文件保存路径);
            DownIofo.备注 = 开始后显示的备注;
            return DownIofo.文件保存路径;
        }
        public static Downloader 新建下载对象(string 平台,string 唯一码,string 标题,string GUID,string 下载地址,string 备注,bool 是否保存)
        {
            Downloader 下载对象 = new Downloader();
            string 保存路径 = string.Empty;

            if(MMPU.下载储存目录=="./tmp/")
            {
                保存路径 = AppDomain.CurrentDomain.BaseDirectory + "tmp\\" + 平台 + "_" + 唯一码 + "\\";
                if (Directory.Exists(保存路径))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory(保存路径);
                }
                保存路径 = 保存路径 + 标题 +"_"+ GUID + ".flv";
            }
            else
            {
                保存路径 = MMPU.下载储存目录+"\\" + 平台+"_" + 唯一码 + "\\";
                if (Directory.Exists(保存路径))//如果不存在就创建file文件夹
                {
                    Directory.CreateDirectory(保存路径);
                }
                保存路径 = 保存路径 + 标题 + "_" + GUID + ".flv";
                //保存路径 = MMPU.下载储存目录
            }
            下载对象.DownIofo = new Downloader.DownIofoData
            {
                平台 = 平台,
                房间_频道号 = 唯一码,

                文件保存路径 = 保存路径,
                事件GUID = GUID,
                下载地址 = 下载地址,
                是否保存 = 是否保存,
                标题 = 标题,
                备注 = 备注
            };
            MMPU.DownList.Add(下载对象);
            下载对象.Start(标题);
            
            return 下载对象;
        }
        private void 下载过程中事件(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytes = e.BytesReceived;
            DownIofo.已下载大小bit = bytes;
            DownIofo.已下载大小str = 转换下载大小数据格式(bytes);
            
            //DownUpdate?.Invoke(this, EventArgs.Empty);
        }
       // public event EventHandler<EventArgs> DownUpdate;
       // public event EventHandler<EventArgs> DownOk;
        private void 下载完成事件(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == false)
            {
                DownIofo.备注 = "下载完成";
                DownIofo.下载状态 = false;
            }
            else if (e.Error.Message.Contains("请求被中止"))
            {              
                DownIofo.备注 = "用户取消，停止下载";   
                if(!DownIofo.播放状态&&DownIofo.是否是播放任务)
                {
                    DownIofo.备注 = "播放窗口关闭";
                }
            }
            else
            {
                if (bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                {
                    switch (DownIofo.平台)
                    {
                        case "bilibili":
                            {
                                Downloader 下载对象 = Downloader.新建下载对象(DownIofo.平台, DownIofo.房间_频道号, bilibili.根据房间号获取房间信息.获取标题(DownIofo.房间_频道号), Guid.NewGuid().ToString(), bilibili.根据房间号获取房间信息.下载地址(DownIofo.房间_频道号), "播放缓冲重连", false);
                                if(!下载对象.DownIofo.下载状态)
                                {
                                    下载对象.DownIofo.备注 = "该房间当前状态不能获取到直播流";  
                                    return;
                                }
                                new Thread(new ThreadStart(delegate
                                {
                                    while(true)
                                    {
                                        Thread.Sleep(1000);
                                        if (下载对象.DownIofo.已下载大小bit > 1000)
                                        {
                                            return;
                                        }
                                        if (!bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                                        {
                                            下载对象.DownIofo.备注 = "停止直播";
                                            下载对象.DownIofo._wc.CancelAsync();
                                            MMPU.DownList.Remove(下载对象);
                                            return;
                                        }
                                    }
                                })).Start();
                                break;
                            }
                        default:
                           Console.WriteLine("发现了与当前版本不支持的平台，请检查更新");
                            return;
                    }
                }
            }
            DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            DownIofo.下载状态 = true;
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
}
