using System.Numerics;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public partial class TokenizerTests
{
    #region IsNull

    [Test]
    [Arguments("null", true)]
    [Arguments("", false)]
    [Arguments("Null", false)]
    [Arguments("NULL", false)]
    [Arguments("null ", false)]
    [Arguments(" null", false)]
    [Arguments("undefined", false)]
    [Arguments("true", false)]
    [Arguments("false", false)]
    [Arguments("0", false)]
    public void IsNull_DetectsLiteral(string input, bool expectedResult)
    {
        Assert.Equal(expectedResult, Tokenizer.IsNull(input));
    }

    #endregion

    #region TryParseBoolean

    [Test]
    [Arguments("true", true)]
    [Arguments("false", false)]
    public void TryParseBoolean_AcceptsValidLiterals(string input, bool expectedValue)
    {
        Assert.True(Tokenizer.TryParseBoolean(input, out var value));
        Assert.Equal(expectedValue, value);
    }

    [Test]
    [Arguments("")]
    [Arguments("null")]
    [Arguments("True")]
    [Arguments("TRUE")]
    [Arguments("true ")]
    [Arguments(" true")]
    [Arguments("1")]
    [Arguments("0")]
    public void TryParseBoolean_RejectsInvalidLiterals(string input)
    {
        Assert.False(Tokenizer.TryParseBoolean(input, out var value));
        Assert.False(value);
    }

    #endregion

    #region TryParseDecimalNumber

    [Test]
    [Arguments("0", false, 0.0)]
    [Arguments("0", true, 0.0)]
    [Arguments("0.", false, 0.0)]
    [Arguments("0.0", false, 0.0)]
    [Arguments("00.", false, 0.0)]
    [Arguments("0e100", false, 0.0)]
    [Arguments("1", false, 1.0)]
    [Arguments("10.", false, 10.0)]
    [Arguments("0.1", false, 0.1)]
    [Arguments("1.5", false, 1.5)]
    [Arguments("1e21", false, 1e21)]
    [Arguments("1e-7", false, 1e-7)]
    [Arguments(".5", false, 0.5)]
    [Arguments("07", false, 7.0)]
    [Arguments("07.", false, 7.0)]
    [Arguments("08", false, 8.0)]
    [Arguments("08.", false, 8.0)]
    [Arguments("09", false, 9.0)]
    [Arguments("09.", false, 9.0)]
    [Arguments("08.125e2", false, 812.5)]
    [Arguments("1_000", true, 1000.0)]
    [Arguments("1_0.5_0e1_0", true, 1.05e11)]
    public void TryParseDecimalNumber_AcceptsValidLiterals(string input, bool allowSeparator, double expectedValue)
    {
        Assert.True(Tokenizer.TryParseDecimalNumber(input, allowSeparator, out var value));
        Assert.Equal(Bits(expectedValue), Bits(value));
    }

    [Test]
    [Arguments("", true)]
    [Arguments(" ", true)]
    [Arguments("+1", true)]
    [Arguments("-1", true)]
    [Arguments("0x10", true)]
    [Arguments("0b10", true)]
    [Arguments("0o10", true)]
    [Arguments("1n", true)]
    [Arguments("1.2.3", true)]
    [Arguments("1e", true)]
    [Arguments("1e+", true)]
    [Arguments(".", true)]
    [Arguments("_1", true)]
    [Arguments("1_", true)]
    [Arguments("1__0", true)]
    [Arguments("0_", true)]
    [Arguments("1_0", false)]
    [Arguments("Infinity", true)]
    [Arguments("NaN", true)]
    public void TryParseDecimalNumber_RejectsInvalidLiterals(string input, bool allowSeparator)
    {
        Assert.False(Tokenizer.TryParseDecimalNumber(input, allowSeparator, out var value));
        Assert.Equal(0d, value);
    }

    #endregion

    #region TryParseNumber

    [Test]
    [Arguments("0", false, false, 0.0)]
    [Arguments("0", false, true, 0.0)]
    [Arguments("0", true, false, 0.0)]
    [Arguments("0", true, true, 0.0)]
    [Arguments("0.", false, false, 0.0)]
    [Arguments("0.", false, true, 0.0)]
    [Arguments("0.0", false, false, 0.0)]
    [Arguments("0.0", false, true, 0.0)]
    [Arguments("1", false, true, 1.0)]
    [Arguments("10.", false, true, 10.0)]
    [Arguments("0.1", false, true, 0.1)]
    [Arguments("1.5", false, true, 1.5)]
    [Arguments("1e21", false, true, 1e21)]
    [Arguments("1e-7", false, true, 1e-7)]
    [Arguments(".5", false, true, 0.5)]
    [Arguments("08", false, false, 8.0)]
    [Arguments("09", false, false, 9.0)]
    [Arguments("0123", false, false, 83.0)]
    [Arguments("1_000", true, true, 1000.0)]
    [Arguments("1_0.5_0e1_0", true, true, 1.05e11)]
    [Arguments("0x10", false, true, 16.0)]
    [Arguments("0X1F", false, true, 31.0)]
    [Arguments("0xFF", false, true, 255.0)]
    [Arguments("0o17", false, true, 15.0)]
    [Arguments("0O17", false, true, 15.0)]
    [Arguments("0b1010", false, true, 10.0)]
    [Arguments("0B1010", false, true, 10.0)]
    [Arguments("0b0_001", true, true, 1.0)]
    [Arguments("0o0_001", true, true, 1.0)]
    [Arguments("0x0_001", true, true, 1.0)]
    [Arguments("1e309", false, false, double.PositiveInfinity)]
    public void TryParseNumber_AcceptsValidLiterals(string input, bool allowSeparator, bool strict, double expectedValue)
    {
        Assert.True(Tokenizer.TryParseNumber(input, allowSeparator, strict, out var value));
        Assert.Equal(Bits(expectedValue), Bits(value));
    }

    [Test]
    [Arguments("", true, false)]
    [Arguments(" ", true, false)]
    [Arguments("+1", true, false)]
    [Arguments("-1", true, false)]
    [Arguments("1n", true, false)]
    [Arguments("0n", true, false)]
    [Arguments("00.", true, false)]
    [Arguments("07.", true, false)]
    [Arguments("01.", true, false)]
    [Arguments("0b", true, false)]
    [Arguments("0b2", true, false)]
    [Arguments("0o", true, false)]
    [Arguments("0o8", true, false)]
    [Arguments("0x", true, false)]
    [Arguments("0xG", true, false)]
    [Arguments("1.2.3", true, false)]
    [Arguments("1e", true, false)]
    [Arguments("1e+", true, false)]
    [Arguments(".", true, false)]
    [Arguments("_1", true, false)]
    [Arguments("1_", true, false)]
    [Arguments("1__0", true, false)]
    [Arguments("0_", true, false)]
    [Arguments("1_0", false, false)]
    [Arguments("Infinity", true, false)]
    [Arguments("NaN", true, false)]
    [Arguments("00", true, true)]
    [Arguments("00.", true, true)]
    [Arguments("07", true, true)]
    [Arguments("07.", true, true)]
    [Arguments("08", true, true)]
    [Arguments("08.", true, true)]
    [Arguments("09", true, true)]
    [Arguments("09.", true, true)]
    [Arguments("017", true, true)]
    [Arguments("0123", true, true)]
    public void TryParseNumber_RejectsInvalidLiterals(string input, bool allowSeparator, bool strict)
    {
        Assert.False(Tokenizer.TryParseNumber(input, allowSeparator, strict, out var value));
        Assert.Equal(0d, value);
    }

    #endregion

    #region TryParseBigInt

    [Test]
    [Arguments("0n", false, "0")]
    [Arguments("1n", false, "1")]
    [Arguments("10n", false, "10")]
    [Arguments("123456789012345678901234567890n", false, "123456789012345678901234567890")]
    [Arguments("0b1010n", false, "10")]
    [Arguments("0B1010n", false, "10")]
    [Arguments("0o17n", false, "15")]
    [Arguments("0O17n", false, "15")]
    [Arguments("0x10n", false, "16")]
    [Arguments("0X10n", false, "16")]
    [Arguments("0xFFn", false, "255")]
    [Arguments("1_000n", true, "1000")]
    [Arguments("0b000_1n", true, "1")]
    [Arguments("0o000_1n", true, "1")]
    [Arguments("0x1F_Fn", true, "511")]
    public void TryParseBigInt_AcceptsValidLiterals(string input, bool allowSeparator, string expectedValue)
    {
        Assert.True(Tokenizer.TryParseBigInt(input, allowSeparator, out var value));
        Assert.Equal(BigInteger.Parse(expectedValue), value);
    }

    [Test]
    [Arguments("", false)]
    [Arguments("0", false)]
    [Arguments("1", true)]
    [Arguments("n", false)]
    [Arguments("1N", false)]
    [Arguments("1.5n", false)]
    [Arguments("0xn", false)]
    [Arguments("0b", false)]
    [Arguments("0b2", false)]
    [Arguments("0o", false)]
    [Arguments("0o8", false)]
    [Arguments("0x", false)]
    [Arguments("0xG", false)]
    [Arguments("_1n", true)]
    [Arguments("1_n", true)]
    [Arguments("1__0n", true)]
    [Arguments("0_1n", true)]
    [Arguments("1e2n", false)]
    public void TryParseBigInt_RejectsInvalidLiterals(string input, bool allowSeparator)
    {
        Assert.False(Tokenizer.TryParseBigInt(input, allowSeparator, out var value));
        Assert.Equal(BigInteger.Zero, value);
    }

    #endregion

    #region TryParseString

    [Test]
    [Arguments("''", false, "")]
    [Arguments("''", true, "")]
    [Arguments("\"\"", false, "")]
    [Arguments("\"\"", true, "")]
    [Arguments("'abc'", true, "abc")]
    [Arguments("\"abc\"", true, "abc")]
    [Arguments(@"'\t\r\n\v\f\b'", true, "\t\r\n\v\f\b")]
    [Arguments(@"'\0'", false, "\0")]
    [Arguments(@"'\0'", true, "\0")]
    [Arguments(@"'\00'", false, "\0")]
    [Arguments(@"'\07'", false, "\x07")]
    [Arguments(@"'\08'", false, "\08")]
    [Arguments(@"'\09'", false, "\09")]
    [Arguments(@"'\7'", false, "\x07")]
    [Arguments(@"'\8'", false, "8")]
    [Arguments(@"'\9'", false, "9")]
    [Arguments(@"'\x41'", true, "A")]
    [Arguments(@"'\u0041'", true, "A")]
    [Arguments(@"'\u{1F4A9}'", true, "💩")]
    [Arguments(@"'\u{D83D}\uDCA9'", true, "💩")]
    [Arguments(@"'\u2028\u2029'", true, "\u2028\u2029")]
    [Arguments(@"'\u{10FFFF}'", true, "\udbff\udfff")]
    public void TryParseString_AcceptsValidLiterals(string input, bool strict, string expectedValue)
    {
        Assert.True(Tokenizer.TryParseString(input, strict, out var value));
        Assert.Equal(expectedValue, value);
    }

    [Test]
    [Arguments("", false)]
    [Arguments("a", false)]
    [Arguments("'", false)]
    [Arguments("\"", false)]
    [Arguments("'abc", false)]
    [Arguments("'abc\"", false)]
    [Arguments("\"abc", false)]
    [Arguments("\"abc'", false)]
    [Arguments(@"'\xg'", false)]
    [Arguments(@"'\u{110000}'", false)]
    [Arguments("'a\nb'", false)]
    [Arguments(@"'\00'", true)]
    [Arguments(@"'\07'", true)]
    [Arguments(@"'\08'", true)]
    [Arguments(@"'\09'", true)]
    [Arguments(@"'\7'", true)]
    [Arguments(@"'\8'", true)]
    [Arguments(@"'\9'", true)]
    public void TryParseString_RejectsInvalidLiterals(string input, bool strict)
    {
        Assert.False(Tokenizer.TryParseString(input, strict, out var value));
        Assert.Null(value);
    }

    #endregion
}
