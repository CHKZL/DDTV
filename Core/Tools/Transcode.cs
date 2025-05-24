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
            string LogTextAsString = string.Empty; // 用来保存LogText转成字符串的结果
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
                    if (!string.IsNullOrEmpty(Config.Core_RunConfig._UsingCustomFFMPEG) && File.Exists(Config.Core_RunConfig._UsingCustomFFMPEG))
                    {
                        process.StartInfo.FileName = Config.Core_RunConfig._UsingCustomFFMPEG;
                    }
                    else if (File.Exists("./Plugins/ffmpeg/ffmpeg.exe"))
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
                        Log.Info(nameof(TranscodeAsync), $"修复后的文件大小不符合预期，放弃删除源文件:[{before}]");
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
                    SMTP.TriggerEvent((Card,LogTextAsString), SMTP.SMTP_EventType.TranscodingFail);
                }

            }
            if (Card != null)
                Card.DownInfo.DownloadFileList.TranscodingCount++;

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

                // 可在config里增加usingCustomMKVmerge增加自定义路径功能
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //if (!string.IsNullOrEmpty(Config.Core_RunConfig._UsingCustomMKVMERGE) && File.Exists(Config.Core_RunConfig._UsingCustomMKVMERGE))
                    //{
                    //	process.StartInfo.FileName = Config.Core_RunConfig._UsingCustomMKVMERGE;
                    //}
                    //else 
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
    }
}
