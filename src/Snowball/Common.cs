using System.Collections;
using System.Collections.Generic;
using MessagePack;

namespace Snowball
{

    public enum QosType
    {
        Unreliable,
        Reliable,
    }

    public enum Compression
    {
        None,
        LZ4,
    }

    public enum PreservedChannelId
    {
        Beacon = 30001,
        Login = 30002,
        Health = 30003,
        //User can use 0 - 29999
    }

}
