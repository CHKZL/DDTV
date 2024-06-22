using AngleSharp.Dom;
using Core.LogModule;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class ProgramUpdates
    {
        public static string MainDomainName = "https://ddtv-update.top";
        public static string AlternativeDomainName = "https://update5.ddtv.pro";
        private static string verFile = "./ver.ini";
        private static string type = string.Empty;
        private static string ver = string.Empty;
        public static event EventHandler<EventArgs> NewVersionAvailableEvent;//检测到新版本
        /// <summary>
        /// 更新程序是否生效
        /// </summary>
        public static bool Effective = true;

        public static async void RegularInspection(object state)
        {
            if (Effective)
                await CheckForNewVersions();
        }

        internal static bool GetCurrentVersion()
        {
            if (File.Exists(verFile))
            {
                string[] Ver = File.ReadAllLines(verFile);
                foreach (string VerItem in Ver)
                {
                    if (VerItem.StartsWith("type="))
                        type = VerItem.Split('=')[1].TrimEnd();
                    if (VerItem.StartsWith("ver="))
                        ver = VerItem.Split('=')[1].TrimEnd();
                }
                ver = ver.ToLower().Replace("dev", "");
                ver = ver.ToLower().Replace("release", "");
                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(ver))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检查有没有新版本
        /// </summary>
        /// <param name="AutoUpdate">是否唤起自动更新</param>
        /// <param name="Manual">是否为手动检查更新</param>
        /// <returns></returns>
        public static async Task<bool> CheckForNewVersions(bool AutoUpdate = false, bool Manual = false)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!GetCurrentVersion())
                    {
                        return false;
                    }
                    string DL_VerFileUrl = $"/{type}/{(Config.Core_RunConfig._DevelopmentVersion ? "dev" : "release")}/ver.ini";
                    string R_Ver = Get(DL_VerFileUrl).TrimEnd().Replace("dev", "").Replace("release", "");
                    if (!string.IsNullOrEmpty(R_Ver) && R_Ver.Split('.').Length > 0)
                    {
                        //老版本
                        Version Before = new Version(ver);
                        //新版本
                        Version After = new Version(R_Ver);
                        if (After > Before)
                        {
                            Update_UpdateProgram update_UpdateProgram = new();
                            update_UpdateProgram.Main([(Config.Core_RunConfig._DevelopmentVersion ? "dev" : "release")]);

                            if (!Manual)
                                NewVersionAvailableEvent?.Invoke(R_Ver, new EventArgs());
                            if (AutoUpdate)
                                CallUpUpdateProgram();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Log.Info(nameof(ProgramUpdates), $"获取新版本失败，请检查网络和代理状况");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(ProgramUpdates), $"获取新版本出现错误，错误堆栈:{ex.ToString()}", ex);
                    return false;
                }
            });
        }
        public static void CallUpUpdateProgram()
        {
            if (File.Exists("./Update/Update.exe"))
            {
                Process process = new Process();
                process.StartInfo.FileName = "./Update/Update.exe";
                process.StartInfo.Arguments = $"{(Core.Config.Core_RunConfig._DevelopmentVersion ? "dev" : "release")}";
                process.Start();
                Environment.Exit(-114514);
            }
            else
            {
                Log.Error(nameof(ProgramUpdates), $"找不到自动更新脚本程序DDTV_Update.exe");
            }
        }
        public static string Get(string URL)
        {
            using (HttpClient _httpClient = new HttpClient())
            {
                bool A = false;
                string str = string.Empty;
                int error_count = 0;
                string FileDownloadAddress = string.Empty;
                do
                {
                    try
                    {
                        if (A)
                            Thread.Sleep(1000);
                        if (!A)
                            A = true;
                        if (error_count > 1)
                        {
                            if (error_count > 3)
                            {
                                Log.Info(nameof(ProgramUpdates), $"更新失败，网络错误过多，请检查网络状况或者代理设置后重试.....");
                                return "";
                            }
                            Log.Info(nameof(ProgramUpdates), $"使用备用服务器进行重试.....");
                            FileDownloadAddress = AlternativeDomainName + URL;
                            Log.Info(nameof(ProgramUpdates), $"从主服务器获取更新失败，尝试从备用服务器获取....");
                        }
                        else
                        {
                            FileDownloadAddress = MainDomainName + URL;
                        }
                        _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                        str = _httpClient.GetStringAsync(FileDownloadAddress).Result;
                    }
                    catch (WebException webex)
                    {
                        error_count++;
                        switch (webex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Info(nameof(ProgramUpdates), $"下载文件超时:{FileDownloadAddress}");
                                break;

                            default:
                                Log.Info(nameof(ProgramUpdates), $"网络错误，请检查网络状况或者代理设置...开始重试.....");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        error_count++;
                        Console.WriteLine($"出现网络错误，错误详情：{ex.ToString()}\r\n\r\n===========下载执行重试，如果没同一个文件重复提示错误，则表示重试成功==============\r\n");


                    }
                } while (string.IsNullOrEmpty(str));

                return str;
            }
        }

        public class Update_UpdateProgram
        {
            public static string MainDomainName = "https://ddtv-update.top";
            public static string AlternativeDomainName = "https://update5.ddtv.pro";
            public static string verFile = "../ver.ini";
            public static string type = string.Empty;
            public static string ver = string.Empty;
            public static string R_ver = string.Empty;
            public static bool Isdev = false;

            
            public void Main(string[] args)
            {
                if (args.Length != 0)
                {
                    if (args.Contains("dev"))
                    {
                        Isdev = true;
                    }
                    if (args.Contains("release"))
                    {
                        Isdev = false;
                    }
                }

                Log.Info(nameof(Update_UpdateProgram),"开始更新DDTV_Update");
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
                Log.Info(nameof(Update_UpdateProgram),$"当前工作路径:{Environment.CurrentDirectory}");
                Dictionary<string, (string Name, string FilePath, long Size)> map = new Dictionary<string, (string Name, string FilePath, long Size)>();
                using (HttpClient _httpClient = new HttpClient())
                {
                    _httpClient.Timeout = new TimeSpan(0, 0, 10);
                    _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                    if (checkVersion())
                    {
                        string DL_FileListUrl = $"/{type}/{(Isdev ? "dev" : "release")}/{type}_Update.json";
                        string web = Get(DL_FileListUrl);
                        var B = JsonConvert.DeserializeObject<FileInfoClass>(web);
                        if (B != null)
                        {
                            foreach (var item in B.files)
                            {
                                //文件更新状态（是否需要更新）
                                bool FileUpdateStatus = true;

                                string FilePath = $"../{item.FilePath}";
                                if (Core.Init.Mode == Config.Mode.Docker)
                                {
                                    FilePath = FilePath.Replace("bin/", "DDTV/");
                                }

                                if (File.Exists(FilePath))
                                {
                                    string Md5 = GetMD5HashFromFile(FilePath);
                                    if (Md5 == item.FileMd5)
                                    {
                                        FileUpdateStatus = false;
                                    }
                                }

                                if (!item.FilePath.Contains("bin/Update"))
                                {

                                    FileUpdateStatus = false;
                                }

                                if (FileUpdateStatus)
                                {
                                    map.Add($"/{type}/{(Isdev ? "dev" : "release")}/{item.FilePath}", (item.FileName, FilePath, item.Size));
                                }
                            }
                            int i = 1;
                            foreach (var item in map)
                            {
                                long bytes = item.Value.Size;
                                string size = (bytes >= 1 << 30) ? $"{(double)bytes / (1 << 30):F2} GB" : (bytes >= 1 << 20) ? $"{(double)bytes / (1 << 20):F2} MB" : (bytes >= 1 << 10) ? $"{(double)bytes / (1 << 10):F2} KB" : $"{bytes} Bytes";
                                Log.Info(nameof(Update_UpdateProgram), $"进度：{i}/{map.Count}  |  文件大小{size}字节，开始更新文件【{item.Value.Name}】.......");
                                string directoryPath = Path.GetDirectoryName(item.Value.FilePath);
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }
                                bool dl_ok = false;
                                do
                                {
                                    try
                                    {
                                        dl_ok = DownloadFileAsync(item.Key, item.Value.FilePath);
                                    }
                                    catch (Exception)
                                    {

                                    }
                                } while (!dl_ok);
                                Log.Info(nameof(Update_UpdateProgram), $" | 更新文件【{item.Value.Name}】成功");
                                i++;
                            }
                            Log.Info(nameof(Update_UpdateProgram), $"更新完成：更新Update程序完成");
                        }
                        else
                        {
                            Log.Info(nameof(Update_UpdateProgram), $"更新Update程序失败：获取更新列表失败，请检查网络状态");
                        }
                    }
                }
            }
            public static bool checkVersion()
            {
                string FI = "";
                if (File.Exists("./ver.ini"))
                { 
                    FI = "./ver.ini";
                }
                else if (File.Exists(verFile))
                {
                    FI = verFile;
                }
                else
                {
                    Log.Info(nameof(Update_UpdateProgram),"更新失败，没找到版本标识文件");
                    return true;
                }
                string[] Ver = File.ReadAllLines(FI);
                foreach (string VerItem in Ver)
                {
                    if (VerItem.StartsWith("type="))
                        type = VerItem.Split('=')[1].TrimEnd();
                    if (VerItem.StartsWith("ver="))
                        ver = VerItem.Split('=')[1].TrimEnd();
                }
                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(ver))
                {
                    Log.Info(nameof(Update_UpdateProgram),"更新失败，版本标识文件内容错数");
                    return true;
                }
                if (ver.ToLower().StartsWith("dev"))
                {
                    Isdev = true;
                }
                Log.Info(nameof(Update_UpdateProgram),$"当前本地版本{type}-{ver}[{(Isdev ? "dev" : "release")}]");
                Log.Info(nameof(Update_UpdateProgram),"开始获取最新版本号....");
                string DL_VerFileUrl = $"/{type}/{(Isdev ? "dev" : "release")}/ver.ini";
                string R_Ver = Get(DL_VerFileUrl).TrimEnd();
                Log.Info(nameof(Update_UpdateProgram),$"获取到当前服务端版本:{R_Ver}");

                if (!string.IsNullOrEmpty(R_Ver) && R_Ver.Split('.').Length > 0)
                {
                    //老版本
                    Version Before = new Version(ver.Replace("dev", "").Replace("release", ""));
                    //新版本
                    Version After = new Version(R_Ver.Replace("dev", "").Replace("release", ""));
                    if (After > Before)
                    {
                        Log.Info(nameof(Update_UpdateProgram),$"检测到新版本，获取远程文件树开始更新Update程序.......");
                        return true;
                    }
                    else
                    {
                        Log.Info(nameof(Update_UpdateProgram),$"当前Update已是最新版本....");
                        return false;
                    }
                }
                else
                {
                    Log.Info(nameof(Update_UpdateProgram),$"获取新版本失败，请检查网络和代理状况.......");
                    return false;
                }
            }

            public static string Get(string URL)
            {
                bool A = false;
                string str = string.Empty;
                int error_count = 0;
                string FileDownloadAddress = string.Empty;
                do
                {
                    try
                    {
                        if (A)
                            Thread.Sleep(1000);
                        if (!A)
                            A = true;

                        if (error_count > 1)
                        {
                            if (error_count > 3)
                            {
                                Log.Info(nameof(Update_UpdateProgram),$"更新失败，网络错误过多，请检查网络状况或者代理设置后重试.....");
                                return "";
                            }
                            Log.Info(nameof(Update_UpdateProgram),$"使用备用服务器进行重试.....");
                            FileDownloadAddress = AlternativeDomainName + URL;
                            Log.Info(nameof(Update_UpdateProgram),$"从主服务器获取更新失败，尝试从备用服务器获取....");
                        }
                        else
                        {
                            FileDownloadAddress = MainDomainName + URL;
                        }
                        using (HttpClient _httpClient = new HttpClient())
                        {
                            _httpClient.Timeout = new TimeSpan(0, 0, 10);
                            _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                            str = _httpClient.GetStringAsync(FileDownloadAddress).Result;
                            error_count++;
                        }
                    }
                    catch (WebException webex)
                    {
                        error_count++;
                        switch (webex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Info(nameof(Update_UpdateProgram),$"下载文件超时:{FileDownloadAddress}");
                                break;

                            default:
                                Log.Info(nameof(Update_UpdateProgram),$"网络错误，请检查网络状况或者代理设置...开始重试.....");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        error_count++;
                        Log.Info(nameof(Update_UpdateProgram),$"出现网络错误");

                    }
                } while (string.IsNullOrEmpty(str));
                return str;
            }

            public static bool DownloadFileAsync(string url, string outputPath)
            {
                int error_count = 0;
                while (true)
                {
                    string FileDownloadAddress;
                    if (error_count > 2)
                    {
                        if (error_count > 5)
                        {
                            break;
                        }
                        FileDownloadAddress = AlternativeDomainName + url;
                        Console.WriteLine($"从主服务器获取更新失败，尝试从备用服务器获取....");
                    }
                    else
                    {
                        FileDownloadAddress = MainDomainName + url;
                    }
                    try
                    {
                        using (HttpClient _httpClient = new HttpClient())
                        {
                            _httpClient.Timeout = new TimeSpan(0, 0, 10);
                            _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                            using var response = _httpClient.GetAsync(FileDownloadAddress, HttpCompletionOption.ResponseHeadersRead).Result;
                            response.EnsureSuccessStatusCode();
                            using var output = new FileStream(outputPath, FileMode.Create);
                            using var contentStream = response.Content.ReadAsStreamAsync().Result;
                            contentStream.CopyTo(output);
                            return true;
                        }
                    }
                    catch (WebException webex)
                    {
                        error_count++;
                        switch (webex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Info(nameof(Update_UpdateProgram),$"下载文件超时:{FileDownloadAddress}");
                                break;

                            default:
                                Log.Info(nameof(Update_UpdateProgram),$"网络错误，请检查网络状况或者代理设置...开始重试.....");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        error_count++;
                        Log.Info(nameof(Update_UpdateProgram),$"出现网络错误2");

                    }
                    Thread.Sleep(1000);
                }
                return false;
            }

            public static void GetFileList(string FilePath, out List<FileInfo> fileInfos)
            {
                fileInfos = new List<FileInfo>();
                DirectoryInfo root = new DirectoryInfo(FilePath);
                foreach (var item in root.GetDirectories())
                {
                    GetFileList(item.FullName, out List<FileInfo> _T);
                    fileInfos.AddRange(_T);
                }
                foreach (FileInfo item in root.GetFiles())
                {
                    fileInfos.Add(item);
                }

            }
            public class FileInfoClass
            {

                public string Ver { get; set; }
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

            /// <summary>
            /// 获取文件MD5值
            /// </summary>
            /// <param name="fileName">文件绝对路径</param>
            /// <returns>MD5值</returns>
            public static string GetMD5HashFromFile(string fileName)
            {
                try
                {
                    FileStream file = new FileStream(fileName, FileMode.Open);
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    file.Close();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
                catch (Exception ex)
                {
                    return "";
                }
            }

        }



    }
}
