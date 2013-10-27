#include <Windows.h>
#include <WinInet.h>
#include <stdio.h>

#pragma comment(lib, "Wininet.lib")

#define BUFFER_SIZE 0x10000

HRESULT createConnection(HINTERNET sessionHandle, wchar_t* hostname, HINTERNET* connectionHandle)
{
    *connectionHandle = InternetConnect(
        sessionHandle,
        hostname,
        INTERNET_DEFAULT_HTTP_PORT,
        nullptr, // username
        nullptr, // password
        INTERNET_SERVICE_HTTP,
        0,
        NULL);
    if (!(*connectionHandle))
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    return S_OK;
}

HRESULT createSession(HINTERNET* sessionHandle)
{
    *sessionHandle = InternetOpen(
        L"", // user agent
        INTERNET_OPEN_TYPE_PRECONFIG,
        nullptr,
        nullptr,
        0);
    if (!(*sessionHandle))
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    return S_OK;
}

HRESULT makeRequest(HINTERNET connectionHandle, wchar_t* rawUrl, bool useCache)
{
    BOOL success;

    // allocate memory
    char* buffer = new char[BUFFER_SIZE];

    HINTERNET requestHandle = HttpOpenRequest(
        connectionHandle,
        L"GET",
        rawUrl, //L"/Entry.xml?private",
        L"HTTP/1.1",
        nullptr, // referer
        nullptr, // accept types
        0, // INTERNET_FLAG_KEEP_CONNECTION, // INTERNET_FLAG_RELOAD to skip cache
        NULL);
    if (!requestHandle)
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    // Timeout.
    UINT32 timeout = 5000;
    if (!InternetSetOption(requestHandle, INTERNET_OPTION_RECEIVE_TIMEOUT, &timeout, sizeof(UINT32)))
    {
        int lastError = GetLastError();
        return HRESULT_FROM_WIN32(lastError);
    }

    wchar_t* additionalHeaders = L"";
    if (useCache)
    {
        additionalHeaders = L"Cache-Control: max-age=3600";
    }

    success = HttpSendRequest(
        requestHandle,
        additionalHeaders, // additional headers
        -1, // headers length
        nullptr, // optional data
        0); // optional data length
    if (!success)
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    // Get response headers.

    LPVOID lpOutBuffer = NULL;
    DWORD dwSize = 0;
    // This will fail, but we will get the size of the headers.
    success = HttpQueryInfo(requestHandle, HTTP_QUERY_RAW_HEADERS_CRLF, lpOutBuffer, &dwSize, NULL);
    if (!success)
    {
        if (GetLastError() == ERROR_INSUFFICIENT_BUFFER)
        { 
            lpOutBuffer = new char[dwSize];
            success = HttpQueryInfo(requestHandle, HTTP_QUERY_RAW_HEADERS_CRLF, lpOutBuffer, &dwSize, NULL);
        }

        if (!success)
        {
            return HRESULT_FROM_WIN32(GetLastError());
        }
    }

    wprintf(L"%s\n", lpOutBuffer);
    delete[] lpOutBuffer;

    // Get response content.

    DWORD bytesReceived;
    DWORD totalBytesReceived = 0;

    do 
    {
        success = InternetReadFile(
            requestHandle,
            buffer + totalBytesReceived,
            BUFFER_SIZE,
            &bytesReceived);

        totalBytesReceived += bytesReceived;
    }
    while (success && bytesReceived > 0);

    if (!success)
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    wprintf(L"conectionHandle %#08x and requestHandle %#08d\n", connectionHandle, requestHandle);
    wprintf(L"totalBytesReceived %d\n", connectionHandle, requestHandle);

    InternetCloseHandle(requestHandle);

    return S_OK;
}

HRESULT doMain()
{
    HRESULT hr;

    HINTERNET sessionHandle;
    hr = createSession(&sessionHandle);
    if (FAILED(hr))
    {
        return hr;
    }

    HINTERNET connectionHandle;
    hr = createConnection(sessionHandle, L"kiewic.com", &connectionHandle);
    if (FAILED(hr))
    {
        return hr;
    }

    hr = makeRequest(connectionHandle, L"/?timeout", true);
    if (FAILED(hr))
    {
        return hr;
    }

    InternetCloseHandle(connectionHandle);
    InternetCloseHandle(sessionHandle);

    return S_OK;
}

int main() {
    HRESULT hr = doMain();
    //HRESULT hr = doSessionFirtOneAndThenTheSecondOne();

    if (FAILED(hr)) {
        wprintf(L"Process failed with %#08x\n", hr);
    }

    wprintf(L"Done.\n");

    return 0;
}
