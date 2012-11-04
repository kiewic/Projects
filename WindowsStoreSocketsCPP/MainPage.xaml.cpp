//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"
#include <ppltasks.h> // task
#include <assert.h> // assert

using namespace WindowsStoreSocketsCPP;

using namespace Concurrency;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::Globalization;
using namespace Windows::Globalization::DateTimeFormatting;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

MainPage::MainPage() : listener(nullptr), socket(nullptr), receiveSocket(nullptr)
{
    InitializeComponent();
}

void MainPage::OnNavigatedTo(NavigationEventArgs^ e)
{
    (void) e; // Unused parameter
}

void MainPage::DisplayOutput(TextBlock^ textBlock, String^ message)
{
    this->Dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this, textBlock, message]()
    {
        textBlock->Text = message;
    }));
}

//
// TCP server.
//

void MainPage::StartServer_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    if (this->listener == nullptr)
    {
        // We call it 'local', becuase if this connection doesn't succeed, we do not want
        // to loose the possible previous conneted listener.
        this->listener = ref new StreamSocketListener();

        // ConnectionReceived handler must be set before BindServiceNameAsync is called, if not
        // "A method was called at an unexpected time. (Exception from HRESULT: 0x8000000E)"
        // error occurs.
        this->listener->ConnectionReceived +=
            ref new TypedEventHandler<StreamSocketListener^, StreamSocketListenerConnectionReceivedEventArgs^>(
                this,
                &MainPage::OnConnectionReceived);

        task<void>(this->listener->BindServiceNameAsync("80")).then([=](task<void> t)
        {
            try
            {
                // Try getting all exceptions from the continuation chain above this point.
                t.get();

                DisplayOutput(TcpServerOutput, "Listening.");

                this->listener = listener;
            }
            catch (Exception^ exception)
            {
                DisplayOutput(TcpServerOutput, exception->ToString());
            }
        });
    }
}

void MainPage::StopServer_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    if (this->listener != nullptr)
    {
        //this->listener->Close();
        this->listener = nullptr;
        DisplayOutput(TcpServerOutput, "Not listening anymore.");
    }
}

bool MainPage::EndsWithCrLf(String^ str)
{
    unsigned int length = str->Length();
    if (length >= 2)
    {
        const wchar_t* strChars = str->Data();
        if (strChars[length - 2] == '\r' && strChars[length - 1] == '\n')
        {
            return true;
        }
    }

    return false;
}

void MainPage::OnConnectionReceived(StreamSocketListener^ listener, StreamSocketListenerConnectionReceivedEventArgs^ args)
{
    try
    {
        DisplayOutput(TcpServerOutput, args->Socket->Information->RemoteAddress->DisplayName + " connected.");

        DataReader^ reader = ref new DataReader(args->Socket->InputStream);
        reader->InputStreamOptions = InputStreamOptions::Partial;

        DataWriter^ writer = ref new DataWriter(args->Socket->OutputStream);

        this->requestReceived = "";
        DoReceiveRequest(reader, writer);
    }
    catch (Exception^ exception)
    {
        DisplayOutput(TcpServerOutput, exception->ToString());
    }
}

void MainPage::DoReceiveRequest(DataReader^ reader, DataWriter^ writer)
{
    if (EndsWithCrLf(this->requestReceived))
    {
        // Done receiving, display request.
        DisplayOutput(TcpServerOutput, this->requestReceived);
        this->requestReceived = "";

        // Do not detach, we will still need the reader.
        //reader->DetachStream();

        DoSendResponse(reader, writer);
        return;
    }

    task<size_t> loadTask = create_task(reader->LoadAsync(16));
    loadTask.then([=](task<size_t> loadTask)
    {
        size_t bytesRead = loadTask.get();
        if (bytesRead == 0)
        {
            // If bytesRead is zero, incoming stream was closed.
            DisplayOutput(TcpServerOutput, "The connection was closed by remote host.");
            return;
        }

        this->requestReceived += reader->ReadString(bytesRead);

        DoReceiveRequest(reader, writer);
    });
}

