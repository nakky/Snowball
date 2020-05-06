#if UNITY_2017_1_OR_NEWER

using System;

using UnityEngine;

namespace Snowball
{
    public class Vector2Converter : Converter
    {
        public static Converter constract() { return new Vector2Converter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write((byte)0);
            }
            else
            {
                packer.Write((byte)1);

                Vector2 vector = (Vector2)data;

                packer.Write(vector.x);
                packer.Write(vector.y);
            }
        }

        public override object Deserialize(BytePacker packer)
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

        public override int GetDataSize(object data)
        {
            return sizeof(byte) + sizeof(float) * 2;
        }

        public override int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 2;
            return sizeof(byte) + sizeof(float) * 2;
        }
    }

    public class Vector3Converter : Converter
    {
        public static Converter constract() { return new Vector3Converter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write((byte)0);
            }
            else
            {
                packer.Write((byte)1);

                Vector3 vector = (Vector3)data;

                packer.Write(vector.x);
                packer.Write(vector.y);
                packer.Write(vector.z);
            }
        }

        public override object Deserialize(BytePacker packer)
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

        public override int GetDataSize(object data)
        {
            return sizeof(byte) + sizeof(float) * 3;
        }

        public override int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 3;
            return sizeof(byte) + sizeof(float) * 3;
        }
    }

    public class Vector4Converter : Converter
    {
        public static Converter constract() { return new Vector4Converter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write((byte)0);
            }
            else
            {
                packer.Write((byte)1);

                Vector4 vector = (Vector4)data;

                packer.Write(vector.x);
                packer.Write(vector.y);
                packer.Write(vector.z);
                packer.Write(vector.w);
            }
        }

        public override object Deserialize(BytePacker packer)
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

        public override int GetDataSize(object data)
        {
            return sizeof(byte) + sizeof(float) * 4;
        }

        public override int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 4;
            return sizeof(byte) + sizeof(float) * 4;
        }
    }

    public class QuaternionConverter : Converter
    {
        public static Converter constract() { return new QuaternionConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write((byte)0);
            }
            else
            {
                packer.Write((byte)1);

                Quaternion quaternion = (Quaternion)data;

                packer.Write(quaternion.x);
                packer.Write(quaternion.y);
                packer.Write(quaternion.z);
                packer.Write(quaternion.w);
            }

        }

        public override object Deserialize(BytePacker packer)
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

        public override int GetDataSize(object data)
        {
            return sizeof(byte) + sizeof(float) * 4;
        }

        public override int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return sizeof(byte);

            packer.Position += sizeof(float) * 4;
            return sizeof(byte) + sizeof(float) * 4;
        }
    }

}

#endif
