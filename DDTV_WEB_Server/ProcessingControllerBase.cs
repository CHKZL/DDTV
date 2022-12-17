using DDTV_Core.SystemAssembly.ConfigModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Net.Mime;

namespace DDTV_WEB_Server
{
    public class ProcessingControllerBase
    {
        [Produces(MediaTypeNames.Application.Json)]
        [ApiController]
        [Route("api/[controller]")]
        [AllowAnonymous]
        [Login]
        public class ApiControllerBase : ControllerBase
        {
           
        }
    }
    /// <summary>
    /// 对于DDTV_WEB_API权限验证和sig校验的筛选器操作
    /// </summary>
    public class LoginAttribute : ActionFilterAttribute
    {
        [HttpPost(Name = "Attribute")]
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Method == "POST")
            {
                string sig = string.Empty;
                string time = string.Empty;
                string cmd = string.Empty;
                string accesskeyid = string.Empty;
                string B = filterContext.HttpContext.Request.ContentType;
                var T = filterContext.HttpContext.Request.Form;
                if (filterContext.HttpContext.Request.Form != null)
                {
                    sig = filterContext.HttpContext.Request.Form["sig"];
                    time = filterContext.HttpContext.Request.Form["time"];
                    cmd = filterContext.HttpContext.Request.Form["cmd"];
                    accesskeyid = filterContext.HttpContext.Request.Form["accesskeyid"];
                }
                if (!string.IsNullOrEmpty(sig))
                {
                    if (!string.IsNullOrEmpty(time) || !string.IsNullOrEmpty(cmd) || !string.IsNullOrEmpty(accesskeyid))
                    {
                        if (accesskeyid == WebServerConfig.AccessKeyId)
                        {
                            Dictionary<string, string> parameters = new Dictionary<string, string>
                        {
                            { "accesskeyid", accesskeyid },
                            { "accesskeysecret", WebServerConfig.AccessKeySecret },
                            { "cmd", cmd.ToLower() },
                            { "time", time }
                        };
                            string Original = string.Empty;
                            foreach (var item in parameters)
                            {
                                Original += $"{item.Key}={item.Value};";
                            }
                            string NewSig = DDTV_Core.SystemAssembly.EncryptionModule.Encryption.SHA1_Encrypt(Original);
                            if (NewSig != sig)
                            {
                                filterContext.HttpContext.Response.WriteAsync(MessageBase.Success("AuthenticationFailed", "sig校验失败", "sig校验失败", MessageBase.code.APIAuthenticationFailed),System.Text.Encoding.UTF8);
                                filterContext.Result = new EmptyResult();
                                return;
                            }
                        }
                        else
                        {
                            filterContext.HttpContext.Response.WriteAsync(MessageBase.Success("AuthenticationFailed", "sig校验失败", "sig校验失败", MessageBase.code.APIAuthenticationFailed), System.Text.Encoding.UTF8);
                            filterContext.Result = new EmptyResult();
                            return;
                        }
                    }
                    else
                    {
                        filterContext.HttpContext.Response.WriteAsync(MessageBase.Success("AuthenticationFailed", "sig校验失败", "sig校验失败", MessageBase.code.APIAuthenticationFailed), System.Text.Encoding.UTF8);
                        filterContext.Result = new EmptyResult();
                        return;
                    }
                }
                else if (filterContext.HttpContext.Request.Cookies != null)
                {
                    string cookis = filterContext.HttpContext.Request.Cookies["DDTVUser"];
                    if (string.IsNullOrEmpty(cookis))
                    {
                        LoginErrer(filterContext);
                        return;
                    }
                    else
                    {
                        if (cookis != WebServerConfig.Cookis)
                        {
                            LoginErrer(filterContext);
                            return;
                        }
                        else
                        {
                            //这里应该是进行sig校验的地方
                        }
                    }
                }
                else
                {
                    LoginErrer(filterContext);
                    return;
                }
            }
        }
        private static void LoginErrer(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Cookies.Delete("DDTVUser");
            filterContext.HttpContext.Response.Redirect("/api/LoginErrer");
            filterContext.Result = new EmptyResult();
        }
    }
}
