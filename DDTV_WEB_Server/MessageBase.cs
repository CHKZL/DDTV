using Newtonsoft.Json;

namespace DDTV_WEB_Server
{
    public class MessageBase
    {
        public static string Success<T>(string cmd, T data,string massage="",code code=code.ok)
        {
            string MESS = "";
            if (typeof(T).Name.Equals("string")|| typeof(T).Name.Equals("String"))
            {
                MESS=data as string;
            }
            DDTV_Core.SystemAssembly.Log.Log.AddLog(nameof(MessageBase), DDTV_Core.SystemAssembly.Log.LogClass.LogType.Info_API, MESS,false,null,false);
            pack<T> pack = new pack<T>()
            {
                cmd = cmd,
                code= code,
                data = data,
                massage = massage
            };
            string B = JsonConvert.SerializeObject(pack);
            return B;
        }
        public enum code
        {
            /// <summary>
            /// 请求成功
            /// </summary>
            ok = 0,
            /// <summary>
            /// 登陆信息失效
            /// </summary>
            LoginInfoFailure=6000,
            /// <summary>
            /// 登陆验证失败
            /// </summary>
            LoginVeriicationFailed=6001,
            /// <summary>
            /// APIsig计算鉴权失败
            /// </summary>
            APIAuthenticationFailed=6002,
            /// <summary>
            /// 删除房间失败
            /// </summary>
            DelRoomFailed = 7000,
            /// <summary>
            /// 修改房间自动录制状态失败
            /// </summary>
            AutoRecRoomFailed = 7001,
            /// <summary>
            /// 修改房间弹幕录制设置失败
            /// </summary>
            DanmuRecRoomFailed = 7002,
        }
        public class pack<T>
        {
            public code code { get; set; }
            public string cmd { get; set; }
            public string massage { get; set; }
            public T data { get; set; }
        }
      
    }
}
