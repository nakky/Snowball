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
        Encryption Encryption { get; }

        void Received(ComNode node, object data);

        CheckMode CheckMode { get; }

        object FromStream(ref BytePacker packer, IDecrypter decrypter = null);
        void ToStream(object data, ref BytePacker packer, IEncrypter encrypter = null);

        int GetDataSize(object data);
    }

    public class DataChannel<T> : IDataChannel
    {
        public short ChannelID { get; private set; }
        public QosType Qos { get; private set; }
        public Compression Compression { get; private set; }
        public Encryption Encryption { get; private set; }

        public delegate void ReceivedHandler(ComNode node, T data);

        public ReceivedHandler OnReceived { get; private set; }

        public CheckMode CheckMode { get; private set; }

        IConverter converter;
        IConverter byteArrayConverter;
        IConverter encArrayConverter;

        public DataChannel(
            short channelID,
            QosType qos,
            Compression compression,
            Encryption encryption,
            ReceivedHandler onReceived,
            CheckMode checkMode = CheckMode.Sequre
            )
        {
            ChannelID = channelID;
            Qos = qos;
            Compression = compression;
            Encryption = encryption;

            OnReceived += onReceived;

            CheckMode = checkMode;

            converter = DataSerializer.GetConverter(typeof(T));

            if (Compression == Compression.LZ4)
            {
                byteArrayConverter = DataSerializer.GetConverter(typeof(byte[]));
            }

            if (Encryption == Encryption.Rsa || Encryption == Encryption.Aes)
            {
                encArrayConverter = DataSerializer.GetConverter(typeof(byte[]));
            }

        }

        public object FromStream(ref BytePacker packer, IDecrypter decrypter)
        {
            BytePacker p = packer;

            if (Compression == Compression.LZ4)
            {
                byte[] encoded = (byte[])byteArrayConverter.Deserialize(p);
                byte[] data = LZ4Pickler.Unpickle(encoded);

                if(decrypter != null)
                {
                    data = decrypter.Decrypt(data);
                }

                p = new BytePacker(data);
            }
            else if(decrypter != null)
            {
                try
                {
                    byte[] data = (byte[])encArrayConverter.Deserialize(p);
                    data = decrypter.Decrypt(data);
                    p = new BytePacker(data);
                }
                catch(Exception e)
                {
                    Util.Log("FromStream:" + e.Message);
                }

            }

            return converter.Deserialize(p);
        }

        public void ToStream(object data, ref BytePacker packer, IEncrypter encrypter)
        {
            if (Compression == Compression.LZ4)
            {
                int size = converter.GetDataSize(data);
                byte[] buf = new byte[size];
                BytePacker lz4packer = new BytePacker(buf);
                converter.Serialize(lz4packer, data);

                //Encrypt
                if(encrypter != null)
                {
                    buf = encrypter.Encrypt(buf);
                }

                byte[] encoded = LZ4Pickler.Pickle(buf);
                byteArrayConverter.Serialize(packer, encoded);
            }
            else
            {
                //Encrypt
                if (encrypter != null)
                {
                    int size = converter.GetDataSize(data);
                    byte[] buf = new byte[size];
                    BytePacker encpacker = new BytePacker(buf);
                    converter.Serialize(encpacker, data);

                    buf = encrypter.Encrypt(buf);
                    encArrayConverter.Serialize(packer, buf);
                }
                else converter.Serialize(packer, data);
            }

        }

        public int GetDataSize(object data)
        {
            int size = converter.GetDataSize(data);
            if(Encryption == Encryption.Rsa)
            {
                if (size < 128) return 128;
            }
            if (Encryption == Encryption.Aes)
            {
                int s = 16;
                while(size >= s) s *= 2;
                return s;
            }
            return size;
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
        public Encryption Encryption { get; private set; }

        public delegate void ReceivedHandler(ComNode node, byte[] data);

        public ReceivedHandler OnReceived { get; private set; }

        public CheckMode CheckMode { get; private set; }

        IConverter converter;
        IConverter byteArrayConverter;

        public RawDataChannel(
            short channelID, QosType qos, Compression compression, Encryption encryption, ReceivedHandler onReceived, CheckMode checkMode = CheckMode.Sequre
            )
        {
            ChannelID = channelID;
            Qos = qos;
            Compression = compression;
            Encryption = encryption;

            OnReceived += onReceived;

            CheckMode = checkMode;

            converter = DataSerializer.GetConverter(typeof(T));

            if (Compression == Compression.LZ4)
            {
                byteArrayConverter = DataSerializer.GetConverter(typeof(byte[]));
            }
        }

        public object FromStream(ref BytePacker packer, IDecrypter decrypter)
        {
            if (Compression == Compression.LZ4)
            {
                byte[] encoded = (byte[])byteArrayConverter.Deserialize(packer);
                return encoded;
            }
            else
            {
                int head = packer.Position;
                int length = converter.GetDataSize(packer);
                byte[] data = new byte[length];

                Array.Copy(packer.Buffer, head, data, 0, length);

                return data;
            }

        }

        public void ToStream(object data, ref BytePacker packer, IEncrypter encrypter)
        {
            if (Compression == Compression.LZ4)
            {
                byteArrayConverter.Serialize(packer, data);
            }
            else
            {
                byte[] arr = (byte[])data;
                packer.WriteByteArray(arr, 0, arr.Length);
            }
        }


        public int GetDataSize(object data)
        {
            if (Compression == Compression.LZ4)
            {
                return byteArrayConverter.GetDataSize(data);
            }
            else
            {
                BytePacker packer = new BytePacker((byte[])data);
                return converter.GetDataSize(packer);
            }
        }


        public void Received(ComNode node, object data)
        {
            if (OnReceived != null) OnReceived(node, (byte[])data);
        }

    }

}



