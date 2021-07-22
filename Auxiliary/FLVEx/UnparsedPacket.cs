using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.FLVEx
{
    public class UnparsedPacket : StreamCopyPacket
    {
        internal UnparsedPacket(DataStream stream, uint prevPacketSize, PacketType type) : base(stream, prevPacketSize, type)
        {
            SkipPayload(stream);
        }
    }
}
