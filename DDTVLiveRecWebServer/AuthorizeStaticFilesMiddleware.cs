using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
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
            try
            {
                int B = context.Request.Query.Count();
            }
            catch (Exception)
            {
                await context.ForbidAsync();
                return;
            }
            foreach (var item in new List<string>() {
                System.Web.HttpUtility.UrlDecode(context.Request.Query["Directory"],System.Text.Encoding.UTF8),
                System.Web.HttpUtility.UrlDecode(context.Request.Query["File"],System.Text.Encoding.UTF8)
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            if (Auxiliary.MMPU.调试模式)
            {
                foreach (var item in context.Request.Query)
                {
                    Console.WriteLine($"{System.Web.HttpUtility.UrlDecode(item.Key, System.Text.Encoding.UTF8)}={System.Web.HttpUtility.UrlDecode(item.Value, System.Text.Encoding.UTF8)}");
                }
            }
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_steam", 鉴权预处理结果 ? true : false);
            //var WEBAPI请求文件 = context.Request.Headers["FileSig"].FirstOrDefault();
            if (!鉴权结果.鉴权结果)
            {

                await context.ForbidAsync();
                return;
            }
            else
            {
                //鉴权通过，并且文件存在，则返回文件流
                await _next(context);
                return;
            }

            ////如果鉴权通过，但是文件权限不足或者不存在时返回ForbidAsync
            //await context.ForbidAsync();
           
        }
    }
}
