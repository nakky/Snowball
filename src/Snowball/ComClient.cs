//#define DISABLE_CHANNEL_VARINT

using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Snowball
{
    public class ComClient : IDisposable
    {
        public bool IsOpened { get; protected set; }

        public bool acceptBeacon = false;
        public bool AcceptBeacon {
            get {
                return acceptBeacon;
            }
            set {
                if (value)
                {
                    if(udpBeaconReceiver == null)
                    {
                        udpBeaconReceiver = new UDPReceiver(beaconPortNumber);
                        udpBeaconReceiver.OnReceive += OnUDPReceived;
                        udpBeaconReceiver.Start();
                    }
                }
                else
                {
                    if(udpBeaconReceiver != null)
                    {
                        udpBeaconReceiver.Close();
                        udpBeaconReceiver = null;
                    }
                }
                acceptBeacon = value;
            }
        }

        string userName = "u001";
        public string UserName { get { return userName; } set { userName = value; } }

        int beaconPortNumber = 32000;
        public int BeaconPortNumber { get { return beaconPortNumber; } set { if (!IsOpened) beaconPortNumber = value; } }

        int listenPortNumber = 32002;
        public int ListenPortNumber { get { return listenPortNumber; } set { if (!IsOpened) listenPortNumber = value; } }

        int sendPortNumber = 32001;
        public int SendPortNumber { get { return sendPortNumber; } set { if (!IsOpened) sendPortNumber = value; } }

        int bufferSize = 8192;
        public int BufferSize { get { return bufferSize; } set { if (!IsOpened) bufferSize = value; } }

        int connectTimeOutMilliSec = 1000;
        public int ConnectTimeOutMilliSec { get { return connectTimeOutMilliSec; } set { connectTimeOutMilliSec = value; } }

        public delegate void ConnectedHandler(ComNode node);
        public ConnectedHandler OnConnected;

        public delegate void ConnectedErrorHandler(string ip);
        public ConnectedErrorHandler OnConnectError;

        public delegate void DisconnectedHandler(ComNode node);
        public DisconnectedHandler OnDisconnected;

        protected Dictionary<short, IDataChannel> dataChannelMap = new Dictionary<short, IDataChannel>();

        public delegate bool BeaconAcceptFunc(string data);
        BeaconAcceptFunc BeaconAccept = (data) => { if (data == "Snowball") return true; else return false; };

        public void SetBeaconAcceptFunction(BeaconAcceptFunc func) { BeaconAccept = func; }

        ComNode serverNode;

        TCPConnector tcpConnector;

        UDPTerminal udpTerminal;
        UDPReceiver udpBeaconReceiver;


        int healthLostCount = 0;
        bool udpAck = true;

        int maxHealthLostCount = 5;
        public int MaxHealthLostCount { get { return maxHealthLostCount; } set { maxHealthLostCount = value; } }

        bool isConnecting = false;

        public bool IsConnected { get { lock (this) { return (serverNode != null); } } }

        public ComClient()
        {
            IsOpened = false;

            AddChannel(new DataChannel<string>((short)PreservedChannelId.Login, QosType.Reliable, Compression.None, (node, data) =>
            {
                udpAck = false;
            }));

            AddChannel(new DataChannel<byte[]>((short)PreservedChannelId.Health, QosType.Unreliable, Compression.None, (node, data) =>
            {
                //Util.Log("Health");
                healthLostCount = 0;
                byte[] encrypted = EncrypteTmpKey(data);
                Send((short)PreservedChannelId.Health, encrypted);
            }));

            AddChannel(new DataChannel<string>((short)PreservedChannelId.UdpNotify, QosType.Unreliable, Compression.None, (node, data) =>
            {
            }));

            AddChannel(new DataChannel<int>((short)PreservedChannelId.UdpNotifyAck, QosType.Reliable, Compression.None, (node, data) =>
            {
                udpAck = true;
                if (OnConnected != null) OnConnected(serverNode);
            }));
        }

        public void Dispose()
        {
            AcceptBeacon = false;
            Close();
        }

        public virtual void Open()
        {
            if (IsOpened) return;

            if (Global.UseSyncContextPost && Global.SyncContext == null)
                Global.SyncContext = SynchronizationContext.Current;

            udpTerminal = new UDPTerminal(listenPortNumber, bufferSize);
            udpTerminal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpTerminal.OnReceive += OnUDPReceived;

            tcpConnector = new TCPConnector(sendPortNumber);
            tcpConnector.ConnectionBufferSize = bufferSize;
            tcpConnector.ConnectTimeOutMilliSec = connectTimeOutMilliSec;
            tcpConnector.OnConnected += OnConnectedInternal;

            IsOpened = true;

            HealthCheck();
            UdpCheck();
        }

        public virtual void Close()
        {
            if (!IsOpened) return;

            Disconnect();

            udpTerminal.Close();

            tcpConnector = null;
            udpTerminal = null;

            IsOpened = false;
        }

        public virtual byte[] EncrypteTmpKey(byte[] decrypted)
        {
            return decrypted;
        }

        public async Task UdpCheck()
        {
            healthLostCount = 0;

            while (IsOpened)
            {
                try
                {
                    await Task.Delay(100);

                    if (IsConnected)
                    {
                        if (!udpAck)
                        {
                            Send((short)PreservedChannelId.UdpNotify, UserName);
                        }

                    }
                }
                catch//(Exception e)
                {
                    //Util.Log("Health:" + e.Message);
                }
            }
        }


        public async Task HealthCheck()
        {
            healthLostCount = 0;

            while (IsOpened)
            {
                try
                {
                    await Task.Delay(500);

                    if (IsConnected)
                    {
                        healthLostCount++;
                        if (healthLostCount > MaxHealthLostCount)
                        {
                            //Util.Log("Client:Disconnect##########");
                            Disconnect();
                        }
                    }
                }
                catch//(Exception e)
                {
                    //Util.Log("Health:" + e.Message);
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
            if (!isConnecting && !IsConnected && tcpConnector != null)
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
                connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                serverNode = new ComSnowballNode(connection);

                udpTerminal.ReceiveStart();

                connection.OnDisconnected = OnDisconnectedInternal;
                connection.OnPoll = OnPoll;

                Send((short)PreservedChannelId.Login, UserName);
                Send((short)PreservedChannelId.UdpNotify, UserName);

                healthLostCount = 0;

                //Util.Log("Client:Connected");

                isConnecting = false;
            }
            else
            {
                isConnecting = false;
                OnConnectError(ip);
            }
        }

        public bool Disconnect()
        {
            if (serverNode != null)
            {
                ((ComSnowballNode)serverNode).Connection.Disconnect();
                return true;
            }
            else return false;
        }

        void OnDisconnectedInternal(TCPConnection connection)
        {
            if (OnDisconnected != null) OnDisconnected(serverNode);

            serverNode = null;
            if(udpTerminal != null) udpTerminal.ReceiveStop();

            //Util.Log("Client:Disconnected");
        }

        void OnUDPReceived(IPEndPoint endPoint, byte[] data, int size)
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
                if (channelId == (short)PreservedChannelId.Beacon)
                {
                    if (acceptBeacon && !isConnecting && !IsConnected)
                    {
                        string beaconData = (string)DataSerializer.Deserialize<string>(packer);

                        if (BeaconAccept(beaconData)) Connect(endPoint.Address.ToString());
                    }
                }
                else if (!dataChannelMap.ContainsKey(channelId))
                {
                }
                else
                {
                    if (channelId == (short)PreservedChannelId.Health)
                    {
                        if (serverNode == null) break;

                        if (serverNode.UdpEndPoint == null && serverNode.TcpEndPoint.Address.Equals(endPoint.Address))
                        {
                            serverNode.UdpEndPoint = endPoint;
                        }
                    }

                    IDataChannel channel = dataChannelMap[channelId];

                    if(channel.CheckMode == CheckMode.Sequre)
                    {
                        if (serverNode == null) break;
                        if (endPoint.Address.Equals(serverNode.UdpEndPoint.Address))
                        {
                            object container = channel.FromStream(ref packer);

                            channel.Received(serverNode, container);
                        }
                    }
                    else
                    {
                        object container = channel.FromStream(ref packer);

                        channel.Received(serverNode, container);
                    }
                }

                head += datasize + 4;

            }


        }

        void OnTCPReceived(IPEndPoint endPoint, short channelId, byte[] data, int size)
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
                IDataChannel channel = dataChannelMap[channelId];

                if (channel.CheckMode == CheckMode.Sequre)
                {
                    if (serverNode == null) ;
                    if (endPoint.Equals(serverNode.TcpEndPoint))
                    {
                        object container = channel.FromStream(ref packer);

                        channel.Received(serverNode, container);
                    }
                }
                else
                {
                    object container = channel.FromStream(ref packer);

                    channel.Received(null, container);
                }
            }
        }

        public class CallbackParam
        {
            public CallbackParam(IPEndPoint endPoint, short channelId, byte[] buffer, int size, bool isRent)
            {
                this.endPoint = endPoint; this.channelId = channelId; this.buffer = buffer; this.size = size; this.isRent = isRent;
            }
            public IPEndPoint endPoint;
            public short channelId;
            public byte[] buffer;
            public int size;
            public bool isRent;
        }


        public async Task<bool> OnPoll(
            TCPConnection connection,
            NetworkStream nStream,
            byte[] receiveBuffer,
            BytePacker receivePacker,
            CancellationTokenSource cancelToken
            )
        {
            int resSize = 0;
            short channelId = 0;

            bool isRent = false;
            byte[] buffer = null;

            try
            {
                resSize = await nStream.ReadAsync(receiveBuffer, 0, 2, cancelToken.Token).ConfigureAwait(false);

                if (resSize != 0)
                {
                    receivePacker.Position = 0;
                    resSize = receivePacker.ReadShort();
#if DISABLE_CHANNEL_VARINT
                        await nStream.ReadAsync(receiveBuffer, 0, 2, cancelToken.Token).ConfigureAwait(false);
                        receivePacker.Position = 0;
                        channelId = receivePacker.ReadShort();
                        await nStream.ReadAsync(receiveBuffer, 0, resSize, cancelToken.Token).ConfigureAwait(false);
#else
                    int s = 0;
                    channelId = VarintBitConverter.ToShort(nStream, out s);
                    await nStream.ReadAsync(receiveBuffer, 0, resSize, cancelToken.Token).ConfigureAwait(false);
#endif


                    buffer = arrayPool.Rent(resSize);
                    if (buffer != null)
                    {
                        isRent = true;
                    }
                    else
                    {
                        buffer = new byte[resSize];
                        isRent = false;
                    }

                    Array.Copy(receiveBuffer, buffer, resSize);

                    //Util.Log("TCP:" + resSize);
                }
            }
            catch//(Exception e)
            {
                //Util.Log("TCP:" + e.Message);
                return false;
            }

            if (resSize == 0)
            {
                return false;
            }

            if (cancelToken.IsCancellationRequested) return false;

            if (Global.SyncContext != null)
            {
                Global.SyncContext.Post((state) =>
                {
                    if (cancelToken.IsCancellationRequested) return;
                    CallbackParam param = (CallbackParam)state;
                    OnTCPReceived(param.endPoint, param.channelId, param.buffer, param.size);
                    if (isRent) arrayPool.Return(buffer);
                }, new CallbackParam((IPEndPoint)connection.Client.Client.RemoteEndPoint, channelId, buffer, resSize, isRent));
            }
            else
            {
                OnTCPReceived((IPEndPoint)connection.Client.Client.RemoteEndPoint, channelId, buffer, resSize);
            }

            return true;
        }

        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

        public void BuildBuffer<T>(IDataChannel channel, T data, ref byte[] buffer, ref int bufferSize, ref bool isRent)
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


        public async Task<bool> Send<T>(short channelId, T data)
        {
            return await Task.Run(async () =>
            {
                if (!IsConnected) return false;
                if (!dataChannelMap.ContainsKey(channelId)) return false;

                IDataChannel channel = dataChannelMap[channelId];

                bool isRent = false;
                byte[] buffer = null;
                int bufferSize = 0;

                BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent);

                if (channel.Qos == QosType.Reliable)
                {
                    await ((ComSnowballNode)serverNode).Connection.Send(bufferSize, buffer);
                }
                else if (channel.Qos == QosType.Unreliable)
                {
                    await udpTerminal.Send(serverNode.Ip, sendPortNumber, bufferSize, buffer);
                }

                if (isRent) arrayPool.Return(buffer);

                return true;
            });

        }
    }

}
