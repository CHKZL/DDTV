using DDTV_Core.SystemAssembly.ConfigModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.TranscodModule
{
    internal class Transcod
    {
        public static bool IsAutoTranscod= bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsAutoTranscod, "false", CoreConfigClass.Group.Core));
        public static string TranscodParmetrs = CoreConfig.GetValue(CoreConfigClass.Key.TranscodParmetrs, "-i {Before} -vcodec copy -acodec copy {After}", CoreConfigClass.Group.Core);
        /// <summary>
        /// 调用ffmpeg进行转码
        /// </summary>
        /// <param name="Filename">转码文件</param>
        public static TranscodClass CallFFMPEG(TranscodClass transcodClass)
        {
            try
            {
                transcodClass.IsTranscod = true;
                Process process = new Process();
                TimeSpan all = new TimeSpan(), now = new TimeSpan();
                int progress = -1;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process.StartInfo.FileName = "./lib/ffmpeg/ffmpeg.exe";
                }
                else
                {
                    process.StartInfo.FileName = "ffmpeg";
                }
                if (!string.IsNullOrEmpty(transcodClass.Parameters))
                {
                    process.StartInfo.Arguments = transcodClass.Parameters.Replace("{After}", transcodClass.AfterFilePath).Replace("{Before}", transcodClass.BeforeFilePath);
                }
                else
                {
                    process.StartInfo.Arguments = TranscodParmetrs.Replace("{After}", transcodClass.AfterFilePath.Replace(".flv",".mp4")).Replace("{Before}", transcodClass.BeforeFilePath);
                }
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true; // 不显示窗口。
                process.EnableRaisingEvents = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                //process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                //{
                //    try
                //    {
                //        string stringResults = e.Data;
                //        if (stringResults == "" || stringResults == null) return;
                //        //Console.WriteLine(stringResults);
                //        if (stringResults.Contains("Duration"))
                //        {
                //            all = TimeSpan.Parse(Regex.Match(stringResults, @"(?<=Duration: ).*?(?=, start)").Value);
                //        }
                //        if (stringResults.Contains("time"))
                //        {
                //            string tmpNow = Regex.Match(stringResults, @"(?<= time=).*?(?= bitrate)").Value;
                //            if (tmpNow != "")
                //            {
                //                now = TimeSpan.Parse(tmpNow);
                //                progress = (int)Math.Ceiling(now.TotalMilliseconds / all.TotalMilliseconds * 100);
                //            }
                //        }
                //        if (progress > 0)
                //        {
                //            SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info_Transcod, $"转码进度:{progress}%");
                //            transcodClass.Progress = progress;
                //        }
                //    }
                //    catch (Exception)
                //    {
                //    }
                //};  // 捕捉的信息
                DateTime beginTime = DateTime.Now;
                process.Start();
                process.BeginErrorReadLine();   // 开始异步读取
                process.Exited += delegate (object sender, EventArgs e)
                {
                    Process P = (Process)sender;
                    SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, "转码任务完成:" + P.StartInfo.Arguments);
                };

                //NagisaCo: 等待转码，使mp4文件完整后再开始上传功能
                process.WaitForExit();
                process.Close();
                transcodClass.IsTranscod = false;
                GC.Collect();
                return transcodClass;
            }
            catch (Exception e)
            {
                transcodClass.IsTranscod = false;
                SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, "转码出现致命错误！错误信息:\n" + e.ToString());
                return transcodClass;
            }

        }
    }
}
