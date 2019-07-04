using System.Collections;
using System.Collections.Generic;

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
        Beacon = -1,
        Login = -2,
        Health = -3,
        //User can use 0 - 32767
    }

}
