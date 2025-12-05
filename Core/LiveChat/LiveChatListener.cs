using AngleSharp.Dom;
using Core.LogModule;
using Core.Network;
using Core.Network.Methods;
using Core.RuntimeObject;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;

namespace Core.LiveChat
{
    public class LiveChatListener : IDisposable
    {
        #region Properties
        private ClientWebSocket m_client;
        private byte[] m_ReceiveBuffer;
        private CancellationTokenSource m_innerRts;
        private DanMuWssInfo WssInfo = new();
        private bool _disposed = false;
        public bool _Cancel = false;

        public long RoomId { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<EventArgs> DisposeSent;
        public bool State { get; set; } = false;
        public List<string> Register { get; set; } = new();
        public RuntimeObject.Danmu.DanmuMessage DanmuMessage { get; set; } = new();
        public Stopwatch TimeStopwatch { get; set; }
        public int SaveCount { get; set; } = 1;
        #endregion

        #region Public Method
        public LiveChatListener(long roomId,Danmu.DanmuMessage danmuMessage = null)
        {
            RoomId = roomId;
            Name = RoomInfo.GetNickname(RoomInfo.GetUid(roomId));
            File = $"{Config.Core_RunConfig._RecFileDirectory}{Core.Tools.KeyCharacterReplacement.ReplaceKeyword($"{Config.Core_RunConfig._DefaultLiverFolderName}/{Core.Config.Core_RunConfig._DefaultDataFolderName}{(string.IsNullOrEmpty(Core.Config.Core_RunConfig._DefaultDataFolderName) ? "" : "/")}{Config.Core_RunConfig._DefaultFileName}", DateTime.Now, RoomInfo.GetUid(roomId))}";
            if (danmuMessage == null)
            {
                DanmuMessage = new RuntimeObject.Danmu.DanmuMessage();
            }
            else
            {
                DanmuMessage = danmuMessage;
            }
        }

        /// <summary>
        /// 连接到直播间弹幕服务器
        /// </summary>
        public async void Connect()
        {
            try
            {
                m_ReceiveBuffer = new byte[1024 * 512];
                State = true;

                await ConnectAsync();
            }
            catch (Exception e)
            {
                Log.Error(nameof(LiveChatListener) + "_" + nameof(Connect), $"LiveChatListener初始化Connect出现错误", e, true);
                Dispose();
            }
        }


        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (Register.Count != 0 && !_Cancel)
            {
                MessageReceived?.Invoke(this, new MessageEventArgs(JsonNode.Parse("{\"cmd\":\"Reconnect\"}").AsObject()));
                return;
            }
            _Cancel = true;
            
            m_ReceiveBuffer = null;

            if (TimeStopwatch != null)
            {
                TimeStopwatch.Stop();
                TimeStopwatch = null;
            }

            try
            {
                m_innerRts?.Cancel();
            }
            catch (Exception) { }

            try
            {
                m_client?.Dispose();
            }
            catch (Exception) { }

            try
            {
                m_innerRts?.Dispose();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (!_disposed)
                {
                    Close();
                }
                _disposed = true;
                State = false;

                DisposeSent?.Invoke(this, EventArgs.Empty);

                if (Register.Count != 0)
                {
                    // 如果有注册信息，可以在这里处理
                }
            }
            catch (Exception E)
            {
                Log.Error(nameof(LiveChatListener), "关闭LiveChatListener连接时出现未知错误", E, false);
            }
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 获取弹幕服务器WebSocket信息
        /// </summary>
        private DanMuWssInfo GetWssInfo()
        {
            string WebText = Get.GetBody($"{Config.Core_RunConfig._LiveDomainName}/xlive/web-room/v1/index/getDanmuInfo?id={RoomId}&type=0&web_location=444.8", true, IsRid: true);
            DanMuWssInfo roomInfo = new();

            try
            {
                roomInfo = JsonSerializer.Deserialize<DanMuWssInfo>(WebText);
                return roomInfo;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// 异步连接到弹幕服务器
        /// </summary>
        private async Task ConnectAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LiveChatListener));
            }

