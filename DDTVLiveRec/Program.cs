using Auxiliary;
using System;
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
#if false
           下载("14275133");

#else
            string B = Auxiliary.VTBS.API.VTBS服务器CDN.根据CDN更新VTBS_Url();
            MMPU.配置文件初始化(1);
            new Task(() => {
                DDTVLiveRecWebServer.Program.Main(new string[] { });
            }).Start();
            
            
#endif
            new Task(() => 
            {
                try
                {
                    string 服务器版本号 = MMPU.TcpSend(20013, "{}", true);
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
                            InfoLog.InfoPrintf("检测到版本更新,更新公告:\n" + MMPU.TcpSend(20014, "{}", true) + "\n\n", InfoLog.InfoClass.下载必要提示);
                            //Console.ReadKey();
                        }
                    }
                }
                catch (Exception) { }
            }).Start();
            MMPU.下载储存目录 = MMPU.缓存路径;
            InfoLog.InfoPrintf(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + "DDTVLiveRec启动完成", InfoLog.InfoClass.下载必要提示);       
            while (true)
            {
                Thread.Sleep(10);
            }
        }
    }
}
