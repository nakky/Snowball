using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Snowball
{
    public class DateTimeConverter : Converter
    {
        byte[] dbuf = new byte[sizeof(long)];

        public static Converter constract() { return new DateTimeConverter(); }


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

                stream.Write(BitConverter.GetBytes(((DateTime)data).ToBinary()), 0, sizeof(long));
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
                stream.Read(dbuf, 0, sizeof(long));
                return DateTime.FromBinary(BitConverter.ToInt64(dbuf, 0));
            }
        }
    }

    public class TimeSpanConverter : Converter
    {
        byte[] dbuf = new byte[sizeof(long)];

        public static Converter constract() { return new TimeSpanConverter(); }


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

                stream.Write(BitConverter.GetBytes(((TimeSpan)data).Ticks), 0, sizeof(long));
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
                stream.Read(dbuf, 0, sizeof(long));
                return TimeSpan.FromTicks(BitConverter.ToInt64(dbuf, 0));
            }
        }
    }

}


