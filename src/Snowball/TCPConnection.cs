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

        public const int DefaultBufferSize = 8192;

        byte[] receiveBuffer;
        BytePacker receivePacker;

        NetworkStream nStream;

        public delegate void ReceiveHandler(string endPointIp, short id, byte[] data, int dataSize);
        public ReceiveHandler OnReceive;

        public delegate void DiconnectedHandler(TCPConnection connection);
        public DiconnectedHandler OnDisconnected;

        public SynchronizationContext SyncContext { get; private set; }
        public static bool UseSyncContextPost = true;

        ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();

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
                        receivePacker.Position = 0;
                        resSize = receivePacker.ReadShort();
#if DISABLE_CHANNEL_VARINT
                        await nStream.ReadAsync(receiveBuffer, 0, 2).ConfigureAwait(false);
                        receivePacker.Position = 0;
                        channelId = receivePacker.ReadShort();
                        await nStream.ReadAsync(receiveBuffer, 0, resSize).ConfigureAwait(false);
#else
                        int s = 0;
                        channelId = VarintBitConverter.ToInt16(nStream, out s);
                        await nStream.ReadAsync(receiveBuffer, 0, resSize).ConfigureAwait(false);
#endif


                        buffer = arrayPool.Rent(resSize);
                        if(buffer != null)
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

                if (SyncContext != null)
                {
                    SyncContext.Post((state) => {
                        if (cancelToken.IsCancellationRequested) return;
                        CallbackParam param = (CallbackParam)state;
                        if (OnReceive != null) OnReceive(param.Ip, param.channelId, param.buffer, param.size);
                        if (isRent) arrayPool.Return(buffer);
                    }, new CallbackParam(IP, channelId, buffer, resSize, isRent));
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
