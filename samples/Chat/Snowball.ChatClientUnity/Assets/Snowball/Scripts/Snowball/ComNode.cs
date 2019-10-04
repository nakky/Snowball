using System;
using System.Net;

namespace Snowball
{

    public class ComNode
    {
        public ComNode(TCPConnection connection)
        {
            this.Connection = connection;
        }

        public TCPConnection Connection { get; private set; }
        public string IP { get { return Connection.IP; } }
        public string UserName { get; set; }
        public int HealthLostCount { get; set; }
    }

}

