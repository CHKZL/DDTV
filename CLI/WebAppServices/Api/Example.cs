using CLI.WebAppServices.Middleware;
using Core.LogModule;
using Microsoft.AspNetCore.Authorization;
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
    [Route("api/Example/[controller]")]
    [Login]
    public class Example : ControllerBase
    {
        /// <summary>
        /// 测试函数，计算a+b的返回值
        /// </summary>
        /// <param name="a">数值A，int</param>
        /// <param name="b">数值B，int</param>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "Example")]
        public ActionResult Post([FromForm] int a, [FromForm] int b, PostCommonParameters commonParameters)
        {
            return Content(MessageBase.Success(nameof(Example), a + b), "application/json");
        }
         /// <summary>
        /// 测试函数，计算a+b的返回值
        /// </summary>
        /// <param name="a">数值A，int</param>
        /// <param name="b">数值B，int</param>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "Example")]
        public ActionResult GET([FromQuery] int a, [FromQuery] int b, GetCommonParameters commonParameters)
        {
            return Content(MessageBase.Success(nameof(Example), a + b), "application/json");
        }
    }
}
