using Core.LogModule;
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
        public static string CompiledVersion = "CompilationTime";
#if DEBUG
        public static bool IsDevDebug = true;
#else
        public static bool IsDevDebug = false;
#endif
        public static void Start()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            System.AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);       
            InitDirectoryAndFile();
            Config.ReadConfiguration();
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 4096 * 16;
            ServicePointManager.Expect100Continue = false;
            Thread.Sleep(2000);
            LogModule.Log.LogInit();
            Log.Info(nameof(Init),$"初始化工作路径为:{Environment.CurrentDirectory}");
            Log.Info(nameof(Init),$"检查和创建必要的目录");
            Log.Info(nameof(Init),$"初始化ServicePointManager对象");
            Config.WriteConfiguration();
            var _ = Core.RuntimeObject.Account.AccountInformation;
            Core.RuntimeObject.Account.CheckLoginStatus();
            Log.Info(nameof(Init),$"Core初始化完成");
        }
        /// <summary>
        /// 初始化文件和目录
        /// </summary>
        private static void InitDirectoryAndFile()
        {
            if (!Directory.Exists(Config.Core._ConfigDirectory))
            {
                Directory.CreateDirectory(Config.Core._ConfigDirectory);
            }
            if (!Directory.Exists(Config.Core._LogFileDirectory))
            {
                Directory.CreateDirectory(Config.Core._LogFileDirectory);
            }
            if (!Directory.Exists(Config.Core._TemporaryFileDirectory))
            {
                Directory.CreateDirectory(Config.Core._TemporaryFileDirectory);
            }
            if (!Directory.Exists(Config.Core._RecFileDirectory))
            {
                Directory.CreateDirectory(Config.Core._RecFileDirectory);
            }
        }
    }
}
