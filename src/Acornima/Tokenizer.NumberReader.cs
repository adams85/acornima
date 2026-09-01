using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Acornima;

// Reading a numeric literal to the nearest double.
//
// ECMA-262 makes a NumericLiteral's value the mathematical value it denotes rounded to the nearest Number
// (https://tc39.es/ecma262/#sec-literals-numeric-literals), which is one rounding, not a chain of them, and
// which may not depend on the runtime the parser happens to be loaded into. Neither the conversions the
// runtime offers nor a digit-at-a-time accumulation gives that (see https://github.com/adams85/acornima/issues/53),
// so everything the tokenizer turns into a double is rounded here instead:
//
// * ulong -> double is rounded twice by every runtime before .NET 9 when the operand has its high bit set,
//   which is what UInt64ToDouble works around,
// * double.Parse is not correctly rounded on .NET Framework at all, which is what ParseFloatToDouble
//   replaces,
// * and accumulating digits in a double rounds once per digit on every runtime, which is what
//   ParseIntToDouble replaces.
//
// The algorithms are ports of the ones Jint carried as a workaround in the meantime
// (sebastienros/jint#3530, #3533, #3536), where they run under the full test262 suite.
public partial class Tokenizer
{
    // The number of bits a double's significand carries.
    private const int SignificandBitCount = 53;

    // The exponent of the smallest positive double, 2^-1074: nothing rounds below it.
    private const int MinBinaryExponent = -1074;

    // 19 decimal digits are the most that always fit a ulong (10^19 - 1 < 2^64 - 1).
    private const int MaxAccumulatedDigitCount = 19;

    // 10^0..10^22 are the powers of ten a double holds exactly; past 10^22 the constant itself is rounded
    // and scaling by it would round twice.
    private const int MaxExactPowerOfTen = 22;

    // A double's rounding boundary - the midpoint between two adjacent doubles - is a dyadic rational whose
    // decimal expansion never runs past 769 significant digits (the widest sits at the smallest normal,
    // where the midpoint carries 5^1074). Keeping 800 therefore places every boundary inside the digits
    // actually read, so a dropped tail can only push the value strictly past a boundary it already sits on,
    // which is what the truncated flag records.
    private const int MaxSignificantDigitCount = 800;

    private const ulong MaxExactSignificand = 1UL << SignificandBitCount;

    // An exponent part this large has already decided the answer: a literal is at most int.MaxValue
    // characters long, so no run of digits can shift the magnitude back into range, and the digits left
    // over only make an infinity more infinite. Stopping here also keeps the accumulator from overflowing.
    private const long MaxScannedExponent = 100000000000000000; // 10^17

    private static readonly double[] s_exactPowersOfTen =
    {
        1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11,
        1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20, 1e21, 1e22,
    };

    private static readonly BigInteger s_ten = new BigInteger(10);
    private static readonly BigInteger s_tenPow19 = new BigInteger(10000000000000000000UL);
    private static readonly BigInteger s_two52 = BigInteger.One << (SignificandBitCount - 1);
    private static readonly BigInteger s_two53 = BigInteger.One << SignificandBitCount;

    /// <summary>
    /// Converts an unsigned 64-bit integer to the nearest <see cref="double"/>, alike on every runtime.
    /// </summary>
    /// <remarks>
    /// No runtime before .NET 9 rounds the unsigned conversion once when the operand has its high bit set:
    /// x64 has no single instruction for it below AVX-512DQ, and the lowering the JIT open-codes instead
    /// rounds twice. The signed conversion is correctly rounded everywhere, so halve the operand to reach
    /// it - OR-ing the bit that falls off back in, so a tie still reads as a tie - and double the result,
    /// which is exact.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double UInt64ToDouble(ulong value)
    {
        if (value < 1UL << 63)
        {
            return (long)value;
        }

        return (double)(long)((value >> 1) | (value & 1)) * 2.0;
    }

