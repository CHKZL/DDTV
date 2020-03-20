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
using System.Drawing;
using System.IO;

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
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    webClient.Headers.Add("Cookie", MMPU.Cookie);
                }
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
            public static string 发送弹幕(string roomid,string mess)
            {
                if(string.IsNullOrEmpty(MMPU.Cookie))
                {
                    return "未登录，发送失败";
                }
                string cookie = MMPU.Cookie;
                try
                {
                    int.Parse(roomid);
                }
                catch (Exception)
                {
                    return "发送失败，未选择房间或者房间异常";
                }
                Dictionary<string, string> POST表单 = new Dictionary<string, string>();
                POST表单.Add("color", "16777215");
                POST表单.Add("fontsize", "25");
                POST表单.Add("mode", "1");
                POST表单.Add("msg", mess);
                POST表单.Add("rnd", (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1))).TotalSeconds.ToString());
                POST表单.Add("roomid", roomid);
                POST表单.Add("csrf_token", MMPU.csrf);
                POST表单.Add("csrf", MMPU.csrf);
                CookieContainer CK = new CookieContainer();
                CK.MaxCookieSize = 4096;
                CK.PerDomainCapacity = 50;

                string[] cook = cookie.Replace(" ", "").Split(';');
                for (int i = 0; i < cook.Length; i++)
                {
                    try
                    {
                        CK.Add(new Cookie(cook[i].Split('=')[0], cook[i].Split('=')[1].Replace(",", "%2C")) { Domain = "live.bilibili.com" });
                    }
                    catch (Exception ex)
                    {
                        string ca = ex.ToString();
                    }
                }

                JObject JO = (JObject)JsonConvert.DeserializeObject(MMPU.返回网页内容("https://api.live.bilibili.com/msg/send", POST表单, CK));
                if (JO["code"].ToString() == "0")
                {
                    return "发送成功";
                }
                else
                {
                    return "发送失败，接口返回消息:" + JO["message"].ToString();

                }
            }
        }
        public static string 通过UID获取房间号(string uid)
        {
            var wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

            //发送HTTP请求



            byte[] roomHtml = wc.DownloadData("https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid="+ uid);

            var result = JObject.Parse(Encoding.UTF8.GetString(roomHtml));
            return result["data"]["roomid"].ToString();
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
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    wc.Headers.Add("Cookie", MMPU.Cookie);
                }
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
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    wc.Headers.Add("Cookie", MMPU.Cookie);
                }
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
               // return "https://d1--cn-gotcha04.bilivideo.com/live-bvc/793662/live_458154143_84879393.flv?cdn=cn-gotcha04&expires=1584633755&len=0&oi=3664520922&pt=web&qn=10000&trid=ceaa755109194e2aa2249569d4945b22&sigparams=cdn,expires,len,oi,pt,qn,trid&sign=81057e08abe71ffa9a1a86d0c93cd0c3&ptype=0&platform=web&pSession=keiyK8Ef-t7iZ-4xGe-hER9-NfFXX7SF7ibN";
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
                if(!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    wc.Headers.Add("Cookie", MMPU.Cookie);
                }
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

                //解析结果使用最高清晰度
                try
                {
                    //foreach (var item in JObject.Parse(resultString)["data"]["durl"])
                    //{
                    //    Console.WriteLine(item["url"].ToString());

                    //}
                    MMPU.判断网络路径是否存在 判断文件是否存在 = new MMPU.判断网络路径是否存在();
                    string BBBC = (JObject.Parse(resultString)["data"]["durl"][0]["url"].ToString() + "&platform=web").Replace("pt=", "pt=web") + "&pSession=" + Guid.NewGuid();
                    if (!判断文件是否存在.判断(BBBC, "bilibili"))
                    {
                        BBBC = (JObject.Parse(resultString)["data"]["durl"][0]["url"].ToString());
                    }
                 
                    Console.WriteLine(BBBC);
                    return BBBC;
                }
                catch (Exception e)
                {
                    Console.WriteLine("视频流地址解析失败：" + e.Message);
                    return "";
                }
            }
            public static string 下载地址(string roomid,int live)
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
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    wc.Headers.Add("Cookie", MMPU.Cookie);
                }

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

                //解析结果使用最高清晰度
                try
                {
                    return JObject.Parse(resultString)["data"]["durl"][live]["url"].ToString();
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
                if (!string.IsNullOrEmpty(MMPU.Cookie))
                {
                    wc.Headers.Add("Cookie", MMPU.Cookie);
                }
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

        public static JObject 根据UID获取关注列表(string UID)
        {
            关注列表类 关注列表 = new 关注列表类() { 
                data = new List<关注列表类.账号信息>()
            };
            int pg = 1;
            int ps = 0;
            do
            {
                JObject JO = JObject.Parse(MMPU.使用WC获取网络内容("https://api.bilibili.com/x/relation/followings?vmid=" + UID + "&pn=" + pg + "&ps=50&order=desc&jsonp=jsonp"));
                ps = JO["data"]["list"].Count();
                foreach (var item in JO["data"]["list"])
                {
                    关注列表.data.Add(new 关注列表类.账号信息()
                    {
                        UID = item["mid"].ToString(),
                        介绍 = item["sign"].ToString(),
                        名称 = item["uname"].ToString()
                    });
                }
              
                pg++;
            }
            while (ps > 0);

            return JObject.FromObject(关注列表);
        }

        public class 关注列表类
        {
            public List<账号信息> data { set; get; }
            public class 账号信息
            {
                public string UID { set; get; }
                public string 名称 { set; get; }
                public string 介绍 { set; get; }
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
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                wc.Headers.Add("Cookie", MMPU.Cookie);
            }
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

        public class BiliUser
        {
            /// <summary>
            /// 读取INI文件值
            /// </summary>
            /// <param name="section">节点名</param>
            /// <param name="key">键</param>
            /// <param name="def">未取到值时返回的默认值</param>
            /// <param name="filePath">INI文件完整路径</param>
            /// <returns>读取的值</returns>
            public static string Read(string section, string key, string def, string filePath)
            {
                StringBuilder sb = new StringBuilder(1024);
                GetPrivateProfileString(section, key, def, sb, 1024, filePath);
                return sb.ToString();
            }

            /// <summary>
            /// 写INI文件值
            /// </summary>
            /// <param name="section">欲在其中写入的节点名称</param>
            /// <param name="key">欲设置的项名</param>
            /// <param name="value">要写入的新字符串</param>
            /// <param name="filePath">INI文件完整路径</param>
            /// <returns>非零表示成功，零表示失败</returns>
            public static int Write(string section, string key, string value, string filePath)
            {
                CheckPath(filePath);
                return WritePrivateProfileString(section, key, value, filePath);
            }

            public static void CheckPath(string FilePath)
            {
                if (!File.Exists(FilePath))
                {
                    
                    new Task(() =>
                    {
                        //File.Create(MMPU.BiliUserFile);//创建INI文件
                        File.AppendAllText(MMPU.BiliUserFile, "#本文件为BiliBili扫码登陆缓存，为登陆缓存cookie，不包含账号密码，请注意");
                       
                    }).Start();
                }
            }

            /// <summary>
            /// 删除节
            /// </summary>
            /// <param name="section">节点名</param>
            /// <param name="filePath">INI文件完整路径</param>
            /// <returns>非零表示成功，零表示失败</returns>
            public static int DeleteSection(string section, string filePath)
            {
                return Write(section, null, null, filePath);
            }

            /// <summary>
            /// 删除键的值
            /// </summary>
            /// <param name="section">节点名</param>
            /// <param name="key">键名</param>
            /// <param name="filePath">INI文件完整路径</param>
            /// <returns>非零表示成功，零表示失败</returns>
            public static int DeleteKey(string section, string key, string filePath)
            {
                return Write(section, key, null, filePath);
            }

            /// <summary>
            /// 为INI文件中指定的节点取得字符串
            /// </summary>
            /// <param name="lpAppName">欲在其中查找关键字的节点名称</param>
            /// <param name="lpKeyName">欲获取的项名</param>
            /// <param name="lpDefault">指定的项没有找到时返回的默认值</param>
            /// <param name="lpReturnedString">指定一个字串缓冲区，长度至少为nSize</param>
            /// <param name="nSize">指定装载到lpReturnedString缓冲区的最大字符数量</param>
            /// <param name="lpFileName">INI文件完整路径</param>
            /// <returns>复制到lpReturnedString缓冲区的字节数量，其中不包括那些NULL中止字符</returns>
            [DllImport("kernel32")]
            public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

            /// <summary>
            /// 修改INI文件中内容
            /// </summary>
            /// <param name="lpApplicationName">欲在其中写入的节点名称</param>
            /// <param name="lpKeyName">欲设置的项名</param>
            /// <param name="lpString">要写入的新字符串</param>
            /// <param name="lpFileName">INI文件完整路径</param>
            /// <returns>非零表示成功，零表示失败</returns>
            [DllImport("kernel32")]
            public static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
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
