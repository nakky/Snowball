using System;
using System.IO;

using System.Collections.Generic;

using System.Reflection;
using System.Runtime.InteropServices;

#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif

namespace Snowball
{
    public static class DataSerializer
    {
        static bool setup = false;

        static Dictionary<Type, Converter> converterMap = new Dictionary<Type, Converter>();
        static Dictionary<Type, ConverterConstractor> constractorMap = new Dictionary<Type, ConverterConstractor>();

        public delegate Converter ConverterConstractor();

        public static void Serialize<T>(Stream stream, T data)
        {
            if (!setup) Setup();

            if (!converterMap.ContainsKey(typeof(T)))
            {
                converterMap.Add(typeof(T), GetConverter(typeof(T)));
            }
            converterMap[typeof(T)].Serialize(stream, data);
        }

        public static T Deserialize<T>(Stream stream)
        {
            if (!setup) Setup();

            if (!converterMap.ContainsKey(typeof(T))){
                converterMap.Add(typeof(T), GetConverter(typeof(T)));
            }
            return (T)converterMap[typeof(T)].Deserialize(stream);
        }

        public static Converter GetConverter(Type type)
        {
            if (!setup) Setup();

            if (constractorMap.ContainsKey(type))
            {
                //already registered
                return constractorMap[type]();
            }
            else if (type.IsEnum)
            {
                //enum
                if (constractorMap.ContainsKey(Enum.GetUnderlyingType(type)))
                {
                    return constractorMap[Enum.GetUnderlyingType(type)]();
                }
            }
            else if (type.IsArray)
            {
                return new ArrayConverter(type);
            }
            else if (typeof(System.Collections.IList).IsAssignableFrom(type))
            {
                return new IListConverter(type);
            }
            else if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                return new IDictionaryConverter(type);
            }
            else if (type.IsSerializable)
            {
                return new SerializableConverter(type);
            }

            return new ClassConverter(type);
        }

        public static void AddConverterConstructor(Type type, ConverterConstractor constractor)
        {
            constractorMap.Add(type, constractor);
        }

        public static void RemoveConverterConstructor(Type type)
        {
            constractorMap.Remove(type);
        }

        public static void Setup()
        {
            constractorMap.Add(typeof(bool), BoolConverter.constract);
            constractorMap.Add(typeof(char), CharConverter.constract);
            constractorMap.Add(typeof(sbyte), SByteConverter.constract);
            constractorMap.Add(typeof(byte), ByteConverter.constract);
            constractorMap.Add(typeof(short), ShortConverter.constract);
            constractorMap.Add(typeof(ushort), UShortConverter.constract);
            constractorMap.Add(typeof(int), IntConverter.constract);
            constractorMap.Add(typeof(uint), UIntConverter.constract);
            constractorMap.Add(typeof(long), LongConverter.constract);
            constractorMap.Add(typeof(ulong), ULongConverter.constract);
            constractorMap.Add(typeof(float), FloatConverter.constract);
            constractorMap.Add(typeof(double), DoubleConverter.constract);

            constractorMap.Add(typeof(string), StringUtf8Converter.constract);

            constractorMap.Add(typeof(DateTime), DateTimeConverter.constract);
            constractorMap.Add(typeof(TimeSpan), TimeSpanConverter.constract);

            constractorMap.Add(typeof(byte[]), ByteArrayConverter.constract);

#if UNITY_2017_1_OR_NEWER

            constractorMap.Add(typeof(Vector2), Vector2Converter.constract);
            constractorMap.Add(typeof(Vector3), Vector3Converter.constract);
            constractorMap.Add(typeof(Vector4), Vector4Converter.constract);
            constractorMap.Add(typeof(Quaternion), QuaternionConverter.constract);

#endif

            setup = true;
        }

    }
}
