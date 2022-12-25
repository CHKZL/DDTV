using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Net.Mime;

namespace DDTV_WEB_Server.Controllers
{

    /// <summary>
    /// 路由器根页面index
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class index : ControllerBase
    {
        [HttpGet(Name = "/")]
        public ActionResult get()
        {
            string Html = System.IO.File.ReadAllText(@"index.html");
            return Content(Html, "text/html");
        }
    }


}
