using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 消息_获取配置文件信息
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
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, systemConfig);
        }
    }
}
