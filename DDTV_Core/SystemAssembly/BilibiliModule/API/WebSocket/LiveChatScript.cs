using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.WebSocket
{
    internal static class Extensions
    {
        internal static void WriteBE(this BinaryWriter writer, int value)
        {
            unsafe { SwapBytes((byte*)&value, 4); }
            writer.Write(value);
        }
        internal static void WriteBE(this BinaryWriter writer, ushort value)
        {
            unsafe { SwapBytes((byte*)&value, 2); }
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