    /// <summary>
    /// Returns the <see cref="double"/> nearest the integer that the radix-<paramref name="radix"/> digits
    /// of <paramref name="slice"/> denote.
    /// </summary>
    /// <remarks>
    /// Every radix a numeric literal can be written in is a power of two, so the digits lay straight onto
    /// the bits of the value: the leading digits are the significand, and whether anything non-zero was
    /// dropped past them is the only other thing the rounding can need. That keeps this a single pass with
    /// no big-integer arithmetic, however long the digit run is.
    /// </remarks>
    private static double ParseIntToDouble(ReadOnlySpan<char> slice, byte radix)
    {
        Debug.Assert(radix is 2 or 8 or 16, $"Unexpected radix: {radix}");

        var bitsPerDigit = radix switch
        {
            2 => 1,
            8 => 3,
            _ => 4
        };

        // The largest accumulator that another digit still fits into.
        var significandCeiling = ulong.MaxValue >> bitsPerDigit;

        ulong significand = 0;
        long binaryExponent = 0;
        var truncated = false;

        foreach (var ch in slice)
        {
            if (ch == '_')
            {
                continue;
            }

            var digitValue = GetDigitValue(ch);
            Debug.Assert(digitValue < radix, $"Invalid digit in number: U+{(ushort)ch:X4}");

            if (significand <= significandCeiling)
            {
                significand = (significand << bitsPerDigit) | digitValue;
            }
            else
            {
                // The accumulator holds at least 61 bits by now - well past the 53 the answer keeps and the
                // one more that decides which way it rounds - so the rest of the digits only need to be
                // remembered as "something non-zero was dropped".
                binaryExponent += bitsPerDigit;
                truncated |= digitValue != 0;
            }
        }

        return ScaleToDouble(significand, binaryExponent, truncated);
    }

    /// <summary>
    /// Returns the <see cref="double"/> nearest the value that the decimal literal <paramref name="slice"/>
    /// denotes.
    /// </summary>
    /// <remarks>
    /// .NET Framework's <c>double.Parse</c> is not correctly rounded, and lands one ULP off from a single
    /// significant digit onwards once an exponent takes the value out of the exactly representable range,
    /// so there is no digit count below which handing the text to the runtime would be safe. Reading the
    /// digits here settles the common shapes - an integer below 2^64, or a significand below 2^53 scaled by
    /// a power of ten a double holds exactly - in one floating-point operation on two exact operands, and
    /// only the shapes that cannot be settled that way pay for exact arithmetic.
    /// </remarks>
    private static double ParseFloatToDouble(ReadOnlySpan<char> slice)
    {
        var length = slice.Length;
        var i = 0;

        ulong significand = 0;
        var digitCount = 0;
        long exponent = 0;
        var truncated = false;

        // Integer part.
        while (i < length)
        {
            var digitValue = (uint)(slice[i] - '0');
            if (digitValue > 9)
            {
                if (slice[i] != '_')
                {
                    break;
                }

                i++;
                continue;
            }

            if (digitCount == 0 && digitValue == 0)
            {
                // A leading zero is not a significant digit and shifts nothing.
            }
            else if (digitCount < MaxAccumulatedDigitCount)
            {
                significand = significand * 10 + digitValue;
                digitCount++;
            }
            else
            {
                exponent++;
                truncated |= digitValue != 0;
            }

            i++;
        }

        // Fractional part.
        if (i < length && slice[i] == '.')
        {
            i++;
            while (i < length)
            {
                var digitValue = (uint)(slice[i] - '0');
                if (digitValue > 9)
                {
                    if (slice[i] != '_')
                    {
                        break;
                    }

                    i++;
                    continue;
                }

                if (digitCount == 0 && digitValue == 0)
                {
                    exponent--;
                }
                else if (digitCount < MaxAccumulatedDigitCount)
                {
                    significand = significand * 10 + digitValue;
                    digitCount++;
                    exponent--;
                }
                else
                {
                    truncated |= digitValue != 0;
                }

                i++;
            }
        }

        var significandEnd = i;

        // Exponent part.
        long literalExponent = 0;
        if (i < length)
        {
            Debug.Assert((slice[i] | 0x20) == 'e', $"Invalid number: {slice.ToString()}");
            i++;

            var exponentIsNegative = i < length && slice[i] == '-';
            if (i < length && (exponentIsNegative || slice[i] == '+'))
            {
                i++;
            }

            long scannedExponent = 0;
            for (; i < length; i++)
            {
                var digitValue = (uint)(slice[i] - '0');
                if (digitValue > 9)
                {
                    Debug.Assert(slice[i] == '_', $"Invalid number: {slice.ToString()}");
                    continue;
                }

                if (scannedExponent < MaxScannedExponent)
                {
                    scannedExponent = scannedExponent * 10 + digitValue;
                }
            }

            literalExponent = exponentIsNegative ? -scannedExponent : scannedExponent;
            exponent += literalExponent;
        }

        if (significand == 0)
        {
            // Every digit read was a zero, whatever the exponent says.
            return 0;
        }

        // 10^(magnitude - 1) <= value < 10^magnitude. The two passes over the digits keep a different number
        // of significant digits, and this is the figure both of them agree on.
        var magnitude = exponent + digitCount;
        if (magnitude > 309)
        {
            return double.PositiveInfinity;
        }

        if (magnitude < -323)
        {
            // Below 10^-324, which is less than half the smallest positive double.
            return 0;
        }

        if (!truncated && TryScaleExactly(significand, exponent, out var value))
        {
            return value;
        }

        return ParseSignificandToDouble(slice.Slice(0, significandEnd), literalExponent);
    }

