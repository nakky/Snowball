using System;
using System.IO;
using System.Collections.Generic;

using System.Reflection;

using System.Linq;

namespace Snowball
{
    public class ClassConverter : Converter
    {
        List<Converter> converters = new List<Converter>();

        Type type;

        List<FieldInfo> parameters = new List<FieldInfo>();

        public ClassConverter(Type type)
        {
            this.type = type;

            object[] attributes = type.GetCustomAttributes(typeof(TransferableAttribute), true);

            foreach (TransferableAttribute attr in attributes)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var fs = fields.OrderBy(x => ((DataAttribute)Attribute.GetCustomAttributes(x, typeof(DataAttribute))[0]).Index);

                foreach (FieldInfo f in fs)
                {
                    //Util.Log("name:" + f.Name);

                    var dattrs = Attribute.GetCustomAttributes(f, typeof(DataAttribute));

                    foreach(var datt in dattrs)
                    {
                        parameters.Add(f);
                        converters.Add(DataSerializer.GetConverter(f.FieldType));
                        break;
                    }
                }

                return;
            }

            throw new InvalidDataException("The class " + type.Name + " is not supported.");
        }

        public override void Serialize(Stream stream, object data)
        {
            for(int i = 0; i < parameters.Count ; i++)
            {
                converters[i].Serialize(stream, parameters[i].GetValue(data));
            }
        }

        public override object Deserialize(Stream stream)
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
