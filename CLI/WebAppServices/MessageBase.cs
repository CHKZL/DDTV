using Core.LogModule;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using static CLI.WebAppServices.MessageCode;

namespace CLI.WebAppServices
{
    /// <summary>
    /// 整个API核心的核心返参打包类
    /// </summary>
    public class MessageBase
    {
        /// <summary>
        /// 打包返回数据
        /// </summary>
        /// <typeparam name="T">data主体的，泛型入参</typeparam>
        /// <param name="cmd">命令名称，默认应该操作类的nameof</param>
        /// <param name="data">返回的数据主体内容</param>
        /// <param name="message">提示文本消息</param>
        /// <param name="code">状态码</param>
        /// <returns></returns>
        public static string MssagePack<T>(string cmd, T data, string message = "", code code = code.ok)
        {
            if (!Core.RuntimeObject.Account.AccountInformation.State)
            {
                code = code.LoginInfoFailure;
            }
            string MESS = "";
            if (typeof(T).Name.ToLower().Equals("string"))
            {
                MESS = data as string;
            }
            Log.Debug(nameof(MessageBase), cmd + " " + code, false);
            pack<T> pack = new pack<T>()
            {
                cmd = cmd,
                code = code,
                data = data,
                message = message
            };

            string MessagePack = JsonConvert.SerializeObject(pack);
            WS_Send(MessagePack);
            return MessagePack;
        }

        /// <summary>
        /// WebSocket数据发送
        /// </summary>
        /// <param name="MessagePack">打包好的数据对象，请使用MssagePack类打包</param>
        public static void WS_Send(string MessagePack)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(MessagePack));
            foreach (WebSocket item in Middleware.WebSocketControl.webSockets)
            {
                item.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

     
        public class pack<T>
        {
            /// <summary>
            /// 状态码
            /// </summary>
            public code code { get; set; }
            /// <summary>
            /// 接口名称
            /// </summary>
            public string cmd { get; set; }
            /// <summary>
            /// 信息
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 对应的接口数据
            /// </summary>
            public T data { get; set; }
        }

    }
}
