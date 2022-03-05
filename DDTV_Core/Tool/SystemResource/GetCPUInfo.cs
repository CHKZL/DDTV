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
        {   // bin/bash -c "awk '{u=$2+$4; t=$2+$4+$5; if (NR==1){u1=u; t1=t;} else print (u-u1) * 100 / (t-t1); }' <(grep 'cpu ' /proc/stat) <(sleep 1;grep 'cpu ' /proc/stat)"
            const string CPU_FILE_PATH = "/proc/stat";
            var cpu_file_info = System.IO.File.ReadAllLines(CPU_FILE_PATH);
            double[] L1 = Array.ConvertAll(cpu_file_info[0][5..].Split(" "), double.Parse);
            System.Threading.Thread.Sleep(1000);

            cpu_file_info = System.IO.File.ReadAllLines(CPU_FILE_PATH);
            double[] L2 = Array.ConvertAll(cpu_file_info[0][5..].Split(" "), double.Parse);
            double value = (L2[0] + L2[2] - L1[0] - L1[2]) * 100 / (L2[0] + L2[2] + L2[3] - L1[0] - L1[2] - L1[3]);
            if (@decimal >= 0) value = Math.Round(value, @decimal);
            return value;
        }
        /*
        public static double GetMacOS
        {
            // 听说Mac OS X下没/proc，可以用system_profiler (先放一个在这，到时需要米姐可以直接修)
        }
        */
    }
}
