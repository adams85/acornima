using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Acornima.Helpers;
using Acornima.Tests.Acorn;
using Xunit;

namespace Acornima.Tests;

public class TokenizerTests
{
    [Fact]
    public void CanResetTokenizer()
    {
        var comments = new List<Comment>();
        var tokens = new List<Token>();
        var tokensDirect = new List<Token>();

        const string code = "var /* c1 */ foo=1; // c2";

        var tokenizer = new Tokenizer(code, new TokenizerOptions
        {
            OnComment = (in Comment comment) => comments.Add(comment),
            OnToken = (in Token token) => tokens.Add(token)
        });

        for (var n = 0; n < 3; n++, tokenizer.Reset(code))
        {
            comments.Clear();
            tokens.Clear();
            tokensDirect.Clear();

            Token token;
            do
            {
                token = tokenizer.GetToken();
                tokensDirect.Add(token);
            }
            while (token.Kind != TokenKind.EOF);

            Assert.Equal(new string[] { "var", "foo", "=", "1", ";", "" }, tokens.Select(t => t.GetRawValue(code).ToString()).ToArray());
            Assert.Equal(tokens, tokensDirect);

            Assert.Equal(new string[] { "/* c1 */", "// c2" }, comments.Select(c => c.GetRawValue(code).ToString()).ToArray());
        }
    }

    [Fact]
    public void CanResetScannerToCustomPosition()
    {
        var comments = new List<Comment>();
        var tokens = new List<Token>();
        var tokensDirect = new List<Token>();

        const string code = "var /* c1 */ foo=1; // c2";

        var tokenizer = new Tokenizer(code, new TokenizerOptions
        {
            OnComment = (in Comment comment) => comments.Add(comment),
            OnToken = (in Token token) => tokens.Add(token)
        });

        tokenizer.Reset(code, 4, code.Length - 4);

        comments.Clear();
        tokens.Clear();
        tokensDirect.Clear();

        Token token;
        do
        {
            token = tokenizer.GetToken();
            tokensDirect.Add(token);
        }
        while (token.Kind != TokenKind.EOF);

        Assert.Equal(new string[] { "foo", "=", "1", ";", "" }, tokens.Select(t => t.GetRawValue(code).ToString()).ToArray());
        Assert.Equal(tokens, tokensDirect);

        Assert.Equal(new string[] { "/* c1 */", "// c2" }, comments.Select(c => c.GetRawValue(code).ToString()).ToArray());
    }

