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
                //下播钩子等调用方处于录制收尾流程：命令挂死会永久卡住该房间的收尾，必须有超时兜底
                const int timeoutMs = 60 * 1000;
                using Process process = new Process
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
                //先异步读stdout再等退出：若顺序ReadToEnd，进程挂死时连超时判断都到不了
                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { }
                    Core.LogModule.Log.Warn(nameof(Shell), $"Shell命令执行超过{timeoutMs / 1000}秒未退出，已强制结束:{Command}", null, false);
                }
                //进程退出或被Kill后stdout管道关闭，读取任务随即完成；极端情况Kill失败则最多再等5秒
                try
                {
                    if (stdoutTask.Wait(TimeSpan.FromSeconds(5)))
                        return stdoutTask.Result;
                }
                catch { }
                return string.Empty;
            }
            else
            {
                return "收到下载调度器提交的Shell执行请求，但是检测到Config.Core_RunConfig._Linux_Only_ShellSwitch为关闭状态，拒绝执行";
            }
        }
    }
}
