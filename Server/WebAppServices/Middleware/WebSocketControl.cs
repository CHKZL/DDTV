using Core.LogModule;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.WebAppServices.Middleware
{
    public class WebSocketControl
    {
        private readonly RequestDelegate _next;

        public static List<WebSocket> webSockets = new List<WebSocket>();

        public WebSocketControl(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    webSockets.Add(webSocket);
                    try
                    {
                        await Echo(context, webSocket);
                    }
                    catch (Exception)
                    {
                        Log.Info(nameof(WebSocketControl),"WS握手/连接出现错误",false);
                        if (webSockets.Contains(webSocket))
                        {
                            _ = webSockets.Remove(webSocket);
                        }
                    }
                    
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {             
                await _next(context);
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 8];

            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.Connecting)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }

        }
    }

}
