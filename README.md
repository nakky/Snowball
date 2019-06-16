# Snowball (.NET Core, Unity)

<img src="https://user-images.githubusercontent.com/5203051/59557925-fd07a280-9020-11e9-8a39-aa5d48fc5215.png" height="400">

Simple Communication Engine for .NET Core and Unity.

## What is Snowball?

Snowball is a Server-Client network engine for C#. It communicate with other terminals on TCP and UDP.

Snowball can be used for communication in LAN (ex. VR/AR, IoT, and so on), and communication via Internet. Programming Interfaces of this engine are very simple, and allow intuitive use. Configuration and Data Channel Registration, almost all these things have to be done.


## Installation

### .NET 
We recommend to insntall Stable Snowball using Nuget.

```
Install-Package Snowball
```

### Unity  
You can download Unity Package of Snowball from [github](https://github.com/nakky/Snowball/releases/).

You must install [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) because Snowball depends on it. Unity Packages of MessagePack are distributed in [github](https://github.com/neuecc/MessagePack-CSharp/releases).

In Snowball, relatively new features of C# are userd (ex. async/await), so you must set "Scripting Runtime Version" to ". NET 4.x".

Some features of MessagePack are limited by environment and settings. 
You must set "API Compatibility Level" to ". NET 4.x" in Player Settings. Refer to MessagePack documentations for more details.

## Quick Start
Declear "using" directive if you need.

```csharp
using Snowball;
```

Server side examples are as follows.  

```csharp
ComServer server = new ComServer();

//Add Data Channel (Id 0 is string data transfar)
server.AddChannel(new DataChannel<string>(0, QosType.Reliable, Compression.None, (node, data) =>{
	Util.Log("Server Receive:" + data);                
}));

//Beacon Settings(Broadcast)
server.AddBeaconList(IPAddress.Broadcast.ToString());

//Start Server
server.Open();

//Beacon Start
server.BeaconStart();
```

Those of client side are as follows. 

```csharp
ComClient client = new ComClient();

//Add Data Channel (Id 0 is string data transfar)
client.AddChannel(new DataChannel<string>(0, QosType.Reliable, Compression.None, (node, data) => {
	Util.Log("Client Receive:" + data);       
}));

//Set Accept Beacon Flag 
client.AcceptBeacon = true;
//Start Client
client.Open();
```
Beacon is useful to connect/reconnet to a server. But Beacon involves Security Risk, so if you use Beacon via internet, the service must be carefully designed.

Sending examples are as follows. 

```csharp
ComNode node = server.GetNodeByIp(ip);
server.SendData(node, 0, "Hello Client!");
```
```csharp
client.SendData(0, "Hello Server!");
```

And ComClient and ComServer should be closed on termination. (ex. Dispose())
 
```csharp
server.Close();
```

```csharp
client.Close();
```
### Unity

In Unity project, you can use ComServer/ComClient as in .NET.
But Unity has useful archtecture (Inspector, and so on), so we implement wrapper which is inherited from MonoBehaviour. (Snowball.Server/Snowball.Client)  
You can use them like ComServer and ComClient, and also set default parameters by Inspector.

```csharp
[SerializeField]
Server server;
~~~
server.Open();
```

In Unity, Prefab is also useful features, so we prepared some Prefabs in Snowball Unity Package.

## Overview

Snowball consists of "ComServer" and "ComClient". ComClient connect to ComServer, and transfer data via Data Channels.  

### Server/Client

ComServer and ComClient can be set some parameters (ex. Send Port, Receive Port, Buffer Size...). You can set those parameters before Open().  
Snowball use UDP, though if you want to test on localhost, you can not use same port in Send and Receive (Two UDP sockets can not be bind to the same port). In those case, you should set contrasting numbers to Server and Client respectively.  
(Default port numbers are contrasting.)

```csharp
server.SendPortNumber = 50001;
server.ListenPortNumber = 50002;
server.Open();
```

```csharp
client.SendPortNumber = 50002;
client.ListenPortNumber = 50001;
client.Open();
```


### Data Channel

<img src="https://user-images.githubusercontent.com/5203051/59557925-fd07a280-9020-11e9-8a39-aa5d48fc5215.png" height="400">

ComServer and ComClient are registered some Data Channels. A Data Channel has transfer settings and can be set a Data Receive Handler.  
For example, if you want to transfer chat text data between server and client, you should create a Data Channel for text data translation and register server and client respectively.  

Data Channel also can be set QoS Type (Reliable / Unreliable), and compression setting. 

```csharp
client.AddChannel(new DataChannel<string>(0, QosType.Reliable, Compression.None, (node, data) => {
	Util.Log("Client Receive:" + data);       
}));
client.AddChannel(new DataChannel<TestClass>(1, QosType.Unreliable, Compression.LZ4, (node, data) => {
	Util.Log("Client Receive:" + data.ToString());       
}));
```

### Data Types

Because of using "[MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp)", Snowball can transfer data which type is supported by MessagePack.
So you can transfer class instances if you define those classes in a way of MessagePack.

```csharp
[MessagePackObject]
public class TestClass
{
    [Key(0)]
    public int intData;
    [Key(1)]
    public float floatData;
    [Key(2)]
    public string stringData;
};
```
```csharp
TestClass testClass = new TestClass();
~~~
client.SendData(1, testClass);
```
### Broadcast
In Snowball, each terminals are expressed as ComNode. So in Server, ComNode is specified in Sending APIs.  
We also implement BroadCasting API and Group of ComNode as ComGroup, and ComGroup is specified in BroadCasting API in Server. 

```csharp
ComNode node = server.GetNodeByIp(ip);
server.SendData(node, 0, "Hello Client!");

ComGroup group = new ComGroup("testGroup");
group.Add(node);
server.Broadcast(group, 0, "Hello Everyone!");
```

### Beacon

<img src="https://user-images.githubusercontent.com/5203051/59557933-1c9ecb00-9021-11e9-923d-531089a22b3c.png" height="400">

ComServer can send beacon signals at regular intervals, and when a client catches a beacon, it connects to server without setting IP Address manually.  
If you want to implement communication between terminals in a LAN, Broadcast beacon is very useful.

```csharp
//Beacon Settings(Broadcast)
server.AddBeaconList(IPAddress.Broadcast.ToString());
//Beacon Start
server.BeaconStart();
```

```csharp
//Set Accept Beacon Flag 
client.AcceptBeacon = true;
```
You can also implement detail about beacon.  
Server can be set a function about generating beacon data. 

```csharp
server.SetBeaconDataCreateFunction(() => {
    return "Test";
});
```

Also, client can be set a function about checking beacon data. 

```csharp
client.SetBeaconAcceptFunction((data) => {
	if (data == "Test") return true;
	else return false;
});
```

## License

This library is under the MIT License.  
Snowball is using [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) for packing data.
