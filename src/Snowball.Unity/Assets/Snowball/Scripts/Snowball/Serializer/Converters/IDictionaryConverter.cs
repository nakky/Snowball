using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class IDictionaryConverter : Converter
    {
        byte[] buf = new byte[sizeof(int)];

        Converter keyConverter;
        Converter valueConverter;

        Type type;
        Type keyType;
        Type valueType;

        public IDictionaryConverter(Type type)
        {
            this.type = type;
            this.keyType = type.GetGenericArguments()[0];
            this.valueType = type.GetGenericArguments()[1];

            keyConverter = DataSerializer.GetConverter(keyType);
            valueConverter = DataSerializer.GetConverter(valueType);
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
                System.Collections.IDictionary dictionary = (System.Collections.IDictionary)data;

                byte[] lbuf = BitConverter.GetBytes(dictionary.Count);
                stream.Write(lbuf, 0, lbuf.Length);

                foreach (System.Collections.DictionaryEntry kvp in dictionary)
                {
                    keyConverter.Serialize(stream, kvp.Key);
                    valueConverter.Serialize(stream, kvp.Value);
                }
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(int));
            int length = BitConverter.ToInt32(buf, 0);

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
                    key = keyConverter.Deserialize(stream);
                    value = valueConverter.Deserialize(stream);
                    dictionary.Add(key, value);
                }

                return dictionary;
            }
        }
    }

}

