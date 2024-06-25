using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using Server;
using Core.LogModule;
using Core;
using static Server.Program;
using static Core.Network.Methods.User.UserInfo;
using Wpf.Ui;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Http.Features;
using System.IO;



namespace Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider _MainSnackbarServiceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
                base.OnStartup(e);
                var services = new ServiceCollection();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                _MainSnackbarServiceProvider = services.BuildServiceProvider();
                Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        Task.Run(() => Service.CreateHostBuilder(new string[] { "--StartMode=Desktop" }).Build().Run());
                        webBuilder.UseStartup<Server.Startup>();
                        string rurl = $"{Config.Core_RunConfig._IP}:{Config.Core_RunConfig._Port}";
                        webBuilder.UseUrls(rurl);
                        Log.Info(nameof(Application), $"WebApplication开始运行，开始监听[{rurl}]");
                        Log.Info(nameof(Application), $"本地访问请浏览器打开[ http://127.0.0.1:{Config.Core_RunConfig._Port} ]");
                    }).Build().RunAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Core发生重大错误，错误堆栈:\n{ex.ToString()}");
            }
        }

    }

}
