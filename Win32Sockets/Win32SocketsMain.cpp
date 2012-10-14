#include <cstdio>
#include <string.h>
#include <WinSock2.h>

// RtlIpv6StringToAddress
#include <mstcpip.h>

// sockaddr_in6
#include <ws2tcpip.h>

// Link with ws2_32.lib
#pragma comment(lib, "Ws2_32.lib")

void PrintHelp(wchar_t* fileName);
int InitializeWinsock();
void DoTcpServer();
void DoTcpClient();
int DoUdpReceive();
int DoUdpSend();

int wmain(int argc, wchar_t* argv[])
{
    int result = 0;

    if (InitializeWinsock() != 0)
    {
        return 1;
    }

    if (argc < 3)
    {
        PrintHelp(argv[0]);
    }
    else if (wcscmp(argv[1], L"tcp") == 0 && wcscmp(argv[2], L"server") == 0)
    {
        DoTcpServer();
    }
    else if (wcscmp(argv[1], L"tcp") == 0 && wcscmp(argv[2], L"client") == 0)
    {
        DoTcpClient();
    }
    else if (wcscmp(argv[1], L"udp") == 0 && wcscmp(argv[2], L"receive") == 0)
    {
        result = DoUdpReceive();
    }
    else if (wcscmp(argv[1], L"udp") == 0 && wcscmp(argv[2], L"send") == 0)
    {
        result = DoUdpSend();
    }
    else {
        PrintHelp(argv[0]);
    }

    // Clean.
    WSACleanup();

    return result;
}

void PrintHelp(wchar_t* fileName)
{
    wprintf(L"Usage: %s tcp {{ server | client }}\n", fileName);
    wprintf(L"Usage: %s udp {{ send | receive }}\n", fileName);
}

int InitializeWinsock()
{
    WSADATA wsaData;

    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0)
    {
        wprintf(L"WSAStartup failed with error %d\n", WSAGetLastError());
    }
    return result;
}

void DoTcpServer()
{


}

void DoTcpClient()
{
}

int DoUdpReceive()
{
    SOCKET recvSocket;
    struct sockaddr_in6 recvAddr;
    struct sockaddr_in6 senderAddr;
    int senderAddrSize = sizeof(senderAddr);

    //char* sourceIp = "0.0.0.0";
    char* sourcePort = "2704";

    WSABUF wsaBuf;
    wchar_t message[1024];
    const int messageLength = 1024;
    char recvBuffer[1024];
    const int recvBufferLength = 1024;
    DWORD bytesRecv = 0;
    DWORD flags = 0; // Don't forget to set this, it is an input parameter.

    int result;

    // Set source address.
    recvAddr.sin6_family = AF_INET6;
    recvAddr.sin6_addr = in6addr_any; // TODO: find a replacement for htonl(INADDR_ANY);
    recvAddr.sin6_flowinfo = NULL;
    recvAddr.sin6_scope_struct.Value = 0;
    recvAddr.sin6_port = htons((u_short)atoi(sourcePort));
    if (recvAddr.sin6_port == 0)
    {
        wprintf(L"Source port must be a legal UDP port.\n", WSAGetLastError());
        return 1;
    }

    // Create socket.
    recvSocket = WSASocket(AF_INET6, SOCK_DGRAM, IPPROTO_UDP, NULL, 0, 0);
    if (recvSocket == INVALID_SOCKET)
    {
        wprintf(L"WSASocket failed with error %d\n", WSAGetLastError());
        return 1;
    }

    // Bind socket.
    result = bind(recvSocket, (SOCKADDR*)&recvAddr, sizeof(recvAddr));
    if (result != 0)
    {
        wprintf(L"bind failed with error %d\n", WSAGetLastError());
        closesocket(recvSocket);
        return 1;
    }

    // TODO: receive multiple messages.
    // Receive message.
    wsaBuf.len = recvBufferLength;
    wsaBuf.buf = recvBuffer;
    result = WSARecvFrom(
        recvSocket, // socket descriptor
        &wsaBuf, // array of WSABUF structures
        1, // number of WSABUF structures in array
        &bytesRecv, // bytes received by this call
        &flags,
        (SOCKADDR*)&senderAddr, // source address
        &senderAddrSize, // size in bytes of source address
        NULL, // WSAOVERLAPPED structure
        NULL); // completion function for overlapped sockets
    if (result == SOCKET_ERROR)
    {
        wprintf(L"WSARecvFrom failed with error %d\n", WSAGetLastError());
        closesocket(recvSocket);
        return 1;
    }

    // Convert message from UTF-8 to unicode.
    // I think char is ISO-8859-1 and wchart_t is little-endian UTF-16.
    int charsWritten = MultiByteToWideChar(
        CP_UTF8,
        0, // flags
        recvBuffer, // string to convert
        bytesRecv, // size in bytes, -1 if string is null-terminated
        message, // buffer for converted string
        messageLength); // size of buffer
    if (charsWritten == 0)
    {
        wprintf(L"MultiByteToWideChar failed with error %d\n", WSAGetLastError());
        return 1;
    }
    message[charsWritten] = '\0';

    // TODO: write where the message comes from.
    wprintf(L"Message received: %s\n", message);

    // Close.
    closesocket(recvSocket);

    return 0;
}

