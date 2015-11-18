#include "pch.h"
#include "HttpServerNoodlesUtils.h"

int GetErrorInfo(_In_z_ wchar_t *name, _In_ int error)
{
    const DWORD size = 100 + 1;
    WCHAR buffer[size];
    int nbytes = FormatMessage(
        FORMAT_MESSAGE_FROM_SYSTEM, // formatting options flag
        NULL, // not a source DLL or format string
        error, // returned by GetLastError()
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        buffer,
        size,
        nullptr);

    wchar_t errorInfo[size * 2];
    swprintf(errorInfo, ARRAYSIZE(errorInfo), L"%s failed with error %d: %s", name, WSAGetLastError(), buffer);
    OutputDebugStringW(errorInfo);

    return error;
}

int GetErrorInfo(_In_z_ wchar_t *name)
{
    return GetErrorInfo(name, GetLastError());
}

int GetWSAErrorInfo(_In_z_ wchar_t *name)
{
    return GetErrorInfo(name, WSAGetLastError());
}
