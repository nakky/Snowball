using System;
using System.IO;

namespace Snowball
{
    public class BoolConverter : Converter
    {
        byte[] buf = new byte[sizeof(bool)];

        public static Converter constract() { return new BoolConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((bool)data), 0, sizeof(bool));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(bool));
            return BitConverter.ToBoolean(buf, 0);
        }

    }

    public class CharConverter : Converter
    {
        byte[] buf = new byte[sizeof(char)];

        public static Converter constract() { return new CharConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((char)data), 0, sizeof(char));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(char));
            return BitConverter.ToChar(buf, 0);
        }
    }

    public class SByteConverter : Converter
    {
        byte[] buf = new byte[sizeof(sbyte)];

        public static Converter constract() { return new SByteConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((sbyte)data), 0, sizeof(sbyte));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(sbyte));
            return (sbyte)buf[0];
        }
    }

    public class ByteConverter : Converter
    {
        byte[] buf = new byte[sizeof(byte)];

        public static Converter constract() { return new ByteConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((byte)data), 0, sizeof(byte));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(byte));
            return buf[0];
        }
    }

    public class ShortConverter : Converter
    {
        byte[] buf = new byte[sizeof(short)];

        public static Converter constract() { return new ShortConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((short)data), 0, sizeof(short));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(short));
            return BitConverter.ToInt16(buf, 0);
        }
    }

    public class UShortConverter : Converter
    {
        byte[] buf = new byte[sizeof(ushort)];

        public static Converter constract() { return new UShortConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((ushort)data), 0, sizeof(ushort));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(ushort));
            return BitConverter.ToUInt16(buf, 0);
        }
    }

    public class IntConverter : Converter
    {
        byte[] buf = new byte[sizeof(int)];

        public static Converter constract() { return new IntConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((int)data), 0, sizeof(int));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(int));
            return BitConverter.ToInt32(buf, 0);
        }
    }

    public class UIntConverter : Converter
    {
        byte[] buf = new byte[sizeof(uint)];

        public static Converter constract() { return new UIntConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((uint)data), 0, sizeof(uint));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(uint));
            return BitConverter.ToUInt32(buf, 0);
        }
    }

    public class LongConverter : Converter
    {
        byte[] buf = new byte[sizeof(long)];

        public static Converter constract() { return new LongConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((long)data), 0, sizeof(long));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(long));
            return BitConverter.ToInt64(buf, 0);
        }
    }

    public class ULongConverter : Converter
    {
        byte[] buf = new byte[sizeof(ulong)];

        public static Converter constract() { return new ULongConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((ulong)data), 0, sizeof(ulong));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(ulong));
            return BitConverter.ToUInt64(buf, 0);
        }
    }

    public class FloatConverter : Converter
    {
        byte[] buf = new byte[sizeof(float)];

        public static Converter constract() { return new FloatConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((float)data), 0, sizeof(float));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(float));
            return BitConverter.ToSingle(buf, 0);
        }
    }

    public class DoubleConverter : Converter
    {
        byte[] buf = new byte[sizeof(double)];

        public static Converter constract() { return new DoubleConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            stream.Write(BitConverter.GetBytes((double)data), 0, sizeof(double));
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(double));
            return BitConverter.ToDouble(buf, 0);
        }
    }

    public class StringASCIIConverter : Converter
    {
        byte[] buf = new byte[sizeof(int)];

        public static Converter constract() { return new StringASCIIConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            string strData = (string)data;

            if(strData == null)
            {
                byte[] lbuf = BitConverter.GetBytes(-1);
                stream.Write(lbuf, 0, lbuf.Length);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.ASCII.GetBytes(strData);

                byte[] lbuf = BitConverter.GetBytes(strbuf.Length);
                stream.Write(lbuf, 0, lbuf.Length);

                if (strbuf.Length > 0) stream.Write(strbuf, 0, strbuf.Length);
            }
        }

        public override object Deserialize(Stream stream)
        {
            object data;

            stream.Read(buf, 0, sizeof(int));
            int length = BitConverter.ToInt32(buf, 0);

            if(length < 0)
            {
                return null;
            }
            else if(length > 0){
                byte[] strData = new byte[length];
                stream.Read(strData, 0, length);

                string text = System.Text.Encoding.ASCII.GetString(strData);

                data = (string)text;
            }else{
                data = "";
            }
           return data;
        }

    }

    public class StringUnicodeConverter : Converter
    {

        byte[] buf = new byte[sizeof(int)];

        public static Converter constract() { return new StringUnicodeConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            string strData = (string)data;

            if (strData == null)
            {
                byte[] lbuf = BitConverter.GetBytes(-1);
                stream.Write(lbuf, 0, lbuf.Length);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.Unicode.GetBytes(strData);

                byte[] lbuf = BitConverter.GetBytes(strbuf.Length);
                stream.Write(lbuf, 0, lbuf.Length);

                if (strbuf.Length > 0) stream.Write(strbuf, 0, strbuf.Length);
            }
        }

        public override object Deserialize(Stream stream)
        {
            object data;

            stream.Read(buf, 0, sizeof(int));
            int length = BitConverter.ToInt32(buf, 0);

            if (length < 0)
            {
                return null;
            }
            else if (length > 0)
            {
                byte[] strData = new byte[length];
                stream.Read(strData, 0, length);

                string text = System.Text.Encoding.Unicode.GetString(strData);

                data = (string)text;
            }
            else
            {
                data = "";
            }
            return data;
        }

    }

    public class StringUtf8Converter : Converter
    {

        byte[] buf = new byte[sizeof(int)];

        public static Converter constract() { return new StringUtf8Converter(); }

        public override void Serialize(Stream stream, object data)
        {
            string strData = (string)data;

            if (strData == null)
            {
                byte[] lbuf = BitConverter.GetBytes(-1);
                stream.Write(lbuf, 0, lbuf.Length);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.UTF8.GetBytes(strData);

                byte[] lbuf = BitConverter.GetBytes(strbuf.Length);
                stream.Write(lbuf, 0, lbuf.Length);

                if (strbuf.Length > 0) stream.Write(strbuf, 0, strbuf.Length);
            }
        }

        public override object Deserialize(Stream stream)
        {
            object data;

            stream.Read(buf, 0, sizeof(int));
            int length = BitConverter.ToInt32(buf, 0);

            if (length < 0)
            {
                return null;
            }
            else if (length > 0)
            {
                byte[] strData = new byte[length];
                stream.Read(strData, 0, length);

                string text = System.Text.Encoding.UTF8.GetString(strData);

                data = (string)text;
            }
            else
            {
                data = "";
            }
            return data;
        }

    }
}
