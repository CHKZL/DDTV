using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;

namespace DDTV_WEB_Server.Controllers
{
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    public class Login : ControllerBase
    {
        [HttpPost(Name = "Login")]
        public ActionResult Post([FromForm] string UserName, [FromForm] string Password,[FromForm] bool CookieExpires=false)
        {
            if (UserName == RuntimeConfig.WebUserName && Password == RuntimeConfig.WebPassword)
            {
                RuntimeConfig.Cookis = Guid.NewGuid().ToString();
                CookieOptions cookieOptions = new CookieOptions();
                if (CookieExpires)
                {
                    cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));
                }
                if(!string.IsNullOrEmpty(DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.CookieDomain))
                {
                    cookieOptions.Domain = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.CookieDomain;
                }
                HttpContext.Response.Cookies.Append("DDTVUser", RuntimeConfig.Cookis, cookieOptions);
                return Content(MessageBase.Success(nameof(Login), new LoginOK(){Cookie = RuntimeConfig.Cookis}), "application/json");
            }
            else
            {
                return Content(MessageBase.Success(nameof(Login), "µÇÂ½ÑéÖ¤Ê§°Ü", "µÇÂ½ÑéÖ¤Ê§°Ü", MessageBase.code.LoginVeriicationFailed), "application/json");
            }
        }
        private class LoginOK
        {
            public string Cookie { get; set; }
        }
    }
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    public class LoginErrer : ControllerBase
    {
        [HttpGet(Name = "LoginErrer")]
        public string get()
        {
            return MessageBase.Success(nameof(LoginErrer), "WEBµÇÂ½Ê§Ð§ÇëÖØÐÂµÇÂ½", "WEBµÇÂ½Ê§Ð§ÇëÖØÐÂµÇÂ½", MessageBase.code.LoginInfoFailure);
        }
    }
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    public class AuthenticationFailed : ControllerBase
    {
        [HttpGet(Name = "AuthenticationFailed")]
        public string get(MessageBase.code code,string message)
        {
            return MessageBase.Success(nameof(LoginErrer), message, message, code);
        }
    }
}