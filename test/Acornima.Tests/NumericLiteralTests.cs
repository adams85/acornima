using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using Xunit;

namespace Acornima.Tests;

/// <summary>
/// Checks that a numeric literal scans to the <see cref="double"/> nearest the value it denotes, which is
/// what ECMA-262 asks for (https://tc39.es/ecma262/#sec-literals-numeric-literals) and what
/// https://github.com/adams85/acornima/issues/53 reported that the three number reading branches of the
/// tokenizer did not do.
/// </summary>
/// <remarks>
/// The expected value never comes from the runtime's own parsing or conversions - those are the things under
/// test, and .NET Framework gets both of them wrong - but from an exact oracle: the literal is turned into
/// the rational number it denotes, and the nearest double is found by searching the bit patterns.
/// </remarks>
public class NumericLiteralTests
{
    private const int LiteralCountPerShape = 2000;

    #region The values named in the issue

    [Theory]
    // All of these are 12345678901234567890, whose nearest double is 12345678901234567168.
    [InlineData("12345678901234567890")]
    [InlineData("0xAB54A98CEB1F0AD2")]
    [InlineData("0o1255245230635307605322")]
    [InlineData("0b1010101101010100101010011000110011101011000111110000101011010010")]
    [InlineData("12_345_678_901_234_567_890")]
    [InlineData("01255245230635307605322")]
    [InlineData("12345678901234567890.0")]
    public void ScansEverySpellingOfTheSameIntegerAlike(string literal)
    {
        Assert.Equal(0x43E56A95319D63E1, BitConverter.DoubleToInt64Bits(Scan(literal)));
    }

