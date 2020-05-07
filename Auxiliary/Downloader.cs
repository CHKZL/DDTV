using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.IO.Path;

namespace Auxiliary
{
    public class Downloader
    {
        public DownIofoData DownIofo = new DownIofoData()
        {
            继承 = new 继承()
        };
        public class DownIofoData
        {
            public WebClient WC { set; get; }
            public bool 下载状态 { set; get; } = false;
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
            public string 重连文件路径 { set; get; }
            public string 主播名称 { set; get; }    
            public 继承 继承 { set; get; }
            public bool 是否是固定视频 { set; get; } = false;
        }
        public class 继承
        {
            public bool 是否为继承对象 { set; get; } = false;
            public string 继承的下载文件路径 { set; get; } = null;
            public string 合并后的文件路径 { set; get; } = null;
        }
        public string Start(string 开始后显示的备注)
        {
            int a = 0;
            DownIofo.WC = new WebClient();
            DownIofo.WC.Headers.Add("Accept: */*");
            DownIofo.WC.Headers.Add("User-Agent: " + MMPU.UA.Ver.UA());
            DownIofo.WC.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            DownIofo.WC.DownloadFileCompleted += 下载完成事件;
            DownIofo.WC.DownloadProgressChanged += 下载过程中事件;
            // rq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            DownIofo.WC.Headers.Add("Accept-Encoding: gzip, deflate, br");
            DownIofo.WC.Headers.Add("Cache-Control: max-age=0");
            DownIofo.WC.Headers.Add("Sec-Fetch-Mode: navigate");
            DownIofo.WC.Headers.Add("Sec-Fetch-Site: none");
            DownIofo.WC.Headers.Add("Sec-Fetch-User: ?1");
            DownIofo.WC.Headers.Add("Upgrade-Insecure-Requests: 1");
            DownIofo.WC.Headers.Add("Cache-Control: max-age=0");
            DownIofo.WC.Headers.Add("Referer: https://www.bilibili.com/");
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                DownIofo.WC.Headers.Add("Cookie", MMPU.Cookie);
            }

