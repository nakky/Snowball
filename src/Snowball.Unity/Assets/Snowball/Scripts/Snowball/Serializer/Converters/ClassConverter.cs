using System;
using System.IO;
using System.Collections.Generic;

using System.Reflection;

using System.Linq;

namespace Snowball
{
    public class ClassConverter : Converter
    {
        Type type;

        List<Converter> converters = new List<Converter>();
        List<PropertyInfo> parameters = new List<PropertyInfo>();

        byte[] buf = new byte[sizeof(byte)];

        public ClassConverter(Type type)
        {
            this.type = type;

            object[] attributes = type.GetCustomAttributes(typeof(TransferableAttribute), true);

            foreach (TransferableAttribute attr in attributes)
            {
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var props = properties.Where(x => Attribute.GetCustomAttributes(x, typeof(DataAttribute)).Length > 0)
                    .OrderBy(x => ((DataAttribute)Attribute.GetCustomAttributes(x, typeof(DataAttribute))[0]).Index);

                foreach (PropertyInfo p in props)
                {
                    //Util.Log("name:" + f.Name);

                    var dattrs = Attribute.GetCustomAttributes(p, typeof(DataAttribute));

                    foreach(var datt in dattrs)
                    {
                        parameters.Add(p);
                        converters.Add(DataSerializer.GetConverter(p.PropertyType));
                        break;
                    }
                }

                return;
            }

            throw new InvalidDataException("The class " + type.Name + " is not supported.");
        }

        public override void Serialize(Stream stream, object data)
        {
            if(data == null)
            {
                byte[] lbuf = { 0 };
                stream.Write(lbuf, 0, lbuf.Length);
            }
            else
            {
                byte[] lbuf = { 1 };
                stream.Write(lbuf, 0, lbuf.Length);
                for (int i = 0; i < parameters.Count; i++)
                {
                    converters[i].Serialize(stream, parameters[i].GetValue(data));
                }
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(buf, 0, sizeof(byte));
            if(buf[0] == 0)
            {
                return null;
            }
            else
            {
                object data = Activator.CreateInstance(type);

                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].SetValue(data, converters[i].Deserialize(stream));
                }

                return data;
            }
        }
    }
}
