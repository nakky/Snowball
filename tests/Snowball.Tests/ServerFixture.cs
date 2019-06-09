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

            Server.AddBeaconList(IPAddress.Broadcast.ToString());

            AddEchoChannel();

            Server.Open();

        }


        public void Dispose()
        {
            Server.Close();
        }

        void AddEchoChannel()
        {
            CompressionType comp = CompressionType.LZ4;

            //Byte
            Server.AddChannel(new DataChannel((short)ChannelId.ByteRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ByteRel, (byte)data);
            }));


            Server.AddChannel(new DataChannel((short)ChannelId.ByteUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ByteUnRel, (byte)data);
            }));

            //Short
            Server.AddChannel(new DataChannel((short)ChannelId.ShortRel, QosType.Reliable, comp, (node, data) =>
            { 
                Server.SendData(node, (short)ChannelId.ShortRel, (short)data);
            }));

            Server.AddChannel(new DataChannel((short)ChannelId.ShortUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ShortUnRel, (short)data);
            }));

            //Int
            Server.AddChannel(new DataChannel((short)ChannelId.IntRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.IntRel, (int)data);
            }));

            Server.AddChannel(new DataChannel((short)ChannelId.IntUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.IntUnRel, (int)data);
            }));

            //Float
            Server.AddChannel(new DataChannel((short)ChannelId.FloatRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.FloatRel, (float)data);
            }));

            Server.AddChannel(new DataChannel((short)ChannelId.FloatUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.FloatUnRel, (float)data);
            }));

            //Double
            Server.AddChannel(new DataChannel((short)ChannelId.DoubleRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.DoubleRel, (double)data);
            }));

            Server.AddChannel(new DataChannel((short)ChannelId.DoubleUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.DoubleUnRel, (double)data);
            }));

            //String
            Server.AddChannel(new DataChannel((short)ChannelId.StringRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.StringRel, (string)data);
            }));

            Server.AddChannel(new DataChannel((short)ChannelId.StringUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.StringUnRel, (string)data);
            }));

            //Class
            Server.AddChannel(new DataChannel((short)ChannelId.ClassRel, QosType.Reliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ClassRel, (TestClass)data);
            }));

            Server.AddChannel(new DataChannel((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                Server.SendData(node, (short)ChannelId.ClassUnRel, (TestClass)data);
            }));


        }
    }

    [CollectionDefinition(nameof(ComServerFixture))]
    public class ComServerFixture : ICollectionFixture<ServerFixture>
    {

    }
}
