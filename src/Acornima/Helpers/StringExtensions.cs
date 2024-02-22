using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System;

internal static class StringExtensions
{
    private static readonly string[] s_charToString = new string[256];

    static StringExtensions()
    {
        for (var i = 0; i < s_charToString.Length; i++)
        {
            s_charToString[i] = ((char)i).ToString();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStringCached(this char c)
    {
        int index = c;
        var temp = s_charToString;
        if ((uint)index < temp.Length)
        {
            return temp[index];
        }
        return c.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRange(this char c, ushort min, ushort max)
    {
        Debug.Assert(min <= max);
        return c - (uint)min <= max - (uint)min;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOctalDigit(this char c) => c.IsInRange('0', '7');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDecimalDigit(this char c) => c.IsInRange('0', '9');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBasicLatinLetter(this char c) => c.IsInRange('a', 'z') || c.IsInRange('A', 'Z');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSurrogate(this char c) => c.IsInRange(0xD800, 0xDFFF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHighSurrogate(this char c) => c.IsInRange(0xD800, 0xDBFF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLowSurrogate(this char c) => c.IsInRange(0xDC00, 0xDFFF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CharCodeAt(this string s, int index)
    {
        if ((uint)index < s.Length)
        {
            return s[index];
        }

        // NOTE: For indicating an unavailable character, we use a negative value which
        // results in '\0' when converted to char. In some cases we may rely on this behavior.
        return int.MinValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CharCodeAt(this string s, int index, int endIndex)
    {
        Debug.Assert((uint)endIndex <= s.Length);

        if ((uint)index < endIndex)
        {
            return s[index];
        }

        // NOTE: For indicating an unavailable character, we use a negative value which
        // results in '\0' when converted to char. In some cases we may rely on this behavior.
        return int.MinValue;
    }

    public static int CodePointAt(this string s, int index, int endIndex)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/util.js > `export function codePointToString`

        var ch = s.CharCodeAt(index, endIndex);
        if (((char)ch).IsHighSurrogate())
        {
            var nextCh = s.CharCodeAt(index + 1, endIndex);
            if (((char)nextCh).IsLowSurrogate())
            {
                return (ch << 10) + nextCh - 0x35FDC00;
            }
        }

        return ch;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> SliceBetween(this string s, int startIndex, int endIndex)
    {
        return s.AsSpan(startIndex, endIndex - startIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    ReadOnlySpan<char>
#else
    string
#endif
    ToParsable(this ReadOnlySpan<char> s)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return s;
#else
        return s.ToString();
#endif
    }

    public static bool TryReadInt(ref this ReadOnlySpan<char> s, out int result)
    {
        result = 0;
        char c;
        int i;
        for (i = 0; i < s.Length && (c = s[i]).IsDecimalDigit(); i++)
        {
            result = checked(result * 10 + c - '0');
        }

        if (i == 0)
        {
            result = default;
            return false;
        }

        s = s.Slice(i);
        return true;
    }

#if NETCOREAPP3_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSequenceHashCode(this ReadOnlySpan<char> span)
    {
        return string.GetHashCode(span);
    }
#else
    /// <summary>
    /// Gets the (positive) hashcode for a given <see cref="ReadOnlySpan{T}"/> instance.
    /// </summary>
    /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance.</param>
    /// <returns>The hashcode for <paramref name="span"/>.</returns>
    public static int GetSequenceHashCode(this ReadOnlySpan<char> span)
    {
        // This can be further optimized as shown here:
        // https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/v7.1.2/Microsoft.Toolkit.HighPerformance/Helpers/Internals/SpanHelper.Hash.cs#L87

        int hash = 5381;

        while (span.Length >= 8)
        {
            // Doing a left shift by 5 and adding is equivalent to multiplying by 33.
            // This is preferred for performance reasons, as when working with integer
            // values most CPUs have higher latency for multiplication operations
            // compared to a simple shift and add. For more info on this, see the
            // details for imul, shl, add: https://gmplib.org/~tege/x86-timing.pdf.
            hash = unchecked(((hash << 5) + hash) ^ span[0].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[1].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[2].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[3].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[4].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[5].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[6].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[7].GetHashCode());

            span = span.Slice(8);
        }

        if (span.Length >= 4)
        {
            hash = unchecked(((hash << 5) + hash) ^ span[0].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[1].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[2].GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ span[3].GetHashCode());

            span = span.Slice(4);
        }

        if (span.Length > 0)
        {
            hash = unchecked(((hash << 5) + hash) ^ span[0].GetHashCode());
            if (span.Length > 1)
            {
                hash = unchecked(((hash << 5) + hash) ^ span[1].GetHashCode());
                if (span.Length > 2)
                {
                    hash = unchecked(((hash << 5) + hash) ^ span[2].GetHashCode());
                }
            }
        }

        return hash;
    }
#endif
}
