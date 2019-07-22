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

   
}
