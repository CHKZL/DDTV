using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Core.Tools
{
    internal class Shell
    {
        public static string Run(string Command)
        {
            if (Config.Core_RunConfig._Linux_Only_ShellSwitch)
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
                return "收到下载调度器提交的Shell执行请求，但是检测到Config.Core_RunConfig._Linux_Only_ShellSwitch为关闭状态，拒绝执行";
            }
        }
    }
}
