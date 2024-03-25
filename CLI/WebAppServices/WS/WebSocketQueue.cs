using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CLI.WebAppServices.WS
{
    internal class WebSocketQueue
    {
        private static bool _start = false;
        internal static void Start()
        {
            Task.Run(() =>
            {
                if (!_start)
                {
                    _start = true;
                    Core.LogModule.OperationQueue.AddOperationRecord += OperationQueue_AddOperationRecord;
                }
            });
        }

        private static void OperationQueue_AddOperationRecord(object? sender, EventArgs e)
        {
            Core.LogModule.OperationQueue.pack<string> pack = (Core.LogModule.OperationQueue.pack<string>)sender;
            string MessagePack = JsonSerializer.Serialize(pack);
            MessageBase.WS_Send(MessagePack);
        }
    }
}
