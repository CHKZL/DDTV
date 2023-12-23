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
            using (StreamWriter fileStream = new StreamWriter(before + "_fix日志.log", true, Encoding.UTF8))
            {
                foreach (var item in LogText)
                {
                    fileStream.WriteLine(item);
                }
            }
            // 转码完成后，删除源文件，将目标文件重命名为源文件
            File.Delete(before);
            File.Move(after, before);
        }
    }
}
