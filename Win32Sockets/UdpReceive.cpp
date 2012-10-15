#include <cstdio>
#include <WinSock2.h>

// sockaddr_in6
#include <ws2tcpip.h>

int DoUdpReceive()
{
    SOCKET recvSocket;
    struct sockaddr_in6 recvAddr;
    struct sockaddr_in6 senderAddr;
    int senderAddrSize = sizeof(senderAddr);

    //char* sourceIp = "0.0.0.0";
    char* sourcePort = "2704";

    wchar_t recvMessage[1024];
    const unsigned int recvMessageLength = 1024;

    WSABUF wsaBuffer;
    char recvBuffer[1024];
    const unsigned int recvBufferLength = 1024;
    DWORD bytesRecv;
    DWORD flags = 0; // Don't forget to set this, this is an input parameter.

    int result;

    // Set source address.
    recvAddr.sin6_family = AF_INET6;
    recvAddr.sin6_addr = in6addr_any; // TODO: Find a replacement for htonl(INADDR_ANY);
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
    if (result == SOCKET_ERROR)
    {
        wprintf(L"bind failed with error %d\n", WSAGetLastError());
        closesocket(recvSocket);
        return 1;
    }

    // TODO: Receive multiple messages.

    // Receive message.
    wsaBuffer.len = recvBufferLength;
    wsaBuffer.buf = recvBuffer;
    result = WSARecvFrom(
        recvSocket, // socket descriptor
        &wsaBuffer, // array of WSABUF structures
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
        recvMessage, // buffer for converted string
        recvMessageLength); // size of buffer
    if (charsWritten == 0)
    {
        wprintf(L"MultiByteToWideChar failed with error %d\n", WSAGetLastError());
        closesocket(recvSocket);
        return 1;
    }

    // Message received is not null-temrinated, add null character.
    recvMessage[charsWritten] = '\0';

    // TODO: Write where the message comes from.
    wprintf(L"Message received: %s\n", recvMessage);

    // Close.
    closesocket(recvSocket);

    return 0;
}
