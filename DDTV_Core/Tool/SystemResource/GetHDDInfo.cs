using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.SystemResource
{
    public class GetHDDInfo
    {
        public static List<HDDInfo> GetWindows(string DriveLetter)
        {
            
            HDDInfo windowsHDDInfo = new HDDInfo();
            ManagementObject disk = new ManagementObject(
                "win32_logicaldisk.deviceid=\"" + DriveLetter + ":\"");
            disk.Get();
            windowsHDDInfo.Avail = SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(long.Parse(disk["FreeSpace"].ToString()));
            windowsHDDInfo.Size = SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(long.Parse(disk["Size"].ToString()));
            windowsHDDInfo.FileSystem = DriveLetter + ":";
            windowsHDDInfo.MountPath = DriveLetter + ":";
            windowsHDDInfo.Usage = SystemAssembly.NetworkRequestModule.NetClass.ConversionSize(long.Parse(disk["Size"].ToString()) - long.Parse(disk["FreeSpace"].ToString()));
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
                        SystemAssembly.Log.Log.AddLog(nameof(GetHDDInfo), SystemAssembly.Log.LogClass.LogType.Warn, "GetLinux硬盘信息出现错误!");
                    }
                }

            }
            return hdd;
        }
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
