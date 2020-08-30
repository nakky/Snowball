using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public class UDPReceiver
    {
        public const int DefaultBufferSize = 8192;

        private UdpClient client;

        int portNum;

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

        public UDPReceiver(int portNum, int bufferSize = DefaultBufferSize)
        {
            this.portNum = portNum;
            client = new UdpClient(portNum);
            client.Client.SendBufferSize = bufferSize;
            client.Client.ReceiveBufferSize = bufferSize;
        }

        ~UDPReceiver()
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

        public async void Start()
        {
            IsActive = true;

            await ReceiveAsync();

        }

      
        public void Stop()
        {
            IsActive = false;
        }

        int numRequest = 0;

        CancellationTokenSource cancelToken = new CancellationTokenSource();

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
