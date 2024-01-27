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
using static Core.Tools.FileOperations;

namespace CLI.WebAppServices.Api
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class reload_configuration : ControllerBase
    {
        /// <summary>
        /// 重新从配置文件加载配置到内存
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "reload_configuration")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            Core.Config.ReadConfiguration();
            return Content(MessageBase.Success(nameof(reload_configuration), true, $"从配置文件重新加载配置\r\n请注意，如果修改了路径相关配置，之后调用路径相关接口获取到的都会是新的配置，可能会造成一场直播写到两个路径中的问题"), "application/json");
        }
    }
}
