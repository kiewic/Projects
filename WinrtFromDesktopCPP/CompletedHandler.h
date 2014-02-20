#pragma once

#include "stdafx.h"

template <class TResult>
class CompletedHandler : public RuntimeClass<RuntimeClassFlags< ClassicCom >, IAsyncOperationCompletedHandler<TResult>, FtmBase>
{
public:
    CompletedHandler()
    {
        this->invokeEvent = CreateEvent(nullptr, false, false, nullptr);
        if (this->invokeEvent == NULL)
        {
            HRESULT hr = GetLastError();
            UNREFERENCED_PARAMETER(hr);
            DebugBreak();
        }
    }

    ~CompletedHandler()
    {
        BOOL result = CloseHandle(this->invokeEvent);
        if (!result)
        {
            HRESULT hr = GetLastError();
            UNREFERENCED_PARAMETER(hr);
            DebugBreak();
        }
    }

    // IAsyncOperationCompletedHandler
    HRESULT STDMETHODCALLTYPE Invoke(IAsyncOperation<TResult>* asyncInfo, AsyncStatus status)
    {
        UNREFERENCED_PARAMETER(asyncInfo);
        UNREFERENCED_PARAMETER(status);

        BOOL result = SetEvent(this->invokeEvent);
        if (!result)
        {
            HRESULT hr = GetLastError();
            UNREFERENCED_PARAMETER(hr);
            return hr;
        }

        return S_OK;
    }

    // Non-inhereted methods.
    HRESULT WaitOne()
    {
        DWORD index;
        return CoWaitForMultipleObjects(
            0,
            INFINITE,
            1,
            &this->invokeEvent,
            &index);
    }

private:
    HANDLE invokeEvent;
};

