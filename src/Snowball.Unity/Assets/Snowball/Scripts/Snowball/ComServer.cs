﻿#define DISABLE_CHANNEL_VARINT

using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Linq;

namespace Snowball
{

    public class ComServer : IDisposable
    {
        public bool IsOpened { get; protected set; }

        int beaconPortNumber = 32000;
        public int BeaconPortNumber { get { return beaconPortNumber; } set { if (!IsOpened) beaconPortNumber = value; } }

        int listenPortNumber = 32001;
        public int ListenPortNumber { get { return listenPortNumber; } set { if (!IsOpened) listenPortNumber = value; } }

        int sendPortNumber = 32002;
        public int SendPortNumber { get { return sendPortNumber; } set { if (!IsOpened) sendPortNumber = value; } }

        int bufferSize = 8192;
        public int BufferSize { get { return bufferSize; } set { if (!IsOpened) bufferSize = value; } }

        public delegate void ConnectedHandler(ComNode node);
        public ConnectedHandler OnConnected;

        public delegate void DisconnectedHandler(ComNode node);
        public DisconnectedHandler OnDisconnected;

        protected Dictionary<short, IDataChannel> dataChannelMap = new Dictionary<short, IDataChannel>();

        protected Dictionary<string, ComNode> userNodeMap = new Dictionary<string, ComNode>();
        protected Dictionary<IPEndPoint, ComNode> nodeTcpMap = new Dictionary<IPEndPoint, ComNode>();
        protected Dictionary<IPEndPoint, ComNode> nodeUdpMap = new Dictionary<IPEndPoint, ComNode>();

        public delegate string BeaconDataGenerateFunc();
        BeaconDataGenerateFunc BeaconDataCreate = () => {
            return "Snowball";
        };

        public void SetBeaconDataCreateFunction(BeaconDataGenerateFunc func) { BeaconDataCreate = func; }

        UDPSender udpBeaconSender;
        UDPTerminal udpTerminal;

        TCPListener tcpListener;


        protected int beaconIntervalMs = 1000;
        public int BeaconIntervalMs { get { return beaconIntervalMs; } set { if (!IsOpened) beaconIntervalMs = value; } }
        System.Timers.Timer beaconTimer;

        protected int healthIntervalMs = 500;
        System.Timers.Timer healthTimer;

        Converter beaconConverter;

        int maxHealthLostCount = 5;
        public int MaxHealthLostCount { get { return maxHealthLostCount; } set { maxHealthLostCount = value; } }

        List<string> beaaconList = new List<string>();

        public ComServer()
        {
            IsOpened = false;

            AddChannel(new DataChannel<string>((short)PreservedChannelId.Login, QosType.Reliable, Compression.None, (node, data) =>
            {
                node.UserName = data;

                if (userNodeMap.ContainsKey(node.UserName)) { userNodeMap.Remove(node.UserName); }

                userNodeMap.Add(node.UserName, node);
                Send(node, (short)PreservedChannelId.Login, "");

                //Util.Log("SetUsername:" + node.UserName);

            }));

            AddChannel(new DataChannel<byte[]>((short)PreservedChannelId.Health, QosType.Unreliable, Compression.None, (node, data) =>
            {
                byte[] decrypted = DecrypteTmpKey(data);
                if (node.TmpKey != null && node.TmpKey.SequenceEqual(decrypted))
                {
                    node.HealthLostCount = 0;
                }
                else
                {
                    node.TmpKey = null;
                }
            }));

            AddChannel(new DataChannel<string>((short)PreservedChannelId.UdpNotify, QosType.Unreliable, Compression.None, (node, data) =>
            {
            }));

            AddChannel(new DataChannel<int>((short)PreservedChannelId.UdpNotifyAck, QosType.Reliable, Compression.None, (node, data) =>
            {
            }));

            beaconConverter = DataSerializer.GetConverter(typeof(string));

            healthTimer = new System.Timers.Timer(healthIntervalMs);
            healthTimer.Elapsed += OnHealthCheck;
        }

        public void Dispose()
        {
            BeaconStop();
            Close();
        }

        public void AddBeaconList(string ip)
        {
            beaaconList.Add(ip);
        }

        public void RemoveBeaconList(string ip)
        {
            beaaconList.Remove(ip);
        }

        public void Open()
        {
            if (IsOpened) return;

            if (Global.UseSyncContextPost && Global.SyncContext == null)
                Global.SyncContext = SynchronizationContext.Current;

            udpTerminal = new UDPTerminal(listenPortNumber, bufferSize);
            udpTerminal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            tcpListener = new TCPListener(listenPortNumber);
            tcpListener.ConnectionBufferSize = bufferSize;
            tcpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            tcpListener.OnConnected += OnConnectedInternal;

            udpTerminal.OnReceive += OnUDPReceived;

            tcpListener.Start();

            udpTerminal.ReceiveStart();

            IsOpened = true;

            healthTimer.Start();
        }

