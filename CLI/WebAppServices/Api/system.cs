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
    public class GetCoreVersion : ControllerBase
    {
        /// <summary>
        /// 获取当前Core的版本号
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "GetCoreVersion")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(GetCoreVersion), System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), "CoreVersion"), "application/json");
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/system/[controller]")]
    [Login]
    [Tags("config")]
    public class GetWebUIVersion : ControllerBase
    {
        /// <summary>
        /// 获取当前WEBUI版本信息
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "GetWebUIVersion")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            if(System.IO.File.Exists("./static/version.ini"))
            {
                string info = System.IO.File.ReadAllText("./static/version.ini");
                return Content(MessageBase.MssagePack(nameof(GetWebUIVersion), info, "WebUIVersion"), "application/json");
            }
            else
            {
                return Content(MessageBase.MssagePack(nameof(GetWebUIVersion), "", "WEBUI版本信息文件不存在"), "application/json");
            }           
        }
    }
}
