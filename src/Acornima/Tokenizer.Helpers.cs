using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

public partial class Tokenizer
{
    internal const int NonIdentifierDeduplicationThreshold = 20;

    [Flags]
    internal enum CharFlags : byte
    {
        None = 0,
        LineTerminator = 1 << 0,
        WhiteSpace = 1 << 1,
        IdentifierStart = 1 << 2,
        IdentifierPart = 1 << 3,
        // NOTE: Line terminators and whitespace characters are disjunct sets,
        // so we can also use this combination of bits to indicate comment start characters (see also Tokenizer.SkipSpace).
        Skipped = LineTerminator | WhiteSpace
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CharFlags GetCharFlags(char ch)
    {
        return (CharFlags)(CharacterData[ch >> 1] >> ((ch & 1) << 2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNewLine(char ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/whitespace.js > `export function isNewLine`

        // NOTE: In isolation (when checking for other character categories is not needed),
        // this is around 2x faster than the lookup approach.
        return ch is '\n' or '\r' or '\u2028' or '\u2029';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ContainsLineBreak(ReadOnlySpan<char> text)
    {
        return text.IndexOfAny("\r\n\u2028\u2029".AsSpan()) >= 0;
    }

    internal static int NextLineBreak(string text, int startIndex, int endIndex)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/whitespace.js > `export function nextLineBreak`

        for (var i = startIndex; i < endIndex; i++)
        {
            var ch = text.CharCodeAt(i);
            if (ch == '\r')
            {
                return text.CharCodeAt(++i) == '\n' ? i + 1 : i;
            }
            else if (ch is '\n' or '\u2028' or '\u2029')
            {
                return i + 1;
            }
        }
        return -1;
    }

    // The `GetLineInfo` function is mostly useful when the
    // `Locations` option is off (for performance reasons) and you
    // want to find the line/column position for a given character
    // offset. `input` should be the code string that the offset refers
    // into.
    internal static Position GetLineInfo(string text, int offset, out int lineStartIndex)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/locutil.js > `export function getLineInfo`

        lineStartIndex = 0;
        for (var line = 1; ;)
        {
            var nextBreak = NextLineBreak(text, lineStartIndex, offset);
            if (nextBreak < 0 || nextBreak > offset)
            {
                return new Position(line, offset - lineStartIndex);
            }
            ++line;
            lineStartIndex = nextBreak;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhiteSpace(char ch)
    {
        return (GetCharFlags(ch) & CharFlags.Skipped) == CharFlags.WhiteSpace;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsIdentifierStart(int cp, bool allowAstral = true)
    {
        return cp <= char.MaxValue
            ? (GetCharFlags((char)cp) & CharFlags.IdentifierStart) != 0
            : allowAstral && cp >= 0 && CodePointRange.RangesContain(cp, IdentifierStartAstralRanges, RangeLengthLookup);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsIdentifierChar(int cp, bool allowAstral = true)
    {
        return cp <= char.MaxValue
            ? (GetCharFlags((char)cp) & CharFlags.IdentifierPart) != 0
            : allowAstral && cp >= 0 && CodePointRange.RangesContain(cp, IdentifierPartAstralRanges, RangeLengthLookup);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CharCodeAtPosition()
    {
        return _input.CharCodeAt(_position, _endPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CharCodeAtPosition(int offset)
    {
        return _input.CharCodeAt(_position + offset, _endPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FullCharCodeAtPosition()
    {
        return _input.CodePointAt(_position, _endPosition);
    }

    [StringMatcher(
        // basic keywords (should include all keywords defined by TokenType.GetKeywordBy)
        "if", "in", "do", "var", "for", "new", "try", "let", "this", "else", "case", "void", "with", "enum",
        "while", "break", "catch", "throw", "const", "yield", "class", "super", "return", "typeof", "delete", "switch",
        "export", "import", "default", "finally", "extends", "function", "continue", "debugger", "instanceof",
        // contextual keywords (should at least include "null", "false" and "true")
        "as", "of", "get", "set", "false", "from", "null", "true", "async", "await", "static", "constructor",
        // some common identifiers/literals in our test data set (benchmarks + test suite)
        "undefined", "length", "object", "Object", "obj", "Array", "Math", "data", "done", "args", "arguments", "Symbol", "prototype",
        "options", "value", "name", "self", "key", "\"use strict\"", "use strict"
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    private static partial string? TryGetInternedString(ReadOnlySpan<char> source);

    internal static string DeduplicateString(ReadOnlySpan<char> s, ref StringPool stringPool)
    {
        if (s.Length > 1)
        {
            return TryGetInternedString(s) ?? stringPool.GetOrCreate(s);
        }
        else if (s.Length > 0)
        {
            return s[0].ToStringCached();
        }
        else
        {
            return string.Empty;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string DeduplicateString(ReadOnlySpan<char> s, ref StringPool stringPool, int threshold)
    {
        return s.Length <= threshold ? DeduplicateString(s, ref stringPool) : s.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort GetDigitValue(int ch)
    {
        var tmp = ch - '0';
        if ((uint)tmp <= 9)
        {
            return (ushort)tmp;
        }
        else if (ch >= 'a')
        {
            return (ushort)(ch - ('a' - 10));
        }
        else if (ch >= 'A')
        {
            return (ushort)(ch - ('A' - 10));
        }
        else
        {
            return ushort.MaxValue;
        }
    }

    private static BigInteger ParseIntToBigInteger(ReadOnlySpan<char> slice, byte radix)
    {
        BigInteger value = 0;
        int i;
        for (i = 0; i < slice.Length; i++)
        {
            var ch = slice[i];
            if (ch == '_')
            {
                continue;
            }

            var digitValue = GetDigitValue(ch);
            Debug.Assert(digitValue < radix, $"Invalid digit in number: U+{(ushort)ch:X4}");

            value = value * radix + digitValue;
        }

        Debug.Assert(i == slice.Length, $"Invalid number: {slice.ToString()}");

        return value;
    }

    private static double ParseIntToDouble(ReadOnlySpan<char> slice, byte radix)
    {
        double value = 0;
        var modulo = 1.0;
        int i;
        for (i = slice.Length; i > 0;)
        {
            var ch = slice[--i];
            if (ch == '_')
            {
                continue;
            }

            var digitValue = GetDigitValue(ch);
            Debug.Assert(digitValue < radix, $"Invalid digit in number: U+{(ushort)ch:X4}");

            value += modulo * digitValue;
            modulo *= radix;
        }

        Debug.Assert(i == 0, $"Invalid number: {slice.ToString()}");

        return value;
    }

    private static double ParseFloatToDouble(ReadOnlySpan<char> slice, bool hasSeparator, Tokenizer tokenizer)
    {
        if (hasSeparator)
        {
            tokenizer.AcquireStringBuilder(out var sb);
            try
            {
                if (sb.Capacity < slice.Length)
                {
                    sb.Capacity = slice.Length;
                }

                for (var i = 0; i < slice.Length; i++)
                {
                    var ch = slice[i];
                    if (ch != '_')
                    {
                        sb.Append(ch);
                    }
                }

                slice = sb.ToString().AsSpan();
            }
            finally { tokenizer.ReleaseStringBuilder(ref sb); }
        }

        try { return double.Parse(slice.ToParsable(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture); }
        catch (OverflowException)
        {
            // In older runtimes, double.Parse throws OverflowException ("Value was either too large or too small for a Double")
            // when we feed a too big number to it. However, big numbers should be converted into double.PositiveInfinity.
            return double.PositiveInfinity;
        }
    }
}
