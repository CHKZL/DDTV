using System;
using System.Collections.Generic;
using System.Text;

namespace DDTV_Core.Tool.Flv.FLVEx
{
    public class FLVHeader
    {
        public const int HEADER_SIZE = 9;

        public byte Version { get; }

        public FLVFlags Flags { get; set; }

        public FLVHeader(DataStream stream)
        {
            // check signature. "FLV" 
            if (stream.ReadByte() != 0x46 ||
                stream.ReadByte() != 0x4C ||
                stream.ReadByte() != 0x56)
                throw new InvalidOperationException("Invalid FLV Header");

            Version = (byte)stream.ReadByte();
            Flags = (FLVFlags)stream.ReadByte();

            int headerSize = stream.ReadInt();
            stream.Position = headerSize;
        }

        public void Write(DataStream stream)
        {
            // signature
            stream.Write((byte)0x46);
            stream.Write((byte)0x4C);
            stream.Write((byte)0x56);

            // different stuff
            stream.Write(Version);
            stream.Write((byte)Flags);

            // header or something
            stream.Write(HEADER_SIZE);
            stream.Position = HEADER_SIZE;
        }
    }

    [Flags]
    public enum FLVFlags : byte
    {
        Video = 0x1,
        Audio = 0x4
    }
}
