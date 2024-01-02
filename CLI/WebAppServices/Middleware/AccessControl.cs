using Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.WebAppServices.Middleware
{
    /// <summary>
    /// 跨域设置
    /// </summary>
    public class AccessControl
    {
        private readonly RequestDelegate _next;
        public AccessControl(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Credentials"))
            {
                context.Response.Headers.Add("Access-Control-Allow-Credentials", Config.Web._AccessControlAllowCredentials);
            }
            if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", Config.Web._AccessControlAllowOrigin);
            }
            await _next(context);
        }
    }

}
