using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DDTV_Server
{
    public class MessageBase
    {
        public static pack<T> Success<T>(string cmd, T data,string massage="",code code=code.ok)
        {
            pack<T> pack = new pack<T>()
            {
                cmd = cmd,
                code= code,
                data = data,
                massage = massage
            };
            return pack;
        }
        public enum code
        {
            ok = 0
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
