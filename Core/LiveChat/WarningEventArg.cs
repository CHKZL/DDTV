using System.Text.Json.Nodes;

namespace Core.LiveChat
{
    public class WarningEventArg : MessageEventArgs
    {
        public string msg { get; set; }
        public int roomID { set; get; }

        internal WarningEventArg(JsonObject obj) : base(obj)
        {
            msg = (string)obj["msg"];
            roomID = (int)obj["roomID"];
        }
    }
}