int DoUdpSend()
{
    SOCKET sendSocket;
    struct sockaddr_in6 sendAddr;
    int sendAddrSize = sizeof(sendAddr);

    wchar_t *targetIp = L"::1";
    //char* targetIp = "127.0.0.1";
    char* targetPort = "2704";

    WSABUF wsaBuffer;
    const wchar_t message[] = L"¡Hello, I am the new guy in the network!";
    char sendBuffer[1024];
    const int sendBufferLength = 1024;
    DWORD bytesSent = 0;
    DWORD flags = 0;

    int result;

    // Set target address.
    result = WSAStringToAddress(targetIp, AF_INET6, NULL, (LPSOCKADDR)&sendAddr, &sendAddrSize);
    if (result == SOCKET_ERROR)
    {
        wprintf(L"WSAStringToAddress failed with error %d\n", WSAGetLastError());
        return 1;
    }
    sendAddr.sin6_port = htons((u_short)atoi(targetPort));
    if (sendAddr.sin6_port == 0)
    {
        wprintf(L"Target port must be a legal UDP port.\n", WSAGetLastError());
        return 1;
    }

    // Create socket.
    sendSocket = WSASocket(AF_INET6, SOCK_DGRAM, IPPROTO_UDP, NULL, 0, 0);
    if (sendSocket == INVALID_SOCKET)
    {
        wprintf(L"WSASocket failed with error %d\n", WSAGetLastError());
        return 1;
    }

    // Convert message from unicode to UTF-8.
    // I think char is ISO-8859-1 and wchart_t is little-endian UTF-16.
    int utf8BufferLength = WideCharToMultiByte(
        CP_UTF8,
        0, // flags
        message, // unicode string to convert
        -1, // size in chars of unicode string, or -1 if it is null-terminated
        sendBuffer, // buffer for converted string
        sendBufferLength, // size of buffer
        NULL, // default char
        NULL); // used default char inidcator
    if (utf8BufferLength == 0)
    {
        wprintf(L"WideCharToMultiByte failed with error %d\n", WSAGetLastError());
        return 1;
    }

    // WSABUF structure containing a pointer to the buffer and the length of the buffer.
    wsaBuffer.buf = sendBuffer;
    wsaBuffer.len = utf8BufferLength;

    // Send message.
    result = WSASendTo(
        sendSocket, // socket descriptor
        &wsaBuffer, // array of WSABUF structures
        1, // number the WSABUF structures
        &bytesSent, // bytes sent by this call
        flags,
        (SOCKADDR*)&sendAddr, // target address
        sendAddrSize, // size in bytes of target address
        NULL, // WSAOVERLAPPED structure
        NULL); // completion function for overlapped sockets
    if (result == SOCKET_ERROR)
    {
        wprintf(L"WSASendTo failed with error %d\n", WSAGetLastError());
        closesocket(sendSocket);
        return 1;
    }

    wprintf(L"Message sent: %s\n", message);

    // Close and clean.
    closesocket(sendSocket);

    return 0;
}
