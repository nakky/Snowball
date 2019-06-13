using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using Xunit;
using Xunit.Abstractions;

using Snowball;

namespace Snowball.Tests
{
    [Collection(nameof(ComServerFixture))]
    public class ClientTest : IDisposable
    {
        ITestOutputHelper logger;
        ServerFixture server;

        public ClientTest(ITestOutputHelper logger, ServerFixture server)
        {
            this.logger = logger;
            this.server = server;

        }

        public void Dispose()
        {
            server.Server.BeaconStop();
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void OpenClose()
        {
            Util.Log("OpenClose");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;
            client.Open();

            client.Close();
        }

        void Connect(ref ComClient client)
        {
            bool connected = false;
            Stopwatch sw = new Stopwatch();

            client.SetBeaconAcceptFunction((data) => { if (data == "Test") return true; else return false; });

            client.OnConnected += (node) =>
            {
                connected = true;
            };
            client.Open();
            client.AcceptBeacon = true;

            server.Server.BeaconStart();

            sw.Start();
            while (true)
            {
                if (client.IsConnected && connected)
                {
                    break;
                }
                else if (sw.Elapsed.Seconds >= 3)
                {
                    client.Close();
                    server.Server.BeaconStop();
                    throw new TimeoutException();
                }
                Task.Delay(100);
            }
            sw.Stop();
        }

        void Disconnect(ref ComClient client)
        {
            bool connected = true;
            Stopwatch sw = new Stopwatch();

            client.OnDisconnected += (node) =>
            {
                connected = false;
            };
            client.Disconnect();

            sw.Start();
            while (true)
            {
                if (!client.IsConnected && !connected)
                {
                    break;
                }
                else if (sw.Elapsed.Seconds >= 3)
                {
                    client.Close();
                    throw new TimeoutException();
                }

                Task.Delay(100);
            }
            sw.Stop();

            client.Close();
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void ConnectDisconnect()
        {
            Util.Log("ConnectDisconnect");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            Connect(ref client);
            server.Server.BeaconStop();

            Disconnect(ref client);

        }


        [Fact]
        //[Fact(Skip = "Skipped")]
        public void SendReceiveReliable()
        {
            Util.Log("SendReceiveReliable");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            Connect(ref client);
            server.Server.BeaconStop();

            //AddChannel
            bool byteCheck = false;
            bool shortCheck = false;
            bool intCheck = false;
            bool floatCheck = false;
            bool doubleCheck = false;
            bool stringCheck = false;
            bool classCheck = false;

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

            Compression comp = Compression.None;

            //Byte
            client.AddChannel(new DataChannel<byte>((short)ChannelId.ByteRel, QosType.Reliable, comp, (node, data) =>
            {
                if (data == byteData) byteCheck = true;
            }));

            //Short
            client.AddChannel(new DataChannel<short>((short)ChannelId.ShortRel, QosType.Reliable, comp, (node, data) =>
            {
                if (data == shortData) shortCheck = true;
            }));

            //Int
            client.AddChannel(new DataChannel<int>((short)ChannelId.IntRel, QosType.Reliable, comp, (node, data) =>
            {
                if (data == intData) intCheck = true;
            }));

            //Float
            client.AddChannel(new DataChannel<float>((short)ChannelId.FloatRel, QosType.Reliable, comp, (node, data) =>
            {
                if (data == floatData) floatCheck = true;
            }));

            //Double
            client.AddChannel(new DataChannel<double>((short)ChannelId.DoubleRel, QosType.Reliable, comp, (node, data) =>
            {
                if (data == doubleData) doubleCheck = true;
            }));

            //String
            client.AddChannel(new DataChannel<string>((short)ChannelId.StringRel, QosType.Reliable, comp, (node, data) =>
            {
                if (data == stringData) stringCheck = true;
            }));

            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRel, QosType.Reliable, comp, (node, data) =>
            {
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                )
                {
                    classCheck = true;
                }
            }));


