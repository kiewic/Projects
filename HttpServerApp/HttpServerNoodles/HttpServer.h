#pragma once
#include "AutoSOCKET.h"
#include "WinSockInitialization.h"

namespace HttpServerNoodles
{

    class FooIoContext : public IMFAsyncCallback
    {
    public:
        // IMFAsyncCallback memebers.

        virtual HRESULT STDMETHODCALLTYPE GetParameters(
            /* [out] */ __RPC__out DWORD *pdwFlags,
            /* [out] */ __RPC__out DWORD *pdwQueue)
        {
            return S_OK;
        }

        virtual HRESULT STDMETHODCALLTYPE Invoke(
            /* [in] */ __RPC__in_opt IMFAsyncResult *pAsyncResult)
        {
            return S_OK;
        }

        // IUnknown members.

        virtual HRESULT STDMETHODCALLTYPE QueryInterface(
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ _COM_Outptr_ void __RPC_FAR *__RPC_FAR *ppvObject)
        {
            if (riid == IID_IUnknown || riid == IID_IMFAsyncCallback)
            {
                // Increment the reference count and return the pointer.
                *ppvObject = (LPVOID)this;
                AddRef();
                return S_OK;
            }

            return E_NOINTERFACE;
        }

        virtual ULONG STDMETHODCALLTYPE AddRef()
        {
            InterlockedIncrement(&m_cRef);
            return m_cRef;
        }

        virtual ULONG STDMETHODCALLTYPE Release()
        {
            // Decrement the object's internal counter.
            ULONG ulRefCount = InterlockedDecrement(&m_cRef);
            if (0 == m_cRef)
            {
                delete this;
            }
            return ulRefCount;
        }

    private:
        volatile unsigned int m_cRef;
    };



    public ref class HttpServer sealed
    {
    public:
        HttpServer();
        virtual ~HttpServer();

        void Start();
        void Stop();

    private:
        int StartTcpListener();

        AutoSOCKET autoListenerSocket;
        WinSockInitialization initialization;

        // TODO: Maybe move this in a class per accept.
        AutoSOCKET autoAcceptSocket;
        WSAOVERLAPPED overlapped;
        char outputBuffer[1024];
        int outputBufferLen = 1024; // At least ((sizeof(sockaddr_in) + 16) * 2
        FooIoContext* context;
    };
}
