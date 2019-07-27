using System;
using System.Runtime.CompilerServices;

namespace Snowball
{
    public class BytePacker
    {
        public byte[] Buffer { get; private set; }
        public int Position { get; set; }

        public BytePacker(byte[] buffer, int offset = 0)
        {
            this.Buffer = buffer;
            this.Position = offset;
        }

        #region Write

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            Buffer[Position] = (value ? (byte)1 : (byte)0);
            Position++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
        {
            Buffer[Position] = unchecked((byte)value);
            Position += sizeof(sbyte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            Buffer[Position] = value;
            Position += sizeof(byte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            Write((short)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            BitShort bits = new BitShort(value);
            if (BitConverter.IsLittleEndian)
            {
                Buffer[Position] = bits.ByteOffset1;
                Buffer[Position + 1] = bits.ByteOffset0;
            }
            else
            {
                Buffer[Position] = bits.ByteOffset0;
                Buffer[Position + 1] = bits.ByteOffset1;
            }
            Position += sizeof(short);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
        {
            Write((short)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            BitInt bits = new BitInt(value);
            if (BitConverter.IsLittleEndian)
            {
                Buffer[Position] = bits.ByteOffset3;
                Buffer[Position + 1] = bits.ByteOffset2;
                Buffer[Position + 2] = bits.ByteOffset1;
                Buffer[Position + 3] = bits.ByteOffset0;
            }
            else
            {
                Buffer[Position] = bits.ByteOffset0;
                Buffer[Position + 1] = bits.ByteOffset1;
                Buffer[Position + 2] = bits.ByteOffset2;
                Buffer[Position + 3] = bits.ByteOffset3;
            }

            Position += sizeof(int);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
        {
            Write((int)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            BitLong bits = new BitLong(value);
            if (BitConverter.IsLittleEndian)
            {
                Buffer[Position] = bits.ByteOffset7;
                Buffer[Position + 1] = bits.ByteOffset6;
                Buffer[Position + 2] = bits.ByteOffset5;
                Buffer[Position + 3] = bits.ByteOffset4;
                Buffer[Position + 4] = bits.ByteOffset3;
                Buffer[Position + 5] = bits.ByteOffset2;
                Buffer[Position + 6] = bits.ByteOffset1;
                Buffer[Position + 7] = bits.ByteOffset0;
            }
            else
            {
                Buffer[Position] = bits.ByteOffset0;
                Buffer[Position + 1] = bits.ByteOffset1;
                Buffer[Position + 2] = bits.ByteOffset2;
                Buffer[Position + 3] = bits.ByteOffset3;
                Buffer[Position + 4] = bits.ByteOffset4;
                Buffer[Position + 5] = bits.ByteOffset5;
                Buffer[Position + 6] = bits.ByteOffset6;
                Buffer[Position + 7] = bits.ByteOffset7;
            }

            Position += sizeof(long);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value)
        {
            Write((long)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            BitFloat bits = new BitFloat(value);
            if (BitConverter.IsLittleEndian)
            {
                Buffer[Position] = bits.ByteOffset3;
                Buffer[Position + 1] = bits.ByteOffset2;
                Buffer[Position + 2] = bits.ByteOffset1;
                Buffer[Position + 3] = bits.ByteOffset0;
            }
            else
            {
                Buffer[Position] = bits.ByteOffset0;
                Buffer[Position + 1] = bits.ByteOffset1;
                Buffer[Position + 2] = bits.ByteOffset2;
                Buffer[Position + 3] = bits.ByteOffset3;
            }

            Position += sizeof(float);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value)
        {
            BitDouble bits = new BitDouble(value);
            if (BitConverter.IsLittleEndian)
            {
                Buffer[Position] = bits.ByteOffset7;
                Buffer[Position + 1] = bits.ByteOffset6;
                Buffer[Position + 2] = bits.ByteOffset5;
                Buffer[Position + 3] = bits.ByteOffset4;
                Buffer[Position + 4] = bits.ByteOffset3;
                Buffer[Position + 5] = bits.ByteOffset2;
                Buffer[Position + 6] = bits.ByteOffset1;
                Buffer[Position + 7] = bits.ByteOffset0;
            }
            else
            {
                Buffer[Position] = bits.ByteOffset0;
                Buffer[Position + 1] = bits.ByteOffset1;
                Buffer[Position + 2] = bits.ByteOffset2;
                Buffer[Position + 3] = bits.ByteOffset3;
                Buffer[Position + 4] = bits.ByteOffset4;
                Buffer[Position + 5] = bits.ByteOffset5;
                Buffer[Position + 6] = bits.ByteOffset6;
                Buffer[Position + 7] = bits.ByteOffset7;
            }

            Position += sizeof(double);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] array, int offset, int size)
        {
            Array.Copy(array, offset, Buffer, Position, size);
            Position += size;
        }

        #endregion //Write

            #region Read

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            bool ret = Buffer[Position] == 1 ? true : false;
            Position++;
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte()
        {
            sbyte ret = unchecked((sbyte)Buffer[Position]);
            Position += sizeof(sbyte);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            byte ret = Buffer[Position];
            Position += sizeof(byte);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar()
        {
            return (char)ReadShort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort()
        {
            BitShort bits = new BitShort();
            if (BitConverter.IsLittleEndian)
            {
                bits.ByteOffset1 = Buffer[Position];
                bits.ByteOffset0 = Buffer[Position + 1];
            }
            else
            {
                bits.ByteOffset0 = Buffer[Position];
                bits.ByteOffset1 = Buffer[Position + 1];
            }

            Position += sizeof(short);
            return bits.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUShort()
        {
            return (ushort)ReadShort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            BitInt bits = new BitInt();
            if (BitConverter.IsLittleEndian)
            {
                bits.ByteOffset3 = Buffer[Position];
                bits.ByteOffset2 = Buffer[Position + 1];
                bits.ByteOffset1 = Buffer[Position + 2];
                bits.ByteOffset0 = Buffer[Position + 3];
            }
            else
            {
                bits.ByteOffset0 = Buffer[Position];
                bits.ByteOffset1 = Buffer[Position + 1];
                bits.ByteOffset2 = Buffer[Position + 2];
                bits.ByteOffset3 = Buffer[Position + 3];
            }

            Position += sizeof(int);
            return bits.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt()
        {
            return (uint)ReadInt();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong()
        {
            BitLong bits = new BitLong();
            if (BitConverter.IsLittleEndian)
            {
                bits.ByteOffset7 = Buffer[Position];
                bits.ByteOffset6 = Buffer[Position + 1];
                bits.ByteOffset5 = Buffer[Position + 2];
                bits.ByteOffset4 = Buffer[Position + 3];
                bits.ByteOffset3 = Buffer[Position + 4];
                bits.ByteOffset2 = Buffer[Position + 5];
                bits.ByteOffset1 = Buffer[Position + 6];
                bits.ByteOffset0 = Buffer[Position + 7];
            }
            else
            {
                bits.ByteOffset0 = Buffer[Position];
                bits.ByteOffset1 = Buffer[Position + 1];
                bits.ByteOffset2 = Buffer[Position + 2];
                bits.ByteOffset3 = Buffer[Position + 3];
                bits.ByteOffset4 = Buffer[Position + 4];
                bits.ByteOffset5 = Buffer[Position + 5];
                bits.ByteOffset6 = Buffer[Position + 6];
                bits.ByteOffset7 = Buffer[Position + 7];
            }

            Position += sizeof(long);
            return bits.Value; ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadULong()
        {
            return (ulong)ReadLong();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            BitFloat bits = new BitFloat();
            if (BitConverter.IsLittleEndian)
            {
                bits.ByteOffset3 = Buffer[Position];
                bits.ByteOffset2 = Buffer[Position + 1];
                bits.ByteOffset1 = Buffer[Position + 2];
                bits.ByteOffset0 = Buffer[Position + 3];
            }
            else
            {
                bits.ByteOffset0 = Buffer[Position];
                bits.ByteOffset1 = Buffer[Position + 1];
                bits.ByteOffset2 = Buffer[Position + 2];
                bits.ByteOffset3 = Buffer[Position + 3];
            }

            Position += sizeof(float);
            return bits.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            BitDouble bits = new BitDouble();
            if (BitConverter.IsLittleEndian)
            {
                bits.ByteOffset7 = Buffer[Position];
                bits.ByteOffset6 = Buffer[Position + 1];
                bits.ByteOffset5 = Buffer[Position + 2];
                bits.ByteOffset4 = Buffer[Position + 3];
                bits.ByteOffset3 = Buffer[Position + 4];
                bits.ByteOffset2 = Buffer[Position + 5];
                bits.ByteOffset1 = Buffer[Position + 6];
                bits.ByteOffset0 = Buffer[Position + 7];
            }
            else
            {
                bits.ByteOffset0 = Buffer[Position];
                bits.ByteOffset1 = Buffer[Position + 1];
                bits.ByteOffset2 = Buffer[Position + 2];
                bits.ByteOffset3 = Buffer[Position + 3];
                bits.ByteOffset4 = Buffer[Position + 4];
                bits.ByteOffset5 = Buffer[Position + 5];
                bits.ByteOffset6 = Buffer[Position + 6];
                bits.ByteOffset7 = Buffer[Position + 7];
            }

            Position += sizeof(double);
            return bits.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadByteArray(out byte[] array, int offset, int size)
        {
            array = new byte[size];
            Array.Copy(Buffer, Position, array, offset, size);
            Position += size;
        }

        #endregion //Read


    }
}
