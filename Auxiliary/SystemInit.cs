using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Auxiliary.bilibili;

namespace Auxiliary
{
    public class SystemInit
    {
        public class DDTV_Core初始化
        {
            public static void 核心配置初始化()
            {
                MMPU.下载储存目录 = MMPU.读取exe默认配置文件("file", "./tmp/");
                MMPU.CheckPath(ref MMPU.下载储存目录);
                if (!Directory.Exists("./tmp"))
                {
                    Directory.CreateDirectory("./tmp");
                }
                InfoLog.InfoPrintf($"配置文件初始化任务[下载储存目录]:{MMPU.下载储存目录}", InfoLog.InfoClass.Debug);
            }      
            public static void 上传系统初始化()
            {
                Upload.Uploader.InitUpload();//初始化上传配置
            }
            public static void 消息系统初始化(int 模式)
            {
                if (模式 == 0)
                {
                    InfoLog.InfoInit("DDTVLog.out", new InfoLog.InfoClasslBool()
                    {
                        Debug = MMPU.Debug模式,
                        下载必要提示 = true,
                        杂项提示 = false,
                        系统错误信息 = true,
                        是否将日志输出到文件 = MMPU.Debug输出到文件,
                        上传必要提示 = true
                    });
                    MMPU.启动模式 = 0;
                }
                else if (模式 == 1)
                {
                    InfoLog.InfoInit("DDTVLiveRecLog.out", new InfoLog.InfoClasslBool()
                    {
                        Debug = MMPU.Debug模式,
                        下载必要提示 = true,
                        杂项提示 = false,
                        系统错误信息 = true,
                        是否将日志输出到文件 = MMPU.Debug输出到文件,
                        上传必要提示 = true
                    });
                    MMPU.启动模式 = 1;
                }
                InfoLog.InfoPrintf("消息系统初始化完成", InfoLog.InfoClass.Debug);

                MMPU.心跳打印间隔 = int.Parse(MMPU.读取exe默认配置文件("DokiDoki", "300"));
                InfoLog.InfoPrintf($"配置文件初始化任务[心跳打印间隔]:{MMPU.心跳打印间隔}", InfoLog.InfoClass.Debug);
                MMPU.网络环境变动监听 = MMPU.读取exe默认配置文件("NetStatusMonitor", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[网络环境变动监听]:{MMPU.网络环境变动监听}", InfoLog.InfoClass.Debug);
            }
            public static void Debug系统初始化()
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句
                MMPU.Debug模式 = MMPU.读取exe默认配置文件("DebugMod", "1") == "0" ? false : true;
                MMPU.Debug输出到文件 = MMPU.读取exe默认配置文件("DebugFile", "1") == "0" ? false : true;
                MMPU.Debug打印到终端 = MMPU.读取exe默认配置文件("DebugCmd", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[Debug模式]:{MMPU.Debug模式}", InfoLog.InfoClass.Debug);
                InfoLog.InfoPrintf($"配置文件初始化任务[Debug输出到文件]:{MMPU.Debug输出到文件}", InfoLog.InfoClass.Debug);
                InfoLog.InfoPrintf($"配置文件初始化任务[Debug打印到终端]:{MMPU.Debug打印到终端}", InfoLog.InfoClass.Debug);
            }
            public static void 录制系统初始化()
            {
                //数据源
                MMPU.数据源 = int.Parse(MMPU.读取exe默认配置文件("DataSource", "0"));
                InfoLog.InfoPrintf($"配置文件初始化任务[数据源]:{ MMPU.数据源}", InfoLog.InfoClass.Debug);
                //是否启动WS连接组
                bilibili.是否启动WSS连接组 = MMPU.读取exe默认配置文件("NotVTBStatus", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[是否启动WSS连接组]:{ bilibili.是否启动WSS连接组}", InfoLog.InfoClass.Debug);
                //转码功能使能和转码后删除文件
                MMPU.转码功能使能 = MMPU.读取exe默认配置文件("AutoTranscoding", "0") == "1" ? true : false;
                InfoLog.InfoPrintf($"配置文件初始化任务[转码功能使能]:{ MMPU.转码功能使能}", InfoLog.InfoClass.Debug);
                MMPU.转码后自动删除文件 = MMPU.读取exe默认配置文件("AutoTranscodingDelFile", "0") == "1" ? true : false;
                InfoLog.InfoPrintf($"配置文件初始化任务[转码后自动删除文件]:{ MMPU.转码后自动删除文件}", InfoLog.InfoClass.Debug);
                //检查配置文件
                bilibili.BiliUser.CheckPath(MMPU.BiliUserFile);
                //检查弹幕录制配置
                MMPU.录制弹幕 = MMPU.读取exe默认配置文件("RecordDanmu", "0") == "1" ? true : false;
                InfoLog.InfoPrintf($"配置文件初始化任务[录制弹幕]:{ MMPU.录制弹幕}", InfoLog.InfoClass.Debug);
                //房间配置文件
                RoomInit.RoomConfigFile = MMPU.读取exe默认配置文件("RoomConfiguration", "./RoomListConfig.json");
                InfoLog.InfoPrintf($"配置文件初始化任务[RoomConfigFile]:{RoomInit.RoomConfigFile}", InfoLog.InfoClass.Debug);

                //直播表刷新默认间隔
                MMPU.直播列表刷新间隔 = int.Parse(MMPU.读取exe默认配置文件("LiveListTime", "5"));
                InfoLog.InfoPrintf($"配置文件初始化任务[直播列表刷新间隔]:{ MMPU.直播列表刷新间隔}", InfoLog.InfoClass.Debug);


                //直播更新时间
                MMPU.直播更新时间 = int.Parse(MMPU.读取exe默认配置文件("RoomTime", "20"));
                InfoLog.InfoPrintf($"配置文件初始化任务[直播更新时间]:{ MMPU.直播更新时间}", InfoLog.InfoClass.Debug);
                if (MMPU.读取exe默认配置文件("DT1", "0") == "0" ? true : false)
                {
                    MMPU.录制弹幕 = false;
                    MMPU.setFiles("DT1", "1");
                }
                if (MMPU.读取exe默认配置文件("DT2", "0") == "0" ? true : false)
                {
                    MMPU.直播更新时间 = 20;
                    MMPU.setFiles("RoomTime", "20");
                    MMPU.setFiles("DT1", "1");
                }
            }
            public static void B站账号初始化(int 模式)
            {
                //账号登陆cookie
                try
                {
                    MMPU.Cookie = Encryption.UnAesStr(MMPU.读ini配置文件("User", "Cookie", MMPU.BiliUserFile), MMPU.AESKey, MMPU.AESVal);
                    InfoLog.InfoPrintf($"配置文件初始化任务[Cookie]敏感信息，隐藏内容，信息长度:{MMPU.Cookie.Length}", InfoLog.InfoClass.Debug);
                    if (!MMPU.Cookie.Contains("=") || !MMPU.Cookie.Contains(";"))
                    {
                        MMPU.Cookie = "";
                        MMPU.写ini配置文件("User", "Cookie", "", MMPU.BiliUserFile);
                        MMPU.csrf = null;
                        MMPU.写ini配置文件("User", "csrf", "", MMPU.BiliUserFile);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("读取cookie缓存错误");
                    MMPU.Cookie = "";
                    MMPU.写ini配置文件("User", "Cookie", "", MMPU.BiliUserFile);
                }
                //账号UID
                MMPU.UID = MMPU.读ini配置文件("User", "UID", MMPU.BiliUserFile); //string.IsNullOrEmpty(MMPU.读取exe默认配置文件("UID", "")) ? null : MMPU.读取exe默认配置文件("UID", "");
                InfoLog.InfoPrintf($"配置文件初始化任务[UID]敏感信息，隐藏内容，信息长度:{MMPU.UID.Length}", InfoLog.InfoClass.Debug);
                //账号登陆cookie的有效期
                try
                {
                    if (!string.IsNullOrEmpty(MMPU.读ini配置文件("User", "CookieEX", MMPU.BiliUserFile)))
                    {
                        MMPU.CookieEX = DateTime.Parse(MMPU.读ini配置文件("User", "CookieEX", MMPU.BiliUserFile));
                        InfoLog.InfoPrintf($"配置文件初始化任务[CookieEX]敏感信息，隐藏内容", InfoLog.InfoClass.Debug);
                        if (DateTime.Compare(DateTime.Now, MMPU.CookieEX) > 0)
                        {
                            MMPU.Cookie = "";

                            if (模式 == 0)
                            {
                                //MessageBox.Show("BILIBILI账号登陆已过期");
                                MMPU.写ini配置文件("User", "Cookie", "", MMPU.BiliUserFile);
                                MMPU.csrf = null;
                                MMPU.写ini配置文件("User", "csrf", "", MMPU.BiliUserFile);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    if (模式 == 0)
                    {
                        MMPU.写ini配置文件("User", "Cookie", "", MMPU.BiliUserFile);
                    }
                    MMPU.Cookie = null;
                }
                //账号csrf
                if (string.IsNullOrEmpty(MMPU.Cookie))
                {
                    InfoLog.InfoPrintf("\r\n===============================\r\nbilibili账号cookie为空或已过期，请更新BiliUser.ini信息\r\n===============================", InfoLog.InfoClass.下载系统信息);
                    //InfoLog.InfoPrintf("\r\n==============\r\nBiliUser.ini文件无效，请使用DDTV本体登陆成功后把DDTV本体里的BiliUser.ini文件覆盖无效的文件\r\n==============", InfoLog.InfoClass.下载必要提示);
                    if (模式 == 1)
                    {
                        //InfoLog.InfoPrintf("\r\n如果短信验证方式验证启动失败，请复制DDTV2本体中有效BiliUser.ini覆盖本地文件后重启DDTVLiveRec\r\n[======如果是非windows系统，请检查文件权限======]", InfoLog.InfoClass.下载必要提示);
                        try
                        {
                            BiliUser.登陆();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        while (string.IsNullOrEmpty(MMPU.Cookie))
                        {
                            string URL = "";
                            if (MMPU.是否启用SSL)
                            {
                                URL = "https://本设备IP或域名:" + MMPU.webServer默认监听端口 + "/loginqr";
                            }
                            else
                            {
                                URL = "http://本设备IP或域名:" + MMPU.webServer默认监听端口 + "/loginqr";
                            }
                            InfoLog.InfoPrintf("请用阿B手机客户端扫描[" + URL + "]进行登陆", InfoLog.InfoClass.系统错误信息);
                            //InfoLog.InfoPrintf("\r\n阿B登陆验证失败！！！请重启DDTVLiveRec进行登陆验证", InfoLog.InfoClass.下载必要提示);
                            Thread.Sleep(10000);
                        }
                    }
                }
                else
                {
                    if (模式 == 0)
                    {
                        if (!MMPU.加载网络房间方法.是否正在缓存)
                        {
                            new Task(() =>
                            {
                                MMPU.加载网络房间方法.更新网络房间缓存();
                            }).Start();
                        }
                    }
                }
                MMPU.csrf = MMPU.读ini配置文件("User", "csrf", MMPU.BiliUserFile);
                InfoLog.InfoPrintf($"配置文件初始化任务[csrf]敏感信息，隐藏内容，信息长度:{MMPU.csrf.Length}", InfoLog.InfoClass.Debug);
                InfoLog.InfoPrintf("Bilibili账号信息加载完成", InfoLog.InfoClass.Debug);
            }
            public static void 房间监控系统初始化()
            {
                try
                {
                    RoomInit.start();
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf($"房间配置文件加载过程中发生错误，文件格式不符合要求，请检查文件内容。错误堆栈:\n{e.ToString()}", InfoLog.InfoClass.系统错误信息);
                }
            }
        }
        public class Web系统初始化
        {
            public static void Web服务初始化()
            {
                MMPU.webadmin验证字符串 = MMPU.读取exe默认配置文件("WebAuthenticationAadminPassword", "admin");
                MMPU.webghost验证字符串 = MMPU.读取exe默认配置文件("WebAuthenticationGhostPasswrod", "ghost");
                MMPU.webghost验证字符串 = MMPU.读取exe默认配置文件("WebAuthenticationCode", "DDTVLiveRec");
                MMPU.webServer_pfx证书名称 = MMPU.读取exe默认配置文件("sslName", "");
                MMPU.webServer_pfx证书密码 = MMPU.读取exe默认配置文件("sslPssword", "");
                if (!string.IsNullOrEmpty(MMPU.webServer_pfx证书名称) && !string.IsNullOrEmpty(MMPU.webServer_pfx证书密码))
                {
                    MMPU.是否启用SSL = true;
                    InfoLog.InfoPrintf($"配置文件初始化任务[SSL证书初始化]:{MMPU.webServer_pfx证书名称}", InfoLog.InfoClass.Debug);
                    InfoLog.InfoPrintf($"======检测到SSL证书=======\r\n\r\n请使用[https://本设备IP或域名:" + MMPU.webServer默认监听端口 + "]进行访问\r\n\r\n======检测到SSL连接=======", InfoLog.InfoClass.下载系统信息);
                }
                else
                {
                    MMPU.是否启用SSL = false;
                    InfoLog.InfoPrintf($"配置文件初始化任务[SSL证书初始化]:证书不存在！或密码不存在，SSL证书加载失败", InfoLog.InfoClass.Debug);
                    InfoLog.InfoPrintf($"======未检测到SSL证书=======\r\n\r\n请使用[http://本设备IP或域名:" + MMPU.webServer默认监听端口 + "]进行访问\r\n\r\n======未检测到SSL连接=======", InfoLog.InfoClass.下载系统信息);
                }
                MMPU.ApiToken = MMPU.读取exe默认配置文件("ApiToken", "1145141919810A");
                MMPU.WebUserName = MMPU.读取exe默认配置文件("WebUserName", "ami");
                MMPU.WebPassword = MMPU.读取exe默认配置文件("WebPassword", "ddtv");
                
               
                MMPU.缓存路径 = MMPU.下载储存目录;
                MMPU.webServer默认监听IP = MMPU.读取exe默认配置文件("LiveRecWebServerDefaultIP", "0.0.0.0");
                InfoLog.InfoPrintf($"配置文件初始化任务[webServer默认监听IP]:{MMPU.webServer默认监听IP}", InfoLog.InfoClass.Debug);
                MMPU.webServer默认监听端口 = MMPU.读取exe默认配置文件("Port", "11419");
                InfoLog.InfoPrintf($"配置文件初始化任务[webServer默认监听端口]:{MMPU.webServer默认监听端口}", InfoLog.InfoClass.Debug);
                MMPU.webServer初始化状态 = true;
            }
            public static void WebHook初始化()
            {
                MMPU.WebhookUrl = MMPU.读取exe默认配置文件("WebhookUrl", "");
                MMPU.WebhookEnable = MMPU.读取exe默认配置文件("WebhookEnable", "0") == "0" ? false : true;
            }
            public static void WebSocket服务端初始化()
            {
                int P = 11451;
                string 端口 = MMPU.读取exe默认配置文件("WebSocketPort", "11451");
                MMPU.WebSocket端口 = int.TryParse(端口, out P) ? P : 11451;
                InfoLog.InfoPrintf($"配置文件初始化任务[WebSocket端口]:{MMPU.WebSocket端口}", InfoLog.InfoClass.Debug);
                MMPU.WebSocketEnable = MMPU.读取exe默认配置文件("WebSocketEnable", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[WebSocketEnable]:{MMPU.WebSocketEnable}", InfoLog.InfoClass.Debug);
                MMPU.WebSocketUserName = MMPU.读取exe默认配置文件("WebSocketUserName", "defaultUserName");
                InfoLog.InfoPrintf($"配置文件初始化任务[WebSocketUserName]:{MMPU.WebSocketUserName}", InfoLog.InfoClass.Debug);
                MMPU.WebSocketPassword = MMPU.读取exe默认配置文件("WebSocketPassword", "defaultPassword");
                InfoLog.InfoPrintf($"配置文件初始化任务[WebSocketPassword]:{MMPU.WebSocketPassword}", InfoLog.InfoClass.Debug);
                if (!WSServer.WSServer.IsOpen)
                {
                    WSServer.WSServer.Open();
                }
            }


        }
        public class DDTV初始化
        {
            public static void 系统初始化()
            {
                //直播缓存目录
                MMPU.直播缓存目录 = MMPU.读取exe默认配置文件("Livefile", "./tmp/LiveCache/");
                InfoLog.InfoPrintf($"配置文件初始化任务[直播缓存目录]:{MMPU.直播缓存目录}", InfoLog.InfoClass.Debug);
                //剪切板监听
                MMPU.剪贴板监听 = MMPU.读取exe默认配置文件("ClipboardMonitoring", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[剪贴板监听]:{MMPU.剪贴板监听}", InfoLog.InfoClass.Debug);
                //第一次使用DDTV
                MMPU.是否第一次使用DDTV = MMPU.读取exe默认配置文件("IsFirstTimeUsing", "1") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[是否第一次使用DDTV]:{MMPU.是否第一次使用DDTV}", InfoLog.InfoClass.Debug);
                MMPU.开机自启动 = MMPU.读取exe默认配置文件("BootUp", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[开机自启动]:{MMPU.开机自启动}", InfoLog.InfoClass.Debug);
                //最大直播并行数量
                MMPU.最大直播并行数量 = int.Parse(MMPU.读取exe默认配置文件("PlayNum", "5"));
                InfoLog.InfoPrintf($"配置文件初始化任务[最大直播并行数量]:{MMPU.最大直播并行数量}", InfoLog.InfoClass.Debug);
                MMPU.清空播放缓存();
            }
            public static void 播放器初始化()
            {
                //默认音量
                MMPU.默认音量 = int.Parse(MMPU.读取exe默认配置文件("DefaultVolume", "50"));
                InfoLog.InfoPrintf($"配置文件初始化任务[默认音量]:{MMPU.默认音量}", InfoLog.InfoClass.Debug);
                //缩小功能
                MMPU.缩小功能 = int.Parse(MMPU.读取exe默认配置文件("Zoom", "1"));
                InfoLog.InfoPrintf($"配置文件初始化任务[缩小功能]:{MMPU.缩小功能}", InfoLog.InfoClass.Debug);
                //默认弹幕颜色
                MMPU.默认弹幕颜色 = MMPU.读取exe默认配置文件("DanMuColor", "0xFF,0x00,0x00,0x00");
                InfoLog.InfoPrintf($"配置文件初始化任务[默认弹幕颜色]:{MMPU.默认弹幕颜色}", InfoLog.InfoClass.Debug);
                //默认字幕颜色
                MMPU.默认字幕颜色 = MMPU.读取exe默认配置文件("ZiMuColor", "0xFF,0x00,0x00,0x00");
                InfoLog.InfoPrintf($"配置文件初始化任务[默认字幕颜色]:{MMPU.默认字幕颜色}", InfoLog.InfoClass.Debug);
                //默认字幕大小
                MMPU.默认字幕大小 = int.Parse(MMPU.读取exe默认配置文件("ZiMuSize", "24"));
                InfoLog.InfoPrintf($"配置文件初始化任务[默认字幕大小]:{MMPU.默认字幕大小}", InfoLog.InfoClass.Debug);
                //默认弹幕大小
                MMPU.默认弹幕大小 = int.Parse(MMPU.读取exe默认配置文件("DanMuSize", "20"));
                InfoLog.InfoPrintf($"配置文件初始化任务[默认弹幕大小]:{MMPU.默认弹幕大小}", InfoLog.InfoClass.Debug);
                //默认弹幕大小
                MMPU.播放缓冲时长 = int.Parse(MMPU.读取exe默认配置文件("BufferDuration", "3"));
                InfoLog.InfoPrintf($"配置文件初始化任务[播放缓冲时长]:{MMPU.播放缓冲时长}", InfoLog.InfoClass.Debug);
                //播放窗口默认高度
                MMPU.播放器默认高度 = int.Parse(MMPU.读取exe默认配置文件("PlayWindowH", "450"));
                InfoLog.InfoPrintf($"配置文件初始化任务[播放器默认高度]:{MMPU.播放器默认高度}", InfoLog.InfoClass.Debug);
                //播放窗口默认宽度
                MMPU.播放器默认宽度 = int.Parse(MMPU.读取exe默认配置文件("PlayWindowW", "800"));
                InfoLog.InfoPrintf($"配置文件初始化任务[播放器默认宽度]:{MMPU.播放器默认宽度}", InfoLog.InfoClass.Debug);
            }
        }
        public class DDTVLiveRec初始化
        {

        }
        public class 辅助功能初始化
        {
            /// <summary>
            /// 心跳和检测网络环境变动
            /// </summary>
            /// <param name="模式"></param>
            public static void 系统心跳初始化(int 模式)
            {
                ///心跳检查
                new Thread(new ThreadStart(delegate {
                    while (true)
                    {
                        try
                        {
                            MMPU.TcpSend(模式 == 0 ? Server.RequestCode.SET_DokiDoki_DDTV : Server.RequestCode.SET_DokiDoki_DDTVLiveRec, "{}", true, 100);
                        }
                        catch (Exception) { }
                        Thread.Sleep(3600 * 1000);
                    }
                })).Start();
                string LIP = string.Empty;
                try
                {
                    LIP = MMPU.TcpSend(Server.RequestCode.GET_IP, "{}", true, 50);
                }
                catch (Exception) { }
                int Num = 0;
                ///网络状态检查
                new Thread(new ThreadStart(delegate {
                    while (MMPU.网络环境变动监听)
                    {
                        Num++;
                        try
                        {

                            string NIP = MMPU.TcpSend(Server.RequestCode.GET_IP, "{}", true, 50);
                            if (Num > 3)
                            {
                                if (LIP != NIP && MMPU.IsCorrectIP(LIP) && MMPU.IsCorrectIP(NIP))
                                {
                                    InfoLog.InfoPrintf($"■■■■■■■■■■■■■■■■■■■■■■■■■■■ERROR!错误警告！■■■■■■■■■■■■■■■■■■■■■■■■■■■\n检测到系统网络中断，多个DDTV录制中的线程抛出无法处理的异常\n这个错误是由于网络环境变化引起的，不是由DDTV引起的，一般是由于光猫、路由器重启或者宽带重新拨号引起的，DDTV无法处理该异常\n网络中断若干时间且外网地址由\n{LIP}变化为{NIP}，网络错误前的任务将冻结任务建立新的续命任务，恢复后新建立的任务正常录制\n■■■■■■■■■■■■■■■■■■■■■■■■■■■ERROR!错误警告！■■■■■■■■■■■■■■■■■■■■■■■■■■■", InfoLog.InfoClass.系统错误信息);
                                    try
                                    {
                                        foreach (var item in Auxiliary.MMPU.DownList)
                                        {
                                            try
                                            {
                                                if (item.DownIofo.下载状态)
                                                {
                                                    item.DownIofo.下载状态 = false;
                                                    item.DownIofo.WC.CancelAsync();
                                                    new Task(() =>
                                                    {
                                                        Downloader DLL = Downloader.新建下载对象(item.DownIofo.平台, item.DownIofo.房间_频道号, bilibili.根据房间号获取房间信息.获取标题(item.DownIofo.标题), Guid.NewGuid().ToString(), bilibili.根据房间号获取房间信息.下载地址(item.DownIofo.标题), "网络环境变化，新建续命任务", item.DownIofo.是否保存, item.DownIofo.主播名称, false, null);
                                                    }).Start();
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                InfoLog.InfoPrintf($"网络环境发生变化造成未知错误:{e.ToString()}", InfoLog.InfoClass.系统错误信息);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        InfoLog.InfoPrintf($"网络环境发生变化造成未知错误:{e.ToString()}", InfoLog.InfoClass.系统错误信息);
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(NIP))
                            {
                                LIP = NIP;
                            }
                        }
                        catch (Exception) { }
                        Thread.Sleep(30 * 1000);
                    }
                })).Start();
                new Task(() => {
                    while (true)
                    {
                        try
                        {
                            int 下载中 = 0;
                            foreach (var item in MMPU.DownList)
                            {
                                if (item.DownIofo.下载状态)
                                {
                                    下载中++;
                                }
                            }
                            InfoLog.InfoPrintf($"[DDTVLR心跳信息]临时API监控房间数:{RoomList.Count - 已连接的直播间状态.Count},WSS长连接数:{已连接的直播间状态.Count},{下载中}个下载中", InfoLog.InfoClass.下载系统信息);
                        }
                        catch (Exception) { }
                        Thread.Sleep(MMPU.心跳打印间隔 * 1000);
                    }
                }).Start();
            }
            public static void 文件删除后台委托初始化()
            {
                new Thread(new ThreadStart(delegate
                {
                    while (true)
                    {
                        try
                        {
                        DEL: if (MMPU.DeleteFileList.Count > 0)
                            {
                                foreach (var item in MMPU.DeleteFileList)
                                {
                                    if (File.Exists(item))
                                    {
                                        if (!MMPU.文件是否正在被使用(item))
                                        {
                                            try
                                            {
                                                File.Delete(item);
                                                InfoLog.InfoPrintf($"后台文件处理池{item}删除委托完成", InfoLog.InfoClass.Debug);
                                                MMPU.DeleteFileList.Remove(item);
                                                goto DEL;
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                    else
                                    {
                                        MMPU.DeleteFileList.Remove(item);
                                        goto DEL;
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                        Thread.Sleep(5000);
                    }
                })).Start();
            }
        }
    }
}
