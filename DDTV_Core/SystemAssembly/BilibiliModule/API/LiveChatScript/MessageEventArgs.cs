using System;
//using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    public class MessageEventArgs : EventArgs
    {
        protected JObject m_innerObject;

        public string Command { get; set; }

        public JObject JsonObject { get => m_innerObject; }

        public  MessageEventArgs(JObject obj)
        {
            //if (obj!=null&&obj.TryGetValue("cmd",out JToken t))
            if(obj!=null)
            {
                //Command = t.Value<string>();
                Command = (string)obj["cmd"];
            }
            m_innerObject = obj;
        }
    }

    public class ExceptionEventArgs: MessageEventArgs
    {
        public Exception Exception { get; private set; }
        internal ExceptionEventArgs(Exception e):base(null)
        {
            Exception = e;
        }
    }
}
