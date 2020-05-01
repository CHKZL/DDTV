using Auxiliary;
using System;
using System.IO;
using System.Threading;


namespace DDTVLiveRec
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
           下载("14275133");

#else
            if(!MMPU.配置文件初始化(1))
            {
                InfoLog.InfoPrintf("\r\nB站账号登陆信息过期或无效,启动失败，请更新BiliUser.ini文件", InfoLog.InfoClass.下载必要提示);
                Console.WriteLine("\n\n按任意键退出.............");
                Console.ReadKey();   
                return;
            }
#endif
            new Thread(new ThreadStart(delegate
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
            })).Start();
            MMPU.下载储存目录 = "./tmp/";
            InfoLog.InfoPrintf(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + "DDTVLiveRec启动", InfoLog.InfoClass.下载必要提示);
            DDTVLiveRecWebServer.Program.Main(new string[] { });
            while (true)
            {
                Thread.Sleep(10);
            }
        }
        public static void 测试下载(string RoomId)
        {
            if (!bilibili.根据房间号获取房间信息.是否正在直播(RoomId))
            {
                // System.Windows.MessageBox.Show("该房间当前未直播");
                return;
            }
            string GUID = Guid.NewGuid().ToString();
            string 标题 = bilibili.根据房间号获取房间信息.获取标题(RoomId);
            string 下载地址 = string.Empty;
            try
            {
                下载地址 = bilibili.根据房间号获取房间信息.下载地址(RoomId);
            }
            catch (Exception)
            {
                //System.Windows.MessageBox.Show("获取下载地址失败");
                return;
            }
            Downloader.新建下载对象("bilibili", RoomId, 标题, GUID, 下载地址, "手动下载任务", true, "TEST", false, null);
        }
        
    }
}
