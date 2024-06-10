using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
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
        public static bool IsDev = true;
        private static Timer Update_Timer;

        public static void Start(string[] args)
        {
            if (File.Exists("./ver.ini"))
            {
                string[] Ver = File.ReadAllLines("./ver.ini");
                foreach (string VerItem in Ver)
                {
                    if (VerItem.StartsWith("ver=") && VerItem.Split('=')[1].TrimEnd().ToLower().StartsWith("release"))
                    {
                        IsDev = false;
                    }           
                }
            }
            CoreStartCompletEvent += (sender, e) =>
            {
                //注册Core启动完成触发事件
                CoreStartAwait.SetResult(true);
            };

            //启动参数初始化
            StartParameterInitialization(args);

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
            //初始化文件和目录
            InitDirectoryAndFile();
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 4096 * 16;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //给文件系统一点时间创建各种路径
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
            Update_Timer = new Timer(Core.Tools.ProgramUpdates.RegularInspection, null, 1, 1000 * 60 * 30);
            Core.Tools.ProgramUpdates.NewVersionAvailableEvent += ProgramUpdates_NewVersionAvailableEvent;
        }

    

        /// <summary>
        /// 启动参数初始化
        /// </summary>
        private static void StartParameterInitialization(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    string optionName;
                    string optionValue = string.Empty;

                    if (arg.Contains('='))
                    {
                        optionName = arg.Substring(2, arg.IndexOf('=') - 2); // 获取选项名称
                        optionValue = arg.Substring(arg.IndexOf('=') + 1); // 获取选项值
                    }
                    else
                    {
                        optionName = arg.Substring(2); // 获取选项名称
                    }

                    if (OptionHandlers.ContainsKey(optionName))
                    {
                        OptionHandlers[optionName](optionValue);
                    }
                    else
                    {
                        Console.WriteLine($"未知的option: {optionName}");
                    }
                }
            }
        }



        /// <summary>
        /// 获取Core初始化完成后的运行毫秒数
        /// </summary>
        /// <returns></returns>
        public static double GetRunTime()
        {
            if (stopwatch.IsRunning)
            {
                TimeSpan elapsed = stopwatch.Elapsed;
                return elapsed.TotalMicroseconds/1000;
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
            if (!Directory.Exists(Config.Core_RunConfig._ConfigDirectory))
            {
                Directory.CreateDirectory(Config.Core_RunConfig._ConfigDirectory);
            }
            if (!Directory.Exists(Config.Core_RunConfig._LogFileDirectory))
            {
                Directory.CreateDirectory(Config.Core_RunConfig._LogFileDirectory);
            }
            if (!Directory.Exists(Config.Core_RunConfig._TemporaryFileDirectory))
            {
                Directory.CreateDirectory(Config.Core_RunConfig._TemporaryFileDirectory);
            }
            if (!Directory.Exists(Config.Core_RunConfig._RecFileDirectory))
            {
                Directory.CreateDirectory(Config.Core_RunConfig._RecFileDirectory);
            }

            DeleteUnexpectedFiles();


        }

        /// <summary>
        /// 清理以外存在的文件
        /// </summary>
        private static void DeleteUnexpectedFiles()
        {
            if(File.Exists($"{Core_RunConfig._ConfigDirectory}{Core_RunConfig._UserInfoCoinfFileExtension}"))
            {
                Tools.FileOperations.Delete($"{Core_RunConfig._ConfigDirectory}{Core_RunConfig._UserInfoCoinfFileExtension}","发现空白登录态文件可能导致错误，已清理");
            }
        }

        private static void ProgramUpdates_NewVersionAvailableEvent(object? sender, EventArgs e)
        {
            switch(Mode)
            {
                case Mode.Client:
                    break;
                case Mode.Desktop:
                    break;
                default:
                    Log.Warn("ProgramUpdates",$"\r\n=====检测到新版本=====\r\n检测到DDTV新版本：【{sender}】，请关闭DDTV后，执行Update文件夹中的Update程序进行升级\r\n=====检测到新版本=====\r\n");
                    break;
            }
        }
    }
}