            //Send
            client.SendData((short)ChannelId.ByteRel, byteData);
            client.SendData((short)ChannelId.ShortRel, shortData);
            client.SendData((short)ChannelId.IntRel, intData);
            client.SendData((short)ChannelId.FloatRel, floatData);
            client.SendData((short)ChannelId.DoubleRel, doubleData);
            client.SendData((short)ChannelId.StringRel, stringData);
            client.SendData((short)ChannelId.ClassRel, testData);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            while (true)
            {
                if (byteCheck
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
                else if (sw.Elapsed.Seconds >= 5)
                {
                    client.Close();
                    throw new TimeoutException();
                }

                Task.Delay(100);
            }
            sw.Stop();

            Disconnect(ref client);
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void SendReceiveUnreliable()
        {
            Util.Log("SendReceiveUnreliable");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            Connect(ref client);
            server.Server.BeaconStop();

            //AddChannel
            bool byteCheck = false;
            bool shortCheck = false;
            bool intCheck = false;
            bool floatCheck = false;
            bool doubleCheck = false;
            bool stringCheck = false;
            bool classCheck = false;

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

            Compression comp = Compression.None;

            //Byte
            client.AddChannel(new DataChannel<byte>((short)ChannelId.ByteUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (data == byteData) byteCheck = true;
            }));

            //Short
            client.AddChannel(new DataChannel<short>((short)ChannelId.ShortUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (data == shortData) shortCheck = true;
            }));

            //Int
            client.AddChannel(new DataChannel<int>((short)ChannelId.IntUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (data == intData) intCheck = true;
            }));

