using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.FLVEx
{
    public partial class DataStream
    {
        /// <summary>
        /// Writes unsigned byte value to the underlaying stream
        /// </summary>
        /// <param name="value">Byte value to write</param>
        public void Write(byte value)
        {
            buffer[0] = value;
            stream.Write(buffer, 0, 1); // faster than create an array each time (as it's made in Stream's sources)
        }

        /// <summary>
        /// Writes signed byte value to the underlaying stream
        /// </summary>
        /// <param name="value">Byte value to write</param>
        public void Write(sbyte value)
        {
            buffer[0] = (byte)value;
            stream.Write(buffer, 0, 1); // faster than create an array each time (as it's made in Stream's sources)
        }

        /// <summary>
        /// Writes two-byte signed short value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="short"/> value to write</param>
        public void Write(short value)
        {
            if (BE)
                ByteUtils.FromShortBE(buffer, value);
            else
                ByteUtils.FromShortLE(buffer, value);

            stream.Write(buffer, 0, 2);
        }

        /// <summary>
        /// Writes two-byte unsigned short value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="ushort"/> value to write</param>
        public void Write(ushort value)
        {
            if (BE)
                ByteUtils.FromUShortBE(buffer, value);
            else
                ByteUtils.FromUShortLE(buffer, value);

            stream.Write(buffer, 0, 2);
        }

        /// <summary>
        /// Writes four-byte signed int value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="int"/> value to write</param>
        public void Write(int value)
        {
            if (BE)
                ByteUtils.FromIntBE(buffer, value);
            else
                ByteUtils.FromIntLE(buffer, value);

            stream.Write(buffer, 0, 4);
        }

        /// <summary>
        /// Writes three-byte signed int value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="int"/> value to write</param>
        public void Write24(int value)
        {
            if (BE)
                ByteUtils.From24IntBE(buffer, value);
            else
                ByteUtils.From24IntLE(buffer, value);

            stream.Write(buffer, 0, 3);
        }

        /// <summary>
        /// Writes three-byte unsigned int value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="uint"/> value to write</param>
        public void Write24(uint value)
        {
            if (BE)
                ByteUtils.From24UIntBE(buffer, value);
            else
                ByteUtils.From24UIntLE(buffer, value);

            stream.Write(buffer, 0, 3);
        }

        /// <summary>
        /// Writes four-byte unsigned int value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="uint"/> value to write</param>
        public void Write(uint value)
        {
            if (BE)
                ByteUtils.FromUIntBE(buffer, value);
            else
                ByteUtils.FromUIntLE(buffer, value);

            stream.Write(buffer, 0, 4);
        }

        /// <summary>
        /// Writes string value to the underlying stream.
        /// UTF-8 encoding is used.
        /// Only 65535 byte length strings are supported. The byte order of the length data depends on <see cref="BigEndian"/> flag.
        /// </summary>
        /// <param name="value">String to write. May be null</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when string's byte representation in UTF8 exceeds the length of 65535</exception>
        public void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Write((ushort)0);
                return;
            }

            byte[] encoded = Encoding.UTF8.GetBytes(value);

            if (encoded.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too long to write (only 65535 bytes supported)");

            Write((ushort)encoded.Length);
            stream.Write(encoded, 0, encoded.Length);
        }

        /// <summary>
        /// Writes string value to the underlying stream.
        /// Single-byte ASCII encoding is used.
        /// Only 255 byte length strings are supported
        /// </summary>
        /// <param name="value">String to write. May be null</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when string's length exceeds 255</exception>
        public void WriteASCII(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Write((byte)0);
                return;
            }

            if (value.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too long to write (only 255 bytes supported)");

#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE
            byte[] encoded = Encoding.ASCII.GetBytes(value);
#else
      byte[] encoded = ASCIIEncoding.GetBytes(value);
#endif

            Write((byte)encoded.Length);
            stream.Write(encoded, 0, encoded.Length);
        }

        /// <summary>
        /// Writes eight-byte signed long value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="long"/> value to write</param>
        public void Write(long value)
        {
            if (BE)
                ByteUtils.FromLongBE(buffer, value);
            else
                ByteUtils.FromLongLE(buffer, value);

            stream.Write(buffer, 0, 8);
        }

        /// <summary>
        /// Writes four-byte unsigned int value to the underlaying stream
        /// </summary>
        /// <param name="value"><see cref="ulong"/> value to write</param>
        public void Write(ulong value)
        {
            if (BE)
                ByteUtils.FromULongBE(buffer, value);
            else
                ByteUtils.FromULongLE(buffer, value);

            stream.Write(buffer, 0, 8);
        }

        /// <summary>
        /// Writes the <see cref="DateTime"/> value to the underlaying stream.
        /// Value will be written in current timezone without conversion.
        /// The format is <see cref="Int64"/> count of milliseconds elapsed from the beginning of the Unix epoch (1.01.1970).
        /// The byte order of the value depends on <see cref="BigEndian"/> flag.
        /// </summary>
        /// <param name="value"><see cref="DateTime"/> value to write</param>
        public void Write(DateTime value)
        {
            Write(value.Ticks / 10 - UNIX_OFFSET);
        }

        /// <summary>
        /// Writes the <see cref="DateTime"/> value to the underlaying stream.
        /// Value will be written in UTC timezone.
        /// The format is <see cref="Int64"/> count of milliseconds elapsed from the beginning of the Unix epoch (1.01.1970).
        /// The byte order of the value depends on <see cref="BigEndian"/> flag.
        /// </summary>
        /// <param name="value"><see cref="DateTime"/> value to write</param>
        public void WriteUtc(DateTime value)
        {
            Write(value.ToUniversalTime().Ticks / 10 - UNIX_OFFSET);
        }

        /// <summary>
        /// Writes boolean value to the underlaying stream
        /// Uses one byte with a value 1 for true and 0 for false
        /// </summary>
        /// <param name="value"><see cref="bool"/> value to write</param>
        public void Write(bool value)
        {
            buffer[0] = (byte)(value ? 1 : 0);
            stream.Write(buffer, 0, 1);
        }

        /// <summary>
        /// Transparently writes byte array to the underlaying stream.
        /// This call is equal to <code>dataStream.Stream.Write(value, 0, value.Length);</code>
        /// </summary>
        /// <param name="value">Byte array to write</param>
        public void Write(byte[] value)
        {
            stream.Write(value, 0, value.Length);
        }

        /// <summary>
        /// Writes single presicion floating point value to underlaying stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <remarks>Causes memory allocation.</remarks>
        public void Write(float value)
        {
            byte[] b = BitConverter.GetBytes(value);
            stream.Write(b, 0, b.Length);
        }

        /// <summary>
        /// Writes double precision floating point value to underlaying stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void Write(double value)
        {
            Write(BitConverter.DoubleToInt64Bits(value));
        }
    }
}
