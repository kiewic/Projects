// DebugMe.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

int fibo(int n)
{
    if (n == 0)
    {
        return 0;
    }

    if (n == 1)
    {
        return 1;
    }

    return fibo(n - 1) + fibo(n - 2);
}

int wmain(int argc, wchar_t* argv[])
{
    int n = 4;
    //scanf("%d", &n);
    printf("fibonacci(%d) = %d\r\n", n, fibo(n));
    return 0;
}

