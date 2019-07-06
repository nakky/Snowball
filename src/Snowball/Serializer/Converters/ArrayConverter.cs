using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class ArrayConverter : Converter
    {
        byte[] buf = new byte[sizeof(int)];

        Converter converter;
        Type type;

        public ArrayConverter(Type type) 
        {
            this.type = type;
            converter = DataSerializer.GetConverter(type.GetElementType());
        }

        public override void Serialize(Stream stream, object data)
        {
            if (data == null)
            {
                byte[] lbuf = BitConverter.GetBytes(-1);
                stream.Write(lbuf, 0, lbuf.Length);
            }
            else
            {
                Array array = (Array)data;

                byte[] lbuf = BitConverter.GetBytes(array.Length);
                stream.Write(lbuf, 0, lbuf.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    converter.Serialize(stream, array.GetValue(i));
                }
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(int));
            int length = BitConverter.ToInt32(buf, 0);

            if (length < 0)
            {
                return null;
            }
            else
            {
                Array array = Array.CreateInstance(type.GetElementType(), length);

                for (int i = 0; i < length; i++)
                {
                    array.SetValue(converter.Deserialize(stream), i);
                }
                return array;
            }
        }
    }


    public class ByteArrayConverter : Converter
    {
        byte[] buf = new byte[sizeof(int)];

        public static Converter constract() { return new ByteArrayConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            if (data == null)
            {
                byte[] lbuf = BitConverter.GetBytes(-1);
                stream.Write(lbuf, 0, lbuf.Length);
            }
            else
            {
                byte[] array = (byte[])data;

                byte[] lbuf = BitConverter.GetBytes(array.Length);
                stream.Write(lbuf, 0, lbuf.Length);

                stream.Write(array, 0, array.Length);
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(int));
            int length = BitConverter.ToInt32(buf, 0);

            if (length < 0)
            {
                return null;
            }
            else
            {
                byte[] array = new byte[length];

                stream.Read(array, 0, length);
                return array;
            }
        }
    }
}
