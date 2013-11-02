# Sockets

The following projects are about sockets (a lot of sockets!!) and they can communicate with each other:

* DotNetSockets | C# | System.Net.Sockets
* Win32Sockets | C++ | Winsock
* WindowsStoreSocketsCPP | C++/CX | Windows.Networking.Sockets
* WindowsStoreSocketsCS | C# | Windows.Networking.Sockets

Each project contains:

* TCP server socket
* TCP client socket
* UDP send socket
* UDP receive socket

#### How to run TCP sockets?

1. Run any version of TCP server.
2. Run any version of TCP client (change localhost, 127.0.0.1 or ::1 
   if you are running the server and the client on different machines).
3. See results:
  * TCP client sends a message ending with \r\n.
  * TCP server receives the message.
  * TCP server displays the message received.
  * TCP server replies with another message.
  * TCP client receives the message.
  * TCP client displays the message received.

#### How to run UDP sockets?

1. Run any  version of UDP receive socket.
2. Run any version of UDP send socket (change localhost, 127.0.0.1 or ::1 
   if you are running the receive socket and the send socket on different machines).
3. See results:
  * UDP send socket sends a message (this message doesn't end with \r\n beacuse datagrams
    size is limited to ~1050 bytes, then, we  can read all data in just one call).
  * UDP receive socket receives the message.
  * UDP receive socket displays the message received.

## More Win32Sockets

* DNS Lookup
* Reverse DNS Lookup

## WinInetForDummies

A project about WinInet.

## DebugMe

A project made to practice WinDBG.

More at http://monkeyweekend.wordpress.com/2013/10/27/set-a-breakpoint-that-breaks-every-time-a-function-returns/


