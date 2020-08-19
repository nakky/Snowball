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

        public delegate void ConnectedHandler(string ip, TCPConnection connection);
        public ConnectedHandler OnConnected;

        int connectionBufferSize = 8192;
        public int ConnectionBufferSize { get { return connectionBufferSize; } set { connectionBufferSize = value; } }

        int connectTimeOutMilliSec = 1000;
        public int ConnectTimeOutMilliSec { get { return connectTimeOutMilliSec; } set { connectTimeOutMilliSec = value; } }

        public class CallbackParam
        {
            public CallbackParam(string ip, TCPConnection connection)
            {
                this.Ip = ip; this.Connection = connection;
            }
            public string Ip;
            public TCPConnection Connection;
        }

        public TCPConnector(int portNum)
        {
            this.portNum = portNum;
        }

        public async Task Connect(string ip)
        {
            TcpClient client = new TcpClient();
            client.SendBufferSize = connectionBufferSize;
            client.ReceiveBufferSize = connectionBufferSize;
            client.SendTimeout = DefaultSendTimeoutMs;
            client.ReceiveTimeout = DefaultReceiveTimeoutMs;

            try
            {
                Task con_task = client.ConnectAsync(ip, portNum);
                if (!con_task.Wait(connectTimeOutMilliSec))
                {
                    client.Close();
                    throw new SocketException(10060); // 10060:WSAETIMEDOUT
                }

            }
            catch (SocketException e)
            {
                if (Global.SyncContext != null)
                {
                    Global.SyncContext.Post((state) => {
                        CallbackParam param = (CallbackParam)state;
                        if (OnConnected != null) OnConnected(param.Ip, param.Connection);
                    }, new CallbackParam(ip, null));
                }
                else
                {
                    if (OnConnected != null) OnConnected(ip, null);
                }
                //Util.Log("SocketException");
            }
            catch (AggregateException e)
            {
                if (e.InnerException is SocketException)
                {
                    if (Global.SyncContext != null)
                    {
                        Global.SyncContext.Post((state) => {
                            CallbackParam param = (CallbackParam)state;
                            if (OnConnected != null) OnConnected(param.Ip, param.Connection);
                        }, new CallbackParam(ip, null));
                    }
                    else
                    {
                        if (OnConnected != null) OnConnected(ip, null);
                    }
                    //Util.Log("AggregateException");
                }
            }

            TCPConnection connection = new TCPConnection(client, connectionBufferSize);

            if (Global.SyncContext != null)
            {
                Global.SyncContext.Post((state) => {
                    CallbackParam param = (CallbackParam)state;
                    if (OnConnected != null) OnConnected(param.Ip, param.Connection);

                    connection.Start();

                }, new CallbackParam(ip, connection));
            }
            else
            {
                if (OnConnected != null) OnConnected(ip, connection);
                connection.Start();
            }
            

        }

    
    }
}
