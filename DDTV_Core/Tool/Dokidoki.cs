using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.ConfigModule;
using System.Runtime.InteropServices;

namespace DDTV_Core.Tool
{
    public class Dokidoki
    {
        public static void SendDokidoki(string Type, string Conut)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>()
            {
                {"Type",Type },
                {"Conut",Conut },
                {"CAID",InitDDTV_Core.ClientAID }
            };
            try
            {
                string Ver = SystemAssembly.NetworkRequestModule.Post.Post.HttpPost("http://api.ddtv.pro/api/Dokidoki", Parameters);
            }
            catch (Exception)
            {

            }
        }
        public static bool IsDoNotSleepState = false;
        public static bool DoNotSleepWhileDownloadingIsRun=false;
        /// <summary>
        /// 设置阻塞系统休眠模式
        /// </summary>
        /// <param name="Type">1：在有下载任务时阻塞休眠，下载任务为0时可以休眠（一旦休眠则无法被再次自动唤醒）   2：设置中打开阻止就一直阻止，一直跟随设置，直到设置关闭或者退出程序才会休眠（默认为模式2）</param>
        public static void DoNotSleepWhileDownloading(int Type =2)
        {
            //子线程未启动，并且环境是Windows的情况下
            if (!DoNotSleepWhileDownloadingIsRun && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DoNotSleepWhileDownloadingIsRun = !DoNotSleepWhileDownloadingIsRun;
                IsDoNotSleepState = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DoNotSleepWhileDownloading, "true", CoreConfigClass.Group.Download));
                Task.Run(() => {
                    while (true)
                    {
                        try
                        {
                            if (IsDoNotSleepState)
                            {
                                switch (Type)
                                {
                                    case 1:
                                        {
                                            #region 在有下载任务时阻塞休眠，下载任务为0时可以休眠
                                            foreach (var room in Rooms.RoomInfo)
                                            {
                                                if (room.Value.DownloadingList.Count > 0)
                                                {
                                                    //如果有下载中的任务，阻止系统休眠，但允许息屏
                                                    SystemResource.SystemSleep.PreventForCurrentThread(false);
                                                    break;
                                                }
                                            }
                                            //没检测到下载中的任务，操作系统现在可以正常进入睡眠状态和关闭屏幕
                                            SystemResource.SystemSleep.RestoreForCurrentThread();
                                            #endregion
                                        }
                                        break;
                                    case 2:
                                        {
                                            #region 设置中打开阻止就一直阻止，一直跟随设置，直到设置关闭或者退出程序才会休眠
                                            //设置打开，阻止系统休眠，但允许息屏
                                            SystemResource.SystemSleep.PreventForCurrentThread(false);
                                            #endregion
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                //设置关闭，恢复休眠
                                SystemResource.SystemSleep.RestoreForCurrentThread();
                            }
                            Thread.Sleep(60 * 1000);
                        }
                        catch (Exception e)
                        {
                            SystemAssembly.Log.Log.AddLog("DoNotSleepWhileDownloading", SystemAssembly.Log.LogClass.LogType.Error, "修改系统休眠阻塞出现未知错误", false, e, false);
                            Thread.Sleep(30 * 1000);
                        }

                    }
                });
            }
        }
    }
}
