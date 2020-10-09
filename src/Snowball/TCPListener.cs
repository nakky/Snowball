using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public class TCPListener
    {
        public const int DefaultSendTimeoutMs = 2000;
        public const int DefaultReceiveTimeoutMs = 2000;

        int portNum;
        TcpListener listener;

        bool IsActive = false;

        public delegate void ConnectedHandler(TCPConnection connection);
        public ConnectedHandler OnConnected;

        int connectionBufferSize = 8192;
        public int ConnectionBufferSize { get { return connectionBufferSize; } set { connectionBufferSize = value; } }

        public SynchronizationContext SyncContext { get; set; }

        public class CallbackParam
        {
            public CallbackParam(TCPConnection connection)
            {
                this.Connection = connection;
            }
            public TCPConnection Connection;
        }

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
                TCPConnection connection = null;

                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    client.SendBufferSize = connectionBufferSize;
                    client.ReceiveBufferSize = connectionBufferSize;
                    client.SendTimeout = DefaultSendTimeoutMs;
                    client.ReceiveTimeout = DefaultReceiveTimeoutMs;

                    connection = new TCPConnection(client, connectionBufferSize);
                    connection.SyncContext = SyncContext;

                }
                catch //(Exception e)
                {
					if (OnConnected != null) OnConnected(null);
					//Util.Log(e.Message);
				}

                if (SyncContext != null)
                {
                    SyncContext.Post((state) => {
                        CallbackParam param = (CallbackParam)state;
                        if (OnConnected != null) OnConnected(param.Connection);

                        if(param.Connection != null) param.Connection.Start();

                    }, new CallbackParam(connection));
                }
                else
                {
                    if (OnConnected != null) OnConnected(connection);
                    if (connection != null) connection.Start();
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
