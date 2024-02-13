using System.Runtime.CompilerServices;

namespace System;

internal static class SpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T Last<T>(this ReadOnlySpan<T> span) => ref span[span.Length - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Last<T>(this Span<T> span) => ref span[span.Length - 1];

    public static int FindIndex<T>(this ReadOnlySpan<T> s, Predicate<T> match, int startIndex = 0)
    {
        for (; startIndex < s.Length; startIndex++)
        {
            if (match(s[startIndex]))
            {
                return startIndex;
            }
        }
        return -1;
    }
}
