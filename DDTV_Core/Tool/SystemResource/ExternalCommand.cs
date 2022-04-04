using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.SystemResource
{
    public class ExternalCommand
    {
        public static string Shell(string Command)
        {
            if (SystemAssembly.ConfigModule.CoreConfig.Shell)
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{Command}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result;
            }
            else
            {
                return "收到下载调度器提交的Shell执行请求，但是检测到SystemAssembly.ConfigModule.CoreConfig.Shell为关闭状态，拒绝执行";
            }
        }
    }
}
