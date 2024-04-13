using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Core.Config;

namespace Core
{
    public class Init
    {
        
        //Core服务启动完成事件
        public static event EventHandler<EventArgs> CoreStartCompletEvent;
        public static TaskCompletionSource<bool> CoreStartAwait = new TaskCompletionSource<bool>();
        public static string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string InitType = "DDTV";
        public static string ClientAID = string.Empty;
        public static string CompiledVersion = "CompilationTime";
        public static Mode Mode = Mode.Core;

#if DEBUG
        public static bool IsDevDebug = true;
#else
        public static bool IsDevDebug = false;
#endif
        public static void Start(string[] args)
        {
            CoreStartCompletEvent += (sender, e) =>
            {
                //注册Core启动完成触发事件
                CoreStartAwait.SetResult(true);
            };

            ///设置mode
            foreach (string arg in args)
            {
                if (modeMap.TryGetValue(arg, out Mode value))
                {
                    Mode = value;
                    break;
                }
            }
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
            InitDirectoryAndFile();
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 4096 * 16;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //给文件系统一点时间创建各种路径，按道理应该是同步操作，不知道为啥有些环境执行到这里还没有来得及创建路径
            Thread.Sleep(50);
            Log.LogInit();
            Log.Info(nameof(Init), $"初始化工作路径为:{Environment.CurrentDirectory}");
            Log.Info(nameof(Init), $"检查和创建必要的目录");
            Log.Info(nameof(Init), $"初始化ServicePointManager对象");
            Config.WriteConfiguration();
            var _ = Core.RuntimeObject.Account.AccountInformation;
            Core.RuntimeObject.Account.CheckLoginStatus();
            Log.Info(nameof(Init), $"Core初始化完成");
            Task.Run(() => CoreStartCompletEvent?.Invoke(null, new EventArgs()));
            stopwatch.Start();   
        }



        /// <summary>
        /// 获取Core初始化完成后的运行毫秒数
        /// </summary>
        /// <returns></returns>
        public static double GetRunTime()
        {
            if(stopwatch.IsRunning)
            {
                TimeSpan elapsed = stopwatch.Elapsed;
                return elapsed.TotalMicroseconds;
            }
            else
            {
                return 0;
            }
        }

        private static Stopwatch stopwatch = new Stopwatch();

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
