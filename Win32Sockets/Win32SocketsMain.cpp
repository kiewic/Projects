#include <cstdio>
#include <string.h>
#include <WinSock2.h>

// Link with ws2_32.lib
#pragma comment(lib, "Ws2_32.lib")

void PrintHelp(wchar_t* fileName);
int InitializeWinsock();
int DoTcpServer();
int DoTcpClient();
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
        result = DoTcpServer();
    }
    else if (wcscmp(argv[1], L"tcp") == 0 && wcscmp(argv[2], L"client") == 0)
    {
        result = DoTcpClient();
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
