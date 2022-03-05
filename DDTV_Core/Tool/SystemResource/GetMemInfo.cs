using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.SystemResource
{
    public class GetMemInfo
    {
        public static MemInfo GetWindows()
        {
            MemInfo memInfo = new MemInfo();
            {
                long capacity = 0;
                ManagementClass cimobject1 = new ManagementClass("Win32_PhysicalMemory");
                ManagementObjectCollection moc1 = cimobject1.GetInstances();
                foreach (ManagementObject mo1 in moc1)
                {
                    capacity += long.Parse(mo1.Properties["Capacity"].Value.ToString());
                }
                moc1.Dispose();
                cimobject1.Dispose();
                memInfo.Total = capacity;
            }
            {
                long totalCapacity = 0;
                ObjectQuery objectQuery = new ObjectQuery("select * from Win32_PerfRawData_PerfOS_Memory");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(objectQuery);
                ManagementObjectCollection vals = searcher.Get();
                foreach (ManagementObject val in vals)
                {
                    totalCapacity += Convert.ToInt64(val.GetPropertyValue("Availablebytes"));
                }
                memInfo.Available = totalCapacity;
            }

            return memInfo;
        }
        public static MemInfo GetLiunx()
        {
            MemInfo memInfo = new MemInfo();
            const string MEM_FILE_PATH = "/proc/meminfo";
            var mem_file_info = System.IO.File.ReadAllText(MEM_FILE_PATH);
            var lines = mem_file_info.Split(new[] { '\n' });
            mem_file_info = string.Empty;

            int count = 0;
            foreach (var item in lines)
            {
                if (item.StartsWith("MemTotal:"))
                {
                    count++;
                    var tt = item.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tt[1].Trim().Split(' ')[0] is null)
                    {
                        memInfo.Total = -1;
                    }
                    else
                    {
                        memInfo.Total += long.Parse(tt[1].Trim().Split(' ')[0]);
                    }
                }
                else if (item.StartsWith("MemAvailable:"))
                {
                    count++;
                    var tt = item.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tt[1].Trim().Split(' ')[0] is null)
                    {
                        memInfo.Available = -1;
                    }
                    else
                    {
                        memInfo.Available += long.Parse(tt[1].Trim().Split(' ')[0]);
                    }
                }
                if (count >= 2) break;
            }
            return memInfo;
        }
        public class MemInfo
        {
            /// <summary>
            /// 总计内存大小
            /// </summary>
            public long Total { get; set; }
            /// <summary>
            /// 可用内存大小
            /// </summary>
            public long Available { get; set; }
        }
    }
}
