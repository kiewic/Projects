#pragma once

#include "stdafx.h"
#include "MotherGorilla.h"

class MotherGorilla;

[uuid("7c8ab438-a275-467f-8bdd-7e556e0016f4")]
class BabyGorilla : public RuntimeClass<FtmBase>
{
    InspectableClass(L"BabyGorillaClass", TrustLevel::BaseTrust);

public:
    ~BabyGorilla();

    HRESULT RuntimeClassInitialize(_In_ MotherGorilla* mother);

private:
    ComPtr<MotherGorilla> mother;
};
