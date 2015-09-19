#pragma once

#include "stdafx.h"

#define IfFailedReturn(x) \
do \
{ \
    hr = x; \
    if (FAILED(hr)) \
    { \
        DebugBreak(); \
        return hr; \
    } \
} while (0)

