// Class1.cpp
#include "pch.h"
#include "HttpServer.h"
#include "HttpServerNoodlesUtils.h"

using namespace HttpServerNoodles;
using namespace Platform;

HttpServer::HttpServer()
{
}

HttpServer::~HttpServer()
{
    Stop();
}

int HttpServer::StartTcpListener()
{
    int ipv6only = 0;
    int result;

    //wchar_t hostName[] = L"::";
    wchar_t serviceName [] = L"80";

    struct sockaddr_in6 localAddr = { 0 };
    struct sockaddr_in6 remoteAddr = { 0 };
    int remoteAddrSize = sizeof(remoteAddr);

    const wchar_t sendMessage [] = L"Yes, I am ñoño. I do not know what time is it.\r\n";
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
    SOCKET listenerSocket = socket(AF_INET6, SOCK_STREAM, 0);
    if (listenerSocket == INVALID_SOCKET)
    {
        return GetWSAErrorInfo(L"socket()");
    }

    // Use AutoSOCKET to never forget to clean up the socket.
    this->autoListenerSocket.Set(listenerSocket);

    // Enable IPv4 and IPv6.
    result = setsockopt(
        listenerSocket,
        IPPROTO_IPV6,
        IPV6_V6ONLY,
        (char*)&ipv6only,
        sizeof(ipv6only));
    if (result == SOCKET_ERROR)
    {
        return GetWSAErrorInfo(L"setsockopt()");
    }

    // Set local address.
    localAddr.sin6_family = AF_INET6;
    localAddr.sin6_addr = in6addr_any;
    localAddr.sin6_flowinfo = NULL;
    localAddr.sin6_scope_struct.Value = 0;
    localAddr.sin6_port = htons(static_cast<u_short>(_wtoi(serviceName)));
    if (localAddr.sin6_port == 0)
    {
        return GetWSAErrorInfo(L"htons(_wtoi())");
    }

    // Bind socket.
    result = bind(listenerSocket, reinterpret_cast<SOCKADDR*>(&localAddr), sizeof(localAddr));
    if (result == SOCKET_ERROR)
    {
        // If localAddr is not properly initialized:
        // "WSAEAFNOSUPPORT (10047): Address family not supported by protocol family." error.
        // If port is already in use:
        // "WSAEADDRINUSE (10048): Address already in use." error. 
        return GetWSAErrorInfo(L"bind()");
    }

    // Put socket to listen.
    result = listen(listenerSocket, SOMAXCONN);
    if (result == SOCKET_ERROR)
    {
        return GetWSAErrorInfo(L"listen()");
    }

    //  AcceptEx function must be obtained at run time by making a call to the WSAIoctl.
    GUID guidAcceptEx = WSAID_ACCEPTEX;
    LPFN_ACCEPTEX acceptEx = NULL;
    DWORD bytes;
    result = WSAIoctl(
        listenerSocket,
        SIO_GET_EXTENSION_FUNCTION_POINTER,
        &guidAcceptEx, // AcceptEx extension function identifier.
        sizeof(guidAcceptEx),
        &acceptEx,
        sizeof(acceptEx),
        &bytes,
        NULL,
        NULL);
    if (result == SOCKET_ERROR)
    {
        return GetWSAErrorInfo(L"WSAIoctl()");
    }

    // Create an accepting socket
    SOCKET acceptSocket = INVALID_SOCKET;
    //acceptSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    acceptSocket = socket(AF_INET6, SOCK_STREAM, 0);
    this->autoAcceptSocket.Set(acceptSocket);
    if (acceptSocket == INVALID_SOCKET)
    {
        return GetWSAErrorInfo(L"socket()");
    }

    // Empty our overlapped structure and accept connections.
    memset(&this->overlapped, 0, sizeof(this->overlapped));

    this->overlapped.hEvent = CreateEventEx(nullptr, nullptr, CREATE_EVENT_MANUAL_RESET, 0);
    if (!this->overlapped.hEvent)
    {
        return GetErrorInfo(L"CreateEventEx");
    }

    BOOL returnValue = acceptEx(
        listenerSocket,
        acceptSocket,
        this->outputBuffer,
        0,
        sizeof(sockaddr_in) + 16,
        sizeof(sockaddr_in) + 16,
        &bytes,
        &this->overlapped);
    if (returnValue == FALSE && WSAGetLastError() != ERROR_IO_PENDING)
    {
        // TODO: closesocket(acceptSocket);
        int error = GetWSAErrorInfo(L"AcceptEx()");

        // Catch WSA_IO_PENDING.
        if (error != WSA_IO_PENDING)
        {
            return error;
        }
    }

    result = WSAWaitForMultipleEvents(1, &this->overlapped.hEvent, FALSE, WSA_INFINITE, FALSE);
    switch (result)
    {
    case WSA_WAIT_IO_COMPLETION:
        OutputDebugString(L"WSA_WAIT_IO_COMPLETION\n");
        break;
    case WSA_WAIT_TIMEOUT:
        OutputDebugString(L"WSA_WAIT_IO_COMPLETION\n");
        break;
    case WSA_WAIT_FAILED:
        OutputDebugString(L"WSA_WAIT_FAILED\n");
        return GetWSAErrorInfo(L"WSAWaitForMultipleEvents");
        break;
    case WSA_WAIT_EVENT_0:
    default:
        OutputDebugString(L"WSA_WAIT_EVENT_0\n");
        break;
    }

    return NOERROR;
}

/*
HRESULT Foo()
{
    //// Accept a client.
    //// TODO: Add a condition callback function and play with the received information.
    //acceptSocket = WSAAccept(
    //    listenerSocket,
    //    reinterpret_cast<SOCKADDR*>(&remoteAddr),
    //    &remoteAddrSize,
    //    Foo, // a callback function that will accept or reject the connection
    //    NULL); // data passed to the callback function

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
    while (totalBytesRecv < 2 ||
        buffer[totalBytesRecv - 2] != '\r' ||
        buffer[totalBytesRecv - 1] != '\n')
    {
        result = WSARecv(acceptSocket, &wsaBuffer, 1, &bytesRecv, &flags, NULL, NULL);
        if (result == SOCKET_ERROR)
        {
            // WSAECONNRESET (10054): Connection reset by peer.
            wprintf(L"WSARecv failed with error %d\n", WSAGetLastError());
            closesocket(acceptSocket);
            return 1;
        }

        if (totalBytesRecv + bytesRecv >= bufferLength)
        {
            wprintf(L"There is not enough buffer.\n");
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
        closesocket(acceptSocket);
        return 1;
    }

    // Close.
    closesocket(acceptSocket);

    return 0;
}
*/

void HttpServer::Start()
{
    //DWORD threadId;
    //HANDLE thread1 = CreateThread(
    //    nullptr, // attributes
    //    0, // stack size
    //    ThreadStartRoutine, // start address
    //    nullptr, // parameter
    //    0, // creatin flag
    //    &threadId);

    // TODO: convert error to HRESULT or exception.
    int error = StartTcpListener();
}

void HttpServer::Stop()
{
}
