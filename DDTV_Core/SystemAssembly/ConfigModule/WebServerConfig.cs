using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class WebServerConfig
    {
        private static string _pfxFileName = string.Empty;
        private static string _pfxPasswordFileName = string.Empty;
        private static string _WebUserName = string.Empty;
        private static string _WebPassword = string.Empty;
        private static string _Cookis = string.Empty;
        private static string _AccessKeyId = string.Empty;
        private static string _AccessKeySecret = string.Empty;
        private static string _ServerAID = string.Empty;
        private static string _ServerName = string.Empty;
        public static bool IsSSL = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.WEB_API_SSL, "false", CoreConfigClass.Group.WEB_API));
        public static string AccessControlAllowOrigin = CoreConfig.GetValue(CoreConfigClass.Key.AccessControlAllowOrigin, "*", CoreConfigClass.Group.WEB_API);
        public static string AccessControlAllowCredentials = CoreConfig.GetValue(CoreConfigClass.Key.AccessControlAllowCredentials, "true", CoreConfigClass.Group.WEB_API);
        public static string CookieDomain = CoreConfig.GetValue(CoreConfigClass.Key.CookieDomain, string.Empty, CoreConfigClass.Group.WEB_API);

        public static string pfxFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_pfxFileName))
                {
                    _pfxFileName = CoreConfig.GetValue(CoreConfigClass.Key.pfxFileName, "pfxFileName", CoreConfigClass.Group.WEB_API);
                }
                return _pfxFileName;
            }
            set
            {
                _pfxFileName = value;
                CoreConfig.SetValue(CoreConfigClass.Key.pfxFileName, _pfxFileName, CoreConfigClass.Group.WEB_API);
            }
        }

        public static string pfxPasswordFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_pfxPasswordFileName))
                {
                    _pfxPasswordFileName = CoreConfig.GetValue(CoreConfigClass.Key.pfxPasswordFileName, "pfxPasswordFileName", CoreConfigClass.Group.WEB_API);
                }
                return _pfxPasswordFileName;
            }
            set
            {
                _pfxPasswordFileName = value;
                CoreConfig.SetValue(CoreConfigClass.Key.pfxPasswordFileName, _pfxPasswordFileName, CoreConfigClass.Group.WEB_API);
            }
        }

        public static string WebUserName
        {
            get
            {
                if (string.IsNullOrEmpty(_WebUserName))
                {
                    _WebUserName = CoreConfig.GetValue(CoreConfigClass.Key.WebUserName, "ami", CoreConfigClass.Group.WEB_API);
                }
                return _WebUserName;
            }
            set
            {
                _WebUserName = value;
                CoreConfig.SetValue(CoreConfigClass.Key.WebUserName, _WebUserName, CoreConfigClass.Group.WEB_API);
            }
        }

        public static string WebPassword
        {
            get
            {
                if (string.IsNullOrEmpty(_WebPassword))
                {
                    _WebPassword = CoreConfig.GetValue(CoreConfigClass.Key.WebPassword, "ddtv", CoreConfigClass.Group.WEB_API);
                }
                return _WebPassword;
            }
            set
            {
                _WebPassword = value;
                CoreConfig.SetValue(CoreConfigClass.Key.WebPassword, _WebPassword, CoreConfigClass.Group.WEB_API);
            }
        }

        public static string Cookis
        {
            get
            {
                if (string.IsNullOrEmpty(_Cookis))
                {
                    _Cookis = Guid.NewGuid().ToString();
                }
                return _Cookis;
            }
            set
            {
                _Cookis = value;
            }
        }
        public static string AccessKeyId
        {
            get
            {
                if (string.IsNullOrEmpty(_AccessKeyId))
                {
                    _AccessKeyId = CoreConfig.GetValue(CoreConfigClass.Key.AccessKeyId, Guid.NewGuid().ToString("N"), CoreConfigClass.Group.WEB_API);
                }
                return _AccessKeyId;
            }
            set
            {
                _AccessKeyId = value;
                CoreConfig.SetValue(CoreConfigClass.Key.AccessKeyId, _AccessKeySecret, CoreConfigClass.Group.WEB_API);
            }
        }
        public static string AccessKeySecret
        {
            get
            {
                if (string.IsNullOrEmpty(_AccessKeySecret))
                {
                    _AccessKeySecret = CoreConfig.GetValue(CoreConfigClass.Key.AccessKeySecret, Guid.NewGuid().ToString("N"), CoreConfigClass.Group.WEB_API);
                }
                return _AccessKeySecret;
            }
            set
            {
                _AccessKeySecret = value;
                CoreConfig.SetValue(CoreConfigClass.Key.AccessKeySecret, _AccessKeySecret, CoreConfigClass.Group.WEB_API);
            }
        }
        public static string ServerAID
        {
            get
            {
                if (string.IsNullOrEmpty(_ServerAID))
                {
                    _ServerAID = CoreConfig.GetValue(CoreConfigClass.Key.ServerAID, Guid.NewGuid().ToString(), CoreConfigClass.Group.WEB_API);
                }
                return _ServerAID;
            }
            set
            {
                _ServerAID = value;
                CoreConfig.SetValue(CoreConfigClass.Key.ServerAID, ServerAID, CoreConfigClass.Group.WEB_API);
            }
        }
        public static string ServerName
        {
            get
            {
                if (string.IsNullOrEmpty(_ServerName))
                {
                    _ServerName = CoreConfig.GetValue(CoreConfigClass.Key.ServerName, "DDTV_Server", CoreConfigClass.Group.WEB_API);
                }
                return _ServerName;
            }
            set
            {
                _ServerName = value;
                CoreConfig.SetValue(CoreConfigClass.Key.ServerName, _ServerName, CoreConfigClass.Group.WEB_API);
            }
        }
    }
}
