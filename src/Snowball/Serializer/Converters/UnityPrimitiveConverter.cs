using System;

#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif

namespace Snowball
{
    public class Vector2Converter : IConverter
    {
        public static IConverter constract() { return new Vector2Converter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                Vector2 vector = (Vector2)data;

                packer.WriteFloat(vector.x);
                packer.WriteFloat(vector.y);
            }
        }

        public object Deserialize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0)
            {
                return null;
            }
            else
            {
                Vector2 vector = new Vector2();

                vector.x = packer.ReadFloat();
                vector.y = packer.ReadFloat();

                return vector;
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(byte);
            return sizeof(byte) + sizeof(float) * 2;
        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 2;
            return sizeof(byte) + sizeof(float) * 2;
        }
    }

    public class Vector3Converter : IConverter
    {
        public static IConverter constract() { return new Vector3Converter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                Vector3 vector = (Vector3)data;

                packer.WriteFloat(vector.x);
                packer.WriteFloat(vector.y);
                packer.WriteFloat(vector.z);
            }
        }

        public object Deserialize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0)
            {
                return null;
            }
            else
            {
                Vector3 vector = new Vector3();

                vector.x = packer.ReadFloat();
                vector.y = packer.ReadFloat();
                vector.z = packer.ReadFloat();

                return vector;
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(byte);
            return sizeof(byte) + sizeof(float) * 3;
        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 3;
            return sizeof(byte) + sizeof(float) * 3;
        }
    }

    public class Vector4Converter : IConverter
    {
        public static IConverter constract() { return new Vector4Converter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                Vector4 vector = (Vector4)data;

                packer.WriteFloat(vector.x);
                packer.WriteFloat(vector.y);
                packer.WriteFloat(vector.z);
                packer.WriteFloat(vector.w);
            }
        }

        public object Deserialize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0)
            {
                return null;
            }
            else
            {
                Vector4 vector = new Vector4();

                vector.x = packer.ReadFloat();
                vector.y = packer.ReadFloat();
                vector.z = packer.ReadFloat();
                vector.w = packer.ReadFloat();

                return vector;
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(byte);
            return sizeof(byte) + sizeof(float) * 4;
        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 4;
            return sizeof(byte) + sizeof(float) * 4;
        }
    }

    public class QuaternionConverter : IConverter
    {
        public static IConverter constract() { return new QuaternionConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                Quaternion quaternion = (Quaternion)data;

                packer.WriteFloat(quaternion.x);
                packer.WriteFloat(quaternion.y);
                packer.WriteFloat(quaternion.z);
                packer.WriteFloat(quaternion.w);
            }

        }

        public object Deserialize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0)
            {
                return null;
            }
            else
            {
                Quaternion quaternion = new Quaternion();

                quaternion.x = packer.ReadFloat();
                quaternion.y = packer.ReadFloat();
                quaternion.z = packer.ReadFloat();
                quaternion.w = packer.ReadFloat();

                return quaternion;
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(byte);
            return sizeof(byte) + sizeof(float) * 4;
        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 4;
            return sizeof(byte) + sizeof(float) * 4;
        }
    }

    public class ColorConverter : IConverter
    {
        public static IConverter constract() { return new ColorConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                Color color = (Color)data;

                packer.WriteFloat(color.r);
                packer.WriteFloat(color.g);
                packer.WriteFloat(color.b);
                packer.WriteFloat(color.a);
            }
        }

        public object Deserialize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0)
            {
                return null;
            }
            else
            {
                Color color = new Color();

                color.r = packer.ReadFloat();
                color.g = packer.ReadFloat();
                color.b = packer.ReadFloat();
                color.a = packer.ReadFloat();

                return color;
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(byte);
            return sizeof(byte) + sizeof(float) * 4;
        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 4;
            return sizeof(byte) + sizeof(float) * 4;
        }
    }

    public class Color32Converter : IConverter
    {
        public static IConverter constract() { return new Color32Converter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                Color32 color = (Color32)data;

                packer.WriteByte(color.r);
                packer.WriteByte(color.g);
                packer.WriteByte(color.b);
                packer.WriteByte(color.a);
            }
        }

        public object Deserialize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0)
            {
                return null;
            }
            else
            {
                Color32 color = new Color32();

                color.r = packer.ReadByte();
                color.g = packer.ReadByte();
                color.b = packer.ReadByte();
                color.a = packer.ReadByte();

                return color;
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null) return sizeof(byte);
            return sizeof(byte) + sizeof(byte) * 4;
        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(byte) * 4;
            return sizeof(byte) + sizeof(byte) * 4;
        }
    }

}

