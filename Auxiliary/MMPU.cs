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
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using static Auxiliary.bilibili;
using System.Drawing;

namespace Auxiliary
{ 
    public class MMPU
    {
        public static bool 开发模式 = 读取exe默认配置文件("DevelopmentModel", "0") == "1" ? true : false;
        public static string[] 开发更改 = new string[] 
        {
            "增加转码完成后自动删除原始flv文件的功能和对应的配置文件"
        };
        public static 弹窗提示 弹窗 = new 弹窗提示();
        public static List<Downloader> DownList = new List<Downloader>();
        public static bool 弹幕显示使能=false;
        public static bool 字幕显示使能 = false;
        public static string 直播缓存目录 = "";
        public static int 直播更新时间 = 20;
        public static string 下载储存目录 = "";
        public static string 版本号 = "2.0.5.1d";
        public static string 开发版本号 = $"开发模式(基于Ver{版本号}主分支)";     
        public static string[] 不检测的版本号 = { "2.0.5.1c" };
        public static bool 第一次打开播放窗口 = true;
        public static int 默认音量 = 0;
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
        public static int 播放器默认高度 = 0;
        public static int 播放器默认宽度 = 0;
        public static int 系统内核版本 = Environment.OSVersion.Version.Major;
        public static int 直播列表刷新间隔 = 0;
        public static string Cookie = "";
        public static string csrf = "";
        public static DateTime CookieEX = new DateTime();
        public static string UID = "";
        public static string BiliUserFile = "./BiliUser.ini";
        public static int 播放缓冲时长 = 4;
        public static string AESKey = "rzqIzYmDQFqQmWfr";
        public static string AESVal = "itkIBBs5JdCLKqpP";
        public static bool 转码功能使能 = false;
        public static bool 转码后自动删除文件 = false;
        public static string 房间状态MD5值 = string.Empty;
        public static bool 初始化后启动下载提示 = true;
        public static bool 是否提示一键导入 = true;
        public static bool 剪贴板监听 = false;
        public static bool 录制弹幕 = true;
        public static bool DDC采集使能 = true;
        public static bool 开机自启动 = false;
        public static int DDC采集间隔 = 60000;
        public static int 数据源 = 0;//0：vdb   1：B API
        public static bool 是否第一次使用DDTV = true;
        public static bool 是否有新版本 = false;
        public static string 更新公告 = "";
        public static string webServer默认监听IP = "0.0.0.0";
        public static string webServer默认监听端口 = "11419";
        public static string webServer_pfx证书名称 = "";
        public static string webServer_pfx证书密码 = "";
        public static string 缓存路径 = "./tmp/";
        public static int 弹幕录制种类 = 2;
        public static int wss连接错误的次数 = 0;
        public static bool 已经提示wss连接错误 = false;
        public static bool Debug模式 = 开发模式 ? true : false;
        public static bool Debug输出到文件 = true;
        public static bool Debug打印到终端 = 开发模式 ? true : false;
        public static bool 强制WSS连接模式 = false;
        public static int 心跳打印间隔 = 180;
        public static string webadmin验证字符串 = "";
        public static string webghost验证字符串 = "";
        public static bool 是否启用SSL = false;
        public static string ApiToken = "";

        public static int 启动模式 = 0;//0：DDTV,1：DDTVLive,2：DDTV服务器
        public static bool 网络环境变动监听 = false;

        public static List<string> DeleteFileList = new List<string>();
        

        /// <summary>
        /// 配置文件初始化
        /// </summary>
        /// <param name="模式">//0：DDTV,1：DDTVLive</param>
        public static bool 配置文件初始化(int 模式)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句
            Debug模式 = 读取exe默认配置文件("DebugMod", "1") == "0" ? false : true;
            Debug输出到文件 = 读取exe默认配置文件("DebugFile", "1") == "0" ? false : true;
            Debug打印到终端 = 读取exe默认配置文件("DebugCmd", "0") == "0" ? false : true;
            
          
            if (模式 == 0)
            {
                InfoLog.InfoInit("DDTVLog.out", new InfoLog.InfoClasslBool()
                {
                    Debug = Debug模式,
                    下载必要提示 = true,
                    杂项提示 = false,
                    系统错误信息 = true,
                    是否将日志输出到文件 = Debug输出到文件,
                    上传必要提示 = true
                });
                启动模式 = 0;
            }
            else if (模式 == 1)
            {
                InfoLog.InfoInit("DDTVLiveRecLog.out", new InfoLog.InfoClasslBool()
                {
                    Debug = Debug模式,
                    下载必要提示 = true,
                    杂项提示 = false,
                    系统错误信息 = true,
                    是否将日志输出到文件 = Debug输出到文件,
                    上传必要提示 = true
                });
                启动模式 = 1;
            }
            InfoLog.InfoPrintf("消息系统初始化完成", InfoLog.InfoClass.Debug);
            InfoLog.InfoPrintf($"配置文件初始化任务[Debug模式]:{Debug模式}", InfoLog.InfoClass.Debug);
            InfoLog.InfoPrintf($"配置文件初始化任务[Debug输出到文件]:{Debug输出到文件}", InfoLog.InfoClass.Debug);
            InfoLog.InfoPrintf($"配置文件初始化任务[Debug打印到终端]:{Debug打印到终端}", InfoLog.InfoClass.Debug);
            心跳打印间隔 = int.Parse(读取exe默认配置文件("DokiDoki", "300"));
            InfoLog.InfoPrintf($"配置文件初始化任务[心跳打印间隔]:{心跳打印间隔}", InfoLog.InfoClass.Debug);
            网络环境变动监听 = 读取exe默认配置文件("NetStatusMonitor", "0") == "0" ? false : true;
            InfoLog.InfoPrintf($"配置文件初始化任务[网络环境变动监听]:{网络环境变动监听}", InfoLog.InfoClass.Debug);

