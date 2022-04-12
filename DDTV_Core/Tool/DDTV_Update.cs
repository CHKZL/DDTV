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
                if (pack.data.Ver == LocalVersion)
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
                return true;
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
                        CheckTheUpdateProgramForUpdates();
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
                                Thread.Sleep(30 * 1000);
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(30 * 1000);
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
                Log.AddLog("Check", LogClass.LogType.Info, "开始手动更新更新器", false, null, true);
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
                    SystemAssembly.NetworkRequestModule.Get.Get.GetFile(DownloadUrl, file.FilePath);
                    //Console.WriteLine("下载完成\n");
                }
                Log.AddLog("Check", LogClass.LogType.Info, "手动更新更新器OK", false, null, true);
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
