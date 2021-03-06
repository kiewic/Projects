﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using System.Runtime.InteropServices.WindowsRuntime;

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
            try
            {
                if (listener == null)
                {
                    // We call it 'local', becuase if this connection doesn't succeed, we do not want
                    // to loose the possible previous conneted listener.
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
            }
            catch (Exception ex)
            {
                DisplayOutput(TcpServerOutput, ex.ToString());
            }
        }

        private async void OnConnectionReceived(
            StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                DisplayOutput(TcpServerOutput, args.Socket.Information.RemoteAddress.DisplayName + " connected.");

                while (true)
                {
                    // Read request.
                    string request = await ReadUntilCrLf(args.Socket.InputStream, TcpServerOutput);
                    if (String.IsNullOrEmpty(request))
                    {
                        // If there was no request. The remote host closed the connection.
                        return;
                    }
                    DisplayOutput(TcpServerOutput, request);

                    // Send response.
                    string response = "Yes, I am ñoño. The time is " + DateTime.Now + ".\r\n";

                    // In this sample since the server doesn´t close the close the socket, we
                    // could do it async (i.e. without await)., but not now.
                    await Send(args.Socket.OutputStream, response);
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
            try
            {
                socket = new StreamSocket();

                await socket.ConnectAsync(new HostName("localhost"), "80");
                DisplayOutput(TcpClientOutput, "Connected.");
            }
            catch (Exception ex)
            {
                DisplayOutput(TcpClientOutput, ex.ToString());
            }
        }

        private async void SendRequest_Click_1(object sender, RoutedEventArgs e)
        {
            if (socket != null)
            {
                string request = "Are you ñoño? Can you tell me what time is it?\r\n";
                await Send(socket.OutputStream, request);

                string response = await ReadUntilCrLf(socket.InputStream, TcpClientOutput);
                DisplayOutput(TcpClientOutput, response);
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
        // TCP server and TCP client common.
        //

        private async Task<string> ReadUntilCrLf(IInputStream inputStream, TextBlock outputTextBlock)
        {
            DataReader reader = new DataReader(inputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;

            string message = "";
            while (!message.EndsWith("\r\n"))
            {
                // Read bytes in multiples of 16, to make it more fun.
                // If the socket is closed while we are reading, the "The I/O operation has
                // been aborted because of either a thread exit or an application request.
                // (Exception from HRESULT: 0x800703E3)" exception is thrown.
                uint bytesRead = await reader.LoadAsync(16);
                if (bytesRead == 0)
                {
                    // If bytesRead is zero, incoming stream was closed.
                    DisplayOutput(outputTextBlock, "The connection was closed by remote host.");
                    break;
                }
                // TODO: Why DataReader doesn't have ReadChar()?
                message += reader.ReadString(bytesRead);
            }

            // Do not use Dispose(). If used, streams cannot be used anymore.
            //reader.Dispose();

            // Without this, the DataReader destructor will close the stream, and closing the
            // stream might set the FIN control bit.
            reader.DetachStream();

            return message;
        }

        private async Task Send(IOutputStream outputStream, string message)
        {
            DataWriter writer = new DataWriter(outputStream);

            // This is useless in this sample. Just a friendly remainder.
            uint messageLength = writer.MeasureString(message);

            writer.WriteString(message);
            uint bytesWritten = await writer.StoreAsync();

            Debug.Assert(bytesWritten == messageLength);

            // Do not use Dispose(). If used, streams cannot be used anymore.
            //writer.Dispose();

            // Without this, the DataReader destructor will close the stream, and closing the
            // stream might set the FIN control bit.
            writer.DetachStream();
        }

        //
        // UDP receive.
        //

        // Make it global, if not, socket goes out of scope after first message is received.
        DatagramSocket receiveSocket = null;

        private async void ConnectUdpReceive_Click_1(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                DisplayOutput(UdpReceiveOutput, ex.ToString());
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
            try
            {
                DataReader reader = args.GetDataReader();
                reader.InputStreamOptions = InputStreamOptions.Partial;

                // LoadAsync not needed. The reader comes already loaded.

                // If called by a 'Udp send socket', next line throws an exception because message was not received.

                // If remote peer didn't received message, "An existing connection was forcibly
                // closed by the remote host. (Exception from HRESULT: 0x80072746)" exception is
                // thrown. Maybe only when using ConenctAsync(), not GetOutputStreamAsync().
                uint bytesRead = reader.UnconsumedBufferLength;
                string message = reader.ReadString(bytesRead);

                DisplayOutput(UdpReceiveOutput, "Message received from [" +
                    args.RemoteAddress.DisplayName + "]:" + args.RemotePort + ": " + message);
            }
            catch (Exception ex)
            {
                DisplayOutput(UdpSendOutput, "Peer didn't receive message.");
            }
        }

        //
        // UDP send.
        //

        private async void UdpSend_Click_1(object sender, RoutedEventArgs e)
        {
            DatagramSocket sendSocket = new DatagramSocket();

            // Even when we do not except any response, this handler is called if any error occurrs.
            sendSocket.MessageReceived += OnMessageReceived;

            try
            {
                await sendSocket.ConnectAsync(new HostName("foohost"), "2704");
                // DatagramSocket.ConnectAsync() vs DatagramSocket.GetOutputStreamAsync()?
                // Use DatagramSocket.GetOutputStreamAsync() if datagrams are sent to multiple
                // GetOutputStreamAsync() does DNS resolution first.
                // If remote host does not exist, "No such host is known. (Exception from HRESULT: 0x80072AF9)"
                // exception is thrown.
                // If remote host is not listening on the specified host, "An existing connection was forcibly
                // closed by the remote host. (Exception from HRESULT: 0x80072746)" exception is thrown.

                string message = "¡Hello, I am the new guy in the network!";
                DataWriter writer = new DataWriter(sendSocket.OutputStream);

                // This is useless in this sample. Just a friendly remainder.
                uint messageLength = writer.MeasureString(message);

                writer.WriteString(message);

                uint bytesWritten = await writer.StoreAsync();

                Debug.Assert(bytesWritten == messageLength);

                DisplayOutput(UdpSendOutput, "Message sent: " + message);
            }
            catch (Exception ex)
            {
                DisplayOutput(UdpSendOutput, ex.ToString());
            }
        }

    }
}
