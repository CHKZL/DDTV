using Core.LogModule;
using Core.RuntimeObject;
using Core.Tools;
using System.ComponentModel;
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
            _Config.AddRange(typeof(Config.RunConfig).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(field => !field.IsAssembly));

            lock (varMap)
                foreach (var fieldInfo in _Config)
                {
                    varMap.Add(fieldInfo.Name, fieldInfo);
                    if (!fieldInfo.Name.ToLower().Contains("Access"))
                    {
                        string ConfigText = $"从配置文件获取参数初始化：{fieldInfo.Name}={fieldInfo.GetValue(null)}";
                        Log.Info(nameof(Config), ConfigText);
                    }
                }
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
                        Thread.Sleep(1000 * 3);
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
                        Thread.Sleep(1000 * 3);
                    }
                });
            }
        }
        public static void ModifyConfig(object value, [System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            string msg = $"修改配置:[{name}]-[{value}]";
            OperationQueue.Add(Opcode.Config.ModifyConfiguration, msg);
            Log.Info(nameof(Config), msg);
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
            { "no-update", ExpandOption.SetNoUpdate}
        };

        public static RunConfig Core_RunConfig { get; set; } = new();

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
                        Core_RunConfig._RecordingMode = RecordingMode.Auto;
                        break;
                    case "flv_only":
                        Core_RunConfig._RecordingMode = RecordingMode.FLV_Only;
                        break;
                    case "hls_only":
                        Core_RunConfig._RecordingMode = RecordingMode.HLS_Only;
                        break;
                }
            }
            internal static void SetNoUpdate(string value)
            {
                ProgramUpdates.Effective = false;
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
                string[] A = File.Exists(Core_RunConfig._ConfigurationFile) ? File.ReadAllLines(Core_RunConfig._ConfigurationFile) : [];
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
                string existingConfig = File.Exists(Core_RunConfig._ConfigurationFile) ? File.ReadAllText(Core_RunConfig._ConfigurationFile) : string.Empty;
                if (existingConfig != newConfig.ToString())
                {
                    using (StreamWriter file = new StreamWriter(Core_RunConfig._ConfigurationFile))
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
                    if (!Directory.Exists(Config.Core_RunConfig._ConfigDirectory))
                    {
                        Directory.CreateDirectory(Config.Core_RunConfig._ConfigDirectory);
                    }
                    if (!File.Exists($"{Core_RunConfig._RoomConfigFile}"))
                    {
                        File.WriteAllText($"{Core_RunConfig._RoomConfigFile}", "{}");
                    }
                    else
                    {
                        string TEXT = File.ReadAllText($@"{Core_RunConfig._RoomConfigFile}");
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
                            File.WriteAllText($"{Core_RunConfig._RoomConfigFile}", "{}");
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

                    string filePath = $"{Core_RunConfig._RoomConfigFile}";
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

            /// <summary>
            /// 从文件导入房间配置
            /// </summary>
            /// <param name="FilePath"></param>
            /// <param name="Count"></param>
            /// <returns></returns>
            public static bool ImportRoomConfiguration(string FilePath, out (int Total, int Success, int Fail, int Repeat, int NotPresent) Count)
            {
                Count = new(0, 0, 0, 0, 0);
                try
                {
                    if (File.Exists(FilePath))
                    {
                        string TEXT = File.ReadAllText(FilePath);
                        RoomListDiscard roomListDiscard = JsonSerializer.Deserialize<RoomListDiscard>(TEXT);
                        Count.Total = roomListDiscard.data.Count;
                        foreach (var item in roomListDiscard.data)
                        {
                            RoomCardClass? roomCard = new();
                            if (_Room.GetCardForUID(item.UID, ref roomCard))
                            {
                                Count.Repeat++;
                            }
                            else
                            {
                                if (item.UID > 0)
                                {

                                    if (_Room.SetRoomCardByUid(item.UID, item))
                                    {
                                        Count.Success++;
                                    }
                                    else
                                    {
                                        Count.Fail++;
                                    }
                                }
                                else
                                {
                                    Count.NotPresent++;
                                }

                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            internal class RoomListDiscard
            {
                [JsonPropertyName("data")]
                public List<RuntimeObject.RoomCardClass> data { set; get; } = [];
            }

        }

        public class RunConfig : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private static string ValidAccount = "-1";
            /// <summary>
            /// 生效的账号配置文件
            /// 默认值：./Temporary/LoginQr.png
            /// </summary>
            public string _ValidAccount
            {
                get => $"{ValidAccount}";
                set
                {
                    if (value != ValidAccount)
                    {
                        ValidAccount = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }


            private static string AutomaticRepair = "true";
            /// <summary>
            /// 录制完成自动修复和转码设置
            /// 默认值：true
            /// </summary>
            public bool _AutomaticRepair
            {
                get => bool.Parse(AutomaticRepair);
                set
                {
                    if (value.ToString() != AutomaticRepair)
                    {
                        AutomaticRepair = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }


            private static string AutomaticRepair_Arguments = "-y -i \"{before}\" -c copy \"{after}\"";
            /// <summary>
            /// 修复和转码的执行参数
            /// 默认值：-y -i "{before}" -c copy "{after}"
            /// </summary>
            public string _AutomaticRepair_Arguments
            {
                get => AutomaticRepair_Arguments;
                set
                {
                    if (value.ToString() != AutomaticRepair_Arguments)
                    {
                        AutomaticRepair_Arguments = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string ForceMerge = "false";
            /// <summary>
            /// 录制完成强制合并为一个视频文件
            /// 默认值：true
            /// </summary>
            public bool _ForceMerge
            {
                get => bool.Parse(ForceMerge);
                set
                {
                    if (value.ToString() != ForceMerge)
                    {
                        ForceMerge = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }




            private static string UseAgree = "false";
            /// <summary>
            /// 用户协议同意状态
            /// 默认值：false
            /// </summary>
            public bool _UseAgree
            {
                get => bool.Parse(UseAgree);
                set
                {
                    if (value.ToString() != UseAgree)
                    {
                        UseAgree = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            internal static string QrUrl = "QrUrlPipeline";
            /// <summary>
            /// 登陆用扫码二维码路径和文件名
            /// 默认值：./Temporary/QrUrlPipeline
            /// </summary>
            public string _QrUrl
            {
                get => $"{_TemporaryFileDirectory}{QrUrl}";
            }

            internal static string QrFileNmae = "LoginQr.png";
            /// <summary>
            /// 登陆用扫码二维码路径和文件名
            /// 默认值：./Temporary/LoginQr.png
            /// </summary>
            public string _QrFileNmae
            {
                get => $"{_TemporaryFileDirectory}{QrFileNmae}";

            }


            internal static string RoomConfigFile = "RoomListConfig.json";
            /// <summary>
            /// 房间配置文件路径（字符串）
            /// 默认值：./Config/RoomListConfig.json
            /// </summary>
            public string _RoomConfigFile
            {
                get => $"{_ConfigDirectory}{RoomConfigFile}";
            }



            internal static string ConfigDirectory = "./Config/";
            /// <summary>
            /// 配置文件路径（字符串）
            /// 默认值：./Config/
            /// </summary>
            public string _ConfigDirectory
            {
                get => ConfigDirectory;
            }

            internal static string ConfigurationFile = $"DDTV_Config.ini";
            /// <summary>
            /// 默认的配置文件路径（字符串）
            /// 默认值：./Config/DDTV_Config.ini
            /// </summary>
            public string _ConfigurationFile
            {
                get => $"{_ConfigDirectory}{ConfigurationFile}";
            }

            private static string Key = "34D3D9‭9D34894461‭91AB9B8‭582454669";
            /// <summary>
            /// 默认的AES加密秘钥（字符串）
            /// 默认值：34D3D9‭9D34894461‭91AB9B8‭582454669
            /// </summary>
            public string _Key
            {
                get => Key;
            }

            private static string IV = "B3FF‭40627013‭F53F";
            /// <summary>
            /// 默认的AES加密初始化向量（字符串）
            /// 默认值：B3FF‭40627013‭F53F
            /// </summary>
            public string _IV
            {
                get => IV;
            }

            internal static string UserInfoCoinfFileExtension = ".Duser";
            /// <summary>
            /// 用户配置文件拓展名（字符串）
            /// 默认值：.Duser
            /// </summary>
            public string _UserInfoCoinfFileExtension
            {
                get => UserInfoCoinfFileExtension;
            }

            internal static string LogFileDirectory = "./Logs/";
            /// <summary>
            /// 日志文件路径（字符串）
            /// 默认值：./Logs/
            /// </summary>
            public string _LogFileDirectory
            {
                get => LogFileDirectory;
            }

            public string PathExample
            {
                get
                {
                    string F = $"{RecFileDirectory}{DefaultLiverFolderName}/{DefaultDataFolderName}{(string.IsNullOrEmpty(DefaultDataFolderName) ? "" : "/")}{DefaultFileName}.mp4";
                    F = KeyCharacterReplacement.ReplaceKeyword(F, DateTime.Parse("2016-12-01 22:33"), -1);
                    return F;
                }
                set
                {
                    OnPropertyChanged();
                }
            }

            private static string RecFileDirectory = "./Rec/";
            /// <summary>
            /// 录制文件储存路径（字符串）
            /// 默认值：./Rec/
            /// </summary>
            public string _RecFileDirectory
            {
                get => RecFileDirectory;
                set
                {
                    if (value != RecFileDirectory)
                    {
                        RecFileDirectory = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                        PathExample = PathExample;
                    }
                }
            }


            private static string DefaultLiverFolderName = "{ROOMID}_{NAME}";
            /// <summary>
            /// 默认一级主播名文件夹格式 (应该为关键字组合，如:{KEY}_{KEY}) 可选值：ROOMID|NAME|DATE|TIME|TITLE
            /// 默认值：{ROOMID}_{NAME}
            /// </summary>
            public string _DefaultLiverFolderName
            {
                get => DefaultLiverFolderName;
                set
                {
                    if (value != DefaultLiverFolderName)
                    {
                        DefaultLiverFolderName = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                        PathExample = PathExample;
                    }
                }
            }

            private static string DefaultDataFolderName = "{YYYY}_{MM}_{DD}";
            /// <summary>
            /// 默认二级主播名下日期分类文件夹格式 (应该为关键字组合，如:{KEY}_{KEY}) 可选值：(年(1949){YYYY}、年(49){YY}、月{MM}、日{DD})
            /// 默认值：{ROOMID}_{NAME}
            /// </summary>
            public string _DefaultDataFolderName
            {
                get => DefaultDataFolderName;
                set
                {
                    if (value != DefaultDataFolderName)
                    {
                        DefaultDataFolderName = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                        PathExample = PathExample;
                    }
                }
            }

            private static string DefaultFileName = "{DATE}_{TIME}_{TITLE}";
            /// <summary>
            /// 默认下载文件名格式,文件名会固定以[_original.mp4]或[_fix.mp4]结尾，具体是哪个取决于[AutomaticRepair]配置状态 (应该为关键字组合，如:{KEY}_{KEY}) 可选值：ROOMID|NAME|DATE|TIME|TITLE
            /// 默认值：{DATE}_{TIME}_{TITLE}
            /// </summary>
            public string _DefaultFileName
            {
                get => DefaultFileName;
                set
                {
                    if (value != DefaultFileName)
                    {
                        DefaultFileName = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }


            internal static string TemporaryFileDirectory = "./Temporary/";
            /// <summary>
            /// 临时文件路径（字符串）
            /// 默认值：./Temporary/
            /// </summary>
            public string _TemporaryFileDirectory
            {
                get => TemporaryFileDirectory;
            }


            private static string LiveDomainName = "https://api.live.bilibili.com";
            /// <summary>
            /// 默认使用的直播API域名（字符串）
            /// 默认值：https://api.live.bilibili.com
            /// </summary>
            public string _LiveDomainName
            {
                get => LiveDomainName;
                set
                {
                    if (value != LiveDomainName)
                    {
                        LiveDomainName = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string MainDomainName = "https://api.bilibili.com";
            /// <summary>
            /// 默认使用的主站API域名（字符串）
            /// 默认值：https://api.bilibili.com
            /// </summary>
            public string _MainDomainName
            {
                get => MainDomainName;
                set
                {
                    if (value != MainDomainName)
                    {
                        MainDomainName = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string HTTP_UA = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0";
            /// <summary>
            /// 请求是默认使用的UA（字符串）
            /// 默认值：$"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0"
            /// </summary>
            public string _HTTP_UA { get { return HTTP_UA; } }

            private static string HlsWaitingTime = "30";
            /// <summary>
            /// 一个新任务等待HLS的时间（int，单位秒）
            /// 默认值：30
            /// </summary>
            public int _HlsWaitingTime
            {
                get => int.Parse(HlsWaitingTime);
                set
                {
                    if (value.ToString() != HlsWaitingTime)
                    {
                        HlsWaitingTime = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DetectIntervalTime = "10000";
            /// <summary>
            /// 直播间状态更新间隔时间（int，单位毫秒）
            /// 默认值：10000
            /// </summary>
            public int _DetectIntervalTime
            {
                get => int.Parse(DetectIntervalTime);
                set
                {
                    if (value.ToString() != DetectIntervalTime)
                    {
                        DetectIntervalTime = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DebugMode = "false";
            /// <summary>
            /// 调试模式开关（bool）
            /// 默认值：false
            /// </summary>
            public bool _DebugMode
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
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string RecordingMode = "1";
            /// <summary>
            /// 录制模式，提供：Auto\FLV_Only\HLS_Only 三种模式取值分别是1/2/3,详细说明请查看Core.RuntimeObject.Download.Basics.RecordingMode
            /// 默认值：Auto
            /// </summary>
            public RecordingMode _RecordingMode
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
            public int RecordingMode_For_ComboBox
            {
                get
                {
                    return (int)_RecordingMode - 1;
                }
            }

            private static string DeleteOriginalFileAfterRepair = "true";
            /// <summary>
            /// 修复完成后删除源文件（bool）
            /// 默认值：false
            /// </summary>
            public bool _DeleteOriginalFileAfterRepair
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
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DefaultResolution = "10000";
            /// <summary>
            /// 默认分辨率 默认值：10000    可选值：流畅:80  高清:150  超清:250  蓝光:400  原画:10000  4K:20000  杜比:30000
            /// 默认值：10000
            /// </summary>
            public int _DefaultResolution
            {
                get => int.Parse(DefaultResolution);
                set
                {
                    if (value.ToString() != DefaultResolution)
                    {
                        DefaultResolution = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            /// <summary>
            /// 默认录制分辨率设置界面下拉框选项
            /// </summary>
            public int _DefaultResolution_For_ComboBox
            {
                get
                {
                   switch(_DefaultResolution)
                    {
                        case 80:
                            return 0;
                        case 150:
                            return 1;
                        case 250:
                            return 2;
                        case 400:
                            return 3;
                        case 10000:
                            return 4;
                        case 20000:
                            return 5;
                        case 30000:
                            return 6;
                        default:
                            return 4; 
                    }
                }
                set
                {
                    switch (value)
                    {
                        case 0:
                            _DefaultResolution = 80;
                            break;
                        case 1:
                            _DefaultResolution = 150;
                            break;
                        case 2:
                            _DefaultResolution = 250;
                            break;
                        case 3:
                            _DefaultResolution = 400;
                            break;
                        case 4:
                            _DefaultResolution = 10000;
                            break;
                        case 5:
                            _DefaultResolution = 20000;
                            break;
                        case 6:
                            _DefaultResolution = 30000;
                            break;
                    }
                    OnPropertyChanged();
                }
            }

            private static string Port = "11419";
            /// <summary>
            /// Api提供的端口地址
            /// 默认值：11419
            /// </summary>
            public int _Port
            {
                get => int.Parse(Port);
                set
                {
                    if (value.ToString() != Port)
                    {
                        Port = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string IP = "http://127.0.0.1";
            /// <summary>
            /// WEB服务监听的IP地址
            /// 默认值：http://127.0.0.1
            /// </summary>
            public string _IP
            {
                get => IP;
                set
                {
                    if (value != IP)
                    {
                        IP = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            internal static string RecordingStorageDirectory = "/rec_file";
            /// <summary>
            /// Web返回录制文件的相对根路径（字符串）
            /// 默认值：/rec_file
            /// </summary>
            public string _RecordingStorageDirectory
            {
                get => RecordingStorageDirectory;
            }

            internal static string WebUiDirectory = "./Static/";
            /// <summary>
            /// WebUi文件路径（字符串）
            /// 默认值：./Static/
            /// </summary>
            public string _WebUiDirectory
            {
                get => WebUiDirectory;
            }

            private static string AccessControlAllowCredentials = "true";
            /// <summary>
            /// WEB的Credentials设置 (布尔值)
            /// 默认值：true
            /// </summary>
            public string _AccessControlAllowCredentials
            {
                get => AccessControlAllowCredentials;
                set
                {
                    if (value != AccessControlAllowCredentials)
                    {
                        AccessControlAllowCredentials = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string AccessControlAllowOrigin = "*";
            /// <summary>
            /// WEB跨域设置路径 （字符串:为*或者完整URL）（完整URL应为前端网址，必须带协议和端口号，如：http://127.0.0.1:11419）
            /// 默认值：*
            /// </summary>
            public string _AccessControlAllowOrigin
            {
                get => AccessControlAllowOrigin;
                set
                {
                    if (value != AccessControlAllowOrigin)
                    {
                        AccessControlAllowOrigin = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }



            private static string AccessKeyId = "ddtv";
            /// <summary>
            /// API鉴权所使用的AccessKeyId 为字符串，默认"ddtv"
            /// 默认值：ddtv
            /// </summary>
            public string _AccessKeyId
            {
                get => AccessKeyId;
                set
                {
                    if (value != AccessKeyId)
                    {
                        AccessKeyId = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string AccessKeySecret = "ddtv";
            /// <summary>
            /// API鉴权所使用的AccessKeySecret 为字符串，默认"ddtv"
            /// 默认值：ddtv
            /// </summary>
            public string _AccessKeySecret
            {
                get => AccessKeySecret;
                set
                {
                    if (value != AccessKeySecret)
                    {
                        AccessKeySecret = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }


            private static string DesktopRemoteServer = "false";
            /// <summary>
            /// 桌面版是否连接远程服务器（bool）
            /// 默认值：false
            /// </summary>
            public bool _DesktopRemoteServer
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
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DesktopPort = "11419";
            /// <summary>
            /// 桌面版连接远程服务器的端口地址（当DesktopRemoteServer为True时生效）
            /// 默认值：11419
            /// </summary>
            public int _DesktopPort
            {
                get
                {
                    if (_DesktopRemoteServer)
                        return int.Parse(DesktopPort);
                    else
                        return Core_RunConfig._Port;
                }
                set
                {
                    if (value.ToString() != DesktopPort)
                    {
                        DesktopPort = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DesktopIP = "http://127.0.0.1";
            /// <summary>
            /// 桌面版连接远程服务器的IP地址（当DesktopRemoteServer为True时生效）
            /// 默认值：http://127.0.0.1
            /// </summary>
            public string _DesktopIP
            {
                get
                {
                    if (_DesktopRemoteServer)
                        return DesktopIP;
                    else
                        return Core_RunConfig._IP;
                }
                set
                {
                    if (value != DesktopIP)
                    {
                        DesktopIP = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DesktopAccessKeyId = "ddtv";
            /// <summary>
            /// 桌面版连接远程服务器时使用的AccessKeyId（当DesktopRemoteServer为True时生效）
            /// 默认值：ddtv
            /// </summary>
            public string _DesktopAccessKeyId
            {
                get
                {
                    if (_DesktopRemoteServer)
                        return DesktopAccessKeyId;
                    else
                        return Core_RunConfig._AccessKeyId;
                }
                set
                {
                    if (value != DesktopAccessKeyId)
                    {
                        DesktopAccessKeyId = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DesktopAccessKeySecret = "ddtv";
            /// <summary>
            /// 桌面版连接远程服务器时使用的AccessKeySecret（当DesktopRemoteServer为True时生效）
            /// 默认值：ddtv
            /// </summary>
            public string _DesktopAccessKeySecret
            {
                get
                {
                    if (_DesktopRemoteServer)
                        return DesktopAccessKeySecret;
                    else
                        return Core_RunConfig._AccessKeySecret;
                }
                set
                {
                    if (value != DesktopAccessKeySecret)
                    {
                        DesktopAccessKeySecret = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            /// <summary>
            /// 当前登录态
            /// 默认值：不会写到配置文件中，也不提供修改
            /// </summary>
            public string _CurrentLoginStatus
            {
                get
                {
                    return $"{Core.RuntimeObject.Account.AccountInformation.Uid}　　　(登陆有效期到:{Core.RuntimeObject.Account.AccountInformation.Expires_Cookies.ToString("yyyy-MM-dd HH:mm:ss")})";
                }
                set
                {
                    OnPropertyChanged();
                }
            }


            private static string ZoomOutMode = "0";
            /// <summary>
            /// 缩小操作默认行为  0:缩小到任务栏   1:缩小到托盘后台
            /// 默认值：0
            /// </summary>
            public int _ZoomOutMode
            {
                get => int.Parse(ZoomOutMode);
                set
                {
                    if (value.ToString() != ZoomOutMode)
                    {
                        ZoomOutMode = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string SystemCardReminder = "true";
            /// <summary>
            /// 开播后Desktop版是否触发系统卡片提醒
            /// 默认值：true
            /// </summary>
            public bool _SystemCardReminder
            {
                get
                {
                    return bool.Parse(SystemCardReminder);
                }
                set
                {
                    if (value.ToString() != SystemCardReminder)
                    {
                        SystemCardReminder = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DevelopmentVersion = "false";
            /// <summary>
            /// 是否接受开发版更新
            /// 默认值：false
            /// </summary>
            public bool _DevelopmentVersion
            {
                get
                {
                    return bool.Parse(DevelopmentVersion);
                }
                set
                {
                    if (value.ToString() != DevelopmentVersion)
                    {
                        DevelopmentVersion = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }


            private static string CutAccordingToSize = "0";
            /// <summary>
            /// 根据文件大小切割视频文件(字节)
            /// 默认值：0
            /// </summary>
            public long _CutAccordingToSize
            {
                get => long.Parse(CutAccordingToSize);
                set
                {
                    if (value.ToString() != CutAccordingToSize)
                    {
                        CutAccordingToSize = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            /// <summary>
            /// 根据文件大小切割视频文件下拉框选项
            /// </summary>
            public int CutAccordingToSize_For_ComboBox
            {
                get
                {
                    long one = 1024 * 1024 * 1024;
                    if (_CutAccordingToSize < 1)
                        return 0;
                    double index = (double)_CutAccordingToSize / (double)one;
                    if (index < 2)
                        return 1;
                    if (index < 3)
                        return 2;
                    if (index < 5)
                        return 3;
                    if (index < 9)
                        return 4;
                    if (index < 17)
                        return 5;
                    return 5;
                }
                set
                {
                    long one = 1024 * 1024 * 1024;
                    switch (value)
                    {
                        case 0:
                            _CutAccordingToSize = 0;
                            break;
                        case 1:
                            _CutAccordingToSize = 1 * one;
                            break;
                        case 2:
                            _CutAccordingToSize = 2 * one;
                            break;
                        case 3:
                            _CutAccordingToSize = 4 * one;
                            break;
                        case 4:
                            _CutAccordingToSize = 8 * one;
                            break;
                        case 5:
                            _CutAccordingToSize = 16 * one;
                            break;
                    }
                    OnPropertyChanged();
                }
            }

            private static string CutAccordingToTime = "0";
            /// <summary>
            /// 根据录制时长切割视频文件(秒)
            /// 默认值：0
            /// </summary>
            public long _CutAccordingToTime
            {
                get => long.Parse(CutAccordingToTime);
                set
                {
                    if (value.ToString() != CutAccordingToTime)
                    {
                        CutAccordingToTime = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            /// <summary>
            /// 根据录制时长切割视频文件下拉框选项
            /// </summary>
            public int CutAccordingToTime_For_ComboBox
            {
                get
                {
                    if (_CutAccordingToTime < 1)
                        return 0;
                    double index = (double)_CutAccordingToTime / 60;
                    if (index <= 10)
                        return 1;
                    if (index <= 20)
                        return 2;
                    if (index <= 30)
                        return 3;
                    if (index <= 60)
                        return 4;
                    if (index <= 120)
                        return 5;

                    return 0;
                }
                set
                {
                    switch (value)
                    {
                        case 0:
                            _CutAccordingToTime = 0;
                            break;
                        case 1:
                            _CutAccordingToTime = 10 * 60;
                            break;
                        case 2:
                            _CutAccordingToTime = 20 * 60;
                            break;
                        case 3:
                            _CutAccordingToTime = 30 * 60;
                            break;
                        case 4:
                            _CutAccordingToTime = 60 * 60;
                            break;
                        case 5:
                            _CutAccordingToTime = 120 * 60;
                            break;
                    }
                    OnPropertyChanged();
                }
            }

            private static string DesktopWidth = "1250";
            /// <summary>
            /// 桌面版最后的宽度
            /// 默认值：1250
            /// </summary>
            public int _DesktopWidth
            {
                get
                {
                    return int.Parse(DesktopWidth);
                }
                set
                {
                    if (value.ToString() != DesktopWidth)
                    {
                        DesktopWidth = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string DesktopHeight = "650";
            /// <summary>
            /// 桌面版最后的高度
            /// 默认值：650
            /// </summary>
            public int _DesktopHeight
            {
                get
                {
                    return int.Parse(DesktopHeight);
                }
                set
                {
                    if (value.ToString() != DesktopHeight)
                    {
                        DesktopHeight = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }


            private static string WebHookSwitch = "false";
            /// <summary>
            /// WebHook功能开关标志
            /// 默认值：false
            /// </summary>
            public bool _WebHookSwitch
            {
                get
                {
                    return bool.Parse(WebHookSwitch);
                }
                set
                {
                    if (value.ToString() != WebHookSwitch)
                    {
                        WebHookSwitch = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }
            private static string WebHookAddress = string.Empty;
            /// <summary>
            /// WebHook功能的推送目标地址
            /// 默认值：string.Empty(空)
            /// </summary>
            public string _WebHookAddress
            {
                get
                {
                    return WebHookAddress;
                }
                set
                {
                    if (value != WebHookAddress)
                    {
                        WebHookAddress = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayWindowSubtitleColor = "0xFF,0xFF,0xFF";
            /// <summary>
            /// 播放窗口字幕颜色
            /// 默认值：0xFF,0xFF,0xFF
            /// </summary>
            public string _PlayWindowSubtitleColor
            {
                get
                {
                    return PlayWindowSubtitleColor;
                }
                set
                {
                    if (value != PlayWindowSubtitleColor)
                    {
                        PlayWindowSubtitleColor = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayWindowDanmaColor = "0xFF,0xFF,0xFF";
            /// <summary>
            /// 播放窗口弹幕颜色
            /// 默认值：0xFF,0xFF,0xFF
            /// </summary>
            public string _PlayWindowDanmaColor
            {
                get
                {
                    return PlayWindowDanmaColor;
                }
                set
                {
                    if (value != PlayWindowDanmaColor)
                    {
                        PlayWindowDanmaColor = value;
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayWindowSubtitleFontSize = "30";
            /// <summary>
            /// 播放窗口字幕字号
            /// 默认值：30
            /// </summary>
            public int _PlayWindowSubtitleFontSize
            {
                get
                {
                    return int.Parse(PlayWindowSubtitleFontSize);
                }
                set
                {
                    if (value.ToString() != PlayWindowSubtitleFontSize)
                    {
                        PlayWindowSubtitleFontSize = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayWindowDanmaFontSize = "30";
            /// <summary>
            /// 播放窗口弹幕字号
            /// 默认值：30
            /// </summary>
            public int _PlayWindowDanmaFontSize
            {
                get
                {
                    return int.Parse(PlayWindowDanmaFontSize);
                }
                set
                {
                    if (value.ToString() != PlayWindowDanmaFontSize)
                    {
                        PlayWindowDanmaFontSize = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayWindowDanMuFontOpacity = "0.8";
            /// <summary>
            /// 播放窗口弹幕透明度（浮点）
            /// 默认值：0.8
            /// </summary>
            public double _PlayWindowDanMuFontOpacity
            {
                get
                {
                    return double.Parse(PlayWindowDanMuFontOpacity);
                }
                set
                {
                    if (value.ToString() != PlayWindowDanMuFontOpacity)
                    {
                        PlayWindowDanMuFontOpacity = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayWindowDanmaSpeed = "6";
            /// <summary>
            /// 播放窗口弹幕速度
            /// 默认值：15
            /// </summary>
            public int _PlayWindowDanmaSpeed
            {
                get
                {
                    return int.Parse(PlayWindowDanmaSpeed);
                }
                set
                {
                    if (value.ToString() != PlayWindowDanmaSpeed)
                    {
                        PlayWindowDanmaSpeed = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayDanmaSpeed_Dynamically = "false";
            /// <summary>
            /// 播放窗口弹幕速度是否跟随窗口变化变化
            /// 默认值：false
            /// </summary>
            public bool _PlayDanmaSpeed_Dynamically
            {
                get
                {
                    return bool.Parse(PlayDanmaSpeed_Dynamically);
                }
                set
                {
                    if (value.ToString() != PlayDanmaSpeed_Dynamically)
                    {
                        PlayDanmaSpeed_Dynamically = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string PlayWindowDanmaSwitch = "false";
            /// <summary>
            /// 打开播放窗时，是否默认打开弹幕
            /// 默认值：false
            /// </summary>
            public bool _PlayWindowDanmaSwitch
            {
                get
                {
                    return bool.Parse(PlayWindowDanmaSwitch);
                }
                set
                {
                    if (value.ToString() != PlayWindowDanmaSwitch)
                    {
                        PlayWindowDanmaSwitch = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string Linux_Only_ShellSwitch = "false";
            /// <summary>
            /// 是否开启Linux下的Shell脚本支持
            /// 默认值：false
            /// </summary>
            public bool _Linux_Only_ShellSwitch
            {
                get
                {
                    return bool.Parse(Linux_Only_ShellSwitch);
                }
                set
                {
                    if (value.ToString() != Linux_Only_ShellSwitch)
                    {
                        Linux_Only_ShellSwitch = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string Linux_Only_ShellCommand = string.Empty;
            /// <summary>
            /// Linux下将执行的Shell命令模版
            /// 默认值：空字符串(string.Empty)
            /// </summary>
            public string _Linux_Only_ShellCommand
            {
                get
                {
                    return Linux_Only_ShellCommand;
                }
                set
                {
                    if (value.ToString() != Linux_Only_ShellCommand)
                    {
                        Linux_Only_ShellCommand = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string SaveCover = "false";
            /// <summary>
            /// 开始录制时时候保存当前直播间封面
            /// 默认值：false
            /// </summary>
            public bool _SaveCover
            {
                get
                {
                    return bool.Parse(SaveCover);
                }
                set
                {
                    if (value.ToString() != SaveCover)
                    {
                        SaveCover = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

            private static string AutomaticFileCleaningThreshold = "8388608";
            /// <summary>
            /// 自动删除过小文件的阈值
            /// 默认值：8388608 （8*1024*1024，8MB）
            /// </summary>
            public long _AutomaticFileCleaningThreshold
            {
                get => long.Parse(AutomaticFileCleaningThreshold);
                set
                {
                    if (value.ToString() != AutomaticFileCleaningThreshold)
                    {
                        AutomaticFileCleaningThreshold = value.ToString();
                        OnPropertyChanged();
                        ModifyConfig(value);
                    }
                }
            }

        }
        #endregion
    }
}
