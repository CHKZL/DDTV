using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class SystemResource
    {
        public class GetHDDInfo
        {
            public static List<HDDInfo> GetWindows(string DriveLetter)
            {

                HDDInfo windowsHDDInfo = new HDDInfo();
                ManagementObject disk = new ManagementObject(
                    "win32_logicaldisk.deviceid=\"" + DriveLetter + ":\"");
                disk.Get();
                windowsHDDInfo.Avail = Linq.ConversionSize(long.Parse(disk["FreeSpace"].ToString()));
                windowsHDDInfo.Size = Linq.ConversionSize(long.Parse(disk["Size"].ToString()));
                windowsHDDInfo.FileSystem = DriveLetter + ":";
                windowsHDDInfo.MountPath = DriveLetter + ":";
                windowsHDDInfo.Usage = Linq.ConversionSize(long.Parse(disk["Size"].ToString()) - long.Parse(disk["FreeSpace"].ToString()));
                windowsHDDInfo.Used = ((1.0 - double.Parse(disk["FreeSpace"].ToString()) / double.Parse(disk["Size"].ToString())) * 100).ToString().Split('.')[0] + "%";
                return new List<HDDInfo>() { windowsHDDInfo };
            }
            public static List<HDDInfo> GetLinux()
            {
                List<HDDInfo> hdd = new List<HDDInfo>();
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo("df", "-h")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
                var hddInfo = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                process.Dispose();
                string[] B2 = hddInfo.Split('\n');
                for (int i = 1; i < B2.Length; i++)
                {
                    string[] B3 = B2[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (B3.Length > 5)
                    {
                        try
                        {
                            hdd.Add(new HDDInfo()
                            {
                                FileSystem = B3[0],
                                Size = B3[1],
                                Usage = B3[2],
                                Avail = B3[3],
                                Used = B3[4],
                                MountPath = B3[5]
                            });
                        }
                        catch (Exception e)
                        {
                            Log.Error(nameof(GetHDDInfo), "GetLinux硬盘信息出现错误", e, false);
                        }
                    }

                }
                return hdd;
            }
            public class HDDInfo
            {
                /// <summary>
                /// 注册路径
                /// </summary>
                public string FileSystem { set; get; }
                /// <summary>
                /// 硬盘大小
                /// </summary>
                public string Size { get; set; }

                /// <summary>
                /// 使用率
                /// </summary>
                public string Used { get; set; }

                /// <summary>
                /// 可用大小
                /// </summary>
                public string Avail { get; set; }

                /// <summary>
                /// 已使用大小
                /// </summary>
                public string Usage { get; set; }
                /// <summary>
                /// 挂载路径
                /// </summary>
                public string MountPath { set; get; }
            }
        }

        public class GetMemInfo
        {
            public static MemInfo GetWindows()
            {
                MemInfo memInfo = new MemInfo();

                // Get total physical memory capacity
                long totalCapacity = GetTotalPhysicalMemoryCapacityAsync();
                memInfo.Total = totalCapacity;

                // Get available memory
                long availableCapacity = GetAvailableMemoryCapacityAsync();
                memInfo.Available = availableCapacity;

                return memInfo;
            }
            private static long GetTotalPhysicalMemoryCapacityAsync()
            {
                long totalCapacity = 0;
                using (var cimobject1 = new ManagementClass("Win32_PhysicalMemory"))
                {
                    var moc1 = cimobject1.GetInstances();
                    foreach (ManagementObject mo1 in moc1)
                    {
                        totalCapacity += long.Parse(mo1.Properties["Capacity"].Value.ToString());
                    }
                }
                return totalCapacity;
            }

            private static long GetAvailableMemoryCapacityAsync()
            {
                long availableCapacity = 0;
                var objectQuery = new ObjectQuery("select * from Win32_PerfRawData_PerfOS_Memory");
                using (var searcher = new ManagementObjectSearcher(objectQuery))
                {
                    var vals = searcher.Get();
                    foreach (ManagementObject val in vals)
                    {
                        availableCapacity += Convert.ToInt64(val.GetPropertyValue("Availablebytes"));
                    }
                }
                return availableCapacity;
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
                if (memInfo.Total > 0)
                    memInfo.Total = memInfo.Total * 1024;
                if (memInfo.Available > 0)
                    memInfo.Available = memInfo.Available * 1024;
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


        public class GetProcess
        {
            /// <summary>
            /// 获取父进程状态
            /// </summary>
            /// <param name="Id">要查询的子进程ID</param>
            /// <returns></returns>
            public static int GetParentProcess(int Id)
            {
                var query = new SelectQuery($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {Id}");
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        return Convert.ToInt32(mo["ParentProcessId"]);
                    }
                }
                return 0;
            }
        }

    }
}
