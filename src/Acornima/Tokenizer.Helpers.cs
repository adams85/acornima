using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

#pragma warning disable IDE0005 // Remove unnecessary import
using System.Buffers;
using System.Runtime.InteropServices;
#pragma warning restore IDE0005 // Remove unnecessary import

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
    internal int CharCodeAt(int position)
    {
        return _input.CharCodeAt(position, _endPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int CharCodeAtPosition()
    {
        return _input.CharCodeAt(_position, _endPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int CharCodeAtPosition(int offset)
    {
        return _input.CharCodeAt(_position + offset, _endPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FullCharCodeAt(int position)
    {
        return _input.CodePointAt(position, _endPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FullCharCodeAtPosition()
    {
        return _input.CodePointAt(_position, _endPosition);
    }

    [StringMatcher(
        // basic keywords (should include all keywords defined by TokenType.GetKeywordBy)
        "if", "in", "do", "var", "for", "new", "try", "let", "this", "else", "case", "void", "with", "enum",
        "while", "break", "catch", "throw", "const", "yield", "class", "super", "return", "typeof", "delete", "switch",
        "export", "import", "default", "finally", "extends", "function", "continue", "debugger", "instanceof",
        // contextual keywords (should at least include "null", "false" and "true")
        "as", "of", "get", "set", "from", "null", "false", "true", "async", "await", "using", "static", "constructor",
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

    #region Number parsing

    // The number of bits a double's significand carries.
    private const int DoubleSignificandBitCount = 53;

    private static readonly BigInteger s_ten = new(10);
    private static readonly BigInteger s_tenPow19 = new(10_000_000_000_000_000_000UL);

    private static ReadOnlySpan<byte> DigitValueLookup
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new byte[64]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F,
            0x3F, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F,
            0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F,
            0x3F, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetDigitValue(int ch)
    {
        // A lookup-based, branchless algorithm to convert a hexadecimal character to its numerical value.
        // It even avoids array bounds checking and is 4–5x faster than the branched implementation.
        // For invalid hex characters, it returns some arbitrary value that is guaranteed to be >= 16.

        var tmp = (uint)(ch - '0');
        var value = DigitValueLookup[(int)(tmp & 0x3FU)];
        return (tmp & ~0x3FU) | value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double UInt64ToDouble(ulong value)
    {
        // Before .NET 9, ulong to double conversion doesn't match the ECMAScript specification
        // (https://tc39.es/ecma262/#sec-literals-numeric-literals) in the [2^63, 2^64) range.
        // See also: https://github.com/adams85/acornima/issues/53

#if NET9_0_OR_GREATER
        return value;
#else
        if (value < 1UL << 63)
        {
            return (long)value;
        }

        return (long)((value >> 1) | (value & 1)) * 2.0;
#endif
    }

    private static BigInteger ParseIntToBigInteger(ReadOnlySpan<char> slice)
    {
        // The maximum number of decimal digits that is guaranteed to fit in a ulong.
        const int maxAccumulatedDigitCount = 19; // Floor(Log10(Pow(2, sizeof(ulong) * 8)))

        var value = BigInteger.Zero;
        var chunk = 0UL;
        var chunkDigitCount = 0;

        int i;
        for (i = 0; i < slice.Length; i++)
        {
            var ch = slice[i];
            var digitValue = GetDigitValue(ch);
            if (digitValue >= 10)
            {
                if (ch == '_')
                {
                    continue;
                }

                Debug.Fail($"Invalid digit in number: U+{(ushort)ch:X4}");
            }

            chunk = chunk * 10 + digitValue;
            chunkDigitCount++;
            if (chunkDigitCount == maxAccumulatedDigitCount)
            {
                value = value * s_tenPow19 + chunk;
                chunk = 0;
                chunkDigitCount = 0;
            }
        }

        Debug.Assert(i == slice.Length, $"Invalid big integer: {slice.ToString()}");

        if (chunkDigitCount > 0)
        {
            value = value * BigInteger.Pow(s_ten, chunkDigitCount) + chunk;
        }

        return value;
    }

    private static BigInteger ParseRadixIntToBigInteger(ReadOnlySpan<char> slice, byte radix)
    {
        Debug.Assert(radix is 2 or 8 or 16, $"Unexpected radix: {radix}");

        int bitsPerDigit, bitsPerChunk;

        // The maximum number of digits that is guaranteed to fit in a ulong.
        int maxAccumulatedDigitCount;

        switch (radix)
        {
            case 2:
                bitsPerDigit = 1;
                bitsPerChunk = maxAccumulatedDigitCount = 64; // sizeof(ulong) * 8
                break;
            case 8:
                bitsPerDigit = 3;
                bitsPerChunk = 63;
                maxAccumulatedDigitCount = 21; // sizeof(ulong) * 8 / 3
                break;
            default:
                bitsPerDigit = 4;
                bitsPerChunk = 64;
                maxAccumulatedDigitCount = 16; // sizeof(ulong) * 8 / 4
                break;
        }

        var value = BigInteger.Zero;
        var chunk = 0UL;
        var chunkDigitCount = 0;

        int i;
        for (i = 0; i < slice.Length; i++)
        {
            var ch = slice[i];
            var digitValue = GetDigitValue(ch);
            if (digitValue >= radix)
            {
                if (ch == '_')
                {
                    continue;
                }

                Debug.Fail($"Invalid digit in number: U+{(ushort)ch:X4}");
            }

            chunk = (chunk << bitsPerDigit) | digitValue;
            chunkDigitCount++;
            if (chunkDigitCount == maxAccumulatedDigitCount)
            {
                value = (value << bitsPerChunk) | chunk;

                chunk = 0;
                chunkDigitCount = 0;
            }
        }

        Debug.Assert(i == slice.Length, $"Invalid big integer: {slice.ToString()}");

        if (chunkDigitCount > 0)
        {
            value = (value << (chunkDigitCount * bitsPerDigit)) | chunk;
        }

        return value;
    }

    private static double ParseRadixIntToDouble(ReadOnlySpan<char> slice, byte radix)
    {
        Debug.Assert(radix is 2 or 8 or 16, $"Unexpected radix: {radix}");

        // Every radix handled here is a power of two, so the digits map directly onto the bits of the value:
        // the leading digits form the significand, and whether any non-zero digit was dropped beyond them is
        // the only other input rounding needs. This allows a single pass with no big-integer arithmetic,
        // however long the run of digits is.

        int bitsPerDigit;

        // The largest accumulator that another digit still fits into.
        ulong significandLimit;

        switch (radix)
        {
            case 2:
                bitsPerDigit = 1;
                significandLimit = ulong.MaxValue >> 1;
                break;
            case 8:
                bitsPerDigit = 3;
                significandLimit = ulong.MaxValue >> 3;
                break;
            default:
                bitsPerDigit = 4;
                significandLimit = ulong.MaxValue >> 4;
                break;
        }

        var significand = 0UL;
        var binaryExponent = 0L;
        var truncated = false;

        for (var i = 0; i < slice.Length; i++)
        {
            var ch = slice[i];
            var digitValue = GetDigitValue(ch);
            if (digitValue >= radix)
            {
                if (ch == '_')
                {
                    continue;
                }

                Debug.Fail($"Invalid digit in number: U+{(ushort)ch:X4}");
            }

            if (significand <= significandLimit)
            {
                significand = (significand << bitsPerDigit) | digitValue;
            }
            else
            {
                // The accumulator holds at least 61 bits by now - well past the 53 bits a double can represent exactly
                // and the one extra bit that determines the rounding direction - so the rest of the digits only need
                // to be remembered as "something non-zero was dropped".
                binaryExponent += bitsPerDigit;
                truncated |= digitValue != 0;
            }
        }

        return ScaleToDouble(significand, binaryExponent, truncated);

        static double ScaleToDouble(ulong significand, long binaryExponent, bool truncated)
        {
            // Returns the double nearest to significand * 2^binaryExponent. Rounding is half-to-even,
            // except when the truncated flag indicates that the value lies strictly above the boundary.
            // (Unlike ComposeDouble this has no subnormal case to serve: an integer read from digits is
            // either zero or at least one, so its exponent never reaches the floor.)

            if (significand == 0)
            {
                Debug.Assert(binaryExponent == 0 && !truncated, "A zero significand cannot have dropped digits.");
                return 0;
            }

            const ulong maxExactSignificand = 1UL << DoubleSignificandBitCount;

            var droppedBitCount = GetBitLength(significand) - DoubleSignificandBitCount;
            if (droppedBitCount > 0)
            {
                var dropped = significand & ((1UL << droppedBitCount) - 1);
                var boundary = 1UL << (droppedBitCount - 1);

                significand >>= droppedBitCount;
                binaryExponent += droppedBitCount;

                if (dropped > boundary || (dropped == boundary && (truncated || (significand & 1) != 0)))
                {
                    significand++;
                    if (significand == maxExactSignificand)
                    {
                        significand >>= 1;
                        binaryExponent++;
                    }
                }
            }

            if (binaryExponent == 0)
            {
                // Nothing was dropped, so the value is an integer below 2^53, which a double can represent exactly.
                return (long)significand;
            }

            // The significand has 53 bits, so the value is 1.f * 2^(binaryExponent + 52).
            Debug.Assert(significand >= 1UL << (DoubleSignificandBitCount - 1), "The significand should have been normalized.");
            var exponentBits = binaryExponent + 1075;
            if (exponentBits >= 2047)
            {
                return double.PositiveInfinity;
            }

            return BitConverter.Int64BitsToDouble((exponentBits << 52) | (long)(significand - (1UL << (DoubleSignificandBitCount - 1))));
        }
    }

    private static ReadOnlySpan<double> ExactPowersOfTen
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new[]
        {
            1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11,
            1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20, 1e21, 1e22,
        };
    }

    private static readonly BigInteger s_twoPow52 = BigInteger.One << (DoubleSignificandBitCount - 1);
    private static readonly BigInteger s_twoPow53 = BigInteger.One << DoubleSignificandBitCount;

    private static double ParseDecimalToDouble(ReadOnlySpan<char> slice, long literalExponent)
    {
        // double.Parse doesn't exactly match the ECMAScript specification (https://tc39.es/ecma262/#sec-literals-numeric-literals).
        // Behavior differs across .NET versions, to varying degrees. On .NET Framework, it doesn't round half-to-even correctly.
        // Even on .NET 9+, it doesn't handle some edge cases correctly (e.g., a tiny significand paired with a huge exponent
        // where the overall magnitude is still normal). See also: https://github.com/adams85/acornima/issues/53

        // The maximum number of decimal digits that is guaranteed to fit in a ulong.
        const int maxAccumulatedDigitCount = 19; // Floor(Log10(Pow(2, sizeof(ulong) * 8)))

        var significand = 0UL;
        var digitCount = 0;
        var exponent = 0L;
        var truncated = false;

        int i, ch;

        // Integer part
        for (i = 0; i < slice.Length; i++)
        {
            ch = slice[i];
            var digitValue = GetDigitValue(ch);
            if (digitValue >= 10)
            {
                if (ch == '_')
                {
                    continue;
                }

                break;
            }

            if (digitCount == 0 && digitValue == 0)
            {
                // A leading zero is not a significant digit. It shifts nothing.
            }
            else if (digitCount < maxAccumulatedDigitCount)
            {
                significand = significand * 10 + digitValue;
                digitCount++;
            }
            else
            {
                truncated |= digitValue != 0;
                exponent++;
            }
        }

        // Fractional part
        if ((uint)i < (uint)slice.Length && slice[i] == '.')
        {
            for (i++; i < slice.Length; i++)
            {
                ch = slice[i];
                var digitValue = GetDigitValue(ch);
                if (digitValue >= 10)
                {
                    if (ch == '_')
                    {
                        continue;
                    }

                    break;
                }

                if (digitCount == 0 && digitValue == 0)
                {
                    exponent--;
                }
                else if (digitCount < maxAccumulatedDigitCount)
                {
                    significand = significand * 10 + digitValue;
                    digitCount++;
                    exponent--;
                }
                else
                {
                    truncated |= digitValue != 0;
                }
            }
        }

        Debug.Assert(i == slice.Length, $"Invalid significand: {slice.ToString()}");

        if (significand == 0)
        {
            Debug.Assert(!truncated, "A zero significand cannot have dropped digits.");
            return 0;
        }

        try { exponent = checked(exponent + literalExponent); }
        catch (OverflowException) { exponent = literalExponent < 0 ? long.MinValue : long.MaxValue; }

        var magnitude =
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            Math.Clamp(exponent, (long)int.MinValue - int.MaxValue, int.MaxValue)
#else
            (exponent > int.MaxValue ? int.MaxValue
            : exponent < (long)int.MinValue - int.MaxValue ? (long)int.MinValue - int.MaxValue
            : exponent)
#endif
            + digitCount;

        if (magnitude > 309)
        {
            return double.PositiveInfinity;
        }

        if (magnitude < -323)
        {
            // Below 10^-324, which is less than half the smallest positive double.
            return 0;
        }

        if (!truncated && TryScaleToDoubleFast(significand, exponent, out var value))
        {
            return value;
        }

        return ParseSlow(slice, literalExponent);

        static bool TryScaleToDoubleFast(ulong significand, long exponent, out double value)
        {
            if (exponent == 0)
            {
                value = UInt64ToDouble(significand);
                return true;
            }

            const ulong maxExactSignificand = 1UL << DoubleSignificandBitCount;

            if (significand > maxExactSignificand)
            {
                value = default;
                return false;
            }

            // 10^0 through 10^22 are the powers of ten a double can represent exactly.
            // Beyond 10^22, the constant itself is rounded, so scaling by it would round twice.
            const int maxExactPowerOfTen = 22;

            Debug.Assert(ExactPowersOfTen.Length > maxExactPowerOfTen);

            if (exponent > 0)
            {
                if (exponent <= maxExactPowerOfTen)
                {
                    value = UInt64ToDouble(significand) * ExactPowersOfTen[(int)exponent];
                    return true;
                }
            }
            else
            {
                if (exponent >= -maxExactPowerOfTen)
                {
                    value = UInt64ToDouble(significand) / ExactPowersOfTen[(int)-exponent];
                    return true;
                }
            }

            value = 0;
            return false;
        }

        static double ParseSlow(ReadOnlySpan<char> slice, long literalExponent)
        {
            // A double's rounding boundary - the midpoint between two adjacent doubles - is a dyadic rational whose
            // decimal expansion never runs past 769 significant digits (the widest sits at the smallest normal,
            // where the midpoint carries 5^1074). Keeping 800 therefore places every boundary inside the digits
            // actually read, so a dropped tail can only push the value strictly past a boundary it already sits on,
            // which is what the truncated flag records.
            const int maxSignificantDigitCount = 800;

            var significand = BigInteger.Zero;
            var chunk = 0UL;
            var chunkDigitCount = 0;
            var digitCount = 0;
            var exponent = literalExponent;
            var truncated = false;
            var inFractionalPart = false;

            int i;
            for (i = 0; i < slice.Length; i++)
            {
                var ch = slice[i];
                var digitValue = GetDigitValue(ch);
                if (digitValue >= 10)
                {
                    if (ch == '_')
                    {
                        continue;
                    }

                    if (ch == '.')
                    {
                        Debug.Assert(!inFractionalPart, $"Invalid significand: {slice.ToString()}");
                        inFractionalPart = true;
                        continue;
                    }

                    Debug.Fail($"Invalid digit in number: U+{(ushort)ch:X4}");
                }

                if (digitCount == 0 && digitValue == 0)
                {
                    if (inFractionalPart)
                    {
                        exponent--;
                    }
                }
                else if (digitCount < maxSignificantDigitCount)
                {
                    chunk = chunk * 10 + digitValue;
                    chunkDigitCount++;
                    if (chunkDigitCount == maxAccumulatedDigitCount)
                    {
                        significand = significand * s_tenPow19 + chunk;
                        chunk = 0;
                        chunkDigitCount = 0;
                    }

                    digitCount++;
                    if (inFractionalPart)
                    {
                        exponent--;
                    }
                }
                else
                {
                    truncated |= digitValue != 0;
                    if (!inFractionalPart)
                    {
                        exponent++;
                    }
                }
            }

            Debug.Assert(i == slice.Length, $"Invalid significand: {slice.ToString()}");

            if (chunkDigitCount > 0)
            {
                significand = significand * BigInteger.Pow(s_ten, chunkDigitCount) + chunk;
            }

            return RoundToDouble(significand, exponent, truncated);
        }

        static double RoundToDouble(BigInteger significand, long exponent, bool truncated)
        {
            BigInteger numerator, denominator;
            if (exponent >= 0)
            {
                numerator = significand * BigInteger.Pow(s_ten, (int)exponent);
                denominator = BigInteger.One;
            }
            else
            {
                numerator = significand;
                denominator = BigInteger.Pow(s_ten, (int)-exponent);
            }

            // The exponent of the smallest positive double (2^-1074). Nothing rounds below it.
            const int minBinaryExponent = -1074;

            // Aim the quotient at exactly 53 significant bits. The two loops below absorb the single bit of
            // error a bit-count estimate can be off by.

            var binaryExponent = (int)(GetBitLength(numerator) - GetBitLength(denominator)) - DoubleSignificandBitCount;
            if (binaryExponent < minBinaryExponent)
            {
                binaryExponent = minBinaryExponent;
            }

            DivideScaled(numerator, denominator, binaryExponent, out var quotient, out var remainder, out var scaledDenominator);
            while (quotient >= s_twoPow53)
            {
                binaryExponent++;
                DivideScaled(numerator, denominator, binaryExponent, out quotient, out remainder, out scaledDenominator);
            }

            while (quotient < s_twoPow52 && binaryExponent > minBinaryExponent)
            {
                binaryExponent--;
                DivideScaled(numerator, denominator, binaryExponent, out quotient, out remainder, out scaledDenominator);
            }

            var comparison = (remainder << 1).CompareTo(scaledDenominator);
            if (comparison > 0 || (comparison == 0 && (truncated || !quotient.IsEven)))
            {
                quotient += BigInteger.One;
                if (quotient >= s_twoPow53)
                {
                    quotient >>= 1;
                    binaryExponent++;
                }
            }

            return ComposeDouble(quotient, binaryExponent);
        }

        static void DivideScaled(
            BigInteger numerator,
            BigInteger denominator,
            int binaryExponent,
            out BigInteger quotient,
            out BigInteger remainder,
            out BigInteger scaledDenominator)
        {
            if (binaryExponent >= 0)
            {
                scaledDenominator = denominator << binaryExponent;
                quotient = BigInteger.DivRem(numerator, scaledDenominator, out remainder);
            }
            else
            {
                scaledDenominator = denominator;
                quotient = BigInteger.DivRem(numerator << -binaryExponent, denominator, out remainder);
            }
        }

        static double ComposeDouble(BigInteger significand, int binaryExponent)
        {
            if (significand.IsZero)
            {
                return 0;
            }

            if (significand < s_twoPow52)
            {
                // Only reachable at the exponent floor, where the significand is the whole encoding.
                return BitConverter.Int64BitsToDouble((long)significand);
            }

            var exponentBits = binaryExponent + 1075;
            if (exponentBits >= 2047)
            {
                return double.PositiveInfinity;
            }

            return BitConverter.Int64BitsToDouble(((long)exponentBits << 52) | (long)(significand - s_twoPow52));
        }
    }

    private static int GetBitLength(ulong value)
    {
#if NETCOREAPP3_0_OR_GREATER
        return 64 - BitOperations.LeadingZeroCount(value);
#else
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        var span = MemoryMarshal.CreateSpan(ref value, 1);
#else
        Span<ulong> span = stackalloc[] { value };
#endif

        var bitLength = GetBitLength(MemoryMarshal.AsBytes(span), BitConverter.IsLittleEndian);
        Debug.Assert(bitLength <= 64);
        return (int)bitLength;
#endif
    }

#if NET5_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static long GetBitLength(BigInteger value)
    {
        Debug.Assert(value.Sign >= 0, "Only non-negative values are expected.");

#if NET5_0_OR_GREATER
        return value.GetBitLength();
#else
        if (value.IsZero)
        {
            return 0;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        var byteCount = value.GetByteCount();
        byte[]? byteArray = null;
        try
        {
            var bytes = byteCount <= 64
                ? stackalloc byte[byteCount]
                : (byteArray = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            var success = value.TryWriteBytes(bytes, out var bytesWritten);
            Debug.Assert(success && bytesWritten == byteCount);
            return GetBitLength(bytes, isLittleEndian: true);
        }
        finally
        {
            if (byteArray is not null)
            {
                ArrayPool<byte>.Shared.Return(byteArray);
            }
        }
#else
        return GetBitLength(value.ToByteArray(), isLittleEndian: true);
#endif
#endif
    }

#if !NET5_0_OR_GREATER
    private static long GetBitLength(ReadOnlySpan<byte> bytes, bool isLittleEndian)
    {
        long bitLength;
        int index;

        if (isLittleEndian)
        {
            for (index = bytes.Length - 1; index > 0 && bytes[index] == 0; index--) { }
            bitLength = (long)index << 3;
        }
        else
        {
            var endIndex = bytes.Length - 1;
            for (index = 0; index < endIndex && bytes[index] == 0; index++) { }
            bitLength = (long)(endIndex - index) << 3;
        }

        for (int msb = bytes[index]; msb != 0; msb >>= 1)
        {
            bitLength++;
        }

        return bitLength;
    }
#endif

    #endregion
}
