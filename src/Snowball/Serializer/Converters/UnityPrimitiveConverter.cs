#if UNITY_2017_1_OR_NEWER

using System;
using System.IO;

using UnityEngine;

namespace Snowball
{
    public class Vector2Converter : Converter
    {
        byte[] dbuf = new byte[sizeof(float)];

        public static Converter constract() { return new Vector2Converter(); }

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

                Vector2 vector = (Vector2)data;

                stream.Write(BitConverter.GetBytes(vector.x), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(vector.y), 0, sizeof(float));
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(dbuf, 0, sizeof(byte));
            if (dbuf[0] == 0)
            {
                return null;
            }
            else
            {
                Vector2 vector = new Vector2();

                stream.Read(dbuf, 0, sizeof(float));
                vector.x = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                vector.y = BitConverter.ToSingle(dbuf, 0);

                return vector;
            }
        }
    }

    public class Vector3Converter : Converter
    {
        byte[] dbuf = new byte[sizeof(float)];

        public static Converter constract() { return new Vector3Converter(); }

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

                Vector3 vector = (Vector3)data;

                stream.Write(BitConverter.GetBytes(vector.x), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(vector.y), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(vector.z), 0, sizeof(float));
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(dbuf, 0, sizeof(byte));
            if (dbuf[0] == 0)
            {
                return null;
            }
            else
            {
                Vector3 vector = new Vector3();

                stream.Read(dbuf, 0, sizeof(float));
                vector.x = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                vector.y = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                vector.z = BitConverter.ToSingle(dbuf, 0);

                return vector;
            } 
        }
    }

    public class Vector4Converter : Converter
    {
        byte[] dbuf = new byte[sizeof(float)];

        public static Converter constract() { return new Vector4Converter(); }

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

                Vector4 vector = (Vector4)data;

                stream.Write(BitConverter.GetBytes(vector.x), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(vector.y), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(vector.z), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(vector.w), 0, sizeof(float));
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(dbuf, 0, sizeof(byte));
            if (buf[0] == 0)
            {
                return null;
            }
            else
            {
                Vector4 vector = new Vector4();

                stream.Read(dbuf, 0, sizeof(float));
                vector.x = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                vector.y = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                vector.z = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                vector.w = BitConverter.ToSingle(dbuf, 0);

                return vector;
            }
        }
    }

    public class QuaternionConverter : Converter
    {
        byte[] dbuf = new byte[sizeof(float)];

        public static Converter constract() { return new QuaternionConverter(); }

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

                Quaternion quaternion = (Quaternion)data;

                stream.Write(BitConverter.GetBytes(quaternion.x), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(quaternion.y), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(quaternion.z), 0, sizeof(float));
                stream.Write(BitConverter.GetBytes(quaternion.w), 0, sizeof(float));
            }
        }

        public override object Deserialize(Stream stream)
        {
            stream.Read(dbuf, 0, sizeof(byte));
            if (buf[0] == 0)
            {
                return null;
            }
            else
            {
                Quaternion quaternion = new Quaternion();

                stream.Read(dbuf, 0, sizeof(float));
                quaternion.x = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                quaternion.y = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                quaternion.z = BitConverter.ToSingle(dbuf, 0);
                stream.Read(dbuf, 0, sizeof(float));
                quaternion.w = BitConverter.ToSingle(dbuf, 0);

                return quaternion;
            }  
        }
    }

}

#endif
