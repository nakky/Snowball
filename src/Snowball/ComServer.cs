//#define DISABLE_CHANNEL_VARINT

using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Timers;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Snowball
{

    public class ComServer : IDisposable
    {
        public bool IsOpened { get; protected set; }

        int listenPortNumber = 59901;
        public int ListenPortNumber { get { return listenPortNumber; } set { if (!IsOpened)listenPortNumber = value; } }

        int sendPortNumber = 59902;
        public int SendPortNumber { get { return sendPortNumber; } set { if (!IsOpened) sendPortNumber = value; } }

        int bufferSize = 8192;
        public int BufferSize { get { return bufferSize; } set { if (!IsOpened) bufferSize = value; } }

        public delegate void ConnectedHandler(ComNode node);
        public ConnectedHandler OnConnected;

        public delegate void DisconnectedHandler(ComNode node);
        public DisconnectedHandler OnDisconnected;

        protected Dictionary<short, IDataChannel> dataChannelMap = new Dictionary<short, IDataChannel>();

        protected Dictionary<string, ComNode> nodeMap = new Dictionary<string, ComNode>();
        protected Dictionary<ComNode, TCPConnection> connectionMap = new Dictionary<ComNode, TCPConnection>();

        public delegate string BeaconDataGenerateFunc();
        BeaconDataGenerateFunc BeaconDataCreate = () => {
            return "Snowball"; 
            };

        public void SetBeaconDataCreateFunction(BeaconDataGenerateFunc func) { BeaconDataCreate = func; }

        UDPSender udpSender;
        UDPReceiver udpReceiver;

        TCPListener tcpListener;


        protected int beaconIntervalMs = 1000;
        public int BeaconIntervalMs { get { return beaconIntervalMs; } set { if (!IsOpened) beaconIntervalMs = value; } }
        Timer beaconTimer;

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

                if (OnConnected != null) OnConnected(node);

                Util.Log("SetUsername:" + node.UserName);
            }));

            AddChannel(new DataChannel<byte>((short)PreservedChannelId.Health, QosType.Unreliable, Compression.None, (node, data) =>
            {
                node.HealthLostCount = 0;
            }));

            beaconConverter = DataSerializer.GetConverter(typeof(string));
        }

        public void Dispose()
        {
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

            udpSender = new UDPSender(sendPortNumber, bufferSize);
            udpReceiver = new UDPReceiver(listenPortNumber, bufferSize);

            tcpListener = new TCPListener(listenPortNumber);
            tcpListener.ConnectionBufferSize = bufferSize;
            tcpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            tcpListener.OnConnected += OnConnectedInternal;

            beaconTimer = new Timer(BeaconIntervalMs);

            udpReceiver.OnReceive += OnUDPReceived;

            beaconTimer.Elapsed += OnBeaconTimer;

            tcpListener.Start();
            udpReceiver.Start();

            IsOpened = true;

            HealthCheck();
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

            udpSender.Send(ip, length, packer.Buffer);
        }

        void OnBeaconTimer(object sender, ElapsedEventArgs args)
        {
            BytePacker packer;
            int length = CreateBeaconData(out packer);

            foreach (var ip in beaaconList)
            {
                udpSender.Send(ip, length, packer.Buffer);
            }
        }

        public void Close()
        {
            if (!IsOpened) return;

            beaconTimer.Stop();

            var cMap = new Dictionary<ComNode, TCPConnection>(connectionMap);
            foreach (var connection in cMap)
            {
                connection.Value.Disconnect();
            }

            tcpListener.Stop();
            udpReceiver.Close();

            beaconTimer = null;

            tcpListener = null;
            udpReceiver = null;
            udpSender = null;

            IsOpened = false;
        }

        public void BeaconStart()
        {
            beaconTimer.Start();
        }

        public void BeaconStop()
        {
            beaconTimer.Stop();
        }

        public ComNode GetNodeByIp(string ip)
        {
            if (nodeMap.ContainsKey(ip))
            {
                return nodeMap[ip];
            }
            return null;
        }

        public async Task HealthCheck()
        {
            while (IsOpened)
            {
                await Task.Delay(500);

                List<ComNode> invalidNodeArray = new List<ComNode>();

                byte dummy = 0;
                foreach(var keypair in nodeMap)
                {
                    Send(keypair.Value, (short)PreservedChannelId.Health, dummy);
                    keypair.Value.HealthLostCount++;
                    if (keypair.Value.HealthLostCount > MaxHealthLostCount)
                    {
                        invalidNodeArray.Add(keypair.Value);
                    }
                }

                foreach (var node in invalidNodeArray)
                {
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
            ComNode node = new ComNode(connection.IP);

            nodeMap.Add(node.IP, node);
            connectionMap.Add(node, connection); 

            connection.OnDisconnected = OnDisconnectedInternal;
            connection.OnReceive = OnTCPReceived;

            Util.Log("Server:Connected");
        }

        public bool Disconnect(ComNode node)
        {
            if (connectionMap.ContainsKey(node))
            {
                TCPConnection connection = connectionMap[node];
                connection.Disconnect();
                return true;
            }
            else return false;
        }

        void OnDisconnectedInternal(TCPConnection connection)
        {
            if (nodeMap.ContainsKey(connection.IP))
            {
                ComNode node = nodeMap[connection.IP];
                connectionMap.Remove(node);
                nodeMap.Remove(connection.IP);

                if (OnDisconnected != null) OnDisconnected(node);

                Util.Log("Server:Disconnected");
            }
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
                }
                else if (!dataChannelMap.ContainsKey(channelId))
                {
                }
                else
                {
                    if (!nodeMap.ContainsKey(endPointIp)) continue;

                    ComNode node = nodeMap[endPointIp];

                    IDataChannel channel = dataChannelMap[channelId];

                    object container = channel.FromStream(ref packer);

                    channel.Received(node, container);
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

                if (!nodeMap.ContainsKey(endPointIp)) return;
                ComNode node = nodeMap[endPointIp];

                IDataChannel channel = dataChannelMap[channelId];

                object container = channel.FromStream(ref packer);

                channel.Received(node, container);
            }
        }


        public async Task<bool> Broadcast<T>(ComGroup group, short channelId, T data)
        {
            bool status = true;

            foreach(var node in group.NodeList)
            {
                if(! await Send(node, channelId, data))
                {
                    status = false;
                }
            }
            return status;
        }

        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

        public async Task<bool> Send<T>(ComNode node, short channelId, T data)
        {
            if (!connectionMap.ContainsKey(node)) return false;

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
                TCPConnection connection = connectionMap[node];
                await connection.Send(maxpos, buf);
            }
            else if(channel.Qos == QosType.Unreliable)
            {
                await udpSender.Send(node.IP, maxpos, buf);
            }

            if (isRent) arrayPool.Return(buf);

            return true;
        }

       
    }

}

