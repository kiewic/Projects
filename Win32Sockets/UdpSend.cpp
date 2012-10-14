#include <cstdio>
#include <WinSock2.h>

// sockaddr_in6
#include <ws2tcpip.h>

int DoUdpSend()
{
    SOCKET sendSocket;
    struct sockaddr_in6 sendAddr;
    int sendAddrSize = sizeof(sendAddr);

    wchar_t *targetIp = L"::1";
    //char* targetIp = "127.0.0.1";
    char* targetPort = "2704";

    const wchar_t sendMessage[] = L"¡Hello, I am the new guy in the network!";

    WSABUF wsaBuffer;
    char sendBuffer[1024];
    const unsigned int sendBufferLength = 1024;
    DWORD bytesSent;

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
        sendMessage, // unicode string to convert
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

    // Do not send the null character.
    utf8BufferLength -= 1;

    // Send message.
    // WSABUF structure contains a pointer to the buffer and the length of the buffer.
    wsaBuffer.buf = sendBuffer;
    wsaBuffer.len = utf8BufferLength;
    result = WSASendTo(
        sendSocket, // socket descriptor
        &wsaBuffer, // array of WSABUF structures
        1, // number the WSABUF structures
        &bytesSent, // bytes sent by this call
        0, // flags
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

    wprintf(L"Message sent: %s\n", sendMessage);

    // Close.
    closesocket(sendSocket);

    return 0;
}
