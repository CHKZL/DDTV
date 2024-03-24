using Core.RuntimeObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            pack<string> pack = new pack<string>()
            {
                cmd = Enum.GetName(typeof(T), code),
                code =Convert.ToInt32(code),
                data = Card == null ? null : JsonConvert.SerializeObject(Card),
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
