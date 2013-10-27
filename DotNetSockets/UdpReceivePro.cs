using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSockets
{
    class UdpReceivePro
    {
        public static void Start()
        {
            using (Socket udpSocket = new Socket(SocketType.Dgram, ProtocolType.Udp))
            {
                // If port is already taken, "SocketException: Only one usage of each socket
                // address (protocol/network address/port) is normally permitted (-2147467259)"
                // is thrown.
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
    }
}
