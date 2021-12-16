using Newtonsoft.Json;

namespace DDTV_WEB_API
{
    public class MessageBase
    {
        public static string Success<T>(string cmd, T data,string massage="",code code=code.ok)
        {
            pack<T> pack = new pack<T>()
            {
                cmd = cmd,
                code= code,
                data = data,
                massage = massage
            };
            return JsonConvert.SerializeObject(pack);
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
