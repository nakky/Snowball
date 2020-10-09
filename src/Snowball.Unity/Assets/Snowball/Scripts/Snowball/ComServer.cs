#define DISABLE_CHANNEL_VARINT

using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Linq;

using System.Security.Cryptography;

namespace Snowball
{
    public sealed class ComServer : IDisposable
    {
        public bool IsOpened { get; private set; }

        int beaconPortNumber = 32000;
        public int BeaconPortNumber { get { return beaconPortNumber; } set { if (!IsOpened) beaconPortNumber = value; } }

        int portNumber = 32001;
        public int PortNumber { get { return portNumber; } set { if (!IsOpened) portNumber = value; } }

        int bufferSize = 81920;
        public int BufferSize { get { return bufferSize; } set { if (!IsOpened) bufferSize = value; } }

        public delegate void ConnectedHandler(ComNode node);
        public ConnectedHandler OnConnected;

        public delegate void DisconnectedHandler(ComNode node);
        public DisconnectedHandler OnDisconnected;

        public delegate void NodeRemovedHandler(ComNode node);
        public NodeRemovedHandler OnNodeRemoved;

        bool isDisconnecting = false;

        Dictionary<short, IDataChannel> dataChannelMap = new Dictionary<short, IDataChannel>();

        Dictionary<int, ComNode> userNodeMap = new Dictionary<int, ComNode>();
        ReaderWriterLock userNodeMapLock = new ReaderWriterLock();

        Dictionary<IPEndPoint, ComNode> nodeTcpMap = new Dictionary<IPEndPoint, ComNode>();
        Dictionary<IPEndPoint, ComNode> nodeUdpMap = new Dictionary<IPEndPoint, ComNode>();

        Random userIdRandom = new Random();

        Dictionary<int, ComNode> disconnectedUserNodeMap = new Dictionary<int, ComNode>();
        ReaderWriterLock disconnectedUserNodeMapLock = new ReaderWriterLock();

        public delegate string BeaconDataGenerateFunc();
        BeaconDataGenerateFunc BeaconDataCreate = () => {
            return "Snowball";
        };
        public void SetBeaconDataCreateFunction(BeaconDataGenerateFunc func) { BeaconDataCreate = func; }

        UDPTerminal udpBeaconSender;
        UDPTerminal udpTerminal;

        TCPListener tcpListener;

        int beaconIntervalMs = 1000;
        public int BeaconIntervalMs { get { return beaconIntervalMs; } set { if (!IsOpened) beaconIntervalMs = value; } }
        System.Timers.Timer beaconTimer;

        int healthIntervalMs = 500;
        System.Timers.Timer healthTimer;

        int removeNodeCheckIntervalMs = 1000;
        System.Timers.Timer removeNodeCheckTimer;

        int removeIntervalSec = 30;
        public int RemoveIntervalSec { get { return removeIntervalSec; } set { if (!IsOpened) removeIntervalSec = value; } }

        Converter beaconConverter;

        int maxHealthLostCount = 5;
        public int MaxHealthLostCount { get { return maxHealthLostCount; } set { maxHealthLostCount = value; } }

        List<string> beaaconList = new List<string>();

        public RSAParameters RsaPrivateKey { get; private set; }
        public RSAParameters RsaPublicKey { get; private set; }

        RsaDecrypter rsaDecrypter;

        public delegate void RsaKeyGenerateFunc(out RSAParameters publicKey, out RSAParameters privateKey);

        RsaKeyGenerateFunc RsaKeyGenerate = (out RSAParameters publicKey, out RSAParameters privateKey) =>
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            publicKey = rsa.ExportParameters(false);
            privateKey = rsa.ExportParameters(true);
        };
        public void SetRsaKeyGenerateFunction(RsaKeyGenerateFunc func) { RsaKeyGenerate = func; }

        bool userSyncContext;
        SynchronizationContext syncContext;

