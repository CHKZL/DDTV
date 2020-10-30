using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DDTVLiveRecWebServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/log", async context =>
                {
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    if (File.Exists("./LOG/DDTVLiveRecLog.out"))
                    {
                        File.Delete("./LOG/DDTVLiveRecLog.out.bak");
                        File.Copy("./LOG/DDTVLiveRecLog.out", "./LOG/DDTVLiveRecLog.out.bak");
                        await context.Response.WriteAsync(File.ReadAllText("./LOG/DDTVLiveRecLog.out.bak", System.Text.Encoding.UTF8));
                        File.Delete("./LOG/DDTVLiveRecLog.out.bak");
                        return;
                    }
                    else
                    {
                        await context.Response.WriteAsync("没有获取到日志文件，请确认DDTVLive正在运行");
                    }
                });
                endpoints.MapGet("/file", async context =>
                {
                    if(!Directory.Exists(Auxiliary.MMPU.缓存路径))
                    {
                        Directory.CreateDirectory(Auxiliary.MMPU.缓存路径);
                    }
                    string A = "当前录制文件夹文件列表:\r\n";
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    foreach (DirectoryInfo NextFolder1 in new DirectoryInfo(Auxiliary.MMPU.缓存路径).GetDirectories())
                    {
                        A = A + "\r\n" + NextFolder1.Name;
                        foreach (FileInfo NextFolder2 in new DirectoryInfo(Auxiliary.MMPU.缓存路径 + NextFolder1.Name).GetFiles())
                        {
                            A = A + "\r\n　　" + Math.Ceiling(NextFolder2.Length / 1024.0 / 1024.0) + " MB |" + NextFolder2.Name;
                        }
                        A = A + "\r\n";
                    }
                    await context.Response.WriteAsync(A, System.Text.Encoding.UTF8);
                });
                endpoints.MapGet("/list", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(Auxiliary.InfoLog.DownloaderInfoPrintf(), System.Text.Encoding.UTF8);
                });
                endpoints.MapGet("/login", async context =>
                {
                    context.Response.ContentType = "image/png";
                    if(File.Exists("./BiliQR.png"))
                    {
                        await context.Response.SendFileAsync("./BiliQR.png"); 
                    }
                   else
                    {
                        await context.Response.WriteAsync("<a>二维码加载失败，请稍等3秒后刷新网页,如多次失败，请查看控制台是否输出错误信息<a/>", System.Text.Encoding.UTF8);
                    }
                });
                endpoints.MapGet("/config", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("访问以下链接以修改配置:<br/>(修改后请重启DDTVLiveRec生效)<br/>" +
                        "<br/>打开弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOn" +
                        "<br/>关闭弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOff");
                });
                endpoints.MapGet("/config-DanmuRecOn", async context =>
                {
                    Auxiliary.MMPU.录制弹幕 = true;
                    Auxiliary.MMPU.setFiles("RecordDanmu", "1");
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("打开弹幕/礼物/舰队录制储存成功");
                });
                endpoints.MapGet("/config-DanmuRecOff", async context =>
                {
                    Auxiliary.MMPU.录制弹幕 = false;
                    Auxiliary.MMPU.setFiles("RecordDanmu", "0");
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("关闭弹幕/礼物/舰队录制储存成功");
                });
            });
        }
    }
}
