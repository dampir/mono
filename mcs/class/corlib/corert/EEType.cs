using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime
{
	// Wrapper around EEType pointers that may be indirected through the IAT if their low bit is set.
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EETypeRef
    {
        private byte* _value;

        public EEType* Value
        {
            get
            {
                if (((int)_value & IndirectionConstants.IndirectionCellPointer) == 0)
                    return (EEType*)_value;
                return *(EEType**)(_value - IndirectionConstants.IndirectionCellPointer);
            }
#if TYPE_LOADER_IMPLEMENTATION
            set
            {
                _value = (byte*)value;
            }
#endif
        }
    }
}