    [Theory]
    // The smallest integer whose ulong -> double conversion is rounded twice before .NET 9. It sits 1025
    // above 2^63, where the doubles are 2048 apart, so it rounds up.
    [InlineData("9223372036854776833", 9223372036854777856d)]
    // Past the ulong accumulator, so the digits used to be rebuilt one rounding at a time.
    [InlineData("0x1F49E9EE4C1BCE961", 36073444770624368640d)]
    [InlineData("0xffffffffffffffff", 18446744073709551616d)]
    public void ScansTheValueTheIssueNames(string literal, double expected)
    {
        Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(Scan(literal)));
    }

    #endregion

    #region The same value in every radix

    [Theory]
    [InlineData("8000000000000401")] // 2^63 + 1025, the smallest operand the ulong conversion differs on
    [InlineData("FFFFFFFFFFFFFFFF")] // 2^64 - 1, the last one the accumulator holds
    [InlineData("1F49E9EE4C1BCE961")] // 36073444770624366945, from the issue
    [InlineData("1FFFFFFFFFFFFFFFFF")] // 2^69 - 1, which needs 23 octal digits
    [InlineData("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")] // 2^120 - 1
    [InlineData("100000000000000800000000000001")] // a significand that ends just above a rounding boundary
    public void ScansTheSameValueInEveryRadix(string hexDigits)
    {
        var value = BigInteger.Parse("0" + hexDigits, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var expected = NearestDouble(value, BigInteger.One);

        var literals = new[]
        {
            value.ToString(CultureInfo.InvariantCulture),
            "0x" + hexDigits,
            "0o" + ToRadix(value, 8),
            "0b" + ToRadix(value, 2),
            "0" + ToRadix(value, 8) // legacy octal
        };

        foreach (var literal in literals)
        {
            Assert.Equal(Bits(expected), Bits(Scan(literal)));
        }
    }

    #endregion

    #region Single literals worth naming

    [Theory]
    [InlineData("0")]
    [InlineData("0.")]
    [InlineData("0.0")]
    [InlineData("0e100")]
    [InlineData("1")]
    [InlineData("10.")]
    [InlineData("0.1")]
    [InlineData("1.5")]
    [InlineData("1e21")]
    [InlineData("1e-7")]
    [InlineData(".5")]
    [InlineData("08")] // a decimal with a leading zero
    [InlineData("08.125e2")]
    [InlineData("0123")] // legacy octal
    [InlineData("9007199254740993")] // 2^53 + 1, a tie that goes to even
    [InlineData("9007199254740995")] // 2^53 + 3, a tie that goes up
    [InlineData("4503599627370497.5")] // a tie at the top of the exactly representable range
    [InlineData("1.7976931348623157e308")] // double.MaxValue
    [InlineData("1.7976931348623158e308")] // just short of the overflow boundary
    [InlineData("1.7976931348623159e308")] // past it
    [InlineData("1e309")]
    [InlineData("2.2250738585072011e-308")] // the value that used to hang strtod
    [InlineData("2.2250738585072014e-308")] // the smallest normal
    [InlineData("1e-320")]
    [InlineData("4.9e-324")] // the smallest subnormal
    [InlineData("2.4703282292062327e-324")] // half the smallest subnormal, rounds to zero
    [InlineData("2.4703282292062328e-324")] // a hair past it, rounds up
    [InlineData("1e-400")]
    [InlineData("7.8459735791271921e65")] // a 17-digit significand that needs the exact path
    [InlineData("3.518437208883201171875e13")] // a 22-digit significand
    [InlineData("1234567890123456789012345678901234567890")]
    [InlineData("0.000000000000000000000000000000000000000000001234567890123456789")]
    [InlineData("1_0.5_0e1_0")]
    public void ScansTheNearestDouble(string literal)
    {
        var (numerator, denominator) = ExactValueOf(literal);
        Assert.Equal(Bits(NearestDouble(numerator, denominator)), Bits(Scan(literal)));
    }

    [Theory]
    [InlineData("1e1000000", double.PositiveInfinity)]
    [InlineData("1e-1000000", 0d)]
    [InlineData("1e100000000000000000000", double.PositiveInfinity)]
    [InlineData("1e-100000000000000000000", 0d)]
    [InlineData("0e100000000000000000000", 0d)]
    public void ScansAnExponentTooLargeToMatter(string literal, double expected)
    {
        Assert.Equal(Bits(expected), Bits(Scan(literal)));
    }

    #endregion

    #region Rounding boundaries

    [Theory]
    [InlineData(1.0)]
    [InlineData(12345.678)]
    [InlineData(1e-300)]
    [InlineData(4.9e-324)] // the smallest subnormal
    [InlineData(2.2250738585072014e-308)] // the smallest normal, where the widest boundary sits
    [InlineData(1.7976931348623157e308)] // the largest double, whose upper boundary is the overflow one
    public void ScansTheNearestDoubleAroundARoundingBoundary(double value)
    {
        var boundary = ExactDecimalOfBoundaryAbove(BitConverter.DoubleToInt64Bits(value));

        var literals = new[]
        {
            boundary, // exactly on the boundary, so the tie goes to even
            boundary + "1", // a hair above it
            // Also above it, but only past the significant digits the reader keeps, so what says so is the
            // flag that records having dropped something non-zero.
            boundary + new string('0', 200) + "1"
        };

        foreach (var literal in literals)
        {
            var (numerator, denominator) = ExactValueOf(literal);
            Assert.Equal(Bits(NearestDouble(numerator, denominator)), Bits(Scan(literal)));
        }
    }

    [Fact]
    public void ScansTheNearestDoubleForASignificandLongerThanTheWindow()
    {
        var random = new Random(13);
        var failureCount = 0;

        for (var i = 0; i < 200; i++)
        {
            // 1200 significant digits is well past both the 800 the reader keeps and the 769 the widest
            // rounding boundary needs, so everything dropped can only be a sticky bit.
            var literal = DecimalDigits(random, 1200) + "e-1200";
            var (numerator, denominator) = ExactValueOf(literal);

            if (Bits(Scan(literal)) != Bits(NearestDouble(numerator, denominator)))
            {
                failureCount++;
            }
        }

        Assert.Equal(0, failureCount);
    }

    #endregion

    #region Every literal shape, over a generated corpus

    [Theory]
    [InlineData("integer-22", 1)] // past the ulong accumulator, so double.Parse used to answer
    [InlineData("fraction-18", 2)]
    [InlineData("exponent+200", 3)]
    [InlineData("exponent-200", 4)]
    [InlineData("exponent-subnormal", 5)]
    [InlineData("decimal-uint64-top", 6)] // in [2^63, 2^64), so the ulong conversion used to answer
    [InlineData("hex-uint64-top", 7)]
    [InlineData("hex-17", 8)] // past the accumulator, so the digits used to be rebuilt in a double
    [InlineData("hex-20", 9)]
    [InlineData("binary-70", 10)]
    [InlineData("legacy-octal-24", 11)]
    public void ScansTheNearestDoubleForEveryLiteralOfShape(string shape, int seed)
    {
        var random = new Random(seed);
        var failureCount = 0;
        var examples = new List<string>();

        for (var i = 0; i < LiteralCountPerShape; i++)
        {
            var literal = GenerateLiteral(shape, random);
            var (numerator, denominator) = ExactValueOf(literal);
            var expected = NearestDouble(numerator, denominator);
            var actual = Scan(literal);

            if (Bits(actual) != Bits(expected))
            {
                failureCount++;
                if (examples.Count < 5)
                {
                    examples.Add($"{literal} scanned as {Bits(actual)}, nearest is {Bits(expected)}");
                }
            }
        }

        Assert.True(failureCount == 0,
            $"{failureCount} of {LiteralCountPerShape} {shape} literals are not the nearest double: {string.Join("; ", examples)}");
    }

    #endregion

    #region The unsigned conversion on its own

    [Fact]
    public void ConvertsEveryUInt64ToTheNearestDouble()
    {
        const int OperandCount = 20000;

        var random = new Random(12);
        var failureCount = 0;

        for (var i = 0; i < OperandCount; i++)
        {
            // The affected octave: below 2^63 the JIT reaches the signed conversion, which every runtime
            // rounds correctly.
            var value = NextUInt64(random) | (1UL << 63);

            if (Bits(Tokenizer.UInt64ToDouble(value)) != Bits(NearestDouble(new BigInteger(value), BigInteger.One)))
            {
                failureCount++;
            }
        }

        Assert.True(failureCount == 0, $"{failureCount} of {OperandCount} operands in [2^63, 2^64) do not convert to the nearest double");
    }

    [Fact]
    public void ConvertsSmallUInt64ValuesExactly()
    {
        for (var value = 0UL; value < 1000; value++)
        {
            Assert.Equal((double)(long)value, Tokenizer.UInt64ToDouble(value));
        }

        Assert.Equal(9223372036854775808d, Tokenizer.UInt64ToDouble(1UL << 63));
        Assert.Equal(18446744073709551616d, Tokenizer.UInt64ToDouble(ulong.MaxValue));
    }

    #endregion

    #region Scanning

    private static double Scan(string literal)
    {
        var tokenizer = new Tokenizer(literal);
        var token = tokenizer.GetToken();

        Assert.Equal(TokenKind.NumericLiteral, token.Kind);
        Assert.Equal(literal.Length, token.End);

        return token.NumericValue!.Value;
    }

    private static string Bits(double value) => BitConverter.DoubleToInt64Bits(value).ToString("X16", CultureInfo.InvariantCulture);

    #endregion

    #region The oracle

    // The bit pattern of an infinity, which is also the exclusive upper bound of the finite ones.
    private const long InfinityBits = 0x7FF0000000000000;

    // The value an infinity would have if the exponent range went on: 2^1024, scaled like every other one.
    private static readonly BigInteger s_infinityScaledValue = BigInteger.One << (1024 + 1074);

    /// <summary>
    /// The <see cref="double"/> nearest <paramref name="numerator"/> / <paramref name="denominator"/>, ties
    /// to even, found without asking the runtime to parse or convert anything.
    /// </summary>
    private static double NearestDouble(BigInteger numerator, BigInteger denominator)
    {
        Assert.True(numerator.Sign >= 0 && denominator.Sign > 0);

        // Every finite non-negative double is an integer multiple of 2^-1074 and grows monotonically with
        // its bit pattern, so scaling the value by 2^1074 turns "which double is it between" into a search
        // over integers. The remainder of that scaling is what decides a tie.
        var scaledValue = BigInteger.DivRem(numerator << 1074, denominator, out var remainder);

        if (scaledValue >= s_infinityScaledValue)
        {
            return double.PositiveInfinity;
        }

        long low = 0, high = InfinityBits;
        while (high - low > 1)
        {
            var middle = low + ((high - low) >> 1);
            if (ScaledValueOf(middle) <= scaledValue)
            {
                low = middle;
            }
            else
            {
                high = middle;
            }
        }

        var lowDistance = ((scaledValue - ScaledValueOf(low)) * denominator) + remainder;
        var highDistance = ((ScaledValueOf(high) - scaledValue) * denominator) - remainder;

        var comparison = lowDistance.CompareTo(highDistance);
        return BitConverter.Int64BitsToDouble(comparison < 0 || (comparison == 0 && (low & 1) == 0) ? low : high);
    }

    /// <summary>
    /// The exact value of the <see cref="double"/> with the given bit pattern, times 2^1074.
    /// </summary>
    private static BigInteger ScaledValueOf(long bits)
    {
        if (bits == InfinityBits)
        {
            return s_infinityScaledValue;
        }

        var exponentBits = bits >> 52;
        var significandBits = bits & 0xFFFFFFFFFFFFF;

        return exponentBits == 0
            ? new BigInteger(significandBits)
            : new BigInteger((1L << 52) | significandBits) << (int)(exponentBits - 1);
    }

    /// <summary>
    /// The rational number a numeric literal denotes, read without any floating-point arithmetic.
    /// </summary>
    private static (BigInteger Numerator, BigInteger Denominator) ExactValueOf(string literal)
    {
        var text = literal.Replace("_", "");
        var radix = 10;

        if (text.Length > 1 && text[0] == '0')
        {
            switch (text[1] | 0x20)
            {
                case 'x':
                    radix = 16;
                    text = text.Substring(2);
                    break;
                case 'o':
                    radix = 8;
                    text = text.Substring(2);
                    break;
                case 'b':
                    radix = 2;
                    text = text.Substring(2);
                    break;
                default:
                    if (text.IndexOfAny(new[] { '.', 'e', 'E', '8', '9' }) < 0)
                    {
                        radix = 8; // legacy octal
                        text = text.Substring(1);
                    }
                    break;
            }
        }

        if (radix != 10)
        {
            var value = BigInteger.Zero;
            foreach (var ch in text)
            {
                value = (value * radix) + DigitValueOf(ch);
            }
            return (value, BigInteger.One);
        }

        var exponent = 0;
        var exponentIndex = text.IndexOfAny(new[] { 'e', 'E' });
        if (exponentIndex >= 0)
        {
            exponent = int.Parse(text.Substring(exponentIndex + 1), CultureInfo.InvariantCulture);
            text = text.Substring(0, exponentIndex);
        }

        var pointIndex = text.IndexOf('.');
        if (pointIndex >= 0)
        {
            exponent -= text.Length - pointIndex - 1;
            text = text.Remove(pointIndex, 1);
        }

        var significand = text.Length > 0 ? BigInteger.Parse(text, CultureInfo.InvariantCulture) : BigInteger.Zero;

        return exponent >= 0
            ? (significand * BigInteger.Pow(10, exponent), BigInteger.One)
            : (significand, BigInteger.Pow(10, -exponent));
    }

    private static int DigitValueOf(char ch) => ch <= '9' ? ch - '0' : ((ch | 0x20) - 'a') + 10;

    /// <summary>
    /// The midpoint between the <see cref="double"/> with the given bit pattern and the next one up, written
    /// out exactly. On the 2^-1075 grid that midpoint is an integer, so multiplying by 5^1075 turns it into
    /// a terminating decimal.
    /// </summary>
    private static string ExactDecimalOfBoundaryAbove(long bits)
    {
        var doubledMidpoint = ScaledValueOf(bits) + ScaledValueOf(bits + 1);
        var digits = (doubledMidpoint * BigInteger.Pow(5, 1075)).ToString(CultureInfo.InvariantCulture);

        if (digits.Length <= 1075)
        {
            digits = digits.PadLeft(1076, '0');
        }

        var integerPart = digits.Substring(0, digits.Length - 1075);
        var fractionPart = digits.Substring(digits.Length - 1075).TrimEnd('0');

        return integerPart + "." + fractionPart;
    }

    private static string ToRadix(BigInteger value, int radix)
    {
        const string Digits = "0123456789abcdef";

        if (value.IsZero)
        {
            return "0";
        }

        var text = new StringBuilder();
        while (!value.IsZero)
        {
            text.Insert(0, Digits[(int)(value % radix)]);
            value /= radix;
        }
        return text.ToString();
    }

    #endregion

    #region Generating a literal of a given shape

    private static string GenerateLiteral(string shape, Random random)
    {
        switch (shape)
        {
            case "integer-22":
                return DecimalDigits(random, 22);

            case "fraction-18":
                var pointIndex = random.Next(1, 18);
                var digits = DecimalDigits(random, 18);
                return digits.Substring(0, pointIndex) + "." + digits.Substring(pointIndex);

            case "exponent+200":
                return NormalizedDecimal(random, 18) + "e+200";

            case "exponent-200":
                return NormalizedDecimal(random, 18) + "e-200";

            case "exponent-subnormal":
                return NormalizedDecimal(random, 18) + "e-" + (310 + random.Next(11)).ToString(CultureInfo.InvariantCulture);

            case "decimal-uint64-top":
                return (NextUInt64(random) | (1UL << 63)).ToString(CultureInfo.InvariantCulture);

            case "hex-uint64-top":
                return "0x" + (NextUInt64(random) | (1UL << 63)).ToString("x16", CultureInfo.InvariantCulture);

            case "hex-17":
                return "0x" + RadixDigits(random, 16, 17);

            case "hex-20":
                return "0x" + RadixDigits(random, 16, 20);

            case "binary-70":
                return "0b" + RadixDigits(random, 2, 70);

            case "legacy-octal-24":
                return "0" + RadixDigits(random, 8, 24);

            default:
                throw new ArgumentOutOfRangeException(nameof(shape), shape, "Unknown literal shape.");
        }
    }

    private static string DecimalDigits(Random random, int count)
    {
        var digits = new char[count];
        digits[0] = (char)('1' + random.Next(9));
        for (var i = 1; i < count; i++)
        {
            digits[i] = (char)('0' + random.Next(10));
        }
        return new string(digits);
    }

    private static string NormalizedDecimal(Random random, int significantDigitCount)
    {
        var digits = DecimalDigits(random, significantDigitCount);
        return digits.Substring(0, 1) + "." + digits.Substring(1);
    }

    private static string RadixDigits(Random random, int radix, int count)
    {
        const string Digits = "0123456789abcdef";

        var digits = new char[count];
        digits[0] = Digits[1 + random.Next(radix - 1)];
        for (var i = 1; i < count; i++)
        {
            digits[i] = Digits[random.Next(radix)];
        }
        return new string(digits);
    }

    private static ulong NextUInt64(Random random)
    {
        return ((ulong)(uint)random.Next() << 32) | (uint)random.Next();
    }

    #endregion
}