        int CreateBeaconData(out BytePacker packer)
        {
            string data = BeaconDataCreate();
            int dataSize = beaconConverter.GetDataSize(data);

            byte[] beaconBuf = new byte[dataSize + 4];
            packer = new BytePacker(beaconBuf);

            packer.Write((short)dataSize);

#if DISABLE_CHANNEL_VARINT
            packer.Write((short)PreservedChannelId.Beacon);
#else
            int s = 0;
            VarintBitConverter.SerializeShort((short)PreservedChannelId.Beacon, packer, out s);
#endif
            beaconConverter.Serialize(packer, data);

            return packer.Position;

        }

        public void SendConnectBeacon(string ip)
        {
            BytePacker packer;
            int length = CreateBeaconData(out packer);

            udpBeaconSender.Send(ip, length, packer.Buffer);
        }

        void OnBeaconTimer(object sender, ElapsedEventArgs args)
        {
            BytePacker packer;
            int length = CreateBeaconData(out packer);

            foreach (var ip in beaaconList)
            {
                udpBeaconSender.Send(ip, length, packer.Buffer);
            }
        }

        public void Close()
        {
            if (!IsOpened) return;

            healthTimer.Stop();

            var nMap = new Dictionary<IPEndPoint, ComNode>(nodeTcpMap);
            foreach (var node in nMap)
            {
                ((ComSnowballNode)node.Value).Connection.Disconnect();
            }

            tcpListener.Stop();
            udpTerminal.Close();

            tcpListener = null;
            udpTerminal = null;

            IsOpened = false;
        }

        public void BeaconStart()
        {
            if (udpBeaconSender == null)
            {
                udpBeaconSender = new UDPSender(beaconPortNumber);
                beaconTimer = new System.Timers.Timer(BeaconIntervalMs);
                beaconTimer.Elapsed += OnBeaconTimer;
                beaconTimer.Start();
            }
        }

        public void BeaconStop()
        {
            if (beaconTimer != null)
            {
                beaconTimer.Stop();
                beaconTimer = null;
                udpBeaconSender.Close();
                udpBeaconSender = null;
            }
        }

        public ComNode GetTcpNodeByEndPoint(IPEndPoint endPoint)
        {
            if (nodeTcpMap.ContainsKey(endPoint))
            {
                return nodeTcpMap[endPoint];
            }
            return null;
        }

        public ComNode GetUdpNodeByEndPoint(IPEndPoint endPoint)
        {
            if (nodeUdpMap.ContainsKey(endPoint))
            {
                return nodeUdpMap[endPoint];
            }
            return null;
        }

        public virtual byte[] GenerateTmpKey()
        {
            Random rand = new Random();
            byte[] key = new byte[4];
            rand.NextBytes(key);
            return key;
        }

        public virtual byte[] DecrypteTmpKey(byte[] encrypted)
        {
            return encrypted;
        }

        public void OnHealthCheck(object sender, ElapsedEventArgs args)
        {
            HealthCheck();
        }

