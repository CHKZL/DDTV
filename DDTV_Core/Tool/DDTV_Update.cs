using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.Log;

namespace DDTV_Core.Tool
{
    public class DDTV_Update
    {
        public static bool ComparisonVersion(string Type, string LocalVersion)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>()
            {
                {"Type",Type },
                {"LocalVersion",LocalVersion },
                {"CAID",InitDDTV_Core.ClientAID }
            };
            try
            {
                string Ver = SystemAssembly.NetworkRequestModule.Post.Post.HttpPost("http://api.ddtv.pro/api/Ver", Parameters);
                ServerMessageClass.MessageBase.pack<ServerMessageClass.MessageClass.VerClass> pack = JsonConvert.DeserializeObject<ServerMessageClass.MessageBase.pack<ServerMessageClass.MessageClass.VerClass>>(Ver);
                Version v1 = new Version(pack.data.Ver);
                Version v2 = new Version(LocalVersion);
                if (v1 > v2)
                {
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
        public static bool IsOn = false;
        /// <summary>
        /// 检查更新程序是否是最新的
        /// </summary>
        public static void CheckUpdateProgram(bool IsUp=false)
        {
            if (IsUp)
            {
                Task.Run(() =>
                {
                    try
                    {
                        Log.AddLog("Check", LogClass.LogType.Info, "开始手动更新更新器", false, null, false);
                        CheckTheUpdateProgramForUpdates();
                        Log.AddLog("Check", LogClass.LogType.Info, "手动更新更新器OK", false, null, false);
                    }
                    catch (Exception){}
                });
            }
            else
            {
                Task.Run(() =>
                {
                    if (!IsOn)
                    {
                        while (true)
                        {
                            IsOn=true;
                            try
                            {
                                CheckTheUpdateProgramForUpdates();
                                Thread.Sleep(10800 * 1000);
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(60 * 1000);
                            }
                        }
                    }
                });
            }
        }
        /// <summary>
        /// 检查更新程序是否是最新的
        /// </summary>
        public static void CheckTheUpdateProgramForUpdates()
        {
            try
            {    
                string Type = "Update";
                string PushUrl = "https://update.ddtv.pro/";
                string _A = SystemAssembly.NetworkRequestModule.Get.Get.GetRequest(PushUrl + $"{Type}_Update.json", false, "https://Update.ddtv.pro/");
                FileInfoClass OldUpdateInfo = JsonConvert.DeserializeObject<FileInfoClass>(_A);
                FileInfoClass files = new FileInfoClass()
                {
                    Bucket = OldUpdateInfo.Bucket,
                    Description = OldUpdateInfo.Description,
                    Type = Type,
                    Ver = OldUpdateInfo.Ver,
                };
                foreach (var item in OldUpdateInfo.files)
                {
                    if (File.Exists(item.FilePath))
                    {
                        if (item.FileMd5 != SystemAssembly.EncryptionModule.Encryption.GetMD5HashFromFile(item.FilePath))
                        {
                            files.files.Add(item);
                        }
                    }
                    else
                    {
                        files.files.Add(item);
                    }
                }
                foreach (var file in files.files)
                {
                    string DownloadUrl = PushUrl + Type + "/" + file.FilePath;

                    //Console.WriteLine($"正在下载大小{file.Size}字节的文件:{file.FilePath}");
                    Log.AddLog("DDTV_Update", LogClass.LogType.Debug, $"检测到更新器文件变化，开始下载文件[{file.FilePath}({SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(file.Size)})]", false, null, false);
                    SystemAssembly.NetworkRequestModule.Get.Get.GetFile(DownloadUrl, file.FilePath);
                    Log.AddLog("DDTV_Update", LogClass.LogType.Debug, $"下载文件[{file.FilePath}]完成", false, null, false);
                }
               
            }
            catch (Exception)
            {
               
            }
        }

        public class FileInfoClass
        {
            public string Ver { get; set; }
            public string Description { get; set; }
            public List<Files> files { set; get; } = new List<Files>();
            public string Bucket { get; set; }
            public string Type { get; set; }
            public class Files
            {
                public string FileName { get; set; }
                public long Size { get; set; }
                public string FileMd5 { get; set; }
                public string FilePath { get; set; }
            }
        }
    }
}
