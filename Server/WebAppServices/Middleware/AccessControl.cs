using Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.WebAppServices.Middleware
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
                context.Response.Headers.Add("Access-Control-Allow-Credentials", Config.Core_RunConfig._AccessControlAllowCredentials);
            }
            if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", Config.Core_RunConfig._AccessControlAllowOrigin);
            }
            await _next(context);
        }
    }

}
