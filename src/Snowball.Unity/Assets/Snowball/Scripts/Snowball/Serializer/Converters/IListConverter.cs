using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class IListConverter : Converter
    {
        byte[] dbuf = new byte[sizeof(int)];

        Converter converter;
        Type type;
        Type elementType;

        public IListConverter(Type type)
        {
            this.type = type;
            this.elementType = type.GetGenericArguments()[0];

            converter = DataSerializer.GetConverter(elementType);
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
                System.Collections.IList list = (System.Collections.IList)data;
                byte[] lbuf = BitConverter.GetBytes(list.Count);
                stream.Write(lbuf, 0, lbuf.Length);

                for (int i = 0; i < list.Count; i++)
                {
                    converter.Serialize(stream, list[i]);
                }
            }   
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(dbuf, 0, sizeof(int));
            int length = BitConverter.ToInt32(dbuf, 0);

            if (length < 0)
            {
                return null;
            }
            else
            {
                System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(type);

                for (int i = 0; i < length; i++)
                {
                    list.Add(converter.Deserialize(stream));
                }

                return list;
            }  
        }
    }

}

