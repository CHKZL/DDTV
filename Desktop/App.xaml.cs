using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using Server;
using Core.LogModule;
using Core;
using static Server.Program;

namespace Desktop
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
                    Task.Run(() => Service.CreateHostBuilder(new string[] { "Desktop" }).Build().Run());
                    webBuilder.UseStartup<Server.Startup>();
                    string rurl = $"http://0.0.0.0:{Config.Web._Port}";
                    webBuilder.UseUrls(rurl);
                    Log.Info(nameof(Application), $"WebApplication开始运行，开始监听[{rurl}]");
                    Log.Info(nameof(Application), $"本地访问请浏览器打开[ http://127.0.0.1:{Config.Web._Port} ]");
                }).Build().RunAsync();
        }
    }

}
