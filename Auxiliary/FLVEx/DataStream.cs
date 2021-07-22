using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Auxiliary.FLVEx
{
#if SHARED_PUBLIC_API
  public partial
#else
    public partial
#endif
  class DataStream : IDisposable
    {
        private Stream stream;
        private bool BE;
        private bool unsecure;
        private readonly byte[] buffer = new byte[64];
        private static readonly long UNIX_OFFSET = new DateTime(1970, 1, 1).Ticks / 10; // ms

        /// <summary>
        /// Creates the instance of the DataStream around given stream.
        /// Read/write capabilities depends on underlaying stream
        /// </summary>
        /// <param name="stream">Stream to operate</param>
        public DataStream(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Creates the instance of the DataStream around given stream.
        /// Read/write capabilities depends on underlaying stream
        /// </summary>
        /// <param name="stream">Stream to operate</param>
        /// <param name="bigEndian">True to use BigEndian byte order and false to use LittleEndian (default for x86)</param>
        public DataStream(Stream stream, bool bigEndian)
        {
            this.stream = stream;
            BE = bigEndian;
        }

        /// <summary>
        /// Creates the instance of the DataStream to read from a byte array.
        /// This call is equal to <code>new DataStream(new MemoryStream(data))</code>
        /// </summary>
        /// <param name="data">Byte array to read from</param>
        public DataStream(byte[] data)
        {
            stream = new MemoryStream(data);
        }

        /// <summary>
        /// Creates the instance of the DataStream to read from a byte array.
        /// This call is equal to <code>new DataStream(new MemoryStream(data, index, data.Length - index))</code>
        /// </summary>
        /// <param name="data">Byte array to read from</param>
        /// <param name="index">Index to start reading from the array</param>
        public DataStream(byte[] data, int index)
        {
            stream = new MemoryStream(data, index, data.Length - index);
        }

        /// <summary>
        /// Creates the instance of the DataStream to read from a byte array.
        /// This call is equal to <code>new DataStream(new MemoryStream(data), bigEndian)</code>
        /// </summary>
        /// <param name="data">Byte array to read from</param>
        /// <param name="bigEndian">True to use BigEndian byte order and false to use LittleEndian (default for x86)</param>
        public DataStream(byte[] data, bool bigEndian)
        {
            stream = new MemoryStream(data);
            BE = bigEndian;
        }

        /// <summary>
        /// Creates the instance of the DataStream to write. You can get results by using <see cref="Stream"/> property.
        /// This call is equal to <code>new DataStream(new MemoryStream())</code>
        /// </summary>
        public DataStream()
        {
            stream = new MemoryStream();
        }

        /// <summary>
        /// Creates the instance of the DataStream to write. You can get results by using <see cref="Stream"/> property.
        /// This call is equal to <code>new DataStream(new MemoryStream(), bigEndian)</code>
        /// </summary>
        /// <param name="bigEndian">True to use BigEndian byte order and false to use LittleEndian (default for x86)</param>
        public DataStream(bool bigEndian)
        {
            stream = new MemoryStream();
            BE = bigEndian;
        }

        /// <summary>
        /// Used to get or set the BigEndian flag. This switches the DataStream between different byte orders.
        /// </summary>
        /// <remarks>
        /// <para>BigEndian is mainly used for Java and TCP sockets.</para>
        /// <para>LittleEndian (when this property is False) is used by default in Intel x86 and AMD64 CPU architectures.</para></remarks>
        public bool BigEndian
        {
            get { return BE; }
            set { BE = value; }
        }

        /// <summary>
        /// Used to get or set the value indicating whether to throw exceptions when End-Of-Stream found during read operation.
        /// </summary>
        /// <remarks>If the flag is set to false, read operations may return corrupt data instead of throwning an exception.</remarks>
        public bool Unsecure
        {
            get { return unsecure; }
            set { unsecure = value; }
        }

        /// <summary>
        /// Used to get or set the marker position in the underlying stream. Setting this property may result nothing
        /// if seeking is not supported by the underlying stream.
        /// </summary>
        public long Position
        {
            get { return stream.Position; }
            set { stream.Position = value; }
        }

        /// <summary>
        /// Disposes the DataStream and the underlaying stream.
        /// </summary>
        /// <remarks>If the underlying stream will be needed in future, to not call the dispose.</remarks>
        public void Dispose()
        {
            if (stream != null)
                stream.Dispose();
            stream = null;
        }

        /// <summary>
        /// Used to access the underlying stream
        /// </summary>
        public Stream Stream
        {
            get { return stream; }
        }

        /// <summary>
        /// Returns the value that indicates if the underlaying stream (and a DataReader) allows reading operations
        /// </summary>
        public bool CanRead
        {
            get { return stream.CanRead; }
        }

        /// <summary>
        /// Returns the value that indicates if the underlaying stream (and a DataReader) allows writing operations
        /// </summary>
        public bool CanWrite
        {
            get { return stream.CanWrite; }
        }
    }
}
