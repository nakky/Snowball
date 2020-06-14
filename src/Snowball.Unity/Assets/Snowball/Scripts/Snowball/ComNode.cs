using System;
using System.Net;

namespace Snowball
{

    public class ComNode
    {
        public ComNode(string ip)
        {
            this.IP = ip;
        }

        public string IP { get; private set; }
        public string UserName { get; set; }
        public int HealthLostCount { get; set; }
    }

    public class ComTCPNode : ComNode
    {
        public ComTCPNode(TCPConnection connection)
            : base(connection.IP)
        {
            this.Connection = connection;
        }

        public TCPConnection Connection { get; private set; }

    }

}

