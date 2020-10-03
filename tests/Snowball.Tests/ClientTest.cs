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

        Compression comp = Compression.LZ4;
        Encryption enc = Encryption.Aes;
        CheckMode checkMode = CheckMode.Speedy;

        public ClientTest(ITestOutputHelper logger, ServerFixture server)
        {
            this.logger = logger;
            this.server = server;

            Global.UseSyncContextPost = false;
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
            //server.Server.SendConnectBeacon("127.0.0.1");

            //client.Connect("127.0.0.1");

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
            client.AddChannel(new DataChannel<bool>((short)ChannelId.BoolRel, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == boolData) boolCheck = true;
            }, checkMode));

            //Byte
            client.AddChannel(new DataChannel<byte>((short)ChannelId.ByteRel, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == byteData) byteCheck = true;
            }, checkMode));

            //Short
            client.AddChannel(new DataChannel<short>((short)ChannelId.ShortRel, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == shortData) shortCheck = true;
            }, checkMode));

            //Int
            client.AddChannel(new DataChannel<int>((short)ChannelId.IntRel, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == intData) intCheck = true;
            }, checkMode));

            //Float
            client.AddChannel(new DataChannel<float>((short)ChannelId.FloatRel, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == floatData) floatCheck = true;
            }, checkMode));

            //Double
            client.AddChannel(new DataChannel<double>((short)ChannelId.DoubleRel, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == doubleData) doubleCheck = true;
            }, checkMode));

            //String
            client.AddChannel(new DataChannel<string>((short)ChannelId.StringRel, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == stringData) stringCheck = true;
            }, checkMode));

            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRel, QosType.Reliable, comp, enc, (node, data) =>
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


            //Send
            client.Send((short)ChannelId.BoolRel, boolData);
            client.Send((short)ChannelId.ByteRel, byteData);
            client.Send((short)ChannelId.ShortRel, shortData);
            client.Send((short)ChannelId.IntRel, intData);
            client.Send((short)ChannelId.FloatRel, floatData);
            client.Send((short)ChannelId.DoubleRel, doubleData);
            client.Send((short)ChannelId.StringRel, stringData);
            client.Send((short)ChannelId.ClassRel, testData);

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
                else if (sw.Elapsed.Seconds >= 55)
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
            client.AddChannel(new DataChannel<bool>((short)ChannelId.BoolUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == boolData) boolCheck = true;
            }, checkMode));

            //Byte
            client.AddChannel(new DataChannel<byte>((short)ChannelId.ByteUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == byteData) byteCheck = true;
            }, checkMode));

            //Short
            client.AddChannel(new DataChannel<short>((short)ChannelId.ShortUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == shortData) shortCheck = true;
            }, checkMode));

            //Int
            client.AddChannel(new DataChannel<int>((short)ChannelId.IntUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == intData) intCheck = true;
            }, checkMode));

            //Float
            client.AddChannel(new DataChannel<float>((short)ChannelId.FloatUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == floatData) floatCheck = true;
            }, checkMode));

            //Double
            client.AddChannel(new DataChannel<double>((short)ChannelId.DoubleUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == doubleData) doubleCheck = true;
            }, checkMode));

            //String
            client.AddChannel(new DataChannel<string>((short)ChannelId.StringUnRel, QosType.Unreliable, comp, enc, (node, data) =>
            {
                if (data == stringData) stringCheck = true;
            }, checkMode));

            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, enc, (node, data) =>
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

            //Send
            client.Send((short)ChannelId.BoolUnRel, boolData);
            client.Send((short)ChannelId.ByteUnRel, byteData);
            client.Send((short)ChannelId.ShortUnRel, shortData);
            client.Send((short)ChannelId.IntUnRel, intData);
            client.Send((short)ChannelId.FloatUnRel, floatData);
            client.Send((short)ChannelId.DoubleUnRel, doubleData);
            client.Send((short)ChannelId.StringUnRel, stringData);
            client.Send((short)ChannelId.ClassUnRel, testData);

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
        public void SendReceiveRaw()
        {
            Util.Log("SendReceiveRaw");

            ComClient client = new ComClient();
            client.ListenPortNumber = this.server.SendPort;
            client.SendPortNumber = this.server.ListenPort;

            Connect(ref client);
            server.Server.BeaconStop();

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
            client.AddChannel(new DataChannel<bool>((short)ChannelId.BoolRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == boolData) boolCheck = true;
            }, checkMode));

            //Byte
            client.AddChannel(new DataChannel<byte>((short)ChannelId.ByteRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == byteData) byteCheck = true;
            }, checkMode));

            //Short
            client.AddChannel(new DataChannel<short>((short)ChannelId.ShortRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == shortData) shortCheck = true;
            }, checkMode));

            //Int
            client.AddChannel(new DataChannel<int>((short)ChannelId.IntRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == intData) intCheck = true;
            }, checkMode));

            //Float
            client.AddChannel(new DataChannel<float>((short)ChannelId.FloatRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == floatData) floatCheck = true;
            }, checkMode));

            //Double
            client.AddChannel(new DataChannel<double>((short)ChannelId.DoubleRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                Util.Log("double:" + data);
                if (data == doubleData) doubleCheck = true;
            }, checkMode));

            //String
            client.AddChannel(new DataChannel<string>((short)ChannelId.StringRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                if (data == stringData) stringCheck = true;
            }, checkMode));

            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRaw, QosType.Reliable, comp, enc, (node, data) =>
            {
                Util.Log("class:" + data);
                if (
                    data.intData == testData.intData
                    && data.floatData == testData.floatData
                    && data.stringData == testData.stringData
                )
                {
                    classCheck = true;
                }
            }, checkMode));


            //Send
            client.Send((short)ChannelId.BoolRaw, boolData);
            client.Send((short)ChannelId.ByteRaw, byteData);
            client.Send((short)ChannelId.ShortRaw, shortData);
            client.Send((short)ChannelId.IntRaw, intData);
            client.Send((short)ChannelId.FloatRaw, floatData);
            client.Send((short)ChannelId.DoubleRaw, doubleData);
            client.Send((short)ChannelId.StringRaw, stringData);
            client.Send((short)ChannelId.ClassRaw, testData);

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
                else if (sw.Elapsed.Seconds >= 2225)
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

            //Byte
            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassRel, QosType.Reliable, comp, enc, (node, data) =>
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

            Random random = new Random();
            sendTestNum = random.Next(500, 600);

            Util.Log("sendTestNum:" + sendTestNum);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            //Send
            for (int i = 0; i < sendTestNum; i++)
            {
                client.Send((short)ChannelId.ClassRel, testData);
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

            //Byte
            //Class
            client.AddChannel(new DataChannel<TestClass>((short)ChannelId.ClassUnRel, QosType.Unreliable, comp, enc, (node, data) =>
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

            Random random = new Random();
            sendTestNum = random.Next(500, 600);

            Util.Log("sendTestNum:" + sendTestNum);

            //Send
            for (int i = 0; i < sendTestNum; i++)
            {
                client.Send((short)ChannelId.ClassUnRel, testData);
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

 
    }
}