    [Fact]
    public void ShouldRejectInvalidUnescapedSurrogateAsIdentifierStart()
    {
        // These values are altered by XUnit if passed in InlineData to the test method
        foreach (var s in new[]
        {
            "\ud800",
            "\ud800b",
            "\ud800\ud800",
            "\udc00",
            "\udc00b",
            "\udc00\ud800",
            "\udc00\udc00",
        })
        {
            var tokenizer = new Tokenizer(s);
            var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.Next());
            Assert.StartsWith("Invalid or unexpected token", ex.Error.Description);
        }
    }

    [Fact]
    public void ShouldRejectInvalidUnescapedSurrogateAsIdentifierPart()
    {
        // These values are altered by XUnit if passed in InlineData to the test method
        foreach (var s in new[]
        {
            "a\ud800",
            "a\ud800b",
            "a\ud800\ud800",
            "a\udc00",
            "a\udc00b",
            "a\udc00\ud800",
            "a\udc00\udc00",
        })
        {
            var tokenizer = new Tokenizer(s);
            var ex = Assert.Throws<SyntaxErrorException>(() =>
            {
                tokenizer.Next();
                tokenizer.Next();
            });
            Assert.StartsWith("Invalid or unexpected token", ex.Error.Description);
        }
    }

    [InlineData(@"\ud800")]
    [InlineData(@"\udc00")]
    [InlineData(@"\ud800\udc00")]
    [InlineData(@"\u{d800}")]
    [InlineData(@"\u{dc00}")]
    [InlineData(@"\u{d800}\u{dc00}")]
    [Theory]
    public void ShouldRejectEscapedSurrogateAsIdentifierStart(string s)
    {
        var tokenizer = new Tokenizer(s);
        var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.Next());
        Assert.StartsWith("Invalid or unexpected token", ex.Error.Description);
    }

    [InlineData(@"a\ud800")]
    [InlineData(@"a\udc00")]
    [InlineData(@"a\ud800\udc00")]
    [InlineData(@"a\u{d800}")]
    [InlineData(@"a\u{dc00}")]
    [InlineData(@"a\u{d800}\u{dc00}")]
    [Theory]
    public void ShouldRejectEscapedSurrogateAsIdentifierPart(string s)
    {
        var tokenizer = new Tokenizer(s);
        var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.Next());
        Assert.StartsWith("Invalid or unexpected token", ex.Error.Description);
    }

    [Fact]
    public void ShouldAcceptSurrogateRangeInLiterals()
    {
        var tokenizer = new Tokenizer(@"'a\u{d800}\u{dc00}'");
        var token = tokenizer.GetToken();
        Assert.Equal(TokenKind.StringLiteral, token.Kind);
        Assert.Equal("a\ud800\udc00", token.StringValue);
    }

    [Fact]
    public void IsLineTerminatorMatchesAcornImpl()
    {
        static bool IsLineTerminatorAcorn(ushort ch) => ch is 10 or 13 or 0x2028 or 0x2029;

        int cp;
        for (cp = 0; cp <= char.MaxValue; cp++)
        {
            var isLineTerminator = IsLineTerminatorAcorn((char)cp);
            Assert.Equal(isLineTerminator, (Tokenizer.GetCharFlags((char)cp) & Tokenizer.CharFlags.Skipped) == Tokenizer.CharFlags.LineTerminator);
            Assert.Equal(isLineTerminator, Tokenizer.IsNewLine((char)cp));

            if (isLineTerminator)
            {
                // Together with IsWhiteSpaceMatchesAcornImpl this test makes sure that
                // the set of line terminators and set of whitespace characters are disjunct because
                // we rely on this assumption (see Tokenizer.CharFlags).
                Assert.False(Tokenizer.IsWhiteSpace((char)cp));
            }
        }
    }

    [Fact]
    public void IsWhiteSpaceMatchesAcornImpl()
    {
        static bool IsWhiteSpaceAcorn(ushort ch) => ch is 32 or 160
            or (> 8 and < 14 and not (10 or 13))
            || ch >= 5760 && AcornWhitespace.NonASCIIwhitespace.IsMatch(((char)ch).ToStringCached());

        int cp;
        for (cp = 0; cp <= char.MaxValue; cp++)
        {
            var isWhiteSpace = IsWhiteSpaceAcorn((char)cp);
            Assert.Equal(isWhiteSpace, (Tokenizer.GetCharFlags((char)cp) & Tokenizer.CharFlags.Skipped) == Tokenizer.CharFlags.WhiteSpace);
            Assert.Equal(isWhiteSpace, Tokenizer.IsWhiteSpace((char)cp));

            if (isWhiteSpace)
            {
                // Together with IsLineTerminatorAcorn this test makes sure that
                // the set of line terminators and set of whitespace characters are disjunct because
                // we rely on this assumption (see Tokenizer.CharFlags).
                Assert.False(Tokenizer.IsNewLine((char)cp));
            }
        }
    }

    [Fact]
    public void IsIdentifierCharMatchesAcornImpl()
    {
        int cp;
        for (cp = 0; cp <= UnicodeHelper.LastCodePoint; cp++)
        {
            Assert.Equal(AcornIdentifier.IsIdentifierStart(cp, astral: true), Tokenizer.IsIdentifierStart(cp, allowAstral: true));
            Assert.Equal(AcornIdentifier.IsIdentifierChar(cp, astral: true), Tokenizer.IsIdentifierChar(cp, allowAstral: true));
        }
    }

    [Fact]
    public void IsIdentifierStartIsASubsetOfIsIdentifierChar()
    {
        // The current parser implementation translated from acornjs relies on the assumption that every char which is
        // an element of the IdentifierStartChar set is also an element of the IdentifierPartChar set (see Tokenizer.ReadWord1, Tokenizer.RegExpParser.ReadIdentifier).
        // This test makes sure that this assumption is true.

        int cp;
        for (cp = 0; cp <= UnicodeHelper.LastCodePoint; cp++)
        {
            Assert.True(!Tokenizer.IsIdentifierStart(cp, allowAstral: true) || Tokenizer.IsIdentifierChar(cp, allowAstral: true));
        }
    }

    private const string ValueOf_255_F_HexDigits = "11,235,582,092,889,474,423,308,157,442,431,404,585,112,356,118,389,416,079,589,380,072,358,292,237,843,810,195,794,279,832,650,471,001,320,007,117,491,962,084,853,674,360,550,901,038,905,802,964,414,967,132,773,610,493,339,054,092,829,768,888,725,077,880,882,465,817,684,505,312,860,552,384,417,646,403,930,092,119,569,408,801,702,322,709,406,917,786,643,639,996,702,871,154,982,269,052,209,770,601,514,008,575";
    private const string ValueOf_256_F_HexDigits = "179,769,313,486,231,590,772,930,519,078,902,473,361,797,697,894,230,657,273,430,081,157,732,675,805,500,963,132,708,477,322,407,536,021,120,113,879,871,393,357,658,789,768,814,416,622,492,847,430,639,474,124,377,767,893,424,865,485,276,302,219,601,246,094,119,453,082,952,085,005,768,838,150,682,342,462,881,473,913,110,540,827,237,163,350,510,684,586,298,239,947,245,938,479,716,304,835,356,329,624,224,137,215";

    [Theory]
    [InlineData("0xfedc_ba98_7654_3210", "18,364,758,544,493,064,720")]
    [InlineData("0xfedc_ba98_7654_3210n", "18,364,758,544,493,064,720")]
    [InlineData("0xFEDC_BA98_7654_3210", "18,364,758,544,493,064,720")]
    [InlineData("0xFEDC_BA98_7654_3210n", "18,364,758,544,493,064,720")]
    [InlineData("0XFEDC_BA98_7654_3210", "18,364,758,544,493,064,720")]
    [InlineData("0XFEDC_BA98_7654_3210n", "18,364,758,544,493,064,720")]
    [InlineData("0o17_7334_5651_4166_2503_1020", "18,364,758,544,493,064,720")]
    [InlineData("0o17_7334_5651_4166_2503_1020n", "18,364,758,544,493,064,720")]
    [InlineData("0O17_7334_5651_4166_2503_1020", "18,364,758,544,493,064,720")]
    [InlineData("0O17_7334_5651_4166_2503_1020n", "18,364,758,544,493,064,720")]
    [InlineData("0b11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "18,364,758,544,493,064,720")]
    [InlineData("0b11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "18,364,758,544,493,064,720")]
    [InlineData("0B11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "18,364,758,544,493,064,720")]
    [InlineData("0B11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "18,364,758,544,493,064,720")]

    [InlineData("0x1_FEDC_BA98_7654_3210", "36,811,502,618,202,616,336")]
    [InlineData("0x1_FEDC_BA98_7654_3210n", "36,811,502,618,202,616,336")]
    [InlineData("0o37_7334_5651_4166_2503_1020", "36,811,502,618,202,616,336")]
    [InlineData("0o37_7334_5651_4166_2503_1020n", "36,811,502,618,202,616,336")]
    [InlineData("0b1_11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "36,811,502,618,202,616,336")]
    [InlineData("0b1_11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "36,811,502,618,202,616,336")]

    [InlineData("0xFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF", ValueOf_255_F_HexDigits)]
    [InlineData("0xFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFFn", ValueOf_255_F_HexDigits)]
    [InlineData("0xFFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF", "Infinity")]
    [InlineData("0xFFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFFn", ValueOf_256_F_HexDigits)]
    public void CanReadRadixNumber(string input, string expectedValue)
    {
        var tokenizer = new Tokenizer(input);

        var token = tokenizer.GetToken();

        var isBigInt = input.AsSpan().Last() == 'n';
        Assert.Equal(isBigInt ? TokenKind.BigIntLiteral : TokenKind.NumericLiteral, token.Kind);

        expectedValue = expectedValue.Replace("_", "");

        object expectedValueObj = !isBigInt
            ? double.Parse(expectedValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
            : BigInteger.Parse(expectedValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        Assert.Equal(expectedValueObj, token.Value);
    }

    [Theory]
    [InlineData("0", false, "0")]
    [InlineData("0", true, "0")]
    [InlineData("0.", false, "0")]
    [InlineData("0.", true, "0")]
    [InlineData("0_", false, "<Numeric separator can not be used after leading 0>")]
    [InlineData("0_", true, "<Numeric separator can not be used after leading 0>")]
    [InlineData("0e", false, "<Invalid or unexpected token>")]
    [InlineData("0e", true, "<Invalid or unexpected token>")]
    [InlineData("0n", false, "0")]
    [InlineData("0n", true, "0")]
    [InlineData("00", false, "0")]
    [InlineData("00", true, "<Octal literals are not allowed in strict mode>")]
    [InlineData("00.", false, "0")]
    [InlineData("00.", true, "<Octal literals are not allowed in strict mode>")]
    [InlineData("00_", false, "<Invalid or unexpected token>")]
    [InlineData("00_", true, "<Invalid or unexpected token>")]
    [InlineData("00e", false, "<Invalid or unexpected token>")]
    [InlineData("00e", true, "<Invalid or unexpected token>")]
    [InlineData("00n", false, "<Invalid or unexpected token>")]
    [InlineData("00n", true, "<Invalid or unexpected token>")]
    [InlineData("00.1", false, "0")]
    [InlineData("00.1e-324", false, "0")]
    [InlineData("07", false, "7")]
    [InlineData("07", true, "<Octal literals are not allowed in strict mode>")]
    [InlineData("07n", false, "<Invalid or unexpected token>")]
    [InlineData("08", false, "8")]
    [InlineData("08", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("08n", false, "<Invalid or unexpected token>")]
    [InlineData("09", false, "9")]
    [InlineData("09", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("09n", false, "<Invalid or unexpected token>")]
    [InlineData("017", false, "15")]
    [InlineData("017", true, "<Octal literals are not allowed in strict mode>")]
    [InlineData("017n", false, "<Invalid or unexpected token>")]
    [InlineData("017.", false, "15")] // TODO: test parse error `var x = 017.;` vs `function f() {   return 017.; }()` 
    [InlineData("017.", true, "<Octal literals are not allowed in strict mode>")]
    [InlineData("017.1", false, "15")]
    [InlineData("017.1", true, "<Octal literals are not allowed in strict mode>")]
    [InlineData("017.e1", false, "15")]
    [InlineData("017.e1", true, "<Octal literals are not allowed in strict mode>")]
    [InlineData("017e", false, "<Invalid or unexpected token>")]
    [InlineData("017e", true, "<Invalid or unexpected token>")]
    [InlineData("018", false, "18")]
    [InlineData("018", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018n", false, "<Invalid or unexpected token>")]
    [InlineData("018.", false, "18")] // TODO: test parse error `var x = 018.;` vs `function f() {   return 018.; }()` 
    [InlineData("018.", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018.1", false, "18.1")]
    [InlineData("018.1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018.e1", false, "180")]
    [InlineData("018.e1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018_1", false, "<Invalid or unexpected token>")]
    [InlineData("018_1", true, "<Invalid or unexpected token>")]
    [InlineData("018e", false, "<Invalid or unexpected token>")]
    [InlineData("018e", true, "<Invalid or unexpected token>")]
    [InlineData("018.e", false, "<Invalid or unexpected token>")]
    [InlineData("018.e", true, "<Invalid or unexpected token>")]
    [InlineData("018e2", false, "1800")]
    [InlineData("018e2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018e+", false, "<Invalid or unexpected token>")]
    [InlineData("018e+", true, "<Invalid or unexpected token>")]
    [InlineData("018e+2", false, "1800")]
    [InlineData("018e+2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018e-", false, "<Invalid or unexpected token>")]
    [InlineData("018e-", true, "<Invalid or unexpected token>")]
    [InlineData("018e-1", false, "1.8")]
    [InlineData("018e-1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018.1e2", false, "1810")]
    [InlineData("018.1e2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018.1e+2", false, "1810")]
    [InlineData("018.1e+2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("018.1e-1", false, "1.81")]
    [InlineData("018.1e-1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("019", false, "19")]
    [InlineData("019", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("019.e1", false, "190")]
    [InlineData("019.e1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [InlineData("019_1", false, "<Invalid or unexpected token>")]
    [InlineData("019_1", true, "<Invalid or unexpected token>")]
    [InlineData("7", true, "7")]
    [InlineData("7n", true, "7")]
    [InlineData("17", true, "17")]
    [InlineData("17n", true, "17")]
    [InlineData("17.", true, "17")] // TODO: test parse error `var x = 17.;` vs `function f() {   return 17.; }(, null)`
    [InlineData("17.1", true, "17.1")]
    [InlineData("17.1n", true, "<Invalid or unexpected token>")]
    [InlineData("17e", false, "<Invalid or unexpected token>")]
    [InlineData("17.e", false, "<Invalid or unexpected token>")]
    [InlineData("17e2", true, "1700")]
    [InlineData("17e+", false, "<Invalid or unexpected token>")]
    [InlineData("17e+2", true, "1700")]
    [InlineData("17e-", false, "<Invalid or unexpected token>")]
    [InlineData("17e-1", true, "1.7")]
    [InlineData("17.1e2", true, "1710")]
    [InlineData("17.1e+2", true, "1710")]
    [InlineData("17.1e-1", true, "1.71")]
    [InlineData(".7", true, "0.7")]
    [InlineData(".7n", false, "<Invalid or unexpected token>")]
    [InlineData(".17", true, "0.17")]
    [InlineData(".17n", false, "<Invalid or unexpected token>")]
    [InlineData(".17.", true, "0.17")] // TODO: test parse error `var x = 17.;` vs `function f() {   return 17.; }()`
    [InlineData(".17.1", true, "0.17")]
    [InlineData(".17e", false, "<Invalid or unexpected token>")]
    [InlineData(".17.e", true, "0.17")]
    [InlineData(".17e2", true, "17")]
    [InlineData(".17e+", false, "<Invalid or unexpected token>")]
    [InlineData(".17e+2", true, "17")]
    [InlineData(".17e-", false, "<Invalid or unexpected token>")]
    [InlineData(".17e-1", true, "0.017")]
    [InlineData(".17.1e2", true, "0.17")]
    [InlineData(".17.1e+2", true, "0.17")]
    [InlineData(".17.1e-1", true, "0.17")]
    [InlineData("0777_7777", false, "<Invalid or unexpected token>")]
    [InlineData("0777_7777", true, "<Invalid or unexpected token>")]

    [InlineData("18_364_758_544_493_064_720", true, "1.8364758544493064e+19")]
    [InlineData("18_364_758_544_493_064_720n", true, "18_364_758_544_493_064_720")]
    [InlineData("36_811_502_618_202_616_336", true, "3.6811502618202616E+19")]
    [InlineData("36_811_502_618_202_616_336n", true, "36,811,502,618,202,616,336")]
    [InlineData("11_235_582_092_889_474_423_308_157_442_431_404_585_112_356_118_389_416_079_589_380_072_358_292_237_843_810_195_794_279_832_650_471_001_320_007_117_491_962_084_853_674_360_550_901_038_905_802_964_414_967_132_773_610_493_339_054_092_829_768_888_725_077_880_882_465_817_684_505_312_860_552_384_417_646_403_930_092_119_569_408_801_702_322_709_406_917_786_643_639_996_702_871_154_982_269_052_209_770_601_514_008_575", true, ValueOf_255_F_HexDigits)]
    [InlineData("11_235_582_092_889_474_423_308_157_442_431_404_585_112_356_118_389_416_079_589_380_072_358_292_237_843_810_195_794_279_832_650_471_001_320_007_117_491_962_084_853_674_360_550_901_038_905_802_964_414_967_132_773_610_493_339_054_092_829_768_888_725_077_880_882_465_817_684_505_312_860_552_384_417_646_403_930_092_119_569_408_801_702_322_709_406_917_786_643_639_996_702_871_154_982_269_052_209_770_601_514_008_575n", true, ValueOf_255_F_HexDigits)]
    [InlineData("179_769_313_486_231_590_772_930_519_078_902_473_361_797_697_894_230_657_273_430_081_157_732_675_805_500_963_132_708_477_322_407_536_021_120_113_879_871_393_357_658_789_768_814_416_622_492_847_430_639_474_124_377_767_893_424_865_485_276_302_219_601_246_094_119_453_082_952_085_005_768_838_150_682_342_462_881_473_913_110_540_827_237_163_350_510_684_586_298_239_947_245_938_479_716_304_835_356_329_624_224_137_215", true, "Infinity")]
    [InlineData("179_769_313_486_231_590_772_930_519_078_902_473_361_797_697_894_230_657_273_430_081_157_732_675_805_500_963_132_708_477_322_407_536_021_120_113_879_871_393_357_658_789_768_814_416_622_492_847_430_639_474_124_377_767_893_424_865_485_276_302_219_601_246_094_119_453_082_952_085_005_768_838_150_682_342_462_881_473_913_110_540_827_237_163_350_510_684_586_298_239_947_245_938_479_716_304_835_356_329_624_224_137_215n", true, ValueOf_256_F_HexDigits)]
    [InlineData("0.000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_01", true, "1e-323")]
    [InlineData("0.000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_001", true, "0")]
    [InlineData(".000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_01", true, "1e-323")]
    [InlineData(".000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_001", true, "0")]
    [InlineData("1.8_364_758_544_493_064_720e19", true, "1.8364758544493064e+19")]
    [InlineData("1.8_364_758_544_493_064_720e+19", true, "1.8364758544493064e+19")]
    [InlineData("1_8_364_758_544_493_064_720_000e-3", true, "1.8364758544493064e+19")]
    [InlineData("1_8_364_758_544_493_064_720_000.0e-3", true, "1.8364758544493064e+19")]
    [InlineData("1.8364758544493064720e1_9", true, "1.8364758544493064e+19")]
    [InlineData("1.1235582092889474E+307", true, ValueOf_255_F_HexDigits)]
    [InlineData("1.797_693_134_862_315_9e308", true, "Infinity")]
    [InlineData("1.797_693_134_862_315_9e+308", true, "Infinity")]
    [InlineData("1e-324", true, "0")]
    [InlineData("0.01e-322", true, "0")]
    [InlineData(".01e-322", true, "0")]

    [InlineData("018364758544493064720", false, "1.8364758544493064e+19")]
    [InlineData("018364758544493064720n", false, "<Invalid or unexpected token>")]
    [InlineData("018364758544493064720n", true, "<Invalid or unexpected token>")]
    [InlineData("036811502618202616336", false, "3.6811502618202616E+19")]
    [InlineData("011235582092889474423308157442431404585112356118389416079589380072358292237843810195794279832650471001320007117491962084853674360550901038905802964414967132773610493339054092829768888725077880882465817684505312860552384417646403930092119569408801702322709406917786643639996702871154982269052209770601514008575", false, ValueOf_255_F_HexDigits)]
    [InlineData("0179769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137215", false, "Infinity")]
    public void CanReadNumber(string input, bool strict, string expectedValue)
    {
        var tokenizer = new Tokenizer(input);
        var tokenizerContext = new TokenizerContext(strict);

        if (!(expectedValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && expectedValue.EndsWith(">", StringComparison.OrdinalIgnoreCase)))
        {
            var token = tokenizer.GetToken(tokenizerContext);

            var isBigInt = input.AsSpan().Last() == 'n';
            Assert.Equal(isBigInt ? TokenKind.BigIntLiteral : TokenKind.NumericLiteral, token.Kind);

            expectedValue = expectedValue.Replace("_", "");

            object expectedValueObj = !isBigInt
                ? double.Parse(expectedValue, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture)
                : BigInteger.Parse(expectedValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

            Assert.Equal(expectedValueObj, token.Value);
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.GetToken(tokenizerContext));

            var expectedMessage = expectedValue.Substring(1, expectedValue.Length - 2);
            Assert.Equal(expectedMessage, ex.Error.Description);
        }
    }

    [Theory]
    [InlineData(@"""", false, "<Invalid or unexpected token>")]
    [InlineData(@"'", false, "<Invalid or unexpected token>")]
    [InlineData(@"""a", false, "<Invalid or unexpected token>")]
    [InlineData(@"'a", false, "<Invalid or unexpected token>")]
    [InlineData(@"""'", false, "<Invalid or unexpected token>")]
    [InlineData(@"'""", false, "<Invalid or unexpected token>")]
    [InlineData(@"""""", true, "")]
    [InlineData(@"''", true, "")]
    // Common escape sequences
    [InlineData(@"'\t\r\n\v\f\b'", true, "\t\r\n\v\f\b")]
    // Identity escape sequences
    [InlineData(@"'\cA'", true, "cA")]
    // New line escape sequences
    [InlineData("'\r", false, "<Invalid or unexpected token>")]
    [InlineData("'\r'", false, "<Invalid or unexpected token>")]
    [InlineData("'\n'", false, "<Invalid or unexpected token>")]
    [InlineData("'\r\n'", false, "<Invalid or unexpected token>")]
    [InlineData("'\u2028\u2029'", true, "\u2028\u2029")]
    [InlineData("'\\\r'", true, "")]
    [InlineData("'\\\n'", true, "")]
    [InlineData("'\\\r\\\n'", true, "")]
    [InlineData("'\\\u2028\\\u2029'", true, "")]
    [InlineData("'a\\\rb'", true, "ab")]
    [InlineData("'a\\\nb'", true, "ab")]
    [InlineData("'a\\\r\\\nb'", true, "ab")]
    [InlineData("'a\\\u2028\\\u2029b'", true, "ab")]
    // Octal escape sequences
    [InlineData(@"'\0'", false, "\0")]
    [InlineData(@"'\0'", true, "\0")]
    [InlineData(@"'\00'", false, "\0")]
    [InlineData(@"'\00'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [InlineData(@"'\000'", false, "\0")]
    [InlineData(@"'\000'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [InlineData(@"'\0000'", false, "\00")]
    [InlineData(@"'\7'", false, "\x07")]
    [InlineData(@"'\7'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [InlineData(@"'\07'", false, "\x07")]
    [InlineData(@"'\07'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [InlineData(@"'\007'", false, "\x07")]
    [InlineData(@"'\0007'", false, "\07")]
    [InlineData(@"'\8'", false, "8")]
    [InlineData(@"'\8'", true, "<\\8 and \\9 are not allowed in strict mode>")]
    [InlineData(@"'\08'", false, "\08")]
    [InlineData(@"'\08'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [InlineData(@"'\008'", false, "\08")]
    [InlineData(@"'\0008'", false, "\08")]
    [InlineData(@"'\9'", false, "9")]
    [InlineData(@"'\9'", true, "<\\8 and \\9 are not allowed in strict mode>")]
    [InlineData(@"'\09'", false, "\09")]
    [InlineData(@"'\09'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [InlineData(@"'\77'", false, "\x3F")]
    [InlineData(@"'\077'", false, "\x3F")]
    [InlineData(@"'\0077'", false, "\x07\x37")]
    [InlineData(@"'\377'", false, "\xFF")]
    [InlineData(@"'\0377'", false, "\x1F\x37")]
    [InlineData(@"'\400'", false, "\x20\x30")]
    [InlineData(@"'\040'", false, "\x20")]
    // Hexadecimal escape sequences (2 digits)
    [InlineData(@"'\x", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\x'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\x0'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\xA'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\xf'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\x0$'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\x00", false, "<Invalid or unexpected token>")]
    [InlineData(@"'\x00'", true, "\0")]
    [InlineData(@"'\x09'", true, "\x09")]
    [InlineData(@"'\x0A'", true, "\x0A")]
    [InlineData(@"'\x0F'", true, "\x0F")]
    [InlineData(@"'\x0G'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\x0a'", true, "\x0A")]
    [InlineData(@"'\x0f'", true, "\x0F")]
    [InlineData(@"'\x0g'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\xff'", true, "\xFF")]
    [InlineData(@"'\xfg'", false, "<Invalid hexadecimal escape sequence>")]
    [InlineData(@"'\x3f0'", true, "?0")]
    [InlineData(@"'\x00\x09\x0d\x0a\x0b\x0C\x08'", true, "\0\t\r\n\v\f\b")]
    // Hexadecimal escape sequences (4 digits)
    [InlineData(@"'\u", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u0'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\uA'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\uf'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u00'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u0A'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u0f'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u000'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u00A'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u00f'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u000$'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u0000", false, "<Invalid or unexpected token>")]
    [InlineData(@"'\u0000'", true, "\0")]
    [InlineData(@"'\u0009'", true, "\x09")]
    [InlineData(@"'\u000A'", true, "\x0A")]
    [InlineData(@"'\u000F'", true, "\x0F")]
    [InlineData(@"'\u000G'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u000a'", true, "\x0A")]
    [InlineData(@"'\u000f'", true, "\x0F")]
    [InlineData(@"'\u000g'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\uffff'", true, "\xFFFF")]
    [InlineData(@"'\ufffg'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u003f0'", true, "?0")]
    [InlineData(@"'\u0000\u0009\u000d\u000a\u000b\u000C\u0008'", true, "\0\t\r\n\v\f\b")]
    // Hexadecimal escape sequences (Unicode code points)
    [InlineData(@"'\u{", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{0", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{0'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{}'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{ }'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{-1}'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{-0}'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{0}", false, "<Invalid or unexpected token>")]
    [InlineData(@"'\u{10FFFF}'", true, "\udbff\udfff")]
    [InlineData(@"'\u{110000}'", false, "<Undefined Unicode code-point>")]
    [InlineData(@"'\u{80000000}'", false, "<Undefined Unicode code-point>")]
    [InlineData(@"'\u{1.0}'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{1e10}'", true, "\u1E10")]
    [InlineData(@"'\u{.1e10}'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{a}'", true, "\x0A")]
    [InlineData(@"'\u{f}'", true, "\x0F")]
    [InlineData(@"'\u{g}'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{FFFF}'", true, "\xFFFF")]
    [InlineData(@"'\u{FFFG}'", false, "<Invalid Unicode escape sequence>")]
    [InlineData(@"'\u{1F4A9}'", true, "💩")]
    [InlineData(@"'\u{D83D}\uDCA9'", true, "💩")]
    [InlineData(@"'\uD83D\u{DCA9}'", true, "💩")]
    [InlineData(@"'\u{0}\u{9}\u{d}\u{a}\u{b}\u{C}\u{00000000000000008}'", true, "\0\t\r\n\v\f\b")]
    public void CanReadString(string input, bool strict, string expectedValue)
    {
        var tokenizer = new Tokenizer(input);
        var tokenizerContext = new TokenizerContext(strict);

        if (!(expectedValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && expectedValue.EndsWith(">", StringComparison.OrdinalIgnoreCase)))
        {
            var token = tokenizer.GetToken(tokenizerContext);

            Assert.Equal(TokenKind.StringLiteral, token.Kind);
            Assert.Equal(expectedValue, token.Value);
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.GetToken(tokenizerContext));

            var expectedMessage = expectedValue.Substring(1, expectedValue.Length - 2);
            Assert.Equal(expectedMessage, ex.Error.Description);
        }
    }

    [Theory]
    [InlineData(@"`", false, "<Unexpected end of input>", null)]
    [InlineData(@"``", true, "", null)]
    [InlineData(@"`$", false, "<Unexpected end of input>", null)]
    [InlineData(@"`${", false, "<Unexpected end of input>", null)]
    [InlineData(@"`${x}", false, "<Unexpected end of input>", null)]
    [InlineData(@"`${x}`", true, "", null)]
    // Common escape sequences
    [InlineData(@"`\t\r\n\v\f\b`", true, "\t\r\n\v\f\b", "\\t\\r\\n\\v\\f\\b")]
    // Identity escape sequences
    [InlineData(@"`\cA`", true, "cA", "\\cA")]
    // New line escape sequences
    [InlineData("`\r", false, "<Unexpected end of input>", null)]
    [InlineData("`\r`", true, "\n", null)]
    [InlineData("`\n`", true, "\n", null)]
    [InlineData("`\r\n`", true, "\n", null)]
    [InlineData("`\u2028\u2029`", true, "\u2028\u2029", null)]
    [InlineData("`\\\r`", true, "", "\\\n")]
    [InlineData("`\\\n`", true, "", "\\\n")]
    [InlineData("`\\\r\\\n`", true, "", "\\\n\\\n")]
    [InlineData("`\\\u2028\\\u2029`", true, "", "\\\u2028\\\u2029")]
    [InlineData("`a\\\rb`", true, "ab", "a\\\nb")]
    [InlineData("`a\\\nb`", true, "ab", "a\\\nb")]
    [InlineData("`a\\\r\\\nb`", true, "ab", "a\\\n\\\nb")]
    [InlineData("`a\\\u2028\\\u2029b`", true, "ab", "a\\\u2028\\\u2029b")]
    // Octal escape sequences
    [InlineData(@"`\0`", false, "\0", "\\0")]
    [InlineData(@"`\0`", true, "\0", "\\0")]
    [InlineData(@"`\00`", false, "<Octal escape sequences are not allowed in template strings>", null)]
    [InlineData(@"`\00`", true, "<Octal escape sequences are not allowed in template strings>", null)]
    [InlineData(@"`\8`", false, "<\\8 and \\9 are not allowed in template strings>", null)]
    [InlineData(@"`\08`", false, "<Octal escape sequences are not allowed in template strings>", null)]
    [InlineData(@"`\9`", false, "<\\8 and \\9 are not allowed in template strings>", null)]
    [InlineData(@"`\09`", false, "<Octal escape sequences are not allowed in template strings>", null)]
    // Hexadecimal escape sequences (2 digits)
    [InlineData(@"`\x", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\x`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\x0`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\xA`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\xf`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\x0$`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\x00", false, "<Unexpected end of input>", null)]
    [InlineData(@"`\x00`", true, "\0", "\\x00")]
    [InlineData(@"`\x09`", true, "\x09", "\\x09")]
    [InlineData(@"`\x0A`", true, "\x0A", "\\x0A")]
    [InlineData(@"`\x0F`", true, "\x0F", "\\x0F")]
    [InlineData(@"`\x0G`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\x0a`", true, "\x0A", "\\x0a")]
    [InlineData(@"`\x0f`", true, "\x0F", "\\x0f")]
    [InlineData(@"`\x0g`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\xff`", true, "\xFF", "\\xff")]
    [InlineData(@"`\xfg`", false, "<Invalid hexadecimal escape sequence>", null)]
    [InlineData(@"`\x3f0`", true, "?0", "\\x3f0")]
    [InlineData(@"`\x00\x09\x0d\x0a\x0b\x0C\x08`", true, "\0\t\r\n\v\f\b", "\\x00\\x09\\x0d\\x0a\\x0b\\x0C\\x08")]
    // Hexadecimal escape sequences (4 digits)
    [InlineData(@"`\u", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u0`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\uA`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\uf`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u00`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u0A`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u0f`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u000`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u00A`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u00f`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u000$`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u0000", false, "<Unexpected end of input>", null)]
    [InlineData(@"`\u0000`", true, "\0", "\\u0000")]
    [InlineData(@"`\u0009`", true, "\x09", "\\u0009")]
    [InlineData(@"`\u000A`", true, "\x0A", "\\u000A")]
    [InlineData(@"`\u000F`", true, "\x0F", "\\u000F")]
    [InlineData(@"`\u000G`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u000a`", true, "\x0A", "\\u000a")]
    [InlineData(@"`\u000f`", true, "\x0F", "\\u000f")]
    [InlineData(@"`\u000g`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\uffff`", true, "\xFFFF", "\\uffff")]
    [InlineData(@"`\ufffg`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u003f0`", true, "?0", "\\u003f0")]
    [InlineData(@"`\u0000\u0009\u000d\u000a\u000b\u000C\u0008`", true, "\0\t\r\n\v\f\b", "\\u0000\\u0009\\u000d\\u000a\\u000b\\u000C\\u0008")]
    // Hexadecimal escape sequences (Unicode code points)
    [InlineData(@"`\u{", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{0", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{0`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{}`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{ }`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{-1}`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{-0}`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{0}", false, "<Unexpected end of input>", null)]
    [InlineData(@"`\u{10FFFF}`", true, "\udbff\udfff", "\\u{10FFFF}")]
    [InlineData(@"`\u{110000}`", false, "<Undefined Unicode code-point>", null)]
    [InlineData(@"`\u{80000000}`", false, "<Undefined Unicode code-point>", null)]
    [InlineData(@"`\u{1.0}`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{1e10}`", true, "\u1E10", "\\u{1e10}")]
    [InlineData(@"`\u{.1e10}`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{a}`", true, "\x0A", "\\u{a}")]
    [InlineData(@"`\u{f}`", true, "\x0F", "\\u{f}")]
    [InlineData(@"`\u{g}`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{FFFF}`", true, "\xFFFF", "\\u{FFFF}")]
    [InlineData(@"`\u{FFFG}`", false, "<Invalid Unicode escape sequence>", null)]
    [InlineData(@"`\u{1F4A9}`", true, "💩", "\\u{1F4A9}")]
    [InlineData(@"`\u{D83D}\uDCA9`", true, "💩", "\\u{D83D}\\uDCA9")]
    [InlineData(@"`\uD83D\u{DCA9}`", true, "💩", "\\uD83D\\u{DCA9}")]
    [InlineData(@"`\u{0}\u{9}\u{d}\u{a}\u{b}\u{C}\u{00000000000000008}`", true, "\0\t\r\n\v\f\b", "\\u{0}\\u{9}\\u{d}\\u{a}\\u{b}\\u{C}\\u{00000000000000008}")]
    public void CanReadTemplate(string input, bool strict, string expectedCookedValue, string? expectedRawValue)
    {
        var tokens = new List<Token>();

        if (!(expectedCookedValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && expectedCookedValue.EndsWith(">", StringComparison.OrdinalIgnoreCase)))
        {
            var parser = new Parser(new ParserOptions { OnToken = (in Token t) => tokens!.Add(t) });
            parser.ParseExpression(input, strict: strict);
            Assert.True(tokens.Count >= 2);

            var token = tokens[0];
            Assert.Equal(TokenKind.Punctuator, token.Kind);
            Assert.Equal("`", token.StringValue);

            token = tokens[1];
            Assert.Equal(TokenKind.Template, token.Kind);
            Assert.Equal(expectedCookedValue, token.TemplateValue!.Value.Cooked);
            Assert.Equal(expectedRawValue ?? expectedCookedValue, token.TemplateValue!.Value.Raw);
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() =>
            {
                var parser = new Parser(new ParserOptions { OnToken = (in Token t) => tokens!.Add(t) });
                parser.ParseExpression(input, strict: strict);
            });

            var expectedMessage = expectedCookedValue.Substring(1, expectedCookedValue.Length - 2);
            Assert.Equal(expectedMessage, ex.Error.Description);
        }
    }
}
