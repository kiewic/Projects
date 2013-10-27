using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSockets
{
    class TcpClientPro
    {
        public static void Start()
        {
            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                ConnectSocket(socket, "localhost", 80);

                NetworkStream networkStream = new NetworkStream(socket);

                string request = "Are you ñoño? Can you tell me what time is it?\r\n";
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
                        Console.WriteLine("Incoming stream from {0} closed.",
                            socket.RemoteEndPoint);
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
            // If host is unknown, "SocketException: No such host is known (-2147467259)" is
            // thrown.
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
                    // If remote host does not reply, "SocketException: No connection could be made
                    // because the target machine actively  refused it (-2147467259)" is thrown.
                    if (ex.HResult == -2147467259 && i + 1 != host.AddressList.Length)
                    {
                        continue;
                    }
                    throw;
                }
            }
        }
    }
}
