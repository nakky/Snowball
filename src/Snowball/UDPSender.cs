using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public class UDPSender
    {
        public const int DefaultBufferSize = 8192;

        int portNum;

        private UdpClient client;

        public UdpClient Client { get { return client; } }

        SemaphoreSlim locker = new SemaphoreSlim(1,1);

        public UDPSender(int portNum, int bufferSize = DefaultBufferSize)
        {
            this.portNum = portNum;
            client = new UdpClient();
            client.Client.SendBufferSize = bufferSize;
            client.Client.ReceiveBufferSize = bufferSize;

        }

        ~UDPSender()
        {
            Close();
        }

        public void Close()
        {
            if(client != null){
                client.Close();
                client = null;
            }
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            Socket socket = client.Client;
            socket.SetSocketOption(level, name, value);
        }

        public async Task Send(string ip, int size, byte[] data)
        {
#if false
            await client.SendAsync(data, size, ip, this.portNum);
#else
            try
            {
                await locker.WaitAsync();
                await client.SendAsync(data, size, ip, this.portNum);
            }
            finally
            {
                locker.Release();
            }
#endif
        }

    }
}
