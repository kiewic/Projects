#pragma once

class AutoSOCKET
{
public:
    AutoSOCKET();
    ~AutoSOCKET();

    SOCKET Get();
    SOCKET* GetAddress();
    void Set(_In_ SOCKET socket);

private:
    SOCKET socket;
};
