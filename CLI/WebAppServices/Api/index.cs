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
    [Route("[controller]")]
    [ApiController]
    public class index : ControllerBase
    {
        /// <summary>
        /// 请求WEB页首页
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "index")]
        public ActionResult Get()
        {
            string Html = System.IO.File.ReadAllText(@"index.html");
            return Content(Html, "text/html");
        }
    }
    /// <summary>
    /// 路由器根页面跳转
    /// </summary>
    [Route("/")]
    [ApiController]
    public class root : ControllerBase
    {
        /// <summary>
        /// 请求WEB页首页
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "root")]
        public ActionResult Get()
        {
            return Redirect("/index");
        }
    }
}
