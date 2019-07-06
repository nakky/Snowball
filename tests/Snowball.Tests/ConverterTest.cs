using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;

using Snowball;

namespace Snowball.Tests
{
    public enum TestEnum
    {
        A,
        B,
        C
    }

    [Serializable]
    class TestSerializableClass
    {
        public int intData;
        public string stringData = "";

        public bool Equals(TestSerializableClass other)
        {
            return intData == other.intData && stringData == other.stringData;
        }
    };

    [Transferable]
    class TestParamClass
    {
        public int unused;

        [Data(2)]
        public int intData { get; set; }

        [Data(0)]
        public string stringData { get; set; }

        public bool Equals(TestParamClass other)
        {
            if (other == null) return false;

            return intData == other.intData && stringData == other.stringData;
        }
    };

    [Transferable]
    class TestConvertClass
    {
        [Data(2)]
        public int intData { get; set; }

        [Data(3)]
        public string stringData { get; set; }

        [Data(1)]
        public float flaotData { get; set; }

        [Data(0)]
        public TestParamClass classData { get; set; }

    public bool Equals(TestConvertClass other)
        {
            if (other == null) return false;

            return intData == other.intData
                && stringData == other.stringData
                && flaotData == other.flaotData
                && ( (classData == null && other.classData == null) || (classData.Equals(other.classData) ) )
                ;
        }
    };

    [Collection(nameof(ComServerFixture))]
    public class ConverterTest : IDisposable
    {
        public ConverterTest()
        {
        }

