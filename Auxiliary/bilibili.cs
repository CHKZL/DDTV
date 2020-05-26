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
using BiliAccount;
using BiliAccount.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Auxiliary.LiveChatScript;

namespace Auxiliary
{
    public class bilibili
    {
        public static List<RoomInit.RoomInfo> RoomList = new List<RoomInit.RoomInfo>();
        public static bool 是否正在更新房间信息 = false;
        public static void start()
        {
            Task.Run(async () =>
            {
                InfoLog.InfoPrintf("启动房间信息本地缓存更新线程", InfoLog.InfoClass.Debug);
                while (true)
                {
                    try
                    {
                        周期更新B站房间状态();
                        await Task.Delay(MMPU.直播更新时间 * 1000);
                    }
                    catch (Exception e)
                    {
                        InfoLog.InfoPrintf("房间信息本地缓存更新出现错误:" + e.ToString(), InfoLog.InfoClass.Debug);
                    }
                }
            });
        }
        public static void 周期更新B站房间状态()
        {
            if(!是否正在更新房间信息)
            {
                是否正在更新房间信息 = true;
                InfoLog.InfoPrintf("本地房间状态缓存更新开始", InfoLog.InfoClass.Debug);


                switch (MMPU.数据源)
                {
                    case 0:
                        {
                            使用vtbsAPI更新房间状态();
                            break;
                        }
                    case 1:
                        {
                            使用B站API更新房间状态();
                            break;
                        }
                }
                InfoLog.InfoPrintf("当前阿B API调用次数为:" + DataCache.BilibiliApiCount, InfoLog.InfoClass.杂项提示);
                InfoLog.InfoPrintf("本地房间状态更新结束", InfoLog.InfoClass.Debug);
                是否正在更新房间信息 = false;
            }     
        }
        public static void 使用vtbsAPI更新房间状态()
        {
            try
            {
                JArray JO = (JArray)JsonConvert.DeserializeObject(MMPU.返回网页内容_GET("https://api.vtbs.moe/v1/living",8000));
                foreach (var roomtask in RoomList)
                {
                    roomtask.直播状态 = false;
                    if (JO.ToString().Contains(roomtask.房间号))
                    {
                        roomtask.直播状态 = true;
                    }
                    else
                    {
                        roomtask.直播状态 = false;
                    }
                }
                InfoLog.InfoPrintf("Vtbs数据加载成功", InfoLog.InfoClass.Debug);
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf("Vtbs数据加载失败，使用备用数据源开始获取", InfoLog.InfoClass.Debug);
                try
                {   
                    JArray JO = (JArray)JsonConvert.DeserializeObject(MMPU.TcpSend(Server.RequestCode.GET_LIVELSIT, "{}", true));
                    foreach (var roomtask in RoomList)
                    {
                        roomtask.直播状态 = false;
                        if (JO.ToString().Contains(roomtask.房间号))
                        {
                            roomtask.直播状态 = true;
                        }
                        else
                        {
                            roomtask.直播状态 = false;
                        }
                    }
                    InfoLog.InfoPrintf("备用数据源加载成功", InfoLog.InfoClass.Debug);
                }
                catch (Exception)
                {
                    InfoLog.InfoPrintf("备用缓存数据加载失败，使用原生阿Bapi开始获取开始获取", InfoLog.InfoClass.Debug);
                    使用B站API更新房间状态();
                }
               
            }
        }
        public static void 使用B站API更新房间状态()
        {
            foreach (var roomtask in RoomList)
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
                            break;
                        }
                    }
                }
                Thread.Sleep(800);
            }
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
                    InfoLog.InfoPrintf("弹幕获取出现错误" + ex.ToString(), InfoLog.InfoClass.系统错误信息);
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
            public static string 发送弹幕(string roomid, string mess)
            {
                if (string.IsNullOrEmpty(MMPU.Cookie))
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
                Dictionary<string, string> POST表单 = new Dictionary<string, string>
                {
                    { "color", "16777215" },
                    { "fontsize", "25" },
                    { "mode", "1" },
                    { "msg", mess },
                    { "rnd", (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1))).TotalSeconds.ToString() },
                    { "roomid", roomid },
                    { "csrf_token", MMPU.csrf },
                    { "csrf", MMPU.csrf }
                };
                CookieContainer CK = new CookieContainer
                {
                    MaxCookieSize = 4096,
                    PerDomainCapacity = 50
                };

                string[] cook = cookie.Replace(" ", "").Split(';');
                for (int i = 0; i < cook.Length; i++)
                {
                    try
                    {
                        CK.Add(new Cookie(cook[i].Split('=')[0], cook[i].Split('=')[1].Replace(",", "%2C")) { Domain = "live.bilibili.com" });
                    }
                    catch (Exception)
                    {
                       
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
            string CacheStr = "byUIDgetROOMID";
            if (DataCache.读缓存(CacheStr + uid, 0, out string CacheData))
            {
                return CacheData;
            }
            //发送HTTP请求
            string roomHtml;
            try
            {
                roomHtml = MMPU.使用WC获取网络内容("https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid=" + uid);
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(uid + " 通过UID获取房间号:" + e.Message, InfoLog.InfoClass.Debug);
                return null;

            }
            try
            {
                var result = JObject.Parse(roomHtml);
                string roomId = result["data"]["roomid"].ToString();
                //InfoLog.InfoPrintf("根据UID获取到房间号:" + roomId, InfoLog.InfoClass.Debug);
                DataCache.写缓存(CacheStr + uid, roomId);
                return roomId;
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(uid + " 通过UID获取房间号:" + e.Message, InfoLog.InfoClass.Debug);
                return null;
            }

        }
        public class 根据房间号获取房间信息
        {
            public static bool 是否正在直播(string RoomId)
            {
                var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + RoomId;
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + MMPU.UA.Ver.UA());
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
                    DataCache.BilibiliApiCount++;
                    return result["data"]["live_status"].ToString() == "1" ? true : false;
                }
                catch (Exception)
                {
                    try
                    {
                        var roomJson = Encoding.UTF8.GetString(roomHtml);
                        var result = JObject.Parse(roomJson);
                        DataCache.BilibiliApiCount++;
                        return result["data"]["live_status"].ToString() == "1" ? true : false;
                    }
                    catch (Exception)
                    {

                        return false;
                    }

                }
            }

            public static string 获取标题(string roomID)
            {
                roomID = 获取真实房间号(roomID);
                if (roomID == null)
                {
                    InfoLog.InfoPrintf("房间号获取错误", InfoLog.InfoClass.下载必要提示);
                    return null;
                }

                string roomHtml;
                try
                {
                    roomHtml = MMPU.使用WC获取网络内容("https://api.live.bilibili.com/room/v1/Room/get_info?id=" + roomID);
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf(roomID + "获取房间信息失败:" + e.Message, InfoLog.InfoClass.下载必要提示);
                    return null;
                }
                //解析结果
                try
                {
                    JObject result = JObject.Parse(roomHtml);
                    string roomName = result["data"]["title"].ToString().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("\"", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", "").ToString();
                    InfoLog.InfoPrintf("根据RoomId获取到标题:" + roomName, InfoLog.InfoClass.Debug);
                    return roomName;
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf("视频标题解析失败：" + e.Message, InfoLog.InfoClass.Debug);
                    return "";
                }
            }
            /// <summary>
            /// 获取BILIBILI直播流下载地址
            /// </summary>
            /// <param name="roomid">房间号</param>
            /// <param name="R">是否为重试</param>
            /// <returns></returns>
            public static string 下载地址(string roomid)
            {
                roomid = 获取真实房间号(roomid);
                if (roomid == null)
                {
                    InfoLog.InfoPrintf("房间号获取错误", InfoLog.InfoClass.Debug);
                    return null;
                }
                string resultString;
                try
                {
                    resultString = MMPU.使用WC获取网络内容("https://api.live.bilibili.com/room/v1/Room/playUrl?cid=" + roomid + "&otype=json&qn=10000&platform=web");
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf("发送解析请求失败：" + e.Message, InfoLog.InfoClass.Debug);
                    return "";
                }

                //解析结果使用最高清晰度
                try
                {
                    MMPU.判断网络路径是否存在 判断文件是否存在 = new MMPU.判断网络路径是否存在();
                    string BBBC = "";
                    BBBC = (JObject.Parse(resultString)["data"]["durl"][0]["url"].ToString());
                    //BBBC = (JObject.Parse(resultString)["data"]["durl"][0]["url"].ToString() + "&platform=web").Replace("&pt=", "&pt=web") + "&pSession=" + Guid.NewGuid();
                    if (!判断文件是否存在.判断(BBBC, "bilibili", roomid))
                    {
                        InfoLog.InfoPrintf("请求的开播房间当前推流数据为空，推测还未开播，等待数据流...：", InfoLog.InfoClass.Debug);
                        BBBC = (JObject.Parse(resultString)["data"]["durl"][1]["url"].ToString());
                    }
                    DataCache.BilibiliApiCount++;
                    return BBBC;
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf("视频流地址解析失败：" + e.Message, InfoLog.InfoClass.Debug);
                    return "";
                }
            }

            public static string 获取真实房间号(string roomID)
            {
                string CacheStr = "byROOMIDgetTRUEroomid";
                if (DataCache.读缓存(CacheStr + roomID, 0, out string CacheData))
                {
                    return CacheData;
                }
                string roomHtml;
                try
                {
                    roomHtml = MMPU.使用WC获取网络内容("https://api.live.bilibili.com/room/v1/Room/get_info?id=" + roomID);
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf(roomID + "获取房间信息失败:" + e.Message, InfoLog.InfoClass.Debug);
                    return null;
                }
                //从返回结果中提取真实房间号
                try
                {
                    JObject result = JObject.Parse(roomHtml);
                    string live_status = result["data"]["live_status"].ToString();
                    if (live_status != "1")
                    {
                        return "-1";
                    }
                    string roomid = result["data"]["room_id"].ToString();
                    DataCache.写缓存(CacheStr + roomID, roomid);
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
            关注列表类 关注列表 = new 关注列表类()
            {
                data = new List<关注列表类.账号信息>()
            };
            int pg = 1;
            int ps;
            do
            {
                JObject JO = JObject.Parse(MMPU.使用WC获取网络内容("https://api.bilibili.com/x/relation/followings?vmid=" + UID + "&pn=" + pg + "&ps=50&order=desc&jsonp=jsonp"));
                DataCache.BilibiliApiCount++;
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
                Thread.Sleep(100);
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

        public static RoomInit.RoomInfo GetRoomInfo(string originalRoomId)
        {
            string roomHtml;
            try
            {
                roomHtml = MMPU.使用WC获取网络内容("https://api.live.bilibili.com/room/v1/Room/get_info?id=" + originalRoomId);
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(originalRoomId + "获取房间信息失败:" + e.Message, InfoLog.InfoClass.Debug);
                return null;
            }
            //解析返回结果
            try
            {
                JObject result = JObject.Parse(roomHtml);
                string uid = result["data"]["uid"].ToString();
                if (result["data"]["room_id"].ToString() != originalRoomId)
                {
                    for (int i = 0; i < RoomList.Count(); i++)
                    {
                        if (RoomList[i].房间号 == originalRoomId)
                        {
                            RoomList[i].房间号 = result["data"]["room_id"].ToString();
                            break;
                        }
                    }
                }
                var roominfo = new RoomInit.RoomInfo
                {
                    房间号 = result["data"]["room_id"].ToString(),
                    标题 = result["data"]["title"].ToString().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("\"", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                    直播状态 = result["data"]["live_status"].ToString() == "1" ? true : false,
                    UID = result["data"]["uid"].ToString(),
                    直播开始时间 = result["data"]["live_time"].ToString(),
                    平台 = "bilibili"
                };
                InfoLog.InfoPrintf("获取到房间信息:" + roominfo.UID + " " + (roominfo.直播状态 ? "已开播" : "未开播") + " " + (roominfo.直播状态 ? "开播时间:" + roominfo.直播开始时间 : ""), InfoLog.InfoClass.Debug);
                DataCache.BilibiliApiCount++;
                return roominfo;
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(originalRoomId + "房间信息解析失败:" + e.Message, InfoLog.InfoClass.Debug);
                return null;
            }
        }
        public class BiliUser
        {
            public static Account account = new Account();

            public static void 登陆()
            {
                ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                ByQRCode.LoginByQrCode("#FF000000","#FFFFFFFF",true).Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            private static void ByQRCode_QrCodeRefresh(Bitmap newQrCode)
            {
                newQrCode.Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
            } 

            public static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, Account account = null)
            {
                if (status == ByQRCode.QrCodeStatus.Success)
                {
                    BiliUser.account = account;
                    InfoLog.InfoPrintf("UID:" + account.Uid + ",登陆成功", InfoLog.InfoClass.杂项提示);
                    //MessageBox.Show("UID:"+account.Uid+",登陆成功");
                    MMPU.UID = account.Uid;
                    MMPU.写ini配置文件("User", "UID", MMPU.UID, MMPU.BiliUserFile);
                    foreach (var item in account.Cookies)
                    {
                        MMPU.Cookie = MMPU.Cookie + item + ";";
                    }
                    MMPU.CookieEX = account.Expires_Cookies;
                    MMPU.csrf = account.CsrfToken;
                    ;
                    MMPU.写ini配置文件("User", "csrf", MMPU.csrf, MMPU.BiliUserFile);
                    MMPU.写ini配置文件("User", "Cookie", Encryption.AesStr(MMPU.Cookie, MMPU.AESKey, MMPU.AESVal), MMPU.BiliUserFile);
                    MMPU.写ini配置文件("User", "CookieEX", MMPU.CookieEX.ToString("yyyy-MM-dd HH:mm:ss"), MMPU.BiliUserFile);
                }
            }
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
        public class BiliWebSocket: RoomInit.RoomInfo
        {
            public int room_id;
            public async void WebSocket(int roomId)
            {
                LiveChatListener listener = new LiveChatListener();
                await listener.ConnectAsync(roomId);
                listener.MessageReceived += Listener_MessageReceived;
                //Debug.LogError(MessageBox(IntPtr.Zero, "???", "只有红茶了可以吗", 1));
                //jiba();
            }

            private void Listener_MessageReceived(object sender, MessageEventArgs e)
            {
                switch (e)
                {
                    case DanmuMessageEventArgs danmu:
                       // Console.WriteLine("WebSocket收到弹幕数据:{0}:{1}", danmu.UserName, danmu.Message);
                        //Debug.Log("M:" + danmu.Message);
                        //mtext.text = "M:" + danmu.Message;
                        break;
                    case SendGiftEventArgs gift:
                        //Debug.Log("G:"+gift.GiftName);
                        //gtext.text = "G:" + gift.GiftName;
                        break;
                    case GuardBuyEventArgs guard:
                        // Debug.LogError("JZ:" + guard.GiftName);
                        break;
                    case WelcomeEventArgs Welcome:

                        break;
                    case ActivityBannerEventArgs activityBannerEventArgs:

                        break;

                    default:
                        // Debug.LogError("???" + e.JsonObject);
                        // Debug.LogError(LitJson.JsonMapper.ToJson(e.JsonObject));
                        
                        break;
                }
            }

            #region ClientWebSocket

            //            readonly ClientWebSocket _webSocket = new ClientWebSocket();
            //            readonly CancellationToken _cancellation = new CancellationToken();
            //            private CancellationTokenSource m_innerRts = new CancellationTokenSource();
            //            private readonly byte[] m_ReceiveBuffer = new byte[] { };

            //            public async void WebSocket()
            //            {
            //                try
            //                {

            //                    //建立连接
            //                    await _webSocket.ConnectAsync(new Uri("wss://broadcastlv.chat.bilibili.com/sub"), _cancellation);
            //                    await _sendObject(7, new
            //                    {
            //                        uid = 0,
            //                        roomid = 21320551,
            //                        protover = 2,
            //                        platform = "web",
            //                        clientver = "1.7.3"
            //                    });
            //                    _ = _innerLoop().ContinueWith((t) =>
            //                    {
            //                        if (t.IsFaulted)
            //                        {
            //                            //UnityEngine.Debug.LogError(t.Exception.);
            //                            if (!m_innerRts.IsCancellationRequested)
            //                            {

            //                                m_innerRts.Cancel();
            //                            }
            //                        }
            //                        else
            //                        {
            //                            //POST-CANCEL
            //#if DEBUG
            //                            Console.WriteLine("LiveChatListender cancelled.");
            //#endif
            //                        }
            //                        _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
            //                        _webSocket.Dispose();
            //                        m_innerRts.Dispose();
            //                    });
            //                    _ = _innerHeartbeat();

            //                }
            //                catch (Exception ex)
            //                {
            //                    Console.WriteLine(ex.Message);
            //                }
            //            }
            //            private async Task _innerLoop()
            //            {
            //#if DEBUG
            //                Console.WriteLine("LiveChatListender start.");
            //#endif
            //                while (!m_innerRts.IsCancellationRequested)
            //                {
            //                    try
            //                    {
            //                        WebSocketReceiveResult result;
            //                        int length = 0;
            //                        do
            //                        {
            //                            result = await _webSocket.ReceiveAsync(
            //                                new ArraySegment<byte>(m_ReceiveBuffer, length, m_ReceiveBuffer.Length - length),
            //                                m_innerRts.Token);
            //                            length += result.Count;
            //                        }
            //                        while (!result.EndOfMessage);

            //                        //=========fuckbilibili==========
            //                        DepackDanmakuData(m_ReceiveBuffer);
            //                        //===============================

            //                        #region 已失效[弃用]
            //                        //var segments = _depack(new ArraySegment<byte>(m_ReceiveBuffer, 0, length));
            //                        //foreach (var segment in segments)
            //                        //{
            //                        //    string jsonBody = System.Text.Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count);
            //                        //    _parse(jsonBody);
            //                        //}
            //                        #endregion
            //                    }
            //                    catch (OperationCanceledException)
            //                    {
            //                        continue;
            //                    }
            //                    catch (ObjectDisposedException)
            //                    {
            //                        continue;
            //                    }
            //                    catch (WebSocketException we)
            //                    {
            //                        throw we;
            //                    }
            //                    catch (Newtonsoft.Json.JsonException)
            //                    {
            //                        continue;
            //                    }
            //                    catch (Exception e)
            //                    {
            //                        //UnityEngine.Debug.LogException(e);
            //                        throw e;
            //                    }
            //                }
            //            }
            //            private async Task _innerHeartbeat()
            //            {
            //                while (!m_innerRts.IsCancellationRequested)
            //                {
            //                    try
            //                    {
            //                        //UnityEngine.Debug.Log("heartbeat");
            //                        await _sendBinary(2, System.Text.Encoding.UTF8.GetBytes("[object Object]"));
            //                        await Task.Delay(30 * 1000, m_innerRts.Token);
            //                    }
            //                    catch (OperationCanceledException)
            //                    {
            //                        break;
            //                    }
            //                    catch (Exception e)
            //                    {
            //                        throw e;
            //                    }
            //                }
            //            }
            //            public async Task _sendObject(int type, object obj)
            //            {

            //                //string jsonBody = JsonConvert.SerializeObject(obj, Formatting.None);
            //                string jsonBody = JsonMapper.ToJson(obj);
            //                await _sendBinary(type, System.Text.Encoding.UTF8.GetBytes(jsonBody));
            //            }
            //            public async Task _sendBinary(int type, byte[] body)
            //            {
            //                byte[] head = new byte[16];
            //                using (MemoryStream ms = new MemoryStream(head))
            //                {
            //                    using (BinaryWriter bw = new BinaryWriter(ms))
            //                    {
            //                        bw.WriteBE(16 + body.Length);
            //                        bw.WriteBE((ushort)16);
            //                        bw.WriteBE((ushort)1);
            //                        bw.WriteBE(type);
            //                        bw.WriteBE(1);
            //                    }
            //                }
            //                await _webSocket.SendAsync(new ArraySegment<byte>(head), WebSocketMessageType.Binary, false, CancellationToken.None);
            //                await _webSocket.SendAsync(new ArraySegment<byte>(body), WebSocketMessageType.Binary, true, CancellationToken.None);
            //            }
            //            private void _parse(string jsonBody)
            //            {
            //                var obj = JsonMapper.ToObject(jsonBody);
            //                //Debug.Log(jsonBody);
            //                string cmd = (string)obj["cmd"];
            //                switch (cmd)
            //                {
            //                    case "DANMU_MSG":
            //                      //  MessageReceived(this, new DanmuMessageEventArgs(obj));
            //                        break;
            //                    case "SEND_GIFT":
            //                       // MessageReceived(this, new SendGiftEventArgs(obj));
            //                        break;
            //                    case "GUARD_BUY":
            //                        //Debug.Log("guraddd\n"+obj);
            //                       // MessageReceived(this, new GuardBuyEventArgs(obj));
            //                        break;
            //                    default:
            //                        //Debug.Log("Unknow\n"+obj);
            //                       // MessageReceived(this, new MessageEventArgs(obj));
            //                        break;
            //                }
            //            }
            //            #endregion
            //            #region 新协议解析方法

            //            /// <summary>
            //            /// 消息协议
            //            /// </summary>
            //            public class DanmakuProtocol
            //            {
            //                /// <summary>
            //                /// 消息总长度 (协议头 + 数据长度)
            //                /// </summary>
            //                public int PacketLength;
            //                /// <summary>
            //                /// 消息头长度 (固定为16[sizeof(DanmakuProtocol)])
            //                /// </summary>
            //                public short HeaderLength;
            //                /// <summary>
            //                /// 消息版本号
            //                /// </summary>
            //                public short Version;
            //                /// <summary>
            //                /// 消息类型
            //                /// </summary>
            //                public int Operation;
            //                /// <summary>
            //                /// 参数, 固定为1
            //                /// </summary>
            //                public int Parameter;

            //                /// <summary>
            //                /// 转为本机字节序
            //                /// </summary>
            //                public DanmakuProtocol(byte[] buff)
            //                {
            //                    PacketLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buff, 0));
            //                    HeaderLength = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(buff, 4));
            //                    Version = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(buff, 6));
            //                    Operation = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buff, 8));
            //                    Parameter = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buff, 12));
            //                }
            //            }

            //            /// <summary>
            //            /// 消息处理
            //            /// </summary>
            //            private void ProcessDanmakuData(int opt, byte[] buffer, int length)
            //            {
            //                switch (opt)
            //                {
            //                    case 5:
            //                        {
            //                            try
            //                            {
            //                                string jsonBody = Encoding.UTF8.GetString(buffer, 0, length);
            //                                jsonBody = Regex.Unescape(jsonBody);
            //                                _parse(jsonBody);
            //                                //Debug.Log(jsonBody);
            //                                //ReceivedDanmaku?.Invoke(this, new ReceivedDanmakuArgs { Danmaku = new Danmaku(json) });
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                if (ex is Newtonsoft.Json.JsonException || ex is KeyNotFoundException)
            //                                {
            //                                    //LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 弹幕识别错误 {json}" });
            //                                }
            //                                else
            //                                {
            //                                    //LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] {ex}" });
            //                                }
            //                            }
            //                            break;
            //                        }
            //                }
            //            }

            //            /// <summary>
            //            /// 消息拆包
            //            /// </summary>
            //            private void DepackDanmakuData(byte[] messages)
            //            {
            //                var headerBuffer = new byte[16];
            //                //for (int i = 0; i < 16; i++)
            //                //{
            //                //    headerBuffer[i] = messages[i];
            //                //}
            //                Array.Copy(messages, 0, headerBuffer, 0, 16);
            //                var protocol = new DanmakuProtocol(headerBuffer);

            //                //Debug.LogError(protocol.Version + "\\" + protocol.Operation);
            //                //
            //                if (protocol.PacketLength < 16)
            //                {
            //                    throw new NotSupportedException($@"协议失败: (L:{protocol.PacketLength})");
            //                }
            //                var bodyLength = protocol.PacketLength - 16;
            //                if (bodyLength == 0)
            //                {
            //                    //continue;
            //                    return;
            //                }
            //                var buffer = new byte[bodyLength];
            //                //for (int i = 0; i < bodyLength; i++)
            //                //{
            //                //    buffer[i] = messages[i + 16];
            //                //}
            //                Array.Copy(messages, 16, buffer, 0, bodyLength);
            //                switch (protocol.Version)
            //                {
            //                    case 1:
            //                        ProcessDanmakuData(protocol.Operation, buffer, bodyLength);
            //                        break;
            //                    case 2:
            //                        {
            //                            var ms = new MemoryStream(buffer, 2, bodyLength - 2);
            //                            var deflate = new DeflateStream(ms, CompressionMode.Decompress);
            //                            while (deflate.Read(headerBuffer, 0, 16) > 0)
            //                            {
            //                                protocol = new DanmakuProtocol(headerBuffer);
            //                                bodyLength = protocol.PacketLength - 16;
            //                                if (bodyLength == 0)
            //                                {
            //                                    continue; // 没有内容了
            //                                }
            //                                if (buffer.Length < bodyLength) // 不够长再申请
            //                                {
            //                                    buffer = new byte[bodyLength];
            //                                }
            //                                deflate.Read(buffer, 0, bodyLength);
            //                                ProcessDanmakuData(protocol.Operation, buffer, bodyLength);
            //                            }
            //                            ms.Dispose();
            //                            deflate.Dispose();
            //                            break;
            //                        }
            //                }
            //            }

            #endregion
        }
    }
    internal static class Extensions
    {
        internal static void WriteBE(this BinaryWriter writer, int value)
        {
            unsafe { SwapBytes((byte*)&value, 4); }
            writer.Write(value);
        }
        internal static void WriteBE(this BinaryWriter writer, ushort value)
        {
            unsafe { SwapBytes((byte*)&value, 2); }
            writer.Write(value);
        }

        internal static unsafe void SwapBytes(byte* ptr, int length)
        {
            for (int i = 0; i < length / 2; ++i)
            {
                byte b = *(ptr + i);
                *(ptr + i) = *(ptr + length - i - 1);
                *(ptr + length - i - 1) = b;
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
}

