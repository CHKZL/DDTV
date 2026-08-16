using Amazon.S3;
using Amazon.S3.Model;
using Downloader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        // 备份目录（相对于程序目录）
        private static readonly string BackupDir = "../.update_backup";

        public static async Task Main(string[] args)
        {
            ParseArgs(args);
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DDTV_Docker_Project")))
            {
                Isdocker = true;
            }

            Console.WriteLine("开始更新DDTV，请勿点击本程序任何位置，防止进入'快捷编辑模式'阻塞更新，如果已点击，请点击回车恢复");
            DisableConsoleQuickEdit.Go();
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;//将当前路径从 引用路径 修改至 程序所在目录
            Console.WriteLine($"当前工作路径:{Environment.CurrentDirectory}");

            if (!checkVersion())
            {
                Console.WriteLine($"未检测到新版本");
                if (Isdocker)
                    return;
                Console.ReadKey();
                return;
            }

            string DL_FileListUrl = $"/{type}/{(Isdev ? "dev" : "release")}/{type}_Update.json";
            string web = Get(DL_FileListUrl);
            var B = JsonConvert.DeserializeObject<FileInfoClass>(web);
            if (B != null)
            {
                var updateList = new List<UpdateItem>();
                foreach (var item in B.files)
                {
                    string FilePath = $"../../{item.FilePath}";
                    if (Isdocker)
                    {
                        FilePath = FilePath.Replace("bin/", "DDTV/");
                    }

                    if (item.FilePath.Contains("bin/Update"))
                    {
                        continue;
                    }

                    bool needUpdate = true;
                    if (File.Exists(FilePath))
                    {
                        string Md5 = MD5Hash.GetMD5HashFromFile(FilePath);
                        if (Md5 == item.FileMd5)
                        {
                            needUpdate = false;
                        }
                    }

                    if (needUpdate)
                    {
                        updateList.Add(new UpdateItem
                        {
                            Url = $"/{type}/{(Isdev ? "dev" : "release")}/{item.FilePath}",
                            FileName = item.FileName,
                            FilePath = FilePath,
                            Size = item.Size,
                            Md5 = item.FileMd5
                        });
                    }
                }

                if (updateList.Count == 0)
                {
                    Console.WriteLine("所有文件已是最新版本");
                    UpdateVerIni();
                    StartMainProgram();
                    if (!Isdocker)
                    {
                        Console.WriteLine("按任意键退出...");
                        Console.ReadKey();
                    }
                    return;
                }

                Console.WriteLine($"共需更新 {updateList.Count} 个文件，总大小 {FormatBytes(updateList.Sum(x => x.Size))}");
                Console.WriteLine("按任意键开始下载...");
                if (!Isdocker)
                {
                    Console.ReadKey(true);
                }
                Console.Clear();

                // 备份阶段
                var backupMap = new Dictionary<string, string>();
                var newFiles = new List<string>();
                if (!PrepareBackup(updateList, backupMap, newFiles))
                {
                    Console.WriteLine("[!] 备份文件失败，更新终止");
                    Environment.Exit(1);
                    return;
                }

                // 读取配置
                var (parallelCount, chunkCount) = LoadConfig();

                // Downloader 5.5.0 配置
                var downloadOpt = new DownloadConfiguration()
                {
                    ChunkCount = chunkCount,
                    ParallelDownload = true,
                    ParallelCount = chunkCount,
                    MaxTryAgainOnFailure = 2,
                    BlockTimeout = 5000,
                    HttpClientTimeout = 30000,
                    RequestConfiguration = new RequestConfiguration()
                    {
                        Referer = "https://update5.ddtv.pro",
                        Headers = new System.Net.WebHeaderCollection(),
                        KeepAlive = true,
                        ProtocolVersion = HttpVersion.Version11,
                    }
                };

                // 进度渲染
                var renderer = new ConsoleProgressRenderer(parallelCount);
                long totalBytes = updateList.Sum(x => x.Size);
                renderer.Initialize(updateList.Count, totalBytes, type, Isdev ? "dev" : "release", ver, R_ver);

                // 并发下载
                var semaphore = new SemaphoreSlim(parallelCount);
                var cts = new CancellationTokenSource();
                var tasks = new List<Task<bool>>();
                var slotLock = new object();
                var slotOccupied = new bool[parallelCount];
                int completedCount = 0;
                long completedBytes = 0;

                foreach (var item in updateList)
                {
                    var localItem = item;
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await semaphore.WaitAsync(cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            return false;
                        }
                        int assignedSlot = -1;
                        try
                        {
                            if (cts.IsCancellationRequested) return false;

                            // 分配槽位
                            lock (slotLock)
                            {
                                for (int i = 0; i < parallelCount; i++)
                                {
                                    if (!slotOccupied[i])
                                    {
                                        assignedSlot = i;
                                        slotOccupied[i] = true;
                                        break;
                                    }
                                }
                                if (assignedSlot < 0) assignedSlot = 0;
                            }

                            renderer.SetSlotFile(assignedSlot, localItem.FileName, localItem.Size);

                            string? directoryPath = Path.GetDirectoryName(localItem.FilePath);
                            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                                Directory.CreateDirectory(directoryPath);

                            bool ok = await DownloadFileWithFallbackAsync(
                                localItem.Url,
                                localItem.FilePath,
                                localItem.Md5,
                                downloadOpt,
                                renderer,
                                assignedSlot,
                                cts.Token);

                            // MD5 校验
                            if (ok)
                            {
                                string? md5 = null;
                                try
                                {
                                    md5 = MD5Hash.GetMD5HashFromFile(localItem.FilePath);
                                }
                                catch { }

                                if (md5 != localItem.Md5)
                                {
                                    Console.WriteLine($"[{localItem.FileName}] MD5校验失败，尝试重新下载...");
                                    try { File.Delete(localItem.FilePath); } catch { }
                                    ok = await DownloadFileWithFallbackAsync(
                                        localItem.Url,
                                        localItem.FilePath,
                                        localItem.Md5,
                                        downloadOpt,
                                        renderer,
                                        assignedSlot,
                                        cts.Token);

                                    if (ok)
                                    {
                                        md5 = null;
                                        try { md5 = MD5Hash.GetMD5HashFromFile(localItem.FilePath); } catch { }
                                        ok = md5 == localItem.Md5;
                                    }
                                }
                            }

                            if (!ok && !cts.IsCancellationRequested)
                            {
                                cts.Cancel();
                            }

                            if (ok)
                            {
                                Interlocked.Increment(ref completedCount);
                                Interlocked.Add(ref completedBytes, localItem.Size);
                                renderer.SetSlotCompleted(assignedSlot, true);
                                renderer.SetOverallProgress(completedCount, completedBytes);
                            }
                            else
                            {
                                renderer.SetSlotCompleted(assignedSlot, false, cts.IsCancellationRequested ? "CANCELED" : "FAILED  ");
                            }

                            return ok;
                        }
                        finally
                        {
                            if (assignedSlot >= 0)
                            {
                                lock (slotLock)
                                {
                                    slotOccupied[assignedSlot] = false;
                                }
                            }
                            semaphore.Release();
                        }
                    }));
                }

                bool[] results = await Task.WhenAll(tasks);
                bool allOk = results.All(x => x);

                renderer.Finish();

                if (!allOk)
                {
                    await RollbackAsync(backupMap, newFiles, updateList);
                    Console.WriteLine("[!] 更新失败，所有更改已回滚");
                    Console.WriteLine("请检查网络状态或代理设置后重试");
                    if (!Isdocker)
                    {
                        Console.WriteLine("按任意键退出...");
                        Console.ReadKey();
                    }
                    Environment.Exit(1);
                    return;
                }

                // 清理备份
                try
                {
                    if (Directory.Exists(BackupDir))
                        Directory.Delete(BackupDir, true);
                }
                catch { }

                UpdateVerIni();

                Console.WriteLine($"更新DDTV到{type}-{R_ver}完成");
                StartMainProgram();

                if (!Isdocker)
                {
                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine($"更新失败：获取更新列表失败，请检查网络状态，按任意键继续");
                if (!Isdocker)
                    Console.ReadKey();
            }
        }

        private static void ParseArgs(string[] args)
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
        }

        private static (int parallelCount, int chunkCount) LoadConfig()
        {
            string configPath = "./config.ini";
            int parallelCount = 8;
            int chunkCount = 2;
            try
            {
                if (File.Exists(configPath))
                {
                    foreach (var line in File.ReadAllLines(configPath))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("parallel_count="))
                        {
                            if (int.TryParse(trimmed.Split('=')[1].Trim(), out int pc))
                                parallelCount = pc;
                        }
                        else if (trimmed.StartsWith("chunk_count="))
                        {
                            if (int.TryParse(trimmed.Split('=')[1].Trim(), out int cc))
                                chunkCount = cc;
                        }
                    }
                }
            }
            catch { }
            parallelCount = Math.Clamp(parallelCount, 1, 16);
            chunkCount = Math.Clamp(chunkCount, 1, 16);
            return (parallelCount, chunkCount);
        }

        /// <summary>
        /// 从版本字符串(可能带dev/release/test等前缀)中提取版本号，提取失败返回null
        /// </summary>
        private static Version? ParseVersion(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;
            Match m = Regex.Match(raw, @"\d+\.\d+(\.\d+){0,2}");
            return m.Success ? new Version(m.Value) : null;
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
                Console.WriteLine("更新失败，版本标识文件内容错数");
                return false;
            }
            if (ver.ToLower().StartsWith("dev") || ver.ToLower().StartsWith("test"))
            {
                Isdev = true;
            }
            Console.WriteLine($"当前本地版本{type}-{ver}[{(Isdev ? "dev" : "release")}]");
            Console.WriteLine("开始获取最新版本号....");
            string DL_VerFileUrl = $"/{type}/{(Isdev ? "dev" : "release")}/ver.ini";
            string remoteVer = Get(DL_VerFileUrl).TrimEnd();
            R_ver = remoteVer;
            Console.WriteLine($"获取到当前服务端版本:{R_ver}");

            Version Before = ParseVersion(ver);
            Version After = ParseVersion(R_ver);
            if (Before != null && After != null)
            {
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
            bool firstAttempt = true;
            string str = string.Empty;
            int error_count = 0;
            string FileDownloadAddress = string.Empty;
            do
            {
                try
                {
                    if (!firstAttempt)
                        Thread.Sleep(100);
                    firstAttempt = false;

                    if (error_count > 0)
                    {
                        if (error_count > 3)
                        {
                            Console.WriteLine($"更新失败，网络错误过多，请检查网络状况或者代理设置后重试.....");
                            return "";
                        }
                        Console.WriteLine($"从主服务器获取更新失败，尝试从备用服务器获取....");
                        FileDownloadAddress = AlternativeDomainName + URL;

                        string FileKey = URL.Substring(1, URL.Length - 1);

                        var config = new AmazonS3Config() { ServiceURL = endpoint, MaxErrorRetry = 2, Timeout = TimeSpan.FromSeconds(20) };
                        var ossClient = new AmazonS3Client(AKID, AKSecret, config);
                        using var response = ossClient.GetObjectAsync(Bucket, FileKey).Result;
                        using var reader = new StreamReader(response.ResponseStream);
                        str = reader.ReadToEndAsync().Result;
                        error_count++;
                    }
                    else
                    {
                        FileDownloadAddress = MainDomainName + URL;
                        using var _httpClient = new HttpClient();
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
                catch (Exception)
                {
                    error_count++;
                    Console.WriteLine($"下载文件超时/错误:{FileDownloadAddress}");
                }
            } while (string.IsNullOrEmpty(str) && error_count <= 3);
            return str;
        }

        private static async Task<bool> DownloadFileWithFallbackAsync(
            string url,
            string outputPath,
            string expectedMd5,
            DownloadConfiguration baseConfig,
            ConsoleProgressRenderer renderer,
            int slot,
            CancellationToken ct)
        {
            // L1: 主服务器
            if (!RemoteFailure)
            {
                string address = MainDomainName + url;
                // L1: 主服务器
                if (await DownloadWithDownloaderAsync(address, outputPath, baseConfig, renderer, slot, ct))
                    return true;
                RemoteFailure = true;
            }

            // L2: 备用服务器
            string altAddress = AlternativeDomainName + url;
            // 主服务器失败，尝试备用服务器
            if (await DownloadWithDownloaderAsync(altAddress, outputPath, baseConfig, renderer, slot, ct))
                return true;

            // L3: 阿里云OSS
            // 备用服务器失败，尝试阿里云OSS
            if (await DownloadFromOssAsync(url, outputPath, 10, renderer, slot, ct))
                return true;

            // 所有下载源均失败
            return false;
        }

        private static async Task<bool> DownloadWithDownloaderAsync(
            string url,
            string outputPath,
            DownloadConfiguration config,
            ConsoleProgressRenderer renderer,
            int slot,
            CancellationToken ct)
        {
            try
            {
                var downloader = new DownloadService(config);
                downloader.DownloadProgressChanged += (s, e) =>
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { downloader.CancelAsync(); } catch { }
                        return;
                    }
                    double percent = e.ProgressPercentage;
                    long received = e.ReceivedBytesSize;
                    double speed = e.BytesPerSecondSpeed;
                    renderer.UpdateSlotProgress(slot, percent, received, speed);
                };

                await downloader.DownloadFileTaskAsync(url, outputPath);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception)
            {
                // 下载失败
                return false;
            }
        }

        private static async Task<bool> DownloadFromOssAsync(
            string url,
            string outputPath,
            long timeoutSeconds,
            ConsoleProgressRenderer renderer,
            int slot,
            CancellationToken ct)
        {
            try
            {
                var config = new AmazonS3Config()
                {
                    ServiceURL = endpoint,
                    MaxErrorRetry = 2,
                    Timeout = TimeSpan.FromSeconds(20 + timeoutSeconds)
                };
                var ossClient = new AmazonS3Client(AKID, AKSecret, config);
                string FileKey = url.Substring(1, url.Length - 1);

                using var response = await ossClient.GetObjectAsync(Bucket, FileKey, ct);

                string tempPath = outputPath + ".oss_download";
                await response.WriteResponseStreamToFileAsync(tempPath, false, ct);

                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                File.Move(tempPath, outputPath);

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception)
            {
                // OSS下载失败已处理
                return false;
            }
        }

        private static bool PrepareBackup(List<UpdateItem> items, Dictionary<string, string> backupMap, List<string> newFiles)
        {
            try
            {
                if (Directory.Exists(BackupDir))
                    Directory.Delete(BackupDir, true);
                Directory.CreateDirectory(BackupDir);

                string appRoot = Path.GetFullPath("../../");

                foreach (var item in items)
                {
                    if (File.Exists(item.FilePath))
                    {
                        string targetAbs = Path.GetFullPath(item.FilePath);
                        string relPath = Path.GetRelativePath(appRoot, targetAbs);
                        string backupPath = Path.Combine(Path.GetFullPath(BackupDir), relPath);

                        string? dir = Path.GetDirectoryName(backupPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        File.Copy(item.FilePath, backupPath, true);
                        backupMap[item.FilePath] = backupPath;
                    }
                    else
                    {
                        newFiles.Add(item.FilePath);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"备份失败: {ex.Message}");
                return false;
            }
        }

        private static async Task RollbackAsync(Dictionary<string, string> backupMap, List<string> newFiles, List<UpdateItem> updateItems)
        {
            Console.WriteLine("\n[!] 更新失败，开始回滚更改...");

            // 1. 恢复备份
            foreach (var kv in backupMap)
            {
                try
                {
                    string? dir = Path.GetDirectoryName(kv.Key);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.Copy(kv.Value, kv.Key, overwrite: true);
                    Console.WriteLine($"  [回滚] 恢复: {Path.GetFileName(kv.Key)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [回滚警告] 恢复失败 {Path.GetFileName(kv.Key)}: {ex.Message}");
                }
            }

            // 2. 删除新增文件
            foreach (var file in newFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        Console.WriteLine($"  [回滚] 删除新增: {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [回滚警告] 删除失败 {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            // 3. 清理 .download 临时文件（精确清理本次更新的文件）
            try
            {
                foreach (var item in updateItems)
                {
                    string tempFile = item.FilePath + ".download";
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }
            }
            catch { }

            // 4. 删除备份目录
            try
            {
                if (Directory.Exists(BackupDir))
                    Directory.Delete(BackupDir, true);
            }
            catch { }

            Console.WriteLine("[!] 回滚完成");
        }

        private static void UpdateVerIni()
        {
            try
            {
                string path = Path.GetFullPath(verFile);
                if (!File.Exists(path)) return;

                var lines = File.ReadAllLines(path).ToList();
                bool updated = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("ver="))
                    {
                        lines[i] = $"ver={R_ver}";
                        updated = true;
                        break;
                    }
                }
                if (updated)
                {
                    File.WriteAllLines(path, lines);
                    Console.WriteLine($"已更新版本号文件: {R_ver}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新版本号文件失败: {ex.Message}");
            }
        }

        private static void StartMainProgram()
        {
            if (OperatingSystem.IsWindows())
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine($"启动主程序失败: {ex.Message}");
                }
            }
            Console.Write($"更新DDTV到{type}-{R_ver}完成，请手动启动DDTV");
            if (Isdocker)
                Console.WriteLine($"，按任意键继续");
        }

        private static string FormatBytes(long bytes)
        {
            return bytes >= 1L << 30 ? $"{(double)bytes / (1L << 30):F2} GB"
                : bytes >= 1L << 20 ? $"{(double)bytes / (1L << 20):F2} MB"
                : bytes >= 1L << 10 ? $"{(double)bytes / (1L << 10):F2} KB"
                : $"{bytes} Bytes";
        }

        private class UpdateItem
        {
            public string Url { get; set; } = "";
            public string FileName { get; set; } = "";
            public string FilePath { get; set; } = "";
            public long Size { get; set; }
            public string Md5 { get; set; } = "";
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
}
