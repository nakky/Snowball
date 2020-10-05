//#define DISABLE_CHANNEL_VARINT

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public sealed class ComTerminal : IDisposable
    {
        public bool IsOpened { get; private set; }

        int listenPortNumber = 0;
        public int ListenPortNumber { get { return listenPortNumber; } set { if (!IsOpened) listenPortNumber = value; } }

        int portNumber = 32001;
        public int PortNumber { get { return portNumber; } set { if (!IsOpened) portNumber = value; } }

        int bufferSize = 8192;
        public int BufferSize { get { return bufferSize; } set { if (!IsOpened) bufferSize = value; } }

        Dictionary<short, IDataChannel> dataChannelMap = new Dictionary<short, IDataChannel>();

        UDPTerminal udpTerminal;

        Dictionary<string, ComNode> nodeUdpMap = new Dictionary<string, ComNode>();

        public ComTerminal()
        {
            IsOpened = false;
        }

        public void Dispose()
        {
            Close();
        }

        public void Open()
        {
            if (IsOpened) return;

            if (Global.UseSyncContextPost && Global.SyncContext == null)
                Global.SyncContext = SynchronizationContext.Current;

            int port = portNumber;
            if (listenPortNumber != 0) port = listenPortNumber;

            udpTerminal = new UDPTerminal(port, bufferSize);
            udpTerminal.OnReceive += OnUnreliableReceived;

            udpTerminal.ReceiveStart();

            IsOpened = true;
        }

        public void Close()
        {
            if (!IsOpened) return;

            if(udpTerminal != null)
            {
                udpTerminal.Close();
                udpTerminal = null;
            }
               
            IsOpened = false;
        }

        public void AddAcceptList(string ip)
        {
            IPAddress address = IPAddress.Parse(ip);
            ComNode node = new ComNode(new IPEndPoint(address, portNumber));
            nodeUdpMap.Add(ip, node);
        }

        public void RemoveAcceptList(string ip)
        {
            nodeUdpMap.Remove(ip);
        }

        public void AddChannel(IDataChannel channel)
        {
            if(channel.Qos != QosType.Unreliable)
            {
                throw new ArgumentException("Qos type is not valid.");
            }

            if (channel.Encryption != Encryption.None)
            {
                throw new ArgumentException("ComTerminal can not use encryption.");
            }
            dataChannelMap.Add(channel.ChannelID, channel);
        }

        public void RemoveChannel(IDataChannel channel)
        {
            dataChannelMap.Remove(channel.ChannelID);
        }

        void OnUnreliableReceived(IPEndPoint endPoint, byte[] data, int size)
        {
            int head = 0;

            while (head < size)
            {
                BytePacker packer = new BytePacker(data);
                short datasize = packer.ReadShort();
#if DISABLE_CHANNEL_VARINT
                short channelId = packer.ReadShort();
#else
                int s = 0;
                short channelId = VarintBitConverter.ToShort(packer, out s);
#endif
                if (!dataChannelMap.ContainsKey(channelId))
                {
                }
                else
                {
                    IDataChannel channel = dataChannelMap[channelId];

                    if (channel.CheckMode == CheckMode.Sequre)
                    {
                        if (nodeUdpMap.ContainsKey(endPoint.Address.ToString()))
                        {
                            ComNode node = nodeUdpMap[endPoint.Address.ToString()];

                            object container = channel.FromStream(ref packer, null);

                            channel.Received(node, container);
                        }
                    }

                    else
                    {
                        object container = channel.FromStream(ref packer, null);
                        channel.Received(null, container);
                    }
 
                }

                head += datasize + 4;

            }
        }

        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

        void BuildBuffer<T>(IDataChannel channel, T data, ref byte[] buffer, ref int bufferSize, ref bool isRent)
        {
            isRent = true;
            int bufSize = channel.GetDataSize(data);
            int lz4ext = 0;
            if (channel.Compression == Compression.LZ4) lz4ext = 4;

            buffer = arrayPool.Rent(bufSize + 6 + lz4ext);
            if (buffer == null)
            {
                isRent = false;
                buffer = new byte[bufSize + 6 + lz4ext];
            }

            BytePacker packer = new BytePacker(buffer);
            packer.Write((short)bufSize);

#if DISABLE_CHANNEL_VARINT
            packer.Write(channelId);
#else
            int s = 0;
            VarintBitConverter.SerializeShort(channel.ChannelID, packer, out s);
#endif
            int start = packer.Position;

            channel.ToStream(data, ref packer);

            bufferSize = (int)packer.Position;

            packer.Position = 0;
            packer.Write((short)(bufferSize - start));
        }


        public async Task<bool> Send<T>(ComNode node, short channelId, T data)
        {
            if (portNumber == 0) return false;

            return await Task.Run(async () => {
                if (!dataChannelMap.ContainsKey(channelId)) return false;

                IDataChannel channel = dataChannelMap[channelId];

                bool isRent = false;
                byte[] buffer = null;
                int bufferSize = 0;

                BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent);

                await udpTerminal.Send(node.Ip, portNumber, bufferSize, buffer).ConfigureAwait(false);

                if (isRent) arrayPool.Return(buffer);

                return true;
            }).ConfigureAwait(false);

        }

    }
}
