using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Acornima.Helpers;

/// <summary>
/// A struct that can store a read-only managed reference.
/// </summary>
internal readonly ref struct ReadOnlyRef<T>
{
#if NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyRef(ref readonly T value)
    {
        Value = ref value;
    }

    public readonly ref readonly T Value;
#else
    private readonly ReadOnlySpan<T> _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    public ReadOnlyRef(ref readonly T value)
    {
        _value = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in value), 1);
    }
#else
    public ReadOnlyRef(ReadOnlySpan<T> span, int index)
    {
        _value = span.Slice(index, 1);
    }
#endif

    public ref readonly T Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref MemoryMarshal.GetReference(_value); }
#endif
}
