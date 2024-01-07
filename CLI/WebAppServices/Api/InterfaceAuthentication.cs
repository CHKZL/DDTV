using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;

namespace CLI.WebAppServices.Middleware
{
    public class InterfaceAuthentication
    {
        [Produces(MediaTypeNames.Application.Json)]
        [ApiController]
        [Route("api/[controller]")]
        [AllowAnonymous]
        [Login]
        public class ApiControllerBase : ControllerBase
        {

        }
        public class PostCommonParameters
        {
            [FromForm]
            public string sig { get; set; }
            [FromForm]
            public long time { get; set; }
            [FromForm]
            public string accesskeyid { get; set; }
        }
        public class GetCommonParameters
        {
            [FromQuery]
            public string sig { get; set; }
            [FromQuery]
            public long time { get; set; }
            [FromQuery]
            public string accesskeyid { get; set; }
        }
    }
    /// <summary>
    /// 权限sig校验的筛选器操作
    /// </summary>
    public class LoginAttribute : ActionFilterAttribute
    {
        [HttpPost(Name = "Attribute")]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 检查请求方法是否为POST或GET，如果不是，则返回未授权
            if (context.HttpContext.Request.Method != "POST" && context.HttpContext.Request.Method != "GET")
            {
                Unauthorized(context);
                return;
            }

            // 根据请求方法从表单或查询字符串中获取参数
            var parameters = context.HttpContext.Request.Method == "POST" ? new Dictionary<string, StringValues>(context.HttpContext.Request.Form) : new Dictionary<string, StringValues>(context.HttpContext.Request.Query);

            // 检查参数是否包含"accesskeysecret"或是否缺少"sig"，如果是，则返回未授权
            if (parameters.ContainsKey("accesskeysecret") || !parameters.ContainsKey("sig"))
            {
                Unauthorized(context);
                return;
            }

            // 将"accesskeysecret"添加到参数字典中
            parameters.Add("accesskeysecret", Core.Config.Web._AccessKeySecret);

            // 创建签名字符串，排除"sig"，并按键排序（字母顺序）
            string AuthenticationOriginalStr = string.Join(";", parameters.Where(p => p.Key.ToLower() != "sig").OrderBy(p => p.Key).Select(p => $"{p.Key.ToLower()}={p.Value}"));

            // 使用SHA1加密签名字符串
            string sig = Core.Tools.Encryption.SHA1_Encrypt(AuthenticationOriginalStr);

            // 如果签名不匹配，则返回未授权
            if (sig != parameters["sig"])
            {
                Unauthorized(context);
                return;
            }
        }



        /// <summary>
        /// 未授权返回HTTP 401
        /// </summary>
        /// <param name="context"></param>
        private static void Unauthorized(ActionExecutingContext context)
        {
            context.HttpContext.Response.StatusCode = 401;
            context.HttpContext.Response.Redirect("/api/Unauthorized");
            context.Result = new UnauthorizedResult();
        }
    }
}
