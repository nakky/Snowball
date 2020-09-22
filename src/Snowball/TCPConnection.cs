
using System;
using System.IO;
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

        public IPEndPoint EndPoint { get; private set; }

        public TcpClient Client { get { return client; } }

        public string IP { get; private set; }
        public int Port { get; private set; }

        int sendTimeOut = 5000;
        public int SendTimeOut
        {
            get
            {
                return sendTimeOut;
            }
            set
            {
                sendTimeOut = value;
                if (nStream != null) nStream.WriteTimeout = sendTimeOut;
            }
        }

        int receiveTimeOut = 5000;
        public int ReceiveTimeOut
        {
            get
            {
                return receiveTimeOut;
            }
            set
            {
                receiveTimeOut = value;
                if (nStream != null) nStream.ReadTimeout = receiveTimeOut;
            }
        }

        public const int DefaultBufferSize = 8192;

        byte[] receiveBuffer;
        BytePacker receivePacker;

        NetworkStream nStream;

        public delegate Task<bool> PollHandler(
            TCPConnection connection,
            NetworkStream nStream,
            byte[] receiveBuffer,
            BytePacker receivePacker,
            CancellationTokenSource cancelToken
            );
        public PollHandler OnPoll;

        public delegate void DiconnectedHandler(TCPConnection connection);
        public DiconnectedHandler OnDisconnected;


        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

        SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        CancellationTokenSource cancelToken = new CancellationTokenSource();

        public TCPConnection(TcpClient client, int receiveBufferSize = DefaultBufferSize)
        {
            receiveBuffer = new byte[receiveBufferSize];
            receivePacker = new BytePacker(receiveBuffer);

            UpdateClient(client);
            nStream = client.GetStream();
            nStream.WriteTimeout = sendTimeOut;
            nStream.ReadTimeout = receiveTimeOut;

            EndPoint = (IPEndPoint)client.Client.RemoteEndPoint;

        }

        ~TCPConnection()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            lock (this)
            {
                try
                {
                    if (!cancelToken.IsCancellationRequested)
                    {
                        cancelToken.Cancel();
                        OnPoll = null;

                        nStream.Close();
                        client.Close();

                        if (Global.SyncContext != null)
                        {
                            Global.SyncContext.Post((state) =>
                            {
                                if (OnDisconnected != null) OnDisconnected(this);
                            }, null);
                        }
                        else
                        {
                            if (OnDisconnected != null) OnDisconnected(this);
                        }

                    }
                }
                catch//(Exception e)
                {
                    //Util.Log("Disconnect" + e.Message);
                }

            }

        }

        internal void UpdateClient(TcpClient client)
        {
            this.client = client;
            this.IP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            this.Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            Socket socket = client.Client;
            socket.SetSocketOption(level, name, value);
        }

        public bool IsConnected { get { return client.Connected; } }

        public async Task Start()
        {
            cancelToken.Token.Register(() => client.Close());

            do
            {
                if(OnPoll != null)
                {
                    var ret = await OnPoll(this, nStream, receiveBuffer, receivePacker, cancelToken);
                    if (!ret) break;
                }
                
            } while (client.Connected);

            Disconnect();
        }


        public async Task Send(int size, byte[] data)
        {
            if (client == null || !client.Connected) return;

            try
            {
                await locker.WaitAsync();
                await nStream.WriteAsync(data, 0, size);
            }
            finally
            {
                locker.Release();
            }

        }

    }
}
