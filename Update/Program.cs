using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using static Update.GetFileSchemaJSON;

namespace Update
{
    public class Program
    {
        public static string MainDomainName = "https://ddtv-update.top";
        public static string AlternativeDomainName = "https://update5.ddtv.pro";
        public static string verFile = "../ver.ini";
        public static string type = string.Empty;
        public static string ver = string.Empty;
        public static string R_ver = string.Empty;
        public static bool Isdev = false;
        public static bool Isdocker = false;
        public static void Main(string[] args)
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
                if (args.Contains("docker"))
                {
                    Isdocker = true;
                }
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DDTV_Docker_Project")))
            {
                Isdocker = true;
            }

            Console.WriteLine("开始更新DDTV");
            DisableConsoleQuickEdit.Go();
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            Console.WriteLine($"当前工作路径:{Environment.CurrentDirectory}");
            Console.WriteLine(Environment.CurrentDirectory);
            Dictionary<string, (string Name, string FilePath, long Size)> map = new Dictionary<string, (string Name, string FilePath, long Size)>();
            HttpClient _httpClient = new HttpClient();
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

                        string FilePath = $"../../{item.FilePath}";
                        if (Isdocker)
                        {
                            FilePath = FilePath.Replace("bin/", "DDTV/");
                        }

                        if (item.FilePath.Contains("bin/Update"))
                        {
                            FileUpdateStatus = false;
                        }
                        else
                        {
                            if (File.Exists(FilePath))
                            {
                                string Md5 = MD5Hash.GetMD5HashFromFile(FilePath);
                                if (Md5 == item.FileMd5)
                                {
                                    FileUpdateStatus = false;
                                }
                            }
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
                        Console.Write($"进度：{i}/{map.Count}  |  文件大小{size}，开始更新文件【{item.Value.Name}】.......");
                        string directoryPath = Path.GetDirectoryName(item.Value.FilePath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        bool dl_ok = false;
                        do
                        {
                            dl_ok = DownloadFileAsync(item.Key, item.Value.FilePath);
                        } while (!dl_ok);
                        Console.WriteLine($" | 更新文件【{item.Value.Name}】成功");
                        i++;
                    }
                    Console.WriteLine($"更新完成");
                    if (OperatingSystem.IsWindows())
                    {
                        if (type.Contains("DDTV-Server"))
                        {
                            Process.Start("../Server.exe");
                            return;
                        }
                        else if (type.Contains("DDTV-Client"))
                        {
                            Process.Start("../Client.exe");
                            return;
                        }
                        else if (type.Contains("DDTV-Desktop"))
                        {
                            Process.Start("../Desktop.exe");
                            return;
                        }
                    }
                    Console.Write($"更新DDTV到{type}-{R_ver}完成，请手动启动DDTV");
                    if (Isdocker)
                        Console.WriteLine($"，按任意键继续");
                }
                else
                {
                    Console.WriteLine($"更新失败：获取更新列表失败，请检查网络状态，按任意键继续");
                }
            }
            else
            {
                Console.WriteLine($"未检测到新版本");
            }
            if (Isdocker)
                return;

            Console.ReadKey();
        }
        public static bool checkVersion()
        {
            if (!File.Exists(verFile))
            {
                Console.WriteLine("更新失败，没找到版本标识文件");
                return false;
            }
            string[] Ver = File.ReadAllLines( verFile);
            foreach (string VerItem in Ver)
            {
                if (VerItem.StartsWith("type="))
                    type = VerItem.Split('=')[1].TrimEnd();
                if (VerItem.StartsWith("ver="))
                    ver = VerItem.Split('=')[1].TrimEnd();
            }
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(ver))
            {
                Console.WriteLine("更新失败，版本标识文件内容错数");
                return false;
            }
            if (ver.ToLower().StartsWith("dev"))
            {
                Isdev = true;
            }
            Console.WriteLine($"当前本地版本{type}-{ver}[{(Isdev ? "dev" : "release")}]");
            Console.WriteLine("开始获取最新版本号....");
            string DL_VerFileUrl = $"/{type}/{(Isdev ? "dev" : "release")}/ver.ini";
            string R_Ver = Get(DL_VerFileUrl).TrimEnd();
            Console.WriteLine($"获取到当前服务端版本:{R_Ver}");

            if (!string.IsNullOrEmpty(R_Ver) && R_Ver.Split('.').Length > 0)
            {
                //老版本
                Version Before = new Version(ver.Replace("dev", "").Replace("release", ""));
                //新版本
                Version After = new Version(R_Ver.Replace("dev", "").Replace("release", ""));
                if (After > Before)
                {
                    Console.WriteLine($"检测到新版本，获取远程文件树开始更新.......");
                    return true;
                }
                else
                {
                    Console.WriteLine($"当前已是最新版本....");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"获取新版本失败，请检查网络和代理状况.......");
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
                        Thread.Sleep(100);
                    if (!A)
                        A = true;

                    if (error_count > 0)
                    {
                        if (error_count > 3)
                        {
                            Console.WriteLine($"更新失败，网络错误过多，请检查网络状况或者代理设置后重试.....");
                            return "";
                        }
                        //Console.WriteLine($"使用备用服务器进行重试.....");
                        FileDownloadAddress = AlternativeDomainName + URL;
                        Console.WriteLine($"从主服务器获取更新失败，尝试从备用服务器获取....");
                    }
                    else
                    {
                        FileDownloadAddress = MainDomainName + URL;
                    }
                    HttpClient _httpClient = new HttpClient();
                    _httpClient.Timeout = new TimeSpan(0, 0, 5);
                    _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                    str = _httpClient.GetStringAsync(FileDownloadAddress).Result;
                    error_count++;
                }
                catch (WebException webex)
                {
                    error_count++;
                    switch (webex.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            Console.WriteLine($"下载文件超时:{FileDownloadAddress}");
                            break;

                        default:
                            Console.WriteLine($"网络错误，请检查网络状况或者代理设置...开始重试.....");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    error_count++;
                    Console.WriteLine($"=下载文件超时:{FileDownloadAddress}\r\n");

                }
            } while (string.IsNullOrEmpty(str));
            Console.WriteLine($"下载成功...");
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
                    HttpClient _httpClient = new HttpClient();
                    _httpClient.Timeout = new TimeSpan(0, 0, 10);
                    _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                    using var response = _httpClient.GetAsync(FileDownloadAddress, HttpCompletionOption.ResponseHeadersRead).Result;
                    response.EnsureSuccessStatusCode();
                    using var output = new FileStream(outputPath, FileMode.Create);
                    using var contentStream = response.Content.ReadAsStreamAsync().Result;
                    contentStream.CopyTo(output);
                    return true;
                }
                catch (WebException webex)
                {
                    error_count++;
                    switch (webex.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            Console.WriteLine($"下载文件超时:{FileDownloadAddress}");
                            break;

                        default:
                            Console.WriteLine($"网络错误，请检查网络状况或者代理设置...开始重试.....");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    error_count++;
                    Console.WriteLine($"出现网络错误，错误详情：{ex.ToString()}\r\n\r\n===========执行重试，如果没同一个文件重复提示错误，则表示重试成功==============\r\n");

                }
                Thread.Sleep(1000);
            }
            return false;
        }
    }
}
