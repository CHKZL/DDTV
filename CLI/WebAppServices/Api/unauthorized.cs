using CLI.WebAppServices.Middleware;
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
    /// <summary>
    /// 修改开播提示设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    public class unauthorized : ControllerBase
    {
        /// <summary>
        /// 未授权
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "unauthorized")]
        public ActionResult Get()
        {     
            return Content("HTTP 401", "application/json");
        }
    }
}