void MainPage::DoSendResponse(DataReader^ reader, DataWriter^ writer)
{
    Calendar^ calendar = ref new Calendar();
    DateTimeFormatter^ longtime = ref new DateTimeFormatter("longtime");
    DateTime time = calendar->GetDateTime();
    String^ response = "Yes, I am ñoño. The time is " + longtime->Format(time) + ".\r\n";

    // This is useless in this sample. Just a friendly remainder.
    size_t responseLength = writer->MeasureString(response);

    writer->WriteString(response);

    task<size_t> storeTask = create_task(writer->StoreAsync());
    storeTask.then([=](task<size_t> t)
    {
        size_t bytesWritten = t.get();

        assert(bytesWritten == responseLength);

        // Do not detach, we will still need the writer.
        //writer->DetachStream();

        DoReceiveRequest(reader, writer);
    });
}

//
// TCP client.
//

void MainPage::ConnectClient_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    try
    {
        this->socket = ref new StreamSocket();

        task<void>(this->socket->ConnectAsync(ref new HostName("localhost"), "80")).then([=](task<void> previousTask)
        {
            // Try getting all exceptions from the continuation chain above this point.
            previousTask.get();

            DisplayOutput(TcpClientOutput, "Connected.");
        });
    }
    catch (Exception^ exception)
    {
        DisplayOutput(TcpClientOutput, exception->ToString());
    }
}

void MainPage::SendRequest_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    if (this->socket != nullptr)
    {
        DataReader^ reader = ref new DataReader(this->socket->InputStream);
        reader->InputStreamOptions = InputStreamOptions::Partial;

        DataWriter^ writer = ref new DataWriter(this->socket->OutputStream);

        DoSendRequest(reader, writer);
    }
}

void MainPage::DisconnectClient_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    if (this->socket != nullptr)
    {
        //this->socket->Close();
        this->socket = nullptr;
        DisplayOutput(TcpClientOutput, "Closed.");
    }
}

void MainPage::DoSendRequest(DataReader^ reader, DataWriter^ writer)
{
    String^ request = "Are you ñoño? Can you tell me what time is it?\r\n";

    // This is useless in this sample. Just a friendly remainder.
    size_t responseLength = writer->MeasureString(request);

    writer->WriteString(request);

    task<size_t> storeTask = create_task(writer->StoreAsync());
    storeTask.then([=](task<size_t> t)
    {
        size_t bytesWritten = t.get();

        assert(bytesWritten == responseLength);

        // Without this, the DataReader destructor will close the stream, and closing the
        // stream might set the FIN control bit.
        writer->DetachStream();

        this->responseReceived = "";
        DoReceiveResponse(reader);
    });
}

void MainPage::DoReceiveResponse(DataReader^ reader)
{
    if (EndsWithCrLf(this->responseReceived))
    {
        // Done receiving, display response.
        DisplayOutput(TcpClientOutput, this->responseReceived);
        this->responseReceived = "";

        // Without this, the DataWriter destructor will close the stream, and closing the
        // stream might set the FIN control bit.
        reader->DetachStream();

        return;
    }

    try
    {
         // TODO: If we use 16 instead of 17, the Left-to-right mark utf-8 representation is truncated.
        task<size_t> loadTask = create_task(reader->LoadAsync(17));
        loadTask.then([=](task<size_t> loadTask)
        {
            try
            {
                size_t bytesRead = loadTask.get();
                if (bytesRead == 0)
                {
                    // If bytesRead is zero, incoming stream was closed.
                    DisplayOutput(TcpClientOutput, "The connection was closed by remote host.");
                    return;
                }

                this->responseReceived += reader->ReadString(bytesRead);

                DoReceiveResponse(reader);
            }
            catch (Exception^ exception)
            {
                DisplayOutput(TcpClientOutput, exception->ToString());
            }
        });
    }
    catch (Exception^ exception)
    {
        DisplayOutput(TcpClientOutput, exception->ToString());
    }
}

