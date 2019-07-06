using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using System.Runtime.Serialization.Formatters.Binary;

namespace Snowball
{
    public class SerializableConverter : Converter
    {

        BinaryFormatter formatter = new BinaryFormatter();

        Type type;

        byte[] buf = new byte[sizeof(byte)];

        public SerializableConverter(Type type)
        {
            this.type = type;
        }

        public override void Serialize(Stream stream, object data)
        {
            if (data == null)
            {
                byte[] lbuf = { 0 };
                stream.Write(lbuf, 0, lbuf.Length);
            }
            else
            {
                byte[] lbuf = { 1 };
                stream.Write(lbuf, 0, lbuf.Length);

                formatter.Serialize(stream, data);
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(byte));
            if (buf[0] == 0)
            {
                return null;
            }
            else
            {
                return formatter.Deserialize(stream);
            }  
        }
    }
}

