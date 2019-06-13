using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using Xunit;
using Xunit.Abstractions;

using Snowball;

namespace Snowball.Tests
{
    [Collection(nameof(ComServerFixture))]
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
                //short src = (short)random.Next(-200, 200);

                int s = 0;
                VarintBitConverter.SerializeShort(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                short dst = VarintBitConverter.ToInt16(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException();
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
                //ushort src = (ushort)random.Next(0, 400);

                int s = 0;
                VarintBitConverter.SerializeUShort(src, stream, out s);
                size = stream.Position;
                stream.Position = 0;

                ushort dst = VarintBitConverter.ToUInt16(stream, out s);

                //Util.Log("src:" + src + ", dst:" + dst + ", size:" + size);

                if (src != dst) throw new InvalidProgramException();
            }
        }

    }
}
