using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DDTVLiveRecWebServer
{
    public class Startup
    {
        public static string 返回标签内容 = "<a href=\"./systeminfo\"><input type=\"button\" value='返回概况页'></a><br/><br/>";
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseFileServer(new FileServerOptions()//直接开启文件目录访问和文件访问
            {
                EnableDirectoryBrowsing = true,//开启目录访问
                FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory+@"/tmp"),
                RequestPath = new PathString("/tmp")
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/log", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    if (File.Exists("./LOG/DDTVLiveRecLog.out"))
                    {
                        Auxiliary.MMPU.文件删除委托("./LOG/DDTVLiveRecLog.out.bak", "生成新的log文件1，删除老旧log文件");
                        File.Copy("./LOG/DDTVLiveRecLog.out", "./LOG/DDTVLiveRecLog.out.bak");
                        string fileText = File.ReadAllText("./LOG/DDTVLiveRecLog.out.bak", System.Text.Encoding.UTF8);
                        fileText = fileText.Replace("\r\n", "<br/>").Replace(" ", "&nbsp;");
                        await context.Response.WriteAsync(返回标签内容 + fileText);
                        Auxiliary.MMPU.文件删除委托("./LOG/DDTVLiveRecLog.out.bak", "生成新的log文件2，删除老旧log文件");
                        return;
                    }
                    else
                    {
                        await context.Response.WriteAsync("没有获取到日志文件，请确认DDTVLive正在运行");
                    }
                });
                endpoints.MapGet("/file", async context =>
                {
                    if (!Directory.Exists(Auxiliary.MMPU.缓存路径))
                    {
                        Directory.CreateDirectory(Auxiliary.MMPU.缓存路径);
                    }
                    string A = "当前录制文件夹文件列表:<br/>";
                    context.Response.ContentType = "text/html; charset=utf-8";
                    foreach (DirectoryInfo NextFolder1 in new DirectoryInfo(Auxiliary.MMPU.缓存路径).GetDirectories())
                    {
                        A = A + "<br/>" + NextFolder1.Name;
                        foreach (FileInfo NextFolder2 in new DirectoryInfo(Auxiliary.MMPU.缓存路径 + NextFolder1.Name).GetFiles())
                        {
                            string FileUrl = Auxiliary.MMPU.缓存路径 + NextFolder1.Name.Replace("+", "ddtvfuhaojia").Replace(" ", "ddtvfuhaokongge").Replace("/", "ddtvfuhaoxiegang").Replace("?", "ddtvfuhaowenhao").Replace("%", "ddtvfuhaobaifenhao").Replace("#", "ddtvfuhaojinhao").Replace("&", "ddtvfuhaoand").Replace("%", "ddtvfuhaobaifenhao") + "/" + NextFolder2.Name.Replace("+", "ddtvfuhaojia").Replace(" ", "ddtvfuhaokongge").Replace("/", "ddtvfuhaoxiegang").Replace("?", "ddtvfuhaowenhao").Replace("%", "ddtvfuhaobaifenhao").Replace("#", "ddtvfuhaojinhao").Replace("&", "ddtvfuhaoand").Replace("%", "ddtvfuhaobaifenhao");
                            FileUrl = FileUrl;
                            A = A + "<br/>&nbsp;&nbsp;" + Math.Ceiling(NextFolder2.Length / 1024.0 / 1024.0) + " MB |" + "<a href=\"./play?FileUrl=" + FileUrl + "&Title=" + NextFolder2.Name + "\" target=\"_blank\">" + NextFolder2.Name + "</a>";


                        }
                        A = A + "<br/>";
                    }
                    await context.Response.WriteAsync(返回标签内容 + A, System.Text.Encoding.UTF8);
                });
                string 播放路径 = "";
                endpoints.MapGet("/play", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    string FileUrl = context.Request.Query["FileUrl"];
                    if(!string.IsNullOrEmpty(FileUrl))
                    {
                        FileUrl= FileUrl.Replace("ddtvfuhaojia", "%2B").Replace("ddtvfuhaokongge", "%20").Replace("ddtvfuhaxiegango", "%2F").Replace("ddtvfuhaowenhao", "%3F").Replace("ddtvfuhaobaifenhao", "%25").Replace("ddtvfuhaojinhao", "%23").Replace("ddtvfuhaoand", "%26").Replace("ddtvfuhaobaifenhao", "%3D");
                    }
                    string Title = context.Request.Query["Title"];
                    string Prompt = context.Request.Query["Prompt"];
                    if (string.IsNullOrEmpty(Prompt))
                    {
                        Prompt = "这是在播放录制的FLV视频,因为阿B本身推流时间轴和推流方编码设置等因素影响，可能会出现：无法加载视频、无法拖动时间轴、无法显示总时长的问题";
                    }
                    string fileText = File.ReadAllText("./play.html", System.Text.Encoding.UTF8);
                    fileText = fileText.Replace("%这是标题%", Title);
                    fileText = fileText.Replace("%D这是提示%", Prompt);
                    fileText = fileText.Replace("%播放路径%", FileUrl);
                    await context.Response.WriteAsync(fileText, System.Text.Encoding.UTF8);
                });
                endpoints.MapGet("/flv.js", async context =>
                {
                    context.Response.ContentType = "application/x-javascript";
                    await context.Response.WriteAsync(File.ReadAllText("./flv.js", System.Text.Encoding.UTF8), System.Text.Encoding.UTF8);
                });
                endpoints.MapGet("/list", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(返回标签内容 + Auxiliary.InfoLog.DownloaderInfoPrintf(0), System.Text.Encoding.UTF8);
                });
                endpoints.MapGet("/wssinfo", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(返回标签内容 + Auxiliary.InfoLog.返回WSS连接状态列表(), System.Text.Encoding.UTF8);
                });
                //endpoints.MapGet("/login", async context =>
                //{
                //    context.Response.ContentType = "image/png";
                //    if(File.Exists("./BiliQR.png"))
                //    {
                //        await context.Response.SendFileAsync("./BiliQR.png"); 
                //    }
                //   else
                //    {
                //        await context.Response.WriteAsync("<a>二维码加载失败，请稍等3秒后刷新网页,如多次失败，请查看控制台是否输出错误信息<a/>", System.Text.Encoding.UTF8);
                //    }
                //});
                endpoints.MapGet("/systeminfo", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    string 跳转url = "<a href=\"./list\"><input type=\"button\" value='下载详情'></a>   " +
                    "<a href =\"./file\"><input type=\"button\" value='下载文件列表'></a>   " +
                    "<a href =\"./log\"><input type=\"button\" value='日志'></a>   " +
                    "<a href =\"./config\"><input type=\"button\" value='可修改配置'></a>   " +
                    "<a href =\"./wssinfo\"><input type=\"button\" value='特殊wss连接列表'></a>   " +
                    "<br/><br/>";
                    if (Auxiliary.MMPU.启动模式 == 1)
                    {
                        await context.Response.WriteAsync(跳转url + Auxiliary.InfoLog.GetSystemInfo());
                    }
                    else if (Auxiliary.MMPU.启动模式 == 2)
                    {

                    }

                });
                endpoints.MapGet("/config", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(返回标签内容 + "访问以下链接以修改配置:<br/>(修改后请重启DDTVLiveRec生效)<br/>" +
                        "<br/>打开弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOn" +
                        "<br/>关闭弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOff" +
                        "<br/>打开DEBUG模式 IP:11419/config-DebugOn" +
                        "<br/>关闭DEBUG模式 IP:11419/config-DebugOff");
                });
                endpoints.MapGet("/config-DanmuRecOn", async context =>
                {
                    Auxiliary.MMPU.录制弹幕 = true;
                    Auxiliary.MMPU.setFiles("RecordDanmu", "1");
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(返回标签内容 + "打开弹幕/礼物/舰队录制储存成功");
                });
                endpoints.MapGet("/config-DanmuRecOff", async context =>
                {
                    Auxiliary.MMPU.录制弹幕 = false;
                    Auxiliary.MMPU.setFiles("RecordDanmu", "0");
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(返回标签内容 + "关闭弹幕/礼物/舰队录制储存成功");
                });
                endpoints.MapGet("/config-DebugOn", async context =>
                {
                    Auxiliary.InfoLog.ClasslBool.Debug = true;
                    Auxiliary.InfoLog.ClasslBool.输出到文件 = true;
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(返回标签内容 + "Debug模式启动，该模式下会在log文件和终端输出大量log信息，请注意文件体积，重启默认关闭debug模式");
                });
                endpoints.MapGet("/config-DebugOff", async context =>
                {
                    Auxiliary.InfoLog.ClasslBool.Debug = false;
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(返回标签内容 + "Debug模式已关闭");
                });
                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    string 跳转url = "<a href=\"./list\"><input type=\"button\" value='下载详情'></a>   " +
                    "<a href =\"./file\"><input type=\"button\" value='下载文件列表'></a>   " +
                    "<a href =\"./log\"><input type=\"button\" value='日志'></a>   " +
                    "<a href =\"./config\"><input type=\"button\" value='可修改配置'></a>   " +
                    "<a href =\"./wssinfo\"><input type=\"button\" value='特殊wss连接列表'></a>   " +
                    "<br/><br/>";
                    if (Auxiliary.MMPU.启动模式 == 1)
                    {
                        await context.Response.WriteAsync(跳转url + Auxiliary.InfoLog.GetSystemInfo());
                    }
                });
            });
        }
    }
}
