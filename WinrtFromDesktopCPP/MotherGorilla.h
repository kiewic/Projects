#pragma once

#include "stdafx.h"
#include "BabyGorilla.h"

class BabyGorilla;

class MotherGorilla : public RuntimeClass<FtmBase>
{
public:
    ~MotherGorilla();
    HRESULT CreateBaby(_Outptr_ BabyGorilla** baby);

private:
    //ComPtr<BabyGorilla> baby;
    WeakRef baby;
};
