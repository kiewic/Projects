#include "stdafx.h"
#include "MotherGorilla.h"

MotherGorilla::~MotherGorilla()
{
    wprintf(L"MotherGorilla dies.\r\n");
}

HRESULT MotherGorilla::CreateBaby(_Outptr_ BabyGorilla** baby)
{
    HRESULT hr;

    *baby = nullptr;

    ComPtr<BabyGorilla> localBaby;
    IfFailedReturn(MakeAndInitialize<BabyGorilla>(&localBaby, this));

    ComPtr<IWeakReference> weakReaference;

    AsWeak(localBaby.Get(), &this->baby);

    // BabyGorilla class requires a UUID, otherwise WeakRef::CopyTo() fails with
    // "error C2787: 'BabyGorilla' : no GUID has been associated with this object"
    return this->baby.CopyTo<BabyGorilla>(baby);
}
