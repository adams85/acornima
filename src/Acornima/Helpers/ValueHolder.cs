using System.Runtime.CompilerServices;

namespace Acornima.Helpers;

// We need an internal type to make some of our logic foolproof
// (e.g., see the type check in the AdditionalDataSlot.PrimaryData property).

internal struct ValueHolder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueHolder(object? data)
    {
        Data = data;
    }

    public object? Data;
}
