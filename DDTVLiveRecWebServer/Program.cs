using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DDTVLiveRecWebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Auxiliary.InfoLog.InfoPrintf($"DDTVLiveRecWebServer启动成功，开始监听{Auxiliary.MMPU.webServer默认监听端口}端口", Auxiliary.InfoLog.InfoClass.下载必要提示);
            CreateHostBuilder(args).Build().Run();     
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://"+Auxiliary.MMPU.webServer默认监听IP+":"+ Auxiliary.MMPU.webServer默认监听端口);
                    //webBuilder.UseStartup<Startup>();
                });
    }
}