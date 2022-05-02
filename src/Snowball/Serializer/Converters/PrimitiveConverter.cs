using System;
using System.IO;

namespace Snowball
{
    public class BoolConverter : IConverter
    {
        public static IConverter constract() { return new BoolConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteBool((bool)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadBool();
        }

        public int GetDataSize(object data)
        {
            return 1;
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += 1;
            return 1;
        }

    }

    public class CharConverter : IConverter
    {
        public static IConverter constract() { return new CharConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteChar((char)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadChar();
        }

        public int GetDataSize(object data)
        {
            return sizeof(char);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(char);
            return sizeof(char);
        }
    }

    public class SByteConverter : IConverter
    {
        public static IConverter constract() { return new SByteConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteSByte((sbyte)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadSByte();
        }

        public int GetDataSize(object data)
        {
            return sizeof(sbyte);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(sbyte);
            return sizeof(sbyte);
        }
    }

    public class ByteConverter : IConverter
    {
        public static IConverter constract() { return new ByteConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteByte((byte)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadByte();
        }

        public int GetDataSize(object data)
        {
            return sizeof(byte);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(byte);
            return sizeof(byte);
        }
    }

    public class ShortConverter : IConverter
    {
        public static IConverter constract() { return new ShortConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteShort((short)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadShort();
        }

        public int GetDataSize(object data)
        {
            return sizeof(short);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(short);
            return sizeof(short);
        }
    }

    public class UShortConverter : IConverter
    {
        public static IConverter constract() { return new UShortConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteUShort((ushort)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadUShort();
        }

        public int GetDataSize(object data)
        {
            return sizeof(ushort);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(ushort);
            return sizeof(ushort);
        }
    }

    public class IntConverter : IConverter
    {
        public static IConverter constract() { return new IntConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteInt((int)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadInt();
        }

        public int GetDataSize(object data)
        {
            return sizeof(int);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(int);
            return sizeof(int);
        }
    }

    public class UIntConverter : IConverter
    {
        public static IConverter constract() { return new UIntConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteUInt((uint)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadUInt();
        }

        public int GetDataSize(object data)
        {
            return sizeof(uint);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(uint);
            return sizeof(uint);
        }
    }

    public class LongConverter : IConverter
    {
        public static IConverter constract() { return new LongConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteLong((long)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadLong();
        }

        public int GetDataSize(object data)
        {
            return sizeof(long);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(long);
            return sizeof(long);
        }
    }

    public class ULongConverter : IConverter
    {
        public static IConverter constract() { return new ULongConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteULong((ulong)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadULong();
        }

        public int GetDataSize(object data)
        {
            return sizeof(ulong);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(ulong);
            return sizeof(ulong);
        }
    }

    public class FloatConverter : IConverter
    {
        public static IConverter constract() { return new FloatConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteFloat((float)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadFloat();
        }

        public int GetDataSize(object data)
        {
            return sizeof(float);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(float);
            return sizeof(float);
        }
    }

    public class DoubleConverter : IConverter
    {
        public static IConverter constract() { return new DoubleConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            packer.WriteDouble((double)data);
        }

        public object Deserialize(BytePacker packer)
        {
            return packer.ReadDouble();
        }

        public int GetDataSize(object data)
        {
            return sizeof(double);
        }

        public int GetDataSize(BytePacker packer)
        {
            packer.Position += sizeof(double);
            return sizeof(double);
        }
    }

    public class StringASCIIConverter : IConverter
    {
        public static IConverter constract() { return new StringASCIIConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            string strData = (string)data;

            if(strData == null)
            {
                packer.WriteInt(-1);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.ASCII.GetBytes(strData);
                packer.WriteInt(strbuf.Length);
                if (strbuf.Length > 0) packer.WriteByteArray(strbuf, 0, strbuf.Length);
            }
        }

        public object Deserialize(BytePacker packer)
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

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(int);
            return sizeof(int) + System.Text.Encoding.ASCII.GetByteCount((string)data);
        }

        public int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            packer.Position += length;
            return sizeof(int) + length;
        }

    }

    public class StringUnicodeConverter : IConverter
    {
        public static IConverter constract() { return new StringUnicodeConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            string strData = (string)data;

            if (strData == null)
            {
                packer.WriteInt(-1);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.Unicode.GetBytes(strData);
                packer.WriteInt(strbuf.Length);
                if (strbuf.Length > 0) packer.WriteByteArray(strbuf, 0, strbuf.Length);
            }
        }

        public object Deserialize(BytePacker packer)
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

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(int);
            return sizeof(int) + System.Text.Encoding.Unicode.GetByteCount((string)data);
        }

        public int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            packer.Position += length;
            return sizeof(int) + length;
        }
    }

    public class StringUtf8Converter : IConverter
    {
        public static IConverter constract() { return new StringUtf8Converter(); }

        public void Serialize(BytePacker packer, object data)
        {
            string strData = (string)data;

            if (strData == null)
            {
                packer.WriteInt(-1);
            }
            else
            {
                byte[] strbuf = System.Text.Encoding.UTF8.GetBytes(strData);
                packer.WriteInt(strbuf.Length);
                if (strbuf.Length > 0) packer.WriteByteArray(strbuf, 0, strbuf.Length);
            }
        }

        public object Deserialize(BytePacker packer)
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

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(int);
            return sizeof(int) + System.Text.Encoding.UTF8.GetByteCount((string)data);
        }

        public int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            packer.Position += length;
            return sizeof(int) + length;
        }
    }
}
