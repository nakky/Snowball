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

        public async Task Send(string ip, int size, byte[] data)
        {
            try
            {
                await locker.WaitAsync();
                await client.SendAsync(data, size, ip, this.portNum);
            }
            finally
            {
                locker.Release();
            }
        }

    }
}
