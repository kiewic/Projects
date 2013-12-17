#include <Windows.h>
#include <WinInet.h>
#include <cstdio>

#pragma comment(lib, "Wininet.lib")

#define BUFFER_SIZE 100

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
    UINT32 timeout = 60000;
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

    LPVOID headersBuffer = NULL; // TODO: Use a smart-pointer.
    DWORD bufferSize = 0;

    // This will fail, but we will get the size of the headers.
    success = HttpQueryInfo(requestHandle, HTTP_QUERY_RAW_HEADERS_CRLF, headersBuffer, &bufferSize, NULL);
    if (!success)
    {
        if (GetLastError() == ERROR_INSUFFICIENT_BUFFER)
        {
            headersBuffer = new char[bufferSize];
            success = HttpQueryInfo(requestHandle, HTTP_QUERY_RAW_HEADERS_CRLF, headersBuffer, &bufferSize, NULL);
        }

        if (!success)
        {
            return HRESULT_FROM_WIN32(GetLastError());
        }
    }

    wprintf(L"%s\n", headersBuffer);
    delete[] headersBuffer;

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

        wprintf(L"bytes received so far: %d\n", totalBytesReceived);
    } while (success && bytesReceived > 0);

    if (!success)
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    wprintf(L"conectionHandle %#08x and requestHandle %#08d\n", connectionHandle, requestHandle);
    wprintf(L"total bytes received: %d\n", totalBytesReceived);

    InternetCloseHandle(requestHandle);

    return S_OK;
}

HRESULT mainCore()
{
    HRESULT hr;

    HINTERNET sessionHandle;
    hr = createSession(&sessionHandle);
    if (FAILED(hr))
    {
        return hr;
    }

    HINTERNET connectionHandle;
    hr = createConnection(sessionHandle, L"localhost", &connectionHandle);
    if (FAILED(hr))
    {
        return hr;
    }

    hr = makeRequest(connectionHandle, L"/?length=101&chunked=1", true);
    if (FAILED(hr))
    {
        return hr;
    }

    InternetCloseHandle(connectionHandle);
    InternetCloseHandle(sessionHandle);

    return S_OK;
}

int wmain(int argc, wchar_t argv[])
{
    HRESULT hr = mainCore();

    if (FAILED(hr))
    {
        wprintf(L"Process failed with %#08x\n", hr);
        return hr;
    }

    wprintf(L"Done.\n");

    return 0;
}
