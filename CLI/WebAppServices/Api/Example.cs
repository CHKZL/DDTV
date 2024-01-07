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
    [Route("api/[controller]")]
    [AllowAnonymous]
    [Login]
    public class Example : ControllerBase
    {
        [HttpPost(Name = "Example")]
        public ActionResult Post([FromForm] int a, [FromForm] int b, PostCommonParameters commonParameters)
        {
            return Content(MessageBase.Success(nameof(Example), a + b), "application/json");
        }
        [HttpGet(Name = "Example")]
        public ActionResult GET([FromQuery] int a, [FromQuery] int b, GetCommonParameters commonParameters)
        {
            return Content(MessageBase.Success(nameof(Example), a + b), "application/json");
        }
    }
}
