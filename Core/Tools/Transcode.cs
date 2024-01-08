using Core.LogModule;
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
        /// 异步转码函数
        /// </summary>
        /// <param name="before">源文件路径</param>
        /// <param name="after">目标文件路径</param>
        /// <returns>Task</returns>
        public async Task TranscodeAsync(string before, string after, long RoomId)
        {

            // 创建ProcessStartInfo对象，设置ffmpeg的路径和参数
            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = $"-y -i {before} -c copy {after}",
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
                    if (e != null && e.Data != null)
                    {
                        LogText.Add(e.Data);
                    }
                }
                catch (Exception)
                {
                }
            };  // 捕捉的信息
            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                try
                {
                    if (e != null && e.Data != null)
                    {
                        LogText.Add(e.Data);
                    }
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
#pragma warning disable CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
            process.Exited += delegate (object sender, EventArgs e)
            {
                Process P = (Process)sender;
                Log.Info(nameof(TranscodeAsync), "修复/转码任务完成:" + P.StartInfo.Arguments);
            };
#pragma warning restore CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
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
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    Tools.FileOperations.Delete(before);
                }
            }
            if (process != null)
                process = null;
            LogText = new();
        }
    }
}
