using Core.LogModule;
using Core.RuntimeObject;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Core;
using Microsoft.AspNetCore.Http.Features;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.WebSockets;
using Server.WebAppServices.Middleware;
using Server.WebAppServices;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static Server.WebAppServices.MessageCode;
using System.Net.NetworkInformation;
using System.Net;

namespace Server
{
    public class Program
    {
        //public static event EventHandler<EventArgs> WebServerStartCompletEvent;//Web服务启动完成事件

        public static async Task Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                if (!Array.Exists(args, element => element.Contains("--StartMode")))
                {
                    string[] N_args = new string[args.Length + 1];
                    for (int i = 0; i < args.Length; i++)
                    {
                        N_args[i] = args[i];
                    }
                    N_args[N_args.Length - 1] = "--StartMode=Server";
                }
                if (!args.Contains("Desktop") && !args.Contains("Client"))
                {
                    Console.OutputEncoding = System.Text.Encoding.UTF8;
                }

                MainAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"系统触发出现了无法处理的错误，DDTV无法处理该问题，错误堆栈:{ex.ToString()}");
            }
        }

        public static async Task MainAsync(string[] args)
        {
            try
            {

                //注册DDTV主要服务
                Task.Run(() => Service.CreateHostBuilder(args).Build().Run());
                ServicePointManager.DefaultConnectionLimit = 4096 * 16;
                //等待Core启动后再启动WEB服务
                await Init.CoreStartAwait.Task;

                if (PortInspect())
                {
                    Console.WriteLine($"[ERROR]!!!启动失败！WEB端口[{Core.Config.Core_RunConfig._Port}]被占用，请检查端口或更换端口！！！");
                    Console.WriteLine($"[ERROR]!!!启动失败！WEB端口[{Core.Config.Core_RunConfig._Port}]被占用，请检查端口或更换端口！！！");
                    Console.WriteLine($"[ERROR]!!!启动失败！WEB端口[{Core.Config.Core_RunConfig._Port}]被占用，请检查端口或更换端口！！！");
                    return;
                }
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                builder.Services.Configure<FormOptions>(options =>
                {
                    options.ValueCountLimit = 1024 * 1024;
                });

                builder.Logging.AddFilter((category, level) =>
                {
                    return level >= LogLevel.Warning;
                });

                builder.Services.AddControllers();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "DDTV5_API",
                        License = new OpenApiLicense
                        {
                            Name = "[项目地址]",
                            Url = new Uri("https://github.com/CHKZL/DDTV")
                        }
                    });
                    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                });
                builder.Services.AddMvc();


                var app = builder.Build();

                app.UseWebSockets();
                app.UseMiddleware<WebSocketControl>();
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseMiddleware<AccessControl>();
                //app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                //app.UseStatusCodePagesWithRedirects("/api/not_found");
                //app.UseStatusCodePagesWithRedirects("/index");
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
                string rurl = $"{Config.Core_RunConfig._IP}:{Config.Core_RunConfig._Port}";
                app.Urls.Add(rurl);
                Log.Info(nameof(Main), $"WebApplication开始运行，开始监听[{rurl}]");
                Log.Info(nameof(Main), $"本地访问请浏览器打开[ http://127.0.0.1:{Config.Core_RunConfig._Port} ]");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Log.Info(nameof(Main), $"检测到当前是Windows环境，请确保终端的快捷编辑功能关闭，否则可能被误触导致该终端暂停运行");
                }
                WebAppServices.WS.WebSocketQueue.Start();
                Log.Info(nameof(Main), $"WebSocket，开始监听，路径：[/ws]");
                //Task.Run(() =>
                //{
                //    Thread.Sleep(1000);
                //    WebServerStartCompletEvent?.Invoke(null, new EventArgs());
                //});
                app.Run();

            }
            catch (Exception e)
            {
                Console.WriteLine($"出现无法解决的重大错误，这一般是由于硬件或者系统层面的问题导致的，DDTV被迫停止运行。错误消息：{e.ToString()}");
                if (Init.Mode != Config.Mode.Client && Init.Mode != Config.Mode.Desktop)
                {
                    Console.WriteLine($"按任意键退出");
                    if (Init.Mode!= Config.Mode.Desktop && Init.Mode!= Config.Mode.Client && Init.Mode!= Config.Mode.Docker)
                        Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// 检查Web的端口是否被占用
        /// </summary>
        /// <returns></returns>
        public static bool PortInspect()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            IPEndPoint[] ipsUDP = ipProperties.GetActiveUdpListeners();
            TcpConnectionInformation[] tcpConnInfoArray = ipProperties.GetActiveTcpConnections();
            foreach (var item in tcpConnInfoArray)
            {
                if (item.LocalEndPoint.Port == Core.Config.Core_RunConfig._Port)
                {
                    return true;
                }
            }
            return false;
        }


        public class Service
        {
            public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService(serviceProvider => new DDTVService(args));
                });
            public class DDTVService : BackgroundService
            {
                private readonly string[] _args;

                public DDTVService(string[] args)
                {
                    _args = args;
                }
                protected override Task ExecuteAsync(CancellationToken stoppingToken)
                {
                    return Task.Run(async () =>
                    {
                        try
                        {
                       
                            Core.Init.Start(_args);//初始化必须执行的
                                                   //_ParentProcessDetection();

                            //启动房间监听并且注册事件
                            Detect detect = new();
                            //控制台打印心跳日志
                            doki();
                            //终端和ws更新事件
                            Core.Tools.ProgramUpdates.NewVersionAvailableEvent += ProgramUpdates_NewVersionAvailableEvent;


                            if (Init.Mode!= Config.Mode.Desktop && Config.Core_RunConfig._DesktopRemoteServer)
                            {
                                return;
                            }
                            if (!Account.AccountInformation.State)
                            {
                                if(Init.Mode!= Config.Mode.Desktop && Init.Mode!= Config.Mode.Client && Init.Mode!= Config.Mode.Docker)
                                {
                                    Log.Info(nameof(DDTVService), "\r\n当前状态:未登录\r\n" +
                                        "使用前须知：\r\n" +
                                        "1、在使用本软件的过程中的产生的任何资料、数据等所有数据都归属原所有者。\r\n" +
                                        "2、本软件所使用的所有资源，以及服务，均搜集自互联网，版权属于相应的个体，我们只是基于互联网使用了公开的资源进行开发。\r\n" +
                                        "3、本软件所登陆的阿B账号仅保存在您本地，且只会用于和阿B的服务接口交互。\r\n" +
                                        "\r\n如果您了解且同意以上内容，请按Y进入登陆流程，按其他任意键退出\r\n");

                                    _UseAgree();

                                    while (!Core.Config.Core_RunConfig._UseAgree)
                                    {
                                        Thread.Sleep(500);
                                    }
                                    await Login.QR();//如果没有登录态，需要执行扫码
                                    while (!Account.AccountInformation.State)
                                    {
                                        Thread.Sleep(1000);//等待登陆
                                    }
                                }


                            }

                            if(Init.Mode!= Config.Mode.Desktop && Init.Mode!= Config.Mode.Client && Init.Mode!= Config.Mode.Docker)
                            {
                                TerminalDisplay.SeKey();
                            }
                        }
                        catch (Exception EX)
                        {
                            File.WriteAllText("./ERROR.TXT", EX.ToString());
                        }


                    });
                }

                private void ProgramUpdates_NewVersionAvailableEvent(object? sender, EventArgs e)
                {
                    OperationQueue.Add(Opcode.Config.UpdateDetect, $"检测到DDTV新版本：【{sender}】");
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

        /// <summary>
        /// WebUI或者Desktop模式下检测父进程状态，如果UI关闭则自动停止运行
        /// </summary>
        public static void _ParentProcessDetection()
        {
            Task.Run(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (Init.Mode == Config.Mode.Client || Init.Mode == Config.Mode.Desktop))
                {
                    while (true)
                    {
                        var parentProcessId = Core.Tools.SystemResource.GetProcess.GetParentProcess(Process.GetCurrentProcess().Id);
                        if (parentProcessId == 0)
                        {
                            Log.Error("ParentProcessDetection", $"GUI模式下父进程被关闭，强制结束CLI进程");
                            Thread.Sleep(3000);
                            Environment.Exit(-114514);
                        }
                        else
                        {
                            try
                            {
                                var parentProcess = Process.GetProcessById(parentProcessId);
                                //Log.Info("ParentProcessDetection", $"父进程ID: {parentProcess.Id}, 父进程名: {parentProcess.ProcessName}");
                            }
                            catch (ArgumentException)
                            {
                                Log.Error("ParentProcessDetection", $"GUI模式下父进程被关闭，强制结束CLI进程");
                                Thread.Sleep(3000);
                                Environment.Exit(-114514);
                            }
                        }
                        Thread.Sleep(1000 * 3);
                    }
                }
                return;
            });
        }


        /// <summary>
        /// 用于打印控制台心跳信息
        /// </summary>
        public static void doki()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var doki = Core.Tools.DokiDoki.GetDoki();
                    Log.Info("DokiDoki", $"总:{doki.Total}|录制中:{doki.Downloading}|使用内存:{doki.UsingMemoryStr}|{doki.InitType}|{doki.Ver}|{Enum.GetName(typeof(Config.Mode), doki.StartMode)}【{doki.CompilationMode}】(编译时间:{doki.CompiledVersion})");
                    if (doki.UsingMemory > 4294967296)
                    {
                        Log.Error("DokiDoki", $"检测到内存泄漏严重，3秒后自动停止运行");
                        Thread.Sleep(3000);
                        Environment.Exit(-114514);
                    }
#if DEBUG
                    Thread.Sleep(60 * 1000);
#else
                    Thread.Sleep(300 * 1000);
#endif
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    var doki = Core.Tools.DokiDoki.GetDoki();
                    MessageBase.MssagePack("dokidoki", doki, "dokidoki");
                    Thread.Sleep(30 * 1000);
                }
            });
        }

        /// <summary>
        /// 用户协议同意操作
        /// </summary>
        public static void _UseAgree()
        {
            Task.Run(() =>
            {

                while (true)
                {

                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    if (keyInfo.Key != ConsoleKey.Y)
                    {
                        if (!Core.Config.Core_RunConfig._UseAgree)
                        {
                            // 用户按了其他键，退出程序
                            Console.WriteLine("\n哔哩哔哩 (゜-゜)つロ 干杯~");
                            Environment.Exit(0);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.Y && !Core.Config.Core_RunConfig._UseAgree)
                    {
                        Core.Config.Core_RunConfig._UseAgree = true;
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            });
        }
    }
}
