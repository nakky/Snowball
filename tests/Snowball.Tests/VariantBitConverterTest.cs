using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using Xunit;
using Xunit.Abstractions;

using Snowball;

namespace Snowball.Tests
{
    public class VariantBitConverterTest : IDisposable
    {
        public VariantBitConverterTest()
        {
        }

        public void Dispose()
        {
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantBitConverterShortTest()
        {
            Util.Log("VariantBitConverterShortTest");

            Random random = new Random();

            MemoryStream stream = new MemoryStream();
            long size = 0;

            for (int i = 0; i < 1000; i++)
            {
                stream.Position = 0;

                short src = (short)random.Next(short.MinValue, short.MaxValue);

                int s = 0;
                VarintBitConverter.SerializeShort(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                short dst = VarintBitConverter.ToShort(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException("index:" + i + ", src:" + src + ", dst:" + dst + ", size:" + size);
            }
        }


        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantBitConverterUShortTest()
        {
            Util.Log("VariantBitConverterUShortTest");

            Random random = new Random();

            MemoryStream stream = new MemoryStream();
            long size = 0;

            for (int i = 0; i < 1000; i++)
            {
                stream.Position = 0;

                ushort src = (ushort)random.Next(ushort.MinValue, ushort.MaxValue);

                int s = 0;
                VarintBitConverter.SerializeUShort(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                ushort dst = VarintBitConverter.ToUShort(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException("index:" + i + ", src:" + src + ", dst:" + dst + ", size:" + size);
            }
        }


        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantBitConverterIntTest()
        {
            Util.Log("VariantBitConverterIntTest");

            Random random = new Random();

            MemoryStream stream = new MemoryStream();
            long size = 0;

            for (int i = 0; i < 1000; i++)
            {
                stream.Position = 0;

                int src = (int)random.Next(int.MinValue, int.MaxValue);

                int s = 0;
                VarintBitConverter.SerializeInt(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                int dst = VarintBitConverter.ToInt(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException("index:" + i + ", src:" + src + ", dst:" + dst + ", size:" + size);
            }
        }


        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantBitConverterUIntTest()
        {
            Util.Log("VariantBitConverterUIntTest");

            Random random = new Random();

            MemoryStream stream = new MemoryStream();
            long size = 0;

            for (int i = 0; i < 1000; i++)
            {
                stream.Position = 0;

                uint src = (uint)random.Next(int.MinValue, int.MaxValue);

                int s = 0;
                VarintBitConverter.SerializeUInt(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                uint dst = VarintBitConverter.ToUInt(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException("index:" + i + ", src:" + src + ", dst:" + dst + ", size:" + size);
            }
        }


        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantBitConverterLongTest()
        {
            Util.Log("VariantBitConverterLongTest");

            Random random = new Random();

            MemoryStream stream = new MemoryStream();
            long size = 0;

            for (int i = 0; i < 1000; i++)
            {
                stream.Position = 0;

                long src = (long)(random.NextDouble() * Int64.MaxValue);
                int s = 0;
                VarintBitConverter.SerializeLong(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                long dst = VarintBitConverter.ToLong(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException("index:" + i + ", src:" + src + ", dst:" + dst + ", size:" + size);
            }
        }


        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantBitConverterULongTest()
        {
            Util.Log("VariantBitConverterULongTest");

            Random random = new Random();

            MemoryStream stream = new MemoryStream();
            long size = 0;

            for (int i = 0; i < 1000; i++)
            {
                stream.Position = 0;

                ulong src = (ulong)(random.NextDouble() * Int64.MaxValue);

                int s = 0;
                VarintBitConverter.SerializeULong(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                ulong dst = VarintBitConverter.ToULong(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException("index:" + i + ", src:" + src + ", dst:" + dst + ", size:" + size);
            }
        }

    }
}
