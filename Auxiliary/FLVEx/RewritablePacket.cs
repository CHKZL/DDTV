using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Auxiliary.FLVEx
{
    public class RewritablePacket : BasePacket
    {
        internal RewritablePacket(DataStream stream, uint prevPacketSize, PacketType type) : base(stream, prevPacketSize, type) { }

        protected RewritablePacket(PacketType packetType, TimeSpan timeStamp, long offset) : base(packetType, timeStamp, offset) { }

        internal sealed override void Write(Stream src, DataStream dest)
        {
            using (DataStream dataCacheStream = new DataStream())
            {
                dataCacheStream.BigEndian = true;
                WriteData(dataCacheStream);
                PayloadSize = (uint)dataCacheStream.Stream.Length;

                base.Write(src, dest);

                dataCacheStream.Stream.Position = 0;
                dataCacheStream.Stream.CopyTo(dest.Stream);
            }

        }

        protected virtual void WriteData(DataStream dest)
        {

        }
    }
}
