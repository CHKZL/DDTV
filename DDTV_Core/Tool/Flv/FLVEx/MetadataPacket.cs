using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDTV_Core.Tool.Flv.FLVEx
{
    public class MetadataPacket : RewritablePacket
    {
        private readonly Dictionary<string, object> variables;

        public Dictionary<string, object> Variables
        {
            get { return variables; }
        }

        private const string MARKER = "onMetaData";

        internal static void ReadVars(DataStream stream, ref Dictionary<string, object> vars)
        {
            stream.Stream.Seek(1, SeekOrigin.Current); // skip type mark

            if (ActionScript.ReadString(stream) != MARKER)
                throw new InvalidOperationException("Invalid metadata marker");

            // global object type
            KnownTypes type = (KnownTypes)stream.ReadByte();

            switch (type)
            {
                case KnownTypes.MixedArray:
                    vars = ActionScript.ReadMixedArray(stream);
                    break;

                case KnownTypes.Object:
                    vars = ActionScript.ReadObject(stream);
                    break;

                default:
                    throw new InvalidOperationException("Invalid or unsupported root object data type: " + type);
            }
        }

        internal static void WriteVars(DataStream stream, Dictionary<string, object> vars)
        {
            stream.Write((byte)KnownTypes.String);

            ActionScript.WriteVariable(stream, MARKER, vars);
        }

        internal MetadataPacket(long offset) : base(PacketType.AMFMetadata, TimeSpan.Zero, offset)
        {
            variables = new Dictionary<string, object>();
        }

        internal MetadataPacket(DataStream stream, uint prevPacketSize, PacketType type) : base(stream, prevPacketSize, type)
        {
            long position = stream.Position;
            ReadVars(stream, ref variables);

            if (stream.Position < position + PayloadSize)
                stream.Position = position + PayloadSize;

            if (stream.Position != position + PayloadSize)
                throw new Exception("Position mismatch detected. Metadata read wrong.");
        }

        protected override void WriteData(DataStream dest)
        {
            base.WriteData(dest);
            WriteVars(dest, Variables);
        }

        internal void PostWrite(DataStream dest)
        {
            dest.Position = Offset;
            WriteVars(dest, Variables);
        }
    }
}
