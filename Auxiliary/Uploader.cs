using Auxiliary.LiveChatScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using static System.IO.Path;
using static Auxiliary.Downloader;


namespace Auxiliary
{
    public class Upload
    {
        public DownIofoData downIofo;
        private static string type;
        private static bool result;
        private static int times;
        private static int RETRY_MAX_TIMES = 3;//重试次数
        private static int RETRY_WAITING_TIME = 10;//重试等待时间
        /// <summary>
        /// 初始化上传任务
        /// </summary>
        private static void Init()
        {
            type = null;
            result = false;
            times = 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dinfo">传入DownIofoData 用于获取直播数据并修改备注</param>
        /// <param name="file">上传源文件路径</param>
        /// <param name="path">上传目标路径</param>
        public void UploadVideo(ref DownIofoData dinfo, string file, string path)
        {
            downIofo = dinfo;
            Init();
            if (!MMPU.enableUpload) return;
            try
            {
                for (int i = 1; i <= MMPU.UploadOrder.Count; i++)
                {
                    switch (MMPU.UploadOrder[i])
                    {
                        case "OneDrive":
                            OneDrive oneDrive = new OneDrive(MMPU.oneDriveConfig);
                            oneDrive.UploadToOneDrive(downIofo, file, MMPU.oneDrivePath + path);
                            oneDrive = null;
                            GC.Collect();
                            break;
                        case "Cos":
                            Cos cos = new Cos(MMPU.cosConfig);
                            cos.UploadToCos(downIofo, file, MMPU.cosPath + path);
                            cos = null;
                            GC.Collect();
                            break;
                        default:
                            break;
                    }
                }
                if (MMPU.deleteAfterUpload == "1" && System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }
            catch (Exception)
            { }
        }

        public class Cos
        {
            private string configFile;
            public Cos(string config)
            {
                configFile = config;
                type = "Cos";
                times = 1;
                result = false;
            }
            internal void UploadToCos(DownIofoData downIofo, string localFile, string remotePath)
            {
                try
                {
                    InfoLog.InfoPrintf($"\r\n==============建立{type}上传任务================\r\n" +
                                  $"主播名:{downIofo.主播名称}" +
                                  $"\r\n标题:{downIofo.标题}" +
                                  $"\r\n开播时间:{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString())}" +
                                  $"\r\n结束时间:{MMPU.Unix转换为DateTime(downIofo.结束时间.ToString())}" +
                                  $"\r\n本地文件:{localFile}" +
                                  $"\r\n上传路径:{remotePath}" +
                                  $"\r\n网盘类型:{type}" +
                                  $"\r\n===============建立{type}上传任务===============\r\n", InfoLog.InfoClass.上传必要提示);
                    InfoLog.InfoPrintf($"{type}:开始第{times}次上传", InfoLog.InfoClass.上传必要提示);
                    downIofo.备注 = $"开始第{times}次上传至{type}";
                    Process proc = new Process();
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        proc.StartInfo.FileName = "coscmd";
                    }
                    else
                    {
                        proc.StartInfo.FileName = "coscmd";
                    }
                    proc.StartInfo.Arguments = $"upload \"{localFile}\" \"{remotePath}\" --skipmd5";

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.CreateNoWindow = true; // 不显示窗口。
                    proc.EnableRaisingEvents = true;
                    proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    proc.OutputDataReceived += new DataReceivedEventHandler(Output);  // 捕捉的信息
                    DateTime beginTime = DateTime.Now;

                    proc.Start();
                    proc.BeginOutputReadLine();   // 开始异步读取
                    proc.Exited += Process_Exited;


                    //proc.Start();
                    proc.Exited += Process_Exited;
                    proc.WaitForExit();
                    proc.Close();
                    GC.Collect();
                    if (result)
                    {
                        InfoLog.InfoPrintf($"\r\n=============={type}上传成功================\r\n" +
                                           $"主播名:{downIofo.主播名称}" +
                                           $"\r\n标题:{downIofo.标题}" +
                                           $"\r\n开播时间:{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString())}" +
                                           $"\r\n结束时间:{MMPU.Unix转换为DateTime(downIofo.结束时间.ToString())}" +
                                           $"\r\n本地文件:{localFile}" +
                                           $"\r\n上传路径:{remotePath}" +
                                           $"\r\n网盘类型:{type}" +
                                           $"\r\n==============={type}上传成功===============\r\n", InfoLog.InfoClass.上传必要提示);
                        downIofo.备注 = $"{type}上传成功";
                        return;
                    }
                    InfoLog.InfoPrintf($"\r\n=============={type}上传失败================\r\n" +
                                               $"主播名:{downIofo.主播名称}" +
                                               $"\r\n标题:{downIofo.标题}" +
                                               $"\r\n开播时间:{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString())}" +
                                               $"\r\n结束时间:{MMPU.Unix转换为DateTime(downIofo.结束时间.ToString())}" +
                                               $"\r\n本地文件:{localFile}" +
                                               $"\r\n上传路径:{remotePath}" +
                                               $"\r\n网盘类型:{type}" +
                                               $"\r\n==============={type}上传失败===============\r\n", InfoLog.InfoClass.上传必要提示);
                    downIofo.备注 = $"{type}上传失败";
                }
                catch (Exception)
                { }
            }

            private static void Process_Exited(object sender, EventArgs e)
            {
                Process P = (Process)sender;
                if (P.ExitCode == 0)
                {
                    InfoLog.InfoPrintf($"{type}:上传完毕", InfoLog.InfoClass.上传必要提示);
                    result = true;
                    times++;
                }
                else
                {
                    if (times < RETRY_MAX_TIMES)
                    {
                        InfoLog.InfoPrintf($"{type}:第{times}/{RETRY_MAX_TIMES}次上传失败，{RETRY_WAITING_TIME}s后重试", InfoLog.InfoClass.上传必要提示);
                        result = false;
                        times++;
                        Thread.Sleep(RETRY_WAITING_TIME * 1000);
                    }
                    else
                    {
                        InfoLog.InfoPrintf($"{type}:第{times}/{RETRY_MAX_TIMES}次{type}上传失败", InfoLog.InfoClass.上传必要提示);
                        result = false;
                        times++;
                    }
                }
            }
            private static void Output(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == "" || e.Data == null) return;
                InfoLog.InfoPrintf($"{type}:{e.Data}", InfoLog.InfoClass.上传必要提示);
                // Console.WriteLine(e.Data);
            }
        }

