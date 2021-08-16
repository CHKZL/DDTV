using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static Auxiliary.Upload.UploadTask;

namespace Auxiliary.Upload
{
    class BaiduPanUpload
    {
        private string configFile { set; get; }
        private int exitCode;
        /// <summary>
        /// 初始化BaiduPan Upload
        /// </summary>
        public BaiduPanUpload()
        { }

        /// <summary>
        /// 上传到BaiduPan
        /// </summary>
        /// <param name="uploadInfo">传入上传信息</param>
        public void doUpload(UploadInfo uploadInfo)
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
            proc.StartInfo.Arguments = $"u \"{uploadInfo.srcFile}\" \"{Uploader.baiduPanPath + uploadInfo.remotePath}\" --norapid -p 1";
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
                uploadInfo.status["BaiduPan"].comments = stringResults;
                InfoLog.InfoPrintf($"BaiduPan: {stringResults}", InfoLog.InfoClass.上传必要提示);
                double uploadSize = double.Parse(Regex.Replace(Regex.Match(comments, @"(?<=↑ ).*?(?=/)").Value, @"[^\d-.]|[-.](?!\d)", ""));
                double allSize = double.Parse(Regex.Replace(Regex.Match(comments, @"(?<=/).*?(?= )").Value, @"[^\d-.]|[-.](?!\d)", ""));
                int progress = (int)Math.Ceiling(uploadSize / allSize * 100);
                uploadInfo.status["BaiduPan"].progress = progress;
            };  // 捕捉的信息
            proc.Start();
            proc.BeginOutputReadLine();   // 开始异步读取
            proc.Exited += Process_Exited;
            proc.WaitForExit();
            proc.Close();
            GC.Collect();
            if (exitCode != 0)
                throw new UploadFailure("fail to upload");
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Process P = (Process)sender;
            exitCode = P.ExitCode;
        }
    }
}
