﻿using System;
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

        public static void Serialize<T>(BytePacker packer, T data)
        {
            if (!setup) Setup();

            Converter converter;
            if (!converterMap.TryGetValue(typeof(T), out converter))
            {
                converter = GetConverter(typeof(T));
                converterMap.Add(typeof(T), converter);
            }
            converter.Serialize(packer, data);
        }

        public static T Deserialize<T>(BytePacker packer)
        {
            if (!setup) Setup();

            Converter converter;
            if (!converterMap.TryGetValue(typeof(T), out converter)){
                converter = GetConverter(typeof(T));
                converterMap.Add(typeof(T), converter);
            }
            return (T)converter.Deserialize(packer);
        }

        public static Converter GetConverter(Type type)
        {
            if (!setup) Setup();

            ConverterConstractor constractor;
            if (constractorMap.TryGetValue(type, out constractor))
            {
                //already registered
                return constractor();
            }
            else if (type.IsEnum)
            {
                //enum
                if (constractorMap.TryGetValue(Enum.GetUnderlyingType(type), out constractor))
                {
                    return constractor();
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

            return new ClassConverter(type);
        }

        public static void AddConverterConstructor(Type type, ConverterConstractor constractor)
        {
            if (!constractorMap.ContainsKey(type))
            {
                constractorMap.Add(type, constractor);
            }
        }


        public static void RemoveConverterConstructor(Type type)
        {
            if (constractorMap.ContainsKey(type))
            {
                constractorMap.Remove(type);
            }
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

            constractorMap.Add(typeof(Vector2), Vector2Converter.constract);
            constractorMap.Add(typeof(Vector3), Vector3Converter.constract);
            constractorMap.Add(typeof(Vector4), Vector4Converter.constract);
            constractorMap.Add(typeof(Quaternion), QuaternionConverter.constract);

            constractorMap.Add(typeof(Color), ColorConverter.constract);
            constractorMap.Add(typeof(Color32), Color32Converter.constract);

            setup = true;
        }

    }
}
