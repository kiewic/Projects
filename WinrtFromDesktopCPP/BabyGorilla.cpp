#include "stdafx.h"
#include "BabyGorilla.h"

BabyGorilla::~BabyGorilla()
{
    wprintf(L"BabyGorilla dies.\r\n");
}

HRESULT BabyGorilla::RuntimeClassInitialize(_In_ MotherGorilla* mother)
{
    this->mother = mother;
    return S_OK;
}

