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

        public delegate void ReceiveHandler(string endPointIp, byte[] data, int size);

        public ReceiveHandler OnReceive;

        public bool IsActive { get; private set; }

        SynchronizationContext syncContext;

        public UDPReceiver(int portNum, int bufferSize = DefaultBufferSize)
        {
            this.portNum = portNum;
            client = new UdpClient(portNum);
            client.Client.SendBufferSize = bufferSize;
            client.Client.ReceiveBufferSize = bufferSize;

#if UNITY_2019_1_OR_NEWER
            syncContext = SynchronizationContext.Current;
#else
            syncContext = null;
#endif
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
                syncContext = null;
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

                    if (syncContext != null)
                    {
                        syncContext.Post((state) =>
                        {
                            if (cancelToken.IsCancellationRequested) return;

                            if (OnReceive != null) OnReceive(result.RemoteEndPoint.Address.ToString(), result.Buffer, result.Buffer.Length);
                        }, null);
                    }
                    else
                    {
                        if (OnReceive != null) OnReceive(result.RemoteEndPoint.Address.ToString(), result.Buffer, result.Buffer.Length);
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
