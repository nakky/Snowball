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
        public int IntData { get; set; }

        [Data(0)]
        public string StringData { get; set; }

        public bool Equals(TestParamClass other)
        {
            if (other == null) return false;

            return IntData == other.IntData && StringData == other.StringData;
        }
    };

    [Transferable]
    class TestConvertClass
    {
        [Data(2)]
        public int IntData { get; set; }

        [Data(3)]
        public string StringData { get; set; }

        [Data(1)]
        public float FlaotData { get; set; }

        [Data(0)]
        public TestParamClass classData { get; set; }

    public bool Equals(TestConvertClass other)
        {
            if (other == null) return false;

            return IntData == other.IntData
                && StringData == other.StringData
                && FlaotData == other.FlaotData
                && ( (classData == null && other.classData == null) || (classData.Equals(other.classData) ) )
                ;
        }
    };


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
        public void BytePackerTest()
        {
            Random random = new Random();

            byte[] buffer = new byte[64];

            BytePacker packer = new BytePacker(buffer);

            //Bool
            bool boolVal = true;
            packer.WriteBool(boolVal);

            sbyte sbyteVal = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue);
            packer.WriteSByte(sbyteVal);

            byte byteVal = (byte)random.Next(byte.MinValue, byte.MaxValue);
            packer.WriteByte(byteVal);

            char charVal = (char)random.Next(char.MinValue, char.MaxValue);
            packer.WriteChar(charVal);

            short shortVal = (short)random.Next(short.MinValue, short.MaxValue);
            packer.WriteShort(shortVal);

            ushort ushortVal = (ushort)random.Next(ushort.MinValue, ushort.MaxValue);
            packer.WriteUShort(ushortVal);

            int intVal = random.Next();
            packer.WriteInt(intVal);

            uint uintVal = (uint)random.Next(0, int.MaxValue) * 2;
            packer.WriteUInt(uintVal);

            long longVal = (long)random.Next() * 2;
            packer.WriteLong(longVal);

            ulong ulongVal = (uint)random.Next(0, int.MaxValue) * 2;
            packer.WriteULong(ulongVal);

            float floatVal = (float)random.NextDouble();
            packer.WriteFloat(floatVal);

            double doubleVal = random.NextDouble();
            packer.WriteDouble(doubleVal);

            packer.Position = 0;

            if(boolVal != packer.ReadBool()) throw new InvalidProgramException("bool");
            else if (sbyteVal != packer.ReadSByte()) throw new InvalidProgramException("sbyte");
            else if(byteVal != packer.ReadByte()) throw new InvalidProgramException("byte");
            else if (charVal != packer.ReadChar()) throw new InvalidProgramException("char");
            else if (shortVal != packer.ReadShort()) throw new InvalidProgramException("short");
            else if (ushortVal != packer.ReadUShort()) throw new InvalidProgramException("ushort");
            else if (intVal != packer.ReadInt()) throw new InvalidProgramException("int");
            else if (uintVal != packer.ReadUInt()) throw new InvalidProgramException("uint");
            else if (longVal != packer.ReadLong()) throw new InvalidProgramException("long");
            else if (ulongVal != packer.ReadULong()) throw new InvalidProgramException("ulong");
            else if (floatVal != packer.ReadFloat()) throw new InvalidProgramException("float");
            else if (doubleVal != packer.ReadDouble()) throw new InvalidProgramException("double");


        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void PrimitiveConverterTest()
        {
            Util.Log("VariantBitConverterShortTest");

            Random random = new Random();

            byte[] buffer = new byte[256];
            BytePacker packer;

            //Bool
            packer = new BytePacker(buffer);
            bool boolSrc = true;
            bool boolDst = false;
            DataSerializer.Serialize(packer, boolSrc);
            packer.Position = 0;
            boolDst = DataSerializer.Deserialize<bool>(packer);

            if (boolSrc != boolDst) throw new InvalidProgramException("bool");

            //char
            packer = new BytePacker(buffer);
            char charSrc = (char)random.Next(char.MinValue, char.MaxValue);
            char charDst = '0';
            DataSerializer.Serialize(packer, charSrc);
            packer.Position = 0;
            charDst = DataSerializer.Deserialize<char>(packer);

            if (charSrc != charDst) throw new InvalidProgramException("char");

            //sbyte
            packer = new BytePacker(buffer);
            sbyte sbyteSrc = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue);
            sbyte sbyteDst = 0;
            DataSerializer.Serialize(packer, sbyteSrc);
            packer.Position = 0;
            sbyteDst = DataSerializer.Deserialize<sbyte>(packer);

            if (sbyteSrc != sbyteDst) throw new InvalidProgramException("sbyte");

            //byte
            packer = new BytePacker(buffer);
            byte byteSrc = (byte)random.Next(byte.MinValue, byte.MaxValue);
            byte byteDst = 0;
            DataSerializer.Serialize(packer, byteSrc);
            packer.Position = 0;
            byteDst = DataSerializer.Deserialize<byte>(packer);

            if (byteSrc != byteDst) throw new InvalidProgramException("byte");

            //short
            packer = new BytePacker(buffer);
            short shortSrc = (short)random.Next(short.MinValue, short.MaxValue);
            short shortDst = 0;
            DataSerializer.Serialize(packer, shortSrc);
            packer.Position = 0;
            shortDst = DataSerializer.Deserialize<short>(packer);

            if (shortSrc != shortDst) throw new InvalidProgramException("short");

            //ushort
            packer = new BytePacker(buffer);
            ushort ushortSrc = (ushort)random.Next(ushort.MinValue, ushort.MaxValue);
            ushort ushortDst = 0;
            DataSerializer.Serialize(packer, ushortSrc);
            packer.Position = 0;
            ushortDst = DataSerializer.Deserialize<ushort>(packer);

            if (ushortSrc != ushortDst) throw new InvalidProgramException("ushort");

            //int
            packer = new BytePacker(buffer);
            int intSrc = random.Next();
            int intDst = 0;
            DataSerializer.Serialize(packer, intSrc);
            packer.Position = 0;
            intDst = DataSerializer.Deserialize<int>(packer);

            if (intSrc != intDst) throw new InvalidProgramException("int");

            //uint
            packer = new BytePacker(buffer);
            uint uintSrc = (uint)random.Next(0, int.MaxValue) * 2;
            uint uintDst = 0;
            DataSerializer.Serialize(packer, uintSrc);
            packer.Position = 0;
            uintDst = DataSerializer.Deserialize<uint>(packer);

            if (uintSrc != uintDst) throw new InvalidProgramException("uint");

            //long
            packer = new BytePacker(buffer);
            long longSrc = (long)random.Next();
            long longDst = 0;
            DataSerializer.Serialize(packer, longSrc);
            packer.Position = 0;
            longDst = DataSerializer.Deserialize<long>(packer);

            if (longSrc != longDst) throw new InvalidProgramException("long");

            //ulong
            packer = new BytePacker(buffer);
            ulong ulongSrc = (ulong)random.Next(0, int.MaxValue) * 2;
            ulong ulongDst = 0;
            DataSerializer.Serialize(packer, ulongSrc);
            packer.Position = 0;
            ulongDst = DataSerializer.Deserialize<ulong>(packer);

            if (ulongSrc != ulongDst) throw new InvalidProgramException("ulong");

            //float
            packer = new BytePacker(buffer);
            float floatSrc = (float)random.NextDouble();
            float floatDst = 0;
            DataSerializer.Serialize(packer, floatSrc);
            packer.Position = 0;
            floatDst = DataSerializer.Deserialize<float>(packer);

            if (floatSrc != floatDst) throw new InvalidProgramException("float");

            //double
            packer = new BytePacker(buffer);
            double doubleSrc = random.NextDouble();
            double doubleDst = 0;
            DataSerializer.Serialize(packer, doubleSrc);
            packer.Position = 0;
            doubleDst = DataSerializer.Deserialize<double>(packer);

            if (doubleSrc != doubleDst) throw new InvalidProgramException("double");


            //string
            packer = new BytePacker(buffer);
            string stringSrc = "こんにちはasdf";
            //string stringSrc = "sehiillhg";
            string stringDst = "";
            DataSerializer.Serialize(packer, stringSrc);
            packer.Position = 0;
            stringDst = DataSerializer.Deserialize<string>(packer);

            if (stringSrc != stringDst) throw new InvalidProgramException("string");

            //enum
            packer = new BytePacker(buffer);
            TestEnum enumSrc = TestEnum.B;
            TestEnum enumDst = TestEnum.A;
            DataSerializer.Serialize(packer, enumSrc);
            packer.Position = 0;
            enumDst = DataSerializer.Deserialize<TestEnum>(packer);

            if (enumSrc != enumDst) throw new InvalidProgramException("enum");

            //DateTime
            packer = new BytePacker(buffer);
            DateTime dateTimeSrc = DateTime.Now;
            DateTime dateTimeDst;
            DataSerializer.Serialize(packer, dateTimeSrc);
            packer.Position = 0;
            dateTimeDst = DataSerializer.Deserialize<DateTime>(packer);

            if (dateTimeSrc != dateTimeDst) throw new InvalidProgramException("DateTime");

            //TimeSpan
            packer = new BytePacker(buffer);
            TimeSpan timeSpanSrc = new TimeSpan(98271983172893);
            TimeSpan timeSpanDst;
            DataSerializer.Serialize(packer, timeSpanSrc);
            packer.Position = 0;
            timeSpanDst = DataSerializer.Deserialize<TimeSpan>(packer);

            if (timeSpanSrc != timeSpanDst) throw new InvalidProgramException("TimeSpan");

            //class
            packer = new BytePacker(buffer);
            TestConvertClass classSrc = new TestConvertClass();
            classSrc.IntData = random.Next();
            classSrc.StringData = "asfeaefaw";
            classSrc.FlaotData = (float)random.NextDouble();
            classSrc.classData = new TestParamClass();
            classSrc.classData.IntData = random.Next();
            classSrc.classData.StringData = "igi3u8z2";

            TestConvertClass classDst;
            DataSerializer.Serialize(packer, classSrc);
            packer.Position = 0;
            classDst = DataSerializer.Deserialize<TestConvertClass>(packer);

            if (!classSrc.Equals(classDst)) throw new InvalidProgramException("TestConvertClass");


            //array primitive
            packer = new BytePacker(buffer);
            byte[] bArraySrc = {1,2,3,4,5 };
            byte[] bArrayDst = null;
            DataSerializer.Serialize(packer, bArraySrc);
            packer.Position = 0;
            bArrayDst = DataSerializer.Deserialize<byte[]>(packer);

            if(bArraySrc.Length != bArrayDst.Length) throw new InvalidProgramException("byte[]");
            for (int i = 0; i < bArraySrc.Length; i++)
            {
                if(bArraySrc[i] != bArrayDst[i])
                throw new InvalidProgramException("byte[]");
            }

            //array class
            packer = new BytePacker(buffer);
            TestConvertClass[] cArraySrc = { new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass() };
            cArraySrc[1].IntData = 90;
            TestConvertClass[] cArrayDst = null;
            DataSerializer.Serialize(packer, cArraySrc);
            packer.Position = 0;
            cArrayDst = DataSerializer.Deserialize<TestConvertClass[]>(packer);

            if (cArraySrc.Length != cArrayDst.Length) throw new InvalidProgramException("TestConvertClass[]");
            for (int i = 0; i < bArraySrc.Length; i++)
            {
                if (!cArraySrc[i].Equals(cArrayDst[i]))
                    throw new InvalidProgramException("TestConvertClass[]");
            }

            //list class
            packer = new BytePacker(buffer);
            List<TestConvertClass> cListSrc = new List<TestConvertClass>{ new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass(), new TestConvertClass() };
            cListSrc[1].IntData = 90;
            List<TestConvertClass> cListDst = null;
            DataSerializer.Serialize(packer, cArraySrc);
            packer.Position = 0;
            cListDst = DataSerializer.Deserialize<List<TestConvertClass>>(packer);

            if (cListSrc.Count != cListDst.Count) throw new InvalidProgramException("List<TestConvertClass>");
            for (int i = 0; i < bArraySrc.Length; i++)
            {
                if (!cArraySrc[i].Equals(cListDst[i]))
                    throw new InvalidProgramException("List<TestConvertClass>");
            }

            //Dictionary class
            packer = new BytePacker(buffer);
            Dictionary<string, TestConvertClass> cDictSrc = new Dictionary<string, TestConvertClass>();

            TestConvertClass tcc = new TestConvertClass();
            tcc.IntData = 90;
            cDictSrc.Add("aaaa", tcc);
            tcc = new TestConvertClass();
            tcc.IntData = 270;
            cDictSrc.Add("bbbb", tcc);

            Dictionary<string, TestConvertClass> cDictDst = null;

            DataSerializer.Serialize(packer, cDictSrc);
            packer.Position = 0;
            cDictDst = DataSerializer.Deserialize<Dictionary<string, TestConvertClass>>(packer);

            if (cDictSrc.Count != cDictDst.Count) throw new InvalidProgramException("Dictionary<string, TestConvertClass>");
            foreach(var pair in cDictSrc)
            {
                if (!pair.Value.Equals(cDictDst[pair.Key]))
                    throw new InvalidProgramException("Dictionary<string, TestConvertClass>");
            }


        }


    }
}
