using Core.LogModule;
using Newtonsoft.Json;

namespace CLI
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
        public static string Success<T>(string cmd, T data, string message = "", code code = code.ok)
        {
            //if (!Core.Config.Core._LoginStatus)
            if(!Core.RuntimeObject.Account.AccountInformation.State)
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
                code= code,
                data = data,
                message = message
            };
            
            string B = JsonConvert.SerializeObject(pack);
            return B;
        }
        /// <summary>
        /// 返回的状态码
        /// </summary>
        public enum code
        {
            /// <summary>
            /// 请求成功
            /// </summary>
            ok = 0,
            /// <summary>
            /// 参数有误
            /// </summary>
            ParameterError= 5000,
            /// <summary>
            /// 登陆信息失效
            /// </summary>
            LoginInfoFailure =6000,
            /// <summary>
            /// 操作失败
            /// </summary>
            OperationFailed = 7000,
          
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
