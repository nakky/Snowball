using System;
using System.Net;

using Snowball;

namespace Snowball.ChatServer
{
    class Program
    {
        static ComServer server = new ComServer();

        static ComGroup room = new ComGroup("Default");

        static void Main(string[] args)
        {
            server.AddBeaconList(IPAddress.Broadcast.ToString());

            server.OnConnected += (node) =>
            {
                room.Add(node);
                server.Broadcast(room, 0, node.UserName + " is logined.");
            };

            server.OnDisconnected += (node) =>
            {
                room.Remove(node);
                server.Broadcast(room, 0, node.UserName + " is logouted.");
            };

            server.AddChannel(new DataChannel<string>(0, QosType.Reliable, Compression.LZ4, Encryption.Aes, (node, data) =>{
                server.Broadcast(room, 0, node.UserName + " > " + data);
            }));

            server.Open();
            server.BeaconStart();

            Console.WriteLine("Running... Press any key to stop...");
            Console.ReadLine();
            Console.WriteLine("Shutdow...");

            server.Close();
        }
    }
}
