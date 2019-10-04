using System;
using System.Runtime.InteropServices;

namespace Snowball
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct BitShort
    {
        [FieldOffset(0)]
        public short Value;

        [FieldOffset(0)]
        public byte ByteOffset0;

        [FieldOffset(1)]
        public byte ByteOffset1;

        public BitShort(short value)
        {
            this = default(BitShort);
            this.Value = value;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BitInt
    {
        [FieldOffset(0)]
        public int Value;

        [FieldOffset(0)]
        public byte ByteOffset0;

        [FieldOffset(1)]
        public byte ByteOffset1;

        [FieldOffset(2)]
        public byte ByteOffset2;

        [FieldOffset(3)]
        public byte ByteOffset3;

        public BitInt(int value)
        {
            this = default(BitInt);
            this.Value = value;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BitLong
    {
        [FieldOffset(0)]
        public long Value;

        [FieldOffset(0)]
        public byte ByteOffset0;

        [FieldOffset(1)]
        public byte ByteOffset1;

        [FieldOffset(2)]
        public byte ByteOffset2;

        [FieldOffset(3)]
        public byte ByteOffset3;

        [FieldOffset(4)]
        public byte ByteOffset4;

        [FieldOffset(5)]
        public byte ByteOffset5;

        [FieldOffset(6)]
        public byte ByteOffset6;

        [FieldOffset(7)]
        public byte ByteOffset7;

        public BitLong(long value)
        {
            this = default(BitLong);
            this.Value = value;
        }

    }
}
