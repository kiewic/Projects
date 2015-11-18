// SChannelSample.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <schnlsp.h>

#define SECURITY_WIN32
#include <security.h>

#pragma comment (lib, "Secur32.lib")

SecBuffer buffers[12] = { 0 };


HRESULT GetMaximumSizeOfANegotiationTokenInBytes(_Out_ DWORD* tokenSize)
{
    SECURITY_STATUS result;
    PSecPkgInfoW description;

    result = QuerySecurityPackageInfoW(SCHANNEL_NAME_W, &description);
    if (result != SEC_E_OK)
    {
        return HRESULT_FROM_WIN32(result);
    }
    *tokenSize = description->cbMaxToken;

    FreeContextBuffer(description);
}

HRESULT PrepareOtbuffer(_Inout_ SecBufferDesc& otbuffer)
{
    HRESULT hr;

    DWORD tokenSize;
    hr = GetMaximumSizeOfANegotiationTokenInBytes(&tokenSize);
    if (FAILED(hr))
    {
        return hr;
    }

    // TODO: Use a secure memory allocation function.
    void* otoken = malloc(tokenSize);

    // 1st buffer contains out-bound token data.
    // 2nd buffer contains in-bound unused data (start of 1st message).
    memset(&otbuffer, 0, sizeof(otbuffer));
    otbuffer.ulVersion = SECBUFFER_VERSION;
    otbuffer.pBuffers = buffers + 2;
    otbuffer.cBuffers = 2;
    otbuffer.pBuffers[0].BufferType = SECBUFFER_TOKEN;
    otbuffer.pBuffers[0].cbBuffer = tokenSize;
    otbuffer.pBuffers[0].pvBuffer = otoken; // Buffer for out-bound negotiation token data.
    otbuffer.pBuffers[1].BufferType = SECBUFFER_EMPTY;
    otbuffer.pBuffers[1].cbBuffer = 0;
    otbuffer.pBuffers[1].pvBuffer = 0;

    return hr;
}

void AcceptToken(_In_ SecBufferDesc& otbuffer)
{
    for (i = 0; (i < otbuffer.cBuffers); ++i)
    {
        PSecBuffer buffer = otbuffer.pBuffers[i];
        if (buffer->BufferType == SECBUFFER_TOKEN)
        {
            /* forward data. */
            channel->accept_encrypted(channel,
                buffer->pvBuffer, buffer->cbBuffer);
            /* expire token. */
            buffer->cbBuffer = 0;
        }
        if (buffer->BufferType == SECBUFFER_EXTRA)
        {
            /* forward data. */
            channel->accept_overflow(channel,
                buffer->pvBuffer, buffer->cbBuffer);
            /* expire token. */
            buffer->cbBuffer = 0;
        }
    }
}

HRESULT StartNegotiationClientHello(_In_ CredHandle& credentialsHandle)
{
    HRESULT hr;

    DWORD query =
        //ISC_REQ_ALLOCATE_MEMORY |
        ISC_REQ_SEQUENCE_DETECT |
        ISC_REQ_REPLAY_DETECT |
        ISC_REQ_CONFIDENTIALITY |
        ISC_RET_EXTENDED_ERROR |
        ISC_REQ_STREAM |
        ISC_REQ_MANUAL_CRED_VALIDATION;

    SecBufferDesc otbuffer = { 0 };
    hr = PrepareOtbuffer(otbuffer);
    if (FAILED(hr))
    {
        return hr;
    }

    PCtxtHandle lhs = nullptr;
    SEC_WCHAR* target = nullptr;
    PSecBufferDesc put = nullptr;
    CtxtHandle channelStateHandle = { 0 };
    PSecBufferDesc get = &otbuffer;
    DWORD reply;

    hr = InitializeSecurityContextW(
        &credentialsHandle,
        lhs, // In first call this is null.
        target,
        query,
        0, // reserved
        0,
        put,
        0, // reserved
        &channelStateHandle,
        get,
        &reply,
        0);
    if (FAILED(hr))
    {
        if (hr == SEC_E_INCOMPLETE_MESSAGE)
        {
            // TODO
            return hr;
        }
        return hr;
    }

    // Negotiation in progress.
    if (hr == SEC_I_CONTINUE_NEEDED)
    {
        printf("Client: obtained context handle.\n");

        AcceptToken(otbuffer);

        //accept_token(channel);
        //prepare_itbuffer(channel);
        //channel->state = secure_channel_unsafe;
    }

    return S_OK;
}

HRESULT ConnectClient()
{
    SECURITY_STATUS result;

    SCHANNEL_CRED identity = { 0 };
    identity.dwVersion = SCHANNEL_CRED_VERSION;
    identity.dwFlags =
        SCH_CRED_NO_DEFAULT_CREDS |
        SCH_CRED_NO_SYSTEM_MAPPER |
        SCH_CRED_REVOCATION_CHECK_CHAIN;

    // Client-side certificate is optional.
    CredHandle credentialsHandle;

    result = AcquireCredentialsHandleW(
        0,
        SCHANNEL_NAME_W,
        SECPKG_CRED_OUTBOUND,
        0,
        &identity,
        0,
        0,
        &credentialsHandle,
        0);
    if (result != SEC_E_OK)
    {
        return HRESULT_FROM_WIN32(result);
    }

    return StartNegotiationClientHello(credentialsHandle);
}

void GetErrorText(_In_ DWORD code)
{
    WCHAR buffer[2000];

    int bytesWritten = FormatMessage(
        FORMAT_MESSAGE_FROM_SYSTEM, // formatting options flag
        NULL, // not a source DLL or format string
        code, // returned by GetLastError()
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        buffer,
        ARRAYSIZE(buffer),
        NULL);

    if (bytesWritten == 0)
    {
        wprintf(L"Format message failed with 0x%x\r\n", GetLastError());
        return;
    }

    wprintf(L"0x%08x: %s\r\n", code, buffer);
}

int _tmain(int argc, _TCHAR* argv[])
{
    HRESULT hr;

    hr = ConnectClient();
    GetErrorText(hr);

    return hr;
}
