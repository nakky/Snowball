using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Snowball
{
    public class TCPListener
    {
        public const int DefaultSendTimeoutMs = 200;
        public const int DefaultReceiveTimeoutMs = 2000;

        int portNum;
        TcpListener listener;

        bool IsActive = false;

        public delegate void ConnectedHandler(TCPConnection connection);
        public ConnectedHandler OnConnected;

        int connectionBufferSize = 8192;
        public int ConnectionBufferSize { get { return connectionBufferSize; } set { connectionBufferSize = value; } }

        public TCPListener(int portNum)
        {
            this.portNum = portNum;
            listener = new TcpListener(IPAddress.Any, portNum);
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            Socket listenerSocket = listener.Server;
            listenerSocket.SetSocketOption(level, name, value);
        }

        public async void Start()
        {
            IsActive = true;

            listener.Start();


            while (IsActive)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    client.SendBufferSize = connectionBufferSize;
                    client.ReceiveBufferSize = connectionBufferSize;
                    client.SendTimeout = DefaultSendTimeoutMs;
                    client.ReceiveTimeout = DefaultReceiveTimeoutMs;

                    TCPConnection connection = new TCPConnection(client, connectionBufferSize);

                    if (OnConnected != null) OnConnected(connection);

                    connection.Start();
                }
                catch //(Exception e)
                {
					if (OnConnected != null) OnConnected(null);
					//Util.Log(e.Message);
				}
            }

        }
    

        public void Stop()
        {
            IsActive = false;
            try
            {
                listener.Stop();
            }
            catch //(Exception e)
            {
                //Util.Log("Stop:" + e.Message);
            }
        }

    }
}
