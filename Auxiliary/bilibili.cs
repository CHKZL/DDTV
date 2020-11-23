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
using Microsoft.Extensions.Configuration.Ini;
using Auxiliary.LiveChatScript;
using static Auxiliary.RoomInit;
using System.Security.Cryptography;

namespace Auxiliary
{
    public class bilibili
    {
        public static List<RoomInit.RoomInfo> RoomList = new List<RoomInit.RoomInfo>();
        public static List<VtbsRoom> VtbsRoomList = new List<VtbsRoom>();
        public static List<string> VTBROOMlist = new List<string>();
        public static List<string> WSokRoomList = new List<string>();
        public static bool 是否正在更新房间信息 = false;
        //public static bool 是否启动WS连接组 = MMPU.读取exe默认配置文件("DataSource", "0") == "1" ? true : MMPU.读取exe默认配置文件("NotVTBStatus", "0") == "1";
        public static bool 是否启动WS连接组 = true;
        public static bool WS连接组是否已经启动 = false;

        public static bool wss连接初始化准备已完成 = false;
  
        public class VtbsRoom
        {
            public int mid { set; get; }
            public int room { set; get; }
            public string name { set; get; }
        }

        public static void start()
        {

            new Task(() => {
                #region MyRegion
                while (true)
                {
                    try
                    {
                        更新VTBS房间数据();
                        Thread.Sleep(1000 * 60 * 60);
                    }
                    catch (Exception) { }
                }
                #endregion
            }).Start();

            Task.Run(async () =>
            {
                InfoLog.InfoPrintf("启动房间信息本地缓存更新线程", InfoLog.InfoClass.Debug);
                //是否启动WS连接组 = MMPU.读取exe默认配置文件("DataSource", "0") == "1" ? true : MMPU.读取exe默认配置文件("NotVTBStatus", "0") == "1";
                if (是否启动WS连接组)
                {
                    new Task(()=> {
                        持续连接获取阿B房间信息类.初始化所有房间连接();
                    }).Start();
                }
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
        public static void 更新VTBS房间数据()
        {
            while (true)
            {
                try
                {
                   
                    string vtbs房间数据 = string.Empty;
                    try
                    {
                        vtbs房间数据 = MMPU.返回网页内容_GET(VTBS.API.VTBS服务器CDN.VTBS_Url + "/v1/short", 5000);
                    }
                    catch (Exception)
                    {
                        vtbs房间数据 = MMPU.TcpSend(Server.RequestCode.GET_VTBSROOMLIST, "{}", true);
                    }

                    JArray JO = (JArray)JsonConvert.DeserializeObject(vtbs房间数据);
                    foreach (var item in JO)
                    {
                        if (int.Parse(item["roomid"].ToString()) != 0)
                        {
                            VtbsRoomList.Add(new VtbsRoom() { mid = int.Parse(item["mid"].ToString()), room = int.Parse(item["roomid"].ToString()), name = item["uname"].ToString() });
                        }
                    }
                    wss连接初始化准备已完成 = true;
                    break;
                }
                catch (Exception)
                {
                }
               
                Thread.Sleep(5000);
            }
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
                            InfoLog.InfoPrintf("本地房间状态更新结束", InfoLog.InfoClass.Debug);
                            是否正在更新房间信息 = false;
                            break;
                        }
                    case 1:
                        {
                            //启动WS房间连接
                         
                            是否正在更新房间信息 = false;
                            break;
                        }
                    case 2:
                        {
                            使用B站API更新房间状态();
                            InfoLog.InfoPrintf("当前阿B API调用次数为:" + DataCache.CacheCount, InfoLog.InfoClass.杂项提示);
                            InfoLog.InfoPrintf("本地房间状态更新结束", InfoLog.InfoClass.Debug);
                            是否正在更新房间信息 = false;
                            break;
                        }
                    default:
                        break;
                   
                }
                是否正在更新房间信息 = false;
            }     
        }
        public static void 使用vtbsAPI更新房间状态()
        {
            int C1 = 0;
            int C2 = RoomList.Count;
            try
            {
                while (!wss连接初始化准备已完成)
                {
                    Thread.Sleep(1000);
                }
                JArray JO = (JArray)JsonConvert.DeserializeObject(MMPU.返回网页内容_GET(VTBS.API.VTBS服务器CDN.VTBS_Url+"/v1/living",8000));
                
                List<int> MTPlist = new List<int>();
                foreach (var item in VtbsRoomList)
                {
                    MTPlist.Add(item.room);
                }
                List<RoomInit.RoomInfo> Vtbs存在的直播间 = new List<RoomInit.RoomInfo>();
                List<RoomInit.RoomInfo> Vtbs不存在的直播间 = new List<RoomInit.RoomInfo>();
                foreach (var item in RoomList)
                {
                    if (MTPlist.Contains(int.Parse(item.房间号)))
                    {
                        Vtbs存在的直播间.Add(item);
                    }
                    else
                    {
                        Vtbs不存在的直播间.Add(item);
                    }
                }

                foreach (var roomtask in Vtbs存在的直播间)
                {
                    if(!WSokRoomList.Contains(roomtask.房间号))
                    {
                        C1++;
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
                    else
                    {
                        ;
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
                        if (!roomtask.名称.Contains("-NV"))
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
                        else
                        {
                            ;
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
            InfoLog.InfoPrintf("使用VTBS数据库加载数据量:"+C1.ToString()+"/"+C2.ToString(), InfoLog.InfoClass.Debug);
        }
        public static void 使用B站API更新房间状态()
        {
        
            foreach (var roomtask in RoomList)
            {
                if (!roomtask.名称.Contains("-NV"))
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
                }
                Thread.Sleep(2000);
            }
        }

        public static List<直播间状态> 已连接的直播间状态 = new List<直播间状态>();
        public class 直播间状态
        {
            public int 房间号 { set; get; }
            public int 心跳值 { set; get; }
            public DateTime 心跳时间 { set; get; }
        }
        public class 持续连接获取阿B房间信息类
        {
            public static void 初始化所有房间连接()
            {
                if (!WS连接组是否已经启动)
                {
                    WS连接组是否已经启动 = true;

                    new Task(() =>
                    {
                        int TJ = 16;
                        while (true)
                        {
                            try
                            {
                                
                                if(TJ>15)
                                {
                                    InfoLog.InfoPrintf($"[DDTVLR心跳信息]当前临时API监控房间数量:{RoomList.Count- 已连接的直播间状态.Count},稳定WSS长连接监控房间数量:{已连接的直播间状态.Count}" , InfoLog.InfoClass.下载必要提示);
                                    TJ = 0;
                                }
                                TJ++;
                                int num = 1;
                                string BB = string.Empty;
                                foreach (var item in 已连接的直播间状态)
                                {
                                    TimeSpan ts = DateTime.Now.Subtract(item.心跳时间);
                                    BB += "\r\n" + num + "　时间差:" + (int)ts.TotalSeconds + "　　　房间号:" + item.房间号 + "　　心跳值:" + item.心跳值 + "　　上次更新时间" + item.心跳时间;
                                    //Console.WriteLine(num + "　时间差:" + (int)ts.TotalSeconds + "　　　房间号:" + item.房间号 + "　　心跳值:" + item.心跳值 + "　　上次更新时间" + item.心跳时间);
                                    num++;
                                }
                                InfoLog.InfoPrintf("wss连接状态:" + BB, InfoLog.InfoClass.Debug);
                                for (int i = 0; i < 已连接的直播间状态.Count; i++)
                                {
                                    TimeSpan ts = DateTime.Now.Subtract(已连接的直播间状态[i].心跳时间);
                                    if ((int)ts.TotalSeconds < 0 || (int)ts.TotalSeconds > 80)
                                    {
                                        int 房间号 = 已连接的直播间状态[i].房间号;
                                        已连接的直播间状态.Remove(已连接的直播间状态[i]);
                                        foreach (var item2 in RoomList)
                                        {
                                            if (房间号 == int.Parse(item2.房间号))
                                            {
                                                if (item2.平台 == "bilibili")
                                                {
                                                    try
                                                    {
                                                        item2.liveChatListener.Dispose();
                                                    }
                                                    catch (Exception) { }
                                                    item2.liveChatListener = new LiveChatListener();
                                                    item2.liveChatListener.Connect(int.Parse(item2.房间号));
                                                    item2.liveChatListener.MessageReceived += 直播间状态WS变化事件;
                                                    已连接的直播间状态.Add(new 直播间状态() { 房间号 = int.Parse(item2.房间号) });
                                                    WSokRoomList.Add(item2.房间号);
                                                    InfoLog.InfoPrintf("-ALL 发起房间监控连接请求：)" + item2.房间号.ToString(), InfoLog.InfoClass.Debug);
                                                    Thread.Sleep(10000);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                
                            }
                            catch (Exception)
                            {
                            }
                            Thread.Sleep(10000);
                        }
                    }).Start();
                    while (!wss连接初始化准备已完成)
                    {
                        Thread.Sleep(1000);
                    }
                    List<int> MTPlist = new List<int>();
                    foreach (var item in VtbsRoomList)
                    {
                        MTPlist.Add(item.room);
                    }
                    List<RoomInit.RoomInfo> Vtbs存在的直播间 = new List<RoomInit.RoomInfo>();
                    List<RoomInit.RoomInfo> Vtbs不存在的直播间 = new List<RoomInit.RoomInfo>();
                    foreach (var item in RoomList)
                    {
                        if(MTPlist.Contains(int.Parse(item.房间号)))
                        {
                            Vtbs存在的直播间.Add(item);
                        }
                        else
                        {
                            Vtbs不存在的直播间.Add(item);
                        }
                    }
                    foreach (var item in Vtbs不存在的直播间)
                    {
                        try
                        {
                            InfoLog.InfoPrintf("LiveChatListener开始连接非VTBS监控范围内房间:" + item.房间号, InfoLog.InfoClass.Debug);
                            if (item.平台 == "bilibili")
                            {
                                item.liveChatListener = new LiveChatListener();
                                item.liveChatListener.Connect(int.Parse(item.房间号));
                                item.liveChatListener.MessageReceived += 直播间状态WS变化事件;
                                已连接的直播间状态.Add(new 直播间状态() { 房间号 = int.Parse(item.房间号) });
                                WSokRoomList.Add(item.房间号);
                                InfoLog.InfoPrintf("-ALL 发起房间监控连接请求：)" + item.房间号.ToString(), InfoLog.InfoClass.Debug);
                            }
                            Thread.Sleep(10000);
                        }
                        catch (Exception) { }
                        Thread.Sleep(5000);
                    }
                    foreach (var item in Vtbs存在的直播间)
                    {
                        try
                        {
                            InfoLog.InfoPrintf("LiveChatListener开始连接常规V房间:"+ item.房间号, InfoLog.InfoClass.Debug);
                            if (item.平台 == "bilibili")
                            {
                                item.liveChatListener = new LiveChatListener();
                                item.liveChatListener.Connect(int.Parse(item.房间号));
                                item.liveChatListener.MessageReceived += 直播间状态WS变化事件;
                                已连接的直播间状态.Add(new 直播间状态() { 房间号 = int.Parse(item.房间号) });
                                WSokRoomList.Add(item.房间号);
                                InfoLog.InfoPrintf("-ALL 发起房间监控连接请求：)" + item.房间号.ToString(), InfoLog.InfoClass.Debug);
                            }
                            Thread.Sleep(10000);
                        }
                        catch (Exception){}
                        Thread.Sleep(5000);
                    }
                }
            }
           
            private static void 直播间状态WS变化事件(object sender, MessageEventArgs e)
            {
                string CacheStr = "byRoomIdgetLiveStatus";
                try
                {
                    switch (e)
                    {
                        case LivePopularity LiveP:
                            foreach (var item in 已连接的直播间状态)
                            {
                                if (item.房间号 == LiveP.roomID)
                                {
                                    item.心跳值 = LiveP.LiveP;
                                    item.心跳时间 = DateTime.Now;
                                    //InfoLog.InfoPrintf("更新心跳：" + item.房间号.ToString(), InfoLog.InfoClass.Debug);
                                    //if(!已连接的直播间状态.Contains(new 直播间状态() { 房间号 = item.房间号 }))
                                    //{
                                    //    已连接的直播间状态.Add(new 直播间状态() { 房间号 = item.房间号 });
                                    //    InfoLog.InfoPrintf("-ALL 添加开始监控的房间：)" + item.房间号.ToString(), InfoLog.InfoClass.Debug);
                                    //}
                                    break;
                                }
                            }
                            foreach (var roomtask in RoomList)
                            {
                                if (roomtask.房间号 == LiveP.roomID.ToString())
                                {
                                    if (LiveP.LiveP > 1)
                                    {
                                      
                                        DataCache.写缓存(CacheStr + LiveP.roomID, "1");
                                        roomtask.直播状态 = true;
                                        break;
                                    }
                                    else
                                    {
                                        DataCache.写缓存(CacheStr + LiveP.roomID, "0");
                                        roomtask.直播状态 = false;
                                        break;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                }
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
                    { "rnd", (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalSeconds.ToString() },
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
                JObject result = JObject.Parse(roomHtml);
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
            public static bool 是否正在直播(string RoomId,bool 强制刷新)
            {
                string CacheStr = "byRoomIdgetLiveStatus";
                
                if (!强制刷新&&DataCache.读缓存(CacheStr + RoomId, 5, out string CacheData))
                {
                    if(CacheData=="1")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                string roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + RoomId;
                WebClient wc = new WebClient();
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
                        DataCache.写缓存(CacheStr + RoomId, "0");
                        return false;
                    }

                }

                //解析返回结果
                try
                {
                    string roomJson = Encoding.UTF8.GetString(roomHtml);
                    JObject result = JObject.Parse(roomJson);
                    DataCache.CacheCount++;
                    if (result["data"]["live_status"].ToString() == "1")
                    {
                        DataCache.写缓存(CacheStr + RoomId, "1");
                        return true;
                    }
                    else
                    {
                        DataCache.写缓存(CacheStr + RoomId, "0");
                        return false;
                    }
                   
                }
                catch (Exception)
                {
                    try
                    {
                        string roomJson = Encoding.UTF8.GetString(roomHtml);
                        JObject result = JObject.Parse(roomJson);
                        DataCache.CacheCount++;
                        if (result["data"]["live_status"].ToString() == "1")
                        {
                            DataCache.写缓存(CacheStr + RoomId, "1");
                            return true;
                        }
                        else
                        {
                            DataCache.写缓存(CacheStr + RoomId, "0");
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        DataCache.写缓存(CacheStr + RoomId, "0");
                        return false;
                    }

                }
            }

            public static string 获取标题(string roomID)
            {
                string CacheStr = "byRoomIdgetRoomTitle";
                if (DataCache.读缓存(CacheStr + roomID, 20, out string CacheData))
                {
                    return CacheData;
                }
                roomID = 获取真实房间号(roomID);
                if (roomID == null)
                {
                    InfoLog.InfoPrintf("房间号获取错误", InfoLog.InfoClass.Debug);
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
                    DataCache.写缓存(CacheStr + roomID, roomName);
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
                        try
                        {
                            if((JObject.Parse(resultString)["message"].ToString()=="房间已加密"))
                            {
                                InfoLog.InfoPrintf("房间已加密", InfoLog.InfoClass.下载必要提示);
                                return "";
                            }
                        }
                        catch (Exception)
                        {
                        }
                        BBBC = (JObject.Parse(resultString)["data"]["durl"][1]["url"].ToString());
                    }
                    DataCache.CacheCount++;
                    return BBBC;
                }
                catch (Exception e)
                {
                    InfoLog.InfoPrintf("视频流地址解析失败：" + e.Message, InfoLog.InfoClass.系统错误信息);
                    return "";
                }
            }

            public static string 获取真实房间号(string roomID)
            {
                string CacheStr = "byROOMIDgetTRUEroomid";
                try
                {
                    if(int.Parse(roomID)>10000)
                    {
                        DataCache.写缓存(CacheStr + roomID, roomID);
                        return roomID;
                    }
                }
                catch (Exception){}
                
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
                DataCache.CacheCount++;
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
               // Thread.Sleep(100);
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
                RoomInit.RoomInfo roominfo = new RoomInit.RoomInfo
                {
                    房间号 = result["data"]["room_id"].ToString(),
                    标题 = result["data"]["title"].ToString().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("\"", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                    直播状态 = result["data"]["live_status"].ToString() == "1" ? true : false,
                    UID = result["data"]["uid"].ToString(),
                    直播开始时间 = result["data"]["live_time"].ToString(),
                    平台 = "bilibili"
                };
                InfoLog.InfoPrintf("获取到房间信息:" + roominfo.UID + " " + (roominfo.直播状态 ? "已开播" : "未开播") + " " + (roominfo.直播状态 ? "开播时间:" + roominfo.直播开始时间 : ""), InfoLog.InfoClass.Debug);
                DataCache.CacheCount++;
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
                
                if(MMPU.启动模式==0)
                {
                    ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                    ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                    ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true).Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
                }
                else if(MMPU.启动模式==1)
                {
                    Console.WriteLine("配置引导方式:手机登陆");
                    Console.Write("请输入手机号以验证短信验证码:");
                    string tel = Console.ReadLine();
                    BySMS.SendSMS(tel);
                    Console.Write("请输入收到的验证码:");
                    string code = Console.ReadLine();
                    try
                    {
                        BiliUser.account = BySMS.Login(code, tel);
                    }
                    catch (Exception)
                    {
                        ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                        ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                        ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true).Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
                        InfoLog.InfoPrintf("短信验证登陆失败，切换备用二维码扫码登陆方式， 请用阿B手机客户端扫描DDTVLiveRec目录中的[BiliQR.png]文件进行登陆", InfoLog.InfoClass.系统错误信息);
                        return;
                    }
                   
                    MMPU.UID = account.Uid;
                    MMPU.写ini配置文件("User", "UID", MMPU.UID, MMPU.BiliUserFile);
                    foreach (var item in account.Cookies)
                    {
                        MMPU.Cookie = MMPU.Cookie + item + ";";
                    }
                    MMPU.CookieEX = account.Expires_Cookies;
                    MMPU.csrf = account.CsrfToken;
                    
                    MMPU.写ini配置文件("User", "csrf", MMPU.csrf, MMPU.BiliUserFile);
                    MMPU.写ini配置文件("User", "Cookie", Encryption.AesStr(MMPU.Cookie, MMPU.AESKey, MMPU.AESVal), MMPU.BiliUserFile);
                    MMPU.写ini配置文件("User", "CookieEX", MMPU.CookieEX.ToString("yyyy-MM-dd HH:mm:ss"), MMPU.BiliUserFile);
                    InfoLog.InfoPrintf("UID:" + account.Uid + ",登陆成功", InfoLog.InfoClass.下载必要提示);
                }
                
                
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
                    File.AppendAllText(MMPU.BiliUserFile, "#本文件为BiliBili扫码登陆缓存，为登陆缓存cookie，不包含账号密码，请注意");
                    //new Task(() =>
                    //{
                    //    //File.Create(MMPU.BiliUserFile);//创建INI文件
                      

                    //}).Start();
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


            public static void AAA()
            {
                         

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
}

