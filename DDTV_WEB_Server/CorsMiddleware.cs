namespace DDTV_WEB_Server
{
    public class CorsMiddleware
    {
       /// <summary>
       /// 跨域设置
       /// </summary>
        public class AccessControlAllowOrigin
        {
            private readonly RequestDelegate _next;
            public AccessControlAllowOrigin(RequestDelegate next)
            {
                _next = next;
            }

            public async Task Invoke(HttpContext context)
            {
                if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
                await _next(context);
            }
        }
       
    }
}
