﻿using Newtonsoft.Json;

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
            DDTV_Core.SystemAssembly.Log.Log.AddLog(nameof(MessageBase), DDTV_Core.SystemAssembly.Log.LogClass.LogType.Info_API, cmd+" "+ code, false,null,false);
            pack<T> pack = new pack<T>()
            {
                cmd = cmd,
                code= code,
                data = data,
                massage = massage
            };
            string B = JsonConvert.SerializeObject(pack);
            //string B= JsonSerializer.Serialize<pack<T>>(pack);
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
            public string massage { get; set; }
            /// <summary>
            /// 对应的接口数据
            /// </summary>
            public T data { get; set; }
        }
      
    }
}
