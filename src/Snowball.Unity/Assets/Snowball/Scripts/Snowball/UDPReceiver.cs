﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public class UDPReceiver : IDisposable
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

        public UDPReceiver(UdpClient client, int bufferSize = DefaultBufferSize)
        {
            this.portNum = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            this.client = client;
            this.client.Client.SendBufferSize = bufferSize;
            this.client.Client.ReceiveBufferSize = bufferSize;
        }

        public void Dispose()
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
                client = null;
            }
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            Socket socket = client.Client;
            socket.SetSocketOption(level, name, value);
        }

        public async void Start()
        {
            IsActive = true;

            await ReceiveAsync().ConfigureAwait(false);

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
