// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include <assert.h>
#include <cstdio>
#include <string.h>
#include <WinSock2.h>

// sockaddr_in6
// IPV6_V6ONLY
#include <ws2tcpip.h>

// SO_UPDATE_CONNECT_CONTEXT
#include <mswsock.h>

// Link with ws2_32.lib
#pragma comment(lib, "Ws2_32.lib")

#define ReturnIfFailed(EXPR) hr = (EXPR); if (FAILED(hr)) { return hr; }
