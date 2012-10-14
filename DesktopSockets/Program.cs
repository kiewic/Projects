using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DesktopSockets
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintHelp();
            }
            else if (args[0] == "tcp" && args[1] == "server")
            {
                DoTcpServer();
            }
            else if (args[0] == "tcp" && args[1] == "client")
            {
                DoTcpClient();
            }
            else if (args[0] == "udp" && args[1] == "receive")
            {
                DoUdpReceive();
            }
            else if (args[0] == "udp" && args[1] == "send")
            {
                DoUdpSend();
            }
            else
            {
                PrintHelp();
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: {0} tcp {{ server | client }}", Environment.GetCommandLineArgs()[0]);
            Console.WriteLine("Usage: {0} udp {{ send | receive }}", Environment.GetCommandLineArgs()[0]);
        }

        private static void DoTcpServer()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Is socket is already taken, "SocketException: Only one usage of each socket address (protocol/network
            // address/port) is normally permitted (-2147467259)" is thrown.
            socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 80));
            socket.Listen(5);
            while (true)
            {
                using (Socket client = socket.Accept())
                {
                    // We cna improve this making it multithreading.
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
                        int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            // If bytesRead is zero, incoming stream was closed.
                            Console.WriteLine("{0} incoming stream closed.", client.RemoteEndPoint);
                            return;
                        }
                        request += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    }
                    Console.WriteLine(request);

                    string response = "Yes, I am ñoño. The time is " + DateTime.Now + ".\r\n";
                    buffer = Encoding.UTF8.GetBytes(response);

                    // If Write() is called multiple times, response may be sent in multiple TCP packets.
                    networkStream.Write(buffer, 0, buffer.Length);
                }
            }
            //catch (IOException)
            finally
            {
                Console.WriteLine("{0} disconnected.", client.RemoteEndPoint);
            }
        }

        private static void DoTcpClient()
        {
            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                ConnectSocket(socket, "localhost", 80);

                NetworkStream networkStream = new NetworkStream(socket);

                string request = "Are you noño? Can you tell me the time?\r\n";
                byte[] buffer = Encoding.UTF8.GetBytes(request);
                networkStream.Write(buffer, 0, buffer.Length);

                // Read bytes in multiples of 16 to make it more fun.
                buffer = new byte[16];
                string response = "";
                while (!response.EndsWith("\r\n"))
                {
                    int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // If bytesRead is zero, incoming stream was closed.
                        Console.WriteLine("Incoming stream from {0} closed.", socket.RemoteEndPoint);
                        return;
                    }
                    response += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
                Console.WriteLine(response);

                Console.WriteLine("Disconnecting from {0}.", socket.RemoteEndPoint);
            }
        }

        private static void ConnectSocket(Socket socket, string hostname, int port)
        {
            // If host is unknown, "SocketException: No such host is known (-2147467259)" is thrown.
            IPHostEntry host = Dns.GetHostEntry(hostname);

            for (int i = 0; i < host.AddressList.Length; i++)
            {
                try
                {
                    socket.Connect(host.AddressList[i], port);
                    Console.WriteLine("Connected to {0}.", socket.RemoteEndPoint);
                    break;
                }
                catch (SocketException ex)
                {
                    // If remote host does not reply, "SocketException: No connection could be made because the target
                    // machine actively  refused it (-2147467259)" is thrown.
                    if (ex.HResult == -2147467259 && i + 1 != host.AddressList.Length)
                    {
                        continue;
                    }
                    throw;
                }
            }
        }

        private static void DoUdpReceive()
        {
            using (Socket udpSocket = new Socket(SocketType.Dgram, ProtocolType.Udp))
            {
                // If port is already taken, "SocketException: Only one usage of each socket address (protocol/network
                // address/port) is normally permitted (-2147467259)" is thrown.
                udpSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 2704));

                while (true)
                {
                    // In theory, 1500 is the maximum size of a datagram.
                    byte[] buffer = new byte[1500];
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                    int bytesReceived = udpSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                    Console.WriteLine("Message received from {0}: {1}", remoteEndPoint, message);
                }
            }
        }

        private static void DoUdpSend()
        {
            using (Socket udpSocket = new Socket(SocketType.Dgram, ProtocolType.Udp))
            {
                string message = "¡Hello, I am the new guy in the network!";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("::1"), 2704);
                int bytesSent = udpSocket.SendTo(buffer, remoteEndPoint);
                Console.WriteLine("Message sent to {0}: {1}", remoteEndPoint, message);
            }
        }

    }
}