        public async Task HealthCheck()
        {
            if (IsOpened)
            {
                List<ComNode> invalidNodeArray = new List<ComNode>();

                foreach (var keypair in nodeTcpMap)
                {
                    byte[] key = GenerateTmpKey();
                    keypair.Value.TmpKey = key;
                    Send(keypair.Value, (short)PreservedChannelId.Health, key);
                    keypair.Value.HealthLostCount++;
                    if (keypair.Value.HealthLostCount > MaxHealthLostCount)
                    {
                        invalidNodeArray.Add(keypair.Value);
                    }
                }

                foreach (var node in invalidNodeArray)
                {
                    //Util.Log("Server:Disconnect##########");
                    Disconnect(node);
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

        void OnConnectedInternal(TCPConnection connection)
        {
            if (connection == null) return;

            connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            lock (this)
            {
                ComSnowballNode node = new ComSnowballNode(connection);

                nodeTcpMap.Add(node.TcpEndPoint, node);

                node.HealthLostCount = 0;

                connection.OnDisconnected = OnDisconnectedInternal;
                connection.OnPoll = OnPoll;

                //Util.Log("Server:Connected");
            }
        }

        public bool Disconnect(ComNode node)
        {
            if (nodeTcpMap.ContainsKey(node.TcpEndPoint))
            {
                ComSnowballNode snode = (ComSnowballNode)node;
                snode.Connection.Disconnect();

                return true;
            }
            else return false;
        }

        void OnDisconnectedInternal(TCPConnection connection)
        {
            lock (this)
            {
                if (nodeTcpMap.ContainsKey((IPEndPoint)connection.EndPoint))
                {
                    ComSnowballNode node = (ComSnowballNode)nodeTcpMap[(IPEndPoint)connection.EndPoint];
                    nodeTcpMap.Remove((IPEndPoint)connection.EndPoint);

                    if (userNodeMap.ContainsKey(node.UserName)) userNodeMap.Remove(node.UserName);
                    if (node.UdpEndPoint != null && nodeUdpMap.ContainsKey(node.UdpEndPoint)) nodeUdpMap.Remove(node.UdpEndPoint);
                    node.UdpEndPoint = null;

                    if (OnDisconnected != null) OnDisconnected(node);

                    //Util.Log("Server:Disconnected");
                }
            }
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
                }
                else if (channelId == (short)PreservedChannelId.UdpNotify)
                {
                    IDataChannel channel = dataChannelMap[channelId];
                    string userName = (string)channel.FromStream(ref packer);
                    if (userNodeMap.ContainsKey(userName))
                    {
                        //Util.Log("UdpUserName:" + userName);
                        ComSnowballNode node = (ComSnowballNode)userNodeMap[userName];
                        if (node.UdpEndPoint == null)
                        {
                            if (OnConnected != null) OnConnected(node);
                            node.UdpEndPoint = endPoint;
                            nodeUdpMap.Add(endPoint, node);
                            Send(node, (short)PreservedChannelId.UdpNotifyAck, endPoint.Port);

                            byte[] key = GenerateTmpKey();
                            node.TmpKey = key;
                            Send(node, (short)PreservedChannelId.Health, key);
                        }
                    }
                }
                else if (!dataChannelMap.ContainsKey(channelId))
                {
                }
                else
                {
                    IDataChannel channel = dataChannelMap[channelId];

                    if (channel.CheckMode == CheckMode.Sequre)
                    {
                        if (nodeUdpMap.ContainsKey(endPoint))
                        {
                            ComNode node = nodeUdpMap[endPoint];

                            object container = channel.FromStream(ref packer);

                            channel.Received(node, container);
                        }
                    }
                    else
                    {
                        object container = channel.FromStream(ref packer);

                        channel.Received(null, container);
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
            else if (channelId == (short)PreservedChannelId.Health)
            {
                if (nodeTcpMap.ContainsKey(endPoint))
                {
                    ComNode node = nodeTcpMap[endPoint];
                }
            }
            else if (!dataChannelMap.ContainsKey(channelId))
            {
            }
            else
            {
                BytePacker packer = new BytePacker(data);

                if (nodeTcpMap.ContainsKey(endPoint))
                {
                    ComNode node = nodeTcpMap[endPoint];

                    IDataChannel channel = dataChannelMap[channelId];

                    object container = channel.FromStream(ref packer);

                    channel.Received(node, container);
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
                tmpSize = await nStream.ReadAsync(receiveBuffer, receivedSize, size - receivedSize, cancelToken.Token);
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

        public async Task<bool> OnPoll(
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
            packer.Write(channel.ChannelID);
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

        public async Task<bool> Broadcast<T>(ComGroup group, short channelId, T data, ComNode exception = null)
        {
            if (!dataChannelMap.ContainsKey(channelId)) return false;

            IDataChannel channel = dataChannelMap[channelId];

            bool isRent = false;
            byte[] buffer = null;
            int bufferSize = 0;

            BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent);

            foreach (var node in group.NodeList)
            {
                if (node == exception) continue;
                if (!nodeTcpMap.ContainsKey(node.TcpEndPoint)) continue;

                ComSnowballNode snode = (ComSnowballNode)node;
                if (channel.Qos == QosType.Reliable)
                {
                    await snode.Connection.Send(bufferSize, buffer);
                }
                else if (channel.Qos == QosType.Unreliable)
                {
                    if (snode.UdpEndPoint != null)
                    {
                        await udpTerminal.Send(snode.Ip, snode.UdpEndPoint.Port, bufferSize, buffer);
                    }

                }
            }

            if (isRent) arrayPool.Return(buffer);

            return true;
        }


        public async Task<bool> Send<T>(ComNode node, short channelId, T data)
        {
            return await Task.Run(async () =>
            {
                if (!nodeTcpMap.ContainsKey(node.TcpEndPoint)) return false;
                if (!dataChannelMap.ContainsKey(channelId)) return false;

                IDataChannel channel = dataChannelMap[channelId];

                bool isRent = false;
                byte[] buffer = null;
                int bufferSize = 0;

                BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent);

                ComSnowballNode snode = (ComSnowballNode)node;
                if (channel.Qos == QosType.Reliable)
                {
                    await snode.Connection.Send(bufferSize, buffer);
                }
                else if (channel.Qos == QosType.Unreliable)
                {
                    if (snode.UdpEndPoint != null)
                    {
                        await udpTerminal.Send(snode.Ip, snode.UdpEndPoint.Port, bufferSize, buffer);
                    }
                }

                if (isRent) arrayPool.Return(buffer);

                return true;
            });
                
        }

    }

}

