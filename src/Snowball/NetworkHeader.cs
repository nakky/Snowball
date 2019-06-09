using System;
using System.IO;

namespace Com
{
    public enum PreservedTcpId
    {
        HelthCheck = -1,
    }

    public struct NetworkHeader
    {
        public const int HeaderSize = 4;

        public short id;
        public short size;

        public NetworkHeader(short id, short size)
        {
            this.id = id;
            this.size = size;
        }
    }

}
