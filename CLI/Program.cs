using Core;
using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject;
using static Core.RuntimeObject.RoomList.RoomCard;
using System.Runtime.Intrinsics.X86;
using ConsoleTableExt;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Core.LiveChat;
using static System.Net.Mime.MediaTypeNames;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using static CLI.Program.Service;

namespace CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //注册DDTV主要服务
            //Task.Run(() => Service.CreateHostBuilder(new string[] { "" }).Build().Run());

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMvc();
             
            //注册Cookie认证服务
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
                {
                    option.AccessDeniedPath = "api/LoginErrer"; //当用户尝试访问资源但没有通过任何授权策略时，这时请求会重定向的相对路径资源
                    option.LoginPath = "api/Login/";
                    option.Cookie.Name = "DDTVUser";//设置存储用户登录信息（用户Token信息）的Cookie名称
                    option.Cookie.HttpOnly = true;//设置存储用户登录信息（用户Token信息）的Cookie，无法通过客户端浏览器脚本(如JavaScript等)访问到
                                                  //option.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                                                  //设置存储用户登录信息（用户Token信息）的Cookie，只会通过HTTPS协议传递，如果是HTTP协议，Cookie不会被发送。注意，option.Cookie.SecurePolicy属性的默认值是Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
                });
           
            builder.Services.AddHostedService<DDTVService>();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            //用于检测是否为开发环境
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<WebAppServices.Middleware.AccessControl>();
            //app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = false,
                FileProvider = new PhysicalFileProvider(Core.Tools.FileOperations.CreateAll(Environment.CurrentDirectory + @"/static")),
                RequestPath = new PathString("/static")
            });
            string rurl = $"http://0.0.0.0:11419";
            app.Urls.Add(rurl);
            Log.Info(nameof(Main), $"WebApplication开始运行，开始监听[{rurl}]");
            app.Run();
        }

        public class Service
        {
            public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<DDTVService>();
                });
            public class DDTVService : BackgroundService
            {
                protected override Task ExecuteAsync(CancellationToken stoppingToken)
                {
                    return Task.Run(async () =>
                    {
                        Core.Init.Start();//初始化必须执行的
                        if (!Account.AccountInformation.State)
                        {
                            await Login.QR();//如果没有登录态，需要执行扫码
                        }
                        while (!Account.AccountInformation.State)
                        {
                            Thread.Sleep(1000);//等待登陆
                        }
                        TerminalDisplay.SeKey();
                        DetectRoom detectRoom = new();//实例化房间监听
                        detectRoom.start();//启动房间监听
                        detectRoom.LiveStart += Record.DetectRoom_LiveStart;//开播事件
                        Log.Info(nameof(DetectRoom), $"注册开播事件");
                        detectRoom.LiveEnd += Record.DetectRoom_LiveEnd;//下播事件
                        Log.Info(nameof(DetectRoom), $"注册下播事件");
                        //TEST();
#if DEBUG
                        Task.Run(() =>
                        {
                            Process currentProcess = null;
                            while (true)
                            {
                                currentProcess = Process.GetCurrentProcess();
                                long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
                                (int Total, int Download) = Core.RuntimeObject.RoomList.GetTasksInDownloadCount();
                                Log.Info("DokiDoki", $"总:{Total}|录制中:{Download}|使用内存:{Core.Tools.Linq.ConversionSize(totalBytesOfMemoryUsed, Core.Tools.Linq.ConversionSizeType.String)}|{Init.InitType}|{Init.Ver}【Dev】(编译时间:{Init.CompiledVersion})");
                                Thread.Sleep(60 * 1000);
                            }
                        });
# endif
                    });
                }

                public override Task StopAsync(CancellationToken stoppingToken)
                {
                    return Task.Run(() =>
                    {
                        Log.Warn(nameof(DDTVService), "收到SIGINT信号(一般是用户Ctrl+C)，主进程被系统结束");
                    });
                }
            }
        }
    }
}
