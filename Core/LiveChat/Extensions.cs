using System.IO;

namespace Core.LiveChat
{
    internal static class Extensions
    {
        internal static void WriteBE(this BinaryWriter writer, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            writer.Write(bytes);
        }

        internal static void WriteBE(this BinaryWriter writer, ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            writer.Write(bytes);
        }

        internal static void SwapBytes(byte[] arr)
        {
            int length = arr.Length;
            for (int i = 0; i < length / 2; ++i)
            {
                byte b = arr[i];
                arr[i] = arr[length - i - 1];
                arr[length - i - 1] = b;
            }
        }

    }
}
