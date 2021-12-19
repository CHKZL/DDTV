
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography.X509Certificates;

namespace DDTV_Server//DDTVLiveRecWebServer
{
    public class Program
    {
        private static bool IsSSL = false;
        private static string pfxFileName = "api.ddtv.pro.SSL";
        private static string pfxPasswordFileName = "api.ddtv.pro.key";
        public static void Main(string[] args)
        {

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureServices(Services =>
            {
                Services.AddControllers();
                Services.AddEndpointsApiExplorer();
                Services.AddSwaggerGen();
                Services.AddMvc();
                Services.AddControllers();

            });
            builder.Services.AddSwaggerGen();
            if (IsSSL)
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        var certPath = Path.Combine(builder.Environment.ContentRootPath, pfxFileName);
                        var keyPath = Path.Combine(builder.Environment.ContentRootPath, pfxPasswordFileName);
                        httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                    });
                });
            }
            var app = builder.Build();

            //用于检测是否为开发环境
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Urls.Add("http://0.0.0.0:10088");
            if (IsSSL)
            {
                app.Urls.Add("https://0.0.0.0:443");
            }
            app.Run();


        }
    }
}