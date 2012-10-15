#include <cstdio>
#include <WinSock2.h>

// IPV6_V6ONLY
#include <WS2tcpip.h>

int DoTcpServer()
{
    SOCKET listenSocket;
    SOCKET acceptSocket;
    int ipv6only = 0;
    int result;
    BOOL success;

    //wchar_t hostName[] = L"::";
    wchar_t serviceName[] = L"80";

    struct sockaddr_in6 localAddr = {0};
    struct sockaddr_in6 remoteAddr = {0};
    DWORD localAddrSize = sizeof(localAddr);
    int remoteAddrSize = sizeof(remoteAddr);

    const wchar_t sendMessage[] = L"Yes, I am ñoño. I do not know what time is it.\r\n";
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
    listenSocket = socket(AF_INET6, SOCK_STREAM, 0);
    if (listenSocket == INVALID_SOCKET)
    {
        wprintf(L"socket failed with error %d\n", WSAGetLastError());
        return 1;
    }

    // Enable IPv4 and IPv6.
    result = setsockopt(
        listenSocket,
        IPPROTO_IPV6,
        IPV6_V6ONLY,
        (char*)&ipv6only,
        sizeof(ipv6only));
    if (result == SOCKET_ERROR)
    {
        wprintf(L"setsockopt failed with error %d\n", WSAGetLastError());
        closesocket(listenSocket);
        return 1;
    }

    // Set local address.
    localAddr.sin6_family = AF_INET6;
    localAddr.sin6_addr = in6addr_any;
    localAddr.sin6_flowinfo = NULL;
    localAddr.sin6_scope_struct.Value = 0;
    localAddr.sin6_port = htons((u_short)_wtoi(serviceName));
    if (localAddr.sin6_port == 0)
    {
        wprintf(L"Source port must be a legal UDP port.\n", WSAGetLastError());
        return 1;
    }


    // Bind socket.
    result = bind(listenSocket, (SOCKADDR*)&localAddr, sizeof(localAddr));
    if (result == SOCKET_ERROR)
    {
        // If localAddr is not properly initialized:
        // "WSAEAFNOSUPPORT (10047): Address family not supported by protocol family." error.
        // If port is already in use:
        // "WSAEADDRINUSE (10048): Address already in use." error. 
        wprintf(L"bind failed with error %d\n", WSAGetLastError());
        closesocket(listenSocket);
        return 1;
    }

    // Put socket to listen.
    result = listen(listenSocket, SOMAXCONN);
    if (result == SOCKET_ERROR)
    {
        wprintf(L"listen failed with error %d\n", WSAGetLastError());
        closesocket(listenSocket);
        return 1;
    }

    // TODO: Accept multiple connections.

    // Accept a client.
    // TODO: Add a condition callback function and play with the received information.
    acceptSocket = WSAAccept(
        listenSocket,
        (SOCKADDR*)&remoteAddr,
        &remoteAddrSize,
        NULL, // a callback function that will accept or reject the connection
        NULL); // data passed to the callback function

    // TODO: Print ip address and port for the remote socket.

    //// TODO: What is this for?
    //result = setsockopt(acceptSocket, SOL_SOCKET, SO_UPDATE_CONNECT_CONTEXT, NULL, 0);
    //if (result == SOCKET_ERROR)
    //{
    //    wprintf(L"setsockopt failed with error %d\n", WSAGetLastError());
    //    closesocket(listenSocket);
    //    closesocket(acceptSocket);
    //    return INVALID_SOCKET;
    //}

    // Receive request.
    // The data received must finish with "\r\n".
    wsaBuffer.buf = recvBuffer;
    wsaBuffer.len = recvBufferLength;
    totalBytesRecv = 0;
    while (totalBytesRecv < 2 || (buffer[totalBytesRecv - 2] != '\r' || buffer[totalBytesRecv - 1] != '\n'))
    {
        result = WSARecv(acceptSocket, &wsaBuffer, 1, &bytesRecv, &flags, NULL, NULL);
        if (result == SOCKET_ERROR)
        {
            // WSAECONNRESET (10054): Connection reset by peer.
            wprintf(L"WSARecv failed with error %d\n", WSAGetLastError());
            closesocket(listenSocket);
            closesocket(acceptSocket);
            return 1;
        }

        if (totalBytesRecv + bytesRecv >= bufferLength)
        {
            wprintf(L"There is not enough buffer.\n");
            closesocket(listenSocket);
            closesocket(acceptSocket);
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
        closesocket(listenSocket);
        closesocket(acceptSocket);
        return 1;
    }

    // Message received is not null-temrinated, add null character.
    recvMessage[charsWritten] = '\0';

    // Print received message.
    wprintf(L"%s\n", recvMessage);

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
        closesocket(listenSocket);
        closesocket(acceptSocket);
        return 1;
    }

    // Do not send the null character.
    utf8BufferLength -= 1;

    // Send response.
    wsaBuffer.buf = buffer;
    wsaBuffer.len = utf8BufferLength;
    result = WSASend(acceptSocket, &wsaBuffer, 1, &bytesSent, 0, NULL, NULL);
    if (result == SOCKET_ERROR)
    {
        wprintf(L"WSASend failed with error %d\n", WSAGetLastError());
        closesocket(listenSocket);
        closesocket(acceptSocket);
        return 1;
    }

    // Close.
    closesocket(listenSocket);
    closesocket(acceptSocket);

    return 0;
}
