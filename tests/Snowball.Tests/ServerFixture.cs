using System;
using System.Net;

using Snowball;

using Xunit;

namespace Snowball.Tests
{
    public class ServerFixture : IDisposable
    {
        public int SendPort { get; private set; }
        public int ListenPort { get; private set; }

        public ComServer Server { get; private set; }

        public ServerFixture()
        {
            TCPConnection.UseSyncContextPost = false;
            UDPReceiver.UseSyncContextPost = false;

            Random rand = new Random();
            SendPort = rand.Next(10000, 20000);
            ListenPort = SendPort;

            while (ListenPort == SendPort)
            {
                ListenPort = rand.Next(10000, 20000);
            }

            Util.Log("send:" + SendPort + ", listen:" + ListenPort);

            Server = new ComServer();
            Server.SendPortNumber = SendPort;
            Server.ListenPortNumber = ListenPort;
            Server.BufferSize = 8192 * 10;

            Server.AddBeaconList(IPAddress.Broadcast.ToString());

            AddEchoChannel();

            Server.SetBeaconDataCreateFunction(() => {
                return "Test";
            });

            Server.Open();

        }


        public void Dispose()
        {
            Server.Close();
        }

        void AddEchoChannel()
        {
            Compression comp = Compression.None;

            //Byte
            Server.AddChannel(new DataChannel<byte>((short)ChannelId.ByteRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ByteRel, data);
            }));


            Server.AddChannel(new DataChannel<byte>((short)ChannelId.ByteUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ByteUnRel, data);
            }));

            //Short
            Server.AddChannel(new DataChannel<short>((short)ChannelId.ShortRel, QosType.Reliable, comp, (node, data) =>
            { 
                Server.SendData(node, (short)ChannelId.ShortRel, data);
            }));

            Server.AddChannel(new DataChannel<short>((short)ChannelId.ShortUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ShortUnRel, data);
            }));

            //Int
            Server.AddChannel(new DataChannel<int>((short)ChannelId.IntRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.IntRel, data);
            }));

            Server.AddChannel(new DataChannel<int>((short)ChannelId.IntUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.IntUnRel, data);
            }));

            //Float
            Server.AddChannel(new DataChannel<float>((short)ChannelId.FloatRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.FloatRel, data);
            }));

            Server.AddChannel(new DataChannel<float>((short)ChannelId.FloatUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.FloatUnRel, data);
            }));

            //Double
            Server.AddChannel(new DataChannel<double>((short)ChannelId.DoubleRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.DoubleRel, data);
            }));

            Server.AddChannel(new DataChannel<double>((short)ChannelId.DoubleUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.DoubleUnRel, data);
            }));

            //String
            Server.AddChannel(new DataChannel<string>((short)ChannelId.StringRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.StringRel, data);
            }));

            Server.AddChannel(new DataChannel<string>((short)ChannelId.StringUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.StringUnRel, data);
            }));

            //Class
            Server.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ClassRel, data);
            }));

            Server.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ClassUnRel, data);
            }));


        }
    }

    [CollectionDefinition(nameof(ComServerFixture))]
    public class ComServerFixture : ICollectionFixture<ServerFixture>
    {

    }
}
