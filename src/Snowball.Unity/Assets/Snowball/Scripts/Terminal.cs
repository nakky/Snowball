using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Snowball;

public class Terminal : MonoBehaviour
{
    [SerializeField]
    int DefaultSendPort = 59901;

    [SerializeField]
    int DefaultListenPort = 59902;

    [SerializeField]
    int DefaultBufferSize = 8192;

    public bool IsOpened { get { return com.IsOpened; } }

    public int SendPort { get { return com.SendPortNumber; } set { com.SendPortNumber = value; } }
    public int ListenPort { get { return com.ListenPortNumber; } set { com.ListenPortNumber = value; } }
    [SerializeField]
    public int BufferSize { get { return com.BufferSize; } set { com.BufferSize = value; } }

    public void AddAcceptList(string ip) { com.AddAcceptList(ip); }
    public void RemoveAcceptList(string ip) { com.RemoveAcceptList(ip); }

    ComTerminal com = new ComTerminal();
    public ComTerminal ComTerminal { get { return com; } }


    public void OnDestroy()
    {
        com.Close();
    }

    public void Open()
    {
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


    public async Task<bool> Send<T>(ComNode node, short channelId, T data)
    {
        return await com.Send(node, channelId, data);
    }
}
