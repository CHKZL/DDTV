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
        public static void Add(Opcode.Code code, string Message, RuntimeObject.RoomCardClass Card = null)
        {
            pack<string> pack = new pack<string>()
            {
                cmd = Enum.GetName(typeof(Opcode.Code), code),
                code=(int)code,
                data=Card==null?null:JsonConvert.SerializeObject(Card),
                message=Message
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
