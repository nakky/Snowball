using System;
using System.IO;
using System.Collections.Generic;
using System.Net;

using K4os.Compression.LZ4;

namespace Snowball
{
    public interface IDataChannel
    {
        short ChannelID { get; }
        QosType Qos { get; }
        Compression Compression { get; }

        void Received(ComNode node, object data);
        
        object FromStream(ref MemoryStream stream);
        void ToStream(object data, ref MemoryStream stream);
    }

    public class DataChannel<T> : IDataChannel
    {
        public short ChannelID { get; private set; }
        public QosType Qos { get; private set; }
        public Compression Compression { get; private set; }

        public delegate void ReceivedHandler(ComNode node, T data);

        public ReceivedHandler OnReceived { get; private set; }

        Converter converter;
        Converter lz4converter;

        public DataChannel(short channelID, QosType qos, Compression compression, ReceivedHandler onReceived)
        {
            ChannelID = channelID;
            Qos = qos;
            Compression = Compression;
            OnReceived += onReceived;

            converter = DataSerializer.GetConverter(typeof(T));
            lz4converter = DataSerializer.GetConverter(typeof(byte[]));
        }

        public object FromStream(ref MemoryStream stream)
        {
            if (Compression == Compression.LZ4)
            {
                byte[] encoded = (byte[])lz4converter.Deserialize(stream);
                MemoryStream lz4stream = new MemoryStream(encoded);
                return converter.Deserialize(lz4stream);
            }
            else
            {
                return DataSerializer.Deserialize<T>(stream);
            }
        }

        public void ToStream(object data, ref MemoryStream stream)
        {
            if (Compression == Compression.LZ4)
            {
                MemoryStream lz4stream = new MemoryStream();
                lz4converter.Serialize(lz4stream, data);
                byte[] encoded = LZ4Pickler.Pickle(lz4stream.GetBuffer());
                converter.Serialize(stream, encoded);
            }
            else
            {
                DataSerializer.Serialize<T>(stream, (T)data);
            }

        }

        public void Received(ComNode node, object data)
        {
            if (OnReceived != null) OnReceived(node, (T)data);
        }

    }

}



