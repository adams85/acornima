using System.Numerics;
using Xunit;

namespace Acornima.Tests;

public partial class TokenizerTests
{
    #region IsNull

    [Theory]
    [InlineData("null", true)]
    [InlineData("", false)]
    [InlineData("Null", false)]
    [InlineData("NULL", false)]
    [InlineData("null ", false)]
    [InlineData(" null", false)]
    [InlineData("undefined", false)]
    [InlineData("true", false)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    public void IsNull_DetectsLiteral(string input, bool expectedResult)
    {
        Assert.Equal(expectedResult, Tokenizer.IsNull(input));
    }

    #endregion

    #region TryParseBoolean

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void TryParseBoolean_AcceptsValidLiterals(string input, bool expectedValue)
    {
        Assert.True(Tokenizer.TryParseBoolean(input, out var value));
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("True")]
    [InlineData("TRUE")]
    [InlineData("true ")]
    [InlineData(" true")]
    [InlineData("1")]
    [InlineData("0")]
    public void TryParseBoolean_RejectsInvalidLiterals(string input)
    {
        Assert.False(Tokenizer.TryParseBoolean(input, out var value));
        Assert.False(value);
    }

    #endregion

    #region TryParseDecimalNumber

    [Theory]
    [InlineData("0", false, 0.0)]
    [InlineData("0", true, 0.0)]
    [InlineData("0.", false, 0.0)]
    [InlineData("0.0", false, 0.0)]
    [InlineData("00.", false, 0.0)]
    [InlineData("0e100", false, 0.0)]
    [InlineData("1", false, 1.0)]
    [InlineData("10.", false, 10.0)]
    [InlineData("0.1", false, 0.1)]
    [InlineData("1.5", false, 1.5)]
    [InlineData("1e21", false, 1e21)]
    [InlineData("1e-7", false, 1e-7)]
    [InlineData(".5", false, 0.5)]
    [InlineData("07", false, 7.0)]
    [InlineData("07.", false, 7.0)]
    [InlineData("08", false, 8.0)]
    [InlineData("08.", false, 8.0)]
    [InlineData("09", false, 9.0)]
    [InlineData("09.", false, 9.0)]
    [InlineData("08.125e2", false, 812.5)]
    [InlineData("1_000", true, 1000.0)]
    [InlineData("1_0.5_0e1_0", true, 1.05e11)]
    public void TryParseDecimalNumber_AcceptsValidLiterals(string input, bool allowSeparator, double expectedValue)
    {
        Assert.True(Tokenizer.TryParseDecimalNumber(input, allowSeparator, out var value));
        Assert.Equal(Bits(expectedValue), Bits(value));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("+1", true)]
    [InlineData("-1", true)]
    [InlineData("0x10", true)]
    [InlineData("0b10", true)]
    [InlineData("0o10", true)]
    [InlineData("1n", true)]
    [InlineData("1.2.3", true)]
    [InlineData("1e", true)]
    [InlineData("1e+", true)]
    [InlineData(".", true)]
    [InlineData("_1", true)]
    [InlineData("1_", true)]
    [InlineData("1__0", true)]
    [InlineData("0_", true)]
    [InlineData("1_0", false)]
    [InlineData("Infinity", true)]
    [InlineData("NaN", true)]
    public void TryParseDecimalNumber_RejectsInvalidLiterals(string input, bool allowSeparator)
    {
        Assert.False(Tokenizer.TryParseDecimalNumber(input, allowSeparator, out var value));
        Assert.Equal(0d, value);
    }

    #endregion

    #region TryParseNumber

    [Theory]
    [InlineData("0", false, false, 0.0)]
    [InlineData("0", false, true, 0.0)]
    [InlineData("0", true, false, 0.0)]
    [InlineData("0", true, true, 0.0)]
    [InlineData("0.", false, false, 0.0)]
    [InlineData("0.", false, true, 0.0)]
    [InlineData("0.0", false, false, 0.0)]
    [InlineData("0.0", false, true, 0.0)]
    [InlineData("1", false, true, 1.0)]
    [InlineData("10.", false, true, 10.0)]
    [InlineData("0.1", false, true, 0.1)]
    [InlineData("1.5", false, true, 1.5)]
    [InlineData("1e21", false, true, 1e21)]
    [InlineData("1e-7", false, true, 1e-7)]
    [InlineData(".5", false, true, 0.5)]
    [InlineData("08", false, false, 8.0)]
    [InlineData("09", false, false, 9.0)]
    [InlineData("0123", false, false, 83.0)]
    [InlineData("1_000", true, true, 1000.0)]
    [InlineData("1_0.5_0e1_0", true, true, 1.05e11)]
    [InlineData("0x10", false, true, 16.0)]
    [InlineData("0X1F", false, true, 31.0)]
    [InlineData("0xFF", false, true, 255.0)]
    [InlineData("0o17", false, true, 15.0)]
    [InlineData("0O17", false, true, 15.0)]
    [InlineData("0b1010", false, true, 10.0)]
    [InlineData("0B1010", false, true, 10.0)]
    [InlineData("0b0_001", true, true, 1.0)]
    [InlineData("0o0_001", true, true, 1.0)]
    [InlineData("0x0_001", true, true, 1.0)]
    [InlineData("1e309", false, false, double.PositiveInfinity)]
    public void TryParseNumber_AcceptsValidLiterals(string input, bool allowSeparator, bool strict, double expectedValue)
    {
        Assert.True(Tokenizer.TryParseNumber(input, allowSeparator, strict, out var value));
        Assert.Equal(Bits(expectedValue), Bits(value));
    }

    [Theory]
    [InlineData("", true, false)]
    [InlineData(" ", true, false)]
    [InlineData("+1", true, false)]
    [InlineData("-1", true, false)]
    [InlineData("1n", true, false)]
    [InlineData("0n", true, false)]
    [InlineData("00.", true, false)]
    [InlineData("07.", true, false)]
    [InlineData("01.", true, false)]
    [InlineData("0b", true, false)]
    [InlineData("0b2", true, false)]
    [InlineData("0o", true, false)]
    [InlineData("0o8", true, false)]
    [InlineData("0x", true, false)]
    [InlineData("0xG", true, false)]
    [InlineData("1.2.3", true, false)]
    [InlineData("1e", true, false)]
    [InlineData("1e+", true, false)]
    [InlineData(".", true, false)]
    [InlineData("_1", true, false)]
    [InlineData("1_", true, false)]
    [InlineData("1__0", true, false)]
    [InlineData("0_", true, false)]
    [InlineData("1_0", false, false)]
    [InlineData("Infinity", true, false)]
    [InlineData("NaN", true, false)]
    [InlineData("00", true, true)]
    [InlineData("00.", true, true)]
    [InlineData("07", true, true)]
    [InlineData("07.", true, true)]
    [InlineData("08", true, true)]
    [InlineData("08.", true, true)]
    [InlineData("09", true, true)]
    [InlineData("09.", true, true)]
    [InlineData("017", true, true)]
    [InlineData("0123", true, true)]
    public void TryParseNumber_RejectsInvalidLiterals(string input, bool allowSeparator, bool strict)
    {
        Assert.False(Tokenizer.TryParseNumber(input, allowSeparator, strict, out var value));
        Assert.Equal(0d, value);
    }

    #endregion

    #region TryParseBigInt

    [Theory]
    [InlineData("0n", false, "0")]
    [InlineData("1n", false, "1")]
    [InlineData("10n", false, "10")]
    [InlineData("123456789012345678901234567890n", false, "123456789012345678901234567890")]
    [InlineData("0b1010n", false, "10")]
    [InlineData("0B1010n", false, "10")]
    [InlineData("0o17n", false, "15")]
    [InlineData("0O17n", false, "15")]
    [InlineData("0x10n", false, "16")]
    [InlineData("0X10n", false, "16")]
    [InlineData("0xFFn", false, "255")]
    [InlineData("1_000n", true, "1000")]
    [InlineData("0b000_1n", true, "1")]
    [InlineData("0o000_1n", true, "1")]
    [InlineData("0x1F_Fn", true, "511")]
    public void TryParseBigInt_AcceptsValidLiterals(string input, bool allowSeparator, string expectedValue)
    {
        Assert.True(Tokenizer.TryParseBigInt(input, allowSeparator, out var value));
        Assert.Equal(BigInteger.Parse(expectedValue), value);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("0", false)]
    [InlineData("1", true)]
    [InlineData("n", false)]
    [InlineData("1N", false)]
    [InlineData("1.5n", false)]
    [InlineData("0xn", false)]
    [InlineData("0b", false)]
    [InlineData("0b2", false)]
    [InlineData("0o", false)]
    [InlineData("0o8", false)]
    [InlineData("0x", false)]
    [InlineData("0xG", false)]
    [InlineData("_1n", true)]
    [InlineData("1_n", true)]
    [InlineData("1__0n", true)]
    [InlineData("0_1n", true)]
    [InlineData("1e2n", false)]
    public void TryParseBigInt_RejectsInvalidLiterals(string input, bool allowSeparator)
    {
        Assert.False(Tokenizer.TryParseBigInt(input, allowSeparator, out var value));
        Assert.Equal(BigInteger.Zero, value);
    }

    #endregion

    #region TryParseString

    [Theory]
    [InlineData("''", false, "")]
    [InlineData("''", true, "")]
    [InlineData("\"\"", false, "")]
    [InlineData("\"\"", true, "")]
    [InlineData("'abc'", true, "abc")]
    [InlineData("\"abc\"", true, "abc")]
    [InlineData(@"'\t\r\n\v\f\b'", true, "\t\r\n\v\f\b")]
    [InlineData(@"'\0'", false, "\0")]
    [InlineData(@"'\0'", true, "\0")]
    [InlineData(@"'\00'", false, "\0")]
    [InlineData(@"'\07'", false, "\x07")]
    [InlineData(@"'\08'", false, "\08")]
    [InlineData(@"'\09'", false, "\09")]
    [InlineData(@"'\7'", false, "\x07")]
    [InlineData(@"'\8'", false, "8")]
    [InlineData(@"'\9'", false, "9")]
    [InlineData(@"'\x41'", true, "A")]
    [InlineData(@"'\u0041'", true, "A")]
    [InlineData(@"'\u{1F4A9}'", true, "💩")]
    [InlineData(@"'\u{D83D}\uDCA9'", true, "💩")]
    [InlineData(@"'\u2028\u2029'", true, "\u2028\u2029")]
    [InlineData(@"'\u{10FFFF}'", true, "\udbff\udfff")]
    public void TryParseString_AcceptsValidLiterals(string input, bool strict, string expectedValue)
    {
        Assert.True(Tokenizer.TryParseString(input, strict, out var value));
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("a", false)]
    [InlineData("'", false)]
    [InlineData("\"", false)]
    [InlineData("'abc", false)]
    [InlineData("'abc\"", false)]
    [InlineData("\"abc", false)]
    [InlineData("\"abc'", false)]
    [InlineData(@"'\xg'", false)]
    [InlineData(@"'\u{110000}'", false)]
    [InlineData("'a\nb'", false)]
    [InlineData(@"'\00'", true)]
    [InlineData(@"'\07'", true)]
    [InlineData(@"'\08'", true)]
    [InlineData(@"'\09'", true)]
    [InlineData(@"'\7'", true)]
    [InlineData(@"'\8'", true)]
    [InlineData(@"'\9'", true)]
    public void TryParseString_RejectsInvalidLiterals(string input, bool strict)
    {
        Assert.False(Tokenizer.TryParseString(input, strict, out var value));
        Assert.Null(value);
    }

    #endregion
}
