using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Timers;
using System.Threading.Tasks;
using System.Net.Sockets;

using MessagePack;

namespace Snowball
{

    public class ComServer : IDisposable
    {
        public bool IsOpened { get; protected set; }

        int listenPortNumber = 59901;
        public int ListenPortNumber { get { return listenPortNumber; } set { if (!IsOpened)listenPortNumber = value; } }

        int sendPortNumber = 59902;
        public int SendPortNumber { get { return sendPortNumber; } set { if (!IsOpened) sendPortNumber = value; } }

        int maxPacketSize = 4096;
        public int MaxPacketSize { get { return maxPacketSize; } set { if (!IsOpened) maxPacketSize = value; } }

        protected BroadcastInfo broadcastInfo = new BroadcastInfo(1, 0);
        public BroadcastInfo BroadcastInfo { get { return broadcastInfo; } set { if (!IsOpened) broadcastInfo = value; } }

        public delegate void ConnectedHandler(ComNode node);
        public ConnectedHandler OnConnected;

        public delegate void DisconnectedHandler(ComNode node);
        public DisconnectedHandler OnDisconnected;

        protected Dictionary<short, DataChannel> dataChannelMap = new Dictionary<short, DataChannel>();

        protected Dictionary<string, ComNode> nodeMap = new Dictionary<string, ComNode>();
        protected Dictionary<ComNode, TCPConnection> connectionMap = new Dictionary<ComNode, TCPConnection>();

        public delegate string BeaconDataGenerateFunc(BroadcastInfo info);
        BeaconDataGenerateFunc BeaconDataCreate = (info) => {
            return "{\"major\"=" + info.Measure + "\"minor\"=" + info.Minor + "}"; 
            };

        public void SetBeaconDataCreateFunction(BeaconDataGenerateFunc func) { BeaconDataCreate = func; }

        UDPSender udpSender;
        UDPReceiver udpReceiver;

        TCPListener tcpListener;


        protected int beaconIntervalMs = 1000;
        public int BeaconIntervalMs { get { return beaconIntervalMs; } set { if (!IsOpened) beaconIntervalMs = value; } }
        Timer beaconTimer;

        int maxHealthLostCount = 5;
        public int MaxHealthLostCount { get { return maxHealthLostCount; } set { maxHealthLostCount = value; } }

        List<string> beaaconList = new List<string>();

        public ComServer()
        {
            IsOpened = false;

            AddChannel(new DataChannel((short)PreservedChannelId.Login, QosType.Reliable, CompressionType.None, (node, data) =>
            {
                node.UserName = (string)data;

                if (OnConnected != null) OnConnected(node);

                Util.Log("SetUsername:" + node.UserName);
            }));

            AddChannel(new DataChannel((short)PreservedChannelId.Health, QosType.Reliable, CompressionType.None, (node, data) =>
            {
                node.HealthLostCount = 0;
            }));
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

            udpSender = new UDPSender(sendPortNumber);
            udpReceiver = new UDPReceiver(listenPortNumber);

            tcpListener = new TCPListener(listenPortNumber);
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

        void OnBeaconTimer(object sender, ElapsedEventArgs args)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((short)0);
            writer.Write((short)PreservedChannelId.Beacon);

            int pos = (int)stream.Position;

            string data = BeaconDataCreate(broadcastInfo);

            MessagePackSerializer.Serialize(stream, data);

            int maxpos = (int)stream.Position;

            stream.Position = 0;
            writer.Write((short)(maxpos - pos));

            foreach(var ip in beaaconList)
            {
                udpSender.Send(ip, maxpos, stream.GetBuffer());
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
                    SendData(keypair.Value, (short)PreservedChannelId.Health, dummy);
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


        public void AddChannel(DataChannel channel)
        {
            dataChannelMap.Add(channel.ChannelID, channel);
        }

        public void RemoveChannel(DataChannel channel)
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
                short datasize = BitConverter.ToInt16(data, head + 0);
                short channelId = BitConverter.ToInt16(data, head + 2);

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

                    DataChannel channel = dataChannelMap[channelId];
                    MemoryStream stream = new MemoryStream(data, head + 4, (int)datasize);

                    object container = null;

                    if (channel.Compression == CompressionType.LZ4)
                    {
                        container = LZ4MessagePackSerializer.Typeless.Deserialize(stream);
                    }
                    else
                    {
                        container = MessagePackSerializer.Typeless.Deserialize(stream);
                    }

                    channel.Received(node, container);
                }

                head += datasize + 4;
            }
        }

        void OnTCPReceived(string endPointIp, byte[] data, int size)
        {
            int head = 0;

            while (head < size)
            {
                short datasize = BitConverter.ToInt16(data, head + 0);
                short channelId = BitConverter.ToInt16(data, head + 2);

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

                    DataChannel channel = dataChannelMap[channelId];
                    MemoryStream stream = new MemoryStream(data, head + 4, (int)datasize);

                    object container = null;

                    if (channel.Compression == CompressionType.LZ4)
                    {
                        container = LZ4MessagePackSerializer.Typeless.Deserialize(stream);
                    }
                    else
                    {
                        container = MessagePackSerializer.Typeless.Deserialize(stream);
                    }

                    channel.Received(node, container);
                }

                head += datasize + 4;
            }

        }

        public bool Broadcast<T>(ComGroup group, short channelId, T data)
        {
            bool status = true;

            foreach(var node in group.NodeList)
            {
                if(!SendData(node, channelId, data))
                {
                    status = false;
                }
            }
            return status;
        }

        public bool SendData<T>(ComNode node, short channelId, T data)
        {
            if (!dataChannelMap.ContainsKey(channelId)) return false;

            DataChannel channel = dataChannelMap[channelId];

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((short)0);
            writer.Write(channelId);

            int pos = (int)stream.Position;

            if (channel.Compression == CompressionType.LZ4)
            {
                LZ4MessagePackSerializer.Typeless.Serialize(stream, data);
            }
            else
            {
                MessagePackSerializer.Typeless.Serialize(stream, data);
            }

            int maxpos = (int)stream.Position;

            stream.Position = 0;

            writer.Write((short)(maxpos - pos));

            if (channel.Qos == QosType.Reliable)
            {
                TCPConnection connection = connectionMap[node];
                connection.Send(maxpos, stream.GetBuffer());
            }
            else if(channel.Qos == QosType.Unreliable)
            {
                udpSender.Send(node.IP, maxpos, stream.GetBuffer());
            }

            return true;
        }

       
    }

}

