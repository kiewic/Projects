using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSockets
{
    class UdpSendPro
    {
        public static void Start()
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
