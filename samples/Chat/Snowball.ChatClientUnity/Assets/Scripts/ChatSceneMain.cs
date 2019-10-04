using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Snowball;

public class ChatSceneMain : MonoBehaviour
{
    [SerializeField]
    Client client;

    [SerializeField]
    Text textviewDisplay;

    [SerializeField]
    InputField textviewInput;

    [SerializeField]
    Button buttonSend;

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


        client.OnConnected += (node) =>
        {
            textviewInput.enabled = true;
            buttonSend.enabled = true;
        };

        client.OnDisconnected += (node) =>
        {
            if(textviewInput != null){
                textviewInput.enabled = false;
                buttonSend.enabled = false;
            }
            
        };

        textviewInput.enabled = false;
        buttonSend.enabled = false;

        client.AddChannel(new DataChannel<string>(0, QosType.Reliable, Compression.None, (endPointIp, data) => { OnReceive(data); }));
        client.AcceptBeacon = true;
        client.Open();

    }

    void OnDestroy() {
		textviewInput = null;
	}


    float time = 0.0f;

    // Update is called once per frame
    void Update()
    {
    }

    void OnReceive(string text)
    {
        textviewDisplay.text += text + "\n";
    }

    public void OnButtonSend()
    {
        if(textviewInput.text.Length > 0)
        {
            client.Send(0, textviewInput.text);
            textviewInput.text = "";
        }
    }
}


