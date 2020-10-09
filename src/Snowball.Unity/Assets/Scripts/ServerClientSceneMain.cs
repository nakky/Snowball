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
    MeshRenderer serverRenderer;

    [SerializeField]
    GameObject clientObject;
    MeshRenderer clientRenderer;

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

        serverRenderer = serverObject.gameObject.GetComponent<MeshRenderer>();
        clientRenderer = clientObject.gameObject.GetComponent<MeshRenderer>();

        server.AddChannel(new DataChannel<ObjState>(0, QosType.Unreliable, Snowball.Compression.None, Encryption.None, (node, data) => {
            serverObject.transform.localPosition = data.Position;
            serverObject.transform.localRotation = data.Rotation;
        }, CheckMode.Speedy));

        server.AddChannel(new DataChannel<string>(1, QosType.Reliable, Snowball.Compression.None, Encryption.Aes, (node, data) => {
            Debug.Log("rec:" + data);
        }));

        server.AddChannel(new DataChannel<Color>(2, QosType.Reliable, Snowball.Compression.None, Encryption.Aes, (node, data) => {
            serverRenderer.material.SetColor("_Color", data);
        }));

        client.AddChannel(new DataChannel<ObjState>(0, QosType.Unreliable, Snowball.Compression.None, Encryption.None, (node, data) => {
        }, CheckMode.Speedy));

        client.AddChannel(new DataChannel<string>(1, QosType.Reliable, Snowball.Compression.None, Encryption.Aes, (node, data) => {
        }));

        client.AddChannel(new DataChannel<Color>(2, QosType.Reliable, Snowball.Compression.None, Encryption.Aes, (node, data) => {
        }));


        server.OnConnected += (node) =>
        {
            if (node != null) Debug.Log("Server:Connected");
        };

        server.OnDisconnected += (node) =>
        {
            if (node != null) Debug.Log("Server:Disconnected");
        };

        client.OnConnected += (node) =>
        {
            if (node != null) Debug.Log("Client:Connected");
        };

        client.OnDisconnected += (node) =>
        {
            if (node != null) Debug.Log("Client:Disconnected");
        };

        client.Open();
        client.AcceptBeacon = true;

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

        if(Input.GetKeyDown(KeyCode.C))
        {
            Color color = new Color(
                Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f),
                1.0f
                );
            clientRenderer.material.SetColor("_Color", color);
            client.Send(2, color);
        }
    }

    public void Disconnect()
    {
        client.Disconnect();
    }
}


