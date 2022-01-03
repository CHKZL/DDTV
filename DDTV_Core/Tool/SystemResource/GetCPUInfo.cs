using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.SystemResource
{
    public class GetCPUInfo
    {
        public static double GetWindows()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
                var cpuTimes = searcher.Get()
                    .Cast<ManagementObject>()
                    .Select(mo => new
                    {
                        Name = mo["Name"],
                        Usage = mo["PercentProcessorTime"]
                    }
                    )
                    .ToList();
                var query = cpuTimes.Where(x => x.Name.ToString() == "_Total").Select(x => x.Usage);
                //cpuTimes.Clear();
                searcher.Dispose();
                return double.Parse(query.SingleOrDefault().ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static double GetLinux(int @decimal = 2)
        {
            string result = RunCommandAsync("awk '{u=$2+$4; t=$2+$4+$5; if (NR==1){u1=u; t1=t;} else print ($2+$4-u1) * 100 / (t-t1); }' <(grep 'cpu ' /proc/stat) <(sleep 1;grep 'cpu ' /proc/stat)");
            double value = double.Parse(result);
            if (@decimal >= 0) value = Math.Round(value, @decimal);
            return value;
        }
        private static string RunCommandAsync(string command)
        {
            string escapedCommand = command.Replace("\"", "\\\"");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedCommand}\"",
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
    }
}
