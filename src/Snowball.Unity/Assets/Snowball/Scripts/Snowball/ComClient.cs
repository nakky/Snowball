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

        public bool IsConnected { get { lock (this) { return serverNode != null; } } }

        public ComClient()
        {
            IsOpened = false;

            AddChannel(new DataChannel<string>((short)PreservedChannelId.Login, QosType.Reliable, Compression.None, (endPointIp, deserializer) =>
            {
            }));

            AddChannel(new DataChannel<byte>((short)PreservedChannelId.Health, QosType.Unreliable, Compression.None, (endPointIp, data) => { }));
        }

        public void Dispose()
        {
            Close();
        }

        public virtual void Open()
        {
            if (IsOpened) return;

            if (Global.UseSyncContextPost && Global.SyncContext == null)
                Global.SyncContext = SynchronizationContext.Current;

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

                if (IsConnected)
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
                serverNode = new ComTCPNode(connection);

                connection.OnDisconnected = OnDisconnectedInternal;
                connection.OnPoll = OnPoll;

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
                ((ComTCPNode)serverNode).Connection.Disconnect();
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
                short channelId = VarintBitConverter.ToShort(packer, out s);
#endif

                if (channelId == (short)PreservedChannelId.Beacon)
                {
                    if (acceptBeacon && !isConnecting && !IsConnected)
                    {
                        string beaconData = (string)DataSerializer.Deserialize<string>(packer);

                        if (BeaconAccept(beaconData)) Connect(endPointIp);
                    }
                }
                else if (channelId == (short)PreservedChannelId.Health)
                {
                    if (serverNode == null) break;
                    if (endPointIp == serverNode.IP)
                    {
                        healthLostCount = 0;
                    }
                }
                else if (!dataChannelMap.ContainsKey(channelId))
                {
                }
                else
                {
                    IDataChannel channel = dataChannelMap[channelId];

                    if(channel.CheckMode == CheckMode.Sequre)
                    {
                        if (serverNode == null) break;
                        if (endPointIp == serverNode.IP)
                        {
                            healthLostCount = 0;

                            object container = channel.FromStream(ref packer);

                            channel.Received(serverNode, container);
                        }
                    }
                    else
                    {
                        healthLostCount = 0;

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
            else if (channelId == (short)PreservedChannelId.Health)
            {
                if (serverNode == null) ;
                if (endPointIp == serverNode.IP)
                {
                    healthLostCount = 0;
                }
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
                    if (endPointIp == serverNode.IP)
                    {
                        healthLostCount = 0;

                        object container = channel.FromStream(ref packer);

                        channel.Received(serverNode, container);
                    }
                }
                else
                {
                    healthLostCount = 0;

                    object container = channel.FromStream(ref packer);

                    channel.Received(null, container);
                }
            }
        }

        public class CallbackParam
        {
            public CallbackParam(string ip, short channelId, byte[] buffer, int size, bool isRent)
            {
                this.Ip = ip; this.channelId = channelId; this.buffer = buffer; this.size = size; this.isRent = isRent;
            }
            public string Ip;
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
                    OnTCPReceived(param.Ip, param.channelId, param.buffer, param.size);
                    if (isRent) arrayPool.Return(buffer);
                }, new CallbackParam(connection.IP, channelId, buffer, resSize, isRent));
            }
            else
            {
                OnTCPReceived(connection.IP, channelId, buffer, resSize);
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
                    await ((ComTCPNode)serverNode).Connection.Send(bufferSize, buffer);
                }
                else if (channel.Qos == QosType.Unreliable)
                {
                    await udpSender.Send(serverNode.IP, bufferSize, buffer);
                }

                if (isRent) arrayPool.Return(buffer);

                return true;
            });

        }
    }

}