            m_client = new ClientWebSocket();
            m_innerRts = new CancellationTokenSource();

            try
            {
                int attempt = 0;
                const int MaxRetryAttempts = 3;
                const int RetryDelayWhenNoHosts = 5000;
                const int NormalRetryDelay = 1000;
                const string LogPrefix = $"{nameof(LiveChatListener)}_{nameof(ConnectAsync)}";

                // 重试获取WSS信息
                do
                {
                    WssInfo = GetWssInfo();
                    attempt++;

                    if (WssInfo == null)
                    {
                        Log.Warn($"{LogPrefix}_{nameof(GetWssInfo)}", "请求wss地址出现空地址", null, false);
                    }
                    else if (WssInfo.data.host_list.Count < 1)
                    {
                        Log.Debug($"{LogPrefix}", "获取到的host列表为空，等待重试...");
                    }

                    if (attempt >= MaxRetryAttempts)
                    {
                        Log.Warn(LogPrefix, "LiveChatListener连接3次都超时，放弃本次连接，10秒后重试", null, true);
                        Dispose();
                        return;
                    }

                    if (WssInfo?.data.host_list.Count < 1)
                    {
                        await Task.Delay(RetryDelayWhenNoHosts);
                        Log.Warn(LogPrefix, "LiveChatListener连接成功，但是获取到的host_list为空，5秒后重试", null, true);
                    }
                    else
                    {
                        await Task.Delay(NormalRetryDelay);
                    }

                } while (WssInfo == null || WssInfo.data.host_list.Count < 1);

                // 随机选择一个服务器连接
                string URL = "wss://" + WssInfo.data.host_list[new Random().Next(0, WssInfo.data.host_list.Count)].host + "/sub";
                await m_client.ConnectAsync(new Uri(URL), new CancellationTokenSource().Token);

                // 发送认证信息
                await _sendObject(7, new
                {
                    uid = long.Parse(RuntimeObject.Account.AccountInformation.Uid),
                    roomid = RoomId,
                    protover = 3,
                    buvid = RuntimeObject.Account.AccountInformation.Buvid,
                    platform = "web",
                    type = 2,
                    key = WssInfo.data.token
                });

                // 启动接收循环
                _ = _innerLoop().ContinueWith((t) =>
                {
                    if (t.IsFaulted)
                    {
                        if (!m_innerRts.IsCancellationRequested)
                        {
                            MessageReceived?.Invoke(this, new ExceptionEventArgs(t.Exception.InnerException));
                            m_innerRts.Cancel();
                        }
                    }
                    else
                    {
                        Log.Info(nameof(LiveChatListener) + "_" + nameof(ConnectAsync), $"LiveChatListener连接断开并回收");
                        Dispose();
                    }

                    try
                    {
                        if (m_client.State == WebSocketState.Open && !_disposed)
                        {
                            m_client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(LiveChatListener) + "_" + nameof(ConnectAsync), $"LiveChatListener连接发生意料外的错误", ex, false);
                        Dispose();
                    }
                });

                // 启动心跳任务
                _ = _innerHeartbeat();
            }
            catch (Exception e)
            {
                Log.Error(nameof(LiveChatListener) + "_" + nameof(ConnectAsync), $"LiveChatListener连接发生错误", e, true);
                Dispose();
            }
        }

