using System;
using System.Collections.Generic;
using System.Text;

namespace DDTV_Core.Tool.Flv.FLVEx
{
    public class UnparsedPacket : StreamCopyPacket
    {
        internal UnparsedPacket(DataStream stream, uint prevPacketSize, PacketType type) : base(stream, prevPacketSize, type)
        {
            SkipPayload(stream);
        }
    }
}
