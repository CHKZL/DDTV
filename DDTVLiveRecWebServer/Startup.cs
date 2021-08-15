using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DDTVLiveRecWebServer
{
    public class Startup
    {
        public static string 返回标签内容 = "<a href=\"./\"><input type=\"button\" value='返回概况页'></a><br/><br/>";
        public static string 验证KEY预设 = "DDTVLiveRec";
        string mobileAdaptationHeader = "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=10.0, user-scalable=yes\" />";

        public const string POLICY_NAME = "xxx";
        public static string MDtoHTML(string MD)
        {
            return CommonMark.CommonMarkConverter.Convert(MD);
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //注册MVC服务
            services.AddMvc();
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    //builder.AllowAnyOrigin() //允许任何来源的主机访问
                    builder

                    .WithOrigins("http://*.*.*.*", "https://*.*.*.*")//.SetIsOriginAllowedToAllowWildcardSubdomains()//设置允许访问的域
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();//
                });

            });
            services.AddControllers();
            //注册Cookie认证服务
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
            {
                option.AccessDeniedPath = "/LoginErrer"; //当用户尝试访问资源但没有通过任何授权策略时，这是请求会重定向的相对路径资源
                option.LoginPath = "/login/";
                option.Cookie.Name = 验证KEY预设;//设置存储用户登录信息（用户Token信息）的Cookie名称
                option.Cookie.HttpOnly = true;//设置存储用户登录信息（用户Token信息）的Cookie，无法通过客户端浏览器脚本(如JavaScript等)访问到
                                              //option.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;//设置存储用户登录信息（用户Token信息）的Cookie，只会通过HTTPS协议传递，如果是HTTP协议，Cookie不会被发送。注意，option.Cookie.SecurePolicy属性的默认值是Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
            });
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = int.Parse(Auxiliary.MMPU.webServer默认监听端口);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (Auxiliary.MMPU.是否启用SSL)
            {
                app.UseHttpsRedirection();
            }
            app.UseRouting();
            app.UseMiddleware<CorsMiddleware>();
            app.UseCors();
           
            if(!Directory.Exists("./static"))
            {
                Directory.CreateDirectory("./static");
            }
            //提供WEB服务器必要的资源文件，该文件夹的文件不会进过鉴权
            app.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = false,//关闭目录结构树访问权限
                FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory + @"/static"),
                RequestPath = new PathString("/static")
            });

            app.UseWhen(
             c => c.Request.Path.Value.Contains("tmp"),
             _ => _.UseMiddleware<AuthorizeStaticFilesMiddleware>()
             );
            if (!Directory.Exists("./tmp"))
            {
                Directory.CreateDirectory("./tmp");
            }
            app.UseFileServer(new FileServerOptions()//直接开启文件目录访问和文件访问
            {
                EnableDirectoryBrowsing = false,//权限目录访问
                FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory + @"/tmp"),
                RequestPath = new PathString("/tmp")
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", async context =>
                {

                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(MDtoHTML("* 就是一个测试页，你看咩啊？"), System.Text.Encoding.UTF8);

                });

                #region API接口
                endpoints.MapGet("/loginqr", async context =>
                {
                    await context.Response.SendFileAsync("./BiliQR.png");
                });
                endpoints.MapPost("/api/weblogin", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.WebLogin.Web(context));
                });
                endpoints.MapPost("/api/system_info", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.system_info.Web(context));
                });
                endpoints.MapPost("/api/system_resource_monitoring", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.system_resource_monitoring.Web(context));
                });
                endpoints.MapPost("/api/system_config", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.system_config.Web(context));
                });
                endpoints.MapPost("/api/system_update", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.system_update.Web(context));
                });
                endpoints.MapPost("/api/system_log", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.system_log.Web(context));
                });
                endpoints.MapPost("/api/rec_processing_list", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.rec_processing_list.Web(context));
                });
                endpoints.MapPost("/api/rec_all_list", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.rec_all_list.Web(context));
                });
                endpoints.MapPost("/api/rec_info", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.rec_info.Web(context));
                });
                endpoints.MapPost("/api/rec_cancel", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.rec_cancel.Web(context));
                });
                endpoints.MapPost("/api/room_add", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.room_add.Web(context));
                });
                endpoints.MapPost("/api/room_delete", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.room_delete.Web(context));
                });
                endpoints.MapPost("/api/room_status", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.room_status.Web(context));
                });
                endpoints.MapPost("/api/room_list", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.room_list.Web(context));
                });
                endpoints.MapPost("/api/file_lists", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.file_lists.Web(context));
                });
                endpoints.MapPost("/api/file_range", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.file_range.Web(context));
                });
                endpoints.MapPost("/api/file_delete", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.file_delete.Web(context));
                });
                endpoints.MapPost("/api/upload_list", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.upload_list.Web(context));
                });
                endpoints.MapPost("/api/upload_ing", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(API.upload_ing.Web(context));
                });
                #endregion

                #region webhook
                endpoints.MapPost("/webhooktest", async context =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync("{\"mgessg\":\"嗯？\"}");
                });
                #endregion
                #region GET
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(File.ReadAllText("./index.html"));
                });
                #endregion
                #region 历史请求(已废弃)，废弃时间2021-07-17
                //endpoints.MapGet("/log", async context =>
                //{
                //    //str:markdown文本

                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        if (File.Exists("./LOG/DDTVLiveRecLog.out"))
                //        {
                //            Auxiliary.MMPU.文件删除委托("./LOG/DDTVLiveRecLog.out.bak", "生成新的log文件1，删除老旧log文件");
                //            File.Copy("./LOG/DDTVLiveRecLog.out", "./LOG/DDTVLiveRecLog.out.bak");
                //            string fileText = "";
                //            foreach (var line in File.ReadLines("./LOG/DDTVLiveRecLog.out", System.Text.Encoding.UTF8).Reverse())//log倒序输出
                //            {
                //                if (line == "") continue;
                //                fileText = fileText + "<br/>" + line;
                //            }
                //            fileText = fileText.Replace(" ", "&nbsp;");
                //            await context.Response.WriteAsync(mobileAdaptationHeader + 返回标签内容 + fileText);
                //            Auxiliary.MMPU.文件删除委托("./LOG/DDTVLiveRecLog.out.bak", "生成新的log文件2，删除老旧log文件");
                //            return;
                //        }
                //        else
                //        {
                //            await context.Response.WriteAsync("没有获取到日志文件，请确认DDTVLive正在运行");
                //        }
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/file", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 1)
                //    {
                //        if (!Directory.Exists(Auxiliary.MMPU.缓存路径))
                //        {
                //            Directory.CreateDirectory(Auxiliary.MMPU.缓存路径);
                //        }
                //        string A = "当前录制文件夹文件列表:<br/>";
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        foreach (DirectoryInfo NextFolder1 in new DirectoryInfo(Auxiliary.MMPU.缓存路径).GetDirectories())
                //        {
                //            bool 换行 = false;
                //            A = A + "<br/>" + NextFolder1.Name;
                //            foreach (FileInfo NextFolder2 in new DirectoryInfo(Auxiliary.MMPU.缓存路径 + NextFolder1.Name).GetFiles())
                //            {
                //                if (Auxiliary.MMPU.转码功能使能)
                //                {
                //                    if (NextFolder2.Name.Substring(NextFolder2.Name.Length - 4, 4) != ".flv")
                //                    {
                //                        换行 = true;
                //                        string FileUrl = Auxiliary.MMPU.缓存路径 + NextFolder1.Name.Replace("+", "ddtvfuhaojia").Replace(" ", "ddtvfuhaokongge").Replace("/", "ddtvfuhaoxiegang").Replace("?", "ddtvfuhaowenhao").Replace("%", "ddtvfuhaobaifenhao").Replace("#", "ddtvfuhaojinhao").Replace("&", "ddtvfuhaoand").Replace("%", "ddtvfuhaobaifenhao") + "/" + NextFolder2.Name.Replace("+", "ddtvfuhaojia").Replace(" ", "ddtvfuhaokongge").Replace("/", "ddtvfuhaoxiegang").Replace("?", "ddtvfuhaowenhao").Replace("%", "ddtvfuhaobaifenhao").Replace("#", "ddtvfuhaojinhao").Replace("&", "ddtvfuhaoand").Replace("%", "ddtvfuhaobaifenhao");
                //                        A = A + "<br/>&nbsp;&nbsp;" + Math.Ceiling(NextFolder2.Length / 1024.0 / 1024.0) + " MB |" + "<a href=\"./play?FileUrl=" + FileUrl + "&Title=" + NextFolder2.Name + "\" target=\"_blank\">" + NextFolder2.Name + "</a>";
                //                    }
                //                }
                //                else
                //                {
                //                    换行 = true;
                //                    string FileUrl = Auxiliary.MMPU.缓存路径 + NextFolder1.Name.Replace("+", "ddtvfuhaojia").Replace(" ", "ddtvfuhaokongge").Replace("/", "ddtvfuhaoxiegang").Replace("?", "ddtvfuhaowenhao").Replace("%", "ddtvfuhaobaifenhao").Replace("#", "ddtvfuhaojinhao").Replace("&", "ddtvfuhaoand").Replace("%", "ddtvfuhaobaifenhao") + "/" + NextFolder2.Name.Replace("+", "ddtvfuhaojia").Replace(" ", "ddtvfuhaokongge").Replace("/", "ddtvfuhaoxiegang").Replace("?", "ddtvfuhaowenhao").Replace("%", "ddtvfuhaobaifenhao").Replace("#", "ddtvfuhaojinhao").Replace("&", "ddtvfuhaoand").Replace("%", "ddtvfuhaobaifenhao");
                //                    A = A + "<br/>&nbsp;&nbsp;" + Math.Ceiling(NextFolder2.Length / 1024.0 / 1024.0) + " MB |" + "<a href=\"./play?FileUrl=" + FileUrl + "&Title=" + NextFolder2.Name + "\" target=\"_blank\">" + NextFolder2.Name + "</a>";
                //                }
                //            }
                //            if (换行)
                //                A = A + "<br/>";
                //        }
                //        await context.Response.WriteAsync(mobileAdaptationHeader + 返回标签内容 + A, System.Text.Encoding.UTF8);
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/play", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 1)
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        string Title = context.Request.Query["Title"];
                //        string Prompt = context.Request.Query["Prompt"];
                //        string FileUrl = context.Request.Query["FileUrl"];
                //        if (!string.IsNullOrEmpty(FileUrl))
                //        {
                //            FileUrl = FileUrl.Replace("ddtvfuhaojia", "%2B").Replace("ddtvfuhaokongge", "%20").Replace("ddtvfuhaxiegango", "%2F").Replace("ddtvfuhaowenhao", "%3F").Replace("ddtvfuhaobaifenhao", "%25").Replace("ddtvfuhaojinhao", "%23").Replace("ddtvfuhaoand", "%26").Replace("ddtvfuhaobaifenhao", "%3D");
                //        }

                //        if (string.IsNullOrEmpty(Prompt))
                //        {
                //            Prompt = "因为阿B本身推流时间轴和推流方编码设置等因素影响，可能会出现：无法加载视频、无法拖动时间轴、无法显示总时长的问题<br/>(注：默认录制文件夹为./tmp，如果修改了录制文件夹路径，则不支持在线播放，问就是因为UseFileServer安全限制，不能用相对路径)";
                //        }
                //        if (File.Exists("./play.html"))
                //        {
                //            string fileText = File.ReadAllText("./play.html", System.Text.Encoding.UTF8);
                //            fileText = fileText.Replace("%这是标题%", Title);
                //            fileText = fileText.Replace("%D这是提示%", Prompt);
                //            fileText = fileText.Replace("%播放路径%", FileUrl);
                //            fileText = fileText.Replace("%这是文件地址%", FileUrl);
                //            await context.Response.WriteAsync(mobileAdaptationHeader + fileText, System.Text.Encoding.UTF8);
                //        }
                //        else
                //        {
                //            await context.Response.WriteAsync(mobileAdaptationHeader + "播放解析文件不存在，请确认目录下有[play.html]文件；该文件包含播放视频所需的布局文件以及MP4解析脚本", System.Text.Encoding.UTF8);
                //        }
                //    }
                //    else
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        await context.Response.WriteAsync(mobileAdaptationHeader + "权限验证未通过", System.Text.Encoding.UTF8);
                //    }
                //});
                //endpoints.MapGet("/login", async context =>
                //{
                //    context.Response.ContentType = "text/html; charset=utf-8";
                //    string html = Properties.Resources.loginHtml;
                //    await context.Response.WriteAsync(mobileAdaptationHeader + MDtoHTML(html), System.Text.Encoding.UTF8);
                //});
                //endpoints.MapGet("/loginACC", async context =>
                //{
                //    string ACC = context.Request.Query["ACC"];
                //    string KEY = "";
                //    int 登陆类型 = 0;
                //    if (ACC == Auxiliary.MMPU.webadmin验证字符串)
                //    {
                //        KEY = Auxiliary.MMPU.webadmin验证字符串;
                //        登陆类型 = 2;
                //    }
                //    else if (ACC == Auxiliary.MMPU.webghost验证字符串)
                //    {
                //        KEY = Auxiliary.MMPU.webghost验证字符串;
                //        登陆类型 = 1;
                //    }
                //    switch (登陆类型)
                //    {
                //        case 0:
                //            {
                //                context.Response.Redirect("LoginErrer");
                //                break;
                //            }
                //        case 1:
                //            {
                //                LoginInputCookie(context, KEY);
                //                context.Response.Redirect("File");
                //                break;
                //            }
                //        case 2:
                //            {
                //                LoginInputCookie(context, KEY);
                //                context.Response.Redirect("/");
                //                break;
                //            }
                //        default:
                //            {
                //                context.Response.Redirect("Error!");
                //                break;
                //            }
                //    }
                //});
                //endpoints.MapGet("/LoginErrer", async context =>
                //{
                //    context.Response.ContentType = "text/html; charset=utf-8";
                //    string OUTTEST = "<br/>使用WEB端需要验证，验证请访问:<br/><a href=\"./login\"><input type=\"button\" value='鉴权登陆页'></a>";
                //    await context.Response.WriteAsync(mobileAdaptationHeader + "<H1>权限验证失败!!!</H1>" + OUTTEST, System.Text.Encoding.UTF8);
                //});
                //endpoints.MapGet("/upload", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        if (Auxiliary.Upload.Uploader.enableUpload == true)
                //            await context.Response.WriteAsync(mobileAdaptationHeader + 返回标签内容 + Auxiliary.InfoLog.UploaderInfoPrintf(0), System.Text.Encoding.UTF8);
                //        else
                //            await context.Response.WriteAsync(mobileAdaptationHeader + 返回标签内容 + "未开启上传功能", System.Text.Encoding.UTF8);
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/list", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        await context.Response.WriteAsync(mobileAdaptationHeader + 返回标签内容 + Auxiliary.InfoLog.DownloaderInfoPrintf(0), System.Text.Encoding.UTF8);
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/wssinfo", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        await context.Response.WriteAsync(mobileAdaptationHeader + 返回标签内容 + Auxiliary.InfoLog.返回WSS连接状态列表(), System.Text.Encoding.UTF8);
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/config", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        string HTML = 返回标签内容;
                //        string command = context.Request.Query["command"];
                //        switch (command)
                //        {
                //            case "":
                //                {
                //                    break;
                //                }
                //            default:
                //                HTML += "访问以下链接以修改配置:<br/>(修改后请重启DDTVLiveRec生效)<br/>" +
                //            "<br/>打开弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOn" +
                //            "<br/>关闭弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOff" +
                //            "<br/>打开DEBUG模式 IP:11419/config-DebugOn" +
                //            "<br/>关闭DEBUG模式 IP:11419/config-DebugOff";
                //                break;
                //        }
                //        await context.Response.WriteAsync(HTML);
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }

                //});
                //endpoints.MapGet("/config-DanmuRecOn", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        Auxiliary.MMPU.录制弹幕 = true;
                //        Auxiliary.MMPU.setFiles("RecordDanmu", "1");
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        await context.Response.WriteAsync(返回标签内容 + "打开弹幕/礼物/舰队录制储存成功");
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/config-DanmuRecOff", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        Auxiliary.MMPU.录制弹幕 = false;
                //        Auxiliary.MMPU.setFiles("RecordDanmu", "0");
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        await context.Response.WriteAsync(返回标签内容 + "关闭弹幕/礼物/舰队录制储存成功");
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/config-DebugOn", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        Auxiliary.InfoLog.ClasslBool.Debug = true;
                //        Auxiliary.InfoLog.ClasslBool.是否将日志输出到文件 = true;
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        await context.Response.WriteAsync(返回标签内容 + "Debug模式启动，该模式下会在log文件和终端输出大量log信息，请注意文件体积，重启默认关闭debug模式");
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/config-DebugOff", async context =>
                //{
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        Auxiliary.InfoLog.ClasslBool.Debug = false;
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        await context.Response.WriteAsync(返回标签内容 + "Debug模式已关闭");
                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                //endpoints.MapGet("/", async context =>
                //{
                //    //Console.WriteLine("进入了根页面");
                //    if (ACCAsync(context, 验证KEY预设) >= 2)
                //    {
                //        context.Response.ContentType = "text/html; charset=utf-8";
                //        string 跳转url = "<a href=\"./list\"><input type=\"button\" value='下载详情'></a>   " +
                //        "<a href =\"./upload\"><input type=\"button\" value='上传详情'></a>   " +
                //        "<a href =\"./file\"><input type=\"button\" value='下载文件列表'></a>   " +
                //        "<a href =\"./log\"><input type=\"button\" value='日志'></a>   " +
                //        "<a href =\"./roomlist\"><input type=\"button\" value='房间配置'></a>   " +
                //        //"<a href =\"./config\"><input type=\"button\" value='可修改配置'></a>   " +
                //        //"<a href =\"./wssinfo\"><input type=\"button\" value='特殊wss连接列表'></a>   " +
                //        "<br/><br/>";
                //        if (Auxiliary.MMPU.启动模式 == 1)
                //        {
                //            await context.Response.WriteAsync(mobileAdaptationHeader + 跳转url + Auxiliary.InfoLog.GetSystemInfo());
                //        }

                //    }
                //    else
                //    {
                //        context.Response.Redirect("LoginErrer");
                //    }
                //});
                #endregion
            });
        }
        #region 历史请求鉴权处理方法(已废弃)，废弃时间2021-07-17
        //public void LoginInputCookie(HttpContext context, string KEY)
        //{
        //    var claims = new[] {
        //                new Claim(验证KEY预设, KEY),
        //            };
        //    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //    ClaimsPrincipal user = new ClaimsPrincipal(claimsIdentity);

        //    Task.Run(async () =>
        //    {
        //        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user);
        //        await context.SignInAsync(
        //        CookieAuthenticationDefaults.AuthenticationScheme,
        //        user, new AuthenticationProperties()
        //        {
        //            IsPersistent = true,
        //            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
        //            AllowRefresh = true
        //        });
        //        //await context.Response.WriteAsync("登陆完成");
        //    }).Wait();
        //}
        //public int ACCAsync(HttpContext context, string Key)
        //{
        //    return 2;
        //    if (context.User.Identity.IsAuthenticated)   //HttpContext.User.Identities.Count()>0)
        //    {
        //        context.AuthenticateAsync();
        //        Console.WriteLine("1");
        //        Dictionary<string, string> currUser = context.User.Claims.ToDictionary(o => o.Type, o => o.Value);
        //        if (currUser.TryGetValue(Key, out string Cache))
        //        {
        //            Console.WriteLine("2");
        //            if (Cache == Auxiliary.MMPU.webadmin验证字符串)
        //            {
        //                Console.WriteLine("3");
        //                return 2;
        //            }
        //            if (Cache == Auxiliary.MMPU.webghost验证字符串)
        //            {
        //                Console.WriteLine("4");
        //                return 1;
        //            }
        //            else
        //            {
        //                Console.WriteLine("5");
        //                NotloginAsync(context);
        //                return 0;
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("6");
        //            NotloginAsync(context);
        //            return 0;
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("7");
        //        NotloginAsync(context);
        //        return 0;
        //    }
        //}
        //public void NotloginAsync(HttpContext context)
        //{
        //    context.Response.ContentType = "text/html; charset=utf-8";
        //    context.Response.Redirect("LoginErrer");
        //}
        #endregion

    }
}