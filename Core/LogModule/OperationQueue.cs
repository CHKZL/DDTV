using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Core.LogModule
{
    public class OperationQueue
    {
        public static event EventHandler<EventArgs> AddOperationRecord;
        public static void Add<T>(T code, string Message, long uid = 0)
        {
            RoomCardClass Card = new();
            _Room.GetCardForUID(uid, ref Card);
            pack<RoomCardClass> pack = new pack<RoomCardClass>()
            {
                cmd = Enum.GetName(typeof(T), code),
                code =Convert.ToInt32(code),
                data = Card == null ? null : Card,
                message = Message
            };
            AddOperationRecord?.Invoke(pack, new EventArgs());
        }
        public class pack<T>
        {
            public string cmd { get; set; }
            public int code {  get; set; }
            public T? data {  get; set; }
            public string message {  get; set; }
        }
    }
}
