using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Snowball;
using MessagePack;

[MessagePackObject]
public class ObjState
{
    [Key(0)]
    public Vector3 position { get; set; }
    [Key(1)]
    public Quaternion rotation { get; set; }
}

public class SampleSceneMain : MonoBehaviour
{
    [SerializeField]
    Server server;

    [SerializeField]
    Client client;

    [SerializeField]
    GameObject serverObject;

    [SerializeField]
    GameObject clientObject;

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        server.AddChannel(new DataChannel<ObjState>(0, QosType.Unreliable, Snowball.Compression.LZ4, (node, data) => {
            serverObject.transform.localPosition = data.position;
            serverObject.transform.localRotation = data.rotation;
        }));

        server.AddChannel(new DataChannel<string>(1, QosType.Reliable, Snowball.Compression.None, (node, data) => {
            Debug.Log("rec:" + data);
        }));

        client.AddChannel(new DataChannel<ObjState>(0, QosType.Unreliable, Snowball.Compression.LZ4, (node, data) => {
        }));

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
            ObjState state = new ObjState();
            state.position = clientObject.transform.localPosition;
            state.rotation = clientObject.transform.localRotation;

            for(int i = 0; i < 70; i++)
            {
                client.SendData(0, state);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            client.SendData(1, "Hello Unity!");
        }
    }
}
