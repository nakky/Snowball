using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class ListConverter : IConverter
    {
        IConverter converter;
        Type type;
        Type elementType;

        public ListConverter(Type type)
        {
            this.type = type;
            this.elementType = type.GetGenericArguments()[0];

            converter = DataSerializer.GetConverter(elementType);
        }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteInt(-1);
            }
            else
            {
                System.Collections.IList list = (System.Collections.IList)data;

                packer.WriteInt(list.Count);

                for (int i = 0; i < list.Count; i++)
                {
                    converter.Serialize(packer, list[i]);
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
                System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(type);

                for (int i = 0; i < length; i++)
                {
                    list.Add(converter.Deserialize(packer));
                }

                return list;
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
                int size = sizeof(int);

                System.Collections.IList list = (System.Collections.IList)data;
                for (int i = 0; i < list.Count ; i++)
                {
                    size += converter.GetDataSize(list[i]);
                }

                return size;
            }

        }

        public int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            int size = sizeof(int);

            System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(type);

            for (int i = 0; i < length; i++)
            {
                size += converter.GetDataSize(packer);
            }

            return size;
        }
    }

}

