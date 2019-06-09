using System;

namespace Snowball
{
    public enum StringEncode{
        ASCII = 0,
        UTF8 = 1
    }

    public class Serializer
    {
        public byte[] Buffer { get; set; }
        public int CurrentPosition { get; private set; }

        public Serializer(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public Serializer(int bufSize)
        {
            this.Buffer = new byte[bufSize];
        }

        public void Reset()
        {
            CurrentPosition = 0;
        }

        public void Write(byte value)
        {
            Buffer[CurrentPosition] = value;
            CurrentPosition += sizeof(byte);
        }

        public void Write(short value)
        {
            byte[] tbuf = BitConverter.GetBytes(value);
            tbuf.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += sizeof(short);
        }

        public void Write(ushort value)
        {
            byte[] tbuf = BitConverter.GetBytes(value);
            tbuf.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += sizeof(ushort);
        }

        public void Write(int value)
        {
            byte[] tbuf = BitConverter.GetBytes(value);
            tbuf.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += sizeof(int);
        }

        public void Write(uint value)
        {
            byte[] tbuf = BitConverter.GetBytes(value);
            tbuf.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += sizeof(uint);
        }

        public void Write(float value)
        {
            byte[] tbuf = BitConverter.GetBytes(value);
            tbuf.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += sizeof(float);
        }

        public void Write(double value)
        {
            byte[] tbuf = BitConverter.GetBytes(value);
            tbuf.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += sizeof(double);
        }

        public void Write(string text, StringEncode encode)
        {
            Write((int)text.Length);
            Write((byte)encode);

            byte[] data;

            if(encode == StringEncode.UTF8){
                data = System.Text.Encoding.UTF8.GetBytes(text);
            }else{
                data = System.Text.Encoding.ASCII.GetBytes(text);
            }

            data.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += data.Length;
        }

        public void Write(byte[] data, int index, int length)
        {
            Array.Copy(data, index, Buffer, CurrentPosition, length);
            CurrentPosition += length;
        }

        public void Write(IntPtr value)
        {
            byte[] tbuf = BitConverter.GetBytes(value.ToInt64());
            tbuf.CopyTo(Buffer, CurrentPosition);
            CurrentPosition += sizeof(Int64);
        }
    }

    public class Deserializer
    {
        public byte[] Buffer { get; set; }
        public int CurrentPosition { get; private set; }

        public Deserializer(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public void Reset()
        {
            CurrentPosition = 0;
        }

        public void Read(ref byte value)
        {
            value = Buffer[CurrentPosition];
            CurrentPosition += sizeof(byte);
        }
        
        public void Read(ref short value)
        {
            value = BitConverter.ToInt16(Buffer, CurrentPosition);
            CurrentPosition += sizeof(short);
        }

        public void Read(ref ushort value)
        {
            value = BitConverter.ToUInt16(Buffer, CurrentPosition);
            CurrentPosition += sizeof(ushort);
        }

        public void Read(ref int value)
        {
            value = BitConverter.ToInt32(Buffer, CurrentPosition);
            CurrentPosition += sizeof(int);
        }

        public void Read(ref uint value)
        {
            value = BitConverter.ToUInt32(Buffer, CurrentPosition);
            CurrentPosition += sizeof(uint);
        }

        public void Read(ref float value)
        {
            value = BitConverter.ToSingle(Buffer, CurrentPosition);
            CurrentPosition += sizeof(float);
        }

        public void Read(ref double value)
        {
            value = BitConverter.ToDouble(Buffer, CurrentPosition);
            CurrentPosition += sizeof(double);
        }

        public void Read(ref string value)
        {
            int length = 0;
            byte encode = 0;

            Read(ref length);
            Read(ref encode);

            if(encode == (byte)StringEncode.UTF8){
                value = System.Text.Encoding.UTF8.GetString(Buffer, CurrentPosition, length);
            }else{
                value = System.Text.Encoding.ASCII.GetString(Buffer, CurrentPosition, length);
            }
            CurrentPosition += length;
        }

        public void Read(ref byte[] data, int length)
        {
            Array.Copy(Buffer, CurrentPosition, data, 0, length);
            CurrentPosition += length;
        }

        public void Read(ref IntPtr value)
        {
            value = (IntPtr)BitConverter.ToInt64(Buffer, CurrentPosition);
            CurrentPosition += sizeof(Int64);
        }

    }

       
}


