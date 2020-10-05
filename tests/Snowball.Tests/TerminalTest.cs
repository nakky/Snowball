using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using Xunit;
using Xunit.Abstractions;

using Snowball;

namespace Snowball.Tests
{
    [Collection(nameof(ComTerminalFixture))]
    public class TerminalTest : IDisposable
    {
        ITestOutputHelper logger;
        TerminalFixture server;

        Compression comp = Compression.LZ4;
        Encryption enc = Encryption.None;
        CheckMode checkMode = CheckMode.Speedy;

        public TerminalTest(ITestOutputHelper logger, TerminalFixture terminal)
        {
            this.logger = logger;
            this.server = terminal;

            Global.UseSyncContextPost = false;
        }

        public void Dispose()
        {
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void OpenClose()
        {
            Util.Log("OpenClose");

            ComTerminal terminal = new ComTerminal();
            terminal.ListenPortNumber = this.server.Port;
            terminal.PortNumber = this.server.Port + 1;
            terminal.Open();

            terminal.Close();
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void SendReceive()
        {
            Util.Log("SendReceive");

            ComTerminal terminal = new ComTerminal();
            terminal.ListenPortNumber = this.server.Port;
            terminal.PortNumber = this.server.Port + 1;

            terminal.AddAcceptList(IPAddress.Loopback.ToString());

            //AddChannel
            bool boolCheck = false;
            bool byteCheck = false;
            bool shortCheck = false;
            bool intCheck = false;
            bool floatCheck = false;
            bool doubleCheck = false;
            bool stringCheck = false;
            bool classCheck = false;

            bool boolData = true;
            byte byteData = 246;
            short shortData = 361;
            int intData = 543;
            float floatData = 5.6f;
            double doubleData = 32.5;
            string stringData = "You are not human!";
            TestClass testData = new TestClass();
            testData.intData = 6;
            testData.floatData = 6.6f;
            testData.stringData = "Are you human?";

            //Bool
            terminal.AddChannel(new DataChannel<bool>((short)ChannelId.BoolUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == boolData) boolCheck = true;
            }, checkMode));

            //Byte
            terminal.AddChannel(new DataChannel<byte>((short)ChannelId.ByteUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == byteData) byteCheck = true;
            }, checkMode));

            //Short
            terminal.AddChannel(new DataChannel<short>((short)ChannelId.ShortUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == shortData) shortCheck = true;
            }, checkMode));

            //Int
            terminal.AddChannel(new DataChannel<int>((short)ChannelId.IntUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == intData) intCheck = true;
            }, checkMode));

            //Float
            terminal.AddChannel(new DataChannel<float>((short)ChannelId.FloatUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == floatData) floatCheck = true;
            }, checkMode));

            //Double
            terminal.AddChannel(new DataChannel<double>((short)ChannelId.DoubleUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == doubleData) doubleCheck = true;
            }, checkMode));

            //String
            terminal.AddChannel(new DataChannel<string>((short)ChannelId.StringUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == stringData) stringCheck = true;
            }, checkMode));

            //Class
            terminal.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                    )
                {
                    classCheck = true;
                }
            }, checkMode));

            terminal.Open();

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, terminal.PortNumber);
            ComNode lnode = new ComNode(endPoint);

            //Send
            terminal.Send(lnode, (short)ChannelId.BoolUnRel, boolData);
            terminal.Send(lnode, (short)ChannelId.ByteUnRel, byteData);
            terminal.Send(lnode, (short)ChannelId.ShortUnRel, shortData);
            terminal.Send(lnode, (short)ChannelId.IntUnRel, intData);
            terminal.Send(lnode, (short)ChannelId.FloatUnRel, floatData);
            terminal.Send(lnode, (short)ChannelId.DoubleUnRel, doubleData);
            terminal.Send(lnode, (short)ChannelId.StringUnRel, stringData);
            terminal.Send(lnode, (short)ChannelId.ClassUnRel, testData);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            while (true)
            {
                if (boolCheck
                    && byteCheck
                    && shortCheck
                    && intCheck
                    && floatCheck
                    && doubleCheck
                    && stringCheck
                    && classCheck
                    )
                {
                    break;
                }
                else if (sw.Elapsed.Seconds >= 50)
                {
                    terminal.Close();
                    throw new TimeoutException();
                }

                Task.Delay(100);
            }
            sw.Stop();
            terminal.Close();
        }

       
        [Fact]
        //[Fact(Skip = "Skipped")]
        public void StressTest()
        {
            Util.Log("StressTest");

            ComTerminal terminal = new ComTerminal();
            terminal.ListenPortNumber = this.server.Port;
            terminal.PortNumber = this.server.Port + 1;

            terminal.AddAcceptList(IPAddress.Loopback.ToString());

            terminal.BufferSize = 8192 * 10;

            int recvTestNum = 0;
            int sendTestNum = 0;

            //AddChannel
            TestClass testData = new TestClass();
            testData.intData = 6;
            testData.floatData = 6.6f;
            testData.stringData = "Are you human?";

            //Byte
            //Class
            terminal.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                )
                {
                    recvTestNum++;
                }
            }, checkMode));

            terminal.Open();

            Random random = new Random();
            sendTestNum = random.Next(500, 600);

            Util.Log("sendTestNum:" + sendTestNum);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, terminal.PortNumber);
            ComNode lnode = new ComNode(endPoint);

            //Send
            for (int i = 0; i < sendTestNum; i++)
            {
                terminal.Send(lnode, (short)ChannelId.ClassUnRel, testData);
            }


            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            while (true)
            {
                if (recvTestNum == sendTestNum)
                {
                    break;
                }
                else if (sw.Elapsed.Seconds >= 10)
                {
                    float percennt = (float)recvTestNum / (float)sendTestNum * 100.0f;
                    Util.Log("percent:" + percennt);
                    if (percennt < 80.0f)
                    {
                        terminal.Close();
                        throw new InvalidProgramException();
                    }
                    break;
                }

                Task.Delay(100);
            }
            sw.Stop();

            terminal.Close();

        }

      
    }
}
