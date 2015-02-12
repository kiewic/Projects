// ThreadsExample.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

void DoCountdown(int id)
{
    for (int i = 10; i > 0; i--)
    {
        wprintf(L"%d from %d\r\n", i, id);
        Sleep(1000);
    }
}

DWORD WINAPI ThreadStartRoutine(_In_ PVOID param)
{
    int id = *reinterpret_cast<int*>(param);

    Sleep(500);
    DoCountdown(id);

    return ERROR_SUCCESS;
}

void DoThreadExample(int id)
{
    DWORD threadId;
    HANDLE thread1 = CreateThread(
        nullptr,
        0,
        ThreadStartRoutine,
        &id,
        0,
        &threadId);

    DoCountdown(id + 1);

    if (threadId)
    {
        DWORD index;
        CoWaitForMultipleObjects(CWMO_DISPATCH_CALLS | CWMO_DISPATCH_WINDOW_MESSAGES, INFINITE, 1, &thread1, &index);
        CloseHandle(thread1);
    }
}

int wmain(int argc, wchar_t* argv[])
{
    DoThreadExample(1);
    return 0;
}

