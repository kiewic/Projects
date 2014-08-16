// WinrtFromDesktop.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "CompletedHandler.h"
#include "BabyGorilla.h"
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

HRESULT LoadFeed()
{
    ComPtr<ISyndicationFeed> syndicationFeed;
    HRESULT hr;
    IfFailedReturn(Windows::Foundation::ActivateInstance(
        HStringReference(RuntimeClass_Windows_Web_Syndication_SyndicationFeed).Get(),
        &syndicationFeed));

    ComPtr<IStorageFileStatics> storageFileStatics;
    IfFailedReturn(Windows::Foundation::GetActivationFactory(
        HStringReference(RuntimeClass_Windows_Storage_StorageFile).Get(),
        &storageFileStatics));

    ComPtr<ABI::Windows::Foundation::IAsyncOperation<StorageFile*>> getFileFromPathOperation;
    IfFailedReturn(storageFileStatics->GetFileFromPathAsync(
        HStringReference(L"c:\\Users\\Gilberto\\Downloads\\feed.xml").Get(),
        &getFileFromPathOperation));

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

    IfFailedReturn(getFileFromPathOperation->put_Completed(getFileFromPathHandler.Get()));

    IfFailedReturn(getFileFromPathHandler->WaitOne());

    ComPtr<IAsyncInfo> asyncInfo;
    IfFailedReturn(getFileFromPathOperation.As(&asyncInfo));

    AsyncStatus status;
    IfFailedReturn(asyncInfo->get_Status(&status));

    if (status != AsyncStatus::Completed)
    {
        HRESULT errorCode;
        IfFailedReturn(asyncInfo->get_ErrorCode(&errorCode));
        IfFailedReturn(errorCode);
        DebugBreak(); // We should never arrive here.
    }

    ComPtr<IStorageFile> file;
    IfFailedReturn(getFileFromPathOperation->GetResults(&file));

    ComPtr<IFileIOStatics> fileIoStatics;
    IfFailedReturn(Windows::Foundation::GetActivationFactory(
        HStringReference(RuntimeClass_Windows_Storage_FileIO).Get(),
        &fileIoStatics));

    ComPtr<ABI::Windows::Foundation::IAsyncOperation<HSTRING>> readTextOperation;
    IfFailedReturn(fileIoStatics->ReadTextAsync(file.Get(), &readTextOperation));

    ComPtr<CompletedHandler<HSTRING>> readTextHandler = Make<CompletedHandler<HSTRING>>();
    if (!readTextHandler.Get())
    {
        DebugBreak();
        return E_OUTOFMEMORY;
    }

    IfFailedReturn(readTextOperation->put_Completed(readTextHandler.Get()));

    IfFailedReturn(readTextHandler->WaitOne());

    IfFailedReturn(readTextOperation.As(&asyncInfo));

    IfFailedReturn(asyncInfo->get_Status(&status));

    if (status != AsyncStatus::Completed)
    {
        HRESULT errorCode;
        IfFailedReturn(asyncInfo->get_ErrorCode(&errorCode));
        IfFailedReturn(errorCode);
        DebugBreak(); // We should never arrive here.
    }

    Microsoft::WRL::Wrappers::HString feedString;
    IfFailedReturn(readTextOperation->GetResults(feedString.GetAddressOf()));

    IfFailedReturn(syndicationFeed->Load(feedString.Get()));

    return S_OK;
}

HRESULT TestJson()
{
    HRESULT hr;

    HSTRING className;
    IfFailedReturn(WindowsCreateString(
        RuntimeClass_Windows_Data_Json_JsonObject,
        wcslen(RuntimeClass_Windows_Data_Json_JsonObject),
        &className));

    IInspectable* inspectable;
    IfFailedReturn(RoActivateInstance(className, &inspectable));

    ULONG iidCount;
    IID* iids;
    IfFailedReturn(inspectable->GetIids(&iidCount, &iids));

    wprintf(L"Iids: %d\r\n", iidCount);

    IJsonValue* jsonValue;
    IfFailedReturn(inspectable->QueryInterface(__uuidof(jsonValue), reinterpret_cast<void**>(&jsonValue)));

    HSTRING jsonString;
    IfFailedReturn(jsonValue->Stringify(&jsonString));

    // TODO: Confirm that length parameter is required. I heard it is needed beacuase it is not guaranteed that the
    // PCWSTR will end on null character.
    UINT32 length;
    const wchar_t* rawJsonString;
    rawJsonString = WindowsGetStringRawBuffer(jsonString, &length);
    wprintf(L"Stringify: %s \r\nlength: %d\r\n", rawJsonString, length);

    IfFailedReturn(WindowsDeleteString(className));

    IfFailedReturn(WindowsDeleteString(jsonString));

    return S_OK;
}

class AutoRoUninitialize
{
public:
    ~AutoRoUninitialize()
    {
        // Closes Windows Runtime in the current thread.
        RoUninitialize();
    }
};

HRESULT GorillasTest()
{
    HRESULT hr;

    ComPtr<MotherGorilla> mother = Make<MotherGorilla>();
    if (mother == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    ComPtr<BabyGorilla> baby;
    IfFailedReturn(mother->CreateBaby(&baby));

    wprintf(L"mother: %d\r\n", mother.Reset());
    wprintf(L"baby: %d\r\n", baby.Reset());

    return S_OK;
}

int wmain(int argc, wchar_t* argv [])
{
    HRESULT hr;
    AutoRoUninitialize autoRoUninitialize;

    UNREFERENCED_PARAMETER(argc);
    UNREFERENCED_PARAMETER(argv);

    // Prior to using Windows Rutnime, the new thread must first enter an apartment by calling the following function.
    IfFailedReturn(::RoInitialize(RO_INIT_MULTITHREADED));

    IfFailedReturn(GorillasTest());

    IfFailedReturn(LoadFeed());

    IfFailedReturn(TestJson());

    return 0;
}
