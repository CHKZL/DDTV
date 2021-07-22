using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.FLVEx
{
    internal static class PacketFactory
    {
        public static BasePacket ReadPacket(DataStream stream)
        {
            uint prevSize = stream.ReadUInt();
            PacketType type = (PacketType)stream.ReadByte();

            switch (type)
            {
                case PacketType.VideoPayload:
                    return new VideoPacket(stream, prevSize, type);

                case PacketType.AudioPayload:
                    return new AudioPacket(stream, prevSize, type);

                case PacketType.AMFMetadata:
                    return new MetadataPacket(stream, prevSize, type);

                default:
                    return new UnparsedPacket(stream, prevSize, type);
            }
        }
    }
}