    /// <summary>
    /// Settles the cases that one floating-point operation on two exact operands answers, and which are
    /// therefore correctly rounded: an integer, or an exact significand scaled by a power of ten a double
    /// holds exactly.
    /// </summary>
    private static bool TryScaleExactly(ulong significand, long exponent, out double value)
    {
        if (exponent == 0)
        {
            value = UInt64ToDouble(significand);
            return true;
        }

        if (significand > MaxExactSignificand)
        {
            value = 0;
            return false;
        }

        if (exponent > 0)
        {
            if (exponent <= MaxExactPowerOfTen)
            {
                value = UInt64ToDouble(significand) * s_exactPowersOfTen[(int)exponent];
                return true;
            }
        }
        else if (exponent >= -MaxExactPowerOfTen)
        {
            value = UInt64ToDouble(significand) / s_exactPowersOfTen[(int)-exponent];
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>
    /// Re-reads the significand at full width and rounds the exact decimal value it denotes once.
    /// </summary>
    /// <param name="slice">The digits and at most one decimal point, without an exponent part.</param>
    /// <param name="literalExponent">The value of the literal's own exponent part, if it had one.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ParseSignificandToDouble(ReadOnlySpan<char> slice, long literalExponent)
    {
        var value = BigInteger.Zero;
        ulong chunk = 0;
        var chunkDigitCount = 0;
        var digitCount = 0;
        var exponent = literalExponent;
        var truncated = false;
        var afterPoint = false;

        foreach (var ch in slice)
        {
            if (ch == '_')
            {
                continue;
            }

            if (ch == '.')
            {
                afterPoint = true;
                continue;
            }

            var digitValue = (uint)(ch - '0');
            Debug.Assert(digitValue <= 9, $"Invalid digit in number: U+{(ushort)ch:X4}");

            if (digitCount == 0 && digitValue == 0)
            {
                if (afterPoint)
                {
                    exponent--;
                }
            }
            else if (digitCount < MaxSignificantDigitCount)
            {
                chunk = chunk * 10 + digitValue;
                chunkDigitCount++;
                if (chunkDigitCount == MaxAccumulatedDigitCount)
                {
                    value = value * s_tenPow19 + chunk;
                    chunk = 0;
                    chunkDigitCount = 0;
                }

                digitCount++;
                if (afterPoint)
                {
                    exponent--;
                }
            }
            else
            {
                truncated |= digitValue != 0;
                if (!afterPoint)
                {
                    exponent++;
                }
            }
        }

        if (chunkDigitCount > 0)
        {
            value = value * BigInteger.Pow(s_ten, chunkDigitCount) + chunk;
        }

        return RoundToDouble(value, exponent, truncated);
    }

    /// <summary>
    /// Returns the <see cref="double"/> nearest <paramref name="significand"/> * 10^<paramref name="exponent"/>,
    /// ties to even unless <paramref name="truncated"/> says the true value sits strictly above the boundary.
    /// </summary>
    private static double RoundToDouble(BigInteger significand, long exponent, bool truncated)
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

        // Aim the quotient straight at 53 significant bits; the two loops below absorb the one place a
        // bit count estimate can be out by.
        var binaryExponent = (int)(GetBitLength(numerator) - GetBitLength(denominator)) - SignificandBitCount;
        if (binaryExponent < MinBinaryExponent)
        {
            binaryExponent = MinBinaryExponent;
        }

        DivideScaled(numerator, denominator, binaryExponent, out var quotient, out var remainder, out var scaledDenominator);
        while (quotient >= s_two53)
        {
            binaryExponent++;
            DivideScaled(numerator, denominator, binaryExponent, out quotient, out remainder, out scaledDenominator);
        }

        while (quotient < s_two52 && binaryExponent > MinBinaryExponent)
        {
            binaryExponent--;
            DivideScaled(numerator, denominator, binaryExponent, out quotient, out remainder, out scaledDenominator);
        }

        var comparison = (remainder << 1).CompareTo(scaledDenominator);
        if (comparison > 0 || (comparison == 0 && (truncated || !quotient.IsEven)))
        {
            quotient += BigInteger.One;
            if (quotient >= s_two53)
            {
                quotient >>= 1;
                binaryExponent++;
            }
        }

        return ComposeDouble(quotient, binaryExponent);
    }

    private static void DivideScaled(
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

    /// <summary>
    /// Returns the <see cref="double"/> nearest <paramref name="significand"/> * 2^<paramref name="binaryExponent"/>,
    /// ties to even unless <paramref name="truncated"/> says the true value sits strictly above the boundary.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="ComposeDouble"/> this has no subnormal case to serve: an integer read from digits
    /// is either zero or at least one, so its exponent never reaches the floor.
    /// </remarks>
    private static double ScaleToDouble(ulong significand, long binaryExponent, bool truncated)
    {
        if (significand == 0)
        {
            Debug.Assert(binaryExponent == 0 && !truncated, "A zero significand cannot have dropped digits");
            return 0;
        }

        var bitCount = GetBitLength(significand);
        if (bitCount > SignificandBitCount)
        {
            var droppedBitCount = bitCount - SignificandBitCount;
            var dropped = significand & ((1UL << droppedBitCount) - 1);
            var boundary = 1UL << (droppedBitCount - 1);

            significand >>= droppedBitCount;
            binaryExponent += droppedBitCount;

            if (dropped > boundary || (dropped == boundary && (truncated || (significand & 1) != 0)))
            {
                significand++;
                if (significand == MaxExactSignificand)
                {
                    significand >>= 1;
                    binaryExponent++;
                }
            }
        }

        if (binaryExponent == 0)
        {
            // Nothing was dropped, so the value is an integer below 2^53, which a double holds exactly.
            return (long)significand;
        }

        // The significand carries 53 bits, so the value is 1.f * 2^(binaryExponent + 52).
        Debug.Assert(significand >= 1UL << (SignificandBitCount - 1), "The significand should have been normalized");
        var exponentBits = binaryExponent + 1075;
        if (exponentBits >= 2047)
        {
            return double.PositiveInfinity;
        }

        return BitConverter.Int64BitsToDouble((exponentBits << 52) | (long)(significand - (1UL << (SignificandBitCount - 1))));
    }

    /// <summary>
    /// Builds the <see cref="double"/> holding <paramref name="significand"/> * 2^<paramref name="binaryExponent"/>,
    /// saturating to an infinity rather than wrapping.
    /// </summary>
    private static double ComposeDouble(BigInteger significand, int binaryExponent)
    {
        if (significand.IsZero)
        {
            return 0;
        }

        if (significand < s_two52)
        {
            // Only reachable at the exponent floor, where the significand is the whole encoding.
            return BitConverter.Int64BitsToDouble((long)significand);
        }

        var exponentBits = binaryExponent + 1075;
        if (exponentBits >= 2047)
        {
            return double.PositiveInfinity;
        }

        return BitConverter.Int64BitsToDouble(((long)exponentBits << 52) | (long)(significand - s_two52));
    }

    private static int GetBitLength(ulong value)
    {
        var bitCount = 0;
        while (value != 0)
        {
            bitCount++;
            value >>= 1;
        }
        return bitCount;
    }

    private static long GetBitLength(BigInteger value)
    {
        Debug.Assert(value.Sign >= 0, "Only non-negative values are expected");

#if NET5_0_OR_GREATER
        return value.GetBitLength();
#else
        if (value.IsZero)
        {
            return 0;
        }

        var bytes = value.ToByteArray();
        var index = bytes.Length - 1;
        while (index > 0 && bytes[index] == 0)
        {
            index--;
        }

        long bitCount = index * 8;
        int top = bytes[index];
        while (top != 0)
        {
            bitCount++;
            top >>= 1;
        }
        return bitCount;
#endif
    }
}
