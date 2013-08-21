#include "stdafx.h"

HRESULT DoIpv4ReverseDnsLookup(wchar_t* argument);
HRESULT DoIpv6ReverseDnsLookup(wchar_t* argument);
HRESULT DoDnsLookup(wchar_t* argument);
HRESULT PrintHostEnt(hostent* hostInfo);
HRESULT PrintSockAddr(SOCKADDR_INET* address);

HRESULT DoDns(wchar_t* argument)
{
    //// Convert wchar_t* to char* (or to multibyte).
    //char dest[MAX_PATH];
    //size_t bytesConverted;
    //errno_t error = wcstombs_s(&bytesConverted, dest, MAX_PATH, argument, wcslen(argument));
    //if (error)
    //{
    //    wprintf(L"wcstombs_s failed with %d.\n", error);
    //    return HRESULT_FROM_WIN32(error);
    //}

    HRESULT hr = DoIpv4ReverseDnsLookup(argument);
    if (hr != __HRESULT_FROM_WIN32(WSAEINVAL))
    {
        return hr;
    }

    hr = DoIpv6ReverseDnsLookup(argument);
    if (hr != __HRESULT_FROM_WIN32(WSAEINVAL))
    {
        return hr;
    }

    return DoDnsLookup(argument);
}

HRESULT DoIpv4ReverseDnsLookup(wchar_t* argument)
{
    IN_ADDR addr;
    int result = InetPton(AF_INET, argument, &addr);
    if (!result)
    {
        int error = WSAGetLastError();
        if (error != WSAEINVAL)
        {
            // WSAEINVAL is handled later.
            printf("InetPton failed with %ld.\n", error);
        }
        return HRESULT_FROM_WIN32(error);
    }

    hostent* hostInfo = gethostbyaddr((char*)&addr, sizeof(addr), AF_INET);

    return PrintHostEnt(hostInfo);
}

HRESULT DoIpv6ReverseDnsLookup(wchar_t* argument)
{
    IN6_ADDR addr6;
    int result = InetPton(AF_INET6, argument, &addr6);
    if (!result)
    {
        int error = WSAGetLastError();
        if (error != WSAEINVAL)
        {
            // WSAEINVAL is handled later.
            printf("InetPton failed with %ld.\n", error);
        }
        return HRESULT_FROM_WIN32(error);
    }

    hostent* hostInfo = gethostbyaddr((char*)&addr6, sizeof(addr6), AF_INET6);

    return PrintHostEnt(hostInfo);
}

HRESULT DoDnsLookup(wchar_t* argument)
{
    ADDRINFOEXW hints = {0};
    //hints.ai_family = AF_INET; // This indicate caller only handles IPv4, and not IPv6.
    PADDRINFOEXW addrInfoResult;

    int retval = GetAddrInfoEx(argument, nullptr, NS_DNS, nullptr, &hints, &addrInfoResult, nullptr, nullptr, nullptr, nullptr);
    if (retval != NO_ERROR)
    {
        WSACleanup();
        return HRESULT_FROM_WIN32(retval);
    }

    PADDRINFOEXW tempAddrInfoResult = addrInfoResult;
    //SOCKADDR_IN address = {0}; // SOCKADDR_IN is for IPv4, SOCKADDR_IN6 if for IPv6
    SOCKADDR_INET address = {AF_INET6};
    while (tempAddrInfoResult != nullptr)
    {
        assert(tempAddrInfoResult->ai_addrlen <= sizeof(address));

        //PSOCKADDR v4Address = reinterpret_cast<PSOCKADDR>(tempAddrInfoResult->ai_addr);
        //if (!INETADDR_ISLOOPBACK(v4Address))
        //{

        CopyMemory(&address, tempAddrInfoResult->ai_addr, tempAddrInfoResult->ai_addrlen);

        PrintSockAddr(&address);

        //}

        tempAddrInfoResult = tempAddrInfoResult->ai_next;
    }

    if (addrInfoResult != nullptr)
    {
        FreeAddrInfoEx(addrInfoResult);
    }

    return S_OK;
}

HRESULT PrintHostEnt(hostent* hostInfo)
{
    if (hostInfo == nullptr)
    {
        int error = WSAGetLastError();

        if (error == WSAHOST_NOT_FOUND)
        {
            printf("Host not found.\n");
        }
        else if (error == WSANO_DATA) {
            printf("No data record found.\n");
        } else {
            printf("Error: %ld\n", error);
        }

        return HRESULT_FROM_WIN32(error);
    }

    printf("Name: %s\n", hostInfo->h_name);
    printf("Alternate names: %s\n", hostInfo->h_aliases);

    return S_OK;
}

HRESULT PrintSockAddr(SOCKADDR_INET* address)
{
    assert(address->si_family == AF_INET || address->si_family == AF_INET6);

    if (address->si_family == AF_INET)
    {
        wprintf(L"Family AF_INET\n");
        wprintf(L"Port %hu\n", address->Ipv4.sin_port);
        wprintf(L"IP Address %d.%d.%d.%d\n",
            address->Ipv4.sin_addr.S_un.S_un_b.s_b1,
            address->Ipv4.sin_addr.S_un.S_un_b.s_b2,
            address->Ipv4.sin_addr.S_un.S_un_b.s_b3,
            address->Ipv4.sin_addr.S_un.S_un_b.s_b4);
    }
    else if (address->si_family == AF_INET6)
    {
        wprintf(L"Family AF_INET6\n");
        wprintf(L"Port %hu\n", address->Ipv6.sin6_port);
        wprintf(L"IP Address ");
        for (int i = 0; i < 16; i += 2)
        {
            if (i > 0)
            {
                wprintf(L":");
            }
            wprintf(L"%02x%02x",
                address->Ipv6.sin6_addr.u.Byte[i],
                address->Ipv6.sin6_addr.u.Byte[i + 1]);
        }
        wprintf(L"\n");
    }
    wprintf(L"\n");

    return S_OK;
}
