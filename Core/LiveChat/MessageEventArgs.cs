using System;
using System.Text.Json.Nodes;

namespace Core.LiveChat
{
    public class MessageEventArgs : EventArgs
    {
        protected JsonObject m_innerObject;

        public string Command { get; set; }

        public JsonObject JsonObject { get => m_innerObject; }

        public MessageEventArgs(JsonObject obj)
        {
            if (obj != null)
            {
                Command = (string)obj["cmd"];
            }
            m_innerObject = obj;
        }
    }

    public class ExceptionEventArgs : MessageEventArgs
    {
        public Exception Exception { get; private set; }
        internal ExceptionEventArgs(Exception e) : base(null)
        {
            Exception = e;
        }
    }
}
