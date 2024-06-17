using AngleSharp.Io.Dom;
using Core.LogModule;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class Transcode
    {

        /// <summary>
        /// 异步转码函数
        /// </summary>
        /// <param name="before">源文件路径</param>
        /// <param name="after">目标文件路径</param>
        /// <returns>Task</returns>
        public async Task TranscodeAsync(string before, string after, RoomCardClass Card = null)
        {
            List<string> LogText = new List<string>();
            try
            {
                if (Card != null)
                    Card.DownInfo.DownloadFileList.TranscodingCount++;
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (File.Exists("./Plugins/ffmpeg/ffmpeg.exe"))
                    {
                        process.StartInfo.FileName = "./Plugins/ffmpeg/ffmpeg.exe";
                    }
                    else if (File.Exists("./Plugins/Plugins/ffmpeg/ffmpeg.exe"))
                    {
                        process.StartInfo.FileName = "./Plugins/Plugins/ffmpeg/ffmpeg.exe";
                    }
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


                // 转码完成后，如果目标文件存在且大小合理，删除源文件
                if (Card != null && File.Exists(after))
                {
                    FileInfo fileInfo = new FileInfo(after);
                    int FileSizeThreshold = 8 * 1024 * 1024;
                    if (fileInfo.Length > 8 * 1024 * 1024 && Config.Core_RunConfig._DeleteOriginalFileAfterRepair)
                    {
                        Tools.FileOperations.Delete(before, $"符合修复条件，自动删除源文件");
                    }
                }
                if (process != null)
                    process = null;
                LogText = null;
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
                Log.Info(nameof(TranscodeAsync), $"修复/转码任务完成:输出fix_log文件[{before + "_fix日志.log"}]");
            }
            if (Card != null)
                Card.DownInfo.DownloadFileList.TranscodingCount++;
        }
    }
}
