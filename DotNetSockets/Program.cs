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
                TcpServerPro.Start();
            }
            else if (args[0] == "tcp" && args[1] == "client")
            {
                TcpClientPro.Start();
            }
            else if (args[0] == "udp" && args[1] == "receive")
            {
                UdpReceivePro.Start();
            }
            else if (args[0] == "udp" && args[1] == "send")
            {
                UdpSendPro.Start();
            }
            else
            {
                PrintHelp();
            }
        }

        private static void PrintHelp()
        {
            string processName = AppDomain.CurrentDomain.FriendlyName;
            Console.WriteLine("Usage: {0} tcp {{ server | client }}", processName);
            Console.WriteLine("Usage: {0} udp {{ send | receive }}", processName);
        }
    }
}
