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

        Compression comp = Compression.LZ4;

        public ServerFixture()
        {
            Util.Log("ServerFixture");

            Global.UseSyncContextPost = false;

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

            Server.SetBeaconDataCreateFunction(() =>
            {
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
            //Bool
            Server.AddChannel(new DataChannel<bool>((short)ChannelId.BoolRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.BoolRel, data);
            }));


            Server.AddChannel(new DataChannel<bool>((short)ChannelId.BoolUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.BoolUnRel, data);
            }));

            //Byte
            Server.AddChannel(new DataChannel<byte>((short)ChannelId.ByteRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ByteRel, data);
            }));


            Server.AddChannel(new DataChannel<byte>((short)ChannelId.ByteUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ByteUnRel, data);
            }));

            //Short
            Server.AddChannel(new DataChannel<short>((short)ChannelId.ShortRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ShortRel, data);
            }));

            Server.AddChannel(new DataChannel<short>((short)ChannelId.ShortUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ShortUnRel, data);
            }));

            //Int
            Server.AddChannel(new DataChannel<int>((short)ChannelId.IntRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.IntRel, data);
            }));

            Server.AddChannel(new DataChannel<int>((short)ChannelId.IntUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.IntUnRel, data);
            }));

            //Float
            Server.AddChannel(new DataChannel<float>((short)ChannelId.FloatRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.FloatRel, data);
            }));

            Server.AddChannel(new DataChannel<float>((short)ChannelId.FloatUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.FloatUnRel, data);
            }));

            //Double
            Server.AddChannel(new DataChannel<double>((short)ChannelId.DoubleRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.DoubleRel, data);
            }));

            Server.AddChannel(new DataChannel<double>((short)ChannelId.DoubleUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.DoubleUnRel, data);
            }));

            //String
            Server.AddChannel(new DataChannel<string>((short)ChannelId.StringRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.StringRel, data);
            }));

            Server.AddChannel(new DataChannel<string>((short)ChannelId.StringUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.StringUnRel, data);
            }));

            //Class
            Server.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ClassRel, data);
            }));

            Server.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ClassUnRel, data);
            }));

            //Raw

            Server.AddChannel(new DataChannel<bool>((short)ChannelId.BoolRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.BoolRaw, data);
            }));
            Server.AddChannel(new RawDataChannel<byte>((short)ChannelId.ByteRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ByteRaw, data);
            }));

            Server.AddChannel(new RawDataChannel<short>((short)ChannelId.ShortRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ShortRaw, data);
            }));

            Server.AddChannel(new RawDataChannel<int>((short)ChannelId.IntRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.IntRaw, data);
            }));

            Server.AddChannel(new RawDataChannel<float>((short)ChannelId.FloatRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.FloatRaw, data);
            }));

            Server.AddChannel(new RawDataChannel<double>((short)ChannelId.DoubleRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.DoubleRaw, data);
            }));

            Server.AddChannel(new RawDataChannel<string>((short)ChannelId.StringRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.StringRaw, data);
            }));

            Server.AddChannel(new RawDataChannel<TestClass>((short)ChannelId.ClassRaw, QosType.Reliable, comp, (node, data) =>
            {
                Server.Send(node, (short)ChannelId.ClassRaw, data);
            }));


        }
    }

    [CollectionDefinition(nameof(ComServerFixture))]
    public class ComServerFixture : ICollectionFixture<ServerFixture>
    {

    }
}
