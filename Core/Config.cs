using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject;
using Masuit.Tools;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;
using static Core.RuntimeObject.RoomList;

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
            _Config.AddRange(typeof(Config.Core).GetFields(BindingFlags.NonPublic | BindingFlags.Static));
            _Config.AddRange(typeof(Config.Download).GetFields(BindingFlags.NonPublic | BindingFlags.Static));
            _Config.AddRange(typeof(Config.Web).GetFields(BindingFlags.NonPublic | BindingFlags.Static));

            lock (varMap)
                foreach (var fieldInfo in _Config)
                    varMap.Add(fieldInfo.Name, fieldInfo);

            RoomConfig.LoadRoomConfigurationFile();
        }

        #endregion

        #region public Method

        /// <summary>
        /// 从持久化配置文件读取配置刷新本地配置
        /// </summary>
        public static void ReadConfiguration()
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
            Log.Info(nameof(ReadConfiguration), $"读取配置文件完成");
        }
        /// <summary>
        /// 把当前配置写入到持久化配置文件
        /// </summary>
        public static void WriteConfiguration()
        {
            using (StreamWriter file = new(Core._ConfigurationFile))
            {
                lock (varMap)
                {
                    foreach (var item in varMap)
                    {
                        file.WriteLine($"{item.Key}={item.Value.GetValue(null)}");
                    }
                }
                Log.Info(nameof(ReadConfiguration), $"刷新配置文件完成");
            }
        }

        #endregion

        #region public Properties Method

        public class RoomConfig
        {
            /// <summary>
            /// 读取房间配置文件更新运行时参数
            /// </summary>
            public static void LoadRoomConfigurationFile()
            {
                (int Total, int Success, int Fail) Count = new(0, 0, 0);
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
                            RoomCard? roomCard = new();
                            if (_Room.GetCard(item.UID, ref roomCard))
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
                Log.Info(nameof(LoadRoomConfigurationFile), $"加载房间列表，一共{Count.Total}个/成功{Count.Success}个/失败{Count.Fail}个");
            }

            /// <summary>
            /// 将房间配置写入配置文件
            /// </summary>
            public static void SaveRoomConfigurationFile()
            {
                RoomListDiscard roomListDiscard = new RoomListDiscard();
                var roomInfos = _Room.GetCardListClone();
                foreach (var item in roomInfos)
                {
                    roomListDiscard.data.Add(item.Value);
                }
                string jsonString = JsonSerializer.Serialize(roomListDiscard);
                File.WriteAllText($"{Core._RoomConfigFile}", jsonString, Encoding.UTF8);
            }

           

            internal class RoomListDiscard
            {
                [JsonPropertyName("data")]
                public List<RuntimeObject.RoomList.RoomCard> data { set; get; } = [];
            }

        }


        public class Core
        {
            private static string LoginStatus = "false";
            /// <summary>
            /// 登陆状态
            /// 默认值：false
            /// </summary>
            public static bool _LoginStatus
            {
                get
                {
                    return bool.Parse(LoginStatus);
                }
                set { LoginStatus = value.ToString(); WriteConfiguration(); }
            }

            private static string RoomConfigFile = "RoomListConfig.json";
            /// <summary>
            /// 房间配置文件路径（字符串）
            /// 默认值：./Config/RoomListConfig.json
            /// </summary>
            public static string _RoomConfigFile
            {
                get
                {
                    return $"{_ConfigDirectory}{RoomConfigFile}";
                }
            }

            private static string ConfigDirectory = "./Config/";
            /// <summary>
            /// 配置文件路径（字符串）
            /// 默认值：./Config/
            /// </summary>
            public static string _ConfigDirectory
            {
                get
                {
                    return ConfigDirectory;
                }
            }

            private static string ConfigurationFile = $"DDTV_Config.ini";
            /// <summary>
            /// 默认的配置文件路径（字符串）
            /// 默认值：./Config/DDTV_Config.ini
            /// </summary>
            public static string _ConfigurationFile
            {
                get
                {
                    return $"{_ConfigDirectory}{ConfigurationFile}";
                }
            }

            private static string Key = "34D3D9‭9D34894461‭91AB9B8‭582454669";
            /// <summary>
            /// 默认的AES加密秘钥（字符串）
            /// 默认值：34D3D9‭9D34894461‭91AB9B8‭582454669
            /// </summary>
            public static string _Key
            {
                get
                {
                    return Key;
                }
            }

            private static string IV = "B3FF‭40627013‭F53F";
            /// <summary>
            /// 默认的AES加密初始化向量（字符串）
            /// 默认值：B3FF‭40627013‭F53F
            /// </summary>
            public static string _IV
            {
                get
                {
                    return IV;
                }
            }

            private static string UserInfoCoinfFileExtension = ".Duser";
            /// <summary>
            /// 用户配置文件拓展名（字符串）
            /// 默认值：.Duser
            /// </summary>
            public static string _UserInfoCoinfFileExtension
            {
                get
                {
                    return UserInfoCoinfFileExtension;
                }
            }

            private static string LogFileDirectory = "./Logs/";
            /// <summary>
            /// 日志文件路径（字符串）
            /// 默认值：./Logs/
            /// </summary>
            public static string _LogFileDirectory
            {
                get
                {
                    return LogFileDirectory;
                }
            }

            private static string RecFileDirectory = "./Rec/";
            /// <summary>
            /// 录制文件储存路径（字符串）
            /// 默认值：./Rec/
            /// </summary>
            public static string _RecFileDirectory
            {
                get
                {
                    return RecFileDirectory;
                }
            }

            private static string TemporaryFileDirectory = "./Temporary/";
            /// <summary>
            /// 临时文件路径（字符串）
            /// 默认值：./Temporary/
            /// </summary>
            public static string _TemporaryFileDirectory
            {
                get
                {
                    return TemporaryFileDirectory;
                }
            }

            private static string LiveDomainName = "https://api.live.bilibili.com";
            /// <summary>
            /// 默认使用的直播API域名（字符串）
            /// 默认值：https://api.live.bilibili.com
            /// </summary>
            public static string _LiveDomainName
            {
                get
                {
                    return LiveDomainName;
                }
                set { LiveDomainName = value; WriteConfiguration(); }
            }

            private static string MainDomainName = "https://api.bilibili.com";
            /// <summary>
            /// 默认使用的主站API域名（字符串）
            /// 默认值：https://api.bilibili.com
            /// </summary>
            public static string _MainDomainName
            {
                get
                {
                    return MainDomainName;
                }
                set { MainDomainName = value; WriteConfiguration(); }
            }

            private static string HTTP_UA = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0";
            /// <summary>
            /// 请求是默认使用的UA（字符串）
            /// 默认值：$"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0"
            /// </summary>
            public static string _HTTP_UA { get { return HTTP_UA; } }

            private static string DetectIntervalTime = "10000";
            /// <summary>
            /// 直播间状态更新间隔时间（int，单位毫秒）
            /// 默认值：10000
            /// </summary>
            public static int _DetectIntervalTime
            {
                get
                {
                    return int.Parse(DetectIntervalTime);
                }
                set { DetectIntervalTime = value.ToString(); WriteConfiguration(); }
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
                set { DebugMode = value.ToString(); WriteConfiguration(); }
            }
        }
        public class Download
        {
            private static string DefaultResolution = "10000";
            /// <summary>
            /// 默认分辨率 默认值：10000    可选值：流畅:80  高清:150  超清:250  蓝光:400  原画:10000
            /// 默认值：https://api.bilibili.com
            /// </summary>
            public static int _DefaultResolution
            {
                get
                {
                    return int.Parse(DefaultResolution);
                }
                set { DefaultResolution = value.ToString(); WriteConfiguration(); }
            }
        }


        public class Web
        {
            private static string AccessControlAllowCredentials = "true";
            /// <summary>
            /// WEB的Credentials设置 (布尔值)
            /// 默认值：true
            /// </summary>
            public static string _AccessControlAllowCredentials
            {
                get
                {
                    return AccessControlAllowCredentials;
                }
                set { AccessControlAllowCredentials = value.ToString(); WriteConfiguration(); }
            }

            private static string AccessControlAllowOrigin = "*";
            /// <summary>
            /// WEB跨域设置路径 （字符串:为*或者完整URL）（应为前端网址，必须带协议和端口号，如：http://127.0.0.1:5500）
            /// 默认值：*
            /// </summary>
            public static string _AccessControlAllowOrigin
            {
                get { return AccessControlAllowOrigin; }
                set
                {
                    AccessControlAllowOrigin = value.ToString();
                    WriteConfiguration();
                }
            }



            private static string AccessKeyId = "ddtv";
            /// <summary>
            /// API鉴权所使用的AccessKeyId 为字符串，默认"ddtv"
            /// 默认值：*
            /// </summary>
            public static string _AccessKeyId
            {
                get { return AccessKeyId; }
                set
                {
                    AccessKeyId = value.ToString();
                    WriteConfiguration();
                }
            }

            private static string AccessKeySecret = "ddtv";
            /// <summary>
            /// API鉴权所使用的AccessKeySecret 为字符串，默认"ami"
            /// 默认值：*
            /// </summary>
            public static string _AccessKeySecret
            {
                get { return AccessKeySecret; }
                set
                {
                    AccessKeySecret = value.ToString();
                    WriteConfiguration();
                }
            }
        }



        #endregion
    }
}