        public ComServer(bool userSyncContext = true)
        {
            IsOpened = false;

            this.userSyncContext = userSyncContext;

            AddChannel(new DataChannel<IssueIdData>((short)PreservedChannelId.IssueId, QosType.Reliable, Compression.None, Encryption.None, (node, data) =>
            {
                OnIssueId(node, data);
            }));

            AddChannel(new DataChannel<byte[]>((short)PreservedChannelId.Health, QosType.Unreliable, Compression.None, Encryption.None, (node, data) =>
            {
                byte[] decrypted = DecrypteTmpKey(data);
                if (node.TmpKey != null && node.TmpKey.SequenceEqual(decrypted)) node.HealthLostCount = 0;
                else node.TmpKey = null;
            }));

            AddChannel(new DataChannel<IssueIdData>((short)PreservedChannelId.UdpNotify, QosType.Unreliable, Compression.None, Encryption.None, (node, data) =>
            {
            }));

            AddChannel(new DataChannel<int>((short)PreservedChannelId.UdpNotifyAck, QosType.Reliable, Compression.None, Encryption.None, (node, data) =>
            {
            }));

            AddChannel(new DataChannel<AesKeyPair>((short)PreservedChannelId.KeyExchange, QosType.Reliable, Compression.None, Encryption.Rsa, (node, data) =>
            {
                OnKeyExchange(node, data);
            }));

            AddChannel(new DataChannel<int>((short)PreservedChannelId.KeyExchangeAck, QosType.Reliable, Compression.None, Encryption.None, (node, data) =>
            {
            }));

            beaconConverter = DataSerializer.GetConverter(typeof(string));

            healthTimer = new System.Timers.Timer(healthIntervalMs);
            healthTimer.Elapsed += OnHealthCheck;

            removeNodeCheckTimer = new System.Timers.Timer(removeNodeCheckIntervalMs);
            removeNodeCheckTimer.Elapsed += OnRemoveNodeCheck;

        }

        public void Dispose()
        {
            BeaconStop();
            Close();
        }

        void OnIssueId(ComNode node, IssueIdData data)
        {
            bool registerd = false;

            if (data.Id != 0 && data.encryptionData != null && data.encryptionData.Length > 0)
            {
                ComNode previousNode = null;
                try
                {
                    userNodeMapLock.AcquireReaderLock(1000);
                    ComNode n;
                    if (userNodeMap.TryGetValue(data.Id, out n)) previousNode = n;
                }
                finally
                {
                    userNodeMapLock.ReleaseReaderLock();
                }

                try
                {
                    disconnectedUserNodeMapLock.AcquireWriterLock(1000);
                    ComNode n;
                    if (previousNode == null && disconnectedUserNodeMap.TryGetValue(data.Id, out n))
                    {
                        previousNode = n;
                        disconnectedUserNodeMap.Remove(data.Id);
                    }
                }
                finally
                {
                    disconnectedUserNodeMapLock.ReleaseWriterLock();
                }

                if (previousNode != null && previousNode.AesDecrypter != null)
                {
                    try
                    {
                        byte[] decrypted = previousNode.AesDecrypter.Decrypt(data.encryptionData);
                        if (decrypted.SequenceEqual(Global.ReconnectRawData))
                        {
                            node.UserId = previousNode.UserId;

                            try
                            {
                                userNodeMapLock.AcquireWriterLock(1000);
                                userNodeMap.Add(node.UserId, node);
                            }
                            finally
                            {
                                userNodeMapLock.ReleaseWriterLock();
                            }
                            registerd = true;
                        }
                    }
                    catch
                    {

                    }
                }

            }

            if (!registerd)
            {
                GenerateId(ref node);
                registerd = true;
            }

            if (!registerd) Disconnect(node);
            else
            {
                IssueIdData ldata = new IssueIdData();
                ldata.Id = node.UserId;
                string xml = rsaDecrypter.ToPublicKeyXmlString();
                ldata.PublicKey = xml;
                ldata.encryptionData = new byte[0];
                SendInternal(node, (short)PreservedChannelId.IssueId, ldata);
            }
        }

        void OnKeyExchange(ComNode node, AesKeyPair data)
        {
            var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = data.Key;
            aes.IV = data.IV;

            node.AesEncrypter = new AesEncrypter(aes);
            node.AesDecrypter = new AesDecrypter(aes);

            SendInternal(node, (short)PreservedChannelId.KeyExchangeAck, 0);

        }

