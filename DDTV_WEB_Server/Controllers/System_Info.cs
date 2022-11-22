using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.Log;
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
        public ActionResult Post([FromForm] string cmd)
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
                ServerAID = WebServerConfig.ServerAID,
                ServerName = WebServerConfig.ServerName,
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
            return Content(MessageBase.Success(nameof(System_Info), systemInfo), "application/json");
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
        [HttpPost(Name = "System_Config")]
        //[Consumes("application/json")]
        public ActionResult Post([FromForm] string cmd)
        {
            ConfigData configData = new ConfigData();
            List<CoreConfigClass.Key> ToRuleOut = new List<CoreConfigClass.Key>()
            { 
                CoreConfigClass.Key.WebPassword,
                CoreConfigClass.Key.WebUserName,
                CoreConfigClass.Key.WEB_API_SSL,
                CoreConfigClass.Key.pfxFileName,
                CoreConfigClass.Key.pfxPasswordFileName,
                CoreConfigClass.Key.AccessKeyId,
                CoreConfigClass.Key.AccessKeySecret,
                CoreConfigClass.Key.HighRiskWebAPIFixedCheckSign,
            };
            foreach (var item in CoreConfigClass.config.datas)
            {
                if (!ToRuleOut.Contains(item.Key))
                {
                    configData.datas.Add(item);
                }
            }
            return Content(MessageBase.Success(nameof(System_Config), configData.datas), "application/json");
        }
    }
    public class ConfigData
    {
        public List<CoreConfigClass.Config.Data> datas = new();
    }
    public class System_Resources : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "System_Resources")]
        public ActionResult Post([FromForm] string cmd)
        {
            SystemResourceClass systemResourceClass = new();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                systemResourceClass = new()
                {
                    CPU_usage = GetCPUInfo.GetLinux(),
                    HDDInfo = GetHDDInfo.GetLinux(),
                    Memory = GetMemInfo.GetLiunx(),
                    Platform = "Linux"
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
                string DriveLetter = Path.GetFullPath(DDTV_Core.SystemAssembly.DownloadModule.Download.DownloadPath)[..1];
                systemResourceClass.HDDInfo = GetHDDInfo.GetWindows(DriveLetter);
            }
            return Content(MessageBase.Success(nameof(System_Resources), systemResourceClass), "application/json");
        }
    }
    public class System_QueryWebFirstStart : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "System_QueryWebFirstStart")]
        //[Consumes("application/json")]
        public ActionResult Post([FromForm] string cmd)
        {
            return Content(MessageBase.Success(nameof(System_QueryWebFirstStart), CoreConfig.WEB_FirstStart), "application/json");
        }
    }

    public class System_GetUpdateLog : ProcessingControllerBase.ApiControllerBase
    {
        /// <summary>
        /// 获取更新公告
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost(Name = "System_GetUpdateLog")]
        //[Consumes("application/json")]
        public ActionResult Post([FromForm] string cmd)
        {
            return Content(MessageBase.Success(nameof(System_GetUpdateLog), DDTV_Core.InitDDTV_Core.UpdateNotice), "application/json");
        }
    }

    public class System_Log : ProcessingControllerBase.ApiControllerBase
    {
        /// <summary>
        /// 获取历史日志
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="page">第几页</param>
        /// <param name="Quantity">每页多少条</param>
        /// <returns></returns>
        [HttpPost(Name = "System_Log")]
        public ActionResult Post([FromForm] string cmd, [FromForm] int page, [FromForm] int Quantity)
        {
            if(page>0&& Quantity>0)
            {
                Log logs = new Log()
                {
                    TotalLogs = DDTV_Core.SystemAssembly.Log.Log.LogList.Count()
                };
                for (int i = (page - 1) * Quantity; i < page * Quantity; i++)
                {
                    if (DDTV_Core.SystemAssembly.Log.Log.LogList.Count > i)
                    {
                        LogClass logClass = new LogClass()
                        {
                            IsDisplay = DDTV_Core.SystemAssembly.Log.Log.LogList[i].IsDisplay,
                            IsError = DDTV_Core.SystemAssembly.Log.Log.LogList[i].IsError,
                            Message = DDTV_Core.SystemAssembly.Log.Log.LogList[i].Message,
                            RunningTime = DDTV_Core.SystemAssembly.Log.Log.LogList[i].RunningTime,
                            Source = DDTV_Core.SystemAssembly.Log.Log.LogList[i].Source,
                            Time = DDTV_Core.SystemAssembly.Log.Log.LogList[i].Time,
                            Type = DDTV_Core.SystemAssembly.Log.Log.LogList[i].Type,
                        };
                        logs.Logs.Add(DDTV_Core.SystemAssembly.Log.Log.LogList[i]);
                    }
                }
                return Content(MessageBase.Success(nameof(System_Log), logs), "application/json");
            }
            else
            {
                return Content(MessageBase.Success(nameof(System_Log), false, $"输入的参数取值范围有误"), "application/json");
            }
        }
        public class Log
        {
            /// <summary>
            /// 总日志条数
            /// </summary>
            public long TotalLogs { get; set; }
            /// <summary>
            /// 查询量的日志信息
            /// </summary>
            public List<LogClass> Logs { get; set; } = new List<LogClass>();
        }
    }

    public class System_LatestLog : ProcessingControllerBase.ApiControllerBase
    {
        /// <summary>
        /// 获取最新日志
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="Quantity">最新的多少条</param>
        /// <returns></returns>
        [HttpPost(Name = "System_LatestLog")]
        public ActionResult Post([FromForm] string cmd, [FromForm] int Quantity)
        {
            List<LogClass> Logs = new List<LogClass>();

            if (Quantity > 0)
            {
                int Count = DDTV_Core.SystemAssembly.Log.Log.LogList.Count();
                for (int i = 0; i < Quantity; i++)
                {
                    if(Count - 1 - i>0)
                    {
                        LogClass logClass = new LogClass()
                        {
                            IsDisplay = DDTV_Core.SystemAssembly.Log.Log.LogList[Count - 1 - i].IsDisplay,
                            IsError = DDTV_Core.SystemAssembly.Log.Log.LogList[Count - 1 - i].IsError,
                            Message = DDTV_Core.SystemAssembly.Log.Log.LogList[Count - 1 - i].Message,
                            RunningTime = DDTV_Core.SystemAssembly.Log.Log.LogList[Count - 1 - i].RunningTime,
                            Source = DDTV_Core.SystemAssembly.Log.Log.LogList[Count - 1 - i].Source,
                            Time = DDTV_Core.SystemAssembly.Log.Log.LogList[Count - 1 - i].Time,
                            Type = DDTV_Core.SystemAssembly.Log.Log.LogList[Count - 1 - i].Type,
                        };
                        Logs.Add(logClass);
                    }
                }
                return Content(MessageBase.Success(nameof(System_LatestLog), Logs), "application/json");
            }
            else
            {
                return Content(MessageBase.Success(nameof(System_LatestLog), false, $"输入的参数取值范围有误"), "application/json");
            }
        }
      
    }

    public class System_SetWEBFirstStart : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "System_SetWebFirstStart")]
        //[Consumes("application/json")]
        public ActionResult Post([FromForm] string cmd, [FromForm] bool state)
        {
            CoreConfig.SetValue(CoreConfigClass.Key.GUI_FirstStart, state.ToString(), CoreConfigClass.Group.Core);
            CoreConfig.GUI_FirstStart = state;
            return Content(MessageBase.Success(nameof(System_SetWEBFirstStart), state, $"设置初始化标志位为:{state}"), "application/json");
        }
    }
    public class System_QueryUserState : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "System_QueryUserState")]
        public ActionResult Post([FromForm] string cmd)
        {
            if (!DDTV_Core.SystemAssembly.BilibiliModule.API.UserInfo.LoginValidityVerification())
            {
                return Content(MessageBase.Success(nameof(System_QueryUserState), false, $"未登录或登陆已失效"), "application/json");
            }
            else
            {
                return Content(MessageBase.Success(nameof(System_QueryUserState), true, $"登陆有效"), "application/json");
            }

        }
    }
}