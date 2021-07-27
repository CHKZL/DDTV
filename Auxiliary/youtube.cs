using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    /// <summary>
    /// youtube录制相关底层依赖
    /// </summary>
    public class youtube
    {
        public static List<RoomInit.RoomInfo> RoomList = new List<RoomInit.RoomInfo>();
        public static void start()
        {
            Task.Run(async () =>
            {
                InfoLog.InfoPrintf("启动youtube频道信息本地缓存更新线程", InfoLog.InfoClass.Debug);
                while (true)
                {
                    try
                    {
                        周期更新youtube频道状态();
                        await Task.Delay(MMPU.直播更新时间 * 1000);
                    }
                    catch (Exception e)
                    {
                        InfoLog.InfoPrintf("youtube频道信息本地缓存更新出现错误:" + e.ToString(), InfoLog.InfoClass.Debug);
                    }


                }
            });
        }
        private static void 周期更新youtube频道状态()
        {
            int a = 0;
            InfoLog.InfoPrintf("本地房间状态缓存更新开始", InfoLog.InfoClass.Debug);
            foreach (var roomtask in RoomList)
            {
                if(MMPU.连接404使能)
                {
                    RoomInit.RoomInfo A = GetRoomInfo(roomtask.房间号);
                    if (A != null)
                    {
                        for (int i = 0; i < RoomList.Count(); i++)
                        {
                            if (RoomList[i].房间号 == A.房间号)
                            {
                                RoomList[i].平台 = A.平台;
                                RoomList[i].标题 = A.标题;
                                RoomList[i].UID = A.UID;
                                RoomList[i].直播开始时间 = A.直播开始时间;
                                RoomList[i].直播状态 = A.直播状态;
                                RoomList[i].youtubeVideoId = A.youtubeVideoId;
                                if (A.直播状态)
                                    a++;
                                break;
                            }
                        }
                    }
                }
            }
            InfoLog.InfoPrintf("本地房间状态更新结束", InfoLog.InfoClass.Debug);
        }
        public static void 使用youtubeDl下载(string url)
        {
            Process process = new Process();

            process.StartInfo.FileName = "./libffmpeg/youtube-dl.exe";  // 这里也可以指定ffmpeg的绝对路径
            process.StartInfo.Arguments = " "+ url+ " --ffmpeg-location ./libffmpeg/ffmpeg.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += new DataReceivedEventHandler(Output);  // 捕捉ffmpeg.exe的错误信息
            process.OutputDataReceived += new DataReceivedEventHandler(Output);  // 捕捉ffmpeg.exe的错误信息
           
            DateTime beginTime = DateTime.Now;

            process.Start();
            process.BeginErrorReadLine();   // 开始异步读取
            process.Exited += Process_Exited;
            GC.Collect();
        }
        private static void Process_Exited(object sender, EventArgs e)
        {
            Process P = (Process)sender;
            InfoLog.InfoPrintf("转码任务完成:" + P.StartInfo.Arguments, InfoLog.InfoClass.下载系统信息);
        }
        private static void Output(object sender, DataReceivedEventArgs e)
        {
            InfoLog.InfoPrintf(e.Data, InfoLog.InfoClass.下载系统信息);
            // Console.WriteLine(e.Data);
        }
        public static RoomInit.RoomInfo GetRoomInfo(string originalRoomId)
        {
            //发送HTTP请求
            string roomHtml;
            try
            {
                roomHtml = MMPU.返回网页内容_GET("https://www.youtube.com/channel/"+ originalRoomId+"/live",8000);
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(originalRoomId + "获取房间信息失败:" + e.Message, InfoLog.InfoClass.Debug);
                return null;
            }

            //解析返回结果
            try
            {
                string tltie = "";
                try
                {
                    tltie = roomHtml.Replace("\\\"title\\\":\\\"", "⒆").Split('⒆')[1].Replace("\\\",\\\"", "⒆").Split('⒆')[0];
                }
                catch (Exception)
                {
                }
                RoomInit.RoomInfo roominfo = new RoomInit.RoomInfo()
                {
                    房间号 = originalRoomId,
                    标题 = tltie,
                    直播状态 = false,
                    UID = originalRoomId,
                    直播开始时间 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    平台 = "youtube",
                    youtubeVideoId = roomHtml.Replace(":{\\\"videoId\\\":\\\"", "⒆").Split('⒆')[1].Replace("\\\",", "⒆").Split('⒆')[0]
                };

                try
                {
                    // Console.WriteLine(roomHtml);
                     string BC = roomHtml.Replace("(window.ytcsi) {window.ytcsi.tick(\"cfg\", null, '')", "⒆").Split('⒆')[1].Replace("</div>", "⒆").Split('⒆')[0];
                    if (BC.Contains("\\\"isLive\\\":true"))
                    {
                        roominfo.直播状态 = true;
                    }
                    else
                    {
                        roominfo.直播状态 = false;
                    }
                }
                catch (Exception )
                {

                    roominfo.直播状态 = false;
                }
            
                InfoLog.InfoPrintf("获取到房间信息:" + roominfo.UID + " " + (roominfo.直播状态 ? "已开播" : "未开播") + " " + (roominfo.直播状态 ? "开播时间:" + roominfo.直播开始时间 : ""), InfoLog.InfoClass.Debug);
                return roominfo;
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(originalRoomId + "房间信息解析失败:" + e.Message, InfoLog.InfoClass.Debug);
                return null;
            }

        }
    }
}
