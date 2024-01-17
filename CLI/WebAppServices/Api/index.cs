using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.WebAppServices.Api
{
    /// <summary>
    /// index首页
    /// </summary>
    [Route("webui")]
    [ApiController]
    [Tags("index")]
    public class webui : ControllerBase
    {
        /// <summary>
        /// 请求WEB页首页
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "webui")]
        public ActionResult Get()
        {
            string Html = System.IO.File.ReadAllText(@"./static/index.html");
            return Content(Html, "text/html");
        }
    }
    /// <summary>
    /// 根路径跳转到webui
    /// </summary>
    [Route("/")]
    [ApiController]
    [Tags("index")]
    public class index : ControllerBase
    {
        /// <summary>
        /// 请求WEB页首页
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "/")]
        public ActionResult Get()
        {
            return Redirect("/webui");
        }
    }
}
