using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSockets
{
    class TcpServerPro
    {
        public static void Start()
        {
            Socket listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Is socket is already taken, "SocketException: Only one usage of each socket
            // address (protocol/network address/port) is normally permitted (-2147467259)" is
            // thrown.
            listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 80));
            listenSocket.Listen(5);
            while (true)
            {
                using (Socket client = listenSocket.Accept())
                {
                    // We can improve this making it multithreading.
                    OnConnectionReceived(client);
                }
            }
        }

        private static void OnConnectionReceived(Socket client)
        {
            try
            {
                Console.WriteLine("{0} connected.", client.RemoteEndPoint);
                NetworkStream networkStream = new NetworkStream(client);

                while (true)
                {
                    // Read bytes in multiples of 16 to make it more fun.
                    byte[] buffer = new byte[16];
                    string request = "";
                    while (!request.EndsWith("\r\n"))
                    {
                        // If connection is closed before this call, "IOException: Unable to read
                        // data from the transport connection: An existing connection was forcibly
                        // closed by the remote host. (-2146232800)" is thrown.
                        int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            // If bytesRead is zero, incoming stream was closed.
                            Console.WriteLine("{0} disconnected.", client.RemoteEndPoint);
                            return;
                        }
                        request += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    }
                    Console.WriteLine(request);

                    string response = "Yes, I am ñoño. The time is " + DateTime.Now + ".\r\n";
                    buffer = Encoding.UTF8.GetBytes(response);

                    // If Write() is called multiple times, response may be sent in multiple TCP
                    // packets.
                    networkStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("{0} disconnected in a rude way.", client.RemoteEndPoint);
            }
        }
    }
}
