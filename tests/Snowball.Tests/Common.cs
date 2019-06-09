using System;
using MessagePack;

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


    [MessagePackObject]
    public class TestClass
    {
        [Key(0)]
        public int intData;
        [Key(1)]
        public float floatData;
        [Key(2)]
        public string stringData;

        public override string ToString()
        {
            return "{" + intData + "," + floatData + "," + stringData + "}";
        }
    };
    
}
