using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace Auxiliary
{


    public class bilibili
    {
        public class RoomInfo
        {
            public bool 是否提醒 { set; get; }
            public string 名称 { set; get; }
            public string 房间号 { set; get; }
            public string 标题 { set; get; }
            public bool 直播状态 { set; get; }
            public string UID { set; get; }
            public string 直播开始时间 { set; get; }
            public bool 是否录制视频 { set; get; }
            public bool 是否录制弹幕 { set; get; }
            public string 原名 { set; get; }
        }
        public static List<RoomInfo> RoomList = new List<RoomInfo>();
        public static void start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        周期更新B站房间状态();
                        await Task.Delay(MMPU.直播更新时间 * 1000);
                    }
                    catch (Exception)
                    {
                    }
                    

                }
            });
        }
        private static void 周期更新B站房间状态()
        {
            int a = 0;
            foreach (var roomtask in RoomList)
            {
                RoomInfo A = GetRoomInfo(roomtask.房间号);
                if (A != null)
                {
                    for (int i = 0; i < RoomList.Count(); i++)
                    {
                        if (RoomList[i].房间号 == A.房间号)
                        {
                            RoomList[i].标题 = A.标题;
                            RoomList[i].UID = A.UID;
                            RoomList[i].直播开始时间 = A.直播开始时间;
                            RoomList[i].直播状态 = A.直播状态;
                            if (A.直播状态)
                                a++;
                            break;
                        }
                    }
                }
            }
            //Console.WriteLine("B"+a);
        }
        public class danmu
        {
            public List<string> 储存的弹幕数据 = new List<string>();
            /// <summary>
            /// 获取房间的弹幕
            /// </summary>
            /// <param name="RoomID">房间号</param>
            /// <returns></returns>
            public string getDanmaku(string RoomID)
            {
                string postString = "roomid=" + RoomID + "&token=&csrf_token=";
                byte[] postData = Encoding.UTF8.GetBytes(postString);
                string url = @"http://api.live.bilibili.com/ajax/msg";
                List<danmuA> 返回的弹幕数据 = new List<danmuA>();
                WebClient webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                webClient.Headers.Add("Cookie", "");
                byte[] responseData = webClient.UploadData(url, "POST", postData);
                string srcString = Encoding.UTF8.GetString(responseData);//解码  
                try
                {
                    JObject jo = (JObject)JsonConvert.DeserializeObject(srcString);
                    for (int i = 0; i < jo["data"]["room"].Count(); i++)
                    {
                        string text = jo["data"]["room"][i]["nickname"].ToString() + "㈨" + jo["data"]["room"][i]["text"].ToString();
                        if (储存的弹幕数据 == null)
                        {
                            返回的弹幕数据.Add(new danmuA() { Name = jo["data"]["room"][i]["nickname"].ToString(), Text = jo["data"]["room"][i]["text"].ToString(), Time = jo["data"]["room"][i]["timeline"].ToString(), uid = jo["data"]["room"][i]["uid"].ToString() });
                            储存的弹幕数据.Add(text);
                        }

                        if (!储存的弹幕数据.Contains(text))
                        {
                            返回的弹幕数据.Add(new danmuA() { Name = jo["data"]["room"][i]["nickname"].ToString(), Text = jo["data"]["room"][i]["text"].ToString(), Time = jo["data"]["room"][i]["timeline"].ToString(), uid = jo["data"]["room"][i]["uid"].ToString() });
                            储存的弹幕数据.Add(text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR:" + ex.ToString());
                }
                Thread.Sleep(600);
                return JsonConvert.SerializeObject(返回的弹幕数据);
            }
            private class danmuA
            {
                public string Name { set; get; }
                public string Text { set; get; }
                public string uid { set; get; }
                public string Time { set; get; }
            }
        }
      
        public class 根据房间号获取房间信息
        {
            public static bool 是否正在直播(string RoomId)
            {
                var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + RoomId;
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                //发送HTTP请求
                byte[] roomHtml;

                try
                {
                    roomHtml = wc.DownloadData(roomWebPageUrl);
                }
                catch (Exception)
                {
                    try
                    {
                        roomHtml = wc.DownloadData(roomWebPageUrl);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                 
                }

                //解析返回结果
                try
                {
                    var roomJson = Encoding.UTF8.GetString(roomHtml);
                    var result = JObject.Parse(roomJson);
                    return result["data"]["live_status"].ToString() == "1" ? true : false;
                }
                catch (Exception)
                {
                    try
                    {
                        var roomJson = Encoding.UTF8.GetString(roomHtml);
                        var result = JObject.Parse(roomJson);
                        return result["data"]["live_status"].ToString() == "1" ? true : false;
                    }
                    catch (Exception)
                    {

                        return false;
                    }
                  
                }
            }

            public static string 获取标题(string roomid)
            {
                roomid = 获取真实房间号(roomid);
                if (roomid == null)
                {
                    Console.WriteLine("房间号获取错误。");
                    return null;
                }
                var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + roomid;
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                //发送HTTP请求
                byte[] roomHtml;

                try
                {
                    roomHtml = wc.DownloadData(roomWebPageUrl);
                }
                catch (Exception e)
                {
                    InfoLogger.SendInfo(roomid, "ERROR", "获取房间信息失败：" + e.Message);
                    return null;
                }

                //解析结果
                try
                {
                    var roomJson = Encoding.UTF8.GetString(roomHtml);
                    var result = JObject.Parse(roomJson);
                    return result["data"]["title"].ToString().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("\"", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", "").ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine("视频流地址解析失败：" + e.Message);
                    return "";
                }
            }
            public static string 下载地址(string roomid)
            {
                roomid = 获取真实房间号(roomid);
                if (roomid == null)
                {
                    Console.WriteLine("房间号获取错误。");
                    return null;
                }
                var apiUrl = "https://api.live.bilibili.com/room/v1/Room/playUrl?cid=" + roomid + "&otype=json";

                //访问API获取结果
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.9,ja;q=0.8");

                string resultString;

                try
                {
                    resultString = wc.DownloadString(apiUrl);
                }
                catch (Exception e)
                {
                    Console.WriteLine("发送解析请求失败：" + e.Message);
                    return "";
                }

                //解析结果
                try
                {
                    var result = JObject.Parse(resultString);
                    return result["data"]["durl"][0]["url"].ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine("视频流地址解析失败：" + e.Message);
                    return "";
                }
            }
            public static string 获取真实房间号(string roomID)
            {
                //InfoLogger.SendInfo(originalRoomId, "DEBUG", "正在刷新信息");

                var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + roomID;
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                //发送HTTP请求
                byte[] roomHtml;

                try
                {
                    roomHtml = wc.DownloadData(roomWebPageUrl);
                }
                catch (Exception e)
                {
                    InfoLogger.SendInfo(roomID, "ERROR", "获取房间信息失败：" + e.Message);
                    return null;
                }
                //从返回结果中提取真实房间号
                try
                {
                    var roomJson = Encoding.UTF8.GetString(roomHtml);
                    var result = JObject.Parse(roomJson);
                    var live_status = result["data"]["live_status"].ToString();
                    if (live_status != "1")
                    {
                        return "-1";
                    }
                    var roomid = result["data"]["room_id"].ToString();
                    // Console.WriteLine("真实房间号: " + roomid);
                    return roomid;
                }
                catch
                {
                    return roomID;
                }
            }
        }


        public static RoomInfo GetRoomInfo(string originalRoomId)
        {

            //InfoLogger.SendInfo(originalRoomId, "DEBUG", "正在刷新信息");

            var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + originalRoomId;
            var wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: " + Ver.UA);
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

            //发送HTTP请求
            byte[] roomHtml;

            try
            {
                roomHtml = wc.DownloadData(roomWebPageUrl);
            }
            catch (Exception e)
            {
                InfoLogger.SendInfo(originalRoomId, "ERROR", "获取房间信息失败：" + e.Message);
                return null;
            }

            //解析返回结果
            try
            {
                var roomJson = Encoding.UTF8.GetString(roomHtml);
                var result = JObject.Parse(roomJson);
                var uid = result["data"]["uid"].ToString();
                if (result["data"]["room_id"].ToString() != originalRoomId)
                {
                    for(int i=0;i< RoomList.Count();i++)
                    {
                        if(RoomList[i].房间号== originalRoomId)
                        {
                            RoomList[i].房间号 = result["data"]["room_id"].ToString();
                            break;
                        }
                    }
                }
                var roominfo = new RoomInfo
                {
                    房间号 = result["data"]["room_id"].ToString(),
                    标题 = result["data"]["title"].ToString().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("\"", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                    直播状态 = result["data"]["live_status"].ToString() == "1" ? true : false,
                    UID = result["data"]["uid"].ToString(),
                    直播开始时间 = result["data"]["live_time"].ToString()
                };
                return roominfo;
            }
            catch (Exception e)
            {
                InfoLogger.SendInfo(originalRoomId, "ERROR", "房间信息解析失败：" + e.Message);
                return null;
            }

        }


    }
    public class WebClientto : WebClient
    {
        /// <summary>
        /// 过期时间
        /// </summary>
        public int Timeout { get; set; }

        public WebClientto(int timeout)
        {
            Timeout = timeout;
        }

        /// <summary>
        /// 重写GetWebRequest,添加WebRequest对象超时时间
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout;
            return request;
        }
    }
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Ver
    {
        public const string VER = "1.5.1";
        public const string DATE = "(2019-3-1)";
        public const string DESC = "修改API";
        public static readonly string OS_VER = "(" + WinVer.SystemVersion.Major + "." + WinVer.SystemVersion.Minor + "." + WinVer.SystemVersion.Build + ")";
        public static readonly string UA = OS_VER + " AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.119 Safari/537.36";
    }
    internal static class WinVer
    {
        public static readonly Version SystemVersion = GetSystemVersion();

        private static Delegate GetFunctionAddress(IntPtr dllModule, string functionName, Type t)
        {
            var address = WinApi.GetProcAddress(dllModule, functionName);
            return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, t);
        }

        private delegate IntPtr RtlGetNtVersionNumbers(ref int dwMajor, ref int dwMinor, ref int dwBuildNumber);

        private static Version GetSystemVersion()
        {
            var hinst = WinApi.LoadLibrary("ntdll.dll");
            var func = (RtlGetNtVersionNumbers)GetFunctionAddress(hinst, "RtlGetNtVersionNumbers", typeof(RtlGetNtVersionNumbers));
            int dwMajor = 0, dwMinor = 0, dwBuildNumber = 0;
            func.Invoke(ref dwMajor, ref dwMinor, ref dwBuildNumber);
            dwBuildNumber &= 0xffff;
            return new Version(dwMajor, dwMinor, dwBuildNumber);
        }
    }

    internal static class WinApi
    {
        [DllImport("Kernel32")]
        public static extern IntPtr LoadLibrary(string funcname);

        [DllImport("Kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string funcname);
    }
}
