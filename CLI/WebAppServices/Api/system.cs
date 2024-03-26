using CLI.WebAppServices.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static CLI.WebAppServices.Middleware.InterfaceAuthentication;

namespace CLI.WebAppServices.Api
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
}
