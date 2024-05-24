using Server.WebAppServices.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static Server.WebAppServices.Middleware.InterfaceAuthentication;

namespace Server.WebAppServices.Api
{
    /// <summary>
    /// 鉴权失败
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("unauthorized")]
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

    /// <summary>
    /// 路径不存在
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("not_found")]
    public class not_found : ControllerBase
    {
        /// <summary>
        /// 路径不存在
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "not_found")]
        public ActionResult Get()
        {
            //如果404.html文件存在，就返回404页面
            if (System.IO.File.Exists(@"./static/404.html"))
            {
                string Html = System.IO.File.ReadAllText(@"./static/404.html");
                return Content(Html, "text/html");
            }
            //如果不存在就返回默认图
            else
            {
                FileInfo fi = new FileInfo(@"./resource/not_found.png");
                using (FileStream fs = fi.OpenRead())
                {
                    byte[] buffer = new byte[fi.Length];
                    //读取图片字节流
                    fs.ReadAsync(buffer, 0, Convert.ToInt32(fi.Length));
                    return File(buffer, "image/png");
                }
            }
        }
    }
}
