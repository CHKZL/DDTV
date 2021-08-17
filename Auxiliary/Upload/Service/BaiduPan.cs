using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Auxiliary.Upload.Service
{
    class BaiduPan : ServiceInterface
    {
        private int exitCode;
        private bool status;
        /// <summary>
        /// 初始化BaiduPan Upload
        /// </summary>
        public BaiduPan()
        { }

        /// <summary>
        /// 上传到BaiduPan
        /// </summary>
        /// <param name="uploadInfo">传入上传信息</param>
        public void doUpload(Info.TaskInfo task)
        {
            Process proc = new Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                proc.StartInfo.FileName = @".\BaiduPCS-Go.exe"; //windows下的程序位置
            }
            else
            {
                proc.StartInfo.FileName = "BaiduPCS-Go";
            }
            DateTime showTime = DateTime.Now;
            proc.StartInfo.Arguments = $"u \"{task.localPath + task.fileName}\" \"{Configer.baiduPanPath + task.remotePath}\" --norapid -p 1";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.CreateNoWindow = true; // 不显示窗口。
            proc.EnableRaisingEvents = true;
            proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                string stringResults = e.Data;
                if (stringResults == "" || stringResults == null) return;
                if (stringResults.Contains("上传文件失败"))
                    status = false;
                if (stringResults.Contains("上传文件成功"))
                    status = true;
                task.comments = stringResults;
                InfoLog.InfoPrintf($"BaiduPan: {stringResults}", InfoLog.InfoClass.上传系统信息);
                try
                {
                    double uploadSize = double.Parse(Regex.Replace(Regex.Match(stringResults, @"(?<=↑ ).*?(?=/)").Value, @"[^\d-.]|[-.](?!\d)", ""));
                    double allSize = double.Parse(Regex.Replace(Regex.Match(stringResults, @"(?<=/).*?(?= )").Value, @"[^\d-.]|[-.](?!\d)", ""));
                    int progress = (int)Math.Ceiling(uploadSize / allSize * 100);
                    task.progress = progress;
                }
                catch (FormatException)
                {
                    task.progress = -1;
                }
                
            };  // 捕捉的信息
            proc.Start();
            proc.BeginOutputReadLine();   // 开始异步读取
            proc.Exited += Process_Exited;
            proc.WaitForExit();
            proc.Close();
            GC.Collect();
            if (exitCode != 0 || !status)
                throw new UploadFailure("fail to upload");
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Process P = (Process)sender;
            exitCode = P.ExitCode;
        }
    }
}
