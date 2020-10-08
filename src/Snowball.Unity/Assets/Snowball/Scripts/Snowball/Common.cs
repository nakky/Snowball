using System.Collections;
using System.Collections.Generic;

using System.Threading;

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

    public enum Encryption
    {
        None,
        Aes,
        Rsa
    }

    public enum CheckMode
    {
        Sequre,
        Speedy
    };

    public enum PreservedChannelId
    {
        Beacon = -1,
        IssueId = -2,
        Health = -3,
        UdpNotify = -4,
        UdpNotifyAck = -5,
        KeyExchange = -6,
        KeyExchangeAck = -7,
        //User can use 0 - 32767
    }

    [Transferable]
    public class IssueIdData
    {
        public IssueIdData() { }

        [Data(0)]
        public int Id { get; set; }
        [Data(1)]
        public byte[] encryptionData { get; set; }
        [Data(2)]
        public string PublicKey { get; set; }
    }

    public static class Global
	{
		public static SynchronizationContext SyncContext { get; set; }
		public static bool UseSyncContextPost = true;
        public static byte[] ReconnectRawData = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        public static byte[] UdpRawData = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
    }

}
