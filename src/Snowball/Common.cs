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

    public enum CompressionType
    {
        None,
        LZ4,
    }

    public enum PreservedChannelId
    {
        Beacon = 30001,
        Login = 30002,
        Health = 30003,
        //User can use 1 - 29999
    }

    public struct BroadcastInfo
    {
        public BroadcastInfo(byte measure, byte minor)
        {
            this.Measure = measure;
            this.Minor = minor;
        }

        public byte Measure;
        public byte Minor;
    }

    /*
    [MessagePackObject]
    public class DataContainer
    {
        public DataContainer(object data)
        {
            Data = data;
        }

        [Key(0)]
        public object Data;
    }
    */

}
