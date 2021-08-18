using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 消息_获取系统资源信息
    {
        
        public static string 获取系统资源情况()
        {
            System_Core.SystemResourceMonitoring systemResourceMonitoring = new System_Core.SystemResourceMonitoring();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                systemResourceMonitoring.CPU_usage = CPU.Liunx获取CPU使用率();
                systemResourceMonitoring.DDTV_use_memory = Environment.WorkingSet;
                systemResourceMonitoring.Platform = "Linux";
                systemResourceMonitoring.Available_memory = 内存.Liunx读取内存信息().Available * 1024;
                systemResourceMonitoring.Total_memory = 内存.Liunx读取内存信息().Total * 1024;
                systemResourceMonitoring.HDDInfo = 硬盘.linux获取硬盘信息();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                systemResourceMonitoring.CPU_usage = CPU.WindowsCPU使用率获取();
                systemResourceMonitoring.DDTV_use_memory = Environment.WorkingSet;
                systemResourceMonitoring.Platform = "Windows";
                systemResourceMonitoring.Available_memory = 内存.Windows获取内存信息().Available;
                systemResourceMonitoring.Total_memory = 内存.Windows获取内存信息().Total;
                string 盘符 = "C";
                if(MMPU.下载储存目录.Contains("./tmp"))
                {
                    盘符 = Environment.CurrentDirectory.Split(':')[0];
                }
                else
                {
                    盘符 = Environment.CurrentDirectory + MMPU.下载储存目录;
                }
                systemResourceMonitoring.HDDInfo =new List<硬盘.HDDInfo>(){ 硬盘.Windows获取硬盘信息(盘符) };
            }
            return ReturnInfoPackage.InfoPkak((int)MessageClass.ServerSendMessageCode.请求成功, new List<System_Core.SystemResourceMonitoring>() { systemResourceMonitoring });
        }
        public class 硬盘
        {
            public static HDDInfo Windows获取硬盘信息(string 盘符)
            {
                HDDInfo windowsHDDInfo = new HDDInfo();
                ManagementObject disk = new ManagementObject(
                    "win32_logicaldisk.deviceid=\"" + 盘符 + ":\"");
                disk.Get();
                windowsHDDInfo.Avail = Downloader.转换下载大小数据格式(long.Parse(disk["FreeSpace"].ToString()));
                windowsHDDInfo.Size = Downloader.转换下载大小数据格式(long.Parse(disk["Size"].ToString()));
                windowsHDDInfo.FileSystem = 盘符+":";
                windowsHDDInfo.MountPath = 盘符 + ":";
                windowsHDDInfo.Usage = ((1.0 - double.Parse(disk["FreeSpace"].ToString()) / double.Parse(disk["Size"].ToString())) * 100).ToString().Split('.')[0] + "%";
                windowsHDDInfo.Used = Downloader.转换下载大小数据格式(long.Parse(disk["Size"].ToString()) - long.Parse(disk["FreeSpace"].ToString()));
                return windowsHDDInfo;
            }
            public static List<HDDInfo> linux获取硬盘信息()
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
                if (MMPU.调试模式)
                    Console.WriteLine("硬盘信息获取到"+hddInfo);
                string[] B2 = hddInfo.Split('\n');
                if (MMPU.调试模式)
                    Console.WriteLine($"硬盘信息B2用\\n分割获得长度:{B2.Length}");
                for (int i = 1; i < B2.Length; i++)
                {
                    string[] B3 = B2[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if(B3.Length>5)
                    {
                        if (MMPU.调试模式)
                            Console.WriteLine($"硬盘信息B3分割获长度为:{B3.Length}的数据库{B3[0]}{B3[1]}{B3[2]}{B3[3]}{B3[4]}{B3[5]}");
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
                        catch (Exception)
                        {
                            if (MMPU.调试模式)
                                Console.WriteLine($"硬盘信息B3分割获添加到HDDInfo中发生了错误");
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
                /// 已使用大小
                /// </summary>
                public string Used { get; set; }

                /// <summary>
                /// 可用大小
                /// </summary>
                public string Avail { get; set; }

                /// <summary>
                /// 使用率
                /// </summary>
                public string Usage { get; set; }
                /// <summary>
                /// 挂载路径
                /// </summary>
                public string MountPath { set; get; }
            }
        }
        public class 内存
        {
            public static MemInfo Windows获取内存信息()
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
                        totalCapacity += System.Convert.ToInt64(val.GetPropertyValue("Availablebytes"));
                    }
                    memInfo.Available = totalCapacity;
                }

                return memInfo;
            }
            public static MemInfo Liunx读取内存信息()
            {
                MemInfo memInfo = new MemInfo();
                const string CPU_FILE_PATH = "/proc/meminfo";
                var mem_file_info = System.IO.File.ReadAllText(CPU_FILE_PATH);
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
        public class CPU
        {
            public static double WindowsCPU使用率获取()
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
            public static double Liunx获取CPU使用率(int 保留几位小数 = 2)
            {
                string result = RunCommandAsync("awk '{u=$2+$4; t=$2+$4+$5; if (NR==1){u1=u; t1=t;} else print ($2+$4-u1) * 100 / (t-t1); }' <(grep 'cpu ' /proc/stat) <(sleep 1;grep 'cpu ' /proc/stat)");
                double value = double.Parse(result);
                if (保留几位小数 >= 0) value = Math.Round(value, 保留几位小数);
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
}
