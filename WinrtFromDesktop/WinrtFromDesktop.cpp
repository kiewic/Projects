// WinrtFromDesktop.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "CompletedHandler.h"
#include <roapi.h>
#include <windows.data.json.h>
#include <windows.foundation.h>
#include <windows.storage.h>
#include <windows.web.syndication.h>
#include <wrl/client.h>
#include <wrl/event.h> // Callback.
#include <wrl/wrappers/corewrappers.h> // HString and HStringReference.

#pragma comment(lib, "runtimeobject.lib")

using namespace ABI::Windows::Data::Json;
using namespace ABI::Windows::Storage;
using namespace ABI::Windows::Web::Syndication;
using namespace Microsoft::WRL::Wrappers;

//HRESULT OnGetFileFromPath(IAsyncOperation<StorageFile*>* file, AsyncStatus asyncStatus)
//{
//    UNREFERENCED_PARAMETER(file);
//    UNREFERENCED_PARAMETER(asyncStatus);
//
//    // Mutex here?
//    return S_OK;
//}

HRESULT LoadFeed()
{
    ComPtr<ISyndicationFeed> syndicationFeed;
    HRESULT hr = Windows::Foundation::ActivateInstance(
        HStringReference(RuntimeClass_Windows_Web_Syndication_SyndicationFeed).Get(),
        &syndicationFeed);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    ComPtr<IStorageFileStatics> storageFileStatics;
    hr = Windows::Foundation::GetActivationFactory(
        HStringReference(RuntimeClass_Windows_Storage_StorageFile).Get(),
        &storageFileStatics);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    ComPtr<ABI::Windows::Foundation::IAsyncOperation<StorageFile*>> getFileFromPathOperation;
    hr = storageFileStatics->GetFileFromPathAsync(HStringReference(L"c:\\Users\\Gilberto\\Downloads\\feed.xml").Get(), &getFileFromPathOperation);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    //ComPtr<IAsyncOperationCompletedHandler<StorageFile*>>
    //    getFileFromPathHandler(Callback<IAsyncOperationCompletedHandler<StorageFile*>>(&OnGetFileFromPath));

    ComPtr<CompletedHandler<StorageFile*>> getFileFromPathHandler = Make<CompletedHandler<StorageFile*>>();
    if (!getFileFromPathHandler.Get())
    {
        DebugBreak();
        return E_OUTOFMEMORY;
    }

    //ComPtr<IAsyncOperationCompletedHandler<StorageFile*>> foo;
    //hr = getFileFromPathHandler.As(&foo);
    //if (FAILED(hr))
    //{
    //    DebugBreak();
    //    return hr;
    //}

    hr = getFileFromPathOperation->put_Completed(getFileFromPathHandler.Get());
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = getFileFromPathHandler->WaitOne();
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    ComPtr<IAsyncInfo> asyncInfo;
    hr = getFileFromPathOperation.As(&asyncInfo);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    AsyncStatus status;
    hr = asyncInfo->get_Status(&status);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    if (status != AsyncStatus::Completed)
    {
        HRESULT errorCode;
        hr = asyncInfo->get_ErrorCode(&errorCode);
        if (FAILED(hr))
        {
            DebugBreak();
        }
        DebugBreak();
        return errorCode;
    }

    ComPtr<IStorageFile> file;
    hr = getFileFromPathOperation->GetResults(&file);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    ComPtr<IFileIOStatics> fileIoStatics;
    hr = Windows::Foundation::GetActivationFactory(
        HStringReference(RuntimeClass_Windows_Storage_FileIO).Get(),
        &fileIoStatics);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    ComPtr<ABI::Windows::Foundation::IAsyncOperation<HSTRING>> readTextOperation;
    hr = fileIoStatics->ReadTextAsync(file.Get(), &readTextOperation);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    ComPtr<CompletedHandler<HSTRING>> readTextHandler = Make<CompletedHandler<HSTRING>>();
    if (!readTextHandler.Get())
    {
        DebugBreak();
        return E_OUTOFMEMORY;
    }

    hr = readTextOperation->put_Completed(readTextHandler.Get());
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = readTextHandler->WaitOne();
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = readTextOperation.As(&asyncInfo);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = asyncInfo->get_Status(&status);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    if (status != AsyncStatus::Completed)
    {
        HRESULT errorCode;
        hr = asyncInfo->get_ErrorCode(&errorCode);
        if (FAILED(hr))
        {
            DebugBreak();
        }
        DebugBreak();
        return errorCode;
    }

    Microsoft::WRL::Wrappers::HString feedString;
    hr = readTextOperation->GetResults(feedString.GetAddressOf());
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = syndicationFeed->Load(feedString.Get());
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    return S_OK;
}

HRESULT TestJson()
{
    HSTRING className;
    HRESULT hr = WindowsCreateString(
        RuntimeClass_Windows_Data_Json_JsonObject,
        wcslen(RuntimeClass_Windows_Data_Json_JsonObject),
        &className);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    IInspectable* inspectable;
    hr = RoActivateInstance(className, &inspectable);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    ULONG iidCount;
    IID* iids;
    hr = inspectable->GetIids(&iidCount, &iids);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    wprintf(L"%d\r\n", iidCount);

    IJsonValue* jsonValue;
    hr = inspectable->QueryInterface(__uuidof(jsonValue), reinterpret_cast<void**>(&jsonValue));
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    HSTRING jsonString;
    hr = jsonValue->Stringify(&jsonString);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    // TODO: Confirm that length parameter is required. I heard it is needed beacuase it is not guaranteed that the
    // PCWSTR will end on null character.
    UINT32 length;
    const wchar_t* rawJsonString;
    rawJsonString = WindowsGetStringRawBuffer(jsonString, &length);
    wprintf(L"%s (%d)\r\n", rawJsonString, length);

    hr = WindowsDeleteString(className);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = WindowsDeleteString(jsonString);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    return S_OK;
}



int wmain(int argc, wchar_t* argv[])
{
    UNREFERENCED_PARAMETER(argc);
    UNREFERENCED_PARAMETER(argv);

    // TODO: Should we call RoUninitialize?
    HRESULT hr = ::RoInitialize(RO_INIT_MULTITHREADED);
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = LoadFeed();
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    hr = TestJson();
    if (FAILED(hr))
    {
        DebugBreak();
        return hr;
    }

    return 0;
}
