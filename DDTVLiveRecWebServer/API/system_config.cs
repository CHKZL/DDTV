using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class system_config
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context,"system_config");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>((int)ReturnInfoPackage.MessgeCode.鉴权失败, null);
            }
            else
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                List<List<string>> config = new List<List<string>>();
                string[] B = configuration.AppSettings.Settings.AllKeys;
                List<systemConfig> systemConfig = new List<systemConfig>();
                foreach (var item in B)
                {
                    systemConfig.Add(new systemConfig {
                        Key= item,
                        Value = configuration.AppSettings.Settings[item].Value
                     
                    });
                }
                return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, systemConfig);
            }
        }
        private class Messge:ReturnInfoPackage.Messge<systemConfig>
        {
            public static new List<systemConfig> Package { set; get; }

        }
        private class systemConfig 
        {
            public string Key { set; get; }
            public string Value { set; get; }
        }
    }
}
