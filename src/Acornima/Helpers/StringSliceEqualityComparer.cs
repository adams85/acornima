using System;
using System.Collections.Generic;

namespace Acornima.Helpers;

internal class StringSliceEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
{
    public static readonly StringSliceEqualityComparer Instance = new();

    private StringSliceEqualityComparer() { }

    public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
    {
        return x.Span.SequenceEqual(y.Span);
    }

    public int GetHashCode(ReadOnlyMemory<char> obj)
    {
        return obj.Span.GetSequenceHashCode();
    }
}
