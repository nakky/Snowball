using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class ArrayConverter : IConverter
    {
        IConverter converter;
        Type type;

        public ArrayConverter(Type type) 
        {
            this.type = type;
            converter = DataSerializer.GetConverter(type.GetElementType());
        }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteInt(-1);
            }
            else
            {
                Array array = (Array)data;

                packer.WriteInt(array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    converter.Serialize(packer, array.GetValue(i));
                }
            }
        }

        public object Deserialize(BytePacker packer)
        {
            int length = packer.ReadInt();

            if (length < 0)
            {
                return null;
            }
            else
            {
                Array array = Array.CreateInstance(type.GetElementType(), length);

                for (int i = 0; i < length; i++)
                {
                    array.SetValue(converter.Deserialize(packer), i);
                }
                return array;
            }
        }

        public int GetDataSize(object data)
        {
            if(data == null)
            {
                return sizeof(int);
            }
            else
            {
                int size = sizeof(int);

                Array array = (Array)data;
                for (int i = 0; i < array.Length; i++)
                {
                    size += converter.GetDataSize(array.GetValue(i));
                }

                return size;
            }
            
        }

        public int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            int size = sizeof(int);
            for (int i = 0; i < length; i++)
            {
                size += converter.GetDataSize(packer);
            }

            return sizeof(int) + length * size;
        }
    }


    public class ByteArrayConverter : IConverter
    {
        public static IConverter constract() { return new ByteArrayConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteInt(-1);
            }
            else
            {
                byte[] array = (byte[])data;

                packer.WriteInt(array.Length);

                packer.WriteByteArray(array, 0, array.Length);
            }
        }

        public object Deserialize(BytePacker packer)
        {
            int length = packer.ReadInt();

            if (length < 0)
            {
                return null;
            }
            else
            {
                byte[] array;

                packer.ReadByteArray(out array, 0, length);

                return array;
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null)
            {
                return sizeof(int);
            }
            else
            {
                byte[] array = (byte[])data;
                return sizeof(int) + array.Length;
            }

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
