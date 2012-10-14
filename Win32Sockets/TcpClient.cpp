#include <cstdio>
#include <WinSock2.h>

// IPV6_V6ONLY
#include <WS2tcpip.h>

// SO_UPDATE_CONNECT_CONTEXT
#include <mswsock.h>

int DoTcpClient()
{
    SOCKET clientSocket;
    int ipv6only = 0;
    int result;
    BOOL success;

    wchar_t hostName[] = L"localhost";
    wchar_t serviceName[] = L"80";

    SOCKADDR_STORAGE localAddr = {0};
    SOCKADDR_STORAGE remoteAddr = {0};
    DWORD localAddrSize = sizeof(localAddr);
    DWORD remoteAddrSize = sizeof(remoteAddr);

    const wchar_t sendMessage[] = L"Are you ñoño? Can you tell me what time is it?\r\n";
    wchar_t recvMessage[1024];
    const unsigned int recvMessageLength = 1024;

    WSABUF wsaBuffer;
    char recvBuffer[16];
    const unsigned int recvBufferLength = 16;

    char buffer[1024];
    const unsigned int bufferLength = 1024;
    DWORD bytesSent;
    DWORD bytesRecv;
    DWORD totalBytesRecv;
    DWORD flags = 0; // Don't forget to set this, this is an input parameter.

    // Create socket.
    clientSocket = socket(AF_INET6, SOCK_STREAM, 0);
    if (clientSocket == INVALID_SOCKET)
    {
        wprintf(L"socket failed with error %d\n", WSAGetLastError());
        return 1;
    }

    // Enable IPv4 and IPv6.
    result = setsockopt(clientSocket, IPPROTO_IPV6, IPV6_V6ONLY, (char*)&ipv6only, sizeof(ipv6only));
    if (result == SOCKET_ERROR)
    {
        wprintf(L"setsockopt failed with error %d\n", WSAGetLastError());
        closesocket(clientSocket);
        return 1;
    }

    // Connect.
    success = WSAConnectByName(
        clientSocket,
        hostName,
        serviceName,
        &localAddrSize,
        (SOCKADDR*)&localAddr,
        &remoteAddrSize,
        (SOCKADDR*)&remoteAddr,
        NULL,
        NULL);
    if (!success)
    {
        wprintf(L"WSAConnectByName failed with error %d\n", WSAGetLastError());
        closesocket(clientSocket);
        return INVALID_SOCKET;
    }

    // TODO: what is this for?
    result = setsockopt(clientSocket, SOL_SOCKET, SO_UPDATE_CONNECT_CONTEXT, NULL, 0);
    if (result == SOCKET_ERROR)
    {
        wprintf(L"setsockopt failed with error %d\n", WSAGetLastError());
        closesocket(clientSocket);
        return INVALID_SOCKET;
    }

    // Convert message from unicode to UTF-8.
    int utf8BufferLength = WideCharToMultiByte(
        CP_UTF8,
        0, // flags
        sendMessage, // unicode string to convert
        -1, // size in chars of unicode string, or -1 if it is null-terminated
        buffer, // buffer for converted string
        bufferLength, // size of buffer
        NULL, // default char
        NULL); // used default char inidcator
    if (utf8BufferLength == 0)
    {
        wprintf(L"WideCharToMultiByte failed with error %d\n", WSAGetLastError());
        return 1;
    }

    // Do not send the null character.
    utf8BufferLength -= 1;

    // Send.
    // The difference between WSA functions and standard fnctions (e.g. WSASaned() and send() is that WSA functions
    // allow to use overlapped IO (non-blocking sockets) and also they allow sending/receiving mmultiple buffers
    // which can save you some copying memory work.
    wsaBuffer.buf = buffer;
    wsaBuffer.len = utf8BufferLength;
    result = WSASend(clientSocket, &wsaBuffer, 1, &bytesSent, 0, NULL, NULL);
    if (result == SOCKET_ERROR)
    {
        wprintf(L"WSASend failed with error %d\n", WSAGetLastError());
        closesocket(clientSocket);
        return 1;
    }

    // Receive.
    // The data received must finish with "\r\n".
    wsaBuffer.buf = recvBuffer;
    wsaBuffer.len = recvBufferLength;
    totalBytesRecv = 0;
    while (totalBytesRecv < 2 || (buffer[totalBytesRecv - 2] != '\r' || buffer[totalBytesRecv - 1] != '\n'))
    {
        result = WSARecv(clientSocket, &wsaBuffer, 1, &bytesRecv, &flags, NULL, NULL);
        if (result == SOCKET_ERROR)
        {
            // WSAECONNRESET (10054): Connection reset by peer.
            wprintf(L"WSARecv failed with error %d\n", WSAGetLastError());
            closesocket(clientSocket);
            return 1;
        }

        if (totalBytesRecv + bytesRecv >= bufferLength)
        {
            wprintf(L"There is not enough buffer.\n");
            closesocket(clientSocket);
            return 1;
        }
        CopyMemory(buffer + totalBytesRecv, recvBuffer, bytesRecv);
        totalBytesRecv += bytesRecv;
    }

    // Convert message from UTF-8 to unicode.
    // I think char is ISO-8859-1 and wchart_t is little-endian UTF-16.
    int charsWritten = MultiByteToWideChar(
        CP_UTF8,
        0, // flags
        buffer, // string to convert
        totalBytesRecv, // size in bytes, -1 if string is null-terminated
        recvMessage, // buffer for converted string
        recvMessageLength); // size of buffer
    if (charsWritten == 0)
    {
        wprintf(L"MultiByteToWideChar failed with error %d\n", WSAGetLastError());
        return 1;
    }

    // Message received is not null-temrinated, add null character.
    recvMessage[charsWritten] = '\0';

    // Print received message.
    wprintf(L"%s\n", recvMessage);

    // Close.
    closesocket(clientSocket);

    return 0;
}
