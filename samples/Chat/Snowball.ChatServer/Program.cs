using System;
using System.Net;
using MessagePack;

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

            server.AddChannel(new DataChannel(0, QosType.Reliable, CompressionType.None, (node, data) =>{
                string text = (string)data;
                server.Broadcast(room, 0, node.UserName + " > " + text);
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
