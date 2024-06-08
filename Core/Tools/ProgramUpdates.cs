using AngleSharp.Dom;
using Core.LogModule;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Core.LogModule.OperationQueue;
using static System.Net.Mime.MediaTypeNames;

namespace Core.Tools
{
    public class ProgramUpdates
    {
        public static string Url = "https://ddtv-update.top";
        private static string verFile = "./ver.ini";
        private static string type = string.Empty;
        private static string ver = string.Empty;
        public static bool Isdev = false;
        public static event EventHandler<EventArgs> NewVersionAvailableEvent;//检测到新版本

        public static async void RegularInspection(object state)
        {
            await CheckForNewVersions();
        }

        internal static bool GetCurrentVersion()
        {
            if (File.Exists(verFile))
            {
                string[] Ver = File.ReadAllLines(verFile);
                foreach (string VerItem in Ver)
                {
                    if (VerItem.StartsWith("type="))
                        type = VerItem.Split('=')[1].TrimEnd();
                    if (VerItem.StartsWith("ver="))
                        ver = VerItem.Split('=')[1].TrimEnd();
                }
                if (ver.ToLower().StartsWith("dev"))
                {
                    Isdev = true;
                    ver = ver.ToLower().Replace("dev", "");
                }
                else
                {
                    ver = ver.ToLower().Replace("release", "");
                }
                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(ver))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检查有没有新版本
        /// </summary>
        /// <param name="AutoUpdate">是否唤起自动更新</param>
        /// <param name="Manual">是否为手动检查更新</param>
        /// <returns></returns>
        public static async Task<bool> CheckForNewVersions(bool AutoUpdate = false, bool Manual = false)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!GetCurrentVersion())
                    {
                        return false;
                    }
                    string DL_VerFileUrl = $"{Url}/{type}/{(Isdev ? "dev" : "release")}/ver.ini";
                    string R_Ver = Get(DL_VerFileUrl).TrimEnd().Replace("dev", "").Replace("release", "");
                    if (!string.IsNullOrEmpty(R_Ver) && R_Ver.Split('.').Length > 0)
                    {
                        //老版本
                        Version Before = new Version(ver);
                        //新版本
                        Version After = new Version(R_Ver);
                        if (After > Before)
                        {
                            if (!Manual)
                                NewVersionAvailableEvent?.Invoke(R_Ver, new EventArgs());
                            if (AutoUpdate)
                                CallUpUpdateProgram();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Log.Info(nameof(ProgramUpdates), $"获取新版本失败，请检查网络和代理状况");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(ProgramUpdates), $"获取新版本出现错误，错误堆栈:{ex.ToString()}", ex);
                    return false;
                }
            });
        }
        public static void CallUpUpdateProgram()
        {
            if (File.Exists("./Update/Update.exe"))
            {
                Process process = new Process();
                process.StartInfo.FileName = "./Update/Update.exe";
                process.Start();
                Environment.Exit(-114514);
            }
            else
            {
                Log.Error(nameof(ProgramUpdates), $"找不到自动更新脚本程序DDTV_Update.exe");
            }
        }
        internal static string Get(string URL)
        {
            HttpClient _httpClient = new HttpClient();
            bool A = false;
            string str = string.Empty;
            do
            {
                if (A)
                    Thread.Sleep(1000);
                if (!A)
                    A = true;

                _httpClient.DefaultRequestHeaders.Referrer = new Uri("http://ddtv-update");
                str = _httpClient.GetStringAsync(URL).Result;
            } while (string.IsNullOrEmpty(str));
            return str;
        }
    }
}
