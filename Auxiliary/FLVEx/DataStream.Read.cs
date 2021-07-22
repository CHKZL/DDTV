using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Auxiliary.FLVEx
{
    public partial class DataStream
    {
#if !DOTNET_35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void AssertRead(int length, string type)
        {
            if (stream.Read(buffer, 0, length) < length && !unsecure)
                throw new IOException("End of stream reached while reading " + type);
        }

        /// <summary>
        /// Reads unsigned byte from the underlaying stream.
        /// If the stream is ended the return value is undefined.
        /// </summary>
        /// <returns><see cref="byte"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public byte ReadByte()
        {
            AssertRead(1, "Byte");
            return buffer[0];
        }

        /// <summary>
        /// Reads signed byte from the underlaying stream.
        /// </summary>
        /// <returns><see cref="byte"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public sbyte ReadSByte()
        {
            AssertRead(1, "SByte");
            return (sbyte)buffer[0];
        }

        /// <summary>
        /// Reads two-bytes signed value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="short"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public short ReadShort()
        {
            AssertRead(2, "Short");

            return BE ? ByteUtils.ToShortBE(buffer) : ByteUtils.ToShortLE(buffer);
        }

        /// <summary>
        /// Reads four-bytes signed value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="int"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public int ReadInt()
        {
            AssertRead(4, "Int");

            return BE ? ByteUtils.ToIntBE(buffer) : ByteUtils.ToIntLE(buffer);
        }

        /// <summary>
        /// Reads three-bytes signed value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="int"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public int Read24Int()
        {
            AssertRead(3, "Int");

            return BE ? ByteUtils.To24IntBE(buffer) : ByteUtils.To24IntLE(buffer);
        }

        /// <summary>
        /// Reads eight-bytes signed value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="long"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public long ReadLong()
        {
            AssertRead(8, "Long");

            return BE ? ByteUtils.ToLongBE(buffer) : ByteUtils.ToLongLE(buffer);
        }

        /// <summary>
        /// Reads two-bytes unsigned value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="ushort"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public ushort ReadUShort()
        {
            AssertRead(2, "UShort");

            return BE ? ByteUtils.ToUShortBE(buffer) : ByteUtils.ToUShortLE(buffer);
        }

        /// <summary>
        /// Reads three-bytes unsigned value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="uint"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public uint Read24UInt()
        {
            AssertRead(3, "UInt");

            return BE ? ByteUtils.To24UIntBE(buffer) : ByteUtils.To24UIntLE(buffer);
        }

        /// <summary>
        /// Reads four-bytes unsigned value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="uint"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public uint ReadUInt()
        {
            AssertRead(4, "UInt");

            return BE ? ByteUtils.ToUIntBE(buffer) : ByteUtils.ToUIntLE(buffer);
        }

        /// <summary>
        /// Reads eight-bytes unsigned value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="ulong"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public ulong ReadULong()
        {
            AssertRead(8, "ULong");

            return BE ? ByteUtils.ToULongBE(buffer) : ByteUtils.ToULongLE(buffer);
        }

        /// <summary>
        /// Reads the DateTime value from the underlaying stream. DateTime will remain its timezone
        /// </summary>
        /// <returns><see cref="DateTime"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public DateTime ReadDateTime()
        {
            return new DateTime((ReadLong() + UNIX_OFFSET) * 10);
        }

        /// <summary>
        /// Reads the DateTime value from the underlaying stream. DateTime will be converted from UTC to local
        /// </summary>
        /// <returns><see cref="DateTime"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public DateTime ReadDateTimeUtc()
        {
            return new DateTime((ReadLong() + UNIX_OFFSET) * 10).ToLocalTime();
        }

        /// <summary>
        /// Reads UTF8 string from the underlaying stream.
        /// </summary>
        /// <returns>String read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public string ReadString()
        {
            ushort length = ReadUShort();

            if (length == 0)
                return string.Empty;

            byte[] data = new byte[length];

            if (stream.Read(data, 0, length) < length && !unsecure)
                throw new IOException("End of stream reached while reading String");
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        /// <summary>
        /// Reads ASCII string from the underlaying stream.
        /// </summary>
        /// <returns>String read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public string ReadASCII()
        {
            byte length = ReadByte();

            if (length == 0)
                return string.Empty;

            byte[] data = new byte[length];

            if (stream.Read(data, 0, length) < length && !unsecure)
                throw new IOException("End of stream reached while reading ASCII String");
#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE
            return Encoding.ASCII.GetString(data);
#else
      return ASCIIEncoding.GetString(data);
#endif
        }

        /// <summary>
        /// Transparently reads byte array from the underlaying stream. This call
        /// is equal to <code>
        /// byte[] result = new byte[length];
        /// dataStream.Stream.Read(result, 0, length);</code>
        /// </summary>
        /// <returns>Byte array read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public byte[] ReadBytes(int length)
        {
            byte[] result = new byte[length];
            if (stream.Read(result, 0, length) < length)
                throw new IOException("End of stream reached while reading byte array");
            return result;
        }

        /// <summary>
        /// Reads the boolean value from the underlaying stream.
        /// </summary>
        /// <returns><see cref="bool"/> read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        public bool ReadBool()
        {
            AssertRead(1, "Bool");
            return buffer[0] == 1;
        }

        /// <summary>
        /// Reads the float value from the underlaying stream.
        /// </summary>
        /// <returns>Float read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        /// <seealso cref="BitConverter.ToSingle"/>
        public float ReadFloat()
        {
            AssertRead(4, "Float");

            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        /// Reads the double value from the underlaying stream.
        /// </summary>
        /// <returns>Double read</returns>
        /// <exception cref="IOException">Thrown when end of stream was reached and <see cref="Unsecure"/> is false.</exception>
        /// <seealso cref="BitConverter.Int64BitsToDouble"/>
        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }
    }
}
