using System;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;

namespace Snowball
{
    public class UDPTerminal
    {
        public const int DefaultBufferSize = 8192;

        UdpClient client;

        SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        CancellationTokenSource cancelToken = new CancellationTokenSource();


        public UDPTerminal(int bufferSize = DefaultBufferSize)
        {
            client = new UdpClient();
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SendBufferSize = bufferSize;
            client.Client.ReceiveBufferSize = bufferSize;
        }

        public UDPTerminal(int port, int bufferSize = DefaultBufferSize)
        {
            client = new UdpClient(port);
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SendBufferSize = bufferSize;
            client.Client.ReceiveBufferSize = bufferSize;
        }

        ~UDPTerminal()
        {
            Close();
        }

        public void Close()
        {
            if (client != null)
            {
                IsActive = false;
                cancelToken.Cancel();
                OnReceive = null;
                client.Close();
            }
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            Socket socket = client.Client;
            socket.SetSocketOption(level, name, value);
        }

        public void Connect(string ip, int port)
        {
            client.Connect(ip, port);
        }

        public async Task Send(string ip, int port, int size, byte[] data)
        {
#if false
            await client.SendAsync(data, size, ip, port);
#else
            try
            {
                await locker.WaitAsync();
                await client.SendAsync(data, size, ip, port);
            }
            finally
            {
                locker.Release();
            }
#endif
        }

        public delegate void ReceiveHandler(IPEndPoint endPoint, byte[] data, int size);

        public ReceiveHandler OnReceive;

        public bool IsActive { get; private set; }

        public class CallbackParam
        {
            public CallbackParam(IPEndPoint endPoint, byte[] buffer, int size)
            {
                this.endPoint = endPoint; this.buffer = buffer; this.size = size;
            }
            public IPEndPoint endPoint;
            public byte[] buffer;
            public int size;
        }

        public async void ReceiveStart()
        {
            IsActive = true;

            await ReceiveAsync();

        }


        public void ReceiveStop()
        {
            IsActive = false;
        }

        public async Task ReceiveAsync()
        {
            while (IsActive)
            {
                try
                {
                    var result = await client.ReceiveAsync().ConfigureAwait(false);
                    if (!IsActive) break;
                    if (cancelToken.IsCancellationRequested) break;

                    if (Global.SyncContext != null)
                    {
                        Global.SyncContext.Post((state) =>
                        {
                            if (cancelToken.IsCancellationRequested) return;
                            CallbackParam param = (CallbackParam)state;
                            if (OnReceive != null) OnReceive(param.endPoint, param.buffer, param.size);
                        }, new CallbackParam(result.RemoteEndPoint, result.Buffer, result.Buffer.Length));
                    }
                    else
                    {
                        if (OnReceive != null) OnReceive(result.RemoteEndPoint, result.Buffer, result.Buffer.Length);
                    }
                }
                catch//(Exception e)
                {
                    //Util.Log("UDPReceiver:" + e.Message);
                    break;
                }

            }

        }
    }
}
