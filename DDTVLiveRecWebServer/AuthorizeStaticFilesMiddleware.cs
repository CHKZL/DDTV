using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            var providedApiKey = context.Request.Headers["FileSig"].FirstOrDefault();

            //和API请求加密过程同理，在Headers里面附带FileSig，然后进行校验


            /*
                (施工中)文件鉴权操作逻辑部分
             */ 

            //如果鉴权失败，返回未经授权的提示
            await context.ChallengeAsync();
            //如果鉴权通过，但是文件权限不足或者不存在时返回ForbidAsync
            await context.ForbidAsync();
            //鉴权通过，并且文件存在，则返回文件流
            await _next(context);
        }
    }
}
