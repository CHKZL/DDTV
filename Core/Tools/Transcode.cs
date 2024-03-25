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

        /// <summary>
        /// 合并多个视频文件为一个
        /// </summary>
        /// <param name="before">目标写入文件</param>
        /// <param name="FilesFile">即将组装的文件列表</param>
        /// <param name="Card">房间本体卡片</param>
        /// <returns></returns>
        public async Task MergeFilesAsync(string before, string[] FilesFile, RoomCardClass Card = null)
        {
            try
            {
                var filesList = string.Join("|", FilesFile);
                // 创建ProcessStartInfo对象，设置ffmpeg的路径和参数
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = $"-i concat:\"{filesList}\" -c copy {before}",
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        StandardOutputEncoding = Encoding.UTF8,
                    }
                };
                List<string> LogText = new List<string>();
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process.StartInfo.FileName = "./plugins/ffmpeg/ffmpeg.exe";
                }
                else
                {
                    process.StartInfo.FileName = "ffmpeg";
                }

                // 启动Process
                process.Start();
                // 开始异步读取错误输出
                process.BeginErrorReadLine();
                process.Exited += delegate (object sender, EventArgs e)
                {
                    Process P = (Process)sender;
                    Log.Info(nameof(TranscodeAsync), "合并任务完成:" + P.StartInfo.Arguments);
                };
                // 等待Process退出
                await process.WaitForExitAsync();
                //if (Core.Config.Core._DebugMode)
                {
                    using (StreamWriter fileStream = new StreamWriter(before + "_fix日志.log", true, Encoding.UTF8))
                    {
                        foreach (var item in LogText)
                        {
                            fileStream.WriteLine(item);
                        }
                    }
                    Log.Info(nameof(TranscodeAsync), $"合并任务完成:输出fix_log文件[{before + "_fix日志.log"}]");
                }            
                if (process != null)
                    process = null;
                LogText = null;
                if (Card != null)
                {
                    Card.DownInfo.DownloadFileList.VideoFile.Clear();
                    Card.DownInfo.DownloadFileList.VideoFile.Add(before);
                }
            }
            catch (Exception e)
            {
                 Log.Warn(nameof(TranscodeAsync), $"合并任务出现未知错误:{e.ToString()}");
            }
        }

        /// <summary>
        /// 异步转码函数
        /// </summary>
        /// <param name="before">源文件路径</param>
        /// <param name="after">目标文件路径</param>
        /// <returns>Task</returns>
        public async Task TranscodeAsync(string before, string after, RoomCardClass Card)
        {
            try
            {
                Card.DownInfo.DownloadFileList.TranscodingCount++;
                // 创建ProcessStartInfo对象，设置ffmpeg的路径和参数
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = $"-y -i '{before}' -c copy '{after}'",
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        StandardOutputEncoding = Encoding.UTF8,
                    }
                };
                List<string> LogText = new List<string>();
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process.StartInfo.FileName = "./plugins/ffmpeg/ffmpeg.exe";
                }
                else
                {
                    process.StartInfo.FileName = "ffmpeg";
                }

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
                if (Core.Config.Core._DebugMode)
                {
                    using (StreamWriter fileStream = new StreamWriter(before + "_fix日志.log", true, Encoding.UTF8))
                    {
                        foreach (var item in LogText)
                        {
                            fileStream.WriteLine(item);
                        }
                    }
                    Log.Info(nameof(TranscodeAsync), $"修复/转码任务完成:输出fix_log文件[{before + "_fix日志.log"}]");
                }
                // 转码完成后，如果目标文件存在且大小合理，删除源文件
                if (File.Exists(after))
                {
                    FileInfo fileInfo = new FileInfo(after);
                    int FileSizeThreshold = 8 * 1024 * 1024;
                    if (fileInfo.Length > 8 * 1024 * 1024)
                    {
                        Tools.FileOperations.Delete(before,$"转码后文件大小小于设置的{(FileSizeThreshold/1024/1024)}MB，自动删除源文件");
                    }
                }
                if (process != null)
                    process = null;
                LogText = null;
            }
            catch (Exception e)
            {
                 Log.Warn(nameof(TranscodeAsync), $"修复/转码任务出现未知错误:{e.ToString()}");
            }
            Card.DownInfo.DownloadFileList.TranscodingCount++;
        }
    }
}
