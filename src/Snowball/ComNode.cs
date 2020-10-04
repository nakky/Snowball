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
        public int UserId { get; set; }
        public int HealthLostCount { get; set; }

        public bool IsConnected { get; set; }

        public byte[] TmpKey { get; set; }

        public bool IsDisconnecting { get; set; }

        public AesEncrypter AesEncrypter { get; set; }
        public AesDecrypter AesDecrypter { get; set; }

        public DateTime DisconnectedTimeStamp { get; set; }
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

