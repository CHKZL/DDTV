using System.IO;

namespace Auxiliary.LiveChatScript
{
    internal static class Extensions
    {
        internal static void WriteBE(this BinaryWriter writer,int value)
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
            for (int i = 0; i < length / 2; ++i)
            {
                byte b = *(ptr + i);
                *(ptr + i) = *(ptr + length - i - 1);
                *(ptr + length - i - 1) = b;
            }
        }
    }
}
