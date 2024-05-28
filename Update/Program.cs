using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using static Update.GetFileSchemaJSON;

namespace Update
{
    internal class Program
    {
        public static string Url = "https://ddtv-update.top";
        public static string verFile = "../ver.ini";
        public static string type = string.Empty;
        public static string ver = string.Empty;
        public static string R_ver = string.Empty;
        public static bool Isdev = false;
        public static HttpClient _httpClient = new HttpClient();
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            Dictionary<string, (string Name, string FilePath, long Size)> map = new Dictionary<string, (string Name, string FilePath, long Size)>();
            _httpClient.Timeout = new TimeSpan(0, 0, 8);
            if (checkVersion())
            {
                string DL_FileListUrl = $"{Url}/{type}/{(Isdev ? "dev" : "release")}/{type}_Update.json";
                string web = Get(DL_FileListUrl);
                var B = JsonConvert.DeserializeObject<FileInfoClass>(web);
                if (B != null)
                {
                    foreach (var item in B.files)
                    {
                        bool IsD = true;
                        string FilePath = $"../../{item.FilePath}";
                        if (File.Exists(FilePath))
                        {
                            string Md5 = MD5Hash.GetMD5HashFromFile(FilePath);
                            if (Md5 == item.FileMd5)
                            {
                                IsD = false;
                            }
                        }
                        if (IsD)
                        {
                            map.Add($"{Url}/{type}/{(Isdev ? "dev" : "release")}/{item.FilePath}", (item.FileName, FilePath, item.Size));
                        }
                    }
                    int i = 1;
                    foreach (var item in map)
                    {
                        Console.WriteLine($"进度：{i}/{map.Count}  |  文件大小{item.Value.Size}字节，开始更新文件【{item.Value.Name}】");
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
                        Console.WriteLine($"进度：{i}/{map.Count}  |  更新文件【{item.Value.Name}】成功");
                        i++;
                    }
                    Console.WriteLine($"更新完成：更新DDTV到{type}-{R_ver}成功");
                }
                else
                {
                    Console.WriteLine($"更新失败：获取更新列表失败，请检查网络状态");
                }
            }
            while (true)
            {
                Console.ReadKey();
            }
        }
        public static bool checkVersion()
        {
            if (!File.Exists(verFile))
            {
                Console.WriteLine("更新失败，没找到版本标识文件");
                return true;
            }
            string[] Ver = File.ReadAllLines(verFile);
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
                return true;
            }
            if (ver.ToLower().StartsWith("dev"))
            {
                Isdev = true;
            }

            string DL_VerFileUrl = $"{Url}/{type}/{(Isdev ? "dev" : "release")}/ver.ini";
            string R_Ver = Get(DL_VerFileUrl).TrimEnd();
            R_ver = R_Ver;
            if (R_Ver != ver)
                return true;
            else
                return false;
        }

        public static string Get(string URL)
        {
            bool A = false;
            string str = string.Empty;
            do
            {
                if (A)
                    Thread.Sleep(1000);
                if (!A)
                    A = true;

                _httpClient.DefaultRequestHeaders.Referrer = new Uri("http://ddtv-update");
                str = _httpClient.GetStringAsync(URL).Result;
            } while (string.IsNullOrEmpty(str));
            return str;
        }

        public static bool DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                using var response = _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                response.EnsureSuccessStatusCode();
                using var output = new FileStream(outputPath, FileMode.Create);
                using var contentStream = response.Content.ReadAsStreamAsync().Result;
                contentStream.CopyTo(output);
                return true;
            }
            catch (WebException webex)
            {
                switch (webex.Status)
                {
                    case WebExceptionStatus.Timeout:
                        Console.WriteLine($"下载文件超时:{url}");
                        break;

                    default:
                        Console.WriteLine($"网络错误，请检查网络状况或者代理设置...开始重试.....");
                        break;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"出现网络错误，错误详情：{ex.ToString()}\r\n\r\n===========执行重试，如果没同一个文件重复提示错误，则表示重试成功==============\r\n");
                return false;
            }
        }
    }
}