        public void AddBeaconList(string ip)
        {
            beaaconList.Add(ip);
        }

        public void RemoveBeaconList(string ip)
        {
            beaaconList.Remove(ip);
        }

        void GenerateId(ref ComNode node)
        {
            int id;

            while (true)
            {
                try
                {
                    userNodeMapLock.AcquireWriterLock(1000);
                    disconnectedUserNodeMapLock.AcquireReaderLock(1000);

                    id = userIdRandom.Next(1, int.MaxValue);

                    if (!userNodeMap.ContainsKey(id) && !disconnectedUserNodeMap.ContainsKey(id))
                    {
                        userNodeMap.Add(id, node);
                        node.UserId = id;
                        break;
                    }
                }
                finally
                {
                    disconnectedUserNodeMapLock.ReleaseReaderLock();
                    userNodeMapLock.ReleaseWriterLock();
                }

            }

        }

        public void Open()
        {
            if (IsOpened) return;

            if (userSyncContext) {
                syncContext = SynchronizationContext.Current;
                if(syncContext == null)
                {
                    syncContext = new SnowballSynchronizationContext(10);
                    SynchronizationContext.SetSynchronizationContext(syncContext);
                }
            }

            RSAParameters publicKey;
            RSAParameters privateKey;

            RsaKeyGenerate(out publicKey, out privateKey);
            RsaPublicKey = publicKey;
            RsaPrivateKey = privateKey;

            rsaDecrypter = new RsaDecrypter(RsaPrivateKey);

            udpTerminal = new UDPTerminal(portNumber, bufferSize);
            udpTerminal.SyncContext = syncContext;
            udpTerminal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            tcpListener = new TCPListener(portNumber);
            tcpListener.SyncContext = syncContext;
            tcpListener.ConnectionBufferSize = bufferSize;
            tcpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            tcpListener.OnConnected += OnConnectedInternal;

            udpTerminal.OnReceive += OnUnreliableReceived;

            tcpListener.Start();

            udpTerminal.ReceiveStart();

            IsOpened = true;

            healthTimer.Start();
            removeNodeCheckTimer.Start();
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

            udpBeaconSender.Send(ip, beaconPortNumber, length, packer.Buffer);
        }

        void OnBeaconTimer(object sender, ElapsedEventArgs args)
        {
            if (!IsOpened) return;

            BytePacker packer;
            int length = CreateBeaconData(out packer);

            foreach (var ip in beaaconList)
            {
                udpBeaconSender.Send(ip, beaconPortNumber, length, packer.Buffer);
            }
        }

