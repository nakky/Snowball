using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Snowball
{
    public class Server : MonoBehaviour
    {
        [SerializeField]
        int DefaultBeaconPort = 32000;

        [SerializeField]
        int DefaultPort = 32001;

        [SerializeField]
        int DefaultBufferSize = 8192;

        public bool IsOpened { get { return com.IsOpened; } }

        public int BeaconPort { get { return com.BeaconPortNumber; } set { com.BeaconPortNumber = value; } }
        public int Port { get { return com.PortNumber; } set { com.PortNumber = value; } }

        [SerializeField]
        public int BufferSize { get { return com.BufferSize; } set { com.BufferSize = value; } }

        public ComServer.ConnectedHandler OnConnected { get { return com.OnConnected; } set { com.OnConnected = value; } }
        public ComServer.DisconnectedHandler OnDisconnected { get { return com.OnDisconnected; } set { com.OnDisconnected = value; } }

        public void SetBeaconDataCreateFunction(ComServer.BeaconDataGenerateFunc func) { com.SetBeaconDataCreateFunction(func); }

        public void SetRsaKeyGenerateFunction(ComServer.RsaKeyGenerateFunc func) { com.SetRsaKeyGenerateFunction(func); }


        public int BeaconIntervalMs { get { return com.BeaconIntervalMs; } set { com.BeaconIntervalMs = value; } }
        public int MaxHealthLostCount { get { return com.MaxHealthLostCount; } set { com.MaxHealthLostCount = value; } }

        public int RemoveIntervalSec { get { return com.RemoveIntervalSec; } set { com.RemoveIntervalSec = value; } }

        ComServer com = new ComServer();
        public ComServer ComServer { get { return com; } }


        private void OnDestroy()
        {
            com.Close();
        }

        public void AddBeaconList(string ip)
        {
            com.AddBeaconList(ip);
        }

        public void RemoveBeaconList(string ip)
        {
            com.RemoveBeaconList(ip);
        }


        public void Open()
        {
            this.BeaconPort = DefaultBeaconPort;
            this.Port = DefaultPort;
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

        public void BeaconStart()
        {
            com.BeaconStart();
        }

        public void BeaconStop()
        {
            com.BeaconStop();
        }

        public bool Disconnect(ComNode node)
        {
            return com.Disconnect(node);
        }

        public async Task<bool> Broadcast<T>(ComGroup group, short channelId, T data)
        {
            return await com.Broadcast(group, channelId, data);
        }

        public async Task<bool> Send<T>(ComNode node, short channelId, T data)
        {
            return await com.Send(node, channelId, data);
        }

    }

}
