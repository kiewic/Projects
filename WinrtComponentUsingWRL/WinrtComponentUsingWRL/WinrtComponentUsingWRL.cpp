#include "pch.h"

#include "WinrtComponentUsingWRL_h.h"
#include <wrl.h>

using namespace Microsoft::WRL;
using namespace Windows::Foundation;

namespace ABI
{
    namespace WinrtComponentUsingWRL
    {
        class Foo : public RuntimeClass<IFoo>
        {
            InspectableClass(L"WinrtComponentUsingWRL.Foo", BaseTrust)

        public:
            Foo()
            {
            }

            HRESULT __stdcall DoBar(_In_ UINT32 value)
            {
                return HRESULT_FROM_WIN32(value);
            }
        };

        ActivatableClass(Foo);
    }
}