        private class OneDrive
        {
            private string configFile;
            public OneDrive(string config)
            {
                configFile = config;
                type = "OneDrive";
                times = 1;
                result = false;
            }
            internal void UploadToOneDrive(DownIofoData downIofo, string localFile, string remotePath)
            {
                try
                {
                    InfoLog.InfoPrintf($"\r\n==============建立{type}上传任务================\r\n" +
                                  $"主播名:{downIofo.主播名称}" +
                                  $"\r\n标题:{downIofo.标题}" +
                                  $"\r\n开播时间:{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString())}" +
                                  $"\r\n结束时间:{MMPU.Unix转换为DateTime(downIofo.结束时间.ToString())}" +
                                  $"\r\n本地文件:{localFile}" +
                                  $"\r\n上传路径:{remotePath}" +
                                  $"\r\n网盘类型:{type}" +
                                  $"\r\n===============建立{type}上传任务===============\r\n", InfoLog.InfoClass.上传必要提示);
                    while (times <= RETRY_MAX_TIMES)
                    {
                        InfoLog.InfoPrintf($"{type}:开始第{times}次上传", InfoLog.InfoClass.上传必要提示);
                        downIofo.备注 = $"开始第{times}次上传至{type}";
                        Process proc = new Process();
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            proc.StartInfo.FileName = "C:\\Users\\Co\\One\\OneDriveUploader.exe";
                        }
                        else
                        {
                            proc.StartInfo.FileName = "OneDriveUploader";
                        }
                        proc.StartInfo.Arguments = $"-f -c \"{configFile}\" -s \"{localFile}\" -r \"{remotePath}\"";
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.RedirectStandardError = true;
                        proc.StartInfo.RedirectStandardInput = true;
                        proc.StartInfo.CreateNoWindow = true; // 不显示窗口。
                        proc.EnableRaisingEvents = true;
                        proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                        proc.OutputDataReceived += new DataReceivedEventHandler(Output);  // 捕捉的信息
                        DateTime beginTime = DateTime.Now;

                        proc.Start();
                        proc.BeginOutputReadLine();   // 开始异步读取
                        proc.Exited += Process_Exited;
                        proc.WaitForExit();
                        proc.Close();
                        GC.Collect();
                        if (result)
                        {
                            InfoLog.InfoPrintf($"\r\n=============={type}上传成功================\r\n" +
                                               $"主播名:{downIofo.主播名称}" +
                                               $"\r\n标题:{downIofo.标题}" +
                                               $"\r\n开播时间:{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString())}" +
                                               $"\r\n结束时间:{MMPU.Unix转换为DateTime(downIofo.结束时间.ToString())}" +
                                               $"\r\n本地文件:{localFile}" +
                                               $"\r\n上传路径:{remotePath}" +
                                               $"\r\n网盘类型:{type}" +
                                               $"\r\n==============={type}上传成功===============\r\n", InfoLog.InfoClass.上传必要提示);
                            downIofo.备注 = $"{type}上传成功";
                            return;
                        }
                    }
                    InfoLog.InfoPrintf($"\r\n=============={type}上传失败================\r\n" +
                                               $"主播名:{downIofo.主播名称}" +
                                               $"\r\n标题:{downIofo.标题}" +
                                               $"\r\n开播时间:{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString())}" +
                                               $"\r\n结束时间:{MMPU.Unix转换为DateTime(downIofo.结束时间.ToString())}" +
                                               $"\r\n本地文件:{localFile}" +
                                               $"\r\n上传路径:{remotePath}" +
                                               $"\r\n网盘类型:{type}" +
                                               $"\r\n==============={type}上传失败===============\r\n", InfoLog.InfoClass.上传必要提示);
                    downIofo.备注 = $"{type}上传失败";
                }
                catch (Exception)
                { }
            }

            private static void Process_Exited(object sender, EventArgs e)
            {
                Process P = (Process)sender;
                if (P.ExitCode == 0)
                {
                    InfoLog.InfoPrintf($"{type}:上传完毕", InfoLog.InfoClass.上传必要提示);
                    result = true;
                    times++;
                }
                else
                {
                    if (times < RETRY_MAX_TIMES)
                    {
                        InfoLog.InfoPrintf($"{type}:第{times}/{RETRY_MAX_TIMES}次上传失败，{RETRY_WAITING_TIME}s后重试", InfoLog.InfoClass.上传必要提示);
                        result = false;
                        times++;
                        Thread.Sleep(RETRY_WAITING_TIME * 1000);
                    }
                    else
                    {
                        InfoLog.InfoPrintf($"{type}:第{times}/{RETRY_MAX_TIMES}次{type}上传失败", InfoLog.InfoClass.上传必要提示);
                        result = false;
                        times++;
                    }
                }
            }
            private static void Output(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == "" || e.Data == null) return;
                InfoLog.InfoPrintf($"{type}:{e.Data}", InfoLog.InfoClass.上传必要提示);
                // Console.WriteLine(e.Data);
            }
        }
    }
}
