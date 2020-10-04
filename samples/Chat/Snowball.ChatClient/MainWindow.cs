using System;
using Gtk;

using Snowball;

public partial class MainWindow : Gtk.Window
{

    ComClient client = new ComClient();

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        buttonSend.Clicked += (sender, e) =>
        {
            if(textviewInput.Buffer.Text.Length > 0)
            {
                client.Send(0, textviewInput.Buffer.Text);
                textviewInput.Buffer.Text = "";
            }

        };

        buttonConnect.Clicked += (sender, e) =>
        {
            if (client.IsConnected) client.Disconnect();
            else client.Connect("127.0.0.1");
        };

        client.OnConnected += (node) =>
        {
            textviewInput.Sensitive = true;
            buttonSend.Sensitive = true;
            buttonConnect.Label = "Disconnect";
        };

        client.OnDisconnected += (node) =>
        {
            textviewInput.Sensitive = false;
            buttonSend.Sensitive = false;
            buttonConnect.Label = "Connect";
        };

        client.AddChannel(new DataChannel<string>(0, QosType.Reliable, Compression.LZ4, Encryption.Aes, (endPointIp, data) =>
        {
            OnReceive(data);
        }));
        client.AcceptBeacon = true;

        client.ListenPortNumber = client.PortNumber + 1;

        client.Open();
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        client.Close();

        Application.Quit();
        a.RetVal = true;
    }

    void OnReceive(string text)
    {
        textviewDisplay.Buffer.Text += text + "\n";
    }

}
