using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Auxiliary
{
    /// <summary>
    /// 重构DDTV整个项目所使用的消息系统，用于区分DDTV和DDTVLIve以及WEB项目中不同等级的提示和提示方式
    /// </summary>
    public class InfoLog
    {
        public static FileStream InfoInitFS;
        public static StreamWriter InfoInitSW;
        public static InfoClasslBool ClasslBool = new InfoClasslBool();
        public class InfoClasslBool
        {
            public bool Debug { set; get; } = false;
            public bool 系统错误信息 { set; get; } = false;
            public bool 杂项提示 { set; get; } = false;
            public bool 下载必要提示 { set; get; } = false;
            public bool 没啥价值的消息 { set; get; } = false;
            public bool 输出到文件 { set; get; } = false;
        }
        public enum InfoClass
        {
            Debug = 0,
            系统错误信息 = 1,
            杂项提示 = 2,
            下载必要提示 = 3,
            没啥价值的消息 = 4
        }
        public static string 返回WSS连接状态列表()
        {
            string 返回字符串 = string.Empty;
            返回字符串 += "<meta http-equiv=\"refresh\" content=\"5\"><html><head><title>%title%</title></head><body>";
            int num = 0;
            foreach (var item in bilibili.已连接的直播间状态)
            {
                num++;
                TimeSpan ts = DateTime.Now.Subtract(item.心跳时间);
                返回字符串 += "<br/>"+ num + "　时间差:" + (int)ts.TotalSeconds + "　　　房间号:" + item.房间号 + "　　心跳值:" + item.心跳值 + "　　上次更新时间" + item.心跳时间 + "    服务器:" + item.连接服务器地址;
            }
            返回字符串 = (返回字符串 + "</body></html>").Replace("%title%", "WSS连接状态");
            return 返回字符串;
        }
        /// <summary>
        /// 返回下载列表HTML字符串
        /// </summary>
        /// <param name="模式">0为全体,1为下载中,2为已完成</param>
        /// <returns></returns>
        public static string DownloaderInfoPrintf(int 模式)
        {
            string 返回字符串 = string.Empty;            
            int 计数 = 1;
            int 完成数量 = 0;
            int 正在下载数量 = 0;
            if(模式==0)
            {
                返回字符串 += "<meta http-equiv=\"refresh\" content=\"5\"><html><head><title>%title%</title><meta charset=\"utfhttps://minigame.qq.com/gameinfo/10075.html#-8\"></head><body>";
            }
            List<string> 下载任务 = new List<string>();
            下载任务.Add(string.Format("<table border=\"5\"><tr><td>序号</td><td>房间号</td><td>下载状态</td><td>已下载大小</td><td>备注</td><td>主播名称</td><td>开始时间</td><td>结束时间</td></td>"));

            foreach (var item in MMPU.DownList)
            {
                string 开始时间 = MMPU.Unix转换为DateTime(item.DownIofo.开始时间.ToString()).ToString("yyyy/MM/dd HH:mm:ss");
                string 结束时间 = item.DownIofo.结束时间 > 0 ? MMPU.Unix转换为DateTime(item.DownIofo.结束时间.ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "";
                if(模式==0)
                {
                    下载任务.Add(string.Format("<tr><td>" + 计数 + "</td><td>" + item.DownIofo.房间_频道号 + "</td><td>" + (item.DownIofo.下载状态 ? "□正在下载" : "■下载完成")) + "</td><td> " + item.DownIofo.已下载大小str + "</td><td>" + item.DownIofo.备注 + "</td><td>" + item.DownIofo.主播名称 + "</td><td>" + 开始时间 + "</td><td>" + 结束时间 + "</td></td>");
                }
                else if(模式==1 && item.DownIofo.下载状态)
                {
                    下载任务.Add(string.Format("<tr><td>" + 计数 + "</td><td>" + item.DownIofo.房间_频道号 + "</td><td>" + (item.DownIofo.下载状态 ? "□正在下载" : "■下载完成")) + "</td><td> " + item.DownIofo.已下载大小str + "</td><td>" + item.DownIofo.备注 + "</td><td>" + item.DownIofo.主播名称 + "</td><td>" + 开始时间 + "</td><td>" + 结束时间 + "</td></td>");
                }
                else if (模式 == 2 && !item.DownIofo.下载状态)
                {
                    下载任务.Add(string.Format("<tr><td>" + 计数 + "</td><td>" + item.DownIofo.房间_频道号 + "</td><td>" + (item.DownIofo.下载状态 ? "□正在下载" : "■下载完成")) + "</td><td> " + item.DownIofo.已下载大小str + "</td><td>" + item.DownIofo.备注 + "</td><td>" + item.DownIofo.主播名称 + "</td><td>" + 开始时间 + "</td><td>" + 结束时间 + "</td></td>");
                }
                计数++;
                if (item.DownIofo.下载状态)
                {
                    正在下载数量++;
                }
                else
                {
                    完成数量++;
                }
            }

            for (int i = 0; i < 下载任务.Count; i++)
            {
                返回字符串 += 下载任务[i];
            }
            switch(模式)
            {
                case 0:
                    返回字符串 = (返回字符串 + "</table></body></html>").Replace("%title%", "正在下载:" + 正在下载数量 + " 已经完成:" + 完成数量);
                    break;
                case 1:
                    返回字符串 = 返回字符串 + "</table>";
                    break;
                case 2:
                    返回字符串 = 返回字符串 + "</table>";
                    break;
            }
            
            return 返回字符串;
        }
        public static void InfoInit(string LogFile, InfoClasslBool InfoLvevlBool)
        {
            if (!Directory.Exists("./LOG"))
            {
                Directory.CreateDirectory("./LOG");
            }
            LogFile = "./LOG/" + LogFile;
            string LogFileBak = "";
            if (File.Exists(LogFile))
            {
                LogFileBak = LogFile + ".backup_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                File.Move(LogFile, LogFileBak);
            }
            string[] filelist = Directory.GetFiles("./LOG");
            foreach (var item in filelist)
            {
                string name = item.Replace("\\", "/");
                if (name != LogFile && name != LogFileBak)
                {
                    MMPU.文件删除委托(item, "历史陈旧log日志删除");
                }
            }
            InfoInitFS = new FileStream(LogFile, FileMode.Append);
            InfoInitSW = new StreamWriter(InfoInitFS);
            ClasslBool = InfoLvevlBool;
        }
        public static void InfoPrintf(string mess, InfoClass Class)
        {
            string A = string.Empty;
            switch (Class)
            {
                case InfoClass.Debug:
                    {
                        if (ClasslBool.Debug)
                        {
                            if(MMPU.Debug打印到终端)
                            {
                                Console.WriteLine("[Debug] " + DateTime.Now.ToString("MM-dd HH:mm:ss") + ": " + mess);
                            }
                            A = "\r\n[Debug]" + DateTime.Now.ToString("MM-dd HH:mm:ss") + ": " + mess;

                        }
                    }
                    break;
                case InfoClass.系统错误信息:
                    {
                        if (ClasslBool.系统错误信息)
                        {
                            Console.WriteLine("\r\n ========= SysteErrorInfoBeginning =========" +
                                "\r\n[SysteErrorInfo]: " + mess +
                                "\r\n=========SysteErrorInfoEnd=========");

                            A = "\r\n=========SysteErrorInfoBeginning=========" + "\r\n[SysteErrorInfo]: " + mess + "\r\n=========SysteErrorInfoEnd=========";

                        }
                    }
                    break;
                case InfoClass.杂项提示:
                    {
                        if (ClasslBool.杂项提示)
                        {
                            Console.WriteLine("[Info]: " + mess);
                            A = "\r\n[Info]: " + mess;
                        }
                    }
                    break;
                case InfoClass.下载必要提示:
                    {
                        if (ClasslBool.下载必要提示)
                        {
                            Console.WriteLine("[下载系统消息]" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + mess);
                            A = "\r\n[下载系统消息]" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + mess;
                        }
                    }
                    break;
                case InfoClass.没啥价值的消息:
                    {
                        if (ClasslBool.没啥价值的消息)
                        {
                            Console.WriteLine("[没啥价值的消息]" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + mess);
                            A = "\r\n[没啥价值的消息]" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + mess;
                        }
                    }
                    break;
            }
            if (ClasslBool.输出到文件 && A.Length > 2)
            {
                OutToFile(A);
            }
        }
        public static void OutToFile(string str)
        {
            InfoInitSW.WriteLine(str);
            InfoInitSW.Flush();
        }

        public static string GetSystemInfo()
        {
            string BB = "<meta http-equiv=\"refresh\" content=\"5\"><html><body>";
            BB += $"系统时间{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
            int 下载中 = 0, 下载完成 = 0;
            foreach (var item in MMPU.DownList)
            {
                if (item.DownIofo.下载状态)
                    下载中++;
                else
                    下载完成++;
            }
            BB += "　　　此页面每5秒自动刷新一次<br/><br/>【DDTVLiveRec信息】";
            var proc = Process.GetCurrentProcess();
            var mem = proc.WorkingSet64;
            var cpu = proc.TotalProcessorTime;
            BB += "<br/>当前DDTVLiveRec核心版本：" + (MMPU.开发模式 ? MMPU.开发版本号 : MMPU.版本号);
            if(MMPU.开发模式)
            {
                BB += $"<br/>当前为开发模式，该开发版本和主流版本({MMPU.版本号})有如下修改<br/>";
                int i = 1;
                foreach (var item in MMPU.开发更改)
                {
                    BB += $"<br/>　　　{i}.{item}";
                    i++;
                }
                BB += $"<br/><br/>(如以上修改内容对你现在所遇到的问题对你有所帮助，请联系我，我将把开发版本编译一个正常工作的版本发送给你)<br/>";
            }
            if(MMPU.是否有新版本)
            {
                BB += "<br/>检测到云端有新版本。如需更新，请上github或加307156949" +
                    "<a target=\"_blank\" href=\"https://qm.qq.com/cgi-bin/qm/qr?k=uwgF9uM9BvSWcHWZu460S6tVcVH1N_YP&jump_from=webapi\"><img border=\"0\" src=\"//pub.idqqimg.com/wpa/images/group.png\" alt=\"每天都是单推的一天\" title=\"每天都是单推的一天\"></a>群共享下载";
                BB += $"<br/><br/>更新公告:{MMPU.更新公告.Replace("\r", "<br/>")}<br/><br/>";
            }
            BB += $"<br/><br/>---运行状态---<br/>下载中:{下载中}个  下载完成:{下载完成}个";
            BB += $"<br/>当前临时API监控房间数量:{bilibili.RoomList.Count - bilibili.已连接的直播间状态.Count},稳定WSS长连接监控房间数量:{bilibili.已连接的直播间状态.Count}";
            BB += $"<br/>当前进程已使用内存：{String.Format("{0:N2}", mem / 1024.0 / 1024.0)}MB,CPU运行时间：{cpu.TotalMilliseconds}";
            BB += $"<br/>当前进程物理内存量：{ String.Format("{0:N2}", Environment.WorkingSet / 1204.0 / 1024.0)}MB";
            try
            {
                BB += "<br/><br/>--系统运行平台--";
                BB += "<br/>运行框架：" + RuntimeInformation.FrameworkDescription;
                BB += "<br/>操作系统：" + RuntimeInformation.OSDescription;
                BB += "<br/>操作系统版本：" + Environment.OSVersion.ToString();
                BB += "<br/>平台架构：" + RuntimeInformation.OSArchitecture;
            }
            catch (Exception){}
            BB += "<br/><br/>";
            try
            {
                BB += "<br/>-- 运行环境 --";
                BB += $"<br/>系统已运行时间：{ Environment.TickCount / 1000}秒";
                BB += "<br/>是否在交互模式中运行：" + Environment.UserInteractive;
                BB += "<br/>当前关联用户名：" + Environment.UserName;
                BB += "<br/>Web程序核心框架版本：" + Environment.Version.ToString();
            }
            catch (Exception){}
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                BB += $"<br/><br/>---Windows信息---";
                string currentDirectory = Path.GetDirectoryName((new Program()).GetType().Assembly.Location);
                BB += $"<br/>程序运行路径：{currentDirectory}";
                //string HDBB = "<table border=\"5\"><tr><td>盘符</td><td>类型</td><td>文件系统</td><td>总容量</td><td>剩余容量</td></td>";
                //List<WindowsInfo.HDDInfo> B2 = WindowsInfo.获取硬盘剩余空间(currentDirectory.Split(':')[0]);
                //foreach (var item in B2)
                //{
                //    HDBB += $"<tr><td>{item.盘符}</td><td>{item.类型}</td><td>{item.文件系统}</td><td>{Downloader.转换下载大小数据格式(item.容量)}</td><td>{Downloader.转换下载大小数据格式(item.剩余)}</td></td>";
                //}
                //BB += $"<br/>"+HDBB;
                //BB += $"<br/><br/>---Linux信息---<br/>当前环境为OSPlatform.Windows，不获执行Linux查询";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //List<LinuxInfo.HDDInfo> HHDIN = LinuxInfo.ServerConfig.读取硬盘信息();
                string HDBB = "<table border=\"6\"><tr><td>文件系统</td><td>容量</td><td>已使用</td><td>剩余空间</td><td>使用率</td><td>挂载路径</td></td>";
                string[] B2 = LinuxInfo.ServerConfig.读取硬盘信息().Split('\n');
                for (int i = 1; i < B2.Length; i++)
                {
                    string[] B3 = B2[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (B3.Length > 5)
                        HDBB += $"<tr><td>{B3[0]}</td><td>{B3[1]}</td><td>{B3[2]}</td><td>{B3[3]}</td><td>{B3[4]}</td><td>{B3[5]}</td></td>";
                }
                //BB += $"<br/><br/>---Windows信息---<br/>当前环境为OSPlatform.Linux，不获执行Windows查询";
                BB += $"<br/><br/>---Linux信息---{HDBB}";
                //foreach (var item in HHDIN)
                //{
                //    BB += $"<br/>容量:{item.Size} 已使用:{item.Used} 剩余空间:{item.Avail}  使用率:{item.Usage}";
                //}
                LinuxInfo.MemInfo Men = LinuxInfo.ServerConfig.读取内存信息();
                BB += $"<br/>可使用内存:{Downloader.转换下载大小数据格式(double.Parse(Men.Available.Split(' ')[0]) * 1024.0)} 总内存大小:{Downloader.转换下载大小数据格式(double.Parse(Men.Total.Split(' ')[0]) * 1024.0)}</body></html>";
            }
            BB += InfoLog.DownloaderInfoPrintf(1);
            return BB;
        }
        public class WindowsInfo
        {
            /// <summary>
            /// 获取硬盘剩余空间
            /// </summary>
            /// <returns></returns>
            public static List<HDDInfo> 获取硬盘剩余空间(string P)
            {
                List<HDDInfo> HDD = new List<HDDInfo>();
                var BB = Directory.GetLogicalDrives();
                DriveInfo drive = new DriveInfo(P+":\\");

                HDD.Add(new HDDInfo() { 盘符 = drive.Name, 容量 = drive.TotalSize, 类型 = drive.DriveType.ToString(), 文件系统 = drive.DriveFormat, 剩余 = drive.AvailableFreeSpace });

                return HDD;
            }
            public class HDDInfo
            {
                public string 盘符 { set; get; }
                public string 类型 { set; get; }
                public string 文件系统 { set; get; }
                public double 容量 { set; get; }
                public double 剩余 { set; get; }
            } 
        }
        public class LinuxInfo
        {
            public class ServerConfig
            {
                //static private log4net.ILog log = log4net.LogManager.GetLogger(typeof(ServerConfig));
                /// <summary>
                /// 获取Linux服务器资源信息
                /// </summary>
                private const string NETWORK_CONFIG_FILE_PATH = @"/etc/NetworkManager/system-connections/";
                private static string logs_service_port = ConfigurationManager.AppSettings["logs_service_port"]; //网口 配置文件中获取 
                /// <summary>
                /// 获取网关 IP信息
                /// </summary>
                /// <returns></returns>
                public static NetworkInfo 获取网关IP信息()
                {
                    NetworkInfo networkInfo = new NetworkInfo();
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo("ifconfig", logs_service_port)
                        {
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        }
                    };
                    process.Start();
                    var hddInfo = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    process.Dispose();

                    var lines = hddInfo.Split('\n');
                    foreach (var item in lines)
                    {
                        if (item.Contains("inet"))
                        {
                            var li = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            // inet 192.168.122.1  netmask 255.255.255.0  broadcast 192.168.122.255
                            networkInfo.IPAddress = li[1];
                            networkInfo.SubnetMark = li[3];
                            networkInfo.Gateway = li[5];
                            break;
                        }
                    }
                    return networkInfo;
                }

                /// <summary>
                /// 读取服务器配置
                /// </summary>
                /// <returns></returns>
                public static NetworkInfo 读取服务器配置()
                {
                    NetworkInfo networkInfo = new NetworkInfo();
                    var config_file = 获取服务器网络配置信息();
                    try
                    {
                        if (!string.IsNullOrEmpty(config_file))
                        {
                            ConfigFile config = new ConfigFile(config_file);

                            var setting_group = config.SettingGroups;
                            var ip_info = setting_group["ipv4"].Settings;
                            networkInfo.DNS = ip_info["dns"].GetValue();

                            string address_info = ip_info["address1"].GetValue();
                            var address = address_info.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            networkInfo.IPAddress = address[0];

                            var gateway = address[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            networkInfo.SubnetMark = NetworkConfig.GetSubnetMarkString(int.Parse(gateway[0]));
                            networkInfo.Gateway = gateway[1];

                            if (setting_group.ContainsKey("ethernet"))
                            {
                                ip_info = setting_group["ethernet"].Settings;
                                if (ip_info.ContainsKey("mac-address"))
                                {
                                    networkInfo.MacAddress = ip_info["mac-address"].GetValue();
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    return networkInfo;
                }

                /// <summary>
                /// 保存网络设置信息
                /// </summary>
                /// <param name="networkInfo"></param>
                public static void 保存网络设置信息(NetworkInfo networkInfo)
                {
                    var config_file = 获取服务器网络配置信息();
                    if (!string.IsNullOrEmpty(config_file))
                    {
                        ConfigFile config = new ConfigFile(config_file);
                        var ip_mark_gateway = string.Format("{0}/{1},{2}", networkInfo.IPAddress,
                                            NetworkConfig.GetSubnetMarkString(NetworkConfig.SubnetMarkValue(networkInfo.SubnetMark)),
                                            networkInfo.Gateway);

                        var ip_address = config.SettingGroups["ipv4"].Settings;
                        ip_address["dns"].SetValue(networkInfo.DNS);
                        ip_address["address1"].SetValue(ip_mark_gateway);
                        config.Save(config_file);
                    }
                }

                /// <summary>
                /// 获取服务器网络配置信息
                /// </summary>
                /// <returns></returns>
                private static string 获取服务器网络配置信息()
                {
                    var files = Directory.GetFiles(NETWORK_CONFIG_FILE_PATH);
                    string config = string.Empty;
                    if (files != null && files.Length > 0)
                    {
                        config = files[0];
                    }
                    return config;
                }

                /// <summary>
                /// 读取CPU序列号信息
                /// </summary>
                /// <returns></returns>
                public static string 读取CPU序列号信息()
                {
                    const string CPU_FILE_PATH = "/proc/cpuinfo";
                    var s = File.ReadAllText(CPU_FILE_PATH);
                    var lines = s.Split(new[] { '\n' });
                    s = string.Empty;

                    foreach (var item in lines)
                    {
                        if (item.StartsWith("Serial"))
                        {
                            var temp = item.Split(new[] { ':' });
                            s = temp[1].Trim();
                            break;
                        }
                    }
                    return s;
                }

                /// <summary>
                /// 重启服务器命令
                /// </summary>
                public static void 重启服务器命令()
                {
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo("reboot");
                    process.Start();
                    process.WaitForExit();
                    process.Dispose();
                }

                /// <summary>
                /// 读取内存信息
                /// </summary>
                /// <returns></returns>
                public static MemInfo 读取内存信息()
                {
                    MemInfo memInfo = new MemInfo();
                    const string CPU_FILE_PATH = "/proc/meminfo";
                    var mem_file_info = File.ReadAllText(CPU_FILE_PATH);
                    var lines = mem_file_info.Split(new[] { '\n' });
                    mem_file_info = string.Empty;

                    int count = 0;
                    foreach (var item in lines)
                    {
                        if (item.StartsWith("MemTotal:"))
                        {
                            count++;
                            var tt = item.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            memInfo.Total = tt[1].Trim();
                        }
                        else if (item.StartsWith("MemAvailable:"))
                        {
                            count++;
                            var tt = item.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            memInfo.Available = tt[1].Trim();
                        }
                        if (count >= 2) break;
                    }
                    return memInfo;
                }

                /// <summary>
                /// 同步系统时间
                /// </summary>
                public static void 同步系统时间()
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            using (var process = new Process())
                            {
                                process.StartInfo = new ProcessStartInfo("ntpdate", "ntp1.aliyun.com");
                                process.Start();
                                process.WaitForExit(50000);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    });
                }

                /// <summary>
                /// 读取系统时间
                /// </summary>
                /// <param name="time"></param>
                public static void 读取系统时间(DateTime time)
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo("date", $"-s \"{time.ToString("yyyy-MM-dd HH:mm:ss")}\"");
                        process.Start();
                        process.WaitForExit(3000);
                    }
                }

                /// <summary>
                /// 读取硬盘信息
                /// </summary>
                /// <returns></returns>
                public static string 读取硬盘信息()
                {
                    List<HDDInfo> hdd = null;
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo("df", "-h")
                        {
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        }
                    };
                    process.Start();
                    var hddInfo = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    process.Dispose();

                    var lines = hddInfo.Split('\r');
                    return hddInfo;
                    foreach (var item in lines)
                    {

                        if (item.Contains("/"))
                        {
                            var li = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < li.Length; i++)
                            {
                                if (li[i].Contains("%"))
                                {
                                    hdd.Add(new HDDInfo()
                                    {
                                        Size = li[i - 3],
                                        Used = li[i - 2],
                                        Avail = li[i - 1],
                                        Usage = li[i]
                                    });
                                    break;
                                }
                            }
                        }
                    }
                    return null;
                }

                /// <summary>
                /// 读取CPU温度信息
                /// </summary>
                /// <returns></returns>
                public static float 读取CPU温度信息()
                {
                    const string CPU_Path = "/sys/class/thermal/thermal_zone0/temp";
                    var values = File.ReadAllText(CPU_Path);
                    float valuef = float.Parse(values);
                    return valuef / 1000f;
                }

                /// <summary>
                /// 读取CPU使用率信息
                /// </summary>
                /// <returns></returns>
                public static int 读取CPU使用率信息()
                {
                    float value = 0f;
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo("top", "-b -n1")
                    };
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    var cpuInfo = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    process.Dispose();

                    var lines = cpuInfo.Split('\n');
                    bool flags = false;
                    foreach (var item in lines)
                    {
                        if (!flags)
                        {
                            if (item.Contains("PID USER"))
                            {
                                flags = true;
                            }
                        }
                        else
                        {
                            var li = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < li.Length; i++)
                            {
                                if (li[i] == "R" || li[i] == "S")
                                {
                                    value += float.Parse(li[i + 1]);
                                    break;
                                }
                            }
                        }
                    }
                    int r = (int)(value / 4f);
                    if (r > 100) r = 100;
                    return r;
                }

                /// <summary>
                /// 字符处理
                /// </summary>
                /// <param name="strings"></param>
                /// <returns></returns>
                private static string TrimString(string strings)
                {
                    char[] tempchat = new char[strings.Length];
                    int n = 0;
                    foreach (var item in strings)
                    {
                        if ((item >= 32 && item <= 126) || item == '\n' || item == ' ')
                        {
                            tempchat[n++] = item;
                        }
                    }
                    return new String(tempchat, 0, n);
                }

                ///// <summary>
                ///// 获取服务器运行时信息
                ///// </summary>
                ///// <returns></returns>
                //public static ServerInfo 获取服务器运行时信息()
                //{
                //    var serverInfo = new ServerInfo
                //    {
                //        HDDInfo = 读取硬盘信息(),
                //        MemInfo = 读取内存信息(),
                //       // NetworkInfo = 获取网关IP信息(),
                //        CpuSerialNumber = 读取CPU序列号信息()
                //    };

                //    serverInfo.CpuTemperature = 读取CPU温度信息();

                //    // serverInfo.CpuTemperature = 24;

                //    serverInfo.CpuUsage = 读取CPU使用率信息();
                //    //从rdis取PacketCount、SessionCount
                //    serverInfo.PacketCount = 0;
                //    serverInfo.SessionCount = 0;
                //    return serverInfo;
                //}
                //public static string GetInfo()
                //{
                //    ServerInfo A = 获取服务器运行时信息();
                //    return JsonConvert.SerializeObject(A);
                //}
            }
            public class NetworkConfig
            {

                /// <summary>
                /// 获取子掩码
                /// </summary>
                /// <param name="mark"></param>
                /// <returns></returns>
                public static int GetSubnetMarkValue(int mark)
                {
                    int value = 0;
                    uint temp_value = 0x80000000;
                    for (int i = 0; i < 32; i++)
                    {
                        if ((mark & temp_value) != 0)
                        {
                            value++;
                        }
                        temp_value >>= 1;
                    }
                    return value;
                }

                /// <summary>
                /// 获取子掩码
                /// </summary>
                /// <param name="value"></param>
                /// <returns></returns>
                public static string GetSubnetMarkString(int value)
                {
                    int temp_value = 0;
                    int temp = unchecked((int)0x80000000);
                    for (int i = 0; i < value; i++)
                    {
                        temp_value |= temp;
                        temp >>= 1;
                    }
                    return SubnetMarkString(temp_value);
                }

                /// <summary>
                /// 子掩码数值
                /// </summary>
                /// <param name="code"></param>
                /// <returns></returns>
                public static int SubnetMarkValue(string code)
                {
                    var array_value = code.Split('.');
                    byte[] byte_array = new byte[4];
                    for (int i = 0; i < byte_array.Length; i++)
                    {
                        byte_array[3 - i] = Convert.ToByte(array_value[i]);
                    }
                    return BitConverter.ToInt32(byte_array, 0);
                }

                /// <summary>
                /// 子掩码
                /// </summary>
                /// <param name="code"></param>
                /// <returns></returns>
                private static string SubnetMarkString(int code)
                {
                    var byte_value = BitConverter.GetBytes(code);
                    string[] string_value = new string[4];
                    for (int i = 0; i < byte_value.Length; i++)
                    {
                        string_value[i] = byte_value[3 - i].ToString();
                    }
                    return string.Join(".", string_value);
                }
            }
            public class HDDInfo
            {
                /// <summary>
                /// 硬盘大小
                /// </summary>
                public string Size { get; set; }

                /// <summary>
                /// 已使用大小
                /// </summary>
                public string Used { get; set; }

                /// <summary>
                /// 可用大小
                /// </summary>
                public string Avail { get; set; }

                /// <summary>
                /// 使用率
                /// </summary>
                public string Usage { get; set; }
            }
            public class MemInfo
            {
                /// <summary>
                /// 总计内存大小
                /// </summary>
                public string Total { get; set; }
                /// <summary>
                /// 可用内存大小
                /// </summary>
                public string Available { get; set; }
            }
            public class NetworkInfo
            {
                /// <summary>
                /// IP地址
                /// </summary>
                public string IPAddress { get; set; }

                /// <summary>
                /// 子掩码
                /// </summary>
                public string SubnetMark { get; set; }

                /// <summary>
                /// 网关
                /// </summary>
                public string Gateway { get; set; }

                /// <summary>
                /// DNS信息
                /// </summary>
                public string DNS { get; set; }

                /// <summary>
                /// MAC地址
                /// </summary>
                public string MacAddress { get; set; }
            }
            public class RunTimeInfo
            {
                /// <summary>
                /// 最后登录时间
                /// </summary>
                public string LastLogon { get; set; } = string.Empty;

                /// <summary>
                /// 用户名
                /// </summary>
                public string UserName { get; set; } = string.Empty;

                /// <summary>
                /// CPU序列号
                /// </summary>
                public string CpuSerialNo { get; set; } = string.Empty;

                /// <summary>
                /// 启动时间
                /// </summary>
                public DateTime Startup { get; set; } = DateTime.Now;

                /// <summary>
                /// 魔方Token
                /// </summary>
                public string MagicToken { get; set; } = string.Empty;

                /// <summary>
                /// MAC地址
                /// </summary>
                public string MacAddress { get; set; } = string.Empty;

                /// <summary>
                /// 数据包大小
                /// </summary>
                public long PacketCount { get; set; } = 0;

                /// <summary>
                /// TCP连接数据
                /// </summary>
                public int TcpSessionCount { get; set; } = 0;
            }
            public class ServerInfo
            {
                /// <summary>
                /// 内存
                /// </summary>
                public MemInfo MemInfo { get; set; }

                /// <summary>
                ///硬盘信息
                /// </summary>
                public HDDInfo HDDInfo { get; set; }

                /// <summary>
                /// 网络信息
                /// </summary>
                public NetworkInfo NetworkInfo { get; set; }

                /// <summary>
                /// CPU序列号
                /// </summary>
                public string CpuSerialNumber { get; set; }

                /// <summary>
                /// CPU温度
                /// </summary>
                public float CpuTemperature { get; set; }

                /// <summary>
                /// CPU使用率
                /// </summary>
                public int CpuUsage { get; set; }

                /// <summary>
                /// 接包数据
                /// </summary>
                public long PacketCount { get; set; }

                /// <summary>
                /// 当前会话连接数
                /// </summary>
                public int SessionCount { get; set; }
                /// <summary>
                /// 写入时间
                /// </summary>
                public DateTime create_time { get; set; }
            }
            public class ConfigFile
            {

                /// <summary>
                /// Gets the groups found in the configuration file.
                /// </summary>
                public Dictionary<string, SettingsGroup> SettingGroups { get; private set; }

                /// <summary>
                /// Creates a blank configuration file.
                /// </summary>
                public ConfigFile()
                {
                    SettingGroups = new Dictionary<string, SettingsGroup>();
                }

                /// <summary>
                /// Loads a configuration file.
                /// </summary>
                /// <param name="file">The filename where the configuration file can be found.</param>
                public ConfigFile(string file)
                {
                    Load(file);
                }

                /// <summary>
                /// Loads a configuration file.
                /// </summary>
                /// <param name="file">The stream from which to load the configuration file.</param>
                public ConfigFile(Stream stream)
                {
                    Load(stream);
                }

                /// <summary>
                /// Adds a new settings group to the configuration file.
                /// </summary>
                /// <param name="groupName">The name of the group.</param>
                /// <returns>The newly created SettingsGroup.</returns>
                public SettingsGroup AddSettingsGroup(string groupName)
                {
                    if (SettingGroups.ContainsKey(groupName))
                        throw new Exception("Group already exists with name '" + groupName + "'");

                    SettingsGroup group = new SettingsGroup(groupName);
                    SettingGroups.Add(groupName, group);

                    return group;
                }

                /// <summary>
                /// Deletes a settings group from the configuration file.
                /// </summary>
                /// <param name="groupName">The name of the group to delete.</param>
                public void DeleteSettingsGroup(string groupName)
                {
                    SettingGroups.Remove(groupName);
                }

                /// <summary>
                /// Loads the configuration from a file.
                /// </summary>
                /// <param name="file">The file from which to load the configuration.</param>
                public void Load(string file)
                {
                    using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        Load(stream);
                    }
                }

                /// <summary>
                /// Loads the configuration from a stream.
                /// </summary>
                /// <param name="stream">The stream from which to read the configuration.</param>
                public void Load(Stream stream)
                {
                    //track line numbers for exceptions
                    int lineNumber = 0;

                    //groups found
                    List<SettingsGroup> groups = new List<SettingsGroup>();

                    //current group information
                    string currentGroupName = null;
                    List<Setting> settings = null;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            lineNumber++;

                            //strip out comments
                            if (line.Contains("#"))
                            {
                                if (line.IndexOf("#") == 0)
                                    continue;

                                line = line.Substring(0, line.IndexOf("#"));
                            }

                            //trim off any extra whitespace
                            line = line.Trim();

                            //try to match a group name
                            Match match = Regex.Match(line, "\\[[a-zA-Z\\d\\s]+\\]");

                            //found group name
                            if (match.Success)
                            {
                                //if we have a current group we're on, we save it
                                if (settings != null && currentGroupName != null)
                                    groups.Add(new SettingsGroup(currentGroupName, settings));

                                //make sure the name exists
                                if (match.Value.Length == 2)
                                    throw new Exception(string.Format("Group must have name (line {0})", lineNumber));

                                //set our current group information
                                currentGroupName = match.Value.Substring(1, match.Length - 2);
                                settings = new List<Setting>();
                            }

                            //no group name, check for setting with equals sign
                            else if (line.Contains("="))
                            {
                                //split the line
                                string[] parts = line.Split(new[] { '=' }, 2);

                                //if we have any more than 2 parts, we have a problem
                                if (parts.Length != 2)
                                    throw new Exception(string.Format("Settings must be in the format 'name = value' (line {0})", lineNumber));

                                //trim off whitespace
                                parts[0] = parts[0].Trim();
                                parts[1] = parts[1].Trim();

                                //figure out if we have an array or not
                                bool isArray = false;
                                bool inString = false;

                                //go through the characters
                                foreach (char c in parts[1])
                                {
                                    //any comma not in a string makes us creating an array
                                    if (c == ',' && !inString)
                                        isArray = true;

                                    //flip the inString value each time we hit a quote
                                    else if (c == '"')
                                        inString = !inString;
                                }

                                //if we have an array, we have to trim off whitespace for each item and
                                //do some checking for boolean values.
                                if (isArray)
                                {
                                    //split our value array
                                    string[] pieces = parts[1].Split(',');

                                    //need to build a new string
                                    StringBuilder builder = new StringBuilder();

                                    for (int i = 0; i < pieces.Length; i++)
                                    {
                                        //trim off whitespace
                                        string s = pieces[i].Trim();

                                        //convert to lower case
                                        string t = s.ToLower();

                                        //check for any of the true values
                                        if (t == "on" || t == "yes" || t == "true")
                                            s = "true";

                                        //check for any of the false values
                                        else if (t == "off" || t == "no" || t == "false")
                                            s = "false";

                                        //append the value
                                        builder.Append(s);

                                        //if we are not on the last value, add a comma
                                        if (i < pieces.Length - 1)
                                            builder.Append(",");
                                    }

                                    //save the built string as the value
                                    parts[1] = builder.ToString();
                                }

                                //if not an array
                                else
                                {
                                    //make sure we are not working with a string value
                                    if (!parts[1].StartsWith("\""))
                                    {
                                        //convert to lower
                                        string t = parts[1].ToLower();

                                        //check for any of the true values
                                        if (t == "on" || t == "yes" || t == "true")
                                            parts[1] = "true";

                                        //check for any of the false values
                                        else if (t == "off" || t == "no" || t == "false")
                                            parts[1] = "false";
                                    }
                                }

                                //add the setting to our list making sure, once again, we have stripped
                                //off the whitespace
                                settings.Add(new Setting(parts[0].Trim(), parts[1].Trim(), isArray));
                            }
                        }
                    }

                    //make sure we save off the last group
                    if (settings != null && currentGroupName != null)
                        groups.Add(new SettingsGroup(currentGroupName, settings));

                    //create our new group dictionary
                    SettingGroups = new Dictionary<string, SettingsGroup>();

                    //add each group to the dictionary
                    foreach (SettingsGroup group in groups)
                    {
                        SettingGroups.Add(group.Name, group);
                    }
                }

                /// <summary>
                /// Saves the configuration to a file
                /// </summary>
                /// <param name="filename">The filename for the saved configuration file.</param>
                public void Save(string filename)
                {
                    using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        Save(stream);
                    }
                }

                /// <summary>
                /// Saves the configuration to a stream.
                /// </summary>
                /// <param name="stream">The stream to which the configuration will be saved.</param>
                public void Save(Stream stream)
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        foreach (KeyValuePair<string, SettingsGroup> groupValue in SettingGroups)
                        {
                            writer.WriteLine("[{0}]", groupValue.Key);
                            foreach (KeyValuePair<string, Setting> settingValue in groupValue.Value.Settings)
                            {
                                writer.WriteLine("{0}={1}", settingValue.Key, settingValue.Value.RawValue);
                            }
                            writer.WriteLine();
                        }
                    }
                }
            }
            public class Setting
            {
                /// <summary>
                /// Gets the name of the setting.
                /// </summary>
                public string Name { get; private set; }

                /// <summary>
                /// Gets the raw value of the setting.
                /// </summary>
                public string RawValue { get; private set; }

                /// <summary>
                /// Gets whether or not the setting is an array.
                /// </summary>
                public bool IsArray { get; private set; }

                internal Setting(string name)
                {
                    Name = name;
                    RawValue = string.Empty;
                    IsArray = false;
                }

                internal Setting(string name, string value, bool isArray)
                {
                    Name = name;
                    RawValue = value;
                    IsArray = isArray;
                }

                /// <summary>
                /// Attempts to return the setting's value as an integer.
                /// </summary>
                /// <returns>An integer representation of the value</returns>
                public int GetValueAsInt()
                {
                    return int.Parse(RawValue, CultureInfo.InvariantCulture.NumberFormat);
                }

                /// <summary>
                /// Attempts to return the setting's value as a float.
                /// </summary>
                /// <returns>A float representation of the value</returns>
                public float GetValueAsFloat()
                {
                    return float.Parse(RawValue, CultureInfo.InvariantCulture.NumberFormat);
                }

                /// <summary>
                /// Attempts to return the setting's value as a bool.
                /// </summary>
                /// <returns>A bool representation of the value</returns>
                public bool GetValueAsBool()
                {
                    return bool.Parse(RawValue);
                }

                /// <summary>
                /// Attempts to return the setting's value as a string.
                /// </summary>
                /// <returns>A string representation of the value</returns>
                public string GetValueAsString()
                {
                    ;

                    return RawValue;
                }

                /// <summary>
                /// Attempts to return the setting's value as an array of integers.
                /// </summary>
                /// <returns>An integer array representation of the value</returns>
                public int[] GetValueAsIntArray()
                {
                    string[] parts = RawValue.Split(',');

                    int[] valueParts = new int[parts.Length];

                    for (int i = 0; i < parts.Length; i++)
                        valueParts[i] = int.Parse(parts[i], CultureInfo.InvariantCulture.NumberFormat);

                    return valueParts;
                }

                /// <summary>
                /// Attempts to return the setting's value as an array of floats.
                /// </summary>
                /// <returns>An float array representation of the value</returns>
                public float[] GetValueAsFloatArray()
                {
                    string[] parts = RawValue.Split(',');

                    float[] valueParts = new float[parts.Length];

                    for (int i = 0; i < parts.Length; i++)
                        valueParts[i] = float.Parse(parts[i], CultureInfo.InvariantCulture.NumberFormat);

                    return valueParts;
                }

                /// <summary>
                /// Attempts to return the setting's value as an array of bools.
                /// </summary>
                /// <returns>An bool array representation of the value</returns>
                public bool[] GetValueAsBoolArray()
                {
                    string[] parts = RawValue.Split(',');

                    bool[] valueParts = new bool[parts.Length];

                    for (int i = 0; i < parts.Length; i++)
                        valueParts[i] = bool.Parse(parts[i]);

                    return valueParts;
                }

                /// <summary>
                /// Attempts to return the setting's value as an array of strings.
                /// </summary>
                /// <returns>An string array representation of the value</returns>
                public string[] GetValueAsStringArray()
                {
                    Match match = Regex.Match(RawValue, "[\\\"][^\\\"]*[\\\"][,]*");

                    List<string> values = new List<string>();

                    while (match.Success)
                    {
                        string value = match.Value;
                        if (value.EndsWith(","))
                            value = value.Substring(0, value.Length - 1);

                        value = value.Substring(1, value.Length - 2);
                        values.Add(value);
                        match = match.NextMatch();
                    }

                    return values.ToArray();
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new value to store.</param>
                public void SetValue(int value)
                {
                    RawValue = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new value to store.</param>
                public void SetValue(float value)
                {
                    RawValue = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new value to store.</param>
                public void SetValue(bool value)
                {
                    RawValue = value.ToString();
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new value to store.</param>
                public void SetValue(string value)
                {
                    RawValue = assertStringQuotes(value);
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new values to store.</param>
                public void SetValue(params int[] values)
                {
                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < values.Length; i++)
                    {
                        builder.Append(values[i].ToString(CultureInfo.InvariantCulture.NumberFormat));
                        if (i < values.Length - 1)
                            builder.Append(",");
                    }

                    RawValue = builder.ToString();
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new values to store.</param>
                public void SetValue(params float[] values)
                {
                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < values.Length; i++)
                    {
                        builder.Append(values[i].ToString(CultureInfo.InvariantCulture.NumberFormat));
                        if (i < values.Length - 1)
                            builder.Append(",");
                    }

                    RawValue = builder.ToString();
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new values to store.</param>
                public void SetValue(params bool[] values)
                {
                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < values.Length; i++)
                    {
                        builder.Append(values[i]);
                        if (i < values.Length - 1)
                            builder.Append(",");
                    }

                    RawValue = builder.ToString();
                }

                /// <summary>
                /// Sets the value of the setting.
                /// </summary>
                /// <param name="value">The new values to store.</param>
                public void SetValue(params string[] values)
                {
                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < values.Length; i++)
                    {
                        builder.Append(assertStringQuotes(values[i]));
                        if (i < values.Length - 1)
                            builder.Append(",");
                    }

                    RawValue = builder.ToString();
                }

                private static string assertStringQuotes(string value)
                {
                    return value;
                }

                public string GetValue()
                {
                    return RawValue;
                }
            }
            public class SettingsGroup
            {
                /// <summary>
                /// Gets the name of the group.
                /// </summary>
                public string Name { get; private set; }

                /// <summary>
                /// Gets the settings found in the group.
                /// </summary>
                public Dictionary<string, Setting> Settings { get; private set; }

                internal SettingsGroup(string name)
                {
                    Name = name;
                    Settings = new Dictionary<string, Setting>();
                }

                internal SettingsGroup(string name, List<Setting> settings)
                {
                    Name = name;
                    Settings = new Dictionary<string, Setting>();

                    foreach (Setting setting in settings)
                        Settings.Add(setting.Name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The value of the setting.</param>
                public void AddSetting(string name, int value)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(value);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The value of the setting.</param>
                public void AddSetting(string name, float value)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(value);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The value of the setting.</param>
                public void AddSetting(string name, bool value)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(value);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The value of the setting.</param>
                public void AddSetting(string name, string value)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(value);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The values of the setting.</param>
                public void AddSetting(string name, params int[] values)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(values);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The values of the setting.</param>
                public void AddSetting(string name, params float[] values)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(values);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The values of the setting.</param>
                public void AddSetting(string name, params bool[] values)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(values);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Adds a setting to the group.
                /// </summary>
                /// <param name="name">The name of the setting.</param>
                /// <param name="value">The values of the setting.</param>
                public void AddSetting(string name, params string[] values)
                {
                    Setting setting = new Setting(name);
                    setting.SetValue(values);
                    Settings.Add(name, setting);
                }

                /// <summary>
                /// Deletes a setting from the group.
                /// </summary>
                /// <param name="name">The name of the setting to delete.</param>
                public void DeleteSetting(string name)
                {
                    Settings.Remove(name);
                }
            }
        }
    }
}
