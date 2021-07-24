using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class system_info
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_info");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>(鉴权结果, null);
            }
            else
            {
                int 下载中 = 0, 下载完成 = 0;
                foreach (var item in Auxiliary.MMPU.DownList)
                {
                    if (item.DownIofo.下载状态)
                        下载中++;
                    else
                        下载完成++;
                }
                var systemInfo = new SystemInfo()
                {
                    DDTVCore_Ver = Auxiliary.MMPU.版本号,
                    Room_Quantity = Auxiliary.bilibili.RoomList.Count,
                    OS_Info = new OS_Info()
                    {
                        Associated_Users = Environment.UserName,
                        OS_Ver = RuntimeInformation.OSDescription,
                        OS_Tpye = RuntimeInformation.OSArchitecture.ToString(),
                        Memory_Usage = Process.GetCurrentProcess().WorkingSet64,
                        Runtime_Ver = RuntimeInformation.ProcessArchitecture.ToString(),
                        UserInteractive = Environment.UserInteractive,
                        Current_Directory = Environment.CurrentDirectory,
                        WebCore_Ver = Environment.Version.ToString(),
                        AppCore_Ver = RuntimeInformation.FrameworkDescription

                    },
                    Download_Info = new Download_Info()
                    {
                        Downloading = 下载中,
                        Completed_Downloads = 下载完成
                    },
                    Ver_Info = new Ver_Info()
                    {
                        IsNewVer = Auxiliary.MMPU.是否有新版本,
                        NewVer= Auxiliary.MMPU.是否有新版本 ? Auxiliary.MMPU.检测到的新版本号 : null,
                        Update_Log = Auxiliary.MMPU.是否有新版本 ? Auxiliary.MMPU.更新公告 : null,
                    }
                };
                return ReturnInfoPackage.InfoPkak(鉴权结果, new List<SystemInfo>(){ systemInfo });
            }
          
        }

        private class Messge : ReturnInfoPackage.Messge<SystemInfo>
        {
            public static new List<SystemInfo> Package { set; get; }

        }
        public class SystemInfo
        {
            /// <summary>
            /// 当前DDTV版本号
            /// </summary>
            public string DDTVCore_Ver { get; set; }
            /// <summary>
            /// 监控房间数量
            /// </summary>
            public int Room_Quantity { get; set; }
            /// <summary>
            /// 操作系统相关信息
            /// </summary>
            public OS_Info OS_Info { get; set; }
            /// <summary>
            /// 下载任务基础信息
            /// </summary>
            public Download_Info Download_Info { get; set; }
            /// <summary>
            /// DDTV更新信息
            /// </summary>
            public Ver_Info Ver_Info { get; set; }
        }
        public class OS_Info
        {
            /// <summary>
            /// 系统版本
            /// </summary>
            public string OS_Ver { get; set; }
            /// <summary>
            /// 系统类型
            /// </summary>
            public string OS_Tpye { get; set; }
            /// <summary>
            /// 使用内存量，单位bit
            /// </summary>
            public long Memory_Usage { get; set; }
            /// <summary>
            /// 运行时版本
            /// </summary>
            public string Runtime_Ver { get; set; }
            /// <summary>
            /// 是否在交互模式下
            /// </summary>
            public bool UserInteractive { get; set; }
            /// <summary>
            /// 关联的用户
            /// </summary>
            public string Associated_Users { get; set; }
            /// <summary>
            /// 工作目录
            /// </summary>
            public string Current_Directory { get; set; }
            /// <summary>
            /// Core程序核心框架版本
            /// </summary>
            public string AppCore_Ver { set; get; }
            /// <summary>
            /// Web程序核心框架版本
            /// </summary>
            public string WebCore_Ver { set; get; }
        }
        public class Download_Info
        {
            /// <summary>
            /// 下载中的任务数
            /// </summary>
            public int Downloading { get; set; }
            /// <summary>
            /// 下载结束的任务数
            /// </summary>
            public int Completed_Downloads { get; set; }
        }
        public class Ver_Info
        {
            /// <summary>
            /// 是否有新版本
            /// </summary>
            public bool IsNewVer { set; get; }
            /// <summary>
            /// 新版本号
            /// </summary>
            public string NewVer { get; set; }
            /// <summary>
            /// 更新日志
            /// </summary>
            public string Update_Log { get; set; }
        }
    }
}
