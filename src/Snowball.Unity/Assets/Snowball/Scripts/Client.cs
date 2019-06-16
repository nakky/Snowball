using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snowball
{
    public class Client : MonoBehaviour
    {
		[SerializeField]
		string DefaultUserName = "u001";

		[SerializeField]
        int DefaultSendPort = 59901;

        [SerializeField]
        int DefaultListenPort = 59902;

        [SerializeField]
        int DefaultBufferSize = 8192;

        public bool IsOpened { get { return com.IsOpened; } }

		public string UserName { get { return com.UserName; } set { com.UserName = value; } }

		public int SendPort { get { return com.SendPortNumber; } set { com.SendPortNumber = value; } }
        public int ListenPort { get { return com.ListenPortNumber; } set { com.ListenPortNumber = value; } }
        [SerializeField]
        public int BufferSize { get { return com.BufferSize; } set { com.BufferSize = value; } }

        public ComClient.ConnectedHandler OnConnected { get { return com.OnConnected; } private set { com.OnConnected = value; } }
        public ComClient.DisconnectedHandler OnDisconnected { get { return com.OnDisconnected; } private set { com.OnDisconnected = value; } }

        public void SetBeaconAcceptFunction(ComClient.BeaconAcceptFunc func) { com.SetBeaconAcceptFunction(func); }

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
			this.UserName = DefaultUserName;
			this.SendPort = DefaultSendPort;
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

        public bool SendData<T>(short channelId, T data)
        {
            return com.SendData(channelId, data);
        }

    }

}

