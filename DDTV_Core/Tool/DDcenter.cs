using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DDTV_Core.InitDDTV_Core;

namespace DDTV_Core.Tool
{
    public static class DDcenter
    {
        public static bool DDcenterSwitch = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DDcenterSwitch, "False", CoreConfigClass.Group.Core));
        public static int TimeIntervalBetween = 60 * 1000;
        private static ClientWebSocket _webSocket = null;
        private static CancellationToken _cancellation = new CancellationToken();
        private static string Type = string.Empty;
        private static string OS = string.Empty;
        private static bool IsOn = false;

        private static long Hits = 0;
        private static SatrtType ST = SatrtType.DDTV_Core;
        public static async void Init(SatrtType satrtType)
        {
            try
            {
                ST = satrtType;
                Dokidoki.DDCtime.Start();
                Type = "";
                OS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Win_x64" : "Linux_x64";
                switch (satrtType)
                {
                    case SatrtType.DDTV_GUI:
                        Type = "DDTV_GUI_x64";
                        break;
                    case SatrtType.DDTV_WEB:
                        Type = "DDTV_WEB_x64";
                        break;
                    case SatrtType.DDTV_CLI:
                        Type = "DDTV_CLI_x64";
                        break;
                    default:
                        Type = "DDTV_CLI_x64";
                        break;
                }
                if (_webSocket != null)
                {
                    _webSocket.Dispose();
                }
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(
                                "wss://cluster.vtbs.moe/?uuid=DDTVvtbs" +
                                "&runtime=" + Type + "|" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version +
                                "&version=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version +
                                "&platform=" + OS +
                                "&name=" + SystemAssembly.ConfigModule.CoreConfig.InstanceAID), _cancellation);
                start();
            }
            catch (Exception)
            {

            }

        }
        public static async void start()
        {
            if (!IsOn && _webSocket != null)
            {
                Thread.Sleep(1000);
                HandleRequest();
                while (true)
                {
                    IsOn = true;
                    while (DDcenterSwitch)
                    {

                        HandleRequest();
                        Thread.Sleep(TimeIntervalBetween);
                    }
                    if (!DDcenterSwitch)
                    {
                        Thread.Sleep(TimeIntervalBetween);
                    }
                   
                }
            }
        }
        public static void Stop()
        {
            DDcenterSwitch = false;
        }
        private static async void HandleRequest()
        {
            try
            {
                if (_webSocket != null)
                {
                    await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("DDDhttp")), WebSocketMessageType.Text, true, _cancellation);
                    var result = new byte[1024];
                    await _webSocket.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken());
                    string JsonStr = Encoding.UTF8.GetString(result, 0, result.Length).Trim('\0');
                    JObject JO = JObject.Parse(JsonStr);
                    if (JO.TryGetValue("data", out var K))
                    {
                        switch ((string)K["type"])
                        {
                            case "http":
                                getUrl((string)JO["data"]["url"], (string)JO["key"], out string instr);
                                if (!string.IsNullOrEmpty(instr))
                                {
                                    await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(instr)), WebSocketMessageType.Text, true, _cancellation);
#if DEBUG
                                    Log.AddLog(nameof(DDcenter), LogClass.LogType.Debug, $"DDC采集成功", false, null, true);
#else
                        Log.AddLog(nameof(DDcenter), LogClass.LogType.Debug_DDcenter, $"DDC采集成功", false, null, false);
#endif
                                    Hits++;
                                    //Console.WriteLine("DDC采集{0}次", Hits);
                                }
                                break;
                            //case "wait":
                            //    Thread.Sleep(TimeIntervalBetween * 3);
                            //    break;
                            default:
#if DEBUG
                                Log.AddLog(nameof(DDcenter), LogClass.LogType.Debug, $"DDC采集失败:{(string)K["type"]}", false, null, true);
#else
                        Log.AddLog(nameof(DDcenter), LogClass.LogType.Debug_DDcenter, $"DDC采集成功:{(string)K["type"]}", false, null, false);
#endif
                                Thread.Sleep(TimeIntervalBetween);
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //_webSocket.Dispose();
                //_webSocket = null;
                ////_webSocket = new ClientWebSocket();
                //Init(ST);
            }
        }
        private static void getUrl(string url, string key, out string str)
        {
            try
            {
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: DDTV_GUI_" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                //发送HTTP请求
                byte[] roomHtml = wc.DownloadData(url);
                JsonTask a = new JsonTask() { key = key, data = Encoding.UTF8.GetString(roomHtml) };
                JsonConvert.SerializeObject(a);
                str = JsonConvert.SerializeObject(a);
            }
            catch (Exception)
            {
                str = null;
            }
        }
        private class JsonTask
        {
            public string key { set; get; }
            public string data { set; get; }
        }
    }
}
