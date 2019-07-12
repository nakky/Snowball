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

        public string IP { get; private set; }
        public int Port { get; private set; }

        public const int DefaultBufferSize = 8192;

        byte[] receiveBuffer;

        NetworkStream nStream;

        public delegate void ReceiveHandler(string endPointIp, short id, byte[] data, int dataSize);
        public ReceiveHandler OnReceive;

        public delegate void DiconnectedHandler(TCPConnection connection);
        public DiconnectedHandler OnDisconnected;

        public SynchronizationContext SyncContext { get; private set; }
        public static bool UseSyncContextPost = true;

        public class CallbackParam
        {
            public CallbackParam(string ip, short channelId, byte[] buffer, int size)
            {
                this.Ip = ip; this.channelId = channelId; this.buffer = buffer; this.size = size;
            }
            public string Ip;
            public short channelId;
            public byte[] buffer;
            public int size;
        }

        public TCPConnection(TcpClient client, int receiveBufferSize = DefaultBufferSize)
        {
            receiveBuffer = new byte[receiveBufferSize];
            
            UpdateClient(client);
            nStream = client.GetStream();

            if (UseSyncContextPost) SyncContext = SynchronizationContext.Current;
            else SyncContext = null;
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
                    cancelToken.Cancel();
                    OnReceive = null;

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

        CancellationTokenSource cancelToken = new CancellationTokenSource();

        public async Task Start()
        {
            int resSize = 0;
            short channelId = 0;

            bool isRent = false;
            byte[] buffer = null;

            do
            {
                try
                {
                    resSize = await nStream.ReadAsync(receiveBuffer, 0, 2).ConfigureAwait(false);
                    if (resSize != 0)
                    {
                        resSize = BitConverter.ToInt16(receiveBuffer, 0);
#if DISABLE_CHANNEL_VARINT
                        await nStream.ReadAsync(receiveBuffer, 0, 2).ConfigureAwait(false);
                        channelId = BitConverter.ToInt16(receiveBuffer, 0);
                        await nStream.ReadAsync(receiveBuffer, 0, resSize).ConfigureAwait(false);
#else
                        int s = 0;
                        channelId = VarintBitConverter.ToInt16(nStream, out s);
                        await nStream.ReadAsync(receiveBuffer, 0, resSize).ConfigureAwait(false);
#endif

                        buffer = new byte[resSize];
                        Array.Copy(receiveBuffer, buffer, resSize);

                    }
                }
                catch//(Exception e)
                {
                    break;
                }

                if (resSize == 0)
                {
                    break;
                }

                if (cancelToken.IsCancellationRequested) break;

                if (SyncContext != null)
                {
                    SyncContext.Post((state) => {
                        if (cancelToken.IsCancellationRequested) return;
                        CallbackParam param = (CallbackParam)state;
                        if (OnReceive != null) OnReceive(param.Ip, param.channelId, param.buffer, param.size);
                    }, new CallbackParam(IP, channelId, buffer, resSize));
                }
                else
                {
                    if (OnReceive != null) OnReceive(IP, channelId, receiveBuffer, resSize);
                }
                

            } while (client.Connected);

            if (SyncContext != null)
            {
                SyncContext.Post((state) => {
                    Disconnect();
                }, null);
            }
            else
            {
                Disconnect();
            }
            

        }


        public async Task Send(int size, byte[] data)
        {
            if (client == null || !client.Connected) return;
            await nStream.WriteAsync(data, 0, size);
        }

    }
}
