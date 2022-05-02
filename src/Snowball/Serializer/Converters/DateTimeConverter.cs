using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class DateTimeConverter : IConverter
    {
        public static IConverter constract() { return new DateTimeConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                packer.WriteLong(((DateTime)data).ToBinary());
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
                return DateTime.FromBinary(packer.ReadLong());
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null)
            {
                return sizeof(byte);
            }
            else
            {
                return sizeof(byte) + sizeof(long) ;
            }

        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return 0;

            packer.Position += sizeof(long);
            return sizeof(byte) + sizeof(long);
        }
    }

    public class TimeSpanConverter : IConverter
    {
        public static IConverter constract() { return new TimeSpanConverter(); }

        public void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.WriteByte((byte)0);
            }
            else
            {
                packer.WriteByte((byte)1);

                packer.WriteLong(((TimeSpan)data).Ticks);
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
                return TimeSpan.FromTicks(packer.ReadLong());
            }
        }

        public int GetDataSize(object data)
        {
            if (data == null)
            {
                return sizeof(byte);
            }
            else
            {
                return sizeof(byte) + sizeof(long);
            }

        }

        public int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return 0;

            packer.Position += sizeof(long);
            return sizeof(byte) + sizeof(long);
        }
    }

}


