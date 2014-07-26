// MutexExample.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <synchapi.h>

void doCountdown()
{
    for (int i = 10; i > 0; i--)
    {
        wprintf(L"%d\r\n", i);
        Sleep(1000);
    }
    wprintf(L"0\r\n");
}

int main(int argc, _TCHAR* argv[])
{
    WCHAR* cacheMutexName = L"HelloWorldMutex";
    HANDLE handle = ::CreateMutex(nullptr, FALSE, cacheMutexName);

    DWORD result = WaitForSingleObject(handle, INFINITE);
    switch (result)
    {
    case WAIT_OBJECT_0:
        printf("The thread got ownership of the mutex.\r\n");
        break;
    case WAIT_ABANDONED:
        printf("The thread got ownership of an abandoned mutex.\r\n");
        break;
    }

    doCountdown();

    ReleaseMutex(handle);
    printf("Mutex released.\r\n");

    return 0;
}

// How to debug on WinDbg?
//
//   0:000> dv
//   argc = 0n1
//   argv = 0x010f6f28
//   handle = 0x00000038
//   result = 0xcccccccc
//   cacheMutexName = 0x003f5858 "HelloWorldMutex
//
//   0:000> !handle 0x00000038 f
//   Handle 38
//   Type Mutant
//   Attributes 0
//   GrantedAccess 0x1f0001:
//   Delete, ReadControl, WriteDac, WriteOwner, Synch
//   QueryState
//   HandleCount 3
//   PointerCount 98306
//   Name \Sessions\1\BaseNamedObjects\HelloWorldMutex
//   Object Specific Information
//   Mutex is Owned
//   Mutant Owner 11ac.1628
