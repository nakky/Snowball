//#define DISABLE_CHANNEL_VARINT

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

        public delegate void ReceiveHandler(string endPointIp, short id, byte[] data, int dataSize);
        public ReceiveHandler OnReceive;

        public delegate void DiconnectedHandler(TCPConnection connection);
        public DiconnectedHandler OnDisconnected;


        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

        SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        public class CallbackParam
        {
            public CallbackParam(string ip, short channelId, byte[] buffer, int size, bool isRent)
            {
                this.Ip = ip; this.channelId = channelId; this.buffer = buffer; this.size = size; this.isRent = isRent;
            }
            public string Ip;
            public short channelId;
            public byte[] buffer;
            public int size;
            public bool isRent;
        }

        public TCPConnection(TcpClient client, int receiveBufferSize = DefaultBufferSize)
        {
            receiveBuffer = new byte[receiveBufferSize];
            receivePacker = new BytePacker(receiveBuffer);


            UpdateClient(client);
            nStream = client.GetStream();
            nStream.WriteTimeout = sendTimeOut;
            nStream.ReadTimeout = receiveTimeOut;

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
                        OnReceive = null;

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

        public bool IsConnected { get { return client.Connected; } }

        CancellationTokenSource cancelToken = new CancellationTokenSource();

        public async Task Start()
        {
            cancelToken.Token.Register(() => client.Close());
            int resSize = 0;
            short channelId = 0;

            bool isRent = false;
            byte[] buffer = null;

            do
            {
                try
                {
                    resSize = await nStream.ReadAsync(receiveBuffer, 0, 2, cancelToken.Token).ConfigureAwait(false);

                    if (resSize != 0)
                    {
                        receivePacker.Position = 0;
                        resSize = receivePacker.ReadShort();
#if DISABLE_CHANNEL_VARINT
                        await nStream.ReadAsync(receiveBuffer, 0, 2, cancelToken.Token).ConfigureAwait(false);
                        receivePacker.Position = 0;
                        channelId = receivePacker.ReadShort();
                        await nStream.ReadAsync(receiveBuffer, 0, resSize, cancelToken.Token).ConfigureAwait(false);
#else
                        int s = 0;
                        channelId = VarintBitConverter.ToShort(nStream, out s);
                        await nStream.ReadAsync(receiveBuffer, 0, resSize, cancelToken.Token).ConfigureAwait(false);
#endif


                        buffer = arrayPool.Rent(resSize);
                        if (buffer != null)
                        {
                            isRent = true;
                        }
                        else
                        {
                            buffer = new byte[resSize];
                            isRent = false;
                        }

                        Array.Copy(receiveBuffer, buffer, resSize);

                        //Util.Log("TCP:" + resSize);
                    }
                }
                catch//(Exception e)
                {
                    //Util.Log("TCP:" + e.Message);
                    break;
                }

                if (resSize == 0)
                {
                    break;
                }

                if (cancelToken.IsCancellationRequested) break;

                if (Global.SyncContext != null)
                {
                    Global.SyncContext.Post((state) =>
                    {
                        if (cancelToken.IsCancellationRequested) return;
                        CallbackParam param = (CallbackParam)state;
                        if (OnReceive != null) OnReceive(param.Ip, param.channelId, param.buffer, param.size);
                        if (isRent) arrayPool.Return(buffer);
                    }, new CallbackParam(IP, channelId, buffer, resSize, isRent));
                }
                else
                {
                    if (OnReceive != null) OnReceive(IP, channelId, buffer, resSize);
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
