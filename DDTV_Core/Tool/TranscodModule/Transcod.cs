using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using ZXing.QrCode.Internal;

namespace DDTV_Core.Tool.TranscodModule
{
    public class Transcod
    {
        public static bool IsAutoTranscod = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsAutoTranscod, "True", CoreConfigClass.Group.Core));
        public static string TranscodParmetrs = CoreConfig.GetValue(CoreConfigClass.Key.TranscodParmetrs, "-y -hide_banner -loglevel warning -i {Before} -c copy {After}", CoreConfigClass.Group.Core);
        public static bool TranscodingCompleteAutoDeleteFiles = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.TranscodingCompleteAutoDeleteFiles, "False", CoreConfigClass.Group.Core));
        /// <summary>
        /// 调用ffmpeg进行转码
        /// </summary>
        /// <param name="Filename">转码文件</param>
        /// <param name="IsAutoDelOldFile">是否删除老文件</param>
        public static TranscodClass CallFFMPEG(TranscodClass transcodClass, bool IsAutoDelOldFile = true, bool IsSaveLogFile = false, string? Parmetrs = null)
        {
            try
            {
                transcodClass.Path = transcodClass.AfterFilePath.Replace(transcodClass.AfterFilePath.Split('/')[transcodClass.AfterFilePath.Split('/').Length-1], "");
                transcodClass.Name = transcodClass.AfterFilePath.Split('/')[transcodClass.AfterFilePath.Split('/').Length - 1];
                Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"开始修复或转码文件：[{transcodClass.BeforeFilePath}]");
                transcodClass.IsTranscod = true;
                Process process = new Process();
                TimeSpan all = new TimeSpan(), now = new TimeSpan();
                int progress = -1;
                string DleFile = transcodClass.BeforeFilePath;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process.StartInfo.FileName = "./plugins/ffmpeg/ffmpeg.exe";
                }
                else
                {
                    process.StartInfo.FileName = "ffmpeg";
                }
                
                if(!string.IsNullOrEmpty(Parmetrs))
                {
                     process.StartInfo.Arguments = Parmetrs.Replace("{After}", "\"" + transcodClass.AfterFilePath + "\"").Replace("{Before}", "\"" + transcodClass.BeforeFilePath + "\"");
                }
                else if (!string.IsNullOrEmpty(transcodClass.Parameters))
                {
                    process.StartInfo.Arguments = transcodClass.Parameters.Replace("{After}", "\"" + transcodClass.AfterFilePath + "\"").Replace("{Before}", "\"" + transcodClass.BeforeFilePath + "\"");
                }
                else
                {
                    
                    process.StartInfo.Arguments = TranscodParmetrs.Replace("{After}", "\"" + transcodClass.AfterFilePath + "\"").Replace("{Before}", "\"" + transcodClass.BeforeFilePath + "\"");
                    //process.StartInfo.Arguments = "-i {Before} -vf scale=640:360 {After} -hide_banner".Replace("{After}", "\"" + transcodClass.AfterFilePath + "\"").Replace("{Before}", "\"" + transcodClass.BeforeFilePath + "\"");
                }
                Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, "修复/转码任务:" + process.StartInfo.Arguments);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true; // 不显示窗口。
                process.EnableRaisingEvents = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                List<string> LogText = new List<string>();;
                if (IsSaveLogFile)
                {
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
                    process.OutputDataReceived  += delegate (object sender, DataReceivedEventArgs e)
                    {
                        try
                        {
                            LogText.Add(e.Data);
                        }
                        catch (Exception)
                        {
                        }
                    };  // 捕捉的信息
                }
                //process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                //{

                //    try
                //    {
                //        string stringResults = e.Data;
                //        if (stringResults == "" || stringResults == null) return;
                //        Console.WriteLine(stringResults);
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
                process.Start();
                process.BeginErrorReadLine();   // 开始异步读取
                process.Exited += delegate (object sender, EventArgs e)
                {
                    Process P = (Process)sender;
                    Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, "修复/转码任务完成:" + P.StartInfo.Arguments);
                };
                if (!process.HasExited)
                {
                    //如果超过10分钟都没有等待到exit信息，就跳过
                    process.WaitForExit(1200 * 1000);
                }
                process.Close();
                if (IsSaveLogFile)
                {
                    using (StreamWriter fileStream = new StreamWriter(transcodClass.Path + transcodClass.Name + "_fix日志.log", true, Encoding.UTF8))
                    {
                        foreach (var item in LogText)
                        {
                            fileStream.WriteLine(item);
                        }
                    }
                }


                //Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"转码任务：[{transcodClass.BeforeFilePath}]，Close");
                transcodClass.IsTranscod = true;
                if (IsAutoDelOldFile)
                {
                    Thread.Sleep(3000);
                    if (File.Exists(transcodClass.AfterFilePath))
                    {
                        Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"删除临时文件：[{transcodClass.BeforeFilePath}]");
                        FileOperation.Del(transcodClass.BeforeFilePath);
                    }
                    else
                    {
                        Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"删除临时文件失败。原因：[{transcodClass.AfterFilePath}]不存在");
                    }
                }
                //if (TranscodingCompleteAutoDeleteFiles && File.Exists(transcodClass.AfterFilePath))
                //{
                //    try
                //    {
                //        FileOperation.Del(transcodClass.BeforeFilePath);
                //        SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, $"转码后删除文件:{transcodClass.BeforeFilePath}成功");
                //    }
                //    catch (Exception e)
                //    {
                //        SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Warn, $"转码后删除文件:{transcodClass.BeforeFilePath}失败，详细日志已写入。", true, e);
                //    }
                //}
                transcodClass.IsTranscod = false;
                GC.Collect();
                //Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"转码任务：[{transcodClass.BeforeFilePath}]，GC");
                return transcodClass;
            }
            catch (Exception e)
            {
                transcodClass.IsTranscod = false;
                Log.AddLog(nameof(Transcod), LogClass.LogType.Warn, "转码出现致命错误！错误信息:\n" + e.ToString(), true, e, true);
                return transcodClass;
            }

        }
    }
}
