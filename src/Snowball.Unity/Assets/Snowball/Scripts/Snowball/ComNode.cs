using System;
using System.Net;

namespace Snowball
{

    public class ComNode
    {
        public ComNode(IPEndPoint tcpEndPoint)
        {
            this.TcpEndPoint = tcpEndPoint;
            if (tcpEndPoint.Address.IsIPv4MappedToIPv6) TcpEndPoint.Address = tcpEndPoint.Address.MapToIPv4();
            Ip = TcpEndPoint.Address.ToString();
        }

        public string Ip { get; private set; }
        public IPEndPoint TcpEndPoint { get; private set; }
        public IPEndPoint UdpEndPoint { get; internal set; }
        public int UserId { get; internal set; }
        public int HealthLostCount { get; internal set; }

        public bool IsConnected { get; internal set; }

        public byte[] TmpKey { get; internal set; }

        public bool IsDisconnecting { get; internal set; }

        public AesEncrypter AesEncrypter { get; internal set; }
        public AesDecrypter AesDecrypter { get; internal set; }

        public DateTime DisconnectedTimeStamp { get; internal set; }
    }

    public class ComSnowballNode : ComNode
    {
        public ComSnowballNode(TCPConnection connection)
            : base((IPEndPoint)connection.Client.Client.RemoteEndPoint)
        {
            this.Connection = connection;
        }

        public TCPConnection Connection { get; private set; }
    }

}

