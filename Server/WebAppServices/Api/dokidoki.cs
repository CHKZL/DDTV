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
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    [Login]
    [Tags("dokidoki")]
    public class dokidoki : ControllerBase
    {
        /// <summary>
        /// 请求当前运行心跳信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "dokidoki")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(dokidoki), Core.Tools.DokiDoki.GetDoki()), "application/json");
        }
        /// <summary>
        /// 请求当前运行心跳信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "dokidoki")]
        public ActionResult GET(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(dokidoki), Core.Tools.DokiDoki.GetDoki()), "application/json");
        }

    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("dokidoki")]
    public class init_inspect : ControllerBase
    {
        /// <summary>
        /// 用于测试web是否初始化完成
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "init_inspect")]
        public ActionResult Get()
        {
            return Content(MessageBase.MssagePack(nameof(init_inspect), "OK", "OK"), "application/json");
        }
    }
}
