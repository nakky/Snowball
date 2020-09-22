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
        public string UserName { get; set; }
        public int HealthLostCount { get; set; }

        public byte[] TmpKey { get; set; }
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

