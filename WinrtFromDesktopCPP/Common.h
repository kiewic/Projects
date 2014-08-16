#pragma once

#include "stdafx.h"

#define IfFailedReturn(x) \
do \
{ \
if (FAILED(x)) \
    { \
    DebugBreak(); \
    return hr; \
    } \
} while (0)

