using Server.WebAppServices.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Server.WebAppServices.Middleware.InterfaceAuthentication;
using static Core.Tools.SystemResource;
using static Core.Tools.SystemResource.GetHDDInfo;
using static Core.Tools.SystemResource.GetMemInfo;
using static FastExpressionCompiler.ImTools.FHashMap;
using System.Net;

namespace Server.WebAppServices.Api
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/system/[controller]")]
    [Login]
    [Tags("config")]
    public class get_core_version : ControllerBase
    {
        /// <summary>
        /// 获取当前Core的版本号
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "get_core_version")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_core_version), System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), "CoreVersion"), "application/json");
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/system/[controller]")]
    [Login]
    [Tags("config")]
    public class get_webui_version : ControllerBase
    {
        /// <summary>
        /// 获取当前WEBUI版本信息
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "get_webui_version")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            if(System.IO.File.Exists("./static/version.ini"))
            {
                string info = System.IO.File.ReadAllText("./static/version.ini");
                return Content(MessageBase.MssagePack(nameof(get_webui_version), info, "WebUIVersion"), "application/json");
            }
            else
            {
                return Content(MessageBase.MssagePack(nameof(get_webui_version), "", "WEBUI版本信息文件不存在"), "application/json");
            }
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/system/[controller]")]
    [Login]
    [Tags("config")]
    public class get_system_resources : ControllerBase
    {
        /// <summary>
        /// 获取内存和录制路径储存空间使用情况（请注意！该接口单次执行都在秒以上，且硬件开销较大，不推荐频繁调用用于刷新！如果一定要用，调用间隔推荐以分钟为单位）
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "get_system_resources")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            SystemResourceClass systemResourceClass = new();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                systemResourceClass = new()
                {
                    HDDInfo = GetHDDInfo.GetLinux(),
                    Memory = GetMemInfo.GetLiunx(),
                    Platform = "Linux"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                systemResourceClass = new()
                {
                    Memory = GetMemInfo.GetWindows(),
                    Platform = "Windows"
                };
                string DriveLetter = Path.GetFullPath(Core.Config.Core_RunConfig._RecFileDirectory)[..1];
                systemResourceClass.HDDInfo = GetHDDInfo.GetWindows(DriveLetter);
            }
            return Content(MessageBase.MssagePack(nameof(get_system_resources), systemResourceClass, "SystemResource"), "application/json");
        }
        public class SystemResourceClass
        {
            /// <summary>
            /// 平台
            /// </summary>
            public string Platform { set; get; }
            /// <summary>
            /// 内存
            /// </summary>
            public MemInfo Memory { set; get; }
            /// <summary>
            /// 硬盘信息
            /// </summary>
            public List<HDDInfo> HDDInfo { set; get; }
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/system/[controller]")]
    [Login]
    [Tags("config")]
    public class get_c : ControllerBase
    {
        /// <summary>
        /// 用于桌面端播放器配置
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "get_c")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_c), Core.RuntimeObject.Account.AccountInformation.strCookies, ""), "application/json");
        }
    }
}
