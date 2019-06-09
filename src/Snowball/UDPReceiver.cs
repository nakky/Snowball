using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using System.Threading.Tasks;

namespace Snowball
{
    public class UDPReceiver
    {
        private UdpClient client;

        int portNum;

        public delegate void ReceiveHandler(string endPointIp, byte[] data, int size);

        public ReceiveHandler OnReceive;

        public bool IsActive { get; private set; }

        public UDPReceiver(int portNum)
        {
            this.portNum = portNum;
            client = new UdpClient(portNum);
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

        public async Task ReceiveAsync()
        {
            while (IsActive)
            {
                try
                {
                    var result = await client.ReceiveAsync();
                    if (!IsActive) break;

                    if (OnReceive != null) OnReceive(result.RemoteEndPoint.Address.ToString(), result.Buffer, result.Buffer.Length);

                }
                catch(Exception e)
                {
                    Util.Log("UDPReceiver:" + e.Message);
                    break;
                }

            }

        }

    }
}
