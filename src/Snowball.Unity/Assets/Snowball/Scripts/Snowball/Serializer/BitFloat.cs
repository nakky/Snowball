using System;
using System.Runtime.InteropServices;

namespace Snowball
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct BitFloat
    {
        [FieldOffset(0)]
        public float Value;

        [FieldOffset(0)]
        public byte ByteOffset0;

        [FieldOffset(1)]
        public byte ByteOffset1;

        [FieldOffset(2)]
        public byte ByteOffset2;

        [FieldOffset(3)]
        public byte ByteOffset3;

        public BitFloat(float value)
        {
            this = default(BitFloat);
            this.Value = value;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BitDouble
    {
        [FieldOffset(0)]
        public double Value;

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

        public BitDouble(double value)
        {
            this = default(BitDouble);
            this.Value = value;
        }

    }

}
