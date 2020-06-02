using System;

namespace Snowball
{
    public class VarShortConverter : Converter
    {
        public static Converter constract() { return new VarShortConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeShort((short)data, packer, out s);
        }

        public override object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToShort(packer, out s);
        }

        public override int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[4];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeShort((short)data, packer, out s);
            return s;
        }

        public override int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToShort(packer, out s);
            return s;
        }
    }

    public class VarUShortConverter : Converter
    {
        public static Converter constract() { return new VarUShortConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeUShort((ushort)data, packer, out s);
        }

        public override object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToUShort(packer, out s);
        }

        public override int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[4];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeUShort((ushort)data, packer, out s);
            return s;
        }

        public override int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToUShort(packer, out s);
            return s;
        }
    }

    public class VarIntConverter : Converter
    {
        public static Converter constract() { return new VarIntConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeInt((int)data, packer, out s);
        }

        public override object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToInt(packer, out s);
        }

        public override int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[8];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeInt((int)data, packer, out s);
            return s;
        }

        public override int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToInt(packer, out s);
            return s;
        }
    }

    public class VarUIntConverter : Converter
    {
        public static Converter constract() { return new VarUIntConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeUInt((uint)data, packer, out s);
        }

        public override object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToUInt(packer, out s);
        }

        public override int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[8];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeUInt((uint)data, packer, out s);
            return s;
        }

        public override int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToUInt(packer, out s);
            return s;
        }
    }

    public class VarLongConverter : Converter
    {
        public static Converter constract() { return new VarLongConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeLong((long)data, packer, out s);
        }

        public override object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToLong(packer, out s);
        }

        public override int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[16];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeLong((long)data, packer, out s);
            return s;
        }

        public override int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToLong(packer, out s);
            return s;
        }
    }

    public class VarULongConverter : Converter
    {
        public static Converter constract() { return new VarULongConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            int s;
            VarintBitConverter.SerializeULong((ulong)data, packer, out s);
        }

        public override object Deserialize(BytePacker packer)
        {
            int s;
            return VarintBitConverter.ToULong(packer, out s);
        }

        public override int GetDataSize(object data)
        {
            int s;
            byte[] array = new byte[16];
            BytePacker packer = new BytePacker(array);
            VarintBitConverter.SerializeULong((ulong)data, packer, out s);
            return s;
        }

        public override int GetDataSize(BytePacker packer)
        {
            int s;
            VarintBitConverter.ToULong(packer, out s);
            return s;
        }
    }
}
