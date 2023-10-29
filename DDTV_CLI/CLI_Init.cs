using DDTV_Core;
using DDTV_Core.SystemAssembly.BilibiliModule.API;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.Tool;
using DDTV_Core.Tool.TranscodModule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DDTV_CLI
{
    
    public class CLI_Init
    {
        internal static string[] _args = default;
        public static void Main(string[] args)
        {
            _args = args;
            //Test();
            Service.CreateHostBuilder(new string[] { "CLI" }).Build().Run();
        }
        public static string GetPackageVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static void Test()
        {
            Console.WriteLine(
                "请注意：该操作会将目录下的所有flv以及mp4文件全部处理一次，但不会修改原始文件\r\n" +
                "请输入待修复的路径后按回车结束:"
                );

            Console.WriteLine();
            string Dir = Console.ReadLine();
            if (Directory.Exists(Dir))
            {
                DirectoryInfo root = new DirectoryInfo(Dir);

                foreach (var fileInfo in root.GetFiles())
                {
                    if (fileInfo.Extension.ToLower() == ".flv" || fileInfo.Extension.ToLower() == ".mp4")
                    {
                        Console.WriteLine($"检测到视频文件:{fileInfo.Name}，开始修复");
                        bool IsMp4File = fileInfo.Extension.ToLower() == ".mp4" ? true : false;
                        Transcod.CallFFMPEG(new TranscodClass()
                        {
                            AfterFilenameExtension = ".mp4",
                            AfterFilePath = IsMp4File ? fileInfo.FullName.Replace("\\", "/").Replace(".mp4", "_fix.mp4") : fileInfo.FullName.Replace("\\", "/").Replace(".flv", "_fix.mp4"),
                            BeforeFilePath = fileInfo.FullName.Replace("\\", "/"),
                        });
                        Console.WriteLine($"{fileInfo.Name}，修复完成");
                    }
                }
                Console.WriteLine($"该路径下符合条件的视频文件全部修复完成，按任意键退出");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine($"输入的路径不存在，按任意键退出");
                Console.ReadKey();
            }

        }
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
                return Task.Run(() =>
                {
                    InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_CLI, CLI_Init._args.Contains("--no-update"));
                    var response = JsonConvert.DeserializeObject<ApiClass.BilibiliApiResponse<ApiClass.RoomPlayInfo>>(File.ReadAllText("./TEST.txt"));
                });
            }

            public override Task StopAsync(CancellationToken stoppingToken)
            {
                return Task.Run(() =>
                {
                    Log.AddLog(nameof(InitDDTV_Core), LogClass.LogType.Warn, "收到SIGINT信号(一般是用户Ctrl+C)，主进程被系统结束");
                });
            }
            
            
        }
    }
}