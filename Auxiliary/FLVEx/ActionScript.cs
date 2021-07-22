using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Auxiliary.FLVEx
{
    internal static class ActionScript
    {
        #region Read

        public static string ReadString(DataStream stream)
        {
            ushort length = stream.ReadUShort();
            if (length == 0)
                return null;

            byte[] buffer = stream.ReadBytes(length);
            return Encoding.ASCII.GetString(buffer);
        }

        public static object ReadValue(DataStream stream)
        {
            KnownTypes type = (KnownTypes)stream.ReadByte();
            switch (type)
            {
                case KnownTypes.Double:
                    return stream.ReadDouble();

                case KnownTypes.Bool:
                    return stream.ReadBool();

                case KnownTypes.String:
                    return ReadString(stream);

                case KnownTypes.Date:
                    return stream.ReadBytes(10);

                case KnownTypes.Null:
                case KnownTypes.Undefined:
                case KnownTypes.Unsupported:
                    return type;

                case KnownTypes.Array:
                    return ReadArray(stream);

                case KnownTypes.MixedArray:
                    return ReadMixedArray(stream);

                case KnownTypes.Object:
                    return ReadObject(stream);

                case KnownTypes.ObjectEnd:
                    return null;

                default:
                    throw new InvalidOperationException("Invalid or unsupported data type: " + type);
            }
        }

        public static object ReadVariable(DataStream stream, out string name)
        {
            name = ReadString(stream);
            return ReadValue(stream);
        }

        public static Dictionary<string, object> ReadObject(DataStream stream)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            while (true)
            {
                string name;
                object value = ReadVariable(stream, out name);
                if (value == null)
                    return result;

                result.Add(name, value);
            }
        }

        public static Dictionary<string, object> ReadMixedArray(DataStream stream)
        {
            // skip max index/count
            stream.Stream.Seek(4, SeekOrigin.Current);

            Dictionary<string, object> result = new Dictionary<string, object>();

            while (true)
            {
                string name;
                object value = ReadVariable(stream, out name);
                if (value == null)
                    return result;

                result.Add(name, value);
            }
        }

        public static List<object> ReadArray(DataStream stream)
        {
            int maxArrayIndex = stream.ReadInt();

            List<object> result = new List<object>();

            for (int i = 0; i < maxArrayIndex; i++)
            {
                object value = ReadValue(stream);
                result.Add(value);
            }

            return result;
        }

        #endregion

        #region Write

        public static void WriteString(DataStream stream, string value)
        {
            byte[] data = Encoding.ASCII.GetBytes(value);
            stream.Write((ushort)data.Length);
            stream.Stream.Write(data, 0, data.Length);
        }

        public static void WriteVariable(DataStream stream, string name, object value)
        {
            WriteString(stream, name);
            WriteValue(stream, value);
        }

        public static void WriteValue(DataStream stream, object value)
        {
            switch (value)
            {
                case double _double:
                    stream.Write((byte)KnownTypes.Double);
                    stream.Write(_double);
                    break;

                case bool _bool:
                    stream.Write((byte)KnownTypes.Bool);
                    stream.Write(_bool);
                    break;

                case string _string:
                    stream.Write((byte)KnownTypes.String);
                    WriteString(stream, _string);
                    break;

                case byte[] _date:
                    stream.Write((byte)KnownTypes.Date);
                    stream.Stream.Write(_date, 0, _date.Length);
                    break;

                case KnownTypes _unknown:
                    stream.Write((byte)_unknown);
                    break;

                case Dictionary<string, object> _mixedArray:
                    stream.Write((byte)KnownTypes.MixedArray);
                    WriteMixedArray(stream, _mixedArray);
                    break;

                case List<object> _array:
                    stream.Write((byte)KnownTypes.Array);
                    WriteArray(stream, _array);
                    break;

                default:
                    throw new InvalidOperationException("Unsupported type: " + value.GetType().Name);
            }
        }

        public static void WriteArray(DataStream stream, List<object> _array)
        {
            // count/max index
            stream.Write(_array.Count);

            for (int i = 0; i < _array.Count; i++)
                WriteValue(stream, _array[i]);
        }

        public static void WriteMixedArray(DataStream stream, Dictionary<string, object> _mixedArray)
        {
            // count/max index
            stream.Write(_mixedArray.Count);

            foreach (KeyValuePair<string, object> pair in _mixedArray)
                WriteVariable(stream, pair.Key, pair.Value);

            // empty string at end
            stream.Write((ushort)0);
            // object end marker
            stream.Write((byte)KnownTypes.ObjectEnd);
        }

        #endregion
    }

    public enum KnownTypes : byte
    {
        Double = 0,
        Bool = 1,
        String = 2,
        Object = 3,
        Null = 5,
        Undefined = 6,
        Reference = 7,
        MixedArray = 8,
        ObjectEnd = 9,
        Array = 10,
        Date = 11,
        LongString = 12,
        Unsupported = 13,
    }
}
