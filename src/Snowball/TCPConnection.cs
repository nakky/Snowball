using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public class TCPConnection
    {
        TcpClient client;

        public string IP { get; private set; }
        public int Port { get; private set; }

        public const int DefaultBufferSize = 8192;

        byte[] receiveBuffer;

        NetworkStream nStream;

        public delegate void ReceiveHandler(string endPointIp, byte[] data, int size);
        public ReceiveHandler OnReceive;

        public delegate void DiconnectedHandler(TCPConnection connection);
        public DiconnectedHandler OnDisconnected;

        public TCPConnection(TcpClient client, int receiveBufferSize = DefaultBufferSize)
        {
            receiveBuffer = new byte[receiveBufferSize];

            UpdateClient(client);
            nStream = client.GetStream();
        }

        ~TCPConnection()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            lock(this){
                if (nStream != null)
                {
                    nStream = null;

                    client.Close();
                    client = null;

                    if (OnDisconnected != null) OnDisconnected(this);
                }
            }

        }

        internal void UpdateClient(TcpClient client)
        {
            this.client = client;
            this.IP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            this.Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
        }

        public bool IsConnected{ get{return client.Connected;} }

        public async Task Start()
        {
            int resSize = 0;

            do
            {
                try
                {
                    //Util.Log("Read");
                    resSize = await nStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

                    //if(resSize == receiveBuffer.Length) Util.Log("read " + resSize + " byte");
                }
                catch//(Exception e)
                {
                    break;
                }

                if (resSize == 0)
                {
                    break;
                }
                
                if (OnReceive != null) OnReceive(IP, receiveBuffer, resSize);

            } while (client.Connected);

            Disconnect();

        }


        public async Task Send(int size, byte[] data)
        {
            if (client == null || !client.Connected) return;
            await nStream.WriteAsync(data, 0, size);
        }

    }
}
