using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDTV_Core.Tool.Flv.FLVEx
{
    public class BasePacket
    {
        public const int PACKET_HEADER_SIZE = 15;
        public const int PREV_PACKET_SIZE_OFFSET = 11;

        public readonly long Offset;

        public uint PrevPacketSize;

        public readonly PacketType PacketType;

        public uint PayloadSize { get; protected set; }

        public TimeSpan TimeStamp;

        protected BasePacket(PacketType packetType, TimeSpan timeStamp, long offset)
        {
            PacketType = packetType;
            TimeStamp = timeStamp;
            Offset = offset + 15;
        }

        internal BasePacket(DataStream stream, uint prevPacketSize, PacketType type)
        {
            if (prevPacketSize > 0)
                PrevPacketSize = prevPacketSize - PREV_PACKET_SIZE_OFFSET;

            PacketType = type;

            PayloadSize = stream.Read24UInt();
            uint timeStamp = stream.Read24UInt();
            timeStamp = timeStamp | (uint)(stream.ReadByte() << 24);

            TimeStamp = TimeSpan.FromMilliseconds(timeStamp);

            // skip Stream position as it is always 0
            stream.Stream.Seek(3, SeekOrigin.Current);

            Offset = stream.Position;
        }

        internal virtual void Write(Stream src, DataStream dest)
        {
            if (PrevPacketSize == 0)
                dest.Write(0);
            else
                dest.Write(PrevPacketSize + PREV_PACKET_SIZE_OFFSET);

            // type
            dest.Write((byte)PacketType);
            // size
            dest.Write24(PayloadSize);
            // timestamp
            uint timeStamp = (uint)TimeStamp.TotalMilliseconds;
            dest.Write24(timeStamp);
            dest.Write((byte)((timeStamp >> 24) & 0xFF));
            // stream id, always 0
            dest.Write24(0);
        }

        public override string ToString()
        {
            return $"{PacketType}: {PayloadSize} bytes (time: {TimeStamp})";
        }
    }

    public enum PacketType : byte
    {
        RTMPReadReport = 4,
        RTMPPing,
        RTMPServerBandwidth,
        RTMPClientBandwidth,
        AudioPayload = 8,
        VideoPayload = 9,
        RTMPFlexStreamSend = 16,
        RTMPFlexMessage,
        AMFMetadata, // 18, 0x12
        SharedObject,
        RTMPInvoke,
        EncapsulatedFlashVideo = 24
    }
}
