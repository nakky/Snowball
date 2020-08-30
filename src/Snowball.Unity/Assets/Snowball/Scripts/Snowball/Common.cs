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

    public enum CheckMode
    {
        Sequre,
        Speedy
    };

    public enum PreservedChannelId
    {
        Beacon = -1,
        Login = -2,
        Health = -3,
        UdpNotify = -4,
        UdpNotifyAck = -5,
        //User can use 0 - 32767
    }

    public static class Global
	{
		public static SynchronizationContext SyncContext { get; set; }
		public static bool UseSyncContextPost = true;
	}

}
