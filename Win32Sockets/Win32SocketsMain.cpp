#include "stdafx.h"

void PrintHelp(wchar_t* fileName);
HRESULT InitializeWinsock();
int DoTcpServer();
int DoTcpClient();
int DoUdpReceive();
int DoUdpSend();
HRESULT DoDns(wchar_t* argument);

int wmain(int argc, wchar_t* argv[])
{
    int result = 0;
    HRESULT hr = S_OK;

    if (FAILED(InitializeWinsock()))
    {
        return 1;
    }

    if (argc < 3)
    {
        PrintHelp(argv[0]);
    }
    else if (argc >= 2 && _wcsicmp(argv[1], L"tcp") == 0 && _wcsicmp(argv[2], L"server") == 0)
    {
        result = DoTcpServer();
    }
    else if (_wcsicmp(argv[1], L"tcp") == 0 && _wcsicmp(argv[2], L"client") == 0)
    {
        result = DoTcpClient();
    }
    else if (_wcsicmp(argv[1], L"udp") == 0 && _wcsicmp(argv[2], L"receive") == 0)
    {
        result = DoUdpReceive();
    }
    else if (_wcsicmp(argv[1], L"udp") == 0 && _wcsicmp(argv[2], L"send") == 0)
    {
        result = DoUdpSend();
    }
    else if (_wcsicmp(argv[1], L"dns") == 0)
    {
        hr = DoDns(argv[2]);
    }
    else {
        PrintHelp(argv[0]);
    }

    // Clean.
    WSACleanup();

    if (FAILED(hr))
    {
        return hr;
    }

    return result;
}

void PrintHelp(wchar_t* fileName)
{
    wprintf(L"Usage: %s tcp {{ server | client }}\n", fileName);
    wprintf(L"Usage: %s udp {{ send | receive }}\n", fileName);
    wprintf(L"Usage: %s dns {{ <ipv4-address> | <ipv6-address> | <hostname> }}\n", fileName);
}

HRESULT InitializeWinsock()
{
    WSADATA wsaData;

    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0)
    {
        int error = WSAGetLastError();
        wprintf(L"WSAStartup failed with error %d.\n", error);
        return HRESULT_FROM_WIN32(error);
    }

    return S_OK;
}
