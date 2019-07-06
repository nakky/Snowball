using System;

using Snowball;

namespace Snowball.Tests
{
    public enum ChannelId
    {
        ByteRel = 0,
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
    };


    [Transferable]
    public class TestClass
    {
        [Data(0)]
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
