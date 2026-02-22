using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using static Update.GetFileSchemaJSON;

namespace Update
{
    public class Program
    {
        public static string MainDomainName = "https://dl.ddtv-update.top";
        public static string AlternativeDomainName = "https://update5.ddtv.pro";
        public static string verFile = "../ver.ini";
        public static string type = string.Empty;
        public static string ver = string.Empty;
        public static string R_ver = string.Empty;
        public static bool Isdev = false;
        public static bool Isdocker = false;

        public static bool RemoteFailure = false;


        //仅拥有OSS目标桶读取权限的AK信息
        private static string endpoint = "https://oss-cn-shanghai.aliyuncs.com";
        public static string Bucket = "ddtv5-update";
        private static AmazonS3Client ossClient = null;

        private static readonly object consoleLock = new object();// 控制台并发访问用的
        private static int completedFiles = 0; // 已成功下载并写入磁盘的文件计数（用InterLocked递增）
        private const int 最大并发下载数 = 10;
        private static DownloadProgress[] downloadProgressList = new DownloadProgress[最大并发下载数];// 按槽位索引存储每个并发槽的当前进度信息
        private static int progressStartLine = 0;// 控制台中进度显示区域的起始行（基于 Console.CursorTop）

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

            Console.WriteLine("开始更新DDTV，请勿点击本程序任何位置，防止进入'快捷编辑模式'阻塞更新，如果已点击，请点击回车回复");
            DisableConsoleQuickEdit.Go();
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            Console.WriteLine($"当前工作路径:{Environment.CurrentDirectory}");
            Console.WriteLine(Environment.CurrentDirectory);
            
