#pragma once

class WinSockInitialization
{
public:
    WinSockInitialization();
    ~WinSockInitialization();
    int GetError();

private:
    int error;
};

