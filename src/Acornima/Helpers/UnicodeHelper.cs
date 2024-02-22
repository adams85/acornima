using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Acornima.Helpers;

internal static class UnicodeHelper
{
    public const int LastCodePoint = 0x10FFFF;

    public static bool ContainsLoneSurrogate(this ReadOnlySpan<char> s)
    {
        for (var i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            if (ch.IsSurrogate())
            {
                if (ch >= 0xDC00 || !(++i < s.Length && s[i].IsLowSurrogate()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetSurrogatePair(uint cp, out char highSurrogate, out char lowSurrogate)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v6.0.16/src/libraries/System.Private.CoreLib/src/System/Text/UnicodeUtility.cs#L58

        Debug.Assert(cp is > char.MaxValue and <= LastCodePoint);

        highSurrogate = (char)((cp + ((0xD800u - 0x40u) << 10)) >> 10);
        lowSurrogate = (char)((cp & 0x3FFu) + 0xDC00u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetCodePoint(char highSurrogate, char lowSurrogate)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v6.0.16/src/libraries/System.Private.CoreLib/src/System/Text/UnicodeUtility.cs#L28

        return ((uint)highSurrogate << 10) + lowSurrogate - ((0xD800U << 10) + 0xDC00U - (1 << 16));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetCodePointLength(uint cp)
    {
        // NOTE: Equivalent to `cp <= 0xFFFFu ? 1 : 2` but around 2x faster.
        return (((int)cp - (char.MaxValue + 1)) >> 31) + 2;
    }

    public static string CodePointToString(int cp)
    {
        Debug.Assert(cp is >= 0 and <= LastCodePoint);

        if (cp <= char.MaxValue)
        {
            return ((char)cp).ToStringCached();
        }

        Span<char> chars = stackalloc char[2];
        GetSurrogatePair((uint)cp, out chars[0], out chars[1]);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return new string(chars);
#else
        return chars.ToString();
#endif
    }

    public static StringBuilder AppendCodePoint(this StringBuilder sb, int cp)
    {
        Debug.Assert(cp is >= 0 and <= LastCodePoint);

        if (cp <= char.MaxValue)
        {
            return sb.Append((char)cp);
        }

        Span<char> chars = stackalloc char[2];
        GetSurrogatePair((uint)cp, out chars[0], out chars[1]);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return sb.Append(chars);
#else
        return sb.Append(chars[0]).Append(chars[1]);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnicodeCategory GetUnicodeCategory(int codePoint)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return CharUnicodeInfo.GetUnicodeCategory(codePoint);
#else
        return codePoint <= char.MaxValue
            ? CharUnicodeInfo.GetUnicodeCategory((char)codePoint)
            : CharUnicodeInfo.GetUnicodeCategory(CodePointToString(codePoint), 0);
#endif
    }
}
