using System;
using System.Collections.Generic;
using System.Net;

namespace Snowball
{

    public class DataChannel
    {
        public short ChannelID { get; private set; }
        public QosType Qos { get; private set; }
        public CompressionType Compression { get; private set; }

        public delegate void ReceivedHandler(ComNode node, object data);

        public event ReceivedHandler OnReceived;

        public DataChannel(short channelID, QosType qos, CompressionType compression, ReceivedHandler onReceived)
        {
            ChannelID = channelID;
            Qos = qos;
            Compression = Compression;
            OnReceived += onReceived;
        }

        public void Received(ComNode node, object data)
        {
            if (OnReceived != null) OnReceived(node, data);
        }

    }

}



