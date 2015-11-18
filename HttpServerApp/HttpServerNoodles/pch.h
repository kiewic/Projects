//
// pch.h
// Header for standard system include files.
//

#pragma once

#include <collection.h>
#include <ppltasks.h>


#include <WinSock2.h>

// sockaddr_in6
// IPV6_V6ONLY
#include <ws2tcpip.h>

// WinSock
#pragma comment(lib, "Ws2_32.lib")

// MSWSock.h
#include <MSWSock.h>

// IMFAsyncCallback
#include <Mfobjects.h>

// MFCreateAsyncResult
#include <Mfapi.h>

// Media Foundation
#pragma comment(lib, "Mfuuid.lib")
#pragma comment(lib, "Mfplat.lib")
