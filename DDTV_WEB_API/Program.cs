
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography.X509Certificates;
using DDTV_Core.SystemAssembly.ConfigModule;

namespace DDTVLiveRecWebServer
{
    public class Program
    {
        private static bool IsLTS = true;
        private static string pfxFileName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadPath, "Rec", CoreConfigClass.Group.WEB_API);
        private static string pfxPasswordFileName = CoreConfig.GetValue(CoreConfigClass.Key.DownloadPath, "Rec", CoreConfigClass.Group.WEB_API);
        public static void Main(string[] args)
        {
            {
                DDTV_Core.InitDDTV_Core.Core_Init(DDTV_Core.InitDDTV_Core.SatrtType.DDTV_WEB);
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                builder.Host.ConfigureServices(Services =>
                {
                    Services.AddControllers();
                    Services.AddEndpointsApiExplorer();
                    Services.AddSwaggerGen();
                    Services.AddMvc();
                    Services.AddControllers();
                    //注册Cookie认证服务
                    Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
                        {
                            option.AccessDeniedPath = "/LoginErrer"; //当用户尝试访问资源但没有通过任何授权策略时，这是请求会重定向的相对路径资源
                            option.LoginPath = "/login/";
                            option.Cookie.Name = "User";//设置存储用户登录信息（用户Token信息）的Cookie名称
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
                if (IsLTS)
                {
                    builder.WebHost.ConfigureKestrel(options =>
                    {
                        options.ConfigureHttpsDefaults(httpsOptions =>
                        {
                           
                            var certPath = Path.Combine(builder.Environment.ContentRootPath, "./6790481.pem");
                            var keyPath = Path.Combine(builder.Environment.ContentRootPath, "./6790481.key");

                            httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath,
                                                             keyPath);
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
                
                //app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                app.UseFileServer(new FileServerOptions()
                {
                    EnableDirectoryBrowsing = false,//关闭目录结构树访问权限
                    FileProvider = new PhysicalFileProvider(DDTV_Core.Tool.FileOperation.CreateAll(Environment.CurrentDirectory + @"/static")),
                    RequestPath = new PathString("/static")
                });
                app.Urls.Add("http://0.0.0.0:30086");
                app.Urls.Add("https://0.0.0.0:30087");
                app.Run();

            }
        }
        public static void SetAPP(IWebHostBuilder app)
        {
            if (IsLTS)
            {
                app.ConfigureKestrel(option =>
                {
                    option.ConfigureHttpsDefaults(i =>
                    {
                        i.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2($"./pfx证书名称", "pfx证书密码");
                    });
                });
                //app.UseStartup<StartupBase>().UseUrls("https://0.0.0.0:10086");
            }
            //else
            //{
            //    app.UseStartup<StartupBase>().UseUrls("http://0.0.0.0:10086");
            //}
        }
        //public static IHostBuilder Start(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<StartupBase>().UseUrls("http://0.0.0.0:10086");
        //            //webBuilder.UseStartup<Startup>();
        //            SetAPP(webBuilder);
            
        //            webBuilder.Configure(app =>
        //            {
        //                app.UseSwagger();
        //                app.UseSwaggerUI();
        //                app.UseHttpsRedirection();

        //                //提供WEB服务器必要的资源文件，该文件夹的文件不会进过鉴权
        //                app.UseFileServer(new FileServerOptions()
        //                {
        //                    EnableDirectoryBrowsing = false,//关闭目录结构树访问权限
        //                    FileProvider = new PhysicalFileProvider(DDTV_Core.Tool.PathOperation.CreateAll(Environment.CurrentDirectory + @"/static")),
        //                    RequestPath = new PathString("/static")
        //                });
        //            });
                    
        //        });
    }
}