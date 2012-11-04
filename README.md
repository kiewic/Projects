# SocketsSockets

This is a collection of projects about sockets. Lots of sockets!!

We have sockets in many flavors:

* Win32
* .NET Framework (C#)
* Windows Runtime/Windows Store Apps (C#)

Each project contains:

* TCP server socket
* TCP client socket
* UDP send socket
* UDP receive socket

### How do TCP sockets work?

1. Run any version of TCP server.
2. Run any version of TCP client (remember to change localhost, 127.0.0.1 or ::1 
   if you are running server and client in separate machines).
3. See results. 
  * TCP client sends a message ending with \r\n.
  * TCP server receives the message.
  * TCP server displays the message received.
  * TCP server replies with another message.
  * TCP client receives the message.
  * TCP client displays the message received.

### How do UDP sockets work?

1. Run any  version of UDP receive socket.
2. Run any version of UDP send socket (remember to change localhost, 127.0.0.1 or ::1 
   if you are running server and client in separate machines).
3. See results. 
  * UDP send socket sends a message (this message doesn't end with \r\n beacuse datagrams
    size is limited to ~1050 bytes, then, we  can read all data in just one call).
  * UDP receive socket receives the message.
  * UDP receive socket displays the message received.

### More info at:

> http://kiewic.com/sockets
