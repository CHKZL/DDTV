using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Auxiliary.Upload.Service
{
    class OneDrive : ServiceInterface
    {
        private string configFile { set; get; }
        private int exitCode;
        /// <summary>
        /// 初始化OneDrive Upload
        /// </summary>
        public OneDrive()
        {
            configFile = Configer.oneDriveConfig;
        }
        /// <summary>
        /// 上传到OneDrive
        /// </summary>
        /// <param name="uploadInfo">传入上传信息</param>
        public void doUpload(Info.TaskInfo task)
        {
            string srcFile = task.localPath + task.fileName;
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
            proc.StartInfo.Arguments = $"-f -c \"{configFile}\" -s \"{srcFile}\" -r \"{Configer.oneDrivePath+task.remotePath}\"";
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
                string comments = Regex.Replace(stringResults, @"(.*\[)(.*)(\].*)", "$2");
                task.comments = comments;
                InfoLog.InfoPrintf($"Onedrive: {stringResults}", InfoLog.InfoClass.上传系统信息);
                string RegexStr = @"\d+(\.\d+)?%";
                Match mt = Regex.Match(comments, RegexStr);
                string progress = mt.Value.Replace("%", "");
                task.progress = int.Parse((progress == "") ? "-1" : progress);
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