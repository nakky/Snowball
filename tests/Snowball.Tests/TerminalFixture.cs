using System;
using System.Net;

using Snowball;

using Xunit;

namespace Snowball.Tests
{
    public class TerminalFixture : IDisposable
    {
        public int SendPort { get; private set; }
        public int ListenPort { get; private set; }

        public ComTerminal Terminal { get; private set; }

        Compression comp = Compression.LZ4;
        Encryption enc = Encryption.None;

        public TerminalFixture()
        {
            Util.Log("TerminalFixture");

            Global.UseSyncContextPost = false;

            Random rand = new Random();
            SendPort = rand.Next(10000, 20000);
            ListenPort = SendPort;

            while (ListenPort == SendPort)
            {
                ListenPort = rand.Next(10000, 20000);
            }

            Util.Log("send:" + SendPort + ", listen:" + ListenPort);

            Terminal = new ComTerminal();
            Terminal.SendPortNumber = SendPort;
            Terminal.ListenPortNumber = ListenPort;
            Terminal.BufferSize = 8192 * 10;

            Terminal.AddAcceptList(IPAddress.Loopback.ToString());

            AddEchoChannel();

            Terminal.Open();

        }


        public void Dispose()
        {
            Terminal.Close();
        }

        void AddEchoChannel()
        {
            //Bool
            Terminal.AddChannel(new DataChannel<bool>((short)ChannelId.BoolUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.BoolUnRel, data);
            }));

            //Byte
            Terminal.AddChannel(new DataChannel<byte>((short)ChannelId.ByteUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.ByteUnRel, data);
            }));

            //Short
            Terminal.AddChannel(new DataChannel<short>((short)ChannelId.ShortUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.ShortUnRel, data);
            }));

            //Int
            Terminal.AddChannel(new DataChannel<int>((short)ChannelId.IntUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.IntUnRel, data);
            }));

            //Float
            Terminal.AddChannel(new DataChannel<float>((short)ChannelId.FloatUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.FloatUnRel, data);
            }));

            //Double
            Terminal.AddChannel(new DataChannel<double>((short)ChannelId.DoubleUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.DoubleUnRel, data);
            }));

            //String
            Terminal.AddChannel(new DataChannel<string>((short)ChannelId.StringUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.StringUnRel, data);
            }));

            //Class
            Terminal.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                Terminal.Send(node, (short)ChannelId.ClassUnRel, data);
            }));

           
        }
    }

    [CollectionDefinition(nameof(ComTerminalFixture))]
    public class ComTerminalFixture : ICollectionFixture<TerminalFixture>
    {

    }
}