        public void Close()
        {
            if (!IsOpened) return;

            healthTimer.Stop();
            removeNodeCheckTimer.Stop();

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
                udpBeaconSender = new UDPTerminal(UDPTerminal.DefaultBufferSize);
                udpBeaconSender.SyncContext = syncContext;
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

        public ComNode GetNodeByEndPoint(IPEndPoint endPoint)
        {
            ComNode node;
            if (nodeTcpMap.TryGetValue(endPoint, out node)) return node;
            return null;
        }

        byte[] GenerateTmpKey()
        {
            Random rand = new Random();
            byte[] key = new byte[4];
            rand.NextBytes(key);
            return key;
        }

        byte[] DecrypteTmpKey(byte[] encrypted)
        {
            return encrypted;
        }

        void OnHealthCheck(object sender, ElapsedEventArgs args)
        {
            if (!IsOpened) return;
            HealthCheck();
        }

        async Task HealthCheck()
        {
            if (IsOpened)
            {
                List<ComNode> invalidNodeArray = new List<ComNode>();

                foreach (var keypair in nodeTcpMap)
                {
                    byte[] key = GenerateTmpKey();
                    keypair.Value.TmpKey = key;
                    SendInternal(keypair.Value, (short)PreservedChannelId.Health, key);
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

        void OnRemoveNodeCheck(object sender, ElapsedEventArgs args)
        {
            RemoveNodeCheck();
        }
        async Task RemoveNodeCheck()
        {
            TimeSpan intervalSpan = new TimeSpan(0, 0, RemoveIntervalSec);

            List<int> removed = new List<int>();

            foreach (var pair in disconnectedUserNodeMap)
            {
                TimeSpan elapsed = DateTime.Now - pair.Value.DisconnectedTimeStamp;

                if (elapsed > intervalSpan)
                {
                    removed.Add(pair.Key);
                }
            }

            foreach (var key in removed)
            {
                try
                {
                    disconnectedUserNodeMapLock.AcquireWriterLock(1000);

                    ComNode node;
                    if (disconnectedUserNodeMap.TryGetValue(key, out node))
                    {
                        disconnectedUserNodeMap.Remove(key);
                        if (OnNodeRemoved != null) OnNodeRemoved(node);
                    }
                }
                finally
                {
                    disconnectedUserNodeMapLock.ReleaseWriterLock();
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

                if (nodeTcpMap.ContainsKey(node.TcpEndPoint))
                {
                    Disconnect(node);
                }
                else
                {
                    nodeTcpMap.Add(node.TcpEndPoint, node);

                    node.HealthLostCount = 0;

                    connection.OnDisconnected = OnDisconnectedInternal;
                    connection.OnPoll = OnPoll;
                }

                //Util.Log("Server:Connected");
            }
        }

        public bool Disconnect(ComNode node)
        {
            if (!node.IsDisconnecting && nodeTcpMap.ContainsKey(node.TcpEndPoint))
            {
                node.IsDisconnecting = true;

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
                ComNode n;

                if (nodeTcpMap.TryGetValue((IPEndPoint)connection.EndPoint, out n))
                {
                    ComSnowballNode node = (ComSnowballNode)n;
                    if (nodeTcpMap.ContainsKey(connection.EndPoint))
                        nodeTcpMap.Remove((IPEndPoint)connection.EndPoint);

                    try
                    {
                        userNodeMapLock.AcquireWriterLock(1000);
                        if (userNodeMap.ContainsKey(node.UserId)) userNodeMap.Remove(node.UserId);
                    }
                    finally
                    {
                        userNodeMapLock.ReleaseWriterLock();
                    }

                    node.DisconnectedTimeStamp = DateTime.Now;

                    try
                    {
                        disconnectedUserNodeMapLock.AcquireWriterLock(1000);

                        if (!disconnectedUserNodeMap.ContainsKey(node.UserId))
                            disconnectedUserNodeMap.Add(node.UserId, node);
                    }
                    finally
                    {
                        disconnectedUserNodeMapLock.ReleaseWriterLock();
                    }

                    if (node.UdpEndPoint != null && nodeUdpMap.ContainsKey(node.UdpEndPoint))
                        nodeUdpMap.Remove(node.UdpEndPoint);

                    node.UdpEndPoint = null;
                    node.IsDisconnecting = false;

                    node.IsConnected = false;
                    if (OnDisconnected != null) OnDisconnected(node);
                }
            }
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

                IDataChannel channel;

                if (dataChannelMap.TryGetValue(channelId, out channel))
                {
                    if (channelId == (short)PreservedChannelId.Beacon)
                    {
                    }
                    else if (channelId == (short)PreservedChannelId.UdpNotify)
                    {
                        IssueIdData idData = (IssueIdData)channel.FromStream(ref packer);

                        try
                        {
                            userNodeMapLock.AcquireReaderLock(1000);

                            ComNode n;
                            if (userNodeMap.TryGetValue(idData.Id, out n))
                            {
                                //Util.Log("UdpUserName:" + userName);
                                ComSnowballNode node = (ComSnowballNode)n;

                                if (node.UdpEndPoint == null && node.AesDecrypter != null)
                                {
                                    byte[] decrypted = node.AesDecrypter.Decrypt(idData.encryptionData);

                                    if (decrypted.SequenceEqual(Global.UdpRawData))
                                    {
                                        node.UdpEndPoint = endPoint;

                                        if (nodeUdpMap.ContainsKey(endPoint)) nodeUdpMap.Remove(endPoint);

                                        nodeUdpMap.Add(endPoint, node);
                                        SendInternal(node, (short)PreservedChannelId.UdpNotifyAck, endPoint.Port);

                                        byte[] key = GenerateTmpKey();
                                        node.TmpKey = key;
                                        SendInternal(node, (short)PreservedChannelId.Health, key);

                                        node.IsConnected = true;
                                        if (OnConnected != null) OnConnected(node);
                                    }

                                }

                            }
                        }
                        finally
                        {
                            userNodeMapLock.ReleaseReaderLock();
                        }
                    }
                    else
                    {
                        if (channel.CheckMode == CheckMode.Sequre || channel.Encryption == Encryption.Aes)
                        {
                            ComNode node;
                            if (nodeUdpMap.TryGetValue(endPoint, out node))
                            {
                                IDecrypter decrypter = null;
                                if (channel.Encryption == Encryption.Rsa) decrypter = rsaDecrypter;
                                else if (channel.Encryption == Encryption.Aes) decrypter = node.AesDecrypter;

                                object container = channel.FromStream(ref packer, decrypter);

                                channel.Received(node, container);
                            }
                        }
                        else
                        {
                            IDecrypter decrypter = null;
                            if (channel.Encryption == Encryption.Rsa) decrypter = rsaDecrypter;

                            object container = channel.FromStream(ref packer, decrypter);

                            channel.Received(null, container);
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
                return;
            }
            else if (channelId == (short)PreservedChannelId.Health)
            {
                return;
            }
            else
            {
                IDataChannel channel;

                if (dataChannelMap.TryGetValue(channelId, out channel))
                {
                    BytePacker packer = new BytePacker(data);

                    ComNode node;
                    if (nodeTcpMap.TryGetValue(endPoint, out node))
                    {
                        IDecrypter decrypter = null;
                        if (channel.Encryption == Encryption.Rsa) decrypter = rsaDecrypter;
                        else if (channel.Encryption == Encryption.Aes) decrypter = node.AesDecrypter;

                        object container = channel.FromStream(ref packer, decrypter);

                        channel.Received(node, container);
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
                //Util.Log("TCP:" + e.Message);
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

            if (channel.Encryption == Encryption.Rsa)
                throw new InvalidOperationException("Server cant send data via RSA channel.");

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

            channel.ToStream(data, ref packer, encrypter);

            bufferSize = (int)packer.Position;

            packer.Position = 0;
            packer.Write((short)(bufferSize - start));
        }

        public async Task<bool> Broadcast<T>(ComGroup group, short channelId, T data, ComNode exception = null)
        {
            IDataChannel channel;
            if (!dataChannelMap.TryGetValue(channelId, out channel)) return false;

            bool isRent = false;
            byte[] buffer = null;
            int bufferSize = 0;

            if (channel.Encryption == Encryption.None)
            {
                BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent, null);
            }

            foreach (var node in group)
            {
                if (node == exception) continue;

                if (!nodeTcpMap.ContainsKey(node.TcpEndPoint)) continue;

                if (channel.Encryption == Encryption.Aes)
                {
                    BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent, node.AesEncrypter);
                }

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
            if (!node.IsConnected) return false;
            else return await SendInternal<T>(node, channelId, data);
        }

        async Task<bool> SendInternal<T>(ComNode node, short channelId, T data)
        {
            return await Task.Run(async () =>
            {
                if (!nodeTcpMap.ContainsKey(node.TcpEndPoint)) return false;

                IDataChannel channel;

                if (!dataChannelMap.TryGetValue(channelId, out channel)) return false;

                bool isRent = false;
                byte[] buffer = null;
                int bufferSize = 0;

                IEncrypter encrypter = null;
                if (channel.Encryption == Encryption.Aes) encrypter = node.AesEncrypter;

                BuildBuffer(channel, data, ref buffer, ref bufferSize, ref isRent, encrypter);

                ComSnowballNode snode = (ComSnowballNode)node;
                if (channel.Qos == QosType.Reliable)
                {
                    await snode.Connection.Send(bufferSize, buffer).ConfigureAwait(false);
                }
                else if (channel.Qos == QosType.Unreliable)
                {
                    if (snode.UdpEndPoint != null)
                    {
                        await udpTerminal.Send(snode.Ip, snode.UdpEndPoint.Port, bufferSize, buffer).ConfigureAwait(false);
                    }
                }

                if (isRent) arrayPool.Return(buffer);

                return true;
            }).ConfigureAwait(false);

        }

    }

}

