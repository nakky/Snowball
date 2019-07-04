#if UNITY_2017_1_OR_NEWER

using System;
using System.IO;

using UnityEngine;

namespace Snowball
{
    public class Vector2Converter : Converter
    {
        byte[] buf = new byte[sizeof(float)];

        public static Converter constract() { return new Vector2Converter(); }

        public override void Serialize(Stream stream, object data)
        {
            Vector2 vector = (Vector2)data;

            stream.Write(BitConverter.GetBytes(vector.x), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(vector.y), 0, sizeof(float));
        }

        public override object Deserialize(Stream stream)
        {
            Vector2 vector = new Vector2();

            stream.Read(buf, 0, sizeof(float));
            vector.x = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            vector.y = BitConverter.ToSingle(buf, 0);

            return vector;
        }
    }

    public class Vector3Converter : Converter
    {
        byte[] buf = new byte[sizeof(float)];

        public static Converter constract() { return new Vector3Converter(); }

        public override void Serialize(Stream stream, object data)
        {
            Vector3 vector = (Vector3)data;

            stream.Write(BitConverter.GetBytes(vector.x), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(vector.y), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(vector.z), 0, sizeof(float));
        }

        public override object Deserialize(Stream stream)
        {
            Vector3 vector = new Vector3();

            stream.Read(buf, 0, sizeof(float));
            vector.x = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            vector.y = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            vector.z = BitConverter.ToSingle(buf, 0);

            return vector;
        }
    }

    public class Vector4Converter : Converter
    {
        byte[] buf = new byte[sizeof(float)];

        public static Converter constract() { return new Vector4Converter(); }

        public override void Serialize(Stream stream, object data)
        {
            Vector4 vector = (Vector4)data;

            stream.Write(BitConverter.GetBytes(vector.x), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(vector.y), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(vector.z), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(vector.w), 0, sizeof(float));
        }

        public override object Deserialize(Stream stream)
        {
            Vector4 vector = new Vector4();

            stream.Read(buf, 0, sizeof(float));
            vector.x = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            vector.y = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            vector.z = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            vector.w = BitConverter.ToSingle(buf, 0);

            return vector;
        }
    }

    public class QuaternionConverter : Converter
    {
        byte[] buf = new byte[sizeof(float)];

        public static Converter constract() { return new QuaternionConverter(); }

        public override void Serialize(Stream stream, object data)
        {
            Quaternion quaternion = (Quaternion)data;

            stream.Write(BitConverter.GetBytes(quaternion.x), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(quaternion.y), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(quaternion.z), 0, sizeof(float));
            stream.Write(BitConverter.GetBytes(quaternion.w), 0, sizeof(float));
        }

        public override object Deserialize(Stream stream)
        {
            Quaternion quaternion = new Quaternion();

            stream.Read(buf, 0, sizeof(float));
            quaternion.x = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            quaternion.y = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            quaternion.z = BitConverter.ToSingle(buf, 0);
            stream.Read(buf, 0, sizeof(float));
            quaternion.w = BitConverter.ToSingle(buf, 0);

            return quaternion;
        }
    }

}

#endif