        public void Dispose()
        {
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void PrimitiveConverterTest()
        {
            Util.Log("VariantBitConverterShortTest");

            Random random = new Random();

            MemoryStream stream;

            //Bool
            stream = new MemoryStream();
            bool boolSrc = true;
            bool boolDst = false;
            DataSerializer.Serialize(stream, boolSrc);
            stream.Position = 0;
            boolDst = DataSerializer.Deserialize<bool>(stream);

            if (boolSrc != boolDst) throw new InvalidProgramException("bool");

            //char
            stream = new MemoryStream();
            char charSrc = (char)random.Next(char.MinValue, char.MaxValue);
            char charDst = '0';
            DataSerializer.Serialize(stream, charSrc);
            stream.Position = 0;
            charDst = DataSerializer.Deserialize<char>(stream);

            if (charSrc != charDst) throw new InvalidProgramException("char");

            //sbyte
            stream = new MemoryStream();
            sbyte sbyteSrc = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue);
            sbyte sbyteDst = 0;
            DataSerializer.Serialize(stream, sbyteSrc);
            stream.Position = 0;
            sbyteDst = DataSerializer.Deserialize<sbyte>(stream);

            if (sbyteSrc != sbyteDst) throw new InvalidProgramException("sbyte");

            //byte
            stream = new MemoryStream();
            byte byteSrc = (byte)random.Next(byte.MinValue, byte.MaxValue);
            byte byteDst = 0;
            DataSerializer.Serialize(stream, byteSrc);
            stream.Position = 0;
            byteDst = DataSerializer.Deserialize<byte>(stream);

            if (byteSrc != byteDst) throw new InvalidProgramException("byte");

            //short
            stream = new MemoryStream();
            short shortSrc = (short)random.Next(short.MinValue, short.MaxValue);
            short shortDst = 0;
            DataSerializer.Serialize(stream, shortSrc);
            stream.Position = 0;
            shortDst = DataSerializer.Deserialize<short>(stream);

            if (shortSrc != shortDst) throw new InvalidProgramException("short");

            //ushort
            stream = new MemoryStream();
            ushort ushortSrc = (ushort)random.Next(ushort.MinValue, ushort.MaxValue);
            ushort ushortDst = 0;
            DataSerializer.Serialize(stream, ushortSrc);
            stream.Position = 0;
            ushortDst = DataSerializer.Deserialize<ushort>(stream);

            if (ushortSrc != ushortDst) throw new InvalidProgramException("ushort");

            //int
            stream = new MemoryStream();
            int intSrc = random.Next();
            int intDst = 0;
            DataSerializer.Serialize(stream, intSrc);
            stream.Position = 0;
            intDst = DataSerializer.Deserialize<int>(stream);

            if (intSrc != intDst) throw new InvalidProgramException("int");

            //uint
            stream = new MemoryStream();
            uint uintSrc = (uint)random.Next(0, int.MaxValue);
            uint uintDst = 0;
            DataSerializer.Serialize(stream, uintSrc);
            stream.Position = 0;
            uintDst = DataSerializer.Deserialize<uint>(stream);

            if (uintSrc != uintDst) throw new InvalidProgramException("uint");

            //long
            stream = new MemoryStream();
            long longSrc = (long)random.Next();
            long longDst = 0;
            DataSerializer.Serialize(stream, longSrc);
            stream.Position = 0;
            longDst = DataSerializer.Deserialize<long>(stream);

            if (longSrc != longDst) throw new InvalidProgramException("long");

            //ulong
            stream = new MemoryStream();
            ulong ulongSrc = (ulong)random.Next(0, int.MaxValue);
            ulong ulongDst = 0;
            DataSerializer.Serialize(stream, ulongSrc);
            stream.Position = 0;
            ulongDst = DataSerializer.Deserialize<ulong>(stream);

            if (ulongSrc != ulongDst) throw new InvalidProgramException("ulong");

            //float
            stream = new MemoryStream();
            float floatSrc = (float)random.NextDouble();
            float floatDst = 0;
            DataSerializer.Serialize(stream, floatSrc);
            stream.Position = 0;
            floatDst = DataSerializer.Deserialize<float>(stream);

            if (floatSrc != floatDst) throw new InvalidProgramException("float");

            //double
            stream = new MemoryStream();
            double doubleSrc = random.NextDouble();
            double doubleDst = 0;
            DataSerializer.Serialize(stream, doubleSrc);
            stream.Position = 0;
            doubleDst = DataSerializer.Deserialize<double>(stream);

            if (doubleSrc != doubleDst) throw new InvalidProgramException("double");


            //string
            stream = new MemoryStream();
            string stringSrc = "こんにちはasdf";
            //string stringSrc = "sehiillhg";
            string stringDst = "";
            DataSerializer.Serialize(stream, stringSrc);
            stream.Position = 0;
            stringDst = DataSerializer.Deserialize<string>(stream);

            if (stringSrc != stringDst) throw new InvalidProgramException("string");

            //enum
            stream = new MemoryStream();
            TestEnum enumSrc = TestEnum.B;
            TestEnum enumDst = TestEnum.A;
            DataSerializer.Serialize(stream, enumSrc);
            stream.Position = 0;
            enumDst = DataSerializer.Deserialize<TestEnum>(stream);

            if (enumSrc != enumDst) throw new InvalidProgramException("enum");

            //DateTime
            stream = new MemoryStream();
            DateTime dateTimeSrc = DateTime.Now;
            DateTime dateTimeDst;
            DataSerializer.Serialize(stream, dateTimeSrc);
            stream.Position = 0;
            dateTimeDst = DataSerializer.Deserialize<DateTime>(stream);

            if (dateTimeSrc != dateTimeDst) throw new InvalidProgramException("DateTime");

            //TimeSpan
            stream = new MemoryStream();
            TimeSpan timeSpanSrc = new TimeSpan(98271983172893);
            TimeSpan timeSpanDst;
            DataSerializer.Serialize(stream, timeSpanSrc);
            stream.Position = 0;
            timeSpanDst = DataSerializer.Deserialize<TimeSpan>(stream);

            if (timeSpanSrc != timeSpanDst) throw new InvalidProgramException("TimeSpan");

            //class
            stream = new MemoryStream();
            TestConvertClass classSrc = new TestConvertClass();
            classSrc.intData = random.Next();
            classSrc.stringData = "asfeaefaw";
            classSrc.flaotData = (float)random.NextDouble();
            classSrc.classData = new TestParamClass();
            classSrc.classData.intData = random.Next();
            classSrc.classData.stringData = "igi3u8z2";

            TestConvertClass classDst;
            DataSerializer.Serialize(stream, classSrc);
            stream.Position = 0;
            classDst = DataSerializer.Deserialize<TestConvertClass>(stream);

            if (!classSrc.Equals(classDst)) throw new InvalidProgramException("TestConvertClass");


            //array primitive
            stream = new MemoryStream();
            byte[] bArraySrc = {1,2,3,4,5 };
            byte[] bArrayDst = null;
            DataSerializer.Serialize(stream, bArraySrc);
            stream.Position = 0;
            bArrayDst = DataSerializer.Deserialize<byte[]>(stream);

            if(bArraySrc.Length != bArrayDst.Length) throw new InvalidProgramException("byte[]");
            for (int i = 0; i < bArraySrc.Length; i++)
            {
                if(bArraySrc[i] != bArrayDst[i])
                throw new InvalidProgramException("byte[]");
            }

            //array class
            stream = new MemoryStream();
            TestConvertClass[] cArraySrc = { new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass() };
            cArraySrc[1].intData = 90;
            TestConvertClass[] cArrayDst = null;
            DataSerializer.Serialize(stream, cArraySrc);
            stream.Position = 0;
            cArrayDst = DataSerializer.Deserialize<TestConvertClass[]>(stream);

            if (cArraySrc.Length != cArrayDst.Length) throw new InvalidProgramException("TestConvertClass[]");
            for (int i = 0; i < bArraySrc.Length; i++)
            {
                if (!cArraySrc[i].Equals(cArrayDst[i]))
                    throw new InvalidProgramException("TestConvertClass[]");
            }

            //list class
            stream = new MemoryStream();
            List<TestConvertClass> cListSrc = new List<TestConvertClass>{ new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass() };
            cListSrc[1].intData = 90;
            List<TestConvertClass> cListDst = null;
            DataSerializer.Serialize(stream, cArraySrc);
            stream.Position = 0;
            cListDst = DataSerializer.Deserialize<List<TestConvertClass>>(stream);

            if (cListSrc.Count != cListDst.Count) throw new InvalidProgramException("List<TestConvertClass>");
            for (int i = 0; i < bArraySrc.Length; i++)
            {
                if (!cArraySrc[i].Equals(cListDst[i]))
                    throw new InvalidProgramException("List<TestConvertClass>");
            }

            //Dictionary class
            stream = new MemoryStream();
            Dictionary<string, TestConvertClass> cDictSrc = new Dictionary<string, TestConvertClass>();

            TestConvertClass tcc = new TestConvertClass();
            tcc.intData = 90;
            cDictSrc.Add("aaaa", tcc);
            tcc = new TestConvertClass();
            tcc.intData = 270;
            cDictSrc.Add("bbbb", tcc);

            Dictionary<string, TestConvertClass> cDictDst = null;

            DataSerializer.Serialize(stream, cDictSrc);
            stream.Position = 0;
            cDictDst = DataSerializer.Deserialize<Dictionary<string, TestConvertClass>>(stream);

            if (cDictSrc.Count != cDictDst.Count) throw new InvalidProgramException("Dictionary<string, TestConvertClass>");
            foreach(var pair in cDictSrc)
            {
                if (!pair.Value.Equals(cDictDst[pair.Key]))
                    throw new InvalidProgramException("Dictionary<string, TestConvertClass>");
            }


            //serializable class
            stream = new MemoryStream();
            TestSerializableClass serialiableSrc = new TestSerializableClass();
            serialiableSrc.intData = random.Next();
            serialiableSrc.stringData = "asfeaefaw";

            TestSerializableClass serialiableDst;
            DataSerializer.Serialize(stream, serialiableSrc);
            stream.Position = 0;
            serialiableDst = DataSerializer.Deserialize<TestSerializableClass>(stream);

            if (!serialiableSrc.Equals(serialiableDst)) throw new InvalidProgramException("TestSerializableClass");




        }




    }
}
