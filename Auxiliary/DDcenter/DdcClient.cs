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
                    MMPU.DDC采集间隔 = int.Parse(MMPU.TcpSend(30001, "{}", true));
                }
                catch (Exception)
                {
                    MMPU.DDC采集间隔 = 1000;
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

            public async void WebSocket()
            {
                if (MMPU.启动模式 == 1)
                {
                    InfoLog.InfoPrintf("环境不满足启动DDC采集，取消DDC上传任务", InfoLog.InfoClass.Debug);
                    return;
                }
                while (true)
                {
                    try
                    {
                        InfoLog.InfoPrintf("DDC采集开始", InfoLog.InfoClass.Debug);

                        await _webSocket.ConnectAsync(new Uri("wss://cluster.vtbs.moe/?runtime=DDTV" + MMPU.版本号 + "&version=" + MMPU.版本号 + "&platform=" + (MMPU.启动模式 == 0 ? "win32" : "linux") + "&name=" + (MMPU.启动模式 == 0 ? "DDTV|" + Encryption.机器码.获取机器码("1145141919810") : "DDTVLiveRec")), _cancellation);

                        while (true)
                        {
                            while (MMPU.DDC采集使能)
                            {
                                try
                                {
                                    await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("DDhttp")), WebSocketMessageType.Text, true, _cancellation);
                                    var result = new byte[1024];
                                    await _webSocket.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken());
                                    JObject str = JObject.Parse(Encoding.UTF8.GetString(result, 0, result.Length).Trim('\0'));
                                    getUrl((string)str["data"]["url"], (string)str["key"], out string instr);
                                    if (!string.IsNullOrEmpty(instr))
                                    {
                                        await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(instr)), WebSocketMessageType.Text, true, _cancellation);
                                        InfoLog.InfoPrintf("DDC采集成功\n" + str + "\n\n" + instr, InfoLog.InfoClass.Debug);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                Thread.Sleep(MMPU.DDC采集间隔);
                            }
                            Thread.Sleep(10000);
                        }
                    }
                    catch (Exception ex)
                    {
                        _webSocket.Dispose();
                        _webSocket = new ClientWebSocket();
                        await _webSocket.ConnectAsync(new Uri("wss://cluster.vtbs.moe"), _cancellation);
                        InfoLog.InfoPrintf("DDC采集失败:" + ex.ToString(), InfoLog.InfoClass.Debug);
                    }
                    Thread.Sleep(1000);
                }
            }

            #endregion
            public void getUrl(string url, string key,out string str)
            {
                try
                {
                    var wc = new WebClient();
                    wc.Headers.Add("Accept: */*");
                    wc.Headers.Add("User-Agent: AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.119 Safari/537.36");
                    wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

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
