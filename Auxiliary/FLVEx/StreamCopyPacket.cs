using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Auxiliary.FLVEx
{
    public class StreamCopyPacket : BasePacket
    {
        private const int BUFFER_SIZE = 262144;

        internal StreamCopyPacket(DataStream stream, uint prevPacketSize, PacketType type) : base(stream, prevPacketSize, type) { }

        protected void SkipPayload(DataStream stream)
        {
            stream.Stream.Position = Offset + PayloadSize;
        }

        internal override void Write(Stream src, DataStream dest)
        {
            base.Write(src, dest);

            if (src.Position != Offset)
                src.Position = Offset;

            byte[] buffer = new byte[BUFFER_SIZE];
            uint bytesRemains = PayloadSize;

            while (bytesRemains > 0)
            {
                uint bytes = bytesRemains;
                if (bytes > buffer.Length)
                    bytes = (uint)buffer.Length;

                src.Read(buffer, 0, (int)bytes);
                dest.Stream.Write(buffer, 0, (int)bytes);
                if (bytesRemains < bytes)
                    throw new InvalidOperationException("Bytes mismatch. Something is really wrong.");
                bytesRemains -= bytes;
            }
        }
    }
}
