using System;
using System.IO;
using System.Collections.Generic;
using System.Net;

using MessagePack;

namespace Snowball
{
    public interface IDataChannel
    {
        short ChannelID { get; }
        QosType Qos { get; }
        CompressionType Compression { get; }

        void Received(ComNode node, object data);
        
        object FromStream(ref MemoryStream stream);
        void ToStream(object data, ref MemoryStream stream);
    }

    public class DataChannel<T> : IDataChannel
    {
        public short ChannelID { get; private set; }
        public QosType Qos { get; private set; }
        public CompressionType Compression { get; private set; }

        public delegate void ReceivedHandler(ComNode node, T data);

        public ReceivedHandler OnReceived { get; private set; }

        public DataChannel(short channelID, QosType qos, CompressionType compression, ReceivedHandler onReceived)
        {
            ChannelID = channelID;
            Qos = qos;
            Compression = Compression;
            OnReceived += onReceived;
        }

        public object FromStream(ref MemoryStream stream)
        {
            if (Compression == CompressionType.LZ4)
            {
                return LZ4MessagePackSerializer.Deserialize<T>(stream);
            }
            else
            {
                return MessagePackSerializer.Deserialize<T>(stream);
            }
        }

        public void ToStream(object data, ref MemoryStream stream)
        {
            if (Compression == CompressionType.LZ4)
            {
                LZ4MessagePackSerializer.Serialize<T>(stream, (T)data);
            }
            else
            {
                MessagePackSerializer.Serialize<T>(stream, (T)data);
            }

        }

        public void Received(ComNode node, object data)
        {
            if (OnReceived != null) OnReceived(node, (T)data);
        }

    }

}



