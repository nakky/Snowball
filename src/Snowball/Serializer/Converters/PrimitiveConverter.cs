using System;
using System.IO;

namespace Snowball
{
    public class BoolConverter : Converter
    {
        public static Converter constract() { return new BoolConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((bool)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadBool();
        }

        public override int GetDataSize(object data)
        {
            return 1;
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += 1;
            return 1;
        }

    }

    public class CharConverter : Converter
    {
        public static Converter constract() { return new CharConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((char)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadChar();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(char);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(char);
            return sizeof(char);
        }
    }

    public class SByteConverter : Converter
    {
        public static Converter constract() { return new SByteConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((sbyte)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadSByte();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(sbyte);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(sbyte);
            return sizeof(sbyte);
        }
    }

    public class ByteConverter : Converter
    {
        public static Converter constract() { return new ByteConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((byte)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadByte();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(byte);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(byte);
            return sizeof(byte);
        }
    }

    public class ShortConverter : Converter
    {
        public static Converter constract() { return new ShortConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((short)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadShort();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(short);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(short);
            return sizeof(short);
        }
    }

    public class UShortConverter : Converter
    {
        public static Converter constract() { return new UShortConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((ushort)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadUShort();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(ushort);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(ushort);
            return sizeof(ushort);
        }
    }

    public class IntConverter : Converter
    {
        public static Converter constract() { return new IntConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((int)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadInt();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(int);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(int);
            return sizeof(int);
        }
    }

    public class UIntConverter : Converter
    {
        public static Converter constract() { return new UIntConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((uint)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadUInt();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(uint);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(uint);
            return sizeof(uint);
        }
    }

    public class LongConverter : Converter
    {
        public static Converter constract() { return new LongConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((long)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadLong();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(long);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(long);
            return sizeof(long);
        }
    }

    public class ULongConverter : Converter
    {
        public static Converter constract() { return new ULongConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((ulong)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadULong();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(ulong);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(ulong);
            return sizeof(ulong);
        }
    }

    public class FloatConverter : Converter
    {
        public static Converter constract() { return new FloatConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((float)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadFloat();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(float);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(float);
            return sizeof(float);
        }
    }

    public class DoubleConverter : Converter
    {
        public static Converter constract() { return new DoubleConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            packer.Write((double)data);
        }

        public override object Deserialize(BytePacker packer)
        {
            return packer.ReadDouble();
        }

        public override int GetDataSize(object data)
        {
            return sizeof(double);
        }

        public override int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(double);
            return sizeof(double);
        }
    }

    public class StringASCIIConverter : Converter
    {
        public static Converter constract() { return new StringASCIIConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            string strData = (string)data;

            if(strData == null)
            {
                packer.Write(-1);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.ASCII.GetBytes(strData);
                packer.Write(strbuf.Length);
                if (strbuf.Length > 0) packer.Write(strbuf, 0, strbuf.Length);
            }
        }

        public override object Deserialize(BytePacker packer)
        {
            object data;

            int length = packer.ReadInt();

            if(length < 0)
            {
                return null;
            }
            else if(length > 0){
                byte[] strData;
                packer.ReadByteArray(out strData, 0, length);

                string text = System.Text.Encoding.ASCII.GetString(strData);

                data = (string)text;
            }else{
                data = "";
            }
           return data;
        }

        public override int GetDataSize(object data)
        {
            if (data == null) return sizeof(int);
            return sizeof(int) + System.Text.Encoding.ASCII.GetByteCount((string)data);
        }

        public override int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            packer.Position += length;
            return sizeof(int) + length;
        }

    }

    public class StringUnicodeConverter : Converter
    {
        public static Converter constract() { return new StringUnicodeConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            string strData = (string)data;

            if (strData == null)
            {
                packer.Write(-1);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.Unicode.GetBytes(strData);
                packer.Write(strbuf.Length);
                if (strbuf.Length > 0) packer.Write(strbuf, 0, strbuf.Length);
            }
        }

        public override object Deserialize(BytePacker packer)
        {
            object data;

            int length = packer.ReadInt();

            if (length < 0)
            {
                return null;
            }
            else if (length > 0)
            {
                byte[] strData;
                packer.ReadByteArray(out strData, 0, length);

                string text = System.Text.Encoding.Unicode.GetString(strData);

                data = (string)text;
            }
            else
            {
                data = "";
            }
            return data;
        }

        public override int GetDataSize(object data)
        {
            if (data == null) return sizeof(int);
            return sizeof(int) + System.Text.Encoding.Unicode.GetByteCount((string)data);
        }

        public override int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            packer.Position += length;
            return sizeof(int) + length;
        }
    }

    public class StringUtf8Converter : Converter
    {
        public static Converter constract() { return new StringUtf8Converter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            string strData = (string)data;

            if (strData == null)
            {
                packer.Write(-1);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.UTF8.GetBytes(strData);
                packer.Write(strbuf.Length);
                if (strbuf.Length > 0) packer.Write(strbuf, 0, strbuf.Length);
            }
        }

        public override object Deserialize(BytePacker packer)
        {
            object data;

            int length = packer.ReadInt();

            if (length < 0)
            {
                return null;
            }
            else if (length > 0)
            {
                byte[] strData;
                packer.ReadByteArray(out strData, 0, length);

                string text = System.Text.Encoding.UTF8.GetString(strData);

                data = (string)text;
            }
            else
            {
                data = "";
            }
            return data;
        }

        public override int GetDataSize(object data)
        {
            if (data == null) return sizeof(int);
            return sizeof(int) + System.Text.Encoding.UTF8.GetByteCount((string)data);
        }

        public override int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            packer.Position += length;
            return sizeof(int) + length;
        }
    }
}
