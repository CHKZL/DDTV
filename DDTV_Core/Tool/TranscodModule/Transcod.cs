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

namespace DDTV_Core.Tool.TranscodModule
{
    public class Transcod
    {
        public static bool IsAutoTranscod = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsAutoTranscod, "False", CoreConfigClass.Group.Core));
        public static string TranscodParmetrs = CoreConfig.GetValue(CoreConfigClass.Key.TranscodParmetrs, "-i {Before} -vcodec copy -acodec copy {After}", CoreConfigClass.Group.Core);
        public static bool TranscodingCompleteAutoDeleteFiles = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.TranscodingCompleteAutoDeleteFiles, "False", CoreConfigClass.Group.Core));
        /// <summary>
        /// 调用ffmpeg进行转码
        /// </summary>
        /// <param name="Filename">转码文件</param>
        public static TranscodClass CallFFMPEG_FLV(TranscodClass transcodClass)
        {
            try
            {
                Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"开始转码文件：[{transcodClass.BeforeFilePath}]");
                transcodClass.IsTranscod = true;
                Process process = new Process();
                TimeSpan all = new TimeSpan(), now = new TimeSpan();
                int progress = -1;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process.StartInfo.FileName = "./plugins/ffmpeg/ffmpeg.exe";
                }
                else
                {
                    process.StartInfo.FileName = "ffmpeg";
                }
                if (!string.IsNullOrEmpty(transcodClass.Parameters))
                {
                    process.StartInfo.Arguments = transcodClass.Parameters.Replace("{After}", "\"" + transcodClass.AfterFilePath + "\"").Replace("{Before}", "\"" + transcodClass.BeforeFilePath + "\"");
                }
                else
                {
                    transcodClass.AfterFilePath = transcodClass.AfterFilePath.Replace(".flv", ".mp4");
                    process.StartInfo.Arguments = TranscodParmetrs.Replace("{After}", "\"" + transcodClass.AfterFilePath + "\"").Replace("{Before}", "\"" + transcodClass.BeforeFilePath + "\"");
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
                    Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, "转码任务完成:" + P.StartInfo.Arguments);
                };
                if (process.HasExited)
                {
                    //如果超过20分钟都没有等待到exit信息，就跳过
                    process.WaitForExit(1200);
                }
                process.Close();
                Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"转码任务：[{transcodClass.BeforeFilePath}]，Close");
                transcodClass.IsTranscod = true;
                if (TranscodingCompleteAutoDeleteFiles && File.Exists(transcodClass.AfterFilePath))
                {
                    try
                    {
                        FileOperation.Del(transcodClass.BeforeFilePath);
                        SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, $"转码后删除文件:{transcodClass.BeforeFilePath}成功");
                    }
                    catch (Exception e)
                    {
                        SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Warn, $"转码后删除文件:{transcodClass.BeforeFilePath}失败，详细日志已写入。", true, e);
                    }
                }
                transcodClass.IsTranscod = false;
                GC.Collect();
                Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"转码任务：[{transcodClass.BeforeFilePath}]，GC");
                return transcodClass;
            }
            catch (Exception e)
            {
                transcodClass.IsTranscod = false;
                Log.AddLog(nameof(Transcod), LogClass.LogType.Warn, "转码出现致命错误！错误信息:\n" + e.ToString(), true, e, true);
                return transcodClass;
            }

        }
        public static TranscodClass CallFFMPEG_HLS(TranscodClass transcodClass)
        {
            try
            {
                Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"开始转码文件：[{transcodClass.BeforeFilePath}]");
                transcodClass.IsTranscod = true;
                Process process = new Process();
                TimeSpan all = new TimeSpan(), now = new TimeSpan();
                int progress = -1;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process.StartInfo.FileName = "./plugins/ffmpeg/ffmpeg.exe";
                }
                else
                {
                    process.StartInfo.FileName = "ffmpeg";
                }
                string Files = $"{transcodClass.AfterFilePath.Replace(transcodClass.AfterFilePath.Split('/')[transcodClass.AfterFilePath.Split('/').Length - 1], "")}{transcodClass.HLS_Files[0]}";
                
                if (transcodClass.HLS_Files.Count > 2)
                {
                    bool F = true;  
                    foreach (var item in transcodClass.HLS_Files)
                    {
                        if(F)
                        {
                            F = false;
                        }
                        else
                        {
                            Files += $"|{transcodClass.AfterFilePath.Replace(transcodClass.AfterFilePath.Split('/')[transcodClass.AfterFilePath.Split('/').Length - 1], "")}{item}.m4s";
                        }   
                    }
                  
                }

                else
                {
                    Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"转码任务：[{transcodClass.BeforeFilePath}]的HLS文件队列长度小于2，跳过合并");
                    return transcodClass;
                }
                int r = new Random().Next(1000,9999);
                File.WriteAllText($"{r}.txt", Files);
                process.StartInfo.Arguments = $"-i concat {r}.txt -c copy {transcodClass.AfterFilePath}{transcodClass.AfterFilenameExtension}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true; // 不显示窗口。
                process.EnableRaisingEvents = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
               
                DateTime beginTime = DateTime.Now;
                process.Start();
                process.BeginErrorReadLine();   // 开始异步读取
                process.Exited += delegate (object sender, EventArgs e)
                {
                    Process P = (Process)sender;
                    Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, "转码任务完成:" + P.StartInfo.Arguments);
                };
               
                if (!process.HasExited)
                {
                    //如果超过20分钟都没有等待到exit信息，就跳过
                    process.WaitForExit(1200);
                }
                process.Close();
                DDTV_Core.Tool.FileOperation.Del($"{r}.txt");
                Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"转码任务：[{transcodClass.BeforeFilePath}]，Close");
                transcodClass.IsTranscod = true;
                if (TranscodingCompleteAutoDeleteFiles && File.Exists(transcodClass.AfterFilePath))
                {
                    try
                    {
                        FileOperation.Del(transcodClass.BeforeFilePath);
                        SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Info, $"转码后删除文件:{transcodClass.BeforeFilePath}成功");
                    }
                    catch (Exception e)
                    {
                        SystemAssembly.Log.Log.AddLog(nameof(Transcod), SystemAssembly.Log.LogClass.LogType.Warn, $"转码后删除文件:{transcodClass.BeforeFilePath}失败，详细日志已写入。", true, e);
                    }
                }
                transcodClass.IsTranscod = false;
                GC.Collect();
                Log.AddLog(nameof(Transcod), LogClass.LogType.Info, $"转码任务：[{transcodClass.BeforeFilePath}]，GC");
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
