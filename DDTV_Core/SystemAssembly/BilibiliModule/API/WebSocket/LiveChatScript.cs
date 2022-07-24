using System;
using System.Buffers;
using System.IO;
using static System.Buffers.Binary.BinaryPrimitives;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.WebSocket
{
    internal static class Extensions
    {
        internal static void WriteBE(this BinaryWriter writer, int value)
        {
            if(BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            writer.Write(value);
        }
        internal static void WriteBE(this BinaryWriter writer, ushort value)
        {
            if(BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            writer.Write(value);
        }
        internal static unsafe void SwapBytes(byte* ptr, int length)
        {
            for (int i = 0 ; i < length / 2 ; ++i)
            {
                byte b = *(ptr + i);
                *(ptr + i) = *(ptr + length - i - 1);
                *(ptr + length - i - 1) = b;
            }
        }
    }
}
