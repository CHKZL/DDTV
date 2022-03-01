
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography.X509Certificates;
using DDTV_Core.SystemAssembly.ConfigModule;

namespace DDTV_WEB_Server//DDTVLiveRecWebServer
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Task.Run(() =>
            {
                DDTV_Core.InitDDTV_Core.Core_Init(DDTV_Core.InitDDTV_Core.SatrtType.DDTV_WEB);
                BilibiliUserConfig.CheckAccount.CheckAccountChanged += CheckAccount_CheckAccountChanged;//注册登陆信息检查失效事件
                ServerInteraction.CheckUpdates.Update();
                ServerInteraction.Dokidoki.Start();
            });
            Thread.Sleep(3000);
            string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine(Ver);
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureServices(Services =>
            {
                Services.AddControllers();
                Services.AddEndpointsApiExplorer();
                Services.AddSwaggerGen();
               
                Services.AddMvc();
                //注册Cookie认证服务
                Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
                    {
                        option.AccessDeniedPath = "api/LoginErrer"; //当用户尝试访问资源但没有通过任何授权策略时，这是请求会重定向的相对路径资源
                        option.LoginPath = "api/Login/";
                        option.Cookie.Name = "UserTEST";//设置存储用户登录信息（用户Token信息）的Cookie名称
                        option.Cookie.HttpOnly = true;//设置存储用户登录信息（用户Token信息）的Cookie，无法通过客户端浏览器脚本(如JavaScript等)访问到
                                                      //option.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                                                      //设置存储用户登录信息（用户Token信息）的Cookie，只会通过HTTPS协议传递，如果是HTTP协议，Cookie不会被发送。注意，option.Cookie.SecurePolicy属性的默认值是Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
                    });

            });
            //builder.Host.ConfigureWebHost(webBuilder =>
            //{
            //    SetAPP(webBuilder);
            //});
            builder.Services.AddSwaggerGen();
            if (RuntimeConfig.IsSSL)
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        var certPath = Path.Combine(builder.Environment.ContentRootPath, RuntimeConfig.pfxFileName);
                        var keyPath = Path.Combine(builder.Environment.ContentRootPath, RuntimeConfig.pfxPasswordFileName);
                        httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                    });
                });
            }

           
            var app = builder.Build();

            //用于检测是否为开发环境
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<CorsMiddleware.AccessControlAllowOrigin>();
            //app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = false,//关闭目录结构树访问权限
                FileProvider = new PhysicalFileProvider(DDTV_Core.Tool.FileOperation.CreateAll(Environment.CurrentDirectory + @"/static")),
                RequestPath = new PathString("/static")
            });
            app.Urls.Add("http://0.0.0.0:11419");
            if (RuntimeConfig.IsSSL)
            {
                app.Urls.Add("https://0.0.0.0:11451");
            }
            app.Run();
        }

        private static void CheckAccount_CheckAccountChanged(object? sender, EventArgs e)
        {
            Task.Run(() =>
            {
                int i = 0;
                while (i < 360)
                {
                    DDTV_Core.SystemAssembly.Log.Log.AddLog("Login", DDTV_Core.SystemAssembly.Log.LogClass.LogType.Error, "账号登陆失效！请重启DDTV进行登陆！");
                    Thread.Sleep(10 * 1000);
                    i++;
                }
            });
        }
        private static void StartWebServices(string[] args)
        {
            Task.Run(async () =>
            {

            });
        }
    }
}