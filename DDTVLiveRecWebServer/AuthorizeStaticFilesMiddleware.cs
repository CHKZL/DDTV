using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer
{
    public class AuthorizeStaticFilesMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthorizeStaticFilesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["Directory"],
                context.Request.Form["File"],
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_steam", 鉴权预处理结果 ? true : false);
            //var WEBAPI请求文件 = context.Request.Headers["FileSig"].FirstOrDefault();
            if (!鉴权结果.鉴权结果)
            {
                //如果鉴权失败，返回未经授权的提示
                await context.ChallengeAsync();
                return;
            }
            else
            {
                //鉴权通过，并且文件存在，则返回文件流
                await _next(context);
            }

            ////如果鉴权通过，但是文件权限不足或者不存在时返回ForbidAsync
            //await context.ForbidAsync();
           
        }
    }
}
