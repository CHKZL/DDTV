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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DDTVLiveRecWebServer
{
    public class Startup
    {
        public static string 返回标签内容 = "<a href=\"./systeminfo\"><input type=\"button\" value='返回概况页'></a><br/><br/>";
        public static string 验证KEY预设 = "DDTVLiveRec";
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //注册MVC服务
            services.AddMvc();
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseCors();
            app.UseFileServer(new FileServerOptions()//直接开启文件目录访问和文件访问
            {
                EnableDirectoryBrowsing = false,//开启目录访问
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
                endpoints.MapGet("/log", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
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
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/file", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 1)
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
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                string 播放路径 = "";
                endpoints.MapGet("/play", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 1)
                    {
                        context.Response.ContentType = "text/html; charset=utf-8";
                        string Title = context.Request.Query["Title"];
                        string Prompt = context.Request.Query["Prompt"];
                        string FileUrl = context.Request.Query["FileUrl"];
                        if (!string.IsNullOrEmpty(FileUrl))
                        {
                            FileUrl = FileUrl.Replace("ddtvfuhaojia", "%2B").Replace("ddtvfuhaokongge", "%20").Replace("ddtvfuhaxiegango", "%2F").Replace("ddtvfuhaowenhao", "%3F").Replace("ddtvfuhaobaifenhao", "%25").Replace("ddtvfuhaojinhao", "%23").Replace("ddtvfuhaoand", "%26").Replace("ddtvfuhaobaifenhao", "%3D");
                        }

                        if (string.IsNullOrEmpty(Prompt))
                        {
                            Prompt = "这是在播放录制的FLV视频,因为阿B本身推流时间轴和推流方编码设置等因素影响，可能会出现：无法加载视频、无法拖动时间轴、无法显示总时长的问题";
                        }
                        string fileText = File.ReadAllText("./play.html", System.Text.Encoding.UTF8);
                        fileText = fileText.Replace("%这是标题%", Title);
                        fileText = fileText.Replace("%D这是提示%", Prompt);
                        fileText = fileText.Replace("%播放路径%", FileUrl);
                        await context.Response.WriteAsync(fileText, System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync("权限验证未通过", System.Text.Encoding.UTF8);
                    }
                });
                endpoints.MapGet("/login", async context =>
                {
                    string ACC = context.Request.Query["ACC"];
                    string KEY = "";
                    int 登陆类型 = 0;
                    if (ACC == Auxiliary.MMPU.webadmin验证字符串)
                    {
                        KEY = Auxiliary.MMPU.webadmin验证字符串;
                        登陆类型 = 2;
                    }
                    else if (ACC == Auxiliary.MMPU.webghost验证字符串)
                    {
                        KEY = Auxiliary.MMPU.webghost验证字符串;
                        登陆类型 = 1;
                    }
                    switch (登陆类型)
                    {
                        case 0:
                            {
                                context.Response.Redirect("LoginErrer");
                                break;
                            }
                        case 1:
                            {
                                LoginInputCookie(context, KEY);
                                context.Response.Redirect("File");
                                break;
                            }
                        case 2:
                            {
                                LoginInputCookie(context, KEY);
                                context.Response.Redirect("systeminfo");
                                break;
                            }
                        default:
                            {
                                context.Response.Redirect("Error!");
                                break;
                            }
                    }
                });
                endpoints.MapGet("/LoginErrer", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    string OUTTEST = "<br/><br/><br/>使用WEB端需要验证，验证请访问:<br/>http://IP:" + Auxiliary.MMPU.webServer默认监听端口+ "/login?ACC=这里填写验证码<br/><br/><br/>注:验证码是DDTVLiveRec.dll.config文件里的[WebAuthenticationGhostPasswrod]和[WebAuthenticationAadminPassword]的value<br/><br/>[WebAuthenticationGhostPasswrod]为游客验证，只能查看/file界面和进行播放预览<br/>[WebAuthenticationAadminPassword]是全功能管理员验证<br/><br/>两个值都能自行修改，修改后请重启DDTVLiveRec";
                    await context.Response.WriteAsync("<H1>权限验证失败!!!</H1><br/><br/>" + OUTTEST, System.Text.Encoding.UTF8);
                });
                endpoints.MapGet("/list", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
                    {
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(返回标签内容 + Auxiliary.InfoLog.DownloaderInfoPrintf(0), System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/wssinfo", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
                    {
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(返回标签内容 + Auxiliary.InfoLog.返回WSS连接状态列表(), System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/systeminfo", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
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
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/config", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
                    {
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(返回标签内容 + "访问以下链接以修改配置:<br/>(修改后请重启DDTVLiveRec生效)<br/>" +
                            "<br/>打开弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOn" +
                            "<br/>关闭弹幕/礼物/舰队录制储存IP:11419/config-DanmuRecOff" +
                            "<br/>打开DEBUG模式 IP:11419/config-DebugOn" +
                            "<br/>关闭DEBUG模式 IP:11419/config-DebugOff");
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }

                });
                endpoints.MapGet("/config-DanmuRecOn", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
                    {
                        Auxiliary.MMPU.录制弹幕 = true;
                        Auxiliary.MMPU.setFiles("RecordDanmu", "1");
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(返回标签内容 + "打开弹幕/礼物/舰队录制储存成功");
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/config-DanmuRecOff", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
                    {
                        Auxiliary.MMPU.录制弹幕 = false;
                        Auxiliary.MMPU.setFiles("RecordDanmu", "0");
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(返回标签内容 + "关闭弹幕/礼物/舰队录制储存成功");
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/config-DebugOn", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
                    {
                        Auxiliary.InfoLog.ClasslBool.Debug = true;
                        Auxiliary.InfoLog.ClasslBool.输出到文件 = true;
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(返回标签内容 + "Debug模式启动，该模式下会在log文件和终端输出大量log信息，请注意文件体积，重启默认关闭debug模式");
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/config-DebugOff", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
                    {
                        Auxiliary.InfoLog.ClasslBool.Debug = false;
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(返回标签内容 + "Debug模式已关闭");
                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
                endpoints.MapGet("/", async context =>
                {
                    if (ACCAsync(context, 验证KEY预设) >= 2)
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

                    }
                    else
                    {
                        context.Response.Redirect("LoginErrer");
                    }
                });
            });
        }
        public void LoginInputCookie(HttpContext context, string KEY)
        {
            var claims = new[] {
                        new Claim(验证KEY预设, KEY),
                    };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal user = new ClaimsPrincipal(claimsIdentity);

            Task.Run(async () =>
            {
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user);
                await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                user, new AuthenticationProperties()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    AllowRefresh = true
                });
                //await context.Response.WriteAsync("登陆完成");
            }).Wait();
        }
        public int ACCAsync(HttpContext context, string Key)
        {
            if (context.User.Identity.IsAuthenticated)   //HttpContext.User.Identities.Count()>0)
            {
                context.AuthenticateAsync();
                Dictionary<string, string> currUser = context.User.Claims.ToDictionary(o => o.Type, o => o.Value);
                if (currUser.TryGetValue(Key, out string Cache))
                {
                    if (Cache == Auxiliary.MMPU.webadmin验证字符串)
                    {
                        return 2;
                    }
                    if (Cache == Auxiliary.MMPU.webghost验证字符串)
                    {
                        return 1;
                    }
                    else
                    {
                        NotloginAsync(context);
                        return 0;
                    }
                }
                else
                {
                    NotloginAsync(context);
                    return 0;
                }
            }
            else
            {
                NotloginAsync(context);
                return 0;
            }
        }
        public void NotloginAsync(HttpContext context)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Redirect("LoginErrer");
        }
    }
}
