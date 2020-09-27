using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Snowball;

[Transferable]
public class ObjState
{
    public ObjState()
    {
    }

    public ObjState(Vector3 position, Quaternion rotation)
    {
        this.Position = position;
        this.Rotation = rotation;
    }

    [Data(0)]
    public Vector3 Position { get; private set; }
    [Data(1)]
    public Quaternion Rotation { get; private set; }
}

public class ServerClientSceneMain : MonoBehaviour
{
    [SerializeField]
    Server server;

    [SerializeField]
    Client client;

    [SerializeField]
    GameObject serverObject;

    [SerializeField]
    GameObject clientObject;

    [SerializeField]
    int numSend = 70;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer ||
            Application.platform == RuntimePlatform.LinuxPlayer)
        {
            Screen.SetResolution(1280, 1024, false);
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        server.AddChannel(new DataChannel<ObjState>(0, QosType.Unreliable, Snowball.Compression.LZ4, (node, data) => {
            serverObject.transform.localPosition = data.Position;
            serverObject.transform.localRotation = data.Rotation;
        }, CheckMode.Speedy));

        server.AddChannel(new DataChannel<string>(1, QosType.Reliable, Snowball.Compression.None, (node, data) => {
            Debug.Log("rec:" + data);
        }));

        client.AddChannel(new DataChannel<ObjState>(0, QosType.Unreliable, Snowball.Compression.LZ4, (node, data) => {
        }, CheckMode.Speedy));

        client.AddChannel(new DataChannel<string>(1, QosType.Reliable, Snowball.Compression.None, (node, data) => {
        }));


        client.AcceptBeacon = true;
        client.Open();

        server.AddBeaconList(IPAddress.Broadcast.ToString());
        server.Open();
        server.BeaconStart();

    }


    float time = 0.0f;

    // Update is called once per frame
    void Update()
    {
        clientObject.transform.localPosition = new Vector3(0.0f, Mathf.Sin(time), 0.0f);

        time += 0.1f;
        
        
        if(client.IsConnected)
        {
            ObjState state = new ObjState(clientObject.transform.localPosition, clientObject.transform.localRotation);

            for(int i = 0; i < numSend; i++)
            {
                client.Send(0, state);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            client.Send(1, "Hello Unity!");
        }
    }

    public void Disconnect()
    {
        client.Disconnect();
    }
}


