#include "pch.h"
#include "AutoSOCKET.h"

AutoSOCKET::AutoSOCKET() : socket(INVALID_SOCKET)
{
}

AutoSOCKET::~AutoSOCKET()
{
    if (this->socket != INVALID_SOCKET)
    {
        closesocket(this->socket);
    }
}

SOCKET AutoSOCKET::Get()
{
    return this->socket;
}

SOCKET* AutoSOCKET::GetAddress()
{
    return &this->socket;
}

void AutoSOCKET::Set(_In_ SOCKET socket)
{
    this->socket = socket;
}
