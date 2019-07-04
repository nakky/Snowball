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
        public int intData;
        [Data(1)]
        public float floatData;
        [Data(2)]
        public string stringData;

        public override string ToString()
        {
            return "{" + intData + "," + floatData + "," + stringData + "}";
        }

    };
    
}