        /// <summary>
        /// 心跳任务，每10秒发送一次心跳包
        /// </summary>
        private async Task _innerHeartbeat()
        {
            while (!m_innerRts.IsCancellationRequested)
            {
                try
                {
                    await _sendBinary(2, Encoding.UTF8.GetBytes("[object Object]"));
                    await Task.Delay(10 * 1000, m_innerRts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Log.Error(nameof(_innerHeartbeat), $"检测到网络环境发生变化，出现错误", e);
                    break;
                }
            }
        }

        /// <summary>
        /// 接收消息的主循环
        /// </summary>
        private async Task _innerLoop()
        {
#if DEBUG
            Log.Info(nameof(LiveChatListener) + "_" + nameof(_innerLoop), $"建立与(room_id:{RoomId})直播间的长连");
#endif
            TimeStopwatch = Stopwatch.StartNew();

            while (!m_innerRts.IsCancellationRequested)
            {
                try
                {
                    WebSocketReceiveResult result = null;
                    int length = 0;

                    do
                    {
                        try
                        {
                            result = await m_client.ReceiveAsync(new ArraySegment<byte>(m_ReceiveBuffer, length, m_ReceiveBuffer.Length - length), m_innerRts.Token);
                            length += result.Count;
                        }
                        catch (Exception e)
                        {
                            Dispose();
                            return;
                        }
                    }
                    while (!result.EndOfMessage);

                    DepackDanmakuData(m_ReceiveBuffer);
                }
                catch (OperationCanceledException ex)
                {
                    Log.Warn(nameof(_innerLoop) + "_OperationCanceledException", $"_sendObject:{ex.ToString()}", ex, false);
                    continue;
                }
                catch (ObjectDisposedException ex)
                {
                    Log.Warn(nameof(_innerLoop) + "_ObjectDisposedException", $"_sendObject:{ex.ToString()}", ex, false);
                    continue;
                }
                catch (WebSocketException we)
                {
                    Log.Warn(nameof(_innerLoop) + "_WebSocketException", $"_sendObject:{we.ToString()}", we, false);
                }
                catch (Newtonsoft.Json.JsonException ex)
                {
                    Log.Warn(nameof(_innerLoop) + "_JsonException", $"_sendObject:{ex.ToString()}", ex, false);
                    continue;
                }
                catch (Exception e)
                {
                    Log.Warn(nameof(_innerLoop) + "_Exception", $"_sendObject:{e.ToString()}", e, false);
                }
            }
        }

        /// <summary>
        /// 消息拆包
        /// </summary>
        private void DepackDanmakuData(byte[] messages)
        {
            if (messages == null || messages.Length < 16)
            {
                Log.Warn(nameof(LiveChatListener) + "_" + nameof(DepackDanmakuData), $"消息长度不足16字节");
                return;
            }

            byte[] headerBuffer = new byte[16];
            Array.Copy(messages, 0, headerBuffer, 0, 16);
            DanmakuProtocol protocol = new DanmakuProtocol(headerBuffer);

            if (protocol.PacketLength < 16)
            {
                Log.Warn(nameof(LiveChatListener) + "_" + nameof(DepackDanmakuData), $"LiveChatListener初始化bodyLength出现错误长度<16");
                return;
            }

            int bodyLength = protocol.PacketLength - 16;
            if (bodyLength == 0)
            {
                Log.Warn(nameof(LiveChatListener) + "_" + nameof(DepackDanmakuData), $"LiveChatListener初始化bodyLength出现错误长度0");
                return;
            }

            byte[] buffer = new byte[bodyLength];
            Array.Copy(messages, 16, buffer, 0, bodyLength);

            switch (protocol.Version)
            {
                case 1:
                    ProcessDanmakuData(protocol.Operation, buffer, bodyLength);
                    break;
                case 2:
                    {
                        var ms = new MemoryStream(buffer, 2, bodyLength - 2);
                        var deflate = new DeflateStream(ms, CompressionMode.Decompress);

                        while (deflate.Read(headerBuffer, 0, 16) > 0)
                        {
                            protocol = new DanmakuProtocol(headerBuffer);
                            bodyLength = protocol.PacketLength - 16;

                            if (bodyLength == 0)
                            {
                                continue; // 没有内容了
                            }

                            if (buffer.Length < bodyLength) // 不够长再申请
                            {
                                buffer = new byte[bodyLength];
                            }

                            deflate.Read(buffer, 0, bodyLength);
                            ProcessDanmakuData(protocol.Operation, buffer, bodyLength);
                        }

                        ms.Dispose();
                        deflate.Dispose();
                        break;
                    }
                case 3:
                    using (var inputStream = new MemoryStream(buffer))
                    using (var outputStream = new MemoryStream())
                    using (var decompressionStream = new BrotliStream(inputStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(outputStream);
                        buffer = outputStream.ToArray();
                    }
                    ProcessDanmakuData(protocol.Operation, buffer, buffer.Length, true);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        private void ProcessDanmakuData(int opt, byte[] buffer, int length, bool IsBrotli = false)
        {
            switch (opt)
            {
                case 3: // 人气值
                    {
                        if (length == 4)
                        {
                            int popularity = buffer[3] + buffer[2] * 255 + buffer[1] * 255 * 255 + buffer[0] * 255 * 255 * 255;
                            _parse("{\"cmd\":\"LiveP\",\"LiveP\":" + popularity + ",\"roomID\":" + RoomId + "}");
                        }
                        break;
                    }
                case 5: // 弹幕消息
                    {
                        try
                        {
                            if (IsBrotli)
                            {
                                do
                                {
                                    int len = buffer[3] + (buffer[2] * 256) + (buffer[1] * 256 * 256) + (buffer[0] * 256 * 256 * 256);
                                    byte[] a = new byte[len - 16];
                                    Array.Copy(buffer, 16, a, 0, len - 16);
                                    string jsonBody = Encoding.UTF8.GetString(a, 0, len - 16);
                                    jsonBody = Regex.Unescape(jsonBody);
                                    _parse(jsonBody);
                                    byte[] b = new byte[buffer.Length - len];
                                    Array.Copy(buffer, len, b, 0, buffer.Length - len);
                                    buffer = b;
                                } while (buffer.Length > 0);
                            }
                            else
                            {
                                string jsonBody = Encoding.UTF8.GetString(buffer, 0, length);
                                jsonBody = Regex.Unescape(jsonBody);
                                _parse(jsonBody);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is Newtonsoft.Json.JsonException || ex is KeyNotFoundException)
                            {
                                // 忽略JSON解析错误
                            }
                            else
                            {
                                Log.Error(nameof(ProcessDanmakuData), "处理弹幕数据时出错", ex);
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// 解析JSON消息并触发相应事件
        /// </summary>
        private void _parse(string jsonBody)
        {
            var obj = new JsonObject();

            try
            {
                jsonBody = ReplaceString(jsonBody);

                // 处理DANMU_MSG的特殊格式
                if (jsonBody.Contains("DANMU_MSG"))
                {
                    jsonBody = jsonBody.Replace("\"extra\":\"", "\"extra\":");
                    jsonBody = jsonBody.Replace("\"{}\",", "");
                    jsonBody = jsonBody.Replace("}\",\"", "},\"");
                }

                // 过滤不需要处理的消息类型
                if (!jsonBody.Contains("DANMU_MSG") && !jsonBody.Contains("SUPER_CHAT_MESSAGE") &&
                    !jsonBody.Contains("SEND_GIFT") && !jsonBody.Contains("GUARD_BUY"))
                {
                    return;
                }

                obj = JsonNode.Parse(jsonBody).AsObject();
            }
            catch (Exception)
            {
                return;
            }

            string cmd = (string)obj["cmd"];

            switch (cmd)
            {
                // 弹幕信息
                case "DANMU_MSG":
                    MessageReceived?.Invoke(this, new DanmuMessageEventArgs(obj));
                    break;
                // SC信息
                case "SUPER_CHAT_MESSAGE":
                    MessageReceived?.Invoke(this, new SuperchatEventArg(obj));
                    break;
                // 礼物
                case "SEND_GIFT":
                    MessageReceived?.Invoke(this, new SendGiftEventArgs(obj));
                    break;
                // 舰组信息(上舰)
                case "GUARD_BUY":
                    MessageReceived?.Invoke(this, new GuardBuyEventArgs(obj));
                    break;
                // 续费舰长
                case "USER_TOAST_MSG":
                case "USER_TOAST_MSG_V2":
                    MessageReceived?.Invoke(this, new GuardRenewEventArgs(obj));
                    break;
                // 其他不需要处理的消息类型
                case "ACTIVITY_BANNER_UPDATE_V2":  // 小时榜单变动通知
                case "COMBO_SEND":                 // 礼物combo
                case "ENTRY_EFFECT":               // 进场特效
                case "NOTICE_MSG":                 // 在房间内续费了舰长
                case "WELCOME":                    // 欢迎
                case "LiveP":                      // 人气值(心跳数据)
                case "WARNING":                    // 管理员警告
                case "LIVE":                       // 开播_心跳
                case "PREPARING":                  // 下播_心跳
                case "INTERACT_WORD":              // 进场消息
                case "PANEL":                      // 小时榜信息更新
                case "ONLINE_RANK_COUNT":          // 服务等级（降级后会变化）
                case "ONLINE_RANK_V2":             // 高能榜更新
                case "ROOM_BANNER":                // 房间横幅信息
                case "ACTIVITY_RED_PACKET":        // 红包抽奖弹幕
                case "CUT_OFF":                    // 切断直播间
                    break;
                default:
                    // 未知CMD类型
                    break;
            }
        }

        /// <summary>
        /// 替换JSON字符串中的特殊字符
        /// </summary>
        private static string ReplaceString(string JsonString)
        {
            if (JsonString == null)
            {
                return JsonString;
            }

            if (JsonString.Contains("\\"))
            {
                JsonString = JsonString.Replace("\\", "\\\\");
            }

            JsonString = Regex.Replace(JsonString, @"[\n\r]", "");
            JsonString = JsonString.Trim();
            return JsonString;
        }

        /// <summary>
        /// 发送JSON对象
        /// </summary>
        private async Task _sendObject(int type, object obj)
        {
            try
            {
                if (!State)
                {
                    return;
                }

                string jsonBody = JsonSerializer.Serialize(obj, new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });

                await _sendBinary(type, Encoding.UTF8.GetBytes(jsonBody));
            }
            catch (Exception ex)
            {
                Log.Error(nameof(_sendObject), "发送JSON对象时出错", ex);
            }
        }

        /// <summary>
        /// 发送二进制数据
        /// </summary>
        private async Task _sendBinary(int type, byte[] body)
        {
            if (m_client?.State != WebSocketState.Open)
            {
                return;
            }

            byte[] head = new byte[16];
            using (MemoryStream ms = new MemoryStream(head))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.WriteBE(16 + body.Length);
                bw.WriteBE((ushort)16);
                bw.WriteBE((ushort)1);
                bw.WriteBE(type);
                bw.WriteBE(1);
            }

            byte[] tail = new byte[16 + body.Length];
            Array.Copy(head, 0, tail, 0, 16);
            Array.Copy(body, 0, tail, 16, body.Length);

            await m_client.SendAsync(new ArraySegment<byte>(tail), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        #endregion

        #region private Class
        /// <summary>
        /// 消息协议
        /// </summary>
        private class DanmakuProtocol
        {
            /// <summary>
            /// 消息总长度 (协议头 + 数据长度)
            /// </summary>
            public int PacketLength;
            /// <summary>
            /// 消息头长度 (固定为16[sizeof(DanmakuProtocol)])
            /// </summary>
            public short HeaderLength;
            /// <summary>
            /// 消息版本号
            /// </summary>
            public short Version;
            /// <summary>
            /// 消息类型
            /// </summary>
            public int Operation;
            /// <summary>
            /// 参数, 固定为1
            /// </summary>
            public int Parameter;

            /// <summary>
            /// 转为本机字节序
            /// </summary>
            public DanmakuProtocol(byte[] buff)
            {
                PacketLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buff, 0));
                HeaderLength = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(buff, 4));
                Version = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(buff, 6));
                Operation = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buff, 8));
                Parameter = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buff, 12));
            }
        }

        /// <summary>
        /// 弹幕WebSocket服务器信息
        /// </summary>
        private class DanMuWssInfo
        {
            public long code { get; set; }
            public string message { get; set; }
            public long ttl { get; set; }
            public Data data { get; set; } = new();

            public class Data
            {
                public long uid { set; get; }
                public string token { set; get; }
                public List<Host> host_list { set; get; } = new List<Host>();

                public class Host
                {
                    public string host { set; get; }
                    public int port { set; get; }
                    public int wss_port { set; get; }
                    public int ws_port { set; get; }
                }
            }
        }
        #endregion
    }
}