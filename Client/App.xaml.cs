using Core;
using Core.LogModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using static Server.Program;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
         protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    Task.Run(() => Service.CreateHostBuilder(new string[] { "--StartMode=Client" }).Build().Run());
                    webBuilder.UseStartup<Server.Startup>();
                    string rurl = $"http://0.0.0.0:{Config.Core_RunConfig._Port}";
                    webBuilder.UseUrls(rurl);
                    Log.Info(nameof(Application), $"WebApplication开始运行，开始监听[{rurl}]");
                    Log.Info(nameof(Application), $"本地访问请浏览器打开[ http://127.0.0.1:{Config.Core_RunConfig._Port} ]");
                }).Build().RunAsync();
        }
    }

}