            Upload.Uploader.InitUpload();//初始化上传配置

            #region 配置文件设置
            if (模式 == 0)
            {
                //默认音量
                MMPU.默认音量 = int.Parse(MMPU.读取exe默认配置文件("DefaultVolume", "50"));
                InfoLog.InfoPrintf($"配置文件初始化任务[默认音量]:{默认音量}", InfoLog.InfoClass.Debug);
                //缩小功能
                MMPU.缩小功能 = int.Parse(MMPU.读取exe默认配置文件("Zoom", "1"));
                InfoLog.InfoPrintf($"配置文件初始化任务[缩小功能]:{缩小功能}", InfoLog.InfoClass.Debug);
                //最大直播并行数量
                MMPU.最大直播并行数量 = int.Parse(MMPU.读取exe默认配置文件("PlayNum", "5"));
                InfoLog.InfoPrintf($"配置文件初始化任务[最大直播并行数量]:{最大直播并行数量}", InfoLog.InfoClass.Debug);
                //默认弹幕颜色
                MMPU.默认弹幕颜色 = MMPU.读取exe默认配置文件("DanMuColor", "0xFF,0x00,0x00,0x00");
                InfoLog.InfoPrintf($"配置文件初始化任务[默认弹幕颜色]:{默认弹幕颜色}", InfoLog.InfoClass.Debug);
                //默认字幕颜色
                MMPU.默认字幕颜色 = MMPU.读取exe默认配置文件("ZiMuColor", "0xFF,0x00,0x00,0x00");
                InfoLog.InfoPrintf($"配置文件初始化任务[默认字幕颜色]:{默认字幕颜色}", InfoLog.InfoClass.Debug);
                //默认字幕大小
                MMPU.默认字幕大小 = int.Parse(MMPU.读取exe默认配置文件("ZiMuSize", "24"));
                InfoLog.InfoPrintf($"配置文件初始化任务[默认字幕大小]:{默认字幕大小}", InfoLog.InfoClass.Debug);
                //默认弹幕大小
                MMPU.默认弹幕大小 = int.Parse(MMPU.读取exe默认配置文件("DanMuSize", "20"));
                InfoLog.InfoPrintf($"配置文件初始化任务[默认弹幕大小]:{默认弹幕大小}", InfoLog.InfoClass.Debug);
                //默认弹幕大小
                MMPU.播放缓冲时长 = int.Parse(MMPU.读取exe默认配置文件("BufferDuration", "3"));
                InfoLog.InfoPrintf($"配置文件初始化任务[播放缓冲时长]:{播放缓冲时长}", InfoLog.InfoClass.Debug);
                //直播缓存目录
                MMPU.直播缓存目录 = MMPU.读取exe默认配置文件("Livefile", "./tmp/LiveCache/");
                InfoLog.InfoPrintf($"配置文件初始化任务[直播缓存目录]:{直播缓存目录}", InfoLog.InfoClass.Debug);
                //播放窗口默认高度
                MMPU.播放器默认高度 = int.Parse(MMPU.读取exe默认配置文件("PlayWindowH", "450"));
                InfoLog.InfoPrintf($"配置文件初始化任务[播放器默认高度]:{播放器默认高度}", InfoLog.InfoClass.Debug);
                //播放窗口默认宽度
                MMPU.播放器默认宽度 = int.Parse(MMPU.读取exe默认配置文件("PlayWindowW", "800"));
                InfoLog.InfoPrintf($"配置文件初始化任务[播放器默认宽度]:{播放器默认宽度}", InfoLog.InfoClass.Debug);
                //剪切板监听
                MMPU.剪贴板监听 = MMPU.读取exe默认配置文件("ClipboardMonitoring", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[剪贴板监听]:{剪贴板监听}", InfoLog.InfoClass.Debug);
                //第一次使用DDTV
                MMPU.是否第一次使用DDTV = MMPU.读取exe默认配置文件("IsFirstTimeUsing", "1") == "0" ? false :true;
                InfoLog.InfoPrintf($"配置文件初始化任务[是否第一次使用DDTV]:{是否第一次使用DDTV}", InfoLog.InfoClass.Debug);
                MMPU.开机自启动 = MMPU.读取exe默认配置文件("BootUp", "0") == "0" ? false : true;
                InfoLog.InfoPrintf($"配置文件初始化任务[开机自启动]:{开机自启动}", InfoLog.InfoClass.Debug);
                清空播放缓存();
            }
            else if (模式 == 1)
            {
                MMPU.webServer默认监听IP = MMPU.读取exe默认配置文件("LiveRecWebServerDefaultIP", "0.0.0.0");
                InfoLog.InfoPrintf($"配置文件初始化任务[webServer默认监听IP]:{webServer默认监听IP}", InfoLog.InfoClass.Debug);
                MMPU.webServer默认监听端口 = MMPU.读取exe默认配置文件("Port", "11419");
                InfoLog.InfoPrintf($"配置文件初始化任务[webServer默认监听端口]:{webServer默认监听端口}", InfoLog.InfoClass.Debug);
                MMPU.webadmin验证字符串 = MMPU.读取exe默认配置文件("WebAuthenticationAadminPassword", "admin");
                MMPU.webghost验证字符串 = MMPU.读取exe默认配置文件("WebAuthenticationGhostPasswrod", "ghost");
                MMPU.webghost验证字符串 = MMPU.读取exe默认配置文件("WebAuthenticationCode", "DDTVLiveRec");
                MMPU.webServer_pfx证书名称 = MMPU.读取exe默认配置文件("sslName", "");
                MMPU.webServer_pfx证书密码 = MMPU.读取exe默认配置文件("sslPssword", "");
                if (!string.IsNullOrEmpty(webServer_pfx证书名称) && !string.IsNullOrEmpty(webServer_pfx证书密码))
                {
                    是否启用SSL = true;
                    InfoLog.InfoPrintf($"配置文件初始化任务[SSL证书初始化]:{webServer_pfx证书名称}", InfoLog.InfoClass.Debug);
                    InfoLog.InfoPrintf($"======检测到SSL证书=======\r\n\r\n请使用[https://本设备IP或域名:" + webServer默认监听端口+ "]进行访问\r\n\r\n======检测到SSL连接=======", InfoLog.InfoClass.下载必要提示);
                }
                else
                {
                    是否启用SSL = false;
                    InfoLog.InfoPrintf($"配置文件初始化任务[SSL证书初始化]:证书不存在！或密码不存在，SSL证书加载失败", InfoLog.InfoClass.Debug);
                    InfoLog.InfoPrintf($"======未检测到SSL证书=======\r\n\r\n请使用[http://本设备IP或域名:" + webServer默认监听端口 + "]进行访问\r\n\r\n======未检测到SSL连接=======", InfoLog.InfoClass.下载必要提示);
                }
                MMPU.ApiToken = MMPU.读取exe默认配置文件("ApiToken", "1145141919810AAAAAA");
            }
            //数据源
            MMPU.数据源 = int.Parse(MMPU.读取exe默认配置文件("DataSource", "0"));
            InfoLog.InfoPrintf($"配置文件初始化任务[数据源]:{数据源}", InfoLog.InfoClass.Debug);
            //是否启动WS连接组
            bilibili.是否启动WSS连接组 = MMPU.读取exe默认配置文件("NotVTBStatus", "0") == "0" ? false : true;
            InfoLog.InfoPrintf($"配置文件初始化任务[是否启动WSS连接组]:{是否启动WSS连接组}", InfoLog.InfoClass.Debug);
            //转码功能使能和转码后删除文件
            MMPU.转码功能使能 = MMPU.读取exe默认配置文件("AutoTranscoding", "0") == "1" ? true : false;
            InfoLog.InfoPrintf($"配置文件初始化任务[转码功能使能]:{转码功能使能}", InfoLog.InfoClass.Debug);
            MMPU.转码后自动删除文件 = MMPU.读取exe默认配置文件("AutoTranscodingDelFile", "0") == "1" ? true : false;
            InfoLog.InfoPrintf($"配置文件初始化任务[转码后自动删除文件]:{转码后自动删除文件}", InfoLog.InfoClass.Debug);
            //检查配置文件
            bilibili.BiliUser.CheckPath(MMPU.BiliUserFile);
            //检查弹幕录制配置
            MMPU.录制弹幕 = MMPU.读取exe默认配置文件("RecordDanmu", "0") == "1" ? true : false;
            InfoLog.InfoPrintf($"配置文件初始化任务[录制弹幕]:{录制弹幕}", InfoLog.InfoClass.Debug);
            //房间配置文件
            RoomInit.RoomConfigFile = MMPU.读取exe默认配置文件("RoomConfiguration", "./RoomListConfig.json");
            InfoLog.InfoPrintf($"配置文件初始化任务[RoomConfigFile]:{RoomInit.RoomConfigFile}", InfoLog.InfoClass.Debug);
            //房间配置文件
            MMPU.下载储存目录 = MMPU.读取exe默认配置文件("file", "./tmp/");
            CheckPath(ref MMPU.下载储存目录);
            if (!Directory.Exists("./tmp"))
            {
                Directory.CreateDirectory("./tmp");
            }
            InfoLog.InfoPrintf($"配置文件初始化任务[下载储存目录]:{下载储存目录}", InfoLog.InfoClass.Debug);
            //直播表刷新默认间隔
            MMPU.直播列表刷新间隔 = int.Parse(MMPU.读取exe默认配置文件("LiveListTime", "5"));
            InfoLog.InfoPrintf($"配置文件初始化任务[直播列表刷新间隔]:{直播列表刷新间隔}", InfoLog.InfoClass.Debug);


            //直播更新时间
            MMPU.直播更新时间 = int.Parse(MMPU.读取exe默认配置文件("RoomTime", "20"));
            InfoLog.InfoPrintf($"配置文件初始化任务[直播更新时间]:{直播更新时间}", InfoLog.InfoClass.Debug);
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
            #endregion
            InfoLog.InfoPrintf("通用配置加载完成", InfoLog.InfoClass.Debug);

            BiliUser配置文件初始化(模式);
            InfoLog.InfoPrintf("Bilibili账号信息加载完成", InfoLog.InfoClass.Debug);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            if (MMPU.数据源 == 0)
            {
                DDcenter.DdcClient.Connect();
            }
            VTBS.API.VTBS服务器CDN.根据CDN更新VTBS_Url();
            try
            {
                RoomInit.start();
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf($"房间配置文件加载过程中发生错误，文件格式不符合要求，请检查文件内容。错误堆栈:\n{e.ToString()}", InfoLog.InfoClass.系统错误信息);
            }
            DokiDoki(模式);
            文件删除后台委托();
            Downloader.轮询检查下载任务();
            return true;
        }
        private static void 文件删除后台委托()
        {
            new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    try
                    {
                       DEL: if(DeleteFileList.Count>0)
                        {
                            foreach (var item in DeleteFileList)
                            {
                                if (File.Exists(item))
                                {
                                    if (!文件是否正在被使用(item))
                                    {
                                        try
                                        {
                                            File.Delete(item);
                                            InfoLog.InfoPrintf($"后台文件处理池{item}删除委托完成", InfoLog.InfoClass.Debug);
                                            DeleteFileList.Remove(item);
                                            goto DEL;
                                        }
                                        catch (Exception) { }  
                                    }
                                }
                                else
                                {
                                    DeleteFileList.Remove(item);
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
        /// <summary>
        /// 心跳和检测网络环境变动
        /// </summary>
        /// <param name="模式"></param>
        public static void DokiDoki(int 模式)
        {
            ///心跳检查
            new Thread(new ThreadStart(delegate {
                while (true)
                {
                    try
                    {
                       MMPU.TcpSend(模式 == 0 ? Server.RequestCode.SET_DokiDoki_DDTV : Server.RequestCode.SET_DokiDoki_DDTVLiveRec, "{}", true,100);
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
            catch (Exception){}
            int Num = 0;
            ///网络状态检查
            new Thread(new ThreadStart(delegate {
                while (网络环境变动监听)
                {
                    Num++;
                    try
                    {
                      
                        string NIP = MMPU.TcpSend(Server.RequestCode.GET_IP, "{}", true, 50);
                        if (Num > 3)
                        {
                            if (LIP != NIP&& IsCorrectIP(LIP)&& IsCorrectIP(NIP))
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
                        if(!string.IsNullOrEmpty(NIP))
                        {
                            LIP = NIP;
                        }            
                    }
                    catch (Exception) { }
                    Thread.Sleep(30 * 1000);
                }
            })).Start();
            new Task(()=> { 
            while(true)
                {
                    try
                    {
                        int 下载中 = 0;
                        foreach (var item in DownList)
                        {
                            if (item.DownIofo.下载状态)
                            {
                                下载中++;
                            }
                        }
                        InfoLog.InfoPrintf($"[DDTVLR心跳信息]临时API监控房间数:{RoomList.Count - 已连接的直播间状态.Count},WSS长连接数:{已连接的直播间状态.Count},{下载中}个下载中", InfoLog.InfoClass.下载必要提示);
                    }
                    catch (Exception){}
                    Thread.Sleep(心跳打印间隔 * 1000);
                }
            }).Start();
        }
        public static void BiliUser配置文件初始化(int 模式)
        {
            //账号登陆cookie
            try
            {
                MMPU.Cookie = Encryption.UnAesStr(MMPU.读ini配置文件("User", "Cookie", MMPU.BiliUserFile), MMPU.AESKey, MMPU.AESVal);
                InfoLog.InfoPrintf($"配置文件初始化任务[Cookie]敏感信息，隐藏内容，信息长度:{Cookie.Length}", InfoLog.InfoClass.Debug);
                if (!MMPU.Cookie.Contains("=")|| !MMPU.Cookie.Contains(";"))
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
            InfoLog.InfoPrintf($"配置文件初始化任务[UID]敏感信息，隐藏内容，信息长度:{UID.Length}", InfoLog.InfoClass.Debug);
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
                InfoLog.InfoPrintf("\r\n===============================\r\nbilibili账号cookie为空或已过期，请更新BiliUser.ini信息\r\n===============================", InfoLog.InfoClass.下载必要提示);
                InfoLog.InfoPrintf("\r\n==============\r\nBiliUser.ini文件无效，请使用DDTV本体登陆成功后把DDTV本体里的BiliUser.ini文件覆盖无效的文件\r\n==============", InfoLog.InfoClass.下载必要提示);
                if (模式 == 1)
                {
                    InfoLog.InfoPrintf("\r\n如果短信验证方式验证启动失败，请复制DDTV2本体中有效BiliUser.ini覆盖本地文件后重启DDTVLiveRec\r\n[======如果是非windows系统，请检查文件权限======]", InfoLog.InfoClass.下载必要提示);
                    try
                    {
                        bilibili.BiliUser.登陆();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    while(string.IsNullOrEmpty(MMPU.Cookie))
                    {
                        InfoLog.InfoPrintf("\r\n请根据登陆验证提示操作", InfoLog.InfoClass.系统错误信息);
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
                            加载网络房间方法.更新网络房间缓存();
                        }).Start();
                    }
                }
            }
            MMPU.csrf = MMPU.读ini配置文件("User", "csrf", MMPU.BiliUserFile);
            InfoLog.InfoPrintf($"配置文件初始化任务[csrf]敏感信息，隐藏内容，信息长度:{csrf.Length}", InfoLog.InfoClass.Debug);
        }
        public static void 修改默认音量设置(int A)
        {
            默认音量 = A;
            MMPU.setFiles("DefaultVolume", A.ToString());
        }
        public static string 寻找下载列表键值(string str, string name)
        {
            str = str.Replace(" ", "").Replace("\"", "").Replace("{", "").Replace("}", "");
            foreach (var item in str.Split(','))
            {
                if (item.Split('=')[0] == name)
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
        /// 读取配置文件(如果不存在该值，则生成该配置键值对)
        /// </summary>
        /// <param name="name">值名称</param>
        /// <param name="V">如果不存在该值默认填写的默认值</param>
        public static string 读取exe默认配置文件(string name, string V)
        {
            string A1 = V;
            try
            {
                A1 = getFiles(name, V);
            }
            catch (Exception)
            {
                if (string.IsNullOrEmpty(V))
                {
                    setFiles(name, "1");
                    setFiles(name, "");
                }
                else;
                {
                    setFiles(name,V);
                }
                
            }
            return A1;
        }

        public static string 读ini配置文件(string 节点, string 项目, string 路径)
        {
            if (MMPU.启动模式 == 0)
            {
                return bilibili.BiliUser.Read(节点, 项目, null, 路径);
            }
            else if(MMPU.启动模式==1)
            {
                string text = File.ReadAllText(路径);
                try
                {
                    text = text.Replace("\r\n", "\r").Replace("\n","\r");

                    string[] A2 = text.Replace("\n","").Replace(项目, "壹").Split('壹')[1].Split('\r')[0].Split('=');
                    string A1 = string.Empty;
                    for (int i =1;i< A2.Length;i++)
                    {
                        A1 +=A2[i];
                    }
                   
                    return A1;
                }
                catch (Exception)
                {
                    return "";
                }
                
            }
            else
            {
                return "";
            }
        }

        public static void 写ini配置文件(string 节点, string 项目, string 值, string 路径)
        {
            if(MMPU.启动模式==0)
            {
                bilibili.BiliUser.Write(节点, 项目, 值, 路径);
            }
            else if (MMPU.启动模式 == 1)
            {
                string text = File.ReadAllText(路径);
                if(!text.Contains("[User]"))
                {
                    text = "[User]\r\n" + text;
                }
                if (text.Contains(项目+"="))
                {
                    string B1 = 项目 + "=" + text.Replace(项目 + "=", "⒆").Split('⒆')[1].Replace("\r\n", "⒆").Split('⒆')[0];
                    string BB = 项目 + "=" + 值;
                    text = text.Replace(B1, BB);
                }
                else
                {
                    text = text + "\r\n" + 项目 + "=" + 值;
                }
                File.WriteAllText(路径, text);
            }
         
        }
        public static IPAddress 根据URL获取IP地址(string URL)
        {
            IPAddress[] ipaddress = Dns.GetHostAddresses(URL);
            return ipaddress[0];
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
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            req.UserAgent = MMPU.UA.Ver.UA();
            if (url.Contains("bilibili"))
            {
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    req.CookieContainer = 转化GET_cookie(MMPU.Cookie);
                }
            }
            // req.Timeout = 5000;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        public static string 返回网页内容_POST_JSON(string url, string jsonParam, string encode)
        {
            string strURL = url;
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=" + encode.ToUpper();
            string paraUrlCoded = jsonParam;
            byte[] payload;
            payload = Encoding.GetEncoding(encode.ToUpper()).GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.GetEncoding(encode.ToUpper()));
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;

        }
        public static string 返回网页内容_GET(string url,int outTime)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            req.UserAgent = MMPU.UA.Ver.UA();
            //if (url.Contains("bilibili"))
            //{
            //    if (!string.IsNullOrEmpty(MMPU.Cookie))
            //    {
            //        req.CookieContainer = 转化GET_cookie(MMPU.Cookie);
            //    }
            //}
            req.Timeout = outTime;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            bool 完成 = false;
            //获取响应内容  
            new Task(() =>
            {
                try
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                        完成 = true;
                    }
                }
                catch (Exception)
                {
                }
            }).Start();
            new Task(() =>
            {
                try
                {
                    Thread.Sleep(5000);
                    完成 = true;

                }
                catch (Exception)
                {
                }
            }).Start();
            while (!完成)
            {
                Thread.Sleep(50);
            }

            return result;
        }
        public static CookieContainer 转化GET_cookie(string cookie)
        {
            CookieContainer CK = new CookieContainer { MaxCookieSize = 4096, PerDomainCapacity = 50 };

            string[] cook = cookie.Replace(" ", "").Split(';');
            for (int i = 0; i < cook.Length; i++)
            {
                try
                {
                    CK.Add(new Cookie(cook[i].Split('=')[0], cook[i].Split('=')[1].Replace(",", "%2C")) { Domain = "live.bilibili.com" });
                }
                catch (Exception)
                {

                }
            }
            return CK;
        }
        public static string 使用WC获取网络内容(string url)
        {
            var wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            wc.Headers.Add("User-Agent: " + MMPU.UA.Ver.UA());
            if (url.Contains("bilibili"))
            {
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    wc.Headers.Add("Cookie", MMPU.Cookie);
                }
            }
            byte[] roomHtml = wc.DownloadData(url);
            wc.Dispose();
            return Encoding.UTF8.GetString(roomHtml);
        }
        public static string 获取网页数据_下载视频用(string url, bool 解码)
        {
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
            rq.CookieContainer = new CookieContainer();
            rq.Method = "GET";
            rq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            rq.Headers.Add("Accept-Encoding: gzip, deflate, br");
            rq.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            rq.Headers.Add("Cache-Control: max-age=0");
            rq.Headers.Add("Sec-Fetch-Mode: navigate");
            rq.Headers.Add("Sec-Fetch-Site: none");
            rq.Headers.Add("Sec-Fetch-User: ?1");
            rq.Headers.Add("Upgrade-Insecure-Requests: 1");
            rq.Headers.Add("Cache-Control: max-age=0");
            //if (url.Contains("bilibili"))
            //{
            //    if (!string.IsNullOrEmpty(MMPU.Cookie))
            //    {
            //        rq.CookieContainer = 转化GET_cookie(MMPU.Cookie);
            //    }
            //}
            //rq.Host = "www.bilibili.com";
            rq.UserAgent = MMPU.UA.Ver.UA();
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
            public static List<列表加载缓存> 列表缓存1 = new List<列表加载缓存>();
            public static List<列表加载缓存> 列表缓存2 = new List<列表加载缓存>();
            public static bool 是否正在缓存 = false;
            public static void 更新网络房间缓存()
            {
                int A = 1;
                new Task((() => 
                {
                    是否正在缓存 = true;
                 //  while(true)
                    {
                        try
                        {
                            InfoLog.InfoPrintf("开始更新网络房间缓存", InfoLog.InfoClass.Debug);
                            try
                            {

                                string roomHtml = "";
                                try
                                {
                                    roomHtml = 返回网页内容_GET(VTBS.API.VTBS服务器CDN.VTBS_Url + "/v1/short", 8000);
                                    InfoLog.InfoPrintf("网络房间缓存vtbs加载完成", InfoLog.InfoClass.Debug);
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        InfoLog.InfoPrintf("网络房间缓存vtbs加载失败", InfoLog.InfoClass.Debug);
                                        roomHtml = 返回网页内容_GET("https://raw.githubusercontent.com/CHKZL/DDTV2/master/Auxiliary/DDcenter/VtbsList.json", 12000);
                                        InfoLog.InfoPrintf("网络房间缓存github加载完成", InfoLog.InfoClass.Debug);
                                    }
                                    catch (Exception)
                                    {            
                                        try
                                        {
                                            InfoLog.InfoPrintf("网络房间缓存github加载失败", InfoLog.InfoClass.Debug);
                                            roomHtml = TcpSend(Server.RequestCode.GET_VTBSROOMLIST, "{}", true, 1500);
                                            InfoLog.InfoPrintf("DDTV服务器加载房间缓存成功", InfoLog.InfoClass.Debug);
                                        }
                                        catch (Exception)
                                        {
                                            InfoLog.InfoPrintf("DDTV服务器加载房间缓存失败", InfoLog.InfoClass.Debug);
                                            roomHtml = File.ReadAllText("VtbsList.json");
                                            InfoLog.InfoPrintf("本地文件缓存加载房间缓存成功", InfoLog.InfoClass.Debug);
                                        }
                                    }
                                }
                                JArray result = JArray.Parse(roomHtml);
                                InfoLog.InfoPrintf("网络房间缓存下载完成，开始预处理", InfoLog.InfoClass.Debug);
                                列表缓存2.Clear();
                                foreach (var item in result)
                                {
                                    try
                                    {
                                        if (int.Parse(item["roomid"].ToString()) != 0)
                                        {
                                            列表缓存2.Add(new 列表加载缓存
                                            {
                                                编号 = A,
                                                roomId = item["roomid"].ToString(),
                                                名称 = item["uname"].ToString(),
                                                官方名称 = item["uname"].ToString(),
                                                平台 = "bilibili",
                                                UID = item["mid"].ToString(),
                                                类型 = "V"
                                            });
                                            A++;
                                        }
                                        else
                                        {
                                            ;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                列表缓存1 = 列表缓存2;
                                //foreach (var item in result["vtbs"])
                                //{
                                //    foreach (var x in item["accounts"])
                                //    {
                                //        try
                                //        {
                                //            string name = item["name"][item["name"]["default"].ToString()].ToString();
                                //            if (x["platform"].ToString() == "bilibili")
                                //            {

                                //                列表缓存.Add(new 列表加载缓存
                                //                {
                                //                    编号 = A,
                                //                    名称 = name,
                                //                    官方名称 = name,
                                //                    平台 = "bilibili",
                                //                    UID = x["id"].ToString(),
                                //                    类型 = x["type"].ToString()
                                //                });
                                //                A++;
                                //            }
                                //            //else if (x["platform"].ToString() == "youtube")
                                //            //{

                                //            //    列表缓存.Add(new 列表加载缓存
                                //            //    {
                                //            //        编号 = A,
                                //            //        名称 = name,
                                //            //        官方名称 = name,
                                //            //        平台 = "youtube",
                                //            //        UID = x["id"].ToString(),
                                //            //        类型 = x["type"].ToString()
                                //            //    });
                                //            //    A++;
                                //            //}
                                //        }
                                //        catch (Exception)
                                //        {
                                //            是否正在缓存 = false;
                                //            //throw;
                                //        }
                                //    }
                                //}
                            }
                            catch (Exception E)
                            {
                                是否正在缓存 = false;
                            }
                            是否正在缓存 = false;
                            InfoLog.InfoPrintf("网络房间缓存更新成功", InfoLog.InfoClass.Debug);
                        }
                        catch (Exception)
                        {

                            是否正在缓存 = false;
                        }
                       // Thread.Sleep(300000);
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
                public string roomId { set; get; }
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
                NewWebClient myWebClient = new NewWebClient(1 * 800);
                //wcl.Timeout = 1000;
                Stopwatch spwatch = new Stopwatch();
                spwatch.Start();
                byte[] resultBytes = myWebClient.DownloadData(new Uri(Url));
                spwatch.Stop();
                return spwatch.Elapsed.TotalMilliseconds;
            }
            catch (Exception)
            {

                return -1;
            }
        }
        public class NewWebClient : WebClient
        {
            private int _timeout;

            /// <summary>
            /// 超时时间(毫秒)
            /// </summary>
            public int Timeout
            {
                get
                {
                    return _timeout;
                }
                set
                {
                    _timeout = value;
                }
            }

            public NewWebClient()
            {
                this._timeout = 60000;
            }

            public NewWebClient(int timeout)
            {
                this._timeout = timeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var result = base.GetWebRequest(address);
                result.Timeout = this._timeout;
                return result;
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
            public bool 判断(string url, string P,string roomId)
            {
                try
                {
                    switch (P)
                    {
                        case "bilibili":
                            {

                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.UserAgent = MMPU.UA.Ver.UA();
                                httpWebRequest.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
                                if (!string.IsNullOrEmpty(MMPU.Cookie))
                                {
                                    httpWebRequest.Headers.Add("Cookie", MMPU.Cookie);
                                }
                                httpWebRequest.Timeout = 3000;
                                //返回响应状态是否是成功比较的布尔值

                                if (((HttpWebResponse)httpWebRequest.GetResponse()).StatusCode == HttpStatusCode.OK)
                                {

                                }
                                InfoLog.InfoPrintf("判断文件存在", InfoLog.InfoClass.杂项提示);
                                return true;
                            }
                        case "主站视频":
                            return true;
                        default:
                            return false;
                    }
                }
                //catch (WebException e)
                //{
                //    if(e.Status== WebExceptionStatus.Timeout)
                //    {
                //        return true;
                //    }
                //    return false;

                //}
                catch (Exception )
                {
                    InfoLog.InfoPrintf("判断文件不存在", InfoLog.InfoClass.杂项提示);
                    return false;
                    //if (E.Message.Contains("404"))
                    //{
                    //    InfoLog.InfoPrintf("判断文件不存在", InfoLog.InfoClass.杂项提示);
                    //    return false;
                    //}
                    //else if (E.Message.Contains("475"))
                    //{
                    //    InfoLog.InfoPrintf("判断文件不存在", InfoLog.InfoClass.杂项提示);
                    //    return false;
                    //}
                    //else
                    //{
                    //    return true;
                    //}
                   
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
        public static void 清空播放缓存()
        {
            new Thread(new ThreadStart(delegate {
                try
                {
                    if (Directory.Exists("./tmp/LiveCache/"))
                    {
                        FileInfo[] files = new DirectoryInfo("./tmp/LiveCache/").GetFiles();
                        foreach (var item in files)
                        {
                            MMPU.文件删除委托("./tmp/LiveCache/" + item.Name, "启动/关闭DDTV清空LiveCache缓存文件");
                        }
                    }
                }
                catch (Exception) { }
            })).Start();
        }
        public static void 文件删除委托(string file, string 任务来源)
        {
            new Task((() =>
            {
                try
                {
                    if (!文件是否正在被使用(file))
                    {
                        InfoLog.InfoPrintf($"收到文件删除委托任务，来自:{任务来源}，删除文件:{file}", InfoLog.InfoClass.Debug);
                        File.Delete(file);
                        return;
                    }
                    else
                    {
                        InfoLog.InfoPrintf($"来自:{任务来源}的文件:{file}删除委托失败，文件还在使用中，已将删除任务委托给后台文件处理池", InfoLog.InfoClass.Debug);
                        DeleteFileList.Add(file);
                    }
                }
                catch (Exception)
                {
                    return;
                }
            })).Start();

        }


        /// <summary>
        /// 文件是否被打开
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
        //public static bool 文件是否正在被使用(string fileName)
        //{
        //    bool inUse = true;
        //    try
        //    {
        //        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,
        //        FileShare.None);
        //        fs.Close();
        //        inUse = false;
        //    }
        //    catch
        //    {

        //    }
        //    return inUse;//true表示正在使用,false没有使用
        //}
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
            DateTime dtStart = TimeZone.CurrentTimeZone.ToUniversalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000")+(288000000000);
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        public static void 储存文本(string data, string CurDir)
        {
            //如果启动方式为LiveRec则直接退出，不更新配置文件
            if (MMPU.启动模式 == 1)
            {
                return;
            }
            //文件覆盖方式添加内容
            InfoLog.InfoPrintf($"文件变化:{CurDir}文件收到储存请求，文件更新，更新文件信息长度:{data.Length}", InfoLog.InfoClass.Debug);
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
        public static string getFiles(string name, string V)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string[] B = config.AppSettings.Settings.AllKeys;
            for (int i = 0; i < B.Length; i++)
            {
                if (B[i] == name)
                {
                    // A = true;
                    return config.AppSettings.Settings[name].Value;
                }
            }
            setFiles(name, V);
            return V;
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

        /// <summary>
        /// 和服务器进行TCP通讯
        /// </summary>
        /// <param name="code">命令码</param>
        /// <param name="msg">消息内容</param>
        /// <param name="是否需要回复"></param>
        /// <returns></returns>
        public static string TcpSend(int code, string msg, bool 是否需要回复,int 等待时间,int 超时时间 = 0)
        {
            try
            {
                string 回复内容 = "";
                Socket tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpClient.ReceiveBufferSize = 1024 * 1024 * 8;
                IPAddress ipaddress = Server.IP_ADDRESS;
                EndPoint point = new IPEndPoint(ipaddress, Server.PORT);
                tcpClient.SendTimeout = 超时时间;
                tcpClient.Connect(point);//通过IP和端口号来定位一个所要连接的服务器端
                tcpClient.Send(Encoding.UTF8.GetBytes(JSON发送拼接(code, msg)));
                if (是否需要回复)
                {
                    byte[] buffer = new byte[1024 * 1024 * 8];
                    Thread.Sleep(等待时间);
                    tcpClient.Receive(buffer);
                    string 收到的数据 = Encoding.UTF8.GetString(buffer).Trim('\0');
                    回复内容 = 收到的数据;
                }
                else
                {
                    回复内容 = null;
                }
                tcpClient.Close();
                try
                {
                    tcpClient.Dispose();
                }
                catch (Exception)
                {
                }
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
        /// 指定Post地址和添加cookis使用Post方式获取网页返回内容  
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
            if (url.Contains("bilibili"))
            {
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    req.CookieContainer = 转化GET_cookie(MMPU.Cookie);
                }
            }
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
        /// <summary>
        /// 验证字符串是否为IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsCorrectIP(string ip)
        {
            bool b = Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
            if (b)
                return b;
            else
                return b;
        }
        ///获取当前系统的dpi数值
        public static void SystemDpi(out int x, out int y)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                x = (int)g.DpiX;
                y = (int)g.DpiY;
                g.Dispose();
            }
        }
        ///根据当前系统dpi数值匹配 当前系统的桌面缩放比例
        public static double Scaling(int DpiIndex)
        {
            switch (DpiIndex)
            {
                case 96: return 1;
                case 120: return 1.25;
                case 144: return 1.5;
                case 168: return 1.75;
                case 192: return 2;
            }
            return 1;
        }
        public class UA
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            internal static class Ver
            {
                public const string DESC = "修改API";
                public static readonly string OS_VER = "(" + WinVer.SystemVersion.Major + "." + WinVer.SystemVersion.Minor + "." + WinVer.SystemVersion.Build + ")";
                //ublic static readonly string UA = OS_VER + " AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.119 Safari/537.36";
                public static string UA()
                {
                    if (MMPU.启动模式 == 0)
                    {
                        return OS_VER + "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4305.2 Safari/537.36";
                    }
                    else if (MMPU.启动模式 == 1)
                    {
                        return "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4305.2 Safari/537.36";
                    }
                    else
                    {
                        return "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4305.2 Safari/537.36";
                    }
                }
            }
            internal static class WinVer
            {
                public static readonly Version SystemVersion = GetSystemVersion();

                private static Delegate GetFunctionAddress(IntPtr dllModule, string functionName, Type t)
                {
                    var address = WinApi.GetProcAddress(dllModule, functionName);
                    return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, t);
                }

                private delegate IntPtr RtlGetNtVersionNumbers(ref int dwMajor, ref int dwMinor, ref int dwBuildNumber);

                private static Version GetSystemVersion()
                {
                    var hinst = WinApi.LoadLibrary("ntdll.dll");
                    var func = (RtlGetNtVersionNumbers)GetFunctionAddress(hinst, "RtlGetNtVersionNumbers", typeof(RtlGetNtVersionNumbers));
                    int dwMajor = 0, dwMinor = 0, dwBuildNumber = 0;
                    func.Invoke(ref dwMajor, ref dwMinor, ref dwBuildNumber);
                    dwBuildNumber &= 0xffff;
                    return new Version(dwMajor, dwMinor, dwBuildNumber);
                }
            }

            internal static class WinApi
            {
                [DllImport("Kernel32")]
                public static extern IntPtr LoadLibrary(string funcname);

                [DllImport("Kernel32")]
                public static extern IntPtr GetProcAddress(IntPtr handle, string funcname);
            }
        }
        /// <summary>
        /// 检查路径末尾分隔符并修正
        /// </summary>
        /// <param name="x">待检查修改路径</param>
        /// <returns>无返回</returns>
        public static void CheckPath(ref string x)
        {
            if (x.Substring(x.Length - 1, 1) != "/" && x.Substring(x.Length - 1, 1) != "\\") x += '/';
            return;
        }
    }
}
