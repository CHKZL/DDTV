using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.Tool.SystemResource;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.InteropServices;
using static DDTV_WEB_Server.Controllers.System_Info.Info;

namespace DDTV_WEB_Server.Controllers
{
    public class System_Info : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "System_Info")]
        public string Post([FromForm] string cmd)
        {
            int Downloading = 0, DownloadComplete = 0;
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadingList.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadingList)
                    {
                        Downloading++;
                    }
                }
                if (A1.Value.DownloadedLog.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadedLog)
                    {
                        DownloadComplete++;
                    }
                }
            }
           
            Info systemInfo = new Info()
            {
                DDTVCore_Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                Room_Quantity = Rooms.RoomInfo.Count,
                ServerAID = RuntimeConfig.ServerAID,
                ServerName = RuntimeConfig.ServerName,
                os_Info = new OS_Info()
                {
                    Associated_Users = Environment.UserName,
                    OS_Ver = RuntimeInformation.OSDescription,
                    OS_Tpye = RuntimeInformation.OSArchitecture.ToString(),
                    Memory_Usage = Process.GetCurrentProcess().WorkingSet64,
                    Runtime_Ver = RuntimeInformation.ProcessArchitecture.ToString(),
                    UserInteractive = Environment.UserInteractive,
                    Current_Directory = Environment.CurrentDirectory,
                    WebCore_Ver = Environment.Version.ToString(),
                    AppCore_Ver = RuntimeInformation.FrameworkDescription

                },
                download_Info = new Download_Info()
                {
                    Downloading = Downloading,
                    Completed_Downloads = DownloadComplete
                },
                
            };
            return MessageBase.Success(nameof(System_Info), systemInfo);
        }
        public class Info
        {
            /// <summary>
            /// 当前DDTV版本号
            /// </summary>
            public string DDTVCore_Ver { get; set; }
            /// <summary>
            /// 监控房间数量
            /// </summary>
            public int Room_Quantity { get; set; }
            /// <summary>
            /// 设置的服务器名称
            /// </summary>
            public string ServerName { get; set; }
            /// <summary>
            /// 服务器的唯一资源编号
            /// </summary>
            public string ServerAID { get; set; }
            /// <summary>
            /// 操作系统相关信息
            /// </summary>
            public OS_Info os_Info { get; set; }
            /// <summary>
            /// 下载任务基础信息
            /// </summary>
            public Download_Info download_Info { get; set; }
            public class OS_Info
            {
                /// <summary>
                /// 系统版本
                /// </summary>
                public string OS_Ver { get; set; }
                /// <summary>
                /// 系统类型
                /// </summary>
                public string OS_Tpye { get; set; }
                /// <summary>
                /// 使用内存量，单位bit
                /// </summary>
                public long Memory_Usage { get; set; }
                /// <summary>
                /// 运行时版本
                /// </summary>
                public string Runtime_Ver { get; set; }
                /// <summary>
                /// 是否在交互模式下
                /// </summary>
                public bool UserInteractive { get; set; }
                /// <summary>
                /// 关联的用户
                /// </summary>
                public string Associated_Users { get; set; }
                /// <summary>
                /// 工作目录
                /// </summary>
                public string Current_Directory { get; set; }
                /// <summary>
                /// Core程序核心框架版本
                /// </summary>
                public string AppCore_Ver { set; get; }
                /// <summary>
                /// Web程序核心框架版本
                /// </summary>
                public string WebCore_Ver { set; get; }
            }
            public class Download_Info
            {
                /// <summary>
                /// 下载中的任务数
                /// </summary>
                public int Downloading { get; set; }
                /// <summary>
                /// 下载结束的任务数
                /// </summary>
                public int Completed_Downloads { get; set; }
            }
        }
    }
    public class System_Config : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "SystemConfig")]
        //[Consumes("application/json")]
        public string Post([FromForm] string cmd)
        {
            return MessageBase.Success(nameof(System_Config), DDTV_Core.SystemAssembly.ConfigModule.CoreConfigClass.config.datas);
        }
    }
    public class System_Resources : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "System_Resources")]
        public string Post([FromForm] string cmd)
        {
            SystemResourceClass systemResourceClass = new();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                systemResourceClass = new()
                {
                    CPU_usage = GetCPUInfo.GetLinux(),
                    HDDInfo=GetHDDInfo.GetLinux(),
                    Memory=GetMemInfo.GetLiunx(),
                    Platform="Linux"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                systemResourceClass = new()
                {
                    CPU_usage = GetCPUInfo.GetWindows(),
                    Memory = GetMemInfo.GetWindows(),
                    Platform = "Windows"
                };
                string DriveLetter = Path.GetFullPath(DDTV_Core.SystemAssembly.DownloadModule.Download.DefaultPath)[..1];
                systemResourceClass.HDDInfo=GetHDDInfo.GetWindows(DriveLetter);
            }
                return MessageBase.Success(nameof(System_Resources), systemResourceClass);
        }
    }
}