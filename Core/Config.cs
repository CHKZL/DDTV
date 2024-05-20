using Core.LogModule;
using Core.RuntimeObject;
using Core.Tools;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using static Core.RuntimeObject.Download.Basics;

namespace Core
{
    public class Config
    {
        #region Private Properties

        private static Dictionary<string, FieldInfo> varMap = new Dictionary<string, FieldInfo>();
        /// <summary>
        /// 构造函数，将这个类下的private参数给生成字典用于配置文件的读写
        /// </summary>
        static Config()
        {

            var _Config = new List<FieldInfo>();
            _Config.AddRange(typeof(Config.Core).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(field => !field.IsAssembly));
            _Config.AddRange(typeof(Config.Download).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(field => !field.IsAssembly));
            _Config.AddRange(typeof(Config.Web).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(field => !field.IsAssembly));

            lock (varMap)
                foreach (var fieldInfo in _Config)
                    varMap.Add(fieldInfo.Name, fieldInfo);
            Thread.Sleep(500);
            ReadConfiguration();
            RoomConfig.LoadRoomConfigurationFile();

            bool isFirstRun = true;

            if (isFirstRun)
            {
                isFirstRun = false;
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            RoomConfig.SaveRoomConfigurationFile();
                        }
                        catch (Exception e)
                        {
                            Log.Error(nameof(RoomConfig.SaveRoomConfigurationFile), $"将房间配置写入配置文件时出错", e, false);
                        }
                        Thread.Sleep(1000 * 5);
                    }
                });
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            WriteConfiguration();
                        }
                        catch (Exception e)
                        {
                            Log.Error(nameof(WriteConfiguration), $"将本地配置写入配置文件时出错", e, false);
                        }
                        Thread.Sleep(1000 * 5);
                    }
                });
            }
        }
        #endregion

        #region public Method
        /// <summary>
        /// 启动参数初始化
        /// </summary>
        internal static Dictionary<string, Action<string>> OptionHandlers = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "StartMode", ExpandOption.SetStartMode },
            { "RecordingMode", ExpandOption.SetRecordingMode },
        };

        public enum Mode
        {
            Core,
            Server,
            Docker,
            Client,
            Desktop
        }

        internal class ExpandOption
        {
            /// <summary>
            /// 设置核心的启动模式
            /// </summary>
            /// <param name="value"></param>
            internal static void SetStartMode(string value)
            {
                switch (value)
                {
                    case "Core":
                        Init.Mode = Mode.Core;
                        break;
                    case "Server":
                        Init.Mode = Mode.Server;
                        break;
                    case "Docker":
                        Init.Mode = Mode.Docker;
                        break;
                    case "Client":
                        Init.Mode = Mode.Client;
                        break;
                    case "Desktop":
                        Init.Mode = Mode.Desktop;
                        break;
                }
            }
            /// <summary>
            /// 设置录制模式
            /// </summary>
            /// <param name="value"></param>
            internal static void SetRecordingMode(string value)
            {
                switch (value.ToLower())
                {
                    case "auto":
                        Download._RecordingMode = RecordingMode.Auto;
                        break;
                    case "flv_only":
                        Download._RecordingMode = RecordingMode.FLV_Only;
                        break;
                    case "hls_only":
                        Download._RecordingMode = RecordingMode.HLS_Only;
                        break;
                }
            }
        }

        private static object _ConfigurationLock = new();

        /// <summary>
        /// 从持久化配置文件读取配置刷新本地配置
        /// </summary>
        public static void ReadConfiguration()
        {
            lock (_ConfigurationLock)
            {
                string[] A = File.Exists(Core._ConfigurationFile) ? File.ReadAllLines(Core._ConfigurationFile) : [];
                lock (varMap)
                {
                    foreach (var item in A)
                    {
                        if (item.Split('=').Length == 2)
                        {
                            try
                            {
                                varMap[item.Split('=')[0]].SetValue(null, item.Split('=')[1]);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                string msg = $"读取配置文件完成";
                OperationQueue.Add(Opcode.Config.ReadingConfigurationFile, msg);
                Log.Info(nameof(ReadConfiguration), msg);
            }

        }
        /// <summary>
        /// 把当前配置写入到持久化配置文件
        /// </summary>
        public static void WriteConfiguration()
        {
            lock (_ConfigurationLock)
            {
                StringBuilder newConfig = new StringBuilder();
                lock (varMap)
                {
                    foreach (var item in varMap)
                    {
                        newConfig.AppendLine($"{item.Key}={item.Value.GetValue(null)}");
                    }
                }
                string existingConfig = File.Exists(Core._ConfigurationFile) ? File.ReadAllText(Core._ConfigurationFile) : string.Empty;
                if (existingConfig != newConfig.ToString())
                {
                    using (StreamWriter file = new StreamWriter(Core._ConfigurationFile))
                    {
                        file.Write(newConfig.ToString());
                    }
                    string msg = $"配置信息发生变化，写入文件";
                    OperationQueue.Add(Opcode.Config.UpdateToConfigurationFile, msg);
                    Log.Info(nameof(ReadConfiguration), $"配置信息发生变化，写入文件");
                }
            }
        }

        #endregion

        #region public Properties Method

        public class RoomConfig
        {
            private static object _RoomConfigurationLock = new();


            /// <summary>
            /// 读取房间配置文件更新运行时参数
            /// </summary>
            public static void LoadRoomConfigurationFile()
            {
                lock (_RoomConfigurationLock)
                {
                    (int Total, int Success, int Fail) Count = new(0, 0, 0);
                    if (!Directory.Exists(Config.Core._ConfigDirectory))
                    {
                        Directory.CreateDirectory(Config.Core._ConfigDirectory);
                    }
                    if (!File.Exists($"{Core._RoomConfigFile}"))
                    {
                        File.WriteAllText($"{Core._RoomConfigFile}", "{}");
                    }
                    else
                    {
                        string TEXT = File.ReadAllText($@"{Core._RoomConfigFile}");
                        RoomListDiscard roomListDiscard = JsonSerializer.Deserialize<RoomListDiscard>(TEXT);
                        if (roomListDiscard != null)
                        {
                            foreach (var item in roomListDiscard.data)
                            {
                                Count.Total++;
                                RoomCardClass? roomCard = new();
                                if (_Room.GetCardForUID(item.UID, ref roomCard))
                                {
                                    Count.Success++;
                                    roomCard.UID = item.UID;
                                    roomCard.description = item.description;
                                    roomCard.RoomId = item.RoomId;
                                    roomCard.Name = item.Name;
                                    roomCard.IsAutoRec = item.IsAutoRec;
                                    roomCard.IsRemind = item.IsRemind;
                                    roomCard.IsRecDanmu = item.IsRecDanmu;
                                    roomCard.Like = item.Like;
                                    roomCard.Shell = item.Shell;
                                    roomCard.AppointmentRecord = item.AppointmentRecord;
                                }
                                else
                                {
                                    Count.Success++;
                                    _Room.SetRoomCardByUid(item.UID, item);
                                }
                            }
                        }
                        else
                        {
                            File.WriteAllText($"{Core._RoomConfigFile}", "{}");
                        }
                    }
                    string msg = $"加载房间列表，一共{Count.Total}个/成功{Count.Success}个/失败{Count.Fail}个";
                    OperationQueue.Add(Opcode.Config.ReadingRoomFiles, msg);
                    Log.Info(nameof(LoadRoomConfigurationFile), msg);
                }
            }


            /// <summary>
            /// 将房间配置写入配置文件
            /// </summary>
            public static void SaveRoomConfigurationFile()
            {
                lock (_RoomConfigurationLock)
                {
                    RoomListDiscard roomListDiscard = new RoomListDiscard();
                    var roomInfos = _Room.GetCardListClone();
                    var sorted = roomInfos.OrderBy(item => item.Key);
                    foreach (var item in sorted)
                    {
                        roomListDiscard.data.Add(item.Value);
                    }
                    // 支持基本拉丁语和中文字符
                    string jsonString = JsonSerializer.Serialize(roomListDiscard, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) });

                    string filePath = $"{Core._RoomConfigFile}";
                    if (File.Exists(filePath))
                    {
                        string existingContent = File.ReadAllText(filePath, Encoding.UTF8);
                        string oldmd5 = Encryption.Md532(existingContent);
                        string newmd5 = Encryption.Md532(jsonString);
                        if (oldmd5 == newmd5)
                        {
                            // 如果文件中的内容与即将写入的内容一致，则跳过写入
                            return;
                        }
                    }

                    // 如果文件不存在，或者文件中的内容与即将写入的内容不一致，则进行写入
                    File.WriteAllText(filePath, jsonString, Encoding.UTF8);

                    string msg = $"房间配置发生变化，写入文件";
                    OperationQueue.Add(Opcode.Config.UpdateToRoomFile, msg);
                    Log.Info(nameof(SaveRoomConfigurationFile), msg);
                }
            }




            internal class RoomListDiscard
            {
                [JsonPropertyName("data")]
                public List<RuntimeObject.RoomCardClass> data { set; get; } = [];
            }

        }


        public class Core
        {
            private static string ValidAccount = "-1";
            /// <summary>
            /// 生效的账号配置文件
            /// 默认值：./Temporary/LoginQr.png
            /// </summary>
            public static string _ValidAccount
            {
                get => $"{ValidAccount}";
                set
                {
                    if (value != ValidAccount)
                    {
                        ValidAccount = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string DefaultFilePathNameFormat = "{ROOMID}_{NAME}/{TITLE}_{DATE}_{TIME}";
            /// <summary>
            /// 保存的文件以怎样的路径和名称格式保存在录制文件夹中
            /// 默认值：{ROOMID}_{NAME}/{TITLE}_{DATE}_{TIME}    (默认值例：2233_哔哩哔哩弹幕网/标题名称_2024_01_27_19_46_58_122)；文件名会固定以[_original.mp4]或[_fix.mp4]结尾，具体是哪个取决于[AutomaticRepair]配置状态
            /// 支持的配置项：房间号{ROOMID}、昵称{NAME}、日期(年_月_日){DATE}、时间(时_分_秒){TIME}、标题{TITLE})、年(2012和12){yyyy和yy}、时{HH}、分{mm}、秒{ss}、毫秒{fff}
            /// </summary>
            public static string _DefaultFilePathNameFormat
            {
                get => DefaultFilePathNameFormat;
                set
                {
                    if (value != DefaultFilePathNameFormat)
                    {
                        DefaultFilePathNameFormat = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string AutomaticRepair = "true";
            /// <summary>
            /// 录制完成自动修复和转码设置
            /// 默认值：true
            /// </summary>
            public static bool _AutomaticRepair
            {
                get => bool.Parse(AutomaticRepair);
                set
                {
                    if (value.ToString() != AutomaticRepair)
                    {
                        AutomaticRepair = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string ForceMerge = "false";
            /// <summary>
            /// 录制完成强制合并为一个视频文件
            /// 默认值：true
            /// </summary>
            public static bool _ForceMerge
            {
                get => bool.Parse(ForceMerge);
                set
                {
                    if (value.ToString() != ForceMerge)
                    {
                        ForceMerge = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }




            private static string UseAgree = "false";
            /// <summary>
            /// 用户协议同意状态
            /// 默认值：false
            /// </summary>
            public static bool _UseAgree
            {
                get => bool.Parse(UseAgree);
                set
                {
                    if (value.ToString() != UseAgree)
                    {
                        UseAgree = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            internal static string QrUrl = "QrUrlPipeline";
            /// <summary>
            /// 登陆用扫码二维码路径和文件名
            /// 默认值：./Temporary/QrUrlPipeline
            /// </summary>
            public static string _QrUrl
            {
                get => $"{_TemporaryFileDirectory}{QrUrl}";
            }

            internal static string QrFileNmae = "LoginQr.png";
            /// <summary>
            /// 登陆用扫码二维码路径和文件名
            /// 默认值：./Temporary/LoginQr.png
            /// </summary>
            public static string _QrFileNmae
            {
                get => $"{_TemporaryFileDirectory}{QrFileNmae}";

            }

            //private static string LoginStatus = "false";
            ///// <summary>
            ///// 配置文件登陆状态缓存
            ///// 默认值：false
            ///// </summary>
            //public static bool _LoginStatus
            //{
            //    get => bool.Parse(LoginStatus);
            //    set
            //    {
            //        if (value.ToString() != LoginStatus)
            //        {
            //            LoginStatus = value.ToString();
            //            string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
            //            OperationQueue.Add(Opcode.Code.ModifyConfiguration, msg);
            //            Log.Info(nameof(Config), msg);
            //        }
            //    }
            //}

            internal static string RoomConfigFile = "RoomListConfig.json";
            /// <summary>
            /// 房间配置文件路径（字符串）
            /// 默认值：./Config/RoomListConfig.json
            /// </summary>
            public static string _RoomConfigFile
            {
                get => $"{_ConfigDirectory}{RoomConfigFile}";
            }



            internal static string ConfigDirectory = "./Config/";
            /// <summary>
            /// 配置文件路径（字符串）
            /// 默认值：./Config/
            /// </summary>
            public static string _ConfigDirectory
            {
                get => ConfigDirectory;
            }

            internal static string ConfigurationFile = $"DDTV_Config.ini";
            /// <summary>
            /// 默认的配置文件路径（字符串）
            /// 默认值：./Config/DDTV_Config.ini
            /// </summary>
            public static string _ConfigurationFile
            {
                get => $"{_ConfigDirectory}{ConfigurationFile}";
            }

            private static string Key = "34D3D9‭9D34894461‭91AB9B8‭582454669";
            /// <summary>
            /// 默认的AES加密秘钥（字符串）
            /// 默认值：34D3D9‭9D34894461‭91AB9B8‭582454669
            /// </summary>
            public static string _Key
            {
                get => Key;
            }

            private static string IV = "B3FF‭40627013‭F53F";
            /// <summary>
            /// 默认的AES加密初始化向量（字符串）
            /// 默认值：B3FF‭40627013‭F53F
            /// </summary>
            public static string _IV
            {
                get => IV;
            }

            internal static string UserInfoCoinfFileExtension = ".Duser";
            /// <summary>
            /// 用户配置文件拓展名（字符串）
            /// 默认值：.Duser
            /// </summary>
            public static string _UserInfoCoinfFileExtension
            {
                get => UserInfoCoinfFileExtension;
            }

            internal static string LogFileDirectory = "./Logs/";
            /// <summary>
            /// 日志文件路径（字符串）
            /// 默认值：./Logs/
            /// </summary>
            public static string _LogFileDirectory
            {
                get => LogFileDirectory;
            }

            private static string RecFileDirectory = "./Rec/";
            /// <summary>
            /// 录制文件储存路径（字符串）
            /// 默认值：./Rec/
            /// </summary>
            public static string _RecFileDirectory
            {
                get => RecFileDirectory;
                set
                {
                    if (value != RecFileDirectory)
                    {
                        RecFileDirectory = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            internal static string TemporaryFileDirectory = "./Temporary/";
            /// <summary>
            /// 临时文件路径（字符串）
            /// 默认值：./Temporary/
            /// </summary>
            public static string _TemporaryFileDirectory
            {
                get => TemporaryFileDirectory;
            }

            private static string LiveDomainName = "https://api.live.bilibili.com";
            /// <summary>
            /// 默认使用的直播API域名（字符串）
            /// 默认值：https://api.live.bilibili.com
            /// </summary>
            public static string _LiveDomainName
            {
                get => LiveDomainName;
                set
                {
                    if (value != LiveDomainName)
                    {
                        LiveDomainName = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string MainDomainName = "https://api.bilibili.com";
            /// <summary>
            /// 默认使用的主站API域名（字符串）
            /// 默认值：https://api.bilibili.com
            /// </summary>
            public static string _MainDomainName
            {
                get => MainDomainName;
                set
                {
                    if (value != MainDomainName)
                    {
                        MainDomainName = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string HTTP_UA = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0";
            /// <summary>
            /// 请求是默认使用的UA（字符串）
            /// 默认值：$"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0"
            /// </summary>
            public static string _HTTP_UA { get { return HTTP_UA; } }

            private static string HlsWaitingTime = "50";
            /// <summary>
            /// 一个新任务等待HLS的时间（int，单位秒）
            /// 默认值：50
            /// </summary>
            public static int _HlsWaitingTime
            {
                get => int.Parse(HlsWaitingTime);
                set
                {
                    if (value.ToString() != HlsWaitingTime)
                    {
                        HlsWaitingTime = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string DetectIntervalTime = "10000";
            /// <summary>
            /// 直播间状态更新间隔时间（int，单位毫秒）
            /// 默认值：10000
            /// </summary>
            public static int _DetectIntervalTime
            {
                get => int.Parse(DetectIntervalTime);
                set
                {
                    if (value.ToString() != DetectIntervalTime)
                    {
                        DetectIntervalTime = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string DebugMode = "false";
            /// <summary>
            /// 调试模式开关（bool）
            /// 默认值：false
            /// </summary>
            public static bool _DebugMode
            {
                get
                {
                    return bool.Parse(DebugMode);
                }
                set
                {
                    if (value.ToString() != DebugMode)
                    {
                        DebugMode = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }
        }
        public class Download
        {
            internal static string RecordingMode = "1";
            /// <summary>
            /// 录制模式，提供：Auto\FLV_Only\HLS_Only 三种模式取值分别是1/2/3,详细说明请查看Core.RuntimeObject.Download.Basics.RecordingMode
            /// 默认值：Auto
            /// </summary>
            public static RecordingMode _RecordingMode
            {
                get
                {
                    switch (RecordingMode)
                    {
                        case "1":
                            return RuntimeObject.Download.Basics.RecordingMode.Auto;
                        case "2":
                            return RuntimeObject.Download.Basics.RecordingMode.FLV_Only;
                        case "3":
                            return RuntimeObject.Download.Basics.RecordingMode.HLS_Only;
                        default:
                            return RuntimeObject.Download.Basics.RecordingMode.Auto;
                    }
                }
                set
                {
                    if ((int)value != int.Parse(RecordingMode))
                    {
                        RecordingMode = ((int)value).ToString();
                    }
                }
            }

            private static string DeleteOriginalFileAfterRepair = "true";
            /// <summary>
            /// 修复完成后删除源文件（bool）
            /// 默认值：false
            /// </summary>
            public static bool _DeleteOriginalFileAfterRepair
            {
                get
                {
                    return bool.Parse(DeleteOriginalFileAfterRepair);
                }
                set
                {
                    if (value.ToString() != DeleteOriginalFileAfterRepair)
                    {
                        DeleteOriginalFileAfterRepair = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string DefaultResolution = "10000";
            /// <summary>
            /// 默认分辨率 默认值：10000    可选值：流畅:80  高清:150  超清:250  蓝光:400  原画:10000
            /// 默认值：https://api.bilibili.com
            /// </summary>
            public static int _DefaultResolution
            {
                get => int.Parse(DefaultResolution);
                set
                {
                    if (value.ToString() != DefaultResolution)
                    {
                        DefaultResolution = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }
        }
        public class Web
        {
            private static string Port = "11419";
            /// <summary>
            /// Api提供的端口地址
            /// 默认值：11419
            /// </summary>
            public static int _Port
            {
                get => int.Parse(Port);
                set
                {
                    if (value.ToString() != Port)
                    {
                        Port = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string IP = "127.0.0.1";
            /// <summary>
            /// WEB服务监听的IP地址
            /// 默认值：127.0.0.1
            /// </summary>
            public static string _IP
            {
                get => IP;
                set
                {
                    if (value != IP)
                    {
                        IP = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            internal static string RecordingStorageDirectory = "/rec_file";
            /// <summary>
            /// Web返回录制文件的相对根路径（字符串）
            /// 默认值：/rec_file
            /// </summary>
            public static string _RecordingStorageDirectory
            {
                get => RecordingStorageDirectory;
            }

            internal static string WebUiDirectory = "./Static/";
            /// <summary>
            /// WebUi文件路径（字符串）
            /// 默认值：./Static/
            /// </summary>
            public static string _WebUiDirectory
            {
                get => WebUiDirectory;
            }

            private static string AccessControlAllowCredentials = "true";
            /// <summary>
            /// WEB的Credentials设置 (布尔值)
            /// 默认值：true
            /// </summary>
            public static string _AccessControlAllowCredentials
            {
                get => AccessControlAllowCredentials;
                set
                {
                    if (value != AccessControlAllowCredentials)
                    {
                        AccessControlAllowCredentials = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string AccessControlAllowOrigin = "*";
            /// <summary>
            /// WEB跨域设置路径 （字符串:为*或者完整URL）（完整URL应为前端网址，必须带协议和端口号，如：http://127.0.0.1:11419）
            /// 默认值：*
            /// </summary>
            public static string _AccessControlAllowOrigin
            {
                get => AccessControlAllowOrigin;
                set
                {
                    if (value != AccessControlAllowOrigin)
                    {
                        AccessControlAllowOrigin = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }



            private static string AccessKeyId = "ddtv";
            /// <summary>
            /// API鉴权所使用的AccessKeyId 为字符串，默认"ddtv"
            /// 默认值：ddtv
            /// </summary>
            public static string _AccessKeyId
            {
                get => AccessKeyId;
                set
                {
                    if (value != AccessKeyId)
                    {
                        AccessKeyId = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string AccessKeySecret = "ddtv";
            /// <summary>
            /// API鉴权所使用的AccessKeySecret 为字符串，默认"ddtv"
            /// 默认值：ddtv
            /// </summary>
            public static string _AccessKeySecret
            {
                get => AccessKeySecret;
                set
                {
                    if (value != AccessKeySecret)
                    {
                        AccessKeySecret = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ReadingRoomFiles, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }
        }

        public class Desktop
        {
            private static string DesktopRemoteServer = "false";
            /// <summary>
            /// 桌面版是否连接远程服务器（bool）
            /// 默认值：false
            /// </summary>
            public static bool _DesktopRemoteServer
            {
                get
                {
                    return bool.Parse(DesktopRemoteServer);
                }
                set
                {
                    if (value.ToString() != DesktopRemoteServer)
                    {
                        DesktopRemoteServer = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }
            private static string DesktopPort = "11419";
            /// <summary>
            /// 桌面版连接远程服务器的端口地址（当DesktopRemoteServer为True时生效）
            /// 默认值：11419
            /// </summary>
            public static int _DesktopPort
            {
                get => int.Parse(DesktopPort);
                set
                {
                    if (value.ToString() != DesktopPort)
                    {
                        DesktopPort = value.ToString();
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string DesktopIP = "127.0.0.1";
            /// <summary>
            /// 桌面版连接远程服务器的IP地址（当DesktopRemoteServer为True时生效）
            /// 默认值：127.0.0.1
            /// </summary>
            public static string _DesktopIP
            {
                get => DesktopIP;
                set
                {
                    if (value != DesktopIP)
                    {
                        DesktopIP = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

             private static string DesktopAccessKeyId = "ddtv";
            /// <summary>
            /// 桌面版连接远程服务器时使用的AccessKeyId（当DesktopRemoteServer为True时生效）
            /// 默认值：ddtv
            /// </summary>
            public static string _DesktopAccessKeyId
            {
                get => DesktopAccessKeyId;
                set
                {
                    if (value != DesktopAccessKeyId)
                    {
                        DesktopAccessKeyId = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }

            private static string DesktopAccessKeySecret = "ddtv";
            /// <summary>
            /// 桌面版连接远程服务器时使用的AccessKeySecret（当DesktopRemoteServer为True时生效）
            /// 默认值：ddtv
            /// </summary>
            public static string _DesktopAccessKeySecret
            {
                get => DesktopAccessKeySecret;
                set
                {
                    if (value != DesktopAccessKeySecret)
                    {
                        DesktopAccessKeySecret = value;
                        string msg = $"修改配置:[{MethodBase.GetCurrentMethod().Name}]-[{value}]";
                        OperationQueue.Add(Opcode.Config.ReadingRoomFiles, msg);
                        Log.Info(nameof(Config), msg);
                    }
                }
            }
        }

        #endregion
    }
}
