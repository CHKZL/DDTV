using Core;
using Core.LogModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using Wpf.Ui;
using static Server.Program;



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
                // 获取启动参数
                string[] args = new string[e.Args.Length + 1];
                for (int i = 0; i < e.Args.Length; i++)
                {
                    args[i] = e.Args[i];
                }
                args[args.Length - 1] = "--StartMode=Desktop";
                Core.Init.StartParameterInitialization(args);
                var services = new ServiceCollection();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                _MainSnackbarServiceProvider = services.BuildServiceProvider();
                Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        Task.Run(() => Service.CreateHostBuilder(new string[] { }).Build().Run());

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
