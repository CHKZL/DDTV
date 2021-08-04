using Fleck;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Auxiliary.WSServer
{
    public class WSServer
    {
        public static WebSocketServer server = new WebSocketServer($"ws://0.0.0.0:{MMPU.WebSocket端口}");
        public static bool IsOpen = false;
        public static List<WebSocket连接封装> webSockets = new List<WebSocket连接封装>();

        public static void Open()
        {
            FleckLog.Level = LogLevel.Debug;
            if (MMPU.是否启用SSL)
            {
                server = new WebSocketServer($"wss://0.0.0.0:{MMPU.WebSocket端口}");
                server.Certificate = new X509Certificate2(MMPU.webServer_pfx证书名称, MMPU.webServer_pfx证书密码, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                InfoLog.InfoPrintf("WebSocket服务端启动，测到加密证书，WS服务器以wss模式启动，请使用" + $"wss://设备IP:{MMPU.WebSocket端口}来访问WebSocket服务器", InfoLog.InfoClass.系统强制信息);
            }
            else
            {
                server = new WebSocketServer($"ws://0.0.0.0:{MMPU.WebSocket端口}");
                InfoLog.InfoPrintf("WebSocket服务端启动，未检测到加密证书，WS服务器以ws模式启动，请使用" + $"ws://设备IP:{MMPU.WebSocket端口}来访问WebSocket服务器", InfoLog.InfoClass.系统强制信息);
            }
           
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    InfoLog.InfoPrintf("WebSocket客户端连接:" + socket.ConnectionInfo.ClientIpAddress + "  " + $"[{socket.ConnectionInfo.Id}]", InfoLog.InfoClass.Debug);
                    webSockets.Add(new WebSocket连接封装() { webSocketConnection = socket, IsCheck = true });
                };
                socket.OnClose = () =>
                {
                    InfoLog.InfoPrintf("WebSocket客户端断开连接:" + socket.ConnectionInfo.ClientIpAddress + "  " + $"[{socket.ConnectionInfo.Id}]", InfoLog.InfoClass.Debug);
                    foreach (var item in webSockets)
                    {
                        if (item.webSocketConnection == socket)
                        {
                            item.IsCheck = false;
                        }
                    }
                };
                socket.OnMessage = message =>
                {
                    foreach (var item in webSockets)
                    {
                        if (item.webSocketConnection == socket)
                        {
                            item.webSocketConnection.Send(MessageProcessing.消息解析(message, item));
                        }
                    }

                //socket.ForEach(s => s.Send("Echo: " + message));
            };
            });
        }
        public static void AllSend(string text)
        {
            foreach (var socket in webSockets)
            {
                try
                {
                    if (socket.IsCheck)
                        socket.webSocketConnection.Send(text);
                }
                catch (Exception)
                {
                }
            }
        }
        public class WebSocket连接封装
        {
            public IWebSocketConnection webSocketConnection { set; get; }
            public string Token { set; get; } = null;
            public bool IsCheck { set; get; } = false;
        }
    }
}
