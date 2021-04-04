using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static Auxiliary.Upload.UploadTask;

namespace Auxiliary.Upload
{
    class OneDriveUpload
    {
        private string configFile { set; get; }
        /// <summary>
        /// 初始化OneDrive Upload
        /// </summary>
        public OneDriveUpload()
        {
            configFile = Uploader.oneDriveConfig;
        }
        /// <summary>
        /// 上传到OneDrive
        /// </summary>
        /// <param name="uploadInfo">传入上传信息</param>
        public void doUpload(UploadInfo uploadInfo)
        {
            Process proc = new Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                proc.StartInfo.FileName = @".\OneDriveUploader.exe"; //windows下的程序位置
            }
            else
            {
                proc.StartInfo.FileName = "OneDriveUploader";
            }
            DateTime showTime = DateTime.Now;
            proc.StartInfo.Arguments = $"-f -c \"{configFile}\" -s \"{uploadInfo.srcFile}\" -r \"{Uploader.oneDrivePath+uploadInfo.remotePath}\"";
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
                uploadInfo.status["OneDrive"].comments = System.Text.RegularExpressions.Regex.Replace(stringResults, @"(.*\[)(.*)(\].*)", "$2");
                if (DateTime.Now - showTime > new TimeSpan(0, 5, 0)) //5min更新一次log
                {
                    InfoLog.InfoPrintf($"Onedrive: {stringResults}", InfoLog.InfoClass.上传必要提示);
                }
                
            };  // 捕捉的信息

            proc.Start();
            proc.BeginOutputReadLine();   // 开始异步读取
            proc.Exited += Process_Exited;
            proc.WaitForExit();
            proc.Close();
            GC.Collect();
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Process P = (Process)sender;
            if (P.ExitCode != 0)
            {
                throw new OneDriveException("fail to upload");
            }
        }
    }
}