            if (checkVersion())
            {
                var fileList = GetUpdateFileList();
                if (fileList != null && fileList.Count > 0)
                {
                    Console.WriteLine($"共需要更新 {fileList.Count} 个文件，开始并发下载（最大并发数：{最大并发下载数}）...");
                    DownloadFilesAsync(fileList).Wait();
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
                    Console.Write($"更新DDTV到{type}-{R_ver}完成，自动启动DDTV失败，请手动启动");
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

        /// <summary>
        /// 获取远程文件列表并生成需要更新的文件信息列表。返回的 List 中每一项包含下载 URL、本地文件路径、文件名和大小。
        /// </summary>
        /// <returns></returns>
        public static List<FileDownloadInfo> GetUpdateFileList()
        {
            try
            {
                string DL_FileListUrl = $"/{type}/{(Isdev ? "dev" : "release")}/{type}_Update.json";
                string web = Get(DL_FileListUrl);
                var B = JsonConvert.DeserializeObject<FileInfoClass>(web);
                
                if (B == null) return null;
                var updateList = new List<FileDownloadInfo>();
                
                foreach (var item in B.files)
                {
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
                        string downloadUrl = $"/{type}/{(Isdev ? "dev" : "release")}/{item.FilePath}";
                        updateList.Add(new FileDownloadInfo
                        {Url = downloadUrl,FileName = item.FileName,FilePath = FilePath,Size = item.Size});
                    }
                }

                return updateList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取文件列表失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 并发下载文件列表。首轮每个文件最多重试 3 次，首轮完成后会对失败文件进行一次统一重试。
        /// 显示实时进度，最大并发由 `最大并发下载数` 控制。
        /// </summary>
        /// <param name="fileList">待下载的文件信息列表（每项包含 URL、本地路径、文件名、大小）。</param>
        /// <param name="currentDepth">当前递归深度/重试轮次，从 0 开始。用于限制最大重试次数。</param>
        /// <param name="originalTotalFiles">首次调用时的总文件数，用于最终统计。传入 -1 表示未初始化，方法在第一次进入时将其设为当前 `fileList.Count`。</param>
        public static async Task DownloadFilesAsync(List<FileDownloadInfo> fileList, int currentDepth = 0, int originalTotalFiles = -1)
        {
            completedFiles = 0;
            int totalFiles = fileList.Count;
            if (originalTotalFiles == -1)
                originalTotalFiles = totalFiles;
            var semaphore = new SemaphoreSlim(最大并发下载数);
            var tasks = new List<Task<bool>>(fileList.Count);
            var failedFiles = new List<FileDownloadInfo>();
            int taskId = 0;

            Console.WriteLine();
            progressStartLine = Console.CursorTop;
            
            for (int i = 0; i < 最大并发下载数; i++)
            {
                Console.WriteLine();
            }

            for (int idx = 0; idx < fileList.Count; idx++)
            {
                var item = fileList[idx];
                await semaphore.WaitAsync();
                int currentTaskId = taskId++;
                int sequenceNumber = idx + 1;
                // 为每个下载任务创建一个独立的 Task，负责下载文件并更新进度
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var progress = new DownloadProgress
                        {
                            文件名 = item.FileName,
                            总大小 = item.Size,
                            任务ID = currentTaskId % 最大并发下载数,
                            Sequence = sequenceNumber
                        };

                        lock (consoleLock)
                        {
                            downloadProgressList[progress.任务ID] = progress;
                        }

                        string directoryPath = Path.GetDirectoryName(item.FilePath);
                        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        bool dl_ok = false;
                        int timeout = 10;
                        int retryCount = 0;
                        const int maxRetries = 3;

                        while (!dl_ok && retryCount < maxRetries)
                        {
                            try
                            {
                                dl_ok = await DownloadFileWithProgressAsync(item.Url, item.FilePath, timeout, progress);
                            }
                            catch (Exception ex)
                            {
                                dl_ok = false;
                                lock (consoleLock)
                                {
                                    progress.Status = $"错误: {ex.Message.Substring(0, Math.Min(30, ex.Message.Length))}";
                                    UpdateProgressDisplay();
                                }
                            }

                            if (!dl_ok)
                            {
                                retryCount++;
                                if (retryCount < maxRetries)
                                {
                                    timeout = Math.Min(timeout * 2, 60);
                                    await Task.Delay(1000);
                                }
                            }
                        }

                        lock (consoleLock)// 更新下载结果和进度显示
                        {
                            if (dl_ok)
                            {
                                Interlocked.Increment(ref completedFiles);
                                progress.Status = "完成";
                                progress.下载百分比 = 100;
                            }
                            else
                            {
                                progress.Status = "失败,稍后重试";
                                failedFiles.Add(item);
                            }
                            UpdateProgressDisplay();
                        }

                        await Task.Delay(500);
                        return dl_ok;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            var updateTask = Task.Run(async () =>
            {
                while (completedFiles < totalFiles && tasks.Any(t => !t.IsCompleted))
                {
                    await Task.Delay(200);
                    lock (consoleLock)
                    {
                        UpdateProgressDisplay();
                    }
                }
            });

            await Task.WhenAll(tasks);
            await updateTask;

            Console.SetCursorPosition(0, progressStartLine + 最大并发下载数);
            Console.WriteLine($"\n首轮下载完成！成功: {completedFiles}/{totalFiles}");

            if (failedFiles.Count > 0)
            {
                // 如果未达到最大递归深度，则对失败文件递归调用本方法进行重试
                const int maxDepth = 2;
                if (currentDepth < maxDepth)
                {
                    Console.SetCursorPosition(0, progressStartLine + 最大并发下载数);
                    Console.WriteLine($"\n第 {currentDepth + 1} 轮重试 {failedFiles.Count} 个失败文件...");
                    await Task.Delay(2000);

                    // 清理进度显示数据，准备新的重试轮次显示
                    Array.Clear(downloadProgressList, 0, downloadProgressList.Length);

                    await DownloadFilesAsync(failedFiles, currentDepth + 1, originalTotalFiles);
                }
                else
                {
                    Console.SetCursorPosition(0, progressStartLine + 最大并发下载数);
                    Console.WriteLine($"\n最终结果：成功 {completedFiles}/{originalTotalFiles}，失败 {originalTotalFiles - completedFiles} 个文件");
                }
            }
        }

        /// <summary>
        /// 更新下载进度信息显示
        /// </summary>
        private static void UpdateProgressDisplay()
        {
            for (int i = 0; i < 最大并发下载数; i++)
            {
                Console.SetCursorPosition(0, progressStartLine + i);
                
                    if (downloadProgressList[i] != null)
                {
                    var progress = downloadProgressList[i];
                    string bar = GenerateProgressBar(progress.下载百分比, 30);
                    string speed = FormatBytes(progress.速度, true);
                    string downloaded = FormatBytes(progress.已下载大小, false);
                    string total = FormatBytes(progress.总大小, false);
                    string status = progress.Status ?? "下载中";
                    int seq = progress.Sequence > 0 ? progress.Sequence : i + 1;
                    
                    string line = $"[{seq}] {bar} {progress.下载百分比:F1}% | {downloaded}/{total} | {speed} | {status} | {TruncateString(progress.文件名)}";
                    Console.Write(line.PadRight(Console.WindowWidth - 1));
                }
                else  Console.Write("".PadRight(Console.WindowWidth - 1));
            }
        }

        /// <summary>
        /// 根据宽度和文件下载百分比生成进度条文本
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="width"></param>
        private static string GenerateProgressBar(double percentage, int width)
        {
            int filled = (int)(percentage / 100.0 * width);
            return new string('█', filled) + new string(' ', width - filled);
        }

        /// <summary>
        /// 字符数转可读格式
        /// 当 <paramref name="isSpeed"/> 为 true 时，会在单位后追加 "/s" 表示速度，否则表示大小
        /// </summary>
        /// <param name="bytes">字节数。</param>
        /// <param name="isSpeed">是否以速度单位格式化（true 则返回速度单位）。</param>
        private static string FormatBytes(long bytes, bool isSpeed)
        {
            if (bytes <= 0) return "0 B" + (isSpeed ? "/s" : "");
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double v = bytes;
            int idx = 0;
            while (v >= 1024 && idx < units.Length - 1) { v /= 1024; idx++; }
            var suf = isSpeed ? "/s" : "";
            return idx == 0 ? $"{(long)v} {units[idx]}{suf}" : $"{v:F2} {units[idx]}{suf}";
        }

        /// <summary>
        /// 超过最大长度时截断字符串并添加省略号
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        private static string TruncateString(string str, int maxLength=20)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Length <= maxLength ? str : str.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// 下载文件并实时更新下载进度信息。根据 `RemoteFailure` 标志决定使用主服务器还是备用服务器进行下载，下载失败时会自动重试，重试次数过多时会切换服务器。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="outputPath"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="progress"></param>
        public static async Task<bool> DownloadFileWithProgressAsync(string url, string outputPath, long timeoutSeconds, DownloadProgress progress)
        {
            int error_count = 0;
            while (true)
            {
                if (error_count > 5) break;

                if (RemoteFailure || error_count > 2)
                {
                    if (!RemoteFailure)
                    {
                        lock (consoleLock)
                        {
                            progress.Status = "切换备用服务器";
                            UpdateProgressDisplay();
                        }
                        RemoteFailure = true;
                    }
                    try
                    {
                        var config = new AmazonS3Config() { ServiceURL = endpoint, MaxErrorRetry = 2, Timeout = TimeSpan.FromSeconds(20 + timeoutSeconds) };
                        var ossClient = new AmazonS3Client(AKID, AKSecret, config);
                        string FileKey = url.Substring(1, url.Length - 1);
                        using var response = await ossClient.GetObjectAsync(Bucket, FileKey);
                        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
                        await CopyStreamWithProgressAsync(response.ResponseStream, fileStream, progress);
                        return true;
                    }
                    catch
                    {
                        error_count++;
                        lock (consoleLock)
                        {
                            progress.Status = $"重试中({error_count})";
                            UpdateProgressDisplay();
                        }
                    }
                }
                else
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        httpClient.Timeout = TimeSpan.FromSeconds(10 + timeoutSeconds);
                        httpClient.DefaultRequestHeaders.Referrer = new Uri("https://update5.ddtv.pro");
                        using var response = await httpClient.GetAsync(MainDomainName + url, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();
                        using var contentStream = await response.Content.ReadAsStreamAsync();
                        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
                        await CopyStreamWithProgressAsync(contentStream, fileStream, progress);
                        return true;
                    }
                    catch
                    {
                        error_count++;
                        lock (consoleLock)
                        {
                            progress.Status = $"重试中({error_count})";
                            UpdateProgressDisplay();
                        }
                    }
                }

                await Task.Delay(250);
            }
            return false;
        }

        /// <summary>
        /// 复制数据流的同时更新下载进度信息。每隔 500ms 更新一次速度和下载百分比
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="progress"></param>
        private static async Task CopyStreamWithProgressAsync(Stream source, FileStream dest, DownloadProgress progress)
        {
            var buffer = new byte[81920];
            int bytesRead;
            var stopwatch = Stopwatch.StartNew();
            long lastBytes = 0;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await dest.WriteAsync(buffer, 0, bytesRead);
                Interlocked.Add(ref progress.已下载大小, bytesRead);

                if (stopwatch.ElapsedMilliseconds > 500)
                {
                    long currentBytes = progress.已下载大小;
                    progress.速度 = (long)((currentBytes - lastBytes) / (stopwatch.ElapsedMilliseconds / 1000.0));
                    progress.下载百分比 = (double)progress.已下载大小 / progress.总大小 * 100;
                    lastBytes = currentBytes;
                    stopwatch.Restart();
                }
            }
        }
        public static bool checkVersion()
        {
            if (!File.Exists(verFile))
            {
                Console.WriteLine("更新失败，没找到版本标识文件");
                return false;
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
                Console.WriteLine("更新失败，版本标识文件内容错误");
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

                        string FileKey = URL.Substring(1, URL.Length - 1);

                        var config = new AmazonS3Config() { ServiceURL = endpoint, MaxErrorRetry = 2, Timeout = TimeSpan.FromSeconds(20) };
                        var ossClient = new AmazonS3Client(AKID, AKSecret, config);
                        using GetObjectResponse response = ossClient.GetObjectAsync(Bucket, FileKey).Result;
                        using StreamReader reader = new StreamReader(response.ResponseStream);
                        str = reader.ReadToEndAsync().Result;
                        error_count++;
                    }
                    else
                    {
                        FileDownloadAddress = MainDomainName + URL;
                        HttpClient _httpClient = new HttpClient();
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

        //更新桶的只读ak
        public static string AKID
        {
            get
            {
                string t = string.Empty;
                t += (char)76;  // 'L'
                t += (char)84;  // 'T'
                t += (char)65;  // 'A'
                t += (char)73;  // 'I'
                t += (char)53;  // '5'
                t += (char)116; // 't'
                t += (char)72;  // 'H'
                t += (char)99;  // 'c'
                t += (char)53;  // '5'
                t += (char)113; // 'q'
                t += (char)107; // 'k'
                t += (char)68;  // 'D'
                t += (char)98;  // 'b'
                t += (char)86;  // 'V'
                t += (char)74;  // 'J'
                t += (char)107; // 'k'
                t += (char)70;  // 'F'
                t += (char)113; // 'q'
                t += (char)69;  // 'E'
                t += (char)98;  // 'b'
                t += (char)120; // 'x'
                t += (char)105; // 'i'
                t += (char)114; // 'r'
                t += (char)72;  // 'H'
                return t;
            }
        }
        public static string AKSecret
        {
            get
            {
                string t = string.Empty;
                t += (char)115; // 's'
                t += (char)54;  // '6'
                t += (char)121; // 'y'
                t += (char)73;  // 'I'
                t += (char)97;  // 'a'
                t += (char)114; // 'r'
                t += (char)65;  // 'A'
                t += (char)86;  // 'V'
                t += (char)75;  // 'K'
                t += (char)108; // 'l'
                t += (char)97;  // 'a'
                t += (char)108; // 'l'
                t += (char)102; // 'f'
                t += (char)68;  // 'D'
                t += (char)78;  // 'N'
                t += (char)57;  // '9'
                t += (char)118; // 'v'
                t += (char)117; // 'u'
                t += (char)84;  // 'T'
                t += (char)111; // 'o'
                t += (char)56;  // '8'
                t += (char)82;  // 'R'
                t += (char)120; // 'x'
                t += (char)90;  // 'Z'
                t += (char)48;  // '0'
                t += (char)101; // 'e'
                t += (char)110; // 'n'
                t += (char)107; // 'k'
                t += (char)106; // 'j'
                t += (char)81;  // 'Q'
                return t;
            }
        }
    }
    /// <summary>
    /// 下载进度信息类（进入下载队列后）
    /// </summary>
    public class DownloadProgress
    {
        public int 任务ID { get; set; }
        public string 文件名 { get; set; }
        public long 总大小 { get; set; }
        public long 已下载大小;
        public double 下载百分比 { get; set; }
        public long 速度 { get; set; }
        public string Status { get; set; }
        public int Sequence { get; set; }
    }
    /// <summary>
    /// 待下载文件的信息（来自远程文件清单）
    /// </summary>
    public class FileDownloadInfo
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long Size { get; set; }
    }
}
