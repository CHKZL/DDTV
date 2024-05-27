using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.WebAppServices.Api
{
    ///// <summary>
    ///// index首页
    ///// </summary>
    //[Route("webui")]
    //[ApiController]
    //[Tags("index")]
    //public class webui : ControllerBase
    //{
    //    /// <summary>
    //    /// 请求WEB页首页
    //    /// </summary>
    //    /// <returns></returns>
    //    [HttpGet(Name = "webui")]
    //    public ActionResult Get()
    //    {
    //        string Html = System.IO.File.ReadAllText(@$"{Config.Web._WebUiDirectory}index.html");
    //        return Content(Html, "text/html");
    //    }
    //}
    /// <summary>
    /// 根路径跳转到webui
    /// </summary>
    /// 
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
            string Html = System.IO.File.ReadAllText(@$"{Config.Core_RunConfig._WebUiDirectory}index.html");
            return Content(Html, "text/html");
            //return Redirect("/webui");
        }
    }
}
