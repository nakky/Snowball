# Snowball (.NET)

Simple Communication Engine for .NET Core.

## What is Snowball?

Snowball is a Server-Client network engine for C#. It communicate with other terminals on TCP and UDP.

You can use Snowball for communication in LAN (ex. VR/AR, Installation, and so on), and communication via Internet. Programming Interfaces of this engine are very simple, and allow intuitive use. Configuration and Data Channel Registration, almost all these things have to be done.

### Data Channel

Data Channel is a data exchange arrangements, manages data transfar.  
For example, you want to transfar chat text data between server and client, you should create a Data Channel for text data translation and register server and client respectively.
Data Channel can be set QoS Type (Reliable / Unreliable), and compression setting.

### Beacon

In Snowball, server can send beacon signal, and when a client catches a beacon, it connects to server without setting IP Address manually.  
If you want to implement communication between terminals in a LAN, Broadcast beacon is very useful.


## Installation

In preparation.

<!-- 
### .NET 
We recommend to insntall Stable Snowball using Nuget.

```
Install-Package Snowball
```
-->

## Quick Start

In preparation.


## License

This library is under the MIT License.
Snowball is using "MessagePack for C#" for packing data.
