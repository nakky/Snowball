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
        public static void SerializeShort(short value, BytePacker packer, out int size)
        {
            var zigzag = EncodeZigZag(value, 16);
            GetVarintBytes((ulong)zigzag, packer, out size);
        }

        public static void SerializeUShort(ushort value, BytePacker packer, out int size)
        {
            GetVarintBytes(value, packer, out size);
        }

        public static void SerializeInt(int value, BytePacker packer, out int size)
        {
            var zigzag = EncodeZigZag(value, 32);
            GetVarintBytes((ulong)zigzag, packer, out size);
        }

        public static void SerializeUInt(uint value, BytePacker packer, out int size)
        {
            GetVarintBytes((ulong)value, packer, out size);
        }

        public static void SerializeLong(long value, BytePacker packer, out int size)
        {
            var zigzag = EncodeZigZag(value, 64);
            GetVarintBytes((ulong)zigzag, packer, out size);
        }

        public static void SerializeULong(ulong value, BytePacker packer, out int size)
        {
            GetVarintBytes((ulong)value, packer, out size);
        }

        public static void GetVarintBytes(ulong value, BytePacker packer, out int size)
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
                packer.Write((byte[])byteval, 0, 1);
                size++;
            } while (value != 0);

        }

        public static void SerializeShort(short value, Stream stream, out int size)
        {
            var zigzag = EncodeZigZag(value, 16);
            GetVarintBytes((ulong)zigzag, stream, out size);
        }

        public static void SerializeUShort(ushort value, Stream stream, out int size)
        {
            GetVarintBytes((ulong)value, stream, out size);
        }

        public static void SerializeInt(int value, Stream stream, out int size)
        {
            var zigzag = EncodeZigZag(value, 32);
            GetVarintBytes((ulong)zigzag, stream, out size);
        }

        public static void SerializeUInt(uint value, Stream stream, out int size)
        {
            GetVarintBytes((ulong)value, stream, out size);
        }

        public static void SerializeLong(long value, Stream stream, out int size)
        {
            var zigzag = EncodeZigZag(value, 64);
            GetVarintBytes((ulong)zigzag, stream, out size);
        }

        public static void SerializeULong(ulong value, Stream stream, out int size)
        {
            GetVarintBytes((ulong)value, stream, out size);
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

        private static long EncodeZigZag(long value, int bitLength)
        {
            return (value << 1) ^ (value >> (bitLength - 1));
        }

        #endregion

        #region Deserialize
        public static short ToShort(BytePacker packer, out int size)
        {
            var zigzag = ToTarget(packer, 16, out size);
            return (short)DecodeZigZag(zigzag);
        }

        public static ushort ToUShort(BytePacker packer, out int size)
        {
            return (ushort)ToTarget(packer, 16, out size);
        }

        public static int ToInt(BytePacker packer, out int size)
        {
            var zigzag = ToTarget(packer, 32, out size);
            return (int)DecodeZigZag(zigzag);
        }

        public static uint ToUInt(BytePacker packer, out int size)
        {
            return (uint)ToTarget(packer, 32, out size);
        }

        public static long ToLong(BytePacker packer, out int size)
        {
            var zigzag = ToTarget(packer, 64, out size);
            return DecodeZigZag(zigzag);
        }

        public static ulong ToULong(BytePacker packer, out int size)
        {
            return (ulong)ToTarget(packer, 64, out size);
        }

        private static ulong ToTarget(BytePacker packer, int sizeBites, out int size)
        {
            size = 0;

            int shift = 0;
            ulong result = 0;
            int pos = 0;

            byte[] byteval;
            ulong byteValue = 0;
            ulong tmp = 0;

            while (true)
            {
                packer.ReadByteArray(out byteval, 0, 1);
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

        public static short ToShort(Stream stream, out int size)
        {
            var zigzag = ToTarget(stream, 16, out size);
            return (short)DecodeZigZag(zigzag);
        }

        public static ushort ToUShort(Stream stream, out int size)
        {
            return (ushort)ToTarget(stream, 16, out size);
        }

        public static int ToInt(Stream stream, out int size)
        {
            var zigzag = ToTarget(stream, 32, out size);
            return (int)DecodeZigZag(zigzag);
        }

        public static uint ToUInt(Stream stream, out int size)
        {
            return (uint)ToTarget(stream, 32, out size);
        }

        public static long ToLong(Stream stream, out int size)
        {
            var zigzag = ToTarget(stream, 64, out size);
            return DecodeZigZag(zigzag);
        }

        public static ulong ToULong(Stream stream, out int size)
        {
            return (ulong)ToTarget(stream, 64, out size);
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
