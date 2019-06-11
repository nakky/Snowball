using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Snowball
{
    public class TCPConnector
    {
        public const int DefaultSendTimeoutMs = 200;
        public const int DefaultReceiveTimeoutMs = 2000;

        int portNum;

        public delegate void ConnectedHandler(TCPConnection connection);
        public ConnectedHandler OnConnected;

        int connectionBufferSize = 8192;
        public int ConnectionBufferSize { get { return connectionBufferSize; } set { connectionBufferSize = value; } }

        public TCPConnector(int portNum)
        {
            this.portNum = portNum;
        }

        public async void Connect(string ip)
        {            
            TcpClient client = new TcpClient();
            client.SendBufferSize = connectionBufferSize;
            client.ReceiveBufferSize = connectionBufferSize;
            client.SendTimeout = DefaultSendTimeoutMs;
            client.ReceiveTimeout = DefaultReceiveTimeoutMs;

            await client.ConnectAsync(ip, portNum);

            TCPConnection connection = new TCPConnection(client, connectionBufferSize);

            if (OnConnected != null) OnConnected(connection);
            await connection.Start();
        }
    }
}
