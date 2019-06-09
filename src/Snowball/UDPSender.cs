using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Snowball
{
    public class UDPSender
    {
        int portNum;

        public const int DefaultBufferSize = 1024;

        private UdpClient client;

        public UDPSender(int portNum)
        {
            this.portNum = portNum;
            client = new UdpClient();
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

        byte[] buffer = new byte[DefaultBufferSize];

        public async Task Send(string ip, int size, byte[] data)
        {
            await client.SendAsync(data, size, ip, this.portNum);
        }

    }
}
