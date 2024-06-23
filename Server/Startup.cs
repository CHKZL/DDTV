using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Core.LogModule;
using Microsoft.Extensions.FileProviders;
using Server.WebAppServices.Middleware;
using System.Runtime.InteropServices;
using Core;
using Microsoft.AspNetCore.Http.Features;
using System.Net;

namespace Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // 这个方法用于添加服务到容器中
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = 1024 * 1024;
            });
           
        }

        // 这个方法用于配置HTTP请求管道
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.DefaultConnectionLimit = 4096 * 16;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            app.UseWebSockets();
            app.UseMiddleware<WebSocketControl>();
            app.UseMiddleware<AccessControl>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseStatusCodePagesWithRedirects("/api/not_found");
            app.UseStaticFiles(new StaticFileOptions
            {
                //将WEBUI的文件映射到根目录提供静态文件服务以提供WEBUI
                FileProvider = new PhysicalFileProvider(Core.Tools.FileOperations.CreateAll(Path.GetFullPath(Config.Core_RunConfig._WebUiDirectory))),
                RequestPath = ""
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                //将录制路径映射为一个虚拟路径的静态文件服务，让web去读取用于播放和下载
                FileProvider = new PhysicalFileProvider(Core.Tools.FileOperations.CreateAll(Path.GetFullPath(Config.Core_RunConfig._RecFileDirectory))),
                RequestPath = Config.Core_RunConfig._RecordingStorageDirectory
            });
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log.Info(nameof(Startup), $"检测到当前是Windows环境，请确保终端的快捷编辑功能关闭，否则可能被误触导致该终端暂停运行");
            }
            WebAppServices.WS.WebSocketQueue.Start();
            Log.Info(nameof(Startup), $"WebSocket，开始监听，路径：[/ws]");
        }
    }
}
