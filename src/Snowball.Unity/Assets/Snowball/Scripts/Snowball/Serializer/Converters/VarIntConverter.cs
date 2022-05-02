using System;

namespace Snowball
{
    public class VarShortConverter : IConverter
    {
        public static IConverter constract() { return new VarShortConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeShort((short)data, packer, out s);
        }

        public object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToShort(packer, out s);
        }

        public int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[4];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeShort((short)data, packer, out s);
            return s;
        }

        public int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToShort(packer, out s);
            return s;
        }
    }

    public class VarUShortConverter : IConverter
    {
        public static IConverter constract() { return new VarUShortConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeUShort((ushort)data, packer, out s);
        }

        public object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToUShort(packer, out s);
        }

        public int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[4];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeUShort((ushort)data, packer, out s);
            return s;
        }

        public int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToUShort(packer, out s);
            return s;
        }
    }

    public class VarIntConverter : IConverter
    {
        public static IConverter constract() { return new VarIntConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeInt((int)data, packer, out s);
        }

        public object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToInt(packer, out s);
        }

        public int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[8];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeInt((int)data, packer, out s);
            return s;
        }

        public int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToInt(packer, out s);
            return s;
        }
    }

    public class VarUIntConverter : IConverter
    {
        public static IConverter constract() { return new VarUIntConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeUInt((uint)data, packer, out s);
        }

        public object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToUInt(packer, out s);
        }

        public int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[8];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeUInt((uint)data, packer, out s);
            return s;
        }

        public int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToUInt(packer, out s);
            return s;
        }
    }

    public class VarLongConverter : IConverter
    {
        public static IConverter constract() { return new VarLongConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeLong((long)data, packer, out s);
        }

        public object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToLong(packer, out s);
        }

        public int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[16];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeLong((long)data, packer, out s);
            return s;
        }

        public int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToLong(packer, out s);
            return s;
        }
    }

    public class VarULongConverter : IConverter
    {
        public static IConverter constract() { return new VarULongConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeULong((ulong)data, packer, out s);
        }

        public object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToULong(packer, out s);
        }

        public int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[16];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeULong((ulong)data, packer, out s);
            return s;
        }

        public int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToULong(packer, out s);
            return s;
        }
    }
}
