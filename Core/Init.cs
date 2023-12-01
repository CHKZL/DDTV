using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Init
    {
        public static string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string InitType = "DDTV";
        public static string ClientAID = string.Empty;
        public static string CompiledVersion = "2023-11-03 21:01:27";
        public static bool IsDevDebug = false;
        public static void Start(string InitType = "DDTV", string ClientAID = "", bool IsDev = false)
        {
            InitDirectoryAndFile();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 4096;
            ServicePointManager.Expect100Continue = false;
            LogModule.Log.LogInit();

            Config.ReadConfiguration();
            Config.WriteConfiguration();
        }
        /// <summary>
        /// 初始化文件和目录
        /// </summary>
        private static void InitDirectoryAndFile()
        {
            if(!Directory.Exists(Config.Core._ConfigDirectory))
            {
                Directory.CreateDirectory(Config.Core._ConfigDirectory);
            }
            if(!Directory.Exists(Config.Core._LogFileDirectory))
            {
                Directory.CreateDirectory(Config.Core._LogFileDirectory);
            }
              if(!Directory.Exists(Config.Core._TemporaryFileDirectory))
            {
                Directory.CreateDirectory(Config.Core._TemporaryFileDirectory);
            }
        }
    }
}
