using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class ArrayConverter : Converter
    {
        Converter converter;
        Type type;

        public ArrayConverter(Type type) 
        {
            this.type = type;
            converter = DataSerializer.GetConverter(type.GetElementType());
        }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write(-1);
            }
            else
            {
                Array array = (Array)data;

                packer.Write(array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    converter.Serialize(packer, array.GetValue(i));
                }
            }
        }

        public override object Deserialize(BytePacker packer)
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

        public override int GetDataSize(object data)
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
    }


    public class ByteArrayConverter : Converter
    {
        public static Converter constract() { return new ByteArrayConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write(-1);
            }
            else
            {
                byte[] array = (byte[])data;

                packer.Write(array.Length);

                packer.Write(array, 0, array.Length);
            }
        }

        public override object Deserialize(BytePacker packer)
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

        public override int GetDataSize(object data)
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
    }
}
