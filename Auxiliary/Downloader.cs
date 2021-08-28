using Auxiliary.LiveChatScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        /// <summary>
        /// 关闭直播流和弹幕储存流
        /// </summary>
        public static void Clear(bool 续命模式, DownIofoData DOL)
        {
            //DOL.备注 = "下载任务结束";
            DOL.下载状态 = false;
            if(!续命模式)
            {
                try
                {
                    DOL.阿B直播流对象.Dispose();
                    InfoLog.InfoPrintf($"{DOL.房间_频道号}房间直播流对象回收完成", InfoLog.InfoClass.Debug);
                }
                catch (Exception) { }
                try
                {
                    string danmuFile = ((System.IO.FileStream)DOL.弹幕储存流.BaseStream).Name;
                    DOL.弹幕储存流.Close();
                    DOL.弹幕储存流.Dispose();
                    if (Path.GetFileNameWithoutExtension(danmuFile) != Path.GetFileNameWithoutExtension(DOL.文件保存路径))
                    {
                        File.Move(danmuFile, DOL.文件保存路径.Substring(0, DOL.文件保存路径.Length - 4) + (MMPU.弹幕录制种类 == 1 ? ".ass" : ".xml"));
                        MMPU.文件删除委托(danmuFile, "弹幕文件重命名");
                    }
                    InfoLog.InfoPrintf($"{DOL.房间_频道号}房间弹幕储存流对象回收完成", InfoLog.InfoClass.Debug);
                }
                catch (Exception) { }
                try
                {
                    string giftFile = ((System.IO.FileStream)DOL.礼物储存流.BaseStream).Name;
                    DOL.礼物储存流.Close();
                    DOL.礼物储存流.Dispose();
                    if (giftFile.Substring(0, giftFile.Length - 4) != DOL.文件保存路径)
                    {
                        File.Move(giftFile, DOL.文件保存路径 + ".txt");
                        MMPU.文件删除委托(giftFile, "礼物文件重命名");
                    }
                    InfoLog.InfoPrintf($"{DOL.房间_频道号}房间礼物储存流对象回收完成", InfoLog.InfoClass.Debug);
                }
                catch (Exception) { }
            }
            try
            {
                DOL.WC.Dispose();
                InfoLog.InfoPrintf($"{DOL.房间_频道号}房间WebClient对象回收完成", InfoLog.InfoClass.Debug);
            }
            catch (Exception) { }
        }
        public class DownIofoData
        {
            public LiveChatListener 阿B直播流对象 = new LiveChatListener();
            public StreamWriter 弹幕储存流;
            public StreamWriter 礼物储存流;
            public DateTime 弹幕录制基准时间 = DateTime.Now;

            public WebClient WC { set; get; }
            public bool 下载状态 { set; get; } = false;
            public int 最后连接时间 { set; get; } = 0;
            public bool 网络超时 { set; get; } = false;
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
            public int 转码进度 { set; get; } = -1;
            public bool 是否转码中 { set; get; } = false;
        }
        public class 继承
        {
            public bool 是否为继承对象 { set; get; } = false;
            public List<string> 待合并文件列表 { set; get; } = new List<string>();
            public string 继承的下载文件路径 { set; get; } = null;
            public string 合并后的文件路径 { set; get; } = null;
        }
        public static bool 轮询检查下载任务开关 = true;
        public static void 轮询检查下载任务()
        {
            new Task(()=> {
                while (true)
                {
                    while (轮询检查下载任务开关)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(MMPU.TcpSend(Server.RequestCode.GET_IP, "{}", true, 50, 5000)))
                            {
                                下载任务状态检测();
                            }
                            else
                            {

                            }
                        }
                        catch (Exception)
                        { }
                        Thread.Sleep(30 * 1000);
                    }
                    Thread.Sleep(500);
                }
            }).Start();
        }
        public static void 下载任务状态检测()
        {
            foreach (var item in MMPU.DownList)
            {
                if(!item.DownIofo.网络超时&& item.DownIofo.下载状态&&item.DownIofo.已下载大小bit>10000&& item.DownIofo.最后连接时间!=0&& Gettime()-item.DownIofo.最后连接时间>60)
                {
                    InfoLog.InfoPrintf(item.DownIofo.房间_频道号 + "下载状态异常，重置下载任务", InfoLog.InfoClass.下载系统信息);
                    item.DownIofo.网络超时 = true;
                    item.DownIofo.备注 = "下载状态异常，重置下载任务";
                    Clear(true, item.DownIofo);
                    item.DownIofo.下载状态 = false;
                    item.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                    item.DownIofo.WC.CancelAsync();
                    new Task(() =>
                    {
                        string 标题 = bilibili.根据房间号获取房间信息.获取标题(item.DownIofo.标题);
                        if(string.IsNullOrEmpty(标题))
                        {
                            标题 = $"重置下载任务{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{new Random().Next(10000,99999)}";
                        }
                        Downloader DLL = Downloader.新建下载对象(item.DownIofo.平台, item.DownIofo.房间_频道号, bilibili.根据房间号获取房间信息.获取标题(item.DownIofo.房间_频道号), Guid.NewGuid().ToString(), bilibili.根据房间号获取房间信息.下载地址(item.DownIofo.房间_频道号), "下载状态异常，重置下载任务", item.DownIofo.是否保存, item.DownIofo.主播名称, false, null);
                    }).Start();

                    Upload.Upload uploadTask = new Upload.Upload(item.DownIofo);
                    uploadTask.upload();
                }
            }
        }


        public string Start(string 开始后显示的备注)
        {
            DownIofo.开始时间 = Gettime();
            MMPU.DownList.Add(this);
            int a = 0;
            DownIofo.WC = new WebClient();
            DownIofo.WC.Encoding = Encoding.UTF8;
            DownIofo.WC.Headers.Add("Accept: */*");
            DownIofo.WC.Headers.Add("User-Agent: " + MMPU.UA.Ver.UA());
            DownIofo.WC.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            DownIofo.WC.DownloadFileCompleted += 下载完成事件;
            DownIofo.WC.DownloadProgressChanged += 下载过程中事件;
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
            DownIofo.下载状态 = true;
            MMPU.判断网络路径是否存在 判断文件是否存在 = new MMPU.判断网络路径是否存在();
            while (true)
            {
                Thread.Sleep(5000);
                switch (DownIofo.平台)
                {
                    case "bilibili":
                        if (!bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号,true))
                        {
                            InfoLog.InfoPrintf(DownIofo.房间_频道号 + "房间:" + DownIofo.主播名称 + " 房间直播状态为False,取消建立新的下载任务", InfoLog.InfoClass.下载系统信息);
                            DownIofo.下载状态 = false;
                            DownIofo.备注 = "该房间未直播";
                            
                            DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                            MMPU.DownList.Remove(this);
                            return null;
                        }
                        break;
                    case "主站视频":
                        break;
                    default:
                        DownIofo.下载状态 = false;
                        DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                        DownIofo.备注 = "不支持的平台";
                        MMPU.DownList.Remove(this);
                        return null;
                }

                if (判断文件是否存在.判断(DownIofo.下载地址, DownIofo.平台, DownIofo.房间_频道号))
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
                                if (bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号,true))
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
                                        else
                                        {
                                            DownIofo.备注 = "等待房间推流...";
                                            Thread.Sleep(30000);
                                            //return null;
                                        }
                                    }
                                }
                                else
                                {
                                    InfoLog.InfoPrintf(DownIofo.房间_频道号 + "房间:" + DownIofo.主播名称 + " 房间未直播，下载任务取消", InfoLog.InfoClass.下载系统信息);
                                    DownIofo.下载状态 = false;
                                    DownIofo.备注 = "该房间未直播";
                                    DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                    MMPU.DownList.Remove(this);
                                    if (DownIofo.继承.是否为继承对象)
                                    {
                                        //MMPU.弹窗.Add(3000, "重连任务取消", DownIofo.房间_频道号 + "，该房间未直播");
                                    }
                                    else
                                    {
                                        MMPU.弹窗.Add(3000, "下载任务取消", DownIofo.房间_频道号 + "，该房间未直播");
                                    }

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
            DownIofo.开始时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            try
            {
                DownIofo.WC.DownloadFileTaskAsync(new Uri(DownIofo.下载地址), DownIofo.文件保存路径);
                InfoLog.InfoPrintf(DownIofo.主播名称 +
                    "开始直播，建立下载任务\r\n" +
                    "==============建立下载任务================\r\n" +
                    "主播名:" + DownIofo.主播名称 + "\r\n" +
                    "房间号:" + DownIofo.房间_频道号 + "\r\n" +
                    "标题:" + DownIofo.标题 + "\r\n" +
                    "开播时间:" + MMPU.Unix转换为DateTime(DownIofo.开始时间.ToString()) + "\r\n" +
                    "保存路径:" + DownIofo.文件保存路径 + "\r\n" +
                    "下载任务类型:" + (DownIofo.继承.是否为继承对象 ? "续下任务" : "新建下载任务") +
                    "\r\n===============建立下载任务===============\r\n",
                    InfoLog.InfoClass.下载系统信息);
                Webhook.Webhook.开播hook(new Webhook.Webhook.开播Info() 
                {
                    GUID=DownIofo.事件GUID,
                    Name=DownIofo.主播名称,
                    RoomId=DownIofo.房间_频道号,
                    StartTime = MMPU.Unix转换为DateTime(DownIofo.开始时间.ToString()),
                    TaskType= DownIofo.继承.是否为继承对象 ? "续下任务" : "新建下载任务",
                    Title=DownIofo.标题
                });
                if (MMPU.录制弹幕 && !DownIofo.继承.是否为继承对象)
                {
                    DownIofo.弹幕储存流 = new StreamWriter(DownIofo.文件保存路径.Substring(0, DownIofo.文件保存路径.Length-4) + (MMPU.弹幕录制种类 == 1 ? ".ass" : ".xml"));
                    DownIofo.礼物储存流 = new StreamWriter(DownIofo.文件保存路径 + ".txt");
                    DownIofo.阿B直播流对象.Connect(int.Parse(DownIofo.房间_频道号));
                    DownIofo.阿B直播流对象.MessageReceived += Listener_MessageReceived;
                    DownIofo.弹幕储存流.WriteLine(danmu.返回ASS字幕文件头(DownIofo.标题, DownIofo.房间_频道号, DownIofo.主播名称));
                }
            }
            catch (WebException) {
                DownIofo.备注 = "主播未推流，已结束直播";
                DownIofo.下载状态 = false;
                return null;
            }
            DownIofo.备注 = 开始后显示的备注;
            DownIofo.下载状态 = true;

            return DownIofo.文件保存路径;
        }
        private void Listener_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e)
                {
                    case DanmuMessageEventArgs danmu:
                        DateTime DT = DateTime.Now;
                        TimeSpan interval = DT - DownIofo.弹幕录制基准时间;
                        switch (MMPU.弹幕录制种类)
                        {
                            case 1:
                                {
                                    DownIofo.弹幕储存流.WriteLine("Dialogue: 0,{0},{1},Fix,{2},20,20,2,,{3}", interval.ToString(), (interval.Seconds).ToString(), danmu.UserName + "[" + danmu.UserId + "]", danmu.Message);
                                    break;
                                }
                            case 2:
                                {
                                    string 弹幕 = danmu.Message.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("&", "&amp;");
                                    弹幕 = Regex.Replace(弹幕, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");
                                    DownIofo.弹幕储存流.WriteLine($"<d p=\"{interval.TotalSeconds},{danmu.MessageType},{danmu.MessageFontSize},16777215,{MMPU.获取时间戳()},0,{danmu.UserId},0\">" +
                                        $"{弹幕}</d>");
                                    break;
                                }
                        }
                        DownIofo.弹幕储存流.Flush();//写入弹幕数据
                        break;
                    /*礼物流格式
                     * timestamp|uid|username|type|content|amount
                     */
                    case SendGiftEventArgs gift:
                        DownIofo.礼物储存流.WriteLine($"{gift.Timestamp}|{gift.UserId}|{gift.UserName}|礼物|{gift.GiftName}|{gift.Amount}");
                        DownIofo.礼物储存流.Flush();//写入礼物数据
                        break;
                    case GuardBuyEventArgs guard:
                        string content = guard.GuardLevel == 3 ? "舰长" : guard.GuardLevel == 2 ? "提督" : "总督";
                        DownIofo.礼物储存流.WriteLine($"{guard.Timestamp}|{guard.UserId}|{guard.UserName}|舰队|{content}|{guard.Number}");
                        DownIofo.礼物储存流.Flush();//写入舰队数据
                        break;
                    case WarningEventArg Warning:
                        DownIofo.礼物储存流.WriteLine($"{MMPU.获取时间戳()}|-1|超管|警告|{Warning.msg}|1");
                        DownIofo.礼物储存流.Flush();//写入超管警告内容
                        break;
                    case SuperchatEventArg sc:
                        DownIofo.礼物储存流.WriteLine($"{sc.timestamp}|{sc.userId}|{sc.userName}|醒目留言|{sc.message}|{sc.timeLength}");
                        DownIofo.礼物储存流.Flush();//写入sc
                        break;

                }
            }
            catch (Exception)
            {
            }
        }
        public static Downloader 新建下载对象(string 平台, string 唯一码, string 标题, string GUID, string 下载地址, string 备注, bool 是否保存, string 主播名称, bool 是否为继承项目, string 继承项目的原始文件,DownIofoData 继承的项目 = null)
        {
            foreach (var item in MMPU.DownList)
            {
                if (item.DownIofo.房间_频道号 == 唯一码)
                {
                    if (item.DownIofo.下载状态 && item.DownIofo.是否保存 && 是否保存)
                    {
                        InfoLog.InfoPrintf($"新建任务冲突，放弃新建任务，任务内容:\r\n房间号:{唯一码}\r\n主播名称:{主播名称}\r\n标题:{标题}", InfoLog.InfoClass.下载系统信息);
                        return null;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if(string.IsNullOrEmpty(标题))
            {
                标题 = $"V{new Random().Next(10000,99999)}";
            }
            Downloader 下载对象 = new Downloader();
            下载对象.DownIofo.继承 = new 继承();
            string 缓存路径 = MMPU.缓存路径;
            string 保存路径;
            if (MMPU.下载储存目录 == 缓存路径)
            {
                保存路径 = 缓存路径 +  唯一码 + "_" + 主播名称 + "_" + 平台 + "/";
                if (!Directory.Exists(保存路径))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory(保存路径);
                }
                保存路径 = 保存路径 + MMPU.文件名格式.Replace("{date}", DateTime.Now.ToString("yyyy_MM_dd")).Replace("{title}", 标题).Replace("{time}", DateTime.Now.ToString("HH_mm_ss")) + "_" + new Random().Next(1000, 9999) + ".flv";

            }
            else
            {
                保存路径 = MMPU.下载储存目录 + "/" + 唯一码 + "_" + 主播名称 + "_" + 平台 + "/";
                if (!Directory.Exists(保存路径))//如果不存在就创建file文件夹
                {
                    try
                    {
                        Directory.CreateDirectory(保存路径);
                    }
                    catch (Exception)
                    {
                        MMPU.下载储存目录 = 缓存路径;
                        MMPU.setFiles("file", MMPU.下载储存目录);
                        保存路径 = MMPU.下载储存目录 + "/" + 唯一码 + "_" + 主播名称 + "_" + 平台 + "/";
                    }

                }
                保存路径 = 保存路径 + MMPU.文件名格式.Replace("{date}", DateTime.Now.ToString("yyyy_MM_dd")).Replace("{title}", 标题).Replace("{time}", DateTime.Now.ToString("HH_mm_ss")) + "_" + new Random().Next(1000, 9999) + ".flv";
                if (File.Exists(保存路径))
                {
                    Thread.Sleep(1);
                    保存路径 = 保存路径 + MMPU.文件名格式.Replace("{date}", DateTime.Now.ToString("yyyy_MM_dd")).Replace("{title}", 标题).Replace("{time}", DateTime.Now.ToString("HH_mm_ss")) + "_" + new Random().Next(1000, 9999) + ".flv";
                }
            }
            switch (平台)
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
                下载状态 = true,
                房间_频道号 = 唯一码,
                文件保存路径 = 保存路径,
                事件GUID = GUID,
                下载地址 = 下载地址,
                是否保存 = 是否保存,
                标题 = 标题,
                备注 = 备注,
                主播名称 = 主播名称,
                继承 = new 继承()
                {
                    是否为继承对象 = 是否为继承项目,
                    继承的下载文件路径 = 继承项目的原始文件,
                }
            };
            if(继承的项目!=null)
            {
                下载对象.DownIofo.继承.待合并文件列表 = 继承的项目.继承.待合并文件列表;
                下载对象.DownIofo.弹幕储存流 = 继承的项目.弹幕储存流;
                下载对象.DownIofo.弹幕录制基准时间 = 继承的项目.弹幕录制基准时间;
                下载对象.DownIofo.礼物储存流 = 继承的项目.礼物储存流;
            }
            if (!是否保存)
            {
                int 随机值 = new Random().Next(1000, 9999);
                下载对象.DownIofo.文件保存路径 = 缓存路径 + "LiveCache/" + 下载对象.DownIofo.标题 + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + 随机值 + ".flv";
                if (File.Exists(下载对象.DownIofo.文件保存路径))
                {
                    下载对象.DownIofo.文件保存路径 = 缓存路径 + "LiveCache/" + 下载对象.DownIofo.标题 + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + (随机值) + 1 + ".flv";
                }
            }

            下载对象.Start(备注);

            return 下载对象;
        }
        public static int Gettime()
        {
            int time = (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            return time;
        }
        private void 下载过程中事件(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytes = e.BytesReceived;
            DownIofo.最后连接时间 = Gettime();
            DownIofo.已下载大小bit = bytes;
            DownIofo.已下载大小str = 转换下载大小数据格式(bytes);
            //e.UserState
        }
        // public event EventHandler<EventArgs> DownUpdate;
        // public event EventHandler<EventArgs> DownOk;

        private void 下载完成事件(object sender, AsyncCompletedEventArgs e)
        {
            new Task((() =>
            {
                try
                {
                    DownIofo.下载状态 = false;
                    DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                    DownIofo.备注 = "下载任务结束";                 
                    if (e.Cancelled&&!DownIofo.网络超时)
                    {
                        if (!DownIofo.播放状态 && DownIofo.是否是播放任务)
                        {
                            DownIofo.备注 = "播放窗口关闭";           
                            DownIofo.下载状态 = false;
                            下载结束提醒(true, "下载任务结束",DownIofo);
                            new Upload.Upload(DownIofo).upload();
                            return;
                        }
                        DownIofo.继承.待合并文件列表.Add(DownIofo.文件保存路径);
                        DownIofo.备注 = "用户取消，停止下载";
                        DownIofo.下载状态 = false;
                        下载结束提醒(true, "下载任务结束", DownIofo);
                        new Upload.Upload(DownIofo).upload();
                    }
                    else if (!e.Cancelled&& !bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号,true))
                    {
                        DownIofo.继承.待合并文件列表.Add(DownIofo.文件保存路径);
                        DownIofo.下载状态 = false;
                        DownIofo.备注 = "下载完成,直播间已关闭";             
                        if (DownIofo.继承 == null)
                        {
                            DownIofo.继承.是否为继承对象 = false;
                        }
                        if (DownIofo.继承.是否为继承对象 && !DownIofo.是否是播放任务)
                        {
                            DownIofo.继承.合并后的文件路径 = 下载完成合并FLV(DownIofo, true);
                            if (!string.IsNullOrEmpty(DownIofo.继承.合并后的文件路径))
                            {
                                DownIofo.文件保存路径 = DownIofo.继承.合并后的文件路径;
                            }
                        }
                        if (!DownIofo.是否是播放任务)
                        {
                            FlvMethod.转码(DownIofo.文件保存路径, DownIofo);
                        }
                        InfoLog.InfoPrintf(DownIofo.房间_频道号 + "房间:" + DownIofo.主播名称 + " 下播，录制完成", InfoLog.InfoClass.下载系统信息);
                        foreach (var item in RoomInit.bilibili房间主表)
                        {
                            if (item.唯一码 == DownIofo.房间_频道号)
                            {
                                item.直播状态 = false;
                                break;
                            }
                        }
                        DownIofo.下载状态 = false;
                        下载结束提醒(true, "下载任务结束", DownIofo);
                        Upload.Upload uploadTask = new Upload.Upload(DownIofo);
                        uploadTask.upload();

                        return;
                    }
                    else
                    {
                        if (bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号,true) && DownIofo.是否保存)
                        {
                            DownIofo.网络超时 = true;
                            DownIofo.下载状态 = false;
                            DownIofo.备注 = "下载流中断，检测到房间仍为开播状态，新建续下任务。";
                            DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                            switch (DownIofo.平台)
                            {
                                case "bilibili":
                                    {
                                        if (!DownIofo.是否是播放任务)
                                        {
                                            DownIofo.继承.待合并文件列表.Add(DownIofo.文件保存路径);
                                            InfoLog.InfoPrintf($"{DownIofo.房间_频道号}:{DownIofo.主播名称}下载任务续下，历史文件加入待合并文件列表:{DownIofo.文件保存路径}", InfoLog.InfoClass.下载系统信息);
                                        }
                                        DownIofo.下载状态 = false;
                                       Downloader 重连下载对象 = Downloader.新建下载对象(
                                            DownIofo.平台,
                                            DownIofo.房间_频道号,
                                            bilibili.根据房间号获取房间信息.获取标题(DownIofo.房间_频道号),
                                            Guid.NewGuid().ToString(),
                                            bilibili.根据房间号获取房间信息.下载地址(DownIofo.房间_频道号),
                                            "重连",
                                            DownIofo.是否保存,
                                            DownIofo.主播名称,
                                            true,
                                            DownIofo.文件保存路径,
                                            DownIofo
                                            );
                                        if (!重连下载对象.DownIofo.下载状态)
                                        {
                                            try
                                            {
                                                重连下载对象.DownIofo.弹幕录制基准时间 = DownIofo.弹幕录制基准时间;
                                                重连下载对象.DownIofo.阿B直播流对象 = DownIofo.阿B直播流对象;
                                                重连下载对象.DownIofo.弹幕储存流 = DownIofo.弹幕储存流;
                                                重连下载对象.DownIofo.礼物储存流 = DownIofo.礼物储存流;
                                                重连下载对象.DownIofo.下载状态 = false;
                                                重连下载对象.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                重连下载对象.DownIofo.备注 = "服务器主动断开连接，直播结束";
                                                foreach (var item in RoomInit.bilibili房间主表)
                                                {
                                                    if (item.唯一码 == DownIofo.房间_频道号)
                                                    {
                                                        item.直播状态 = false;
                                                        break;
                                                    }
                                                }
                                                if (DownIofo.继承.是否为继承对象 && !DownIofo.是否是播放任务)
                                                {
                                                    DownIofo.继承.合并后的文件路径 = 下载完成合并FLV(DownIofo, true);
                                                    if (!string.IsNullOrEmpty(DownIofo.继承.合并后的文件路径))
                                                    {
                                                        DownIofo.文件保存路径 = DownIofo.继承.合并后的文件路径;
                                                    }
                                                }
                                                if (!DownIofo.是否是播放任务)
                                                {
                                                    FlvMethod.转码(DownIofo.文件保存路径, DownIofo);
                                                }
                                                DownIofo.备注 = "服务器主动断开连接，直播结束";
                                                重连下载对象.DownIofo.下载状态 = false;
                                                重连下载对象.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                DownIofo.下载状态 = false;
                                                DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                下载结束提醒(true, "下载任务结束", 重连下载对象.DownIofo);
                                                Upload.Upload uploadTask = new Upload.Upload(DownIofo);
                                                uploadTask.upload();
                                            }
                                            catch (Exception){}
                                            return;
                                        }
                                        new Task((() =>
                                        {
                                            while (true)
                                            {
                                                Thread.Sleep(10000);
                                                if (重连下载对象.DownIofo.已下载大小bit > 5000)
                                                {
                                                    DownIofo.下载状态 = false;
                                                    DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                    //NagisaCo: 被继承项目不关闭弹幕礼物流
                                                    //下载结束提醒(true, "下载任务结束", DownIofo);
                                                    下载结束提醒(false, "下载任务结束", DownIofo);
                                                    重连下载对象.DownIofo.备注 = "完成重连，正在续命..";
                                                    //下载对象.DownIofo.下载状态 = true;
                                                    return;
                                                }
                                                if (!bilibili.根据房间号获取房间信息.是否正在直播(DownIofo.房间_频道号,true))
                                                {
                                                    重连下载对象.DownIofo.备注 = "停止直播";
                                                    DownIofo.备注 = "直播停止，下载完成下载完成";    
                                                    重连下载对象.DownIofo.下载状态 = false;
                                                    重连下载对象.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                    DownIofo.下载状态 = false;
                                                    DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                                    重连下载对象.DownIofo.WC.CancelAsync();
                                                    MMPU.DownList.Remove(重连下载对象);
                                                    下载结束提醒(true, "下载任务结束", DownIofo);
                                                    下载结束提醒(true, "下载任务结束", 重连下载对象.DownIofo);
                                                    return;
                                                }
                                            }
                                        })).Start();
                                        DownIofo.下载状态 = false;
                                        break;
                                    }
                                default:
                                    DownIofo.备注 = "不受支持的平台";
                                    DownIofo.下载状态 = false;
                                    DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                                    InfoLog.InfoPrintf("该房间的配置文件发现了与当前版本不支持的平台，请检查文件配置或者检查更新", InfoLog.InfoClass.系统错误信息);
                                    //下载结束提醒(false);
                                    return;
                            }
                        }
                        else
                        {
                            DownIofo.备注 = "直播停止，下载完成下载完成";
                            下载结束提醒(true, "下载任务结束", DownIofo);
                            DownIofo.下载状态 = false;
                            return;
                        }
                    }
                    DownIofo.下载状态 = false;
                    DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                    DownIofo.备注 = "下载任务结束";
                }
                catch (Exception ES)
                {
                    InfoLog.InfoPrintf($"录制任务意外终止:\r\n{ES.ToString()}", InfoLog.InfoClass.系统错误信息);
                    DownIofo.下载状态 = false;
                    DownIofo.备注 = "录制任务意外终止，已新建续命任务";
                    下载结束提醒(true, "录制任务意外终止，已新建续命任务", DownIofo);
                    Downloader 下载对象 = new Downloader();
                    try
                    {
                        DownIofo.下载状态 = false;
                        下载对象 = Downloader.新建下载对象(
                            DownIofo.平台,
                            DownIofo.房间_频道号,
                            bilibili.根据房间号获取房间信息.获取标题(DownIofo.房间_频道号),
                            Guid.NewGuid().ToString(),
                            bilibili.根据房间号获取房间信息.下载地址(DownIofo.房间_频道号),
                            "前一个下载出现异常，新建下载",
                            DownIofo.是否保存,
                            DownIofo.主播名称,
                            false,
                            DownIofo.文件保存路径
                            );
                    }
                    catch (Exception)
                    {
                        try
                        {
                            下载对象.DownIofo.备注 = "新建续下载对象出现异常，放弃新建任务";
                            下载结束提醒(true,"下载任务结束", DownIofo);
                            下载对象.DownIofo.下载状态 = false;
                            下载对象.DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                            下载对象.DownIofo.WC.CancelAsync();
                            MMPU.DownList.Remove(下载对象);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            })).Start();
        }
        public void 下载结束提醒( bool 是否结束弹幕录制, string 提醒标题, DownIofoData DOL)
        {
            try
            {
                if (是否结束弹幕录制 && MMPU.录制弹幕 && MMPU.弹幕录制种类 == 2)
                {
                    try
                    {
                        DOL.弹幕储存流.WriteLine("</i>");        
                        DOL.弹幕储存流.Flush();//写入弹幕数据
                    }
                    catch (Exception)
                    { }
                    if (DOL != DownIofo)
                    {
                        Clear(false, DownIofo);
                    }
                    Clear(false, DOL); 
                }
               else
                {
                    Clear(true, DOL);
                }
            }
            catch (Exception) { }
            InfoLog.InfoPrintf($"\r\n=============={提醒标题}================\r\n" +
                               $"主播名:{DOL.主播名称}" +
                               $"\r\n房间号:{DOL.房间_频道号}" +
                               $"\r\n标题:{DOL.标题}" +
                               $"\r\n开播时间:{MMPU.Unix转换为DateTime(DOL.开始时间.ToString())}" +
                               $"\r\n结束时间:{MMPU.Unix转换为DateTime(DOL.结束时间.ToString())}" +
                               $"\r\n保存路径:{DOL.文件保存路径}" +
                               $"\r\n下载任务类型:{(DOL.继承.是否为继承对象 ? "续下任务" : "新建下载任务")}" +
                               $"\r\n结束原因:{DOL.备注}" +
                               $"\r\n==============={提醒标题}===============\r\n", InfoLog.InfoClass.下载系统信息);
            Webhook.Webhook.下播hook(new Webhook.Webhook.下播Info()
            {
                GUID = DOL.事件GUID,
                Name = DOL.主播名称,
                RoomId = DOL.房间_频道号,
                StartTime = MMPU.Unix转换为DateTime(DOL.开始时间.ToString()),
                EndTime= MMPU.Unix转换为DateTime(DOL.结束时间.ToString()),
                TaskType = DOL.继承.是否为继承对象 ? "续下任务" : "新建下载任务",
                Title = DOL.标题,
                Reason= DOL.备注,
                SavePath= DOL.文件保存路径
            });
        }
        public static string 下载完成合并FLV(DownIofoData downIofo, bool 是否直播结束)
        {
            string filename = string.Empty;
            List<string> DelFileList = new List<string>();
            if (downIofo.继承.待合并文件列表.Count>1)
            {
                filename = downIofo.继承.待合并文件列表[0];
               
                for (int i = 0; i< downIofo.继承.待合并文件列表.Count-1; i++)
                {
                    DelFileList.Add(downIofo.继承.待合并文件列表[i + 1]);
                    FlvMethod.Flv A = new FlvMethod.Flv()
                    {
                        File1Url = filename,
                        File2Url = downIofo.继承.待合并文件列表[i+1]
                    };
                    DelFileList.Add(filename);
                    string BB = FlvMethod.FlvSum(A, 是否直播结束);
                    if(string.IsNullOrEmpty(BB))
                    {
                        InfoLog.InfoPrintf($"{downIofo.房间_频道号}:{downIofo.主播名称}因为网络连接不稳定，无法获取文件头，放弃合并该flv",InfoLog.InfoClass.下载系统信息);
                        return filename;
                    }
                    filename = BB;
                }
            }
            foreach (var item in DelFileList)
            {
                MMPU.文件删除委托(item, "FLV合并任务");
            }
            return filename;
        }
        public static string 转换下载大小数据格式(double size)
        {
            if (size < 0) {
                return "未知";
            }
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
