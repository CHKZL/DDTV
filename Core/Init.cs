using Core.LogModule;
using Core.RuntimeObject;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Core.Config;
using static Core.Tools.ProgramUpdates;
using static System.Net.Mime.MediaTypeNames;

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
        private static Timer Core_Update_Timer;
        private static Timer FFMPEG_Update_Timer;

        private static Stopwatch stopwatch = new Stopwatch();

        public static void Start(string[] args)
        {
            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("当前为非Windows环境，请确保已安装ffmpeg，否则自动修复和封装转码会失败！");
                Console.WriteLine("当前为非Windows环境，请确保已安装ffmpeg，否则自动修复和封装转码会失败！");
                Console.WriteLine("当前为非Windows环境，请确保已安装ffmpeg，否则自动修复和封装转码会失败！");
                Console.WriteLine("如确认已安装，可忽略该消息。");
            }
            stopwatch.Start();
            CoreStartCompletEvent += (sender, e) =>
            {
                //注册Core启动完成触发事件
                CoreStartAwait.SetResult(true);
            };
            //启动参数初始化
            StartParameterInitialization(args);
            //将当前路径从 引用路径 修改至 程序所在目录
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //设置当前运行模式
            AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
            //初始化文件和目录
            InitDirectoryAndFile();
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.DefaultConnectionLimit = 4096 * 16;
            System.Net.ServicePointManager.MaxServicePoints = 1024;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //给文件系统一点时间创建各种路径
            Thread.Sleep(100);
            //初始化日志系统
            Log.LogInit();
            //初始化邮件事件系统
            SMTP.Init();
            Log.Info(nameof(Init), $"初始化工作路径为:{Environment.CurrentDirectory}");
            Log.Info(nameof(Init), $"检查和创建必要的目录");
            Log.Info(nameof(Init), $"初始化ServicePointManager对象");
            //触发一次写配置文件以生成可能不存在的配置文件
            Config.WriteConfiguration();
            //初始化一次登录态信息，防止后面空对象
            var _ = Core.RuntimeObject.Account.AccountInformation;
            //如果没有大聪明乱输UID，就把Uid设置为配置文件中的uid信息
            long.TryParse(_.Uid, out Core.Network.Methods.User.uid);
            //启动登录态检查进程
            Core.RuntimeObject.Account.CheckLoginStatus();

            Log.Info(nameof(Init), $"Core初始化完成");
            Log.Info(nameof(Init), $"启动耗时{stopwatch.ElapsedMilliseconds}毫秒");

            CheckStatusFFMPEGFile();
            Task.Run(() => CoreStartCompletEvent?.Invoke(null, new EventArgs()));
            if (Mode != Mode.Core)
            {
                Core_Update_Timer = new Timer(Core.Tools.ProgramUpdates.RegularInspection, null, 1, 1000 * 60 * 30);
                Core.Tools.ProgramUpdates.NewVersionAvailableEvent += ProgramUpdates_NewVersionAvailableEvent;
                FFMPEG_Update_Timer = new Timer(Core.Tools.ProgramUpdates.Update_FFMPEG.CheckUpdateFFmpegAsync, null, 1, 1000 * 60 * 60 * 24);
            }
            StartStatistics();
            Timer_Heartbeat = new Timer(HeartbeatStatistics, null, 1, 1000 * 3600);
        }

        /// <summary>
        /// 启动参数初始化
        /// </summary>
        public static void StartParameterInitialization(string[] args)
        {
            string _conf = args.Where(x => x.Contains("--conf")).FirstOrDefault();
            if (!string.IsNullOrEmpty(_conf))
            {
                Core.Config.RunConfig.ConfigurationFile = _conf.Split('=')?[1];
            }
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
                    try
                    {
                        if (OptionHandlers.ContainsKey(optionName))
                        {
                            OptionHandlers[optionName](optionValue);
                        }
                        else
                        {
                            Console.WriteLine($"未知的option: {optionName}");
                        }
                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                }
            }
            string? DockerEnvironment = Environment.GetEnvironmentVariable("DDTV_Docker_Project");
            if (!string.IsNullOrEmpty(DockerEnvironment))
            {
                if (DockerEnvironment == "DDTV_Server")
                {
                    Init.Mode = Mode.Docker;
                }
            }
        }


        /// <summary>
        /// 获取Core初始化完成后的运行秒数
        /// </summary>
        /// <returns></returns>
        public static double GetRunTime()
        {
            if (stopwatch.IsRunning)
            {
                TimeSpan elapsed = stopwatch.Elapsed;
                return elapsed.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }



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
            if (!Directory.Exists(Config.Core_RunConfig._DebugFileDirectory))
            {
                Directory.CreateDirectory(Config.Core_RunConfig._DebugFileDirectory);
            }
            DeleteUnexpectedFiles();


        }

        /// <summary>
        /// 清理意外存在的文件
        /// </summary>
        private static void DeleteUnexpectedFiles()
        {
            if (File.Exists($"{Core_RunConfig._ConfigDirectory}{Core_RunConfig._UserInfoCoinfFileExtension}"))
            {
                Tools.FileOperations.Delete($"{Core_RunConfig._ConfigDirectory}{Core_RunConfig._UserInfoCoinfFileExtension}", "发现空白登录态文件可能导致错误，已清理");
            }
            Tools.FileOperations.DeleteEmptyDirectories(Core.Config.Core_RunConfig._TemporaryFileDirectory);
        }

        private static void ProgramUpdates_NewVersionAvailableEvent(object? sender, EventArgs e)
        {
            switch (Mode)
            {
                case Mode.Client:
                    break;
                case Mode.Desktop:
                    break;
                default:
                    Log.Warn("ProgramUpdates", $"\r\n=====检测到新版本=====\r\n检测到DDTV新版本：【{sender}】，请关闭DDTV后，执行Update文件夹中的Update程序进行升级\r\n=====检测到新版本=====\r\n");
                    break;
            }
        }

        public static Timer Timer_Heartbeat;

        private static void StartStatistics()
        {
            try
            {
                using (HttpClient _httpClient = new HttpClient())
                {
                    _httpClient.Timeout = new TimeSpan(0, 0, 8);
                    _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"System/{System.Runtime.InteropServices.RuntimeInformation.OSDescription} DDTV/{Ver} Bili/{RuntimeObject.Account.AccountInformation.Uid}");
                    string A = _httpClient.GetStringAsync("https://update5.ddtv.pro/Start.txt").Result;
                    if (A == "1" || A == "1\r\n")
                    {
                        Log.Info(nameof(StartStatistics), "启动统计正常");
                    }
                    else
                    {
                        Log.Info(nameof(StartStatistics), "启动统计异常");
                    }
                }
            }
            catch (Exception) { }
        }

        public static void HeartbeatStatistics(object state)
        {
            try
            {
                using (HttpClient _httpClient = new HttpClient())
                {
                    _httpClient.Timeout = new TimeSpan(0, 0, 8);
                    _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"System/{System.Runtime.InteropServices.RuntimeInformation.OSDescription} DDTV/{Ver} Bili/{RuntimeObject.Account.AccountInformation.Uid}");
                    string A = _httpClient.GetStringAsync("https://update5.ddtv.pro/Heartbeat.txt").Result;
                    if (A == "1" || A == "1\r\n")
                    {
                        Log.Info(nameof(HeartbeatStatistics), "心跳正常");
                    }
                    else
                    {
                        Log.Warn(nameof(HeartbeatStatistics), "心跳异常");
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 检查FFMPEG文件状态
        /// </summary>
        public static void CheckStatusFFMPEGFile()
        {
            if (OperatingSystem.IsWindows() && Directory.Exists("./plugins/ffmpeg") && File.Exists("./plugins/ffmpeg/new_ffmpeg.exe"))
            {    
                Log.Info(nameof(CheckStatusFFMPEGFile),$"检测到新FFMPEG文件【./plugins/ffmpeg/new_ffmpeg.exe】，开始替换新文件");
                File.Copy("./plugins/ffmpeg/new_ffmpeg.exe", "./plugins/ffmpeg/ffmpeg.exe", true);
                Tools.FileOperations.Delete("./plugins/ffmpeg/new_ffmpeg.exe");
                Log.Info(nameof(CheckStatusFFMPEGFile),$"自动替换FFMPEG生效");
            }
        }

    }
}
