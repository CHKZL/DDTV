using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.FLVEx
{
#if SHARED_PUBLIC_API
  public
#else
    internal
#endif
  static class ByteUtils
    {
        public static short ToShortBE(byte[] buffer)
        {
            return (short)(buffer[0] << 8 | buffer[1]);
        }

        public static short ToShortLE(byte[] buffer)
        {
            return (short)(buffer[0] | buffer[1] << 8);
        }

        public static int ToIntBE(byte[] buffer)
        {
            return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        }

        public static int To24IntBE(byte[] buffer)
        {
            return buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        }

        public static int To24IntLE(byte[] buffer)
        {
            return buffer[0] | buffer[1] << 8 | buffer[2] << 16;
        }

        public static int ToIntLE(byte[] buffer)
        {
            return buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
        }

        public static long ToLongBE(byte[] buffer)
        {
            return (long)buffer[0] << 56 | (long)buffer[1] << 48 | (long)buffer[2] << 40 | (long)buffer[3] << 32 | (long)buffer[4] << 24 | (long)buffer[5] << 16 | (long)buffer[6] << 8 | (long)buffer[7];
        }

        public static long ToLongLE(byte[] buffer)
        {
            return (long)buffer[0] | (long)buffer[1] << 8 | (long)buffer[2] << 16 | (long)buffer[3] << 24 | (long)buffer[4] << 32 | (long)buffer[5] << 40 | (long)buffer[6] << 48 | (long)buffer[7] << 56;
        }

        public static ushort ToUShortBE(byte[] buffer)
        {
            return (ushort)(buffer[0] << 8 | buffer[1]);
        }

        public static ushort ToUShortLE(byte[] buffer)
        {
            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        public static uint To24UIntBE(byte[] buffer)
        {
            return (uint)(buffer[0] << 16 | buffer[1] << 8 | buffer[2]);
        }

        public static uint To24UIntLE(byte[] buffer)
        {
            return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16);
        }

        public static uint ToUIntBE(byte[] buffer)
        {
            return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
        }

        public static uint ToUIntLE(byte[] buffer)
        {
            return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
        }

        public static ulong ToULongBE(byte[] buffer)
        {
            return (ulong)buffer[0] << 56 | (ulong)buffer[1] << 48 | (ulong)buffer[2] << 40 | (ulong)buffer[3] << 32 | (ulong)buffer[4] << 24 | (ulong)buffer[5] << 16 | (ulong)buffer[6] << 8 | (ulong)buffer[7];
        }

        public static ulong ToULongLE(byte[] buffer)
        {
            return (ulong)buffer[0] | (ulong)buffer[1] << 8 | (ulong)buffer[2] << 16 | (ulong)buffer[3] << 24 | (ulong)buffer[4] << 32 | (ulong)buffer[5] << 40 | (ulong)buffer[6] << 48 | (ulong)buffer[7] << 56;
        }

        public static void FromShortBE(byte[] buffer, short value)
        {
            buffer[0] = (byte)((value >> 8) & 0xFF);
            buffer[1] = (byte)((value >> 0) & 0xFF);
        }

        public static void FromShortLE(byte[] buffer, short value)
        {
            buffer[0] = (byte)((value >> 0) & 0xFF);
            buffer[1] = (byte)((value >> 8) & 0xFF);
        }

        public static void From24IntBE(byte[] buffer, int value)
        {
            buffer[0] = ((byte)((value >> 16) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 0) & 0xFF));
        }

        public static void From24IntLE(byte[] buffer, int value)
        {
            buffer[0] = ((byte)((value >> 0) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 16) & 0xFF));
        }

        public static void FromIntBE(byte[] buffer, int value)
        {
            buffer[0] = ((byte)((value >> 24) & 0xFF));
            buffer[1] = ((byte)((value >> 16) & 0xFF));
            buffer[2] = ((byte)((value >> 8) & 0xFF));
            buffer[3] = ((byte)((value >> 0) & 0xFF));
        }

        public static void FromIntLE(byte[] buffer, int value)
        {
            buffer[0] = ((byte)((value >> 0) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 16) & 0xFF));
            buffer[3] = ((byte)((value >> 24) & 0xFF));
        }

        public static void FromLongBE(byte[] buffer, long value)
        {
            buffer[0] = ((byte)((value >> 56) & 0xFF));
            buffer[1] = ((byte)((value >> 48) & 0xFF));
            buffer[2] = ((byte)((value >> 40) & 0xFF));
            buffer[3] = ((byte)((value >> 32) & 0xFF));
            buffer[4] = ((byte)((value >> 24) & 0xFF));
            buffer[5] = ((byte)((value >> 16) & 0xFF));
            buffer[6] = ((byte)((value >> 8) & 0xFF));
            buffer[7] = ((byte)((value >> 0) & 0xFF));
        }

        public static void FromLongLE(byte[] buffer, long value)
        {
            buffer[0] = ((byte)((value >> 0) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 16) & 0xFF));
            buffer[3] = ((byte)((value >> 24) & 0xFF));
            buffer[4] = ((byte)((value >> 32) & 0xFF));
            buffer[5] = ((byte)((value >> 40) & 0xFF));
            buffer[6] = ((byte)((value >> 48) & 0xFF));
            buffer[7] = ((byte)((value >> 56) & 0xFF));
        }

        public static void FromUShortBE(byte[] buffer, ushort value)
        {
            buffer[0] = (byte)((value >> 8) & 0xFF);
            buffer[1] = (byte)((value >> 0) & 0xFF);
        }

        public static void FromUShortLE(byte[] buffer, ushort value)
        {
            buffer[0] = (byte)((value >> 0) & 0xFF);
            buffer[1] = (byte)((value >> 8) & 0xFF);
        }

        public static void From24UIntBE(byte[] buffer, uint value)
        {
            buffer[0] = ((byte)((value >> 16) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 0) & 0xFF));
        }

        public static void From24UIntLE(byte[] buffer, uint value)
        {
            buffer[0] = ((byte)((value >> 0) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 16) & 0xFF));
        }

        public static void FromUIntBE(byte[] buffer, uint value)
        {
            buffer[0] = ((byte)((value >> 24) & 0xFF));
            buffer[1] = ((byte)((value >> 16) & 0xFF));
            buffer[2] = ((byte)((value >> 8) & 0xFF));
            buffer[3] = ((byte)((value >> 0) & 0xFF));
        }

        public static void FromUIntLE(byte[] buffer, uint value)
        {
            buffer[0] = ((byte)((value >> 0) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 16) & 0xFF));
            buffer[3] = ((byte)((value >> 24) & 0xFF));
        }

        public static void FromULongBE(byte[] buffer, ulong value)
        {
            buffer[0] = ((byte)((value >> 56) & 0xFF));
            buffer[1] = ((byte)((value >> 48) & 0xFF));
            buffer[2] = ((byte)((value >> 40) & 0xFF));
            buffer[3] = ((byte)((value >> 32) & 0xFF));
            buffer[4] = ((byte)((value >> 24) & 0xFF));
            buffer[5] = ((byte)((value >> 16) & 0xFF));
            buffer[6] = ((byte)((value >> 8) & 0xFF));
            buffer[7] = ((byte)((value >> 0) & 0xFF));
        }

        public static void FromULongLE(byte[] buffer, ulong value)
        {
            buffer[0] = ((byte)((value >> 0) & 0xFF));
            buffer[1] = ((byte)((value >> 8) & 0xFF));
            buffer[2] = ((byte)((value >> 16) & 0xFF));
            buffer[3] = ((byte)((value >> 24) & 0xFF));
            buffer[4] = ((byte)((value >> 32) & 0xFF));
            buffer[5] = ((byte)((value >> 40) & 0xFF));
            buffer[6] = ((byte)((value >> 48) & 0xFF));
            buffer[7] = ((byte)((value >> 56) & 0xFF));
        }
    }
}
