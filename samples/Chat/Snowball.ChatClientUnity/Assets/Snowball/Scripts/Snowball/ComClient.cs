//#define DISABLE_CHANNEL_VARINT

using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public class ComClient : IDisposable
    {
        public bool IsOpened { get; protected set; }

        public bool acceptBeacon = false;
        public bool AcceptBeacon { get { return acceptBeacon; } set { acceptBeacon = value; } }

        string userName = "u001";
        public string UserName { get { return userName; } set { userName = value; } }

        int listenPortNumber = 59902;
        public int ListenPortNumber { get { return listenPortNumber; } set { if (!IsOpened) listenPortNumber = value; } }

        int sendPortNumber = 59901;
        public int SendPortNumber { get { return sendPortNumber; } set { if (!IsOpened) sendPortNumber = value; } }

        int bufferSize = 8192;
        public int BufferSize { get { return bufferSize; } set { if (!IsOpened) bufferSize = value; } }

        public delegate void ConnectedHandler(ComNode node);
        public ConnectedHandler OnConnected;

        public delegate void DisconnectedHandler(ComNode node);
        public DisconnectedHandler OnDisconnected;

        protected Dictionary<short, IDataChannel> dataChannelMap = new Dictionary<short, IDataChannel>();

        public delegate bool BeaconAcceptFunc(string data);
        BeaconAcceptFunc BeaconAccept = (data) => { if (data == "Snowball") return true; else return false; };

        public void SetBeaconAcceptFunction(BeaconAcceptFunc func) { BeaconAccept = func; }

        ComNode serverNode;

        UDPSender udpSender;
        UDPReceiver udpReceiver;

        TCPConnector tcpConnector;

        int healthLostCount = 0;

        int maxHealthLostCount = 5;
        public int MaxHealthLostCount { get { return maxHealthLostCount; } set { maxHealthLostCount = value; } }

        bool isConnecting = false;

        public bool IsConnected { get{ lock (this) { return serverNode != null; } } }

        public ComClient()
        {
            IsOpened = false;

			if (Global.UseSyncContextPost && Global.SyncContext == null)
				Global.SyncContext = SynchronizationContext.Current;

			AddChannel(new DataChannel<string>((short)PreservedChannelId.Login, QosType.Reliable, Compression.None, (endPointIp, deserializer) =>
            {
            }));

            AddChannel(new DataChannel<byte>((short)PreservedChannelId.Health, QosType.Unreliable, Compression.None, (endPointIp, data) =>{}));
		}

        public void Dispose()
        {
            Close();
        }

        public virtual void Open()
        {
            if (IsOpened) return;

            udpSender = new UDPSender(sendPortNumber, bufferSize);
            udpReceiver = new UDPReceiver(listenPortNumber, bufferSize);

            tcpConnector = new TCPConnector(sendPortNumber);
            tcpConnector.ConnectionBufferSize = bufferSize;
            tcpConnector.OnConnected += OnConnectedInternal;

            udpReceiver.OnReceive += OnUDPReceived;

            udpReceiver.Start();

            IsOpened = true;

            HealthCheck();
        }

        public virtual void Close()
        {
            if (!IsOpened) return;

            Disconnect();

            udpReceiver.Close();

            tcpConnector = null;
            udpReceiver = null;
            udpSender = null;

            IsOpened = false;
        }

        public async Task HealthCheck()
        {
            healthLostCount = 0;

            while (IsOpened)
            {
                await Task.Delay(500);

                if(IsConnected)
                {
                    byte dummy = 0;

                    Send((short)PreservedChannelId.Health, dummy);

                    healthLostCount++;
                    if (healthLostCount > MaxHealthLostCount)
                    {
                        Disconnect();
                        break;
                    }
                }

            }
        }

        public void AddChannel(IDataChannel channel)
        {
            dataChannelMap.Add(channel.ChannelID, channel);
        }

        public void RemoveChannel(IDataChannel channel)
        {
            dataChannelMap.Remove(channel.ChannelID);
        }

        public bool Connect(string ip)
        {
            if (!isConnecting && !IsConnected)
            {
                isConnecting = true;
                tcpConnector.Connect(ip);
                return true;
            }
            return false;
        }

        void OnConnectedInternal(string ip, TCPConnection connection)
        {
            if (connection != null)
            {
                serverNode = new ComNode(connection);

                connection.OnDisconnected = OnDisconnectedInternal;
                connection.OnReceive = OnTCPReceived;

                Send((short)PreservedChannelId.Login, UserName);
                if (OnConnected != null) OnConnected(serverNode);

                Util.Log("Client:Connected");
            }

            isConnecting = false;
        }

        public bool Disconnect()
        {
            if (serverNode != null)
            {
                serverNode.Connection.Disconnect();
                return true;
            }
            else return false;
        }

        void OnDisconnectedInternal(TCPConnection connection)
        {
       
            if (OnDisconnected != null) OnDisconnected(serverNode);

            serverNode = null;

            Util.Log("Client:Disconnected");
        }

        void OnUDPReceived(string endPointIp, byte[] data, int size)
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
                short channelId = VarintBitConverter.ToInt16(packer, out s);
#endif

                if (channelId == (short)PreservedChannelId.Beacon)
                {
                    if (acceptBeacon && !isConnecting && !IsConnected)
                    {
                        string beaconData = (string)DataSerializer.Deserialize<string>(packer);

                        if(BeaconAccept(beaconData)) Connect(endPointIp);
                    }
                }
                else if (!dataChannelMap.ContainsKey(channelId))
                {
                }
                else
                {
                    if (serverNode == null) break;
                    if (endPointIp == serverNode.IP)
                    {
                        healthLostCount = 0;

                        IDataChannel channel = dataChannelMap[channelId];
                        object container = channel.FromStream(ref packer);

                        channel.Received(serverNode, container);
                    }
                }

                head += datasize + 4;

            }

          
        }

        void OnTCPReceived(string endPointIp, short channelId, byte[] data, int size)
        {
            if (channelId == (short)PreservedChannelId.Beacon)
            {
            }
            else if (!dataChannelMap.ContainsKey(channelId))
            {
            }
            else
            {
                BytePacker packer = new BytePacker(data);

                if (serverNode == null);
                if (endPointIp == serverNode.IP)
                {
                    healthLostCount = 0;

                    IDataChannel channel = dataChannelMap[channelId];
                    object container = channel.FromStream(ref packer);

                    channel.Received(serverNode, container);
                }
            }
        }

        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

        public async Task<bool> Send<T>(short channelId, T data)
        {
            if (!IsConnected) return false;

            if (!dataChannelMap.ContainsKey(channelId)) return false;

            IDataChannel channel = dataChannelMap[channelId];

            bool isRent = true;
            int bufSize = channel.GetDataSize(data);
            byte[] buf = arrayPool.Rent(bufSize + 6);
            if (buf == null)
            {
                isRent = false;
                buf = new byte[bufSize + 6];
            }

            BytePacker packer = new BytePacker(buf);
            packer.Write((short)bufSize);

#if DISABLE_CHANNEL_VARINT
            packer.Write(channelId);
#else
            int s = 0;
            VarintBitConverter.SerializeShort(channelId, packer, out s);
#endif
            channel.ToStream(data, ref packer);

            int maxpos = (int)packer.Position;

            if (channel.Qos == QosType.Reliable)
            {
                await serverNode.Connection.Send(maxpos, buf);
            }
            else if (channel.Qos == QosType.Unreliable)
            {
                await udpSender.Send(serverNode.IP, maxpos, buf);
            }

            if (isRent) arrayPool.Return(buf);

            return true;
        }
    }

}
