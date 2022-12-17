using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using static DDTV_Core.SystemAssembly.ConfigModule.BilibiliUserConfig;

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
            if (UserName.Equals(WebServerConfig.WebUserName) && Password.Equals(WebServerConfig.WebPassword))
            {
                WebServerConfig.Cookis = Guid.NewGuid().ToString();
                CookieOptions cookieOptions = new CookieOptions();
                if (CookieExpires)
                {
                    cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));
                }
                if(!string.IsNullOrEmpty(WebServerConfig.CookieDomain))
                {
                    cookieOptions.Domain = WebServerConfig.CookieDomain;
                }
                HttpContext.Response.Cookies.Append("DDTVUser", WebServerConfig.Cookis, cookieOptions);
                return Content(MessageBase.Success(nameof(Login), new LoginOK(){Cookie = WebServerConfig.Cookis}), "application/json");
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
        public ActionResult get()
        {
            return Content(MessageBase.Success(nameof(LoginErrer), "WEBµÇÂ½Ê§Ð§ÇëÖØÐÂµÇÂ½", "WEBµÇÂ½Ê§Ð§ÇëÖØÐÂµÇÂ½", MessageBase.code.LoginInfoFailure), "application/json");
        }
    }
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    public class AuthenticationFailed : ControllerBase
    {
        [HttpGet(Name = "AuthenticationFailed")]
        public ActionResult get(MessageBase.code code,string message)
        {
            return Content(MessageBase.Success(nameof(AuthenticationFailed), message, message, code), "application/json");
        }
    }
    /// <summary>
    /// ÖØÐÂµÇÂ½
    /// </summary>
    public class Login_Reset : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Login_Reset")]
        public ActionResult post([FromForm] string cmd)
        {
            if(DDTV_Core.SystemAssembly.ConfigModule.BilibiliUserConfig.account.loginStatus!= BilibiliUserConfig.LoginStatus.LoggingIn)
            {
                DDTV_Core.SystemAssembly.BilibiliModule.User.login.ReLogin.Login();
            }
            return Content(MessageBase.Success(nameof(Login_Reset), "Çë·ÃÎÊ api/login ½Ó¿Ú½øÐÐÉ¨ÂëµÇÂ½"), "application/json");
        }
    }
    /// <summary>
    /// ²éÑ¯ÄÚ²¿µÇÂ½×´Ì¬
    /// </summary>
    public class Login_State : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Login_State ")]
        public ActionResult post([FromForm] string cmd)
        {
            LoginC login = new LoginC()
            {
                LoginState = BilibiliUserConfig.account.loginStatus, 
            };
            return Content(MessageBase.Success(nameof(Login_State), login), "application/json");
        }
        public class LoginC
        {
            public LoginStatus LoginState { get; set; }
        }
    }
}