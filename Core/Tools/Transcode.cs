using AngleSharp.Io.Dom;
using Core.LogModule;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class Transcode
    {


        public static string GetFFMPEGPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!string.IsNullOrEmpty(Config.Core_RunConfig._UsingCustomFFMPEG) && File.Exists(Config.Core_RunConfig._UsingCustomFFMPEG))
                {
                    return Config.Core_RunConfig._UsingCustomFFMPEG;
                }
                else if (File.Exists("./Plugins/ffmpeg/ffmpeg.exe"))
                {
                    return "./Plugins/ffmpeg/ffmpeg.exe";
                }
                else if (File.Exists("./Plugins/Plugins/ffmpeg/ffmpeg.exe"))
                {
                    return "./Plugins/Plugins/ffmpeg/ffmpeg.exe";
                }
            }
            return "ffmpeg";
        }

        /// <summary>
        /// 异步转码函数
        /// </summary>
        /// <param name="before">源文件路径</param>
        /// <param name="after">目标文件路径</param>
        /// <returns>Task</returns>
        public async Task TranscodeAsync(string before, string after, RoomCardClass Card = null)
        {
            List<string> LogText = new List<string>();
            string LogTextAsString = string.Empty; // 用来保存LogText转成字符串的结果
            //捕获当前任务的文件列表引用再计数：录制会话结束时DownloadCompletedReset会把DownloadFileList替换成新对象，
            //如果每次都用Card.DownInfo.DownloadFileList现取，开始计数落在旧对象、结束计数落在新对象，计数永远失衡
            var fileList = Card?.DownInfo.DownloadFileList;
            try
            {
                if (fileList != null)
                    fileList.TranscodingCount++;
                // 创建ProcessStartInfo对象，设置ffmpeg的路径和参数
                var process = new Process
                {

                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = Config.Core_RunConfig._AutomaticRepair_Arguments.Replace("{before}", before).Replace("{after}", after),
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        StandardOutputEncoding = Encoding.UTF8,
                    }
                };

                process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    try
                    {
                        LogText.Add(e.Data);
                    }
                    catch (Exception)
                    {
                    }
                };  // 捕捉的信息
                process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    try
                    {
                        LogText.Add(e.Data);
                    }
                    catch (Exception)
                    {
                    }
                };  // 捕捉的信息

                process.StartInfo.FileName = GetFFMPEGPath();

                // 启动Process
                process.Start();
                // 开始异步读取错误输出
                process.BeginErrorReadLine();
                process.Exited += delegate (object sender, EventArgs e)
                {
                    Process P = (Process)sender;
                    Log.Info(nameof(TranscodeAsync), "修复/转码任务完成:" + P.StartInfo.Arguments);
                };
                // 等待Process退出
                await process.WaitForExitAsync();


                // 转码完成后，如果目标文件存在且大小合理，删除源文件
                if (Card != null && File.Exists(after))
                {
                    FileInfo after_fileInfo = new FileInfo(after);
                    FileInfo before_fileInfo = new FileInfo(before);
                    //根据配置文件和原始文件，计算可以接受的文件大小差异
                    long FileDifferenceSize = (long)(before_fileInfo.Length * Config.Core_RunConfig._TranscodeFileDifference);
                    //如果修复后的文件大小大于原始文件大小+差异值，删除源文件
                    if (after_fileInfo.Length + FileDifferenceSize > before_fileInfo.Length && Config.Core_RunConfig._DeleteOriginalFileAfterRepair)
                    {
                        Tools.FileOperations.Delete(before, $"符合修复条件，自动删除源文件");
                        Log.Info(nameof(TranscodeAsync), $"符合修复条件，自动删除源文件:[{before}]");
                    }
                    else if (!Config.Core_RunConfig._DeleteOriginalFileAfterRepair)
                    {
                        Log.Info(nameof(TranscodeAsync), $"配置项[_DeleteOriginalFileAfterRepair]已设置关闭，放弃删除源文件:[{before}]");
                    }
                    else
                    {
                        SMTP.TriggerEvent(Card, SMTP.SMTP_EventType.AbandonTranscod);

                        //判断高级修复
                        if (Config.Core_RunConfig._DetectErroneousFilesFixThem)
                        {
                            Log.Info(nameof(TranscodeAsync), $"修复后的文件大小不符合预期，进行高级修复，源文件:[{before}]，目标文件：[{after}]");
                            await FixDurationWithMkvToolnixAsync(before, after);
                        }
                        else
                        {
                            Log.Info(nameof(TranscodeAsync), $"修复后的文件大小不符合预期，放弃删除源文件:[{before}]");
                        }
                    }
                }
                if (process != null)
                    process = null;
            }
            catch (Exception e)
            {
                Log.Error(nameof(TranscodeAsync), $"修复/转码任务出现未知错误:{e.ToString()}", e, true);
                using (StreamWriter fileStream = new StreamWriter(before + "_fix日志.log", true, Encoding.UTF8))
                {
                    foreach (var item in LogText)
                    {
                        fileStream.WriteLine(item);
                    }
                }

                // 将LogText转成字符串并保存到LogTextAsString
                LogTextAsString = string.Join(Environment.NewLine, LogText);

                Log.Info(nameof(TranscodeAsync), $"修复/转码任务完成:输出fix_log文件[{before + "_fix日志.log"}]");
                if (Card != null)
                {
                    SMTP.TriggerEvent((Card, LogTextAsString), SMTP.SMTP_EventType.TranscodingFail);
                }

            }
            if (fileList != null)
                fileList.TranscodingCount--;

            LogText = null;
        }
        /// <summary>
        /// 使用 MKVToolNix 修复 MKV 文件时长的异步函数
        /// </summary>
        /// <param name="before">源文件路径</param>
        /// <param name="after">目标文件路径</param>
        /// <param name="Card">可选，房间卡对象</param>
        /// <returns>Task</returns>
        public async Task FixDurationWithMkvToolnixAsync(string before, string after)
        {
            List<string> LogText = new List<string>();
            string LogTextAsString = string.Empty;
            try
            {


                // 构造命令行参数
                string arguments =
                    $"--ui-language zh_CN --priority lower --output \"{after}\" " +
                    "--language 0:und --color-matrix-coefficients 0:1 --color-transfer-characteristics 0:1 " +
                    "--color-primaries 0:1 --fix-bitstream-timing-information 0:1 --language 1:und " +
                    $"\"(\" \"{before}\" \")\" --track-order 0:0,0:1";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = arguments,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        StandardOutputEncoding = Encoding.UTF8,
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    try { if (!string.IsNullOrEmpty(e.Data)) LogText.Add(e.Data); } catch { }
                };
                process.OutputDataReceived += (sender, e) =>
                {
                    try { if (!string.IsNullOrEmpty(e.Data)) LogText.Add(e.Data); } catch { }
                };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (File.Exists("./Plugins/MKVToolNix/mkvmerge.exe"))
                    {
                        process.StartInfo.FileName = "./Plugins/MKVToolNix/mkvmerge.exe";
                    }
                    else if (File.Exists("./Plugins/Plugins/MKVToolNix/mkvmerge.exe"))
                    {
                        process.StartInfo.FileName = "./Plugins/Plugins/MKVToolNix/mkvmerge.exe";
                    }
                    else
                    {
                        process.StartInfo.FileName = "mkvmerge.exe";
                    }
                }
                else
                {
                    process.StartInfo.FileName = "mkvmerge";
                }

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.Exited += (sender, e) =>
                {
                    Process P = (Process)sender;
                    Log.Info(nameof(FixDurationWithMkvToolnixAsync), "MKVToolNix 修复任务完成:" + P.StartInfo.Arguments);
                };
                await process.WaitForExitAsync();
                process = null;
            }
            catch (Exception e)
            {
                Log.Error(nameof(FixDurationWithMkvToolnixAsync), $"MKVToolNix修复任务出现未知错误:{e}", e, true);
                using (StreamWriter fileStream = new StreamWriter(before + "_MKVToolNix_fix日志.log", true, Encoding.UTF8))
                {
                    foreach (var item in LogText)
                    {
                        fileStream.WriteLine(item);
                    }
                }
                LogTextAsString = string.Join(Environment.NewLine, LogText);
                Log.Info(nameof(FixDurationWithMkvToolnixAsync), $"MKVToolNix修复任务完成:输出fix_log文件[{before + "_MKVToolNix_fix日志.log"}]");

            }
            LogText = null;
        }

        /// <summary>
        /// 将多个分片文件整体重编码合并为单个MP4文件。
        /// 注意：分片之间的编码参数(分辨率/SPS/PPS/音频采样率等)大概率不同——DDTV正是在检测到这些参数变化时才切割的。
        /// 这种分片用-c copy流拷贝拼接，解码器会拿第一段的参数集去解后续段的码流，导致花屏/绿屏，
        /// 因此合并必须整体重编码(解码→拼接→重新编码)，CPU开销大、耗时约等于整场录像时长。
        /// 本方法只负责合并本身，不删除任何源文件，源文件清理由调用方决定
        /// </summary>
        /// <param name="inputFiles">待合并的输入文件路径列表(按时间顺序)</param>
        /// <param name="outputFile">合并输出文件路径</param>
        /// <returns>是否合并成功；失败时源文件全部保留</returns>
        public async Task<bool> MergeSessionFragmentsAsync(List<string> inputFiles, string outputFile)
        {
            List<string> LogText = new List<string>();
            string listFilePath = string.Empty;
            try
            {
                //输入验证：至少2个文件且全部存在
                if (inputFiles == null || inputFiles.Count < 2)
                {
                    Log.Warn(nameof(MergeSessionFragmentsAsync), $"强制合并跳过：分片数量不足({inputFiles?.Count ?? 0}个)");
                    return false;
                }
                foreach (var path in inputFiles)
                {
                    if (!File.Exists(path))
                    {
                        Log.Warn(nameof(MergeSessionFragmentsAsync), $"强制合并跳过：分片文件不存在[{path}]，源分片全部保留");
                        return false;
                    }
                }

                //生成concat分片列表文件：文件名带GUID，避免多个合并任务并发时互相覆盖(旧的MergeFragmentsToFile用固定list.txt有这个坑)
                //必须写无BOM的UTF-8：Encoding.UTF8会带BOM，ffmpeg concat demuxer会把BOM当成第一行内容的一部分导致解析失败
                string outputDir = Path.GetDirectoryName(Path.GetFullPath(outputFile));
                listFilePath = Path.Combine(outputDir, $"force_merge_{Guid.NewGuid():N}.txt");
                using (var writer = new StreamWriter(listFilePath, false, new UTF8Encoding(false)))
                {
                    foreach (var path in inputFiles)
                    {
                        //ffmpeg concat格式：路径用单引号包裹，路径中的单引号转义为 '\''，统一用正斜杠
                        string escaped = Path.GetFullPath(path).Replace("\\", "/").Replace("'", "'\\''");
                        writer.WriteLine($"file '{escaped}'");
                    }
                }

                string arguments = Config.Core_RunConfig._ForceMerge_Arguments
                    .Replace("{list}", listFilePath)
                    .Replace("{after}", outputFile);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = GetFFMPEGPath(),
                        Arguments = arguments,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                    }
                };
                process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    try { if (!string.IsNullOrEmpty(e.Data)) LogText.Add(e.Data); } catch (Exception) { }
                };
                process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    try { if (!string.IsNullOrEmpty(e.Data)) LogText.Add(e.Data); } catch (Exception) { }
                };
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();
                //WaitForExitAsync只保证进程退出，异步输出流可能还没读完，再同步等一次把缓冲区刷完，否则失败日志可能缺尾部
                process.WaitForExit();

                //合并结果校验：进程退出码为0、输出文件存在且大小不为0
                //重编码后体积可能与源总和差异较大(取决于编码参数)，只按"非空"校验，不按比例卡死
                if (process.ExitCode == 0 && File.Exists(outputFile) && new FileInfo(outputFile).Length > 0)
                {
                    return true;
                }
                Log.Warn(nameof(MergeSessionFragmentsAsync), $"强制合并失败(Exit Code:{process.ExitCode})，源分片全部保留，输出文件:[{outputFile}]");
                WriteMergeFailLog(outputFile, LogText);
                return false;
            }
            catch (Exception e)
            {
                Log.Error(nameof(MergeSessionFragmentsAsync), $"强制合并任务出现未知错误:{e}", e, true);
                WriteMergeFailLog(outputFile, LogText);
                return false;
            }
            finally
            {
                if (!string.IsNullOrEmpty(listFilePath) && File.Exists(listFilePath))
                {
                    try { File.Delete(listFilePath); } catch (Exception) { }
                }
            }
        }

        /// <summary>
        /// 合并失败时把ffmpeg输出写到日志文件，便于排查
        /// </summary>
        private static void WriteMergeFailLog(string outputFile, List<string> logText)
        {
            try
            {
                if (logText.Count == 0)
                {
                    return;
                }
                using (StreamWriter fileStream = new StreamWriter(outputFile + "_merge日志.log", true, Encoding.UTF8))
                {
                    foreach (var item in logText)
                    {
                        fileStream.WriteLine(item);
                    }
                }
                Log.Info(nameof(WriteMergeFailLog), $"强制合并失败，输出merge_log文件[{outputFile + "_merge日志.log"}]");
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 使用 ffmpeg 将多个媒体分片文件追加合并到一个现有的视频文件中。
        /// </summary>
        /// <param name="outputFilePath">目标输出文件路径（将被追加内容）。</param>
        /// <param name="fragmentFilePaths">待合并的分片文件路径列表（顺序必须正确）。</param>
        /// <param name="deleteFragmentsAfterMerge">合并成功后是否删除源分片文件。</param>
        /// <returns>一个元组，包含合并操作的成功状态和错误信息（如果失败）。</returns>
        public static (bool Success, string ErrorMessage) MergeFragmentsToFile(
            string outputFilePath,
            List<string> fragmentFilePaths,
            bool deleteFragmentsAfterMerge = true)
        {
            // 1. 输入参数验证
            if (string.IsNullOrEmpty(outputFilePath))
            {
                return (false, "目标文件路径不能为空。");
            }
            if (fragmentFilePaths == null || fragmentFilePaths.Count == 0)
            {
                return (false, "分片文件列表不能为空。");
            }
            foreach (var path in fragmentFilePaths)
            {
                if (!File.Exists(path))
                {
                    return (false, $"分片文件不存在: {path}");
                }
            }

            // 2. 生成分片列表文件 (list.txt)
            string listFilePath = Path.Combine(Path.GetDirectoryName(outputFilePath) ?? "", "list.txt");
            try
            {
                using (var writer = new StreamWriter(listFilePath))
                {
                    foreach (var path in fragmentFilePaths)
                    {
                        // ffmpeg 的 concat 协议要求路径用单引号包裹，并且反斜杠需要转义
                        writer.WriteLine($"file '{path.Replace("\\", "\\\\")}'");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"创建分片列表文件失败: {ex.Message}");
            }

            // 3. 构建并执行 ffmpeg 命令
            string arguments = $"-f concat -safe 0 -i \"{listFilePath}\" -c copy -bsf:a aac_adtstoasc -y \"{outputFilePath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = GetFFMPEGPath(),
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                try
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        // 合并失败，返回错误信息
                        return (false, $"ffmpeg 执行失败 (Exit Code: {process.ExitCode})。错误详情: {error}。命令行: {GetFFMPEGPath()} {arguments}");
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"启动 ffmpeg 进程失败: {ex.Message}");
                }
            }

            // 4. 清理工作
            try
            {
                // 删除临时的列表文件
                if (File.Exists(listFilePath))
                {
                    File.Delete(listFilePath);
                }

                // 如果设置了，删除源分片文件
                if (deleteFragmentsAfterMerge)
                {
                    foreach (var path in fragmentFilePaths)
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 清理失败不影响合并结果，但应记录警告
                Console.WriteLine($"合并成功，但清理临时文件时发生警告: {ex.Message}");
            }

            // 5. 成功完成
            return (true, string.Empty);
        }
    }
}
