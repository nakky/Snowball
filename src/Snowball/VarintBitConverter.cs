using System;
using System.IO;

namespace Snowball
{
    public class VarintBitConverter
    {
        public VarintBitConverter()
        {
        }

        #region Serialize
        public static void SerializeShort(short value, Stream stream, out int size)
        {
            var zigzag = EncodeZigZag(value, 16);
            GetVarintBytes((ulong)zigzag, stream, out size);
        }

        public static void SerializeUShort(ushort value, Stream stream, out int size)
        {
            GetVarintBytes((ulong)value, stream, out size);
        }

        private static long EncodeZigZag(long value, int bitLength)
        {
            return (value << 1) ^ (value >> (bitLength - 1));
        }

        public static void GetVarintBytes(ulong value, Stream stream, out int size)
        {
            size = 0;
            ulong byteVal;
            byte[] byteval = new byte[1];
            do
            {
                byteVal = value & 0x7f;
                value >>= 7;

                if (value != 0)
                {
                    byteVal |= 0x80;
                }

                byteval[0] = (byte)byteVal;
                stream.Write((byte[])byteval, 0, 1);
                size++;
            } while (value != 0);

        }
        #endregion

        #region Deserialize
        public static short ToInt16(Stream stream, out int size)
        {
            var zigzag = ToTarget(stream, 16, out size);
            return (short)DecodeZigZag(zigzag);
        }

        public static ushort ToUInt16(Stream stream, out int size)
        {
            return (ushort)ToTarget(stream, 16, out size);
        }

        private static ulong ToTarget(Stream stream, int sizeBites, out int size)
        {
            size = 0;

            int shift = 0;
            ulong result = 0;
            int pos = 0;

            byte[] byteval = new byte[1];
            ulong byteValue = 0;
            ulong tmp = 0;

            while (true)
            {
                stream.Read(byteval, 0, 1);
                pos++;
                byteValue = (ulong)byteval[0];

                tmp = byteValue & 0x7f;
                result |= tmp << shift;
                size++;

                if (shift > sizeBites)
                {
                    throw new ArgumentOutOfRangeException("stream", "too large.");
                }

                if ((byteValue & 0x80) != 0x80)
                {
                    return result;
                }

                shift += 7;
            }

            throw new ArgumentException("Cannot decode varint.", "stream");
        }

        private static long DecodeZigZag(ulong value)
        {
            if ((value & 0x1) == 0x1)
            {
                return (-1 * ((long)(value >> 1) + 1));
            }

            return (long)(value >> 1);
        }
        #endregion

    }
}
