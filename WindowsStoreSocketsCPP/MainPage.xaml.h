//
// MainPage.xaml.h
// Declaration of the MainPage class.
//

#pragma once

#include "MainPage.g.h"

using namespace Platform; // String
using namespace Windows::Networking::Sockets;
using namespace Windows::Networking;
using namespace Windows::Storage::Streams;
using namespace Windows::UI::Xaml::Controls;

namespace WindowsStoreSocketsCPP
{
    public ref class MainPage sealed
    {
    public:
        MainPage();

    protected:
        virtual void OnNavigatedTo(Windows::UI::Xaml::Navigation::NavigationEventArgs^ e) override;

    private:
        void DisplayOutput(TextBlock^ textBlock, String^ message);
        bool EndsWithCrLf(String^ str);

        // TCP server.
        void StartServer_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
        void StopServer_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
        void OnConnectionReceived(StreamSocketListener^ listener, StreamSocketListenerConnectionReceivedEventArgs^ args);
        void DoReceiveRequest(DataReader^ reader, DataWriter^ writer);
        void DoSendResponse(DataReader^ reader, DataWriter^ writer);

        StreamSocketListener^ listener;

        // TODO: If multiple request are received simultaneously, requests may get messed up.
        String^ requestReceived; 

        // TCP client.
        void ConnectClient_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
        void SendRequest_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
        void DisconnectClient_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
        void DoSendRequest(DataReader^ reader, DataWriter^ writer);
        void DoReceiveResponse(DataReader^ reader);

        StreamSocket^ socket;
        String^ responseReceived;

        // UDP receive.
        void ConnectUdpReceive_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
        void DisconnectUdpReceive_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);

        DatagramSocket^ receiveSocket;

        // UDP send.
        void UdpSend_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
        void OnMessageReceived(DatagramSocket^ sender, DatagramSocketMessageReceivedEventArgs^ args);
    };
}
