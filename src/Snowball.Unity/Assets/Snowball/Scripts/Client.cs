using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Snowball
{
    public class Client : MonoBehaviour
    {
        [SerializeField]
        int DefaultBeaconPort = 32000;

        [SerializeField]
        int DefaultPort = 32001;

        [SerializeField]
        int DefaultListenPort = 0;

        [SerializeField]
        int DefaultBufferSize = 8192;

        public bool IsOpened { get { return com.IsOpened; } }

        public int UserId { get { return com.UserId; } }

        public int BeaconPort { get { return com.BeaconPortNumber; } set { com.BeaconPortNumber = value; } }
        public int Port { get { return com.PortNumber; } set { com.PortNumber = value; } }
        public int ListenPort { get { return com.ListenPortNumber; } set { com.ListenPortNumber = value; } }

        [SerializeField]
        public int BufferSize { get { return com.BufferSize; } set { com.BufferSize = value; } }

        public ComClient.ConnectedHandler OnConnected { get { return com.OnConnected; } set { com.OnConnected = value; } }
        public ComClient.DisconnectedHandler OnDisconnected { get { return com.OnDisconnected; } set { com.OnDisconnected = value; } }

        public void SetBeaconAcceptFunction(ComClient.BeaconAcceptFunc func) { com.SetBeaconAcceptFunction(func); }

        public void SetValidateRsaKeyFunction(ComClient.ValidateRsaKeyFunc func) { com.SetValidateRsaKeyFunction(func); }


        public int MaxHealthLostCount { get { return com.MaxHealthLostCount; } set { com.MaxHealthLostCount = value; } }

        public bool AcceptBeacon { get { return com.AcceptBeacon; } set { com.AcceptBeacon = value; } }

        public bool IsConnected { get { return com.IsConnected; } }


        ComClient com = new ComClient();
        public ComClient ComClient { get { return com; } }


		public void OnDestroy()
        {
            com.Close();
        }

        public void Open()
        {
            this.BeaconPort = DefaultBeaconPort;
			this.Port = DefaultPort;
			this.ListenPort = DefaultListenPort;
			this.BufferSize = DefaultBufferSize;

			com.Open();
        }

        public void Close()
        {
            com.Close();
        }

        public void AddChannel(IDataChannel channel)
        {
            com.AddChannel(channel);
        }

        public void RemoveChannel(IDataChannel channel)
        {
            com.RemoveChannel(channel);
        }

        public bool Connect(string ip)
        {
            return com.Connect(ip);
        }

        public bool Disconnect()
        {
            return com.Disconnect();
        }

        public async Task<bool> Send<T>(short channelId, T data)
        {
            return await com.Send(channelId, data);
        }

    }

}

