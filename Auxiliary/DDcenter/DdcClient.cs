using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auxiliary.DDcenter
{
    public class DdcClient
    {
        public static void Connect()
        {
            new Task(() =>
            {
                try
                {
                    MMPU.DDC采集间隔 = int.Parse(MMPU.TcpSend(Server.RequestCode.GET_DDC_TIME_NUMBER, "{}", true,50));
                }
                catch (Exception)
                {
                    //如果服务器无响应或者返回内容错误，保守使用延迟
                    MMPU.DDC采集间隔 = 60000;
                }
                DDC DDC = new DDC();
                DDC.WebSocket();
            }).Start();
        }
        public class DDC
        {

            #region ClientWebSocket

            public ClientWebSocket _webSocket = new ClientWebSocket();
            public CancellationToken _cancellation = new CancellationToken();
            public static int 采集次数 = 0;
            public async void WebSocket()
            {
                if (MMPU.启动模式 == 1 || MMPU.数据源==1)
                {
                    InfoLog.InfoPrintf("环境不满足启动DDC采集，取消DDC上传任务", InfoLog.InfoClass.Debug);
                    return;
                }
                while (MMPU.DDC采集使能)
                {
                    try
                    {
                        InfoLog.InfoPrintf("vtbs监听启动", InfoLog.InfoClass.Debug);

                        await _webSocket.ConnectAsync(new Uri(
                            "wss://cluster.vtbs.moe/?uuid=DDTVvtbs" +
                            "&runtime=DDTV" + MMPU.DDTV版本号 + 
                            "&version=" + MMPU.DDTV版本号 + 
                            "&platform=" + (MMPU.启动模式 == 0 ? "win64" : "linux") + 
                            "&name=" + (MMPU.启动模式 == 0 ? "DDTV|" + Encryption.机器码.获取机器码("1145141919810") : "DDTVLiveRec")), _cancellation);


                        while (MMPU.DDC采集使能)
                            
                            {
                            await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("DDDhttp")), WebSocketMessageType.Text, true, _cancellation);
                            var result = new byte[1024];
                            await _webSocket.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken());
                            string JsonStr = Encoding.UTF8.GetString(result, 0, result.Length).Trim('\0');
                            JObject JO = JObject.Parse(JsonStr);
                            switch((string)JO["data"]["type"])
                            {
                                case "http":
                                    getUrl((string)JO["data"]["url"], (string)JO["key"], out string instr);
                                    if (!string.IsNullOrEmpty(instr))
                                    {
                                        await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(instr)), WebSocketMessageType.Text, true, _cancellation);
                                        InfoLog.InfoPrintf("DDC采集成功\n" + JO + "\n\n" + instr, InfoLog.InfoClass.Debug);
                                        采集次数++;
                                        Console.WriteLine("DDC采集{0}次", 采集次数);
                                    }
                                    break;
                                case "wait":
                                    Thread.Sleep(MMPU.DDC采集间隔*3);
                                    break;
                                default:
                                    break;
                            }
                            Thread.Sleep(MMPU.DDC采集间隔);
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _webSocket.Dispose();
                            _webSocket = new ClientWebSocket();
                        }
                        catch (Exception)
                        {

                        }
                        InfoLog.InfoPrintf("DDC采集失败:" + ex.ToString(), InfoLog.InfoClass.Debug);
                    }
                    Thread.Sleep(10000);
                }
            }

            #endregion
            public void getUrl(string url, string key, out string str)
            {
                try
                {
                    var wc = new WebClient();
                    wc.Headers.Add("Accept: */*");
                    wc.Headers.Add("User-Agent: " + MMPU.UA.Ver.UA());
                    wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
                    if (url.Contains("bilibili"))
                    {
                        if (!string.IsNullOrEmpty(MMPU.Cookie))
                        {
                            wc.Headers.Add("Cookie", MMPU.Cookie);
                        }
                    }
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
            public class JsonTask
            {
                public string key { set; get; }
                public string data { set; get; }
            }
        }
    }
}
