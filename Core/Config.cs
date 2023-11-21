using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Config
    {
        private static string key = "34D3D99D3489446191AB9B8582454669";
        /// <summary>
        /// 默认的AES加密秘钥
        /// </summary>
        public static string Key { get { return key; } }
        private static string iv = "B3FF40627013F53F";
        /// <summary>
        /// 默认的AES加密初始化向量
        /// </summary>
        public static string IV { get { return iv; } }
        private static string userInfoCoinfFileExtension = ".Duser";
        /// <summary>
        /// 用户配置文件拓展名
        /// </summary>
        public static string UserInfoCoinfFileExtension { get { return userInfoCoinfFileExtension; } }
        private static string configDirectory = "./Config/";
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigDirectory { get { return configDirectory; } }
        private static string logFileDirectory = "./Logs/";
        /// <summary>
        /// 日志文件路径
        /// </summary>
        public static string LogFileDirectory { get { return logFileDirectory; } }
    }
}
