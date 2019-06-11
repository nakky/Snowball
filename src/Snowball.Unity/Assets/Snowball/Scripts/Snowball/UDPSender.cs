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

        public async Task Send(string ip, int size, byte[] data)
        {
            await client.SendAsync(data, size, ip, this.portNum);
        }

    }
}
