using Core.LogModule;
using Newtonsoft.Json;

namespace CLI
{
    public class MessageBase
    {
        public static string Success<T>(string cmd, T data,string massage="",MessageCode code=MessageCode.ok)
        {
            string? MESS = "";
            if (typeof(T).Name.Equals("string")|| typeof(T).Name.Equals("String"))
            {
                MESS = data as string;
            }
            Log.Info(nameof(MessageBase),cmd+" "+ code);
            Pack<T> pack = new Pack<T>()
            {
                cmd = cmd,
                code= code,
                Data = data,
                massage = massage
            };
            string B = JsonConvert.SerializeObject(pack);
            //string B= JsonSerializer.Serialize<pack<T>>(pack);
            return B;
        }
        /// <summary>
        /// 返回的状态码
        /// </summary>
        public enum MessageCode
        {
            /// <summary>
            /// 请求成功
            /// </summary>
            ok = 0,
            /// <summary>
            /// UID不存在
            /// </summary>
            UIDFailed=-2,
            /// <summary>
            /// 登陆信息失效
            /// </summary>
            LoginInfoFailure =6000,
            /// <summary>
            /// 登陆验证失败
            /// </summary>
            LoginVeriicationFailed=6001,
            /// <summary>
            /// APIsig计算鉴权失败
            /// </summary>
            APIAuthenticationFailed=6002,
            /// <summary>
            /// 操作失败
            /// </summary>
            OperationFailed = 7000,
          
        }
        public class Pack<T>
        {
            /// <summary>
            /// 状态码
            /// </summary>
            public MessageCode code { get; set; }
            /// <summary>
            /// 接口名称
            /// </summary>
            public string? cmd { get; set; }
            /// <summary>
            /// 信息
            /// </summary>
            public string? massage { get; set; }
            /// <summary>
            /// 对应的接口数据
            /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public T Data { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
      
    }
}