            //Float
            client.AddChannel(new DataChannel<float>((short)ChannelId.FloatUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (data == floatData) floatCheck = true;
            }));

            //Double
            client.AddChannel(new DataChannel<double>((short)ChannelId.DoubleUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (data == doubleData) doubleCheck = true;
            }));

            //String
            client.AddChannel(new DataChannel<string>((short)ChannelId.StringUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (data == stringData) stringCheck = true;
            }));

            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                    )
                {
                    classCheck = true;
                }
            }));

            //Send
            client.SendData((short)ChannelId.ByteUnRel, byteData);
            client.SendData((short)ChannelId.ShortUnRel, shortData);
            client.SendData((short)ChannelId.IntUnRel, intData);
            client.SendData((short)ChannelId.FloatUnRel, floatData);
            client.SendData((short)ChannelId.DoubleUnRel, doubleData);
            client.SendData((short)ChannelId.StringUnRel, stringData);
            client.SendData((short)ChannelId.ClassUnRel, testData);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            while (true)
            {
                if (byteCheck
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
                else if (sw.Elapsed.Seconds >= 5)
                {
                    client.Close();
                    throw new TimeoutException();
                }

                Task.Delay(100);
            }
            sw.Stop();

            Disconnect(ref client);
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void StressTestReliable()
        {
            Util.Log("StressTestReliable");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            client.BufferSize = 8192 * 10;

            Connect(ref client);
            server.Server.BeaconStop();

            int recvTestNum = 0;
            int sendTestNum = 0;

            //AddChannel
            TestClass testData = new TestClass();
            testData.intData = 6;
            testData.floatData = 6.6f;
            testData.stringData = "Are you human?";

            Compression comp = Compression.None;

            //Byte
            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRel, QosType.Reliable, comp, (node, data) =>
            {
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                )
                {
                    recvTestNum++;
                }
            }));

            Random random = new Random();
            sendTestNum = random.Next(500, 600);

            Util.Log("sendTestNum:" + sendTestNum);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            //Send
            for (int i = 0; i < sendTestNum; i++)
            {
                client.SendData((short)ChannelId.ClassRel, testData);
            }


            while (true)
            {
                if (recvTestNum == sendTestNum)
                {
                    break;
                }
                else if (sw.Elapsed.Seconds >= 10)
                {
                    Util.Log("recvTestNum:" + recvTestNum);
                    client.Close();
                    throw new TimeoutException();
                }

                Task.Delay(100);
            }
            sw.Stop();

            Disconnect(ref client);
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void StressTestUnreliable()
        {
            Util.Log("StressTestUnreliable");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            client.BufferSize = 8192 * 10;

            Connect(ref client);
            server.Server.BeaconStop();

            int recvTestNum = 0;
            int sendTestNum = 0;

            //AddChannel
            TestClass testData = new TestClass();
            testData.intData = 6;
            testData.floatData = 6.6f;
            testData.stringData = "Are you human?";

            Compression comp = Compression.None;

            //Byte
            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, (node, data) =>
            {
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                )
                {
                    recvTestNum++;
                }
            }));

            Random random = new Random();
            sendTestNum = random.Next(500, 600);

            Util.Log("sendTestNum:" + sendTestNum);

            //Send
            for (int i = 0; i < sendTestNum; i++)
            {
                client.SendData((short)ChannelId.ClassUnRel, testData);
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
                        client.Close();
                        throw new InvalidProgramException();
                    }
                    break;
                }

                Task.Delay(100);
            }
            sw.Stop();

            Disconnect(ref client);
        }


        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantEffectTestFloat()
        {
            Util.Log("VariantEffectTestFloat");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            client.BufferSize = 8192 * 10;

            Connect(ref client);
            server.Server.BeaconStop();

            int recvTestNum = 0;
            int sendTestNum = 0;

            //AddChannel
            float testfloat = 1.0f;

            Compression comp = Compression.None;

            //Byte
            //Class
            client.AddChannel(new DataChannel<float>((short)ChannelId.FloatRel, QosType.Reliable, comp, (node, data) =>
            {
                if (
                    testfloat == data
                )
                {
                    recvTestNum++;
                }
            }));

            sendTestNum = 100000;

            Util.Log("sendTestNum:" + sendTestNum);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            //Send
            for (int i = 0; i < sendTestNum; i++)
            {
                client.SendData((short)ChannelId.FloatRel, testfloat);
            }


            while (true)
            {
                if (recvTestNum == sendTestNum)
                {
                    Util.Log("float time:" + sw.Elapsed);
                    break;
                }
                else if (sw.Elapsed.Seconds >= 30)
                {
                    Util.Log("recvTestNum:" + recvTestNum);
                    client.Close();
                    throw new TimeoutException();
                }

                Task.Delay(100);
            }
            sw.Stop();

            Disconnect(ref client);
        }

        [Fact]
        //[Fact(Skip = "Skipped")]
        public void VariantEffectTestClass()
        {
            Util.Log("VariantEffectTestClass");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            client.BufferSize = 8192 * 10;

            Connect(ref client);
            server.Server.BeaconStop();

            int recvTestNum = 0;
            int sendTestNum = 0;

            //AddChannel
            TestClass testData = new TestClass();
            testData.intData = 6;
            testData.floatData = 6.6f;
            testData.stringData = "Are you human?";

            Compression comp = Compression.None;

            //Byte
            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRel, QosType.Reliable, comp, (node, data) =>
            {
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                )
                {
                    recvTestNum++;
                }
            }));

            sendTestNum = 100000;

            Util.Log("sendTestNum:" + sendTestNum);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            //Send
            for (int i = 0; i < sendTestNum; i++)
            {
                client.SendData((short)ChannelId.ClassRel, testData);
            }


            while (true)
            {
                if (recvTestNum == sendTestNum)
                {
                    Util.Log("class time:" + sw.Elapsed);
                    break;
                }
                else if (sw.Elapsed.Seconds >= 30)
                {
                    Util.Log("recvTestNum:" + recvTestNum);
                    client.Close();
                    throw new TimeoutException();
                }

                Task.Delay(100);
            }
            sw.Stop();

            Disconnect(ref client);
        }

    }
}
