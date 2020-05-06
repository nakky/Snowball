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
        
        object FromStream(ref BytePacker packer);
        void ToStream(object data, ref BytePacker packer);

        int GetDataSize(object data);
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

        public object FromStream(ref BytePacker packer)
        {
            if (Compression == Compression.LZ4)
            {
                byte[] encoded = (byte[])lz4converter.Deserialize(packer);
                BytePacker lz4packer = new BytePacker(encoded);
                return converter.Deserialize(lz4packer);
            }
            else
            {
                return converter.Deserialize(packer);
            }
        }

        public void ToStream(object data, ref BytePacker packer)
        {
            if (Compression == Compression.LZ4)
            {
                int size = lz4converter.GetDataSize(data);
                byte[] buf = new byte[size];
                BytePacker lz4packer = new BytePacker(buf);
                lz4converter.Serialize(lz4packer, data);
                byte[] encoded = LZ4Pickler.Pickle(buf);
                converter.Serialize(packer, encoded);
            }
            else
            {
                int start = packer.Position;

                converter.Serialize(packer, data);
            }

        }

        public int GetDataSize(object data)
        {
            return converter.GetDataSize(data);
        }

        public void Received(ComNode node, object data)
        {
            if (OnReceived != null) OnReceived(node, (T)data);
        }

    }

    public class RawDataChannel<T> : IDataChannel
    {
        public short ChannelID { get; private set; }
        public QosType Qos { get; private set; }
        public Compression Compression { get; private set; }

        public delegate void ReceivedHandler(ComNode node, byte[] data);

        public ReceivedHandler OnReceived { get; private set; }

        Converter converter;
        Converter lz4converter;

        public RawDataChannel(short channelID, QosType qos, Compression compression, ReceivedHandler onReceived)
        {
            ChannelID = channelID;
            Qos = qos;
            Compression = Compression;
            OnReceived += onReceived;

            converter = DataSerializer.GetConverter(typeof(T));
            lz4converter = DataSerializer.GetConverter(typeof(byte[]));
        }

        public object FromStream(ref BytePacker packer)
        {
            int head = packer.Position;
            int length = converter.GetDataSize(packer);
            byte[] data = new byte[length];

            Array.Copy(packer.Buffer, head, data, 0, length);

            return data;
        }

        public void ToStream(object data, ref BytePacker packer)
        {
            byte[] arr = (byte[])data;
            packer.Write(arr, 0, arr.Length);
        }

        public int GetDataSize(object data)
        {
            BytePacker packer = new BytePacker((byte[])data);
            return converter.GetDataSize(packer);
        }

        public void Received(ComNode node, object data)
        {
            if (OnReceived != null) OnReceived(node, (byte[])data);
        }

    }

}