//
// UDP receive.
//

void MainPage::ConnectUdpReceive_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    if (this->receiveSocket == nullptr)
    {
        this->receiveSocket = ref new DatagramSocket();

        // MessageReceived handler must be set before BindServiceAsync is called, if not
        // "A method was called at an unexpected time. (Exception from HRESULT: 
        // 0x8000000E)" exception is thrown.
        this->receiveSocket->MessageReceived += ref new TypedEventHandler<DatagramSocket^, DatagramSocketMessageReceivedEventArgs^>(
            this,
            &MainPage::OnMessageReceived);

        // If port is already in used by another socket, "Only one usage of each socket
        // address (protocol/network address/port) is normally permitted. (Exception from
        // HRESULT: 0x80072740)" exception is thrown.
        task<void>(receiveSocket->BindServiceNameAsync("2704")).then([=](task<void> t){
            try
            {
                // Was an exception throw?
                t.get();

                DisplayOutput(UdpReceiveOutput, "Connected (bound).");
            }
            catch (Exception^ exception)
            {
                DisplayOutput(UdpReceiveOutput, exception->ToString());
            }
        });
    }
}

void MainPage::DisconnectUdpReceive_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    if (this->receiveSocket != nullptr)
    {
        //receiveSocket->Close();
        receiveSocket = nullptr;
        DisplayOutput(UdpReceiveOutput, "Disconnected.");
    }
}

//
// UDP send.
//

void MainPage::UdpSend_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    DatagramSocket^ socket = ref new DatagramSocket();

    // Even when we do not except any response, this handler is called if any error occurrs.
    socket->MessageReceived += ref new TypedEventHandler<DatagramSocket^, DatagramSocketMessageReceivedEventArgs^>(
        this,
        &MainPage::OnMessageReceived);

    // TODO: DatagramSocket.ConnectAsync() vs datagramSocket.GetOutputStreamAsync()?

    // If GetOutputStreamAsync was used instead of ConnectAsync, "An existing connection
    // was forcibly closed by the remote host. (Exception from HRESULT: 0x80072746)"
    // exepction is thrown.
    task<void>(socket->ConnectAsync(ref new HostName("localhost"), "2704")).then([=](task<void> t)
    {
        // Was an exception throw?
        t.get();

        DataWriter^ writer = ref new DataWriter(socket->OutputStream);
        String^ message = "¡Hello, I am the new guy in the network!";

        // This is useless in this sample. Just a friendly remainder.
        size_t messageLength = writer->MeasureString(message);

        writer->WriteString(message);

        task<size_t>(writer->StoreAsync()).then([=](task<size_t> t){
            // Was an exception throw?
            size_t bytesWritten = t.get();

            assert(bytesWritten == messageLength);

            DisplayOutput(UdpSendOutput, "Message sent: " + message);
        });
    });
}

void MainPage::OnMessageReceived(DatagramSocket^ sender, DatagramSocketMessageReceivedEventArgs^ args)
{
    try
    {
        // Udp receive:
        DataReader^ reader = args->GetDataReader();
        reader->InputStreamOptions = InputStreamOptions::Partial;

        // LoadAsync not needed. The reader comes already loaded.

        // If called by a 'Udp send socket', next line throws an exception because message was not received.

        // If remote peer didn't received message, "An existing connection was forcibly
        // closed by the remote host. (Exception from HRESULT: 0x80072746)" exception is
        // thrown.
        size_t bytesRead = reader->UnconsumedBufferLength;
        String^ message = reader->ReadString(bytesRead);

        DisplayOutput(UdpReceiveOutput, "Message received from [" + 
            args->RemoteAddress->DisplayName + "]:" + args->RemotePort + ": " + message);
    }
    catch (Exception^ exception)
    {
        DisplayOutput(UdpSendOutput, "Peer didn't receive message.");
    }
}

