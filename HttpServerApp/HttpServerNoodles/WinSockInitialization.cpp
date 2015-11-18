#include "pch.h"
#include "HttpServerNoodlesUtils.h"
#include "WinSockInitialization.h"

WinSockInitialization::WinSockInitialization()
{
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
    {
        this->error = GetWSAErrorInfo(L"WSAStartup()");
    }
}

WinSockInitialization::~WinSockInitialization()
{
    if (WSACleanup() != 0)
    {
        this->error = GetWSAErrorInfo(L"WSACleanup()");
    }
}

int WinSockInitialization::GetError()
{
    return this->error;
}
