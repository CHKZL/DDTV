using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.Tool.ServerMessageClass;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    public class ServerInteraction
    {
        public class CheckUpdates
        {
            private static bool IsChecked = false;
            private static bool Is = false;
            public static void Update(string Type)
            {
                if (!Is)
                {
                    Task.Run(() =>
                    {
                        Is = true;
                        while (true)
                        {
                            try
                            {
                                if (IsChecked || DDTV_Update.ComparisonVersion(Type, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                                {
                                    IsChecked = true;
                                    if(false && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && CoreConfig.AutoInsallUpdate)
                                    {
                                        if (File.Exists("./DDTV_Update.exe"))
                                        {
                                            bool IsDL = false;
                                            foreach (var A1 in Rooms.RoomInfo)
                                            {
                                                if (A1.Value.DownloadingList.Count > 0)
                                                {
                                                    IsDL = true;
                                                }
                                            }
                                            if (IsDL)
                                            {
                                                Log.AddLog(nameof(CheckUpdates), LogClass.LogType.Info, $"DDTV检测到更新，但是当前有录制任务正在进行中，等待任务结束后空闲时间会自动更新", false, null, false);
                                            }
                                            else
                                            {

                                                Log.AddLog(nameof(CheckUpdates), LogClass.LogType.Info, $"触发自动更新", false, null, false);
                                                Process process = new Process();
                                                process.StartInfo.FileName = "./DDTV_Update.exe";
                                                process.StartInfo.Arguments += " autoupdate";
                                                if (CoreConfig.IsDev)
                                                {
                                                    process.StartInfo.Arguments += " dev";
                                                }
                                                process.Start();
                                                Environment.Exit(0);
                                            }
                                        }
                                        else
                                        {
                                            Log.AddLog(nameof(CheckUpdates), LogClass.LogType.Info, $"找不到自动更新脚本程序DDTV_Update.exe", false, null, false);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("\n==========检测到新版本==========\n请退出本程序后运行目录中的[DDTV_Update]进行更新:\nWindows请退出后直接运行DDTV_Update.exe\nLinux环境请使用dotnet DDTV_Update.dll运行更新程序\n[10分钟后再次提示]\n==========检测到新版本==========\n");
                                    }
                                }

                            }
                            catch (Exception)
                            {
                            }
                            Thread.Sleep(60 * 30 * 1000);
                        }
                    });
                }
            }
        }

        public class UpdateNotice
        {
            private static bool Is = false;
            private static int i = 0;
            public static void Start(string Type)
            {
                if (!Is)
                {
                    Is = true;
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                string N = DDTV_Core.Tool.Notice.GetNotice(Type);
                                MessageBase.pack<MessageClass.NoticeText> pack = JsonConvert.DeserializeObject<MessageBase.pack<MessageClass.NoticeText>>(N);
                                if (!string.IsNullOrEmpty(pack.data.Text))
                                {
                                    DDTV_Core.InitDDTV_Core.UpdateNotice = pack.data.Text;
                                }
                                Thread.Sleep(3600 * 1000);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    });
                }
            }
        }

        public class Dokidoki
        {
            private static bool Is = false;
            private static int i = 0;
            public static void Start(string Type)
            {
                if (!Is)
                {
                    Is = true;
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                DDTV_Core.Tool.Dokidoki.SendDokidoki(Type, i.ToString());
                                i++;
                                Thread.Sleep(3600 * 1000);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    });
                }
            }
        }
    }
}
