using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsStoreSockets
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void DisplayOutput(TextBlock textBlock, string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => textBlock.Text = message);
        }

        //
        // TCP server.
        //
        StreamSocketListener listener = null;

        private async void StartServer_Click_1(object sender, RoutedEventArgs e)
        {
            listener = new StreamSocketListener();

            // ConnectionReceived handler must be set before BindServiceNameAsync is called, if not
            // "A method was called at an unexpected time. (Exception from HRESULT: 0x8000000E)"
            // error occurs.
            listener.ConnectionReceived += OnConnectionReceived;

            // Trying to bind more than once to the same port throws "Only one usage of each socket
            // address (protocol/network address/port) is normally permitted. (Exception from
            // HRESULT: 0x80072740)" exception.
            await listener.BindServiceNameAsync("80");
            DisplayOutput(TcpServerOutput, "Listening.");
        }

        private async void OnConnectionReceived(
            StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                DataReader reader = new DataReader(args.Socket.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
                DataWriter writer = new DataWriter(args.Socket.OutputStream);

                DisplayOutput(TcpServerOutput, args.Socket.Information.RemoteAddress.DisplayName +
                    " connected.");

                while (true)
                {
                    // Read request.
                    string request = "";
                    while (!request.EndsWith("\r\n"))
                    {
                        // Read bytes in multiples of 16 to make it more fun.
                        uint bytesRead = await reader.LoadAsync(16);
                        if (bytesRead == 0)
                        {
                            // If bytesRead is zero, incoming stream was closed.
                            DisplayOutput(TcpServerOutput, "The client is gone.");
                            return;
                        }
                        request += reader.ReadString(bytesRead);
                    }
                    DisplayOutput(TcpServerOutput, request);

                    // Send response.
                    string response = "Yes, I am ñoño. The time is " + DateTime.Now + ".\r\n";

                    // This is useless in this sample.
                    uint responseLength = writer.MeasureString(response);

                    writer.WriteString(response);
                    uint bytesWritten = await writer.StoreAsync();

                    Debug.Assert(bytesWritten == responseLength);
                }
            }
            catch (Exception ex)
            {
                DisplayOutput(TcpServerOutput, ex.ToString());
            }
        }

        private void StopServer_Click_1(object sender, RoutedEventArgs e)
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
                DisplayOutput(TcpServerOutput, "Not listening anymore.");
            }
        }

        //
        // TCP client.
        //

        private StreamSocket socket = null;

        private async void ConnectClient_Click_1(object sender, RoutedEventArgs e)
        {
            socket = new StreamSocket();

            await socket.ConnectAsync(new HostName("localhost"), "80");
            DisplayOutput(TcpClientOutput, "Connected.");
        }

        private async void SendRequest_Click_1(object sender, RoutedEventArgs e)
        {
            if (socket != null)
            {
                DataReader reader = new DataReader(socket.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
                DataWriter writer = new DataWriter(socket.OutputStream);

                string request = "Are you ñoño? Can you tell me what time is it?\r\n";

                writer.WriteString(request);
                uint bytesWritten = await writer.StoreAsync();

                // Why DataReader doesn't have ReadChar()?
                string response = "";
                while (!response.EndsWith("\r\n"))
                {
                    // Read bytes in multiples of 16, to make it more fun.
                    // If the socket is closed while we are reading, the "The I/O operation has
                    // been aborted because of either a thread exit or an application request.
                    // (Exception from HRESULT: 0x800703E3)" exception is thrown.
                    uint bytesRead = await reader.LoadAsync(16);
                    if (bytesRead == 0)
                    {
                        // If bytesRead is zero, incoming stream was closed.
                        DisplayOutput(TcpClientOutput, "The server is gone.");
                        return;
                    }
                    response += reader.ReadString(bytesRead);
                }
                DisplayOutput(TcpClientOutput, response);

                // Do not use Dispose(). If used, streams cannot be used anymore.
                //reader.Dispose();
                //writer.Dispose();

                // I think this is needed, because without this, the DataReader destructor closes
                // the stream, and closing the stream might set the FIN control bit.
                reader.DetachStream();
                writer.DetachStream();
            }
        }

        private void DisconnectClient_Click_1(object sender, RoutedEventArgs e)
        {
            if (socket != null)
            {
                socket.Dispose();
                socket = null;
                DisplayOutput(TcpClientOutput, "Closed.");
            }
        }

        //
        // UDP receive.
        //

        // Make it global, if not, socket goes out of scope after first message is received.
        DatagramSocket receiveSocket = null;

        private async void ConnectUdpReceive_Click_1(object sender, RoutedEventArgs e)
        {
            if (receiveSocket == null)
            {
                receiveSocket = new DatagramSocket();

                // MessageReceived handler must be set before BindServiceAsync is called, if not
                // "A method was called at an unexpected time. (Exception from HRESULT: 
                // 0x8000000E)" exception is thrown.
                receiveSocket.MessageReceived += OnMessageReceived;

                // If port is already in used by another socket, "Only one usage of each socket
                // address (protocol/network address/port) is normally permitted. (Exception from
                // HRESULT: 0x80072740)" exception is thrown.
                await receiveSocket.BindServiceNameAsync("2704");

                DisplayOutput(UdpReceiveOutput, "Connected (bound).");
            }
        }

        private void DisconnectUdpReceive_Click_1(object sender, RoutedEventArgs e)
        {
            if (receiveSocket != null)
            {
                receiveSocket.Dispose();
                receiveSocket = null;
                DisplayOutput(UdpReceiveOutput, "Disconnected.");
            }
        }

        private void OnMessageReceived(
            DatagramSocket sender,
            DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            reader.InputStreamOptions = InputStreamOptions.Partial;
            // LoadAsync not needed. The reader comes already loaded.
            uint bytesRead = reader.UnconsumedBufferLength;
            string message = reader.ReadString(bytesRead);

            DisplayOutput(UdpReceiveOutput, "Message received from [" + 
                args.RemoteAddress.DisplayName + "]:" + args.RemotePort + ": " + message);
        }

        //
        // UDP send.
        //

        private async void UdpSend_Click_1(object sender, RoutedEventArgs e)
        {
            DatagramSocket socket = new DatagramSocket();

            // Even when we do not except any response, this handler is called if any error
            // occurrs.
            socket.MessageReceived += socket_MessageReceived;

            // DatagramSocket.ConnectAsync() vs datagramSocket.GetOutputStreamAsync()?
            await socket.ConnectAsync(new HostName("localhost"), "2704");

            DataWriter writer = new DataWriter(socket.OutputStream);
            string message = "¡Hello, I am the new guy in the network!";
            writer.WriteString(message);

            // If GetOutputStreamAsync was used instead of ConnectAsync, "An existing connection
            // was forcibly closed by the remote host. (Exception from HRESULT: 0x80072746)"
            // exepction is thrown.
            uint bytesWritten = await writer.StoreAsync();

            DisplayOutput(UdpSendOutput, "Message sent: " + message);
        }

        private void socket_MessageReceived(
            DatagramSocket sender,
            DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                // If remote peer didn't received message, "An existing connection was forcibly
                // closed by the remote host. (Exception from HRESULT: 0x80072746)" exception is
                // thrown.
                uint bytesRead = args.GetDataReader().UnconsumedBufferLength;
            }
            catch (Exception)
            {
                DisplayOutput(UdpSendOutput, "Peer didn't receive message.");
            }
        }

    }
}
