using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu
{
    internal class DepackDanmaData
    {
        internal static void DepackDanmakuData(RoomInfoClass.RoomInfo roomInfo)
        {

            byte[] headerBuffer = new byte[16];
            //for (int i = 0; i < 16; i++)
            //{
            //    headerBuffer[i] = messages[i];
            //}
            Array.Copy(roomInfo.roomWebSocket.LiveChatListener.m_ReceiveBuffer, 0, headerBuffer, 0, 16);
            DanmakuProtocol protocol = new DanmakuProtocol(headerBuffer);

            //Debug.LogError(protocol.Version + "\\" + protocol.Operation);
            //
            if (protocol.PacketLength < 16)
            {
                Log.Log.AddLog(nameof(DepackDanmaData),Log.LogClass.LogType.Debug, $@"协议失败: (L:{protocol.PacketLength})");
                Log.Log.AddLog(nameof(DepackDanmaData), Log.LogClass.LogType.Debug, $@"收到协议PacketLength长度小于16，作为观测包更新心跳时间处理");
                return;
            }
            int bodyLength = protocol.PacketLength - 16;
            if (bodyLength == 0)
            {
                //continue;
                return;
            }
            byte[] buffer = new byte[bodyLength];
            //for (int i = 0; i < bodyLength; i++)
            //{
            //    buffer[i] = messages[i + 16];
            //}
            Array.Copy(roomInfo.roomWebSocket.LiveChatListener.m_ReceiveBuffer, 16, buffer, 0, bodyLength);

            switch (protocol.Version)
            {
                case 1:
                    ProcessDanmakuData(protocol.Operation, buffer, bodyLength, roomInfo);
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
                            ProcessDanmakuData(protocol.Operation, buffer, bodyLength, roomInfo);
                        }
                        ms.Dispose();
                        deflate.Dispose();
                        break;
                    }
                case 3:
                    ;
                    break;
                case 5:
                    ;
                    break;
                case 7:
                    ;
                    break;
                case 8:
                    ;
                    break;
                default:
                    ;
                    break;
            }
        }
        /// <summary>
        /// 消息处理
        /// </summary>
        internal static void ProcessDanmakuData(int opt, byte[] buffer, int length, RoomInfoClass.RoomInfo roomInfo)
        {
            switch (opt)
            {
                case 99:
                    {
                        _parse("{\"cmd\":\"DDTV_T1\",\"T1\":1,\"roomID\":" + roomInfo.room_id + "}");
                        break;
                    }
                case 3:
                    {
                        if (length == 4)
                        {
                            int 人气值 = buffer[3] + buffer[2] * 255 + buffer[1] * 255 * 255 + buffer[0] * 255 * 255 * 255;
                            _parse("{\"cmd\":\"LiveP\",\"LiveP\":" + 人气值 + ",\"roomID\":" + roomInfo.room_id + "}");
                        }
                        break;
                    }
                case 5:
                    {
                        try
                        {
                            string jsonBody = Encoding.UTF8.GetString(buffer, 0, length);
                            jsonBody = Regex.Unescape(jsonBody);
                            _parse(jsonBody);
                            _parse("{\"cmd\":\"DDTV_T1\",\"T1\":1,\"roomID\":" + roomInfo.room_id + "}");
                            //Debug.Log(jsonBody);
                            //ReceivedDanmaku?.Invoke(this, new ReceivedDanmakuArgs { Danmaku = new Danmaku(json) });
                        }
                        catch (Exception ex)
                        {
                            if (ex is JsonException || ex is KeyNotFoundException)
                            {
                                //LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 弹幕识别错误 {json}" });
                            }
                            else
                            {
                                //LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] {ex}" });
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
        }
        private static void _parse(string jsonBody)
        {

            var obj = new JObject();
            try
            {
                jsonBody = ReplaceString(jsonBody);
                obj = JObject.Parse(jsonBody); ///JsonMapper.ToObject(jsonBody);
            }
            catch (Exception) { return; }

            //Console.WriteLine(jsonBody);
            /*StreamWriter streamWriter = new StreamWriter(@"E:/data.json", true);
            streamWriter.WriteLine(jsonBody);
            streamWriter.Flush();
            streamWriter.Close();*/

            string cmd = (string)obj["cmd"];
            //Console.WriteLine(cmd);
            switch (cmd)
            {
                //弹幕信息
                case "DANMU_MSG":
                  
                    break;
                //SC信息
                case "SUPER_CHAT_MESSAGE":
                   
                    break;
                case "SEND_GIFT":
                    
                    break;
                //舰组信息
                case "GUARD_BUY":
                    //Debug.Log("guraddd\n"+obj);
                    
                    break;
                //欢迎
                case "WELCOME":
                   
                    break;
                case "ACTIVITY_BANNER_UPDATE_V2":
                    
                    break;
                //从来不准的心跳数据
                case "LiveP":
                   
                    break;
                //管理员警告
                case "WARNING":
                    
                    break;
                //开播心跳
                case "LIVE":
                    
                    break;
                //下播心跳
                case "PREPARING":
                    
                    break;
                case "INTERACT_WORD":
                    //互动词，暂时不知道作用
                    break;
                case "PANEL":
                    //小时榜信息更新
                    break;
                case "ONLINE_RANK_COUNT":
                    //不知道是什么“在线等级计数”
                    break;
                case "ONLINE_RANK_V2":
                    //不知道是啥
                    break;
                case "ROOM_BANNER":
                    //房间横幅信息，应该就是置顶的那个跳转广告
                    break;
                case "COMBO_SEND":
                    //礼物combo
                    break;
                case "DDTV_T1":
                    
                    break;
                default:
                    
                    break;
            }
            return;
        }
        /// <summary>
        ///   替换部分字符串
        /// </summary>
        /// <param name="sPassed">需要替换的字符串</param>
        /// <returns></returns>
        public static string ReplaceString(string JsonString)
        {
            if (JsonString == null) { return JsonString; }
            if (JsonString.Contains("\\"))
            {
                JsonString = JsonString.Replace("\\", "\\\\");
            }
            //if (JsonString.Contains("\'"))
            //{
            //    JsonString = JsonString.Replace("\'", "\\\'");
            //}
            //if (JsonString.Contains("\""))
            //{
            //    JsonString = JsonString.Replace("\"", "\\\"");
            //}
            //去掉字符串的回车换行符
            JsonString = Regex.Replace(JsonString, @"[\n\r]", "");
            JsonString = JsonString.Trim();
            return JsonString;
        }
        /// <summary>
        /// 消息协议
        /// </summary>
        public class DanmakuProtocol
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
    }
}
