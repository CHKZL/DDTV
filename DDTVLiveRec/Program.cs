using Auxiliary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DDTVLiveRec
{
    class Program
    {
        static void Main(string[] args)
        {
            Auxiliary.VTBS.API.VTBS服务器CDN.根据CDN更新VTBS_Url();
            MMPU.配置文件初始化(1);
            new Task(() =>
            {
                DDTVLiveRecWebServer.Program.Main(new string[] { });
            }).Start();
            new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        string 服务器版本号 = MMPU.TcpSend(Server.RequestCode.GET_VER, "{}", true, 50);
                        if (!string.IsNullOrEmpty(服务器版本号))
                        {
                            bool 检测状态 = true;
                            foreach (var item in MMPU.不检测的版本号)
                            {
                                if (服务器版本号 == item)
                                {
                                    检测状态 = false;
                                }
                            }
                            if (MMPU.版本号 != 服务器版本号 && 检测状态)
                            {
                                MMPU.更新公告 = MMPU.TcpSend(Server.RequestCode.GET_UPDATE_ANNOUNCEMENT, "{}", true, 100);
                                MMPU.是否有新版本 = true;
                                InfoLog.InfoPrintf("检测到版本更新,更新内容:\n" + MMPU.TcpSend(Server.RequestCode.GET_VERTEXT, "{}", true, 100) + "\n\n", InfoLog.InfoClass.下载必要提示);
                            }
                        }
                    }
                    catch (Exception) { }
                    Thread.Sleep(3600*1000);
                }
            }).Start();
            MMPU.缓存路径 = MMPU.下载储存目录;
            InfoLog.InfoPrintf(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + "DDTVLiveRec启动完成", InfoLog.InfoClass.下载必要提示);
            while (true)
            {
                Thread.Sleep(10);

            }
        }
    }
}
