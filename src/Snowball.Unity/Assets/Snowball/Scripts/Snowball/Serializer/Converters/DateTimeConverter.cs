using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class DateTimeConverter : Converter
    {
        public static Converter constract() { return new DateTimeConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write((byte)0);
            }
            else
            {
                packer.Write((byte)1);

                packer.Write(((DateTime)data).ToBinary());
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
                return DateTime.FromBinary(packer.ReadLong());
            }
        }

        public override int GetDataSize(object data)
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

        public override int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return 0;

            packer.Position += sizeof(long);
            return sizeof(byte) + sizeof(long);
        }
    }

    public class TimeSpanConverter : Converter
    {
        public static Converter constract() { return new TimeSpanConverter(); }

        public override void Serialize(BytePacker packer, object data)
        {
            if (data == null)
            {
                packer.Write((byte)0);
            }
            else
            {
                packer.Write((byte)1);

                packer.Write(((TimeSpan)data).Ticks);
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
                return TimeSpan.FromTicks(packer.ReadLong());
            }
        }

        public override int GetDataSize(object data)
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

        public override int GetDataSize(BytePacker packer)
        {
            byte isNull = packer.ReadByte();
            if (isNull == 0) return 0;

            packer.Position += sizeof(long);
            return sizeof(byte) + sizeof(long);
        }
    }

}