            if (!Directory.Exists(GetDirectoryName(DownIofo.文件保存路径)))
            {

                Directory.CreateDirectory(GetDirectoryName(DownIofo.文件保存路径));

            }
            // ReSharper restore AssignNullToNotNullAttribute
            DownIofo.备注 = "等待接收直播数据流";
            MMPU.判断网络路径是否存在 判断文件是否存在 = new MMPU.判断网络路径是否存在();
            while (true)
            {
                Thread.Sleep(1000);
                switch (DownIofo.平台)
                {
                    case "bilibili":
                        if (!bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                        {
                            InfoLog.InfoPrintf(DownIofo.房间_频道号 + "房间:" + DownIofo.主播名称 + " 房间直播状态为False,下载任务结束", InfoLog.InfoClass.下载必要提示);
                            DownIofo.下载状态 = false;
                            DownIofo.备注 = "该房间未直播";
                            DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                            return null;
                        }
                        break;
                    case "主站视频":
                        break;
                    default:
                        DownIofo.下载状态 = false;
                        DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                        DownIofo.备注 = "不支持的平台";
                        return null;
                }
                
                if(判断文件是否存在.判断(DownIofo.下载地址,DownIofo.平台, DownIofo.房间_频道号))
                {
                    break;
                }
                else
                {
                    Thread.Sleep(5000);
                    switch (DownIofo.平台)
                    {
                        case "bilibili":
                            {
                                if (bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                                {

                                    a++;
                                    if (判断文件是否存在.判断(DownIofo.下载地址, DownIofo.平台, DownIofo.房间_频道号))
                                    {
                                        //DownIofo.下载地址 = bilibili.根据房间号获取房间信息.下载地址(DownIofo.房间_频道号);
                                        break;
                                    }
                                    else
                                    {
                                        DownIofo.下载地址 = bilibili.根据房间号获取房间信息.下载地址(DownIofo.房间_频道号);
                                        if (判断文件是否存在.判断(DownIofo.下载地址, DownIofo.平台, DownIofo.房间_频道号))
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    InfoLog.InfoPrintf(DownIofo.房间_频道号 + "房间:" + DownIofo.主播名称 + " 房间未直播，下载任务取消", InfoLog.InfoClass.下载必要提示);
                                    DownIofo.下载状态 = false;
                                    DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                    if (DownIofo.继承.是否为继承对象)
                                    {
                                        //MMPU.弹窗.Add(3000, "重连任务取消", DownIofo.房间_频道号 + "，该房间未直播");
                                    }
                                    else
                                    {
                                        MMPU.弹窗.Add(3000, "下载任务取消", DownIofo.房间_频道号 + "，该房间未直播");
                                    }
                                    DownIofo.备注 = "该房间未直播";
                                    return null;
                                }
                                break;
                            }
                        case "主站视频":
                            {
                                break;
                            }
                    }
                    
                }
            }
            DownIofo.开始时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            
            try
            {
                DownIofo.WC.DownloadFileTaskAsync(new Uri(DownIofo.下载地址), DownIofo.文件保存路径);
            }
            catch (WebException)
            {
                ;
                //throw;
            }
            DownIofo.备注 = 开始后显示的备注;
            DownIofo.下载状态 = true;
            return DownIofo.文件保存路径;
        }
        public static int testNum = 0;
        public static Downloader 新建下载对象(string 平台, string 唯一码, string 标题, string GUID, string 下载地址, string 备注, bool 是否保存,string 主播名称,bool 是否为继承项目,string 继承项目的原始文件)
        {
            Downloader 下载对象 = new Downloader();
            下载对象.DownIofo.继承 = new 继承();
            string 保存路径;
            if (MMPU.下载储存目录 == "./tmp/")
            {
                保存路径 =  "./tmp/" + 平台+"_"+ 主播名称 + "_" + 唯一码 + "/";
                if (Directory.Exists(保存路径))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory(保存路径);
                }
                保存路径 = 保存路径 + 标题 + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".flv";
            }
            else
            {
                保存路径 = MMPU.下载储存目录 + "/" + 平台 + "_" + 主播名称 + "_" + 唯一码 + "/";
                if (!Directory.Exists(保存路径))//如果不存在就创建file文件夹
                {
                    try
                    {
                        Directory.CreateDirectory(保存路径);
                    }
                    catch (Exception)
                    {
                        MMPU.下载储存目录 = "./tmp/";
                        MMPU.setFiles("file", MMPU.下载储存目录);
                        保存路径 = MMPU.下载储存目录 + "/" + 平台 + "_" + 主播名称 + "_" + 唯一码 + "/";
                    }
                    
                }
                保存路径 = 保存路径 + 标题 + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".flv";
            }
            switch(平台)
            {
                case "bilibili":
                    {
                        下载地址 = 下载地址;
                        break;
                    }
                case "youtube":
                    {

                        break;
                    }
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
                备注 = 备注,
                主播名称 = 主播名称,
                继承=new 继承() { 
                是否为继承对象= 是否为继承项目,
                继承的下载文件路径= 继承项目的原始文件,
                
                }
            };
            if(!是否保存)
            {
                int 随机值 = new Random().Next(1000, 9999);
                下载对象.DownIofo.文件保存路径 = "./tmp/LiveCache/" + 下载对象.DownIofo.标题 + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + 随机值 + ".flv";
                if (File.Exists(下载对象.DownIofo.文件保存路径))
                {
                    下载对象.DownIofo.文件保存路径 = "./tmp/LiveCache/" + 下载对象.DownIofo.标题 + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + (随机值)+1 + ".flv";
                } 
            }     
            MMPU.DownList.Add(下载对象);
            下载对象.Start(备注);
            InfoLog.InfoPrintf(主播名称 + "开始直播，建立下载任务\n==============建立下载任务================\n主播名:" + 下载对象.DownIofo.主播名称 + "\n房间号:" + 唯一码 + "\n标题:" + 标题 + "\n开播时间:" + MMPU.Unix转换为DateTime(下载对象.DownIofo.开始时间.ToString())+ "\n保存路径:" + 下载对象.DownIofo.文件保存路径 + "\n下载任务类型:" + (下载对象.DownIofo.继承.是否为继承对象 ? "续下任务" : "新建下载任务") + "\n===============建立下载任务===============\n", InfoLog.InfoClass.下载必要提示);
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
            WebClient WWC = (WebClient)sender;
            new Task((() => 
            {
                DownIofo.下载状态 = false;
                DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                DownIofo.备注 = "下载任务结束";
                // if (e.Error != null && e.Error.)
                //Clipboard.SetDataObject(e.Error.Message);

                if (e.Cancelled)
                {
                    DownIofo.备注 = "用户取消，停止下载";
                    if (!DownIofo.播放状态 && DownIofo.是否是播放任务)
                    {
                        DownIofo.备注 = "播放窗口关闭";
                        InfoLog.InfoPrintf("下载任务结束\n==============下载任务结束================\n主播名:" + DownIofo.主播名称 + "\n房间号:" + DownIofo.房间_频道号 + "\n标题:" + DownIofo.标题 + "\n开播时间:" + DownIofo.开始时间 + "\n结束时间:"+ DownIofo .结束时间+ "\n保存路径:" + DownIofo.文件保存路径 + "\n下载任务类型:" + (DownIofo.继承.是否为继承对象 ? "续下任务" : "新建下载任务") + "\n结束原因:"+ DownIofo.备注 + "\n===============下载任务结束===============\n", InfoLog.InfoClass.下载必要提示);

                        return;
                    }
                }
                else if (e.Cancelled == false && !bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                {
                    
                    DownIofo.备注 = "直播停止，下载完成下载完成";
                    InfoLog.InfoPrintf("下载任务结束\n==============下载任务结束================\n主播名:" + DownIofo.主播名称 + "\n房间号:" + DownIofo.房间_频道号 + "\n标题:" + DownIofo.标题 + "\n开播时间:" + DownIofo.开始时间 + "\n结束时间:" + DownIofo.结束时间 + "\n保存路径:" + DownIofo.文件保存路径 + "\n下载任务类型:" + (DownIofo.继承.是否为继承对象 ? "续下任务" : "新建下载任务") + "\n结束原因:" + DownIofo.备注 + "\n===============下载任务结束===============\n", InfoLog.InfoClass.下载必要提示);
                    if (DownIofo.继承==null)
                    {
                        DownIofo.继承.是否为继承对象 = false;
                    }
                    if(DownIofo.继承.是否为继承对象 && !DownIofo.是否是播放任务)
                    {
                        DownIofo.继承.合并后的文件路径 = 下载完成合并FLV(DownIofo.继承.继承的下载文件路径, DownIofo.文件保存路径, true);
                        if(!string.IsNullOrEmpty(DownIofo.继承.合并后的文件路径))
                        {
                            DownIofo.文件保存路径 = DownIofo.继承.合并后的文件路径;
                        }
                    }
                    else if (!DownIofo.是否是播放任务)
                    {
                        FlvMethod.转码(DownIofo.文件保存路径);
                    }
                    InfoLog.InfoPrintf(DownIofo.房间_频道号 + "房间:" + DownIofo.主播名称 + " 下播，录制完成", InfoLog.InfoClass.下载必要提示);
                    return;
                }
                else
                {
                    if (bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号) && DownIofo.是否保存)
                    {
                        DownIofo.备注 = "下载流断开，但检测到房间仍为开播状态，已新建续下任务";
                        DownIofo.下载状态 = false;
                        DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                        switch (DownIofo.平台)
                        {
                            case "bilibili":
                                {
                                    if (DownIofo.继承.是否为继承对象 && !DownIofo.是否是播放任务)
                                    {
                                        DownIofo.继承.合并后的文件路径 = 下载完成合并FLV(DownIofo.继承.继承的下载文件路径, DownIofo.文件保存路径,false);
                                        if (!string.IsNullOrEmpty(DownIofo.继承.合并后的文件路径))
                                        {
                                            DownIofo.文件保存路径 = DownIofo.继承.合并后的文件路径;
                                        }
                                    }
                                   
                                    Downloader 下载对象 = Downloader.新建下载对象(DownIofo.平台, DownIofo.房间_频道号, bilibili.根据房间号获取房间信息.获取标题(DownIofo.房间_频道号), Guid.NewGuid().ToString(), bilibili.根据房间号获取房间信息.下载地址(DownIofo.房间_频道号), "重连", DownIofo.是否保存, DownIofo.主播名称, true, DownIofo.文件保存路径);
                                    if (!下载对象.DownIofo.下载状态)
                                    {
                                       
                                        下载对象.DownIofo.下载状态 = false;
                                        下载对象.DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                        下载对象.DownIofo.备注 = "服务器主动断开连接，直播结束";
                                        if (DownIofo.继承.是否为继承对象&&!DownIofo.是否是播放任务)
                                        {
                                            DownIofo.继承.合并后的文件路径 = 下载完成合并FLV(DownIofo.继承.继承的下载文件路径, DownIofo.文件保存路径, true);
                                            if (!string.IsNullOrEmpty(DownIofo.继承.合并后的文件路径))
                                            {
                                                DownIofo.文件保存路径 = DownIofo.继承.合并后的文件路径;
                                            }
                                        }
                                        else if(!DownIofo.是否是播放任务)
                                        {
                                            FlvMethod.转码(DownIofo.文件保存路径);
                                        }
                                        
                                        InfoLog.InfoPrintf("下载任务结束\n==============下载任务结束================\n主播名:" + DownIofo.主播名称 + "\n房间号:" + DownIofo.房间_频道号 + "\n标题:" + DownIofo.标题 + "\n开播时间:" + DownIofo.开始时间 + "\n结束时间:" + DownIofo.结束时间 + "\n保存路径:" + DownIofo.文件保存路径 + "\n下载任务类型:" + (DownIofo.继承.是否为继承对象 ? "续下任务" : "新建下载任务") + "\n结束原因:" + DownIofo.备注 + "\n===============下载任务结束===============\n", InfoLog.InfoClass.下载必要提示);
                                        下载对象.DownIofo.下载状态 = false;
                                        下载对象.DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                        DownIofo.下载状态 = false;
                                        DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                        return;
                                    }
                                    new Task((() => 
                                    {
                                        while (true)
                                        {
                                            Thread.Sleep(1000);
                                            if (下载对象.DownIofo.已下载大小bit > 1000)
                                            {
                                                DownIofo.下载状态 = true;
                                                return;
                                            }
                                            if (!bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号))
                                            {
                                                InfoLog.InfoPrintf("下载任务结束\n==============下载任务结束================\n主播名:" + DownIofo.主播名称 + "\n房间号:" + DownIofo.房间_频道号 + "\n标题:" + DownIofo.标题 + "\n开播时间:" + DownIofo.开始时间 + "\n结束时间:" + DownIofo.结束时间 + "\n保存路径:" + DownIofo.文件保存路径 + "\n下载任务类型:" + (DownIofo.继承.是否为继承对象 ? "续下任务" : "新建下载任务") + "\n结束原因:" + DownIofo.备注 + "\n===============下载任务结束===============\n", InfoLog.InfoClass.下载必要提示);
                                                下载对象.DownIofo.备注 = "停止直播";
                                                下载对象.DownIofo.下载状态 = false;
                                                下载对象.DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                DownIofo.下载状态 = false;
                                                DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                下载对象.DownIofo.WC.CancelAsync();
                                                MMPU.DownList.Remove(下载对象);
                                                return;
                                            }
                                        }
                                    })).Start();
                                    break;
                                }
                            //case "youtube":
                            //    {
                            //        break;
                            //    }
                            //case "T台":
                            //    {
                            //        break;
                            //    }
                            //case "FC2":
                            //    {
                            //        break;
                            //    }
                            //case "DDTV直播服务器":
                            //    { 
                            //        break;
                            //    }
                            default:
                                DownIofo.下载状态 = false;
                                DownIofo.结束时间 = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                InfoLog.InfoPrintf("该房间的配置文件发现了与当前版本不支持的平台，请检查文件配置或者检查更新", InfoLog.InfoClass.系统错误信息);
                                return;
                        }
                    }
                }
            })).Start();
        }
        public static string 下载完成合并FLV(string File1,string File2,bool 是否直播结束)
        {
            FlvMethod.Flv A = new FlvMethod.Flv()
            {
                File1Url = File1,
                File2Url = File2
            };
            return FlvMethod.FlvSum(A, 是否直播结束);
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
