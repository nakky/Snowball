﻿#define DISABLE_CHANNEL_VARINT

using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Snowball
{
    public sealed class ComClient : IDisposable
    {
        public bool IsOpened { get; private set; }

        bool acceptBeacon = false;
        public bool AcceptBeacon {
            get {
                return acceptBeacon;
            }
            set {
                if (value)
                {
                    if(udpBeaconReceiver == null)
                    {
                        udpBeaconReceiver = new UDPTerminal(beaconPortNumber, UDPTerminal.DefaultBufferSize);
                        udpBeaconReceiver.SyncContext = syncContext;
                        udpBeaconReceiver.OnReceive += OnUnreliableReceived;
                        udpBeaconReceiver.ReceiveStart();
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

        public int UserId { get; private set; }

        int beaconPortNumber = 32000;
        public int BeaconPortNumber { get { return beaconPortNumber; } set { if (!IsOpened) beaconPortNumber = value; } }

        int portNumber = 32001;
        public int PortNumber { get { return portNumber; } set { if (!IsOpened) portNumber = value; } }

        int listenPortNumber = 0;
        public int ListenPortNumber { get { return listenPortNumber; } set { if (!IsOpened) listenPortNumber = value; } }

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

        Dictionary<short, IDataChannel> dataChannelMap = new Dictionary<short, IDataChannel>();

        public delegate bool BeaconAcceptFunc(string data);
        BeaconAcceptFunc BeaconAccept = (data) => { if (data == "Snowball") return true; else return false; };

        public void SetBeaconAcceptFunction(BeaconAcceptFunc func) { BeaconAccept = func; }

        ComNode serverNode;
        ComNode previousServerNode;

        TCPConnector tcpConnector;

        UDPTerminal udpTerminal;
        UDPTerminal udpBeaconReceiver;

        int healthIntervalMs = 500;
        System.Timers.Timer healthTimer;

        int healthLostCount = 0;
        bool udpAck = true;

        int maxHealthLostCount = 5;
        public int MaxHealthLostCount { get { return maxHealthLostCount; } set { maxHealthLostCount = value; } }

        bool isConnecting = false;
        bool isDisconnecting = false;

        object serverNodeLocker = new object();

        bool IsTcpConnected { get { lock (serverNodeLocker) { return (serverNode != null); } } }
        public bool IsConnected { get; private set; }

        RsaEncrypter rsaEncrypter;

        public delegate bool ValidateRsaKeyFunc(string publicKey);
        ValidateRsaKeyFunc ValidateRsaKey = (string publicKey) =>
        {
            return true;
        };
        public void SetValidateRsaKeyFunction(ValidateRsaKeyFunc func) { ValidateRsaKey = func; }

        bool userSyncContext;
        SynchronizationContext syncContext;

        public ComClient(bool userSyncContext = true)
        {
            IsOpened = false;

            this.userSyncContext = userSyncContext;

            AddChannel(new DataChannel<IssueIdData>((short)PreservedChannelId.IssueId, QosType.Reliable, Compression.None, Encryption.None, (node, data) =>
            {
                UserId = data.Id;

                bool isValid = ValidateRsaKey(data.PublicKey);
                if (isValid)
                {
                    rsaEncrypter = new RsaEncrypter();
                    rsaEncrypter.FromPublicKeyXmlString(data.PublicKey);

                    AesKeyPair pair = GenerateAesKey();
                    SendInternal((short)PreservedChannelId.KeyExchange, pair);
                }
                else Disconnect();
            }));

            AddChannel(new DataChannel<byte[]>((short)PreservedChannelId.Health, QosType.Unreliable, Compression.None, Encryption.None, (node, data) =>
            {
                //Util.Log("Health");
                healthLostCount = 0;
                byte[] encrypted = EncrypteTmpKey(data);
                SendInternal((short)PreservedChannelId.Health, encrypted);
            }));

            AddChannel(new DataChannel<IssueIdData>((short)PreservedChannelId.UdpNotify, QosType.Unreliable, Compression.None, Encryption.None, (node, data) =>
            {
            }));

            AddChannel(new DataChannel<int>((short)PreservedChannelId.UdpNotifyAck, QosType.Reliable, Compression.None, Encryption.None, (node, data) =>
            {
                udpAck = true;
                IsConnected = true;
                if (OnConnected != null) OnConnected(serverNode);
            }));

            AddChannel(new DataChannel<AesKeyPair>((short)PreservedChannelId.KeyExchange, QosType.Reliable, Compression.None, Encryption.Rsa, (node, data) =>
            {
                
            }));

            AddChannel(new DataChannel<int>((short)PreservedChannelId.KeyExchangeAck, QosType.Reliable, Compression.None, Encryption.None, (node, data) =>
            {
                udpAck = false;
            }));

            healthTimer = new System.Timers.Timer(healthIntervalMs);
            healthTimer.Elapsed += OnHealthCheck;
        }

        public void Dispose()
        {
            AcceptBeacon = false;
            Close();
        }

        public void Open()
        {
            if (IsOpened) return;

            if (userSyncContext)
            {
                syncContext = SynchronizationContext.Current;
                if (syncContext == null)
                {
                    syncContext = new SnowballSynchronizationContext(10);
                    SynchronizationContext.SetSynchronizationContext(syncContext);
                }
            }

            int port = PortNumber;
            if (listenPortNumber != 0) port = listenPortNumber;
            udpTerminal = new UDPTerminal(port, bufferSize);
            udpTerminal.SyncContext = syncContext;
            udpTerminal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpTerminal.OnReceive += OnUnreliableReceived;

            tcpConnector = new TCPConnector(portNumber);
            tcpConnector.SyncContext = syncContext;
            tcpConnector.ConnectionBufferSize = bufferSize;
            tcpConnector.ConnectTimeOutMilliSec = connectTimeOutMilliSec;
            tcpConnector.OnConnected += OnConnectedInternal;

            IsOpened = true;

            healthTimer.Start();

            UdpCheck();
        }

        public void Close()
        {
            if (!IsOpened) return;

            healthTimer.Stop();

            AcceptBeacon = false;

            Disconnect();

            udpTerminal.Close();

            tcpConnector = null;
            udpTerminal = null;

            IsOpened = false;
        }

        byte[] EncrypteTmpKey(byte[] decrypted)
        {
            return decrypted;
        }

        AesKeyPair GenerateAesKey()
        {
            var aes = Aes.Create();
            aes.GenerateIV();
            aes.GenerateKey();
            aes.Padding = PaddingMode.PKCS7;

            serverNode.AesEncrypter = new AesEncrypter(aes);
            serverNode.AesDecrypter = new AesDecrypter(aes);

            AesKeyPair pair = new AesKeyPair();
            pair.Key = aes.Key;
            pair.IV = aes.IV;

            return pair;
        }

        async Task UdpCheck()
        {
            healthLostCount = 0;

            while (IsOpened)
            {
                try
                {
                    await Task.Delay(100);

                    if (IsTcpConnected)
                    {
                        if (!udpAck)
                        {
                            if (serverNode != null && serverNode.AesEncrypter != null)
                            {
                                byte[] encrypted = serverNode.AesEncrypter.Encrypt(Global.UdpRawData);

                                IssueIdData data = new IssueIdData();
                                data.Id = UserId;
                                data.encryptionData = encrypted;
                                SendInternal((short)PreservedChannelId.UdpNotify, data);
                            }
                        }

                    }
                }
                catch//(Exception e)
                {
                    //Util.Log("Health:" + e.Message);
                }
            }
        }

        void OnHealthCheck(object sender, ElapsedEventArgs args)
        {
            HealthCheck();
        }

        async Task HealthCheck()
        {
            healthLostCount = 0;

            if (IsOpened)
            {
                try
                {
                    await Task.Delay(500);

                    if (IsTcpConnected)
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
            if (!isConnecting && !IsTcpConnected && tcpConnector != null)
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

                lock (serverNodeLocker)
                {
                    serverNode = new ComSnowballNode(connection);
                }

                udpTerminal.ReceiveStart();

                connection.OnDisconnected = OnDisconnectedInternal;
                connection.OnPoll = OnPoll;

                IssueIdData ldata = new IssueIdData();

                if (UserId != 0)
                {
                    if (previousServerNode != null && previousServerNode.AesEncrypter != null)
                    {
                        ldata.Id = UserId;
                        ldata.encryptionData = previousServerNode.AesEncrypter.Encrypt(Global.ReconnectRawData);
                    }
                    else UserId = 0;
                }

                if (UserId == 0)
                {
                    ldata.Id = UserId;
                }

                SendInternal((short)PreservedChannelId.IssueId, ldata);
                
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
            if (!isDisconnecting && serverNode != null)
            {
                isDisconnecting = true;
                ((ComSnowballNode)serverNode).Connection.Disconnect();
                return true;
            }
            else return false;
        }

        void OnDisconnectedInternal(TCPConnection connection)
        {
            IsConnected = false;
            if (OnDisconnected != null) OnDisconnected(serverNode);

            lock (serverNodeLocker)
            {
                previousServerNode = serverNode;
                serverNode = null;
            }
            
            if(udpTerminal != null) udpTerminal.ReceiveStop();

            isDisconnecting = false;
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
                if (channelId == (short)PreservedChannelId.Beacon)
                {
                    if (acceptBeacon && !isConnecting && !IsTcpConnected)
                    {
                        string beaconData = (string)DataSerializer.Deserialize<string>(packer);

                        if (BeaconAccept(beaconData)) Connect(endPoint.Address.ToString());
                    }
                }
                else
                {
                    IDataChannel channel;

                    if (dataChannelMap.TryGetValue(channelId, out channel))
                    {
                        if (channelId == (short)PreservedChannelId.Health)
                        {
                            if (serverNode == null) break;

                            if (serverNode.UdpEndPoint == null && serverNode.TcpEndPoint.Address.Equals(endPoint.Address))
                            {
                                serverNode.UdpEndPoint = endPoint;
                            }
                        }

                        if (channel.CheckMode == CheckMode.Sequre)
                        {
                            IDecrypter decrypter = null;
                            if (channel.Encryption == Encryption.Rsa) throw new InvalidOperationException("Client cant receive data via RSA channel.");
                            else if (channel.Encryption == Encryption.Aes) decrypter = serverNode.AesDecrypter;

                            if (serverNode == null) break;
                            if (endPoint.Address.Equals(serverNode.UdpEndPoint.Address))
                            {
                                object container = channel.FromStream(ref packer, decrypter);

                                channel.Received(serverNode, container);
                            }
                        }
                        else
                        {
                            IDecrypter decrypter = null;
                            if (channel.Encryption == Encryption.Rsa) throw new InvalidOperationException("Client cant receive data via RSA channel.");
                            else if (channel.Encryption == Encryption.Aes) decrypter = serverNode.AesDecrypter;

                            object container = channel.FromStream(ref packer, decrypter);

                            channel.Received(serverNode, container);
                        }
                    }
                }

                head += datasize + 4;

            }
        }

        void OnReliableReceived(IPEndPoint endPoint, short channelId, byte[] data, int size)
        {
            if (channelId == (short)PreservedChannelId.Beacon)
            {
            }
            else
            {
                IDataChannel channel;

                if (dataChannelMap.TryGetValue(channelId, out channel))
                {
                    BytePacker packer = new BytePacker(data);

                    IDecrypter decrypter = null;
                    if (channel.Encryption == Encryption.Rsa) throw new InvalidOperationException("Client cant receive data via RSA channel.");
                    else if (channel.Encryption == Encryption.Aes) decrypter = serverNode.AesDecrypter;

                    if (channel.CheckMode == CheckMode.Sequre || decrypter != null)
                    {
                        if (serverNode == null) ;
                        if (endPoint.Equals(serverNode.TcpEndPoint))
                        {
                            object container = channel.FromStream(ref packer, decrypter);

                            channel.Received(serverNode, container);
                        }
                    }
                    else
                    {
                        object container = channel.FromStream(ref packer, decrypter);

                        channel.Received(null, container);
                    }
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

        async Task<int> ReadAsync(NetworkStream nStream, byte[] receiveBuffer, int size, CancellationTokenSource cancelToken)
        {
            int tmpSize = 0;
            int receivedSize = 0;
            while (true)
            {
                tmpSize = 0;
                tmpSize = await nStream.ReadAsync(receiveBuffer, receivedSize, size - receivedSize, cancelToken.Token).ConfigureAwait(false);

                if (tmpSize == 0)
                {
                    return 0;
                }
                receivedSize += tmpSize;
                if (receivedSize == size)
                {
                    return receivedSize;
                }
            }
        }

        async Task<bool> OnPoll(
            TCPConnection connection,
            NetworkStream nStream,
            byte[] receiveBuffer,
            BytePacker receivePacker,
            CancellationTokenSource cancelToken
            )
        {
            int resSize = 0;
            int tmpSize = 0;
            short channelId = 0;

            bool isRent = false;
            byte[] buffer = null;

            try
            {
                resSize = await ReadAsync(nStream, receiveBuffer, 2, cancelToken).ConfigureAwait(false);

                if (resSize != 0)
                {
                    receivePacker.Position = 0;
                    resSize = receivePacker.ReadShort();
#if DISABLE_CHANNEL_VARINT
                    tmpSize = await ReadAsync(nStream, receiveBuffer, 2, cancelToken).ConfigureAwait(false);
                    if (tmpSize == 0) throw new EndOfStreamException();

                    receivePacker.Position = 0;
                    channelId = receivePacker.ReadShort();
#else
                    int s = 0;
                    channelId = VarintBitConverter.ToShort(nStream, out s);
#endif
                    tmpSize = await ReadAsync(nStream, receiveBuffer, resSize, cancelToken).ConfigureAwait(false);
                    if (tmpSize == 0) throw new EndOfStreamException();

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
                //Util.Log("TCP:" + e.GetType().Name + ":" + e.Message + ":" +  e.StackTrace);
                return false;
            }

            if (resSize == 0)
            {
                return false;
            }

            if (cancelToken.IsCancellationRequested) return false;

            if (syncContext != null)
            {
                syncContext.Post((state) =>
                {
                    if (cancelToken.IsCancellationRequested) return;
                    CallbackParam param = (CallbackParam)state;
                    OnReliableReceived(param.endPoint, param.channelId, param.buffer, param.size);
                    if (isRent) arrayPool.Return(buffer);
                }, new CallbackParam((IPEndPoint)connection.Client.Client.RemoteEndPoint, channelId, buffer, resSize, isRent));
            }
            else
            {
                OnReliableReceived((IPEndPoint)connection.Client.Client.RemoteEndPoint, channelId, buffer, resSize);
            }

            return true;
        }

        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

        void BuildBuffer<T>(
            IDataChannel channel, T data, ref byte[] buffer, ref int bufferSize, ref bool isRent, IEncrypter encrypter
            )
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
            packer.WriteShort((short)bufSize);

#if DISABLE_CHANNEL_VARINT
        packer.WriteShort(channel.ChannelID);
#else
            int s = 0;
            VarintBitConverter.SerializeShort(channel.ChannelID, packer, out s);
#endif
            int start = packer.Position;

            channel.ToStream(data, ref packer, encrypter);

            bufferSize = (int)packer.Position;

            packer.Position = 0;
            packer.WriteShort((short)(bufferSize - start));
        }

        public async Task<bool> Send<T>(short channelId, T data)
        {
            if (!IsConnected) return false;
            else return await SendInternal<T>(channelId, data);
        }

        async Task<bool> SendInternal<T>(short channelId, T data)
        {
            return await Task.Run(async () =>
            {
                if (!IsTcpConnected) return false;

                IDataChannel channel;
                if (!dataChannelMap.TryGetValue(channelId, out channel)) return false;

                bool isRent = false;
                byte[] buffer = null;
                int bufferSize = 0;

                IEncrypter encrypter = null;
                if (channel.Encryption == Encryption.Rsa) encrypter = rsaEncrypter;
                else if (channel.Encryption == Encryption.Aes) encrypter = serverNode.AesEncrypter;

                BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent, encrypter);

                if (channel.Qos == QosType.Reliable)
                {
                    await ((ComSnowballNode)serverNode).Connection.Send(bufferSize, buffer).ConfigureAwait(false);
                }
                else if (channel.Qos == QosType.Unreliable)
                {
                    await udpTerminal.Send(serverNode.Ip, portNumber, bufferSize, buffer).ConfigureAwait(false);
                }

                if (isRent) arrayPool.Return(buffer);

                return true;
            }).ConfigureAwait(false);

        }
    }

}
