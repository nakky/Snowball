using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class DictionaryConverter : IConverter
    {
        IConverter keyConverter;
        IConverter valueConverter;

        Type type;
        Type keyType;
        Type valueType;

        public DictionaryConverter(Type type)
        {
            this.type = type;
            this.keyType = type.GetGenericArguments()[0];
            this.valueType = type.GetGenericArguments()[1];

            keyConverter = DataSerializer.GetConverter(keyType);
            valueConverter = DataSerializer.GetConverter(valueType);
        }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteInt(-1);
            }
            else
            {
                System.Collections.IDictionary dictionary = (System.Collections.IDictionary)data;

                packer.WriteInt(dictionary.Count);

                foreach (System.Collections.DictionaryEntry kvp in dictionary)
                {
                    keyConverter.Serialize(packer, kvp.Key);
                    valueConverter.Serialize(packer, kvp.Value);
                }
            }
        }

        public object Deserialize(BytePacker packer)
        {
            int length = packer.ReadInt();

            if(length < 0)
            {
                return null;
            }
            else
            {
                System.Collections.IDictionary dictionary = (System.Collections.IDictionary)Activator.CreateInstance(type);

                object key, value;

                for (int i = 0; i < length; i++)
                {
                    key = keyConverter.Deserialize(packer);
                    value = valueConverter.Deserialize(packer);
                    dictionary.Add(key, value);
                }

                return dictionary;
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

                System.Collections.IDictionary dictionary = (System.Collections.IDictionary)data;

                foreach (System.Collections.DictionaryEntry kvp in dictionary)
                {
                    size += keyConverter.GetDataSize(kvp.Key);
                    size += valueConverter.GetDataSize(kvp.Value);
                }

                return size;
            }

        }

        public int GetDataSize(BytePacker packer)
        {
            int length = packer.ReadInt();
            if (length < 0) return sizeof(int);

            int size = sizeof(int);

            System.Collections.IDictionary dictionary = (System.Collections.IDictionary)Activator.CreateInstance(type);

            for (int i = 0; i < length; i++)
            {
                size += keyConverter.GetDataSize(packer);
                size += valueConverter.GetDataSize(packer);
            }

            return size;
        }
    }

}

