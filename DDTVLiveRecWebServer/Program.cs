using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DDTVLiveRecWebServer
{
    public class Program
    {
        private static IConfigurationRoot ConfigRoot;

        public static void Main(string[] args)
        {
            while (!Auxiliary.MMPU.webServer初始化状态)
            {
                Thread.Sleep(50);
            }
            Auxiliary.InfoLog.InfoPrintf($"DDTVLiveRecWebServer启动成功，开始监听{Auxiliary.MMPU.webServer默认监听端口}端口", Auxiliary.InfoLog.InfoClass.下载系统信息);

            if (Auxiliary.MMPU.是否启用SSL)
            {
                SSL证书方式启动(args).Build().Run();
                
            }
            else
            {
                非SSL启动(args).Build().Run();
            }
        }

        public static IHostBuilder 非SSL启动(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://" + Auxiliary.MMPU.webServer默认监听IP + ":" + Auxiliary.MMPU.webServer默认监听端口);
                    //webBuilder.UseStartup<Startup>();
                });

        public static IWebHostBuilder SSL证书方式启动(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
          .UseKestrel(option =>
          {
              option.ConfigureHttpsDefaults(i =>
              {
                  i.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2($"./{Auxiliary.MMPU.webServer_pfx证书名称}", Auxiliary.MMPU.webServer_pfx证书密码);
              });
          }).UseStartup<Startup>().UseUrls("https://" + Auxiliary.MMPU.webServer默认监听IP + ":" + Auxiliary.MMPU.webServer默认监听端口, "http://" + Auxiliary.MMPU.webServer默认监听IP + ":80");
    }
}