using System;

using Snowball;

namespace Snowball.Tests
{
    public enum ChannelId
    {
        BoolRel = 0,
        BoolUnRel,
        ByteRel,
        ByteUnRel,
        ShortRel,
        ShortUnRel,
        IntRel,
        IntUnRel,
        FloatRel,
        FloatUnRel,
        DoubleRel,
        DoubleUnRel,
        StringRel,
        StringUnRel,
        ClassRel,
        ClassUnRel,
        BoolRaw,
        ByteRaw,
        ShortRaw,
        IntRaw,
        FloatRaw,
        DoubleRaw,
        StringRaw,
        ClassRaw,
    };


    [Transferable]
    public class TestClass
    {
        [Data(0, typeof(VarIntConverter))]
        public int intData { get; set; }
        [Data(1)]
        public float floatData { get; set; }
        [Data(2)]
        public string stringData { get; set; }

        public override string ToString()
        {
            return "{" + intData + "," + floatData + "," + stringData + "}";
        }

    };
    
}
