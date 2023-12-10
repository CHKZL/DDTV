using Core.Network.Methods;
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
                    if (item == "DetectIntervalTime=10000")
                    {
                        int B = int.Parse(item.Split('=')[1]);
                    }
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
                if (!File.Exists($"{Core._ConfigDirectory}{Core._RoomConfig}"))
                {
                    File.WriteAllText($"{Core._ConfigDirectory}{Core._RoomConfig}", "{}");
                }
                else
                {
                    Console.WriteLine("test");
                    string TEXT = File.ReadAllText("./Config/RoomListConfig.json");
                    //string TEXT = File.ReadAllText($@"{Core._ConfigDirectory}{Core._RoomConfig}");
                    var options = new JsonSerializerOptions();
                    options.TypeInfoResolver = null; // Unset the resolver for the options instance.
                    RoomListDiscard roomListDiscard = JsonSerializer.Deserialize<RoomListDiscard>(TEXT,options);
                    if (roomListDiscard != null)
                    {
                        foreach (var item in roomListDiscard.data)
                        {
                            RoomCard? roomCard = roomInfos.FirstOrDefault(x => x.UID == item.UID);
                            if (roomCard != null)
                            {
                                int index = roomInfos.FindIndex(x => x.UID == item.UID);
                                if (index != -1)
                                {
                                    roomInfos[index].UID = item.UID;
                                    roomInfos[index].description = item.description;
                                    roomInfos[index].RoomId = item.RoomId;
                                    roomInfos[index].Name = item.Name;
                                    roomInfos[index].IsAutoRec = item.IsAutoRec;
                                    roomInfos[index].IsRemind = item.IsRemind;
                                    roomInfos[index].IsRecDanmu = item.IsRecDanmu;
                                    roomInfos[index].Like = item.Like;
                                    roomInfos[index].Shell= item.Shell;
                                }
                            }
                            else
                            {
                                 roomInfos.Add(item);
                            }
                        }
                    }
                    else
                    {
                        File.WriteAllText($"{Core._ConfigDirectory}{Core._RoomConfig}", "{}");
                    }
                }
            }

            public static void SaveRoomConfigurationFile()
            {
                RoomListDiscard roomListDiscard = new RoomListDiscard();
                foreach (var item in roomInfos)
                {
                    roomListDiscard.data.Add(item);
                }
                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string jsonString = JsonSerializer.Serialize(roomListDiscard,options);
                File.WriteAllText($"{Core._ConfigDirectory}{Core._RoomConfig}", jsonString, Encoding.UTF8);
            }

            internal class RoomListDiscard
            {
                [JsonPropertyName("data")]
                public List<RuntimeObject.RoomList.RoomCard> data { set; get; } = [];
            }

        }


        public class Core
        {
            private static string RoomConfig = "RoomListConfig.json";
            /// <summary>
            /// 房间配置文件路径（字符串）
            /// 默认值：RoomListConfig.json
            /// </summary>
            public static string _RoomConfig { get { return RoomConfig; } }

            private static string ConfigDirectory = "./Config/";
            /// <summary>
            /// 配置文件路径（字符串）
            /// 默认值：./Config/
            /// </summary>
            public static string _ConfigDirectory { get { return ConfigDirectory; } }

            private static string ConfigurationFile = $"{ConfigDirectory}DDTV_Config.ini";
            /// <summary>
            /// 默认的配置文件路径（字符串）
            /// 默认值：./Config/DDTV_Config.ini
            /// </summary>
            public static string _ConfigurationFile { get { return ConfigurationFile; } }

            private static string Key = "34D3D9‭9D34894461‭91AB9B8‭582454669";
            /// <summary>
            /// 默认的AES加密秘钥（字符串）
            /// 默认值：34D3D99D3489446191AB9B8582454669
            /// </summary>
            public static string _Key { get { return Key; } }

            private static string IV = "B3FF‭40627013‭F53F";
            /// <summary>
            /// 默认的AES加密初始化向量（字符串）
            /// 默认值：B3FF40627013F53F
            /// </summary>
            public static string _IV { get { return IV; } }

            private static string UserInfoCoinfFileExtension = ".Duser";
            /// <summary>
            /// 用户配置文件拓展名（字符串）
            /// 默认值：.Duser
            /// </summary>
            public static string _UserInfoCoinfFileExtension { get { return UserInfoCoinfFileExtension; } }

            private static string LogFileDirectory = "./Logs/";
            /// <summary>
            /// 日志文件路径（字符串）
            /// 默认值：./Logs/
            /// </summary>
            public static string _LogFileDirectory { get { return LogFileDirectory; } }

            private static string RecFileDirectory = "./Rec/";
            /// <summary>
            /// 录制文件储存路径（字符串）
            /// 默认值：./Rec/
            /// </summary>
            public static string _RecFileDirectory { get { return RecFileDirectory; } }

            private static string TemporaryFileDirectory = "./Temporary/";
            /// <summary>
            /// 临时文件路径（字符串）
            /// 默认值：./Temporary/
            /// </summary>
            public static string _TemporaryFileDirectory { get { return TemporaryFileDirectory; } }

            private static string LiveDomainName = "https://api.live.bilibili.com";
            /// <summary>
            /// 默认使用的直播API域名（字符串）
            /// 默认值：https://api.live.bilibili.com
            /// </summary>
            public static string _LiveDomainName { get { return LiveDomainName; } set { LiveDomainName = value; } }

            private static string MainDomainName = "https://api.bilibili.com";
            /// <summary>
            /// 默认使用的主站API域名（字符串）
            /// 默认值：https://api.bilibili.com
            /// </summary>
            public static string _MainDomainName { get { return MainDomainName; } set { MainDomainName = value; } }

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
            public static int _DetectIntervalTime { get { return int.Parse(DetectIntervalTime); } set { DetectIntervalTime = value.ToString(); } }
        }
        public class Download
        {
            private static string DefaultResolution = "10000";
            /// <summary>
            /// 默认分辨率 默认值：10000    可选值：流畅:80  高清:150  超清:250  蓝光:400  原画:10000
            /// 默认值：https://api.bilibili.com
            /// </summary>
            public static int _DefaultResolution { get { return int.Parse(DefaultResolution); } set { DefaultResolution = value.ToString(); } }
        }



        #endregion
    }
}
