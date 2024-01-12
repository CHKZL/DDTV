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
    [Route("/")]
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
            return Redirect("/index.html");
        }
    }
}
