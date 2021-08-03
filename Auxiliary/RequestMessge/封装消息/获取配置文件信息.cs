using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 获取配置文件信息
    {
        public static string 配置文件信息()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            List<List<string>> config = new List<List<string>>();
            string[] B = configuration.AppSettings.Settings.AllKeys;
            List<System_Core.systemConfig> systemConfig = new List<System_Core.systemConfig>();
            foreach (var item in B)
            {
                systemConfig.Add(new System_Core.systemConfig
                {
                    Key = item,
                    Value = configuration.AppSettings.Settings[item].Value

                });
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, systemConfig);
        }
    }
}
