using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;
using Acornima.Tests.Acorn;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public partial class TokenizerTests
{
    [Test]
    public void CanResetTokenizer()
    {
        var comments = new List<Comment>();
        var tokens = new List<Token>();
        var tokensDirect = new List<Token>();

        const string code = "var /* c1 */ foo=1; // c2";

        var tokenizer = new Tokenizer(code, new TokenizerOptions
        {
            OnComment = (in comment) => comments.Add(comment),
            OnToken = (in token) => tokens.Add(token)
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

            Assert.Equal(new string[] { "var", "foo", "=", "1", ";", "" }, tokens.Select(t => t.GetRawValue(code).ToString()));
            Assert.Equal(tokens, tokensDirect);

            Assert.Equal(new string[] { "/* c1 */", "// c2" }, comments.Select(c => c.GetRawValue(code).ToString()));
        }
    }

    [Test]
    public void CanResetTokenizerToCustomPosition()
    {
        var comments = new List<Comment>();
        var tokens = new List<Token>();
        var tokensDirect = new List<Token>();

        const string code = "var /* c1 */ foo=1; // c2";

        var tokenizer = new Tokenizer(code, new TokenizerOptions
        {
            OnComment = (in comment) => comments.Add(comment),
            OnToken = (in token) => tokens.Add(token)
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

        Assert.Equal(new string[] { "foo", "=", "1", ";", "" }, tokens.Select(t => t.GetRawValue(code).ToString()));
        Assert.Equal(tokens, tokensDirect);

        Assert.Equal(new string[] { "/* c1 */", "// c2" }, comments.Select(c => c.GetRawValue(code).ToString()));
    }

    [Test]
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

    [Test]
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

    [Arguments(@"\ud800")]
    [Arguments(@"\udc00")]
    [Arguments(@"\ud800\udc00")]
    [Arguments(@"\u{d800}")]
    [Arguments(@"\u{dc00}")]
    [Arguments(@"\u{d800}\u{dc00}")]
    [Test]
    public void ShouldRejectEscapedSurrogateAsIdentifierStart(string s)
    {
        var tokenizer = new Tokenizer(s);
        var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.Next());
        Assert.StartsWith("Invalid or unexpected token", ex.Error.Description);
    }

    [Arguments(@"a\ud800")]
    [Arguments(@"a\udc00")]
    [Arguments(@"a\ud800\udc00")]
    [Arguments(@"a\u{d800}")]
    [Arguments(@"a\u{dc00}")]
    [Arguments(@"a\u{d800}\u{dc00}")]
    [Test]
    public void ShouldRejectEscapedSurrogateAsIdentifierPart(string s)
    {
        var tokenizer = new Tokenizer(s);
        var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.Next());
        Assert.StartsWith("Invalid or unexpected token", ex.Error.Description);
    }

    [Test]
    public void ShouldAcceptSurrogateRangeInLiterals()
    {
        var tokenizer = new Tokenizer(@"'a\u{d800}\u{dc00}'");
        var token = tokenizer.GetToken();
        Assert.Equal(TokenKind.StringLiteral, token.Kind);
        Assert.Equal("a\ud800\udc00", token.StringValue);
    }

    [Test]
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

    [Test]
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

    [Test]
    public void IsIdentifierCharMatchesAcornImpl()
    {
        int cp;
        for (cp = 0; cp <= UnicodeHelper.LastCodePoint; cp++)
        {
            Assert.Equal(AcornIdentifier.IsIdentifierStart(cp, astral: true), Tokenizer.IsIdentifierStart(cp, allowAstral: true));
            Assert.Equal(AcornIdentifier.IsIdentifierChar(cp, astral: true), Tokenizer.IsIdentifierChar(cp, allowAstral: true));
        }
    }

    [Test]
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

    [Test]
    [Arguments("0xfedc_ba98_7654_3210", "18,364,758,544,493,064,720")]
    [Arguments("0xfedc_ba98_7654_3210n", "18,364,758,544,493,064,720")]
    [Arguments("0xFEDC_BA98_7654_3210", "18,364,758,544,493,064,720")]
    [Arguments("0xFEDC_BA98_7654_3210n", "18,364,758,544,493,064,720")]
    [Arguments("0XFEDC_BA98_7654_3210", "18,364,758,544,493,064,720")]
    [Arguments("0XFEDC_BA98_7654_3210n", "18,364,758,544,493,064,720")]
    [Arguments("0o17_7334_5651_4166_2503_1020", "18,364,758,544,493,064,720")]
    [Arguments("0o17_7334_5651_4166_2503_1020n", "18,364,758,544,493,064,720")]
    [Arguments("0O17_7334_5651_4166_2503_1020", "18,364,758,544,493,064,720")]
    [Arguments("0O17_7334_5651_4166_2503_1020n", "18,364,758,544,493,064,720")]
    [Arguments("0b11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "18,364,758,544,493,064,720")]
    [Arguments("0b11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "18,364,758,544,493,064,720")]
    [Arguments("0B11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "18,364,758,544,493,064,720")]
    [Arguments("0B11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "18,364,758,544,493,064,720")]

    [Arguments("0x1_FEDC_BA98_7654_3210", "36,811,502,618,202,616,336")]
    [Arguments("0x1_FEDC_BA98_7654_3210n", "36,811,502,618,202,616,336")]
    [Arguments("0o37_7334_5651_4166_2503_1020", "36,811,502,618,202,616,336")]
    [Arguments("0o37_7334_5651_4166_2503_1020n", "36,811,502,618,202,616,336")]
    [Arguments("0b1_11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "36,811,502,618,202,616,336")]
    [Arguments("0b1_11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "36,811,502,618,202,616,336")]

    [Arguments("0xFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF", ValueOf_255_F_HexDigits)]
    [Arguments("0xFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFFn", ValueOf_255_F_HexDigits)]
    [Arguments("0o7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777n", ValueOf_255_F_HexDigits)]
    [Arguments("0b1111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111n", ValueOf_255_F_HexDigits)]
    [Arguments("0xFFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF", "Infinity")]
    [Arguments("0xFFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFFn", ValueOf_256_F_HexDigits)]
    [Arguments("0o17_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777_7777n", ValueOf_256_F_HexDigits)]
    [Arguments("0b11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111n", ValueOf_256_F_HexDigits)]
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

        Unsafe.SkipInit(out BigInteger parsedBigIntegerValue);
        Unsafe.SkipInit(out double parsedDoubleValue);
        Assert.True(isBigInt
            ? Tokenizer.TryParseBigInt(input.SliceBetween(token.Start, token.End), allowSeparator: true, out parsedBigIntegerValue)
            : Tokenizer.TryParseNumber(input.SliceBetween(token.Start, token.End), allowSeparator: true, strict: true, out parsedDoubleValue));
        Assert.Equal(expectedValueObj, isBigInt ? (object)parsedBigIntegerValue : parsedDoubleValue);
    }

    [Test]
    [Arguments("0", false, "0")]
    [Arguments("0", true, "0")]
    [Arguments("0.", false, "0")]
    [Arguments("0.", true, "0")]
    [Arguments("0_", false, "<Numeric separator can not be used after leading 0>")]
    [Arguments("0_", true, "<Numeric separator can not be used after leading 0>")]
    [Arguments("0e", false, "<Invalid or unexpected token>")]
    [Arguments("0e", true, "<Invalid or unexpected token>")]
    [Arguments("0n", false, "0")]
    [Arguments("0n", true, "0")]
    [Arguments("00", false, "0")]
    [Arguments("00", true, "<Octal literals are not allowed in strict mode>")]
    [Arguments("00.", false, "0")]
    [Arguments("00.", true, "<Octal literals are not allowed in strict mode>")]
    [Arguments("00_", false, "<Invalid or unexpected token>")]
    [Arguments("00_", true, "<Invalid or unexpected token>")]
    [Arguments("00e", false, "<Invalid or unexpected token>")]
    [Arguments("00e", true, "<Invalid or unexpected token>")]
    [Arguments("00n", false, "<Invalid or unexpected token>")]
    [Arguments("00n", true, "<Invalid or unexpected token>")]
    [Arguments("00.1", false, "0")]
    [Arguments("00.1e-324", false, "0")]
    [Arguments("07", false, "7")]
    [Arguments("07", true, "<Octal literals are not allowed in strict mode>")]
    [Arguments("07n", false, "<Invalid or unexpected token>")]
    [Arguments("08", false, "8")]
    [Arguments("08", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("08n", false, "<Invalid or unexpected token>")]
    [Arguments("09", false, "9")]
    [Arguments("09", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("09n", false, "<Invalid or unexpected token>")]
    [Arguments("017", false, "15")]
    [Arguments("017", true, "<Octal literals are not allowed in strict mode>")]
    [Arguments("017n", false, "<Invalid or unexpected token>")]
    [Arguments("017.", false, "15")] // TODO: test parse error `var x = 017.;` vs `function f() {   return 017.; }()` 
    [Arguments("017.", true, "<Octal literals are not allowed in strict mode>")]
    [Arguments("017.1", false, "15")]
    [Arguments("017.1", true, "<Octal literals are not allowed in strict mode>")]
    [Arguments("017.e1", false, "15")]
    [Arguments("017.e1", true, "<Octal literals are not allowed in strict mode>")]
    [Arguments("017e", false, "<Invalid or unexpected token>")]
    [Arguments("017e", true, "<Invalid or unexpected token>")]
    [Arguments("018", false, "18")]
    [Arguments("018", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018n", false, "<Invalid or unexpected token>")]
    [Arguments("018.", false, "18")] // TODO: test parse error `var x = 018.;` vs `function f() {   return 018.; }()` 
    [Arguments("018.", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018.1", false, "18.1")]
    [Arguments("018.1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018.e1", false, "180")]
    [Arguments("018.e1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018_1", false, "<Invalid or unexpected token>")]
    [Arguments("018_1", true, "<Invalid or unexpected token>")]
    [Arguments("018e", false, "<Invalid or unexpected token>")]
    [Arguments("018e", true, "<Invalid or unexpected token>")]
    [Arguments("018.e", false, "<Invalid or unexpected token>")]
    [Arguments("018.e", true, "<Invalid or unexpected token>")]
    [Arguments("018e2", false, "1800")]
    [Arguments("018e2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018e+", false, "<Invalid or unexpected token>")]
    [Arguments("018e+", true, "<Invalid or unexpected token>")]
    [Arguments("018e+2", false, "1800")]
    [Arguments("018e+2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018e-", false, "<Invalid or unexpected token>")]
    [Arguments("018e-", true, "<Invalid or unexpected token>")]
    [Arguments("018e-1", false, "1.8")]
    [Arguments("018e-1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018.1e2", false, "1810")]
    [Arguments("018.1e2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018.1e+2", false, "1810")]
    [Arguments("018.1e+2", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("018.1e-1", false, "1.81")]
    [Arguments("018.1e-1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("019", false, "19")]
    [Arguments("019", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("019.e1", false, "190")]
    [Arguments("019.e1", true, "<Decimals with leading zeros are not allowed in strict mode>")]
    [Arguments("019_1", false, "<Invalid or unexpected token>")]
    [Arguments("019_1", true, "<Invalid or unexpected token>")]
    [Arguments("7", true, "7")]
    [Arguments("7n", true, "7")]
    [Arguments("17", true, "17")]
    [Arguments("17n", true, "17")]
    [Arguments("17.", true, "17")] // TODO: test parse error `var x = 17.;` vs `function f() {   return 17.; }(, null)`
    [Arguments("17.1", true, "17.1")]
    [Arguments("17.1n", true, "<Invalid or unexpected token>")]
    [Arguments("17e", false, "<Invalid or unexpected token>")]
    [Arguments("17.e", false, "<Invalid or unexpected token>")]
    [Arguments("17e2", true, "1700")]
    [Arguments("17e+", false, "<Invalid or unexpected token>")]
    [Arguments("17e+2", true, "1700")]
    [Arguments("17e-", false, "<Invalid or unexpected token>")]
    [Arguments("17e-1", true, "1.7")]
    [Arguments("17.1e2", true, "1710")]
    [Arguments("17.1e+2", true, "1710")]
    [Arguments("17.1e-1", true, "1.71")]
    [Arguments(".7", true, "0.7")]
    [Arguments(".7n", false, "<Invalid or unexpected token>")]
    [Arguments(".17", true, "0.17")]
    [Arguments(".17n", false, "<Invalid or unexpected token>")]
    [Arguments(".17.", true, "0.17")] // TODO: test parse error `var x = 17.;` vs `function f() {   return 17.; }()`
    [Arguments(".17.1", true, "0.17")]
    [Arguments(".17e", false, "<Invalid or unexpected token>")]
    [Arguments(".17.e", true, "0.17")]
    [Arguments(".17e2", true, "17")]
    [Arguments(".17e+", false, "<Invalid or unexpected token>")]
    [Arguments(".17e+2", true, "17")]
    [Arguments(".17e-", false, "<Invalid or unexpected token>")]
    [Arguments(".17e-1", true, "0.017")]
    [Arguments(".17.1e2", true, "0.17")]
    [Arguments(".17.1e+2", true, "0.17")]
    [Arguments(".17.1e-1", true, "0.17")]
    [Arguments("0777_7777", false, "<Invalid or unexpected token>")]
    [Arguments("0777_7777", true, "<Invalid or unexpected token>")]

    [Arguments("18_364_758_544_493_064_720", true, "1.8364758544493064e+19")]
    [Arguments("18_364_758_544_493_064_720n", true, "18_364_758_544_493_064_720")]
    [Arguments("36_811_502_618_202_616_336", true, "3.6811502618202616E+19")]
    [Arguments("36_811_502_618_202_616_336n", true, "36,811,502,618,202,616,336")]
    [Arguments("11_235_582_092_889_474_423_308_157_442_431_404_585_112_356_118_389_416_079_589_380_072_358_292_237_843_810_195_794_279_832_650_471_001_320_007_117_491_962_084_853_674_360_550_901_038_905_802_964_414_967_132_773_610_493_339_054_092_829_768_888_725_077_880_882_465_817_684_505_312_860_552_384_417_646_403_930_092_119_569_408_801_702_322_709_406_917_786_643_639_996_702_871_154_982_269_052_209_770_601_514_008_575", true, ValueOf_255_F_HexDigits)]
    [Arguments("11_235_582_092_889_474_423_308_157_442_431_404_585_112_356_118_389_416_079_589_380_072_358_292_237_843_810_195_794_279_832_650_471_001_320_007_117_491_962_084_853_674_360_550_901_038_905_802_964_414_967_132_773_610_493_339_054_092_829_768_888_725_077_880_882_465_817_684_505_312_860_552_384_417_646_403_930_092_119_569_408_801_702_322_709_406_917_786_643_639_996_702_871_154_982_269_052_209_770_601_514_008_575n", true, ValueOf_255_F_HexDigits)]
    [Arguments("179_769_313_486_231_590_772_930_519_078_902_473_361_797_697_894_230_657_273_430_081_157_732_675_805_500_963_132_708_477_322_407_536_021_120_113_879_871_393_357_658_789_768_814_416_622_492_847_430_639_474_124_377_767_893_424_865_485_276_302_219_601_246_094_119_453_082_952_085_005_768_838_150_682_342_462_881_473_913_110_540_827_237_163_350_510_684_586_298_239_947_245_938_479_716_304_835_356_329_624_224_137_215", true, "Infinity")]
    [Arguments("179_769_313_486_231_590_772_930_519_078_902_473_361_797_697_894_230_657_273_430_081_157_732_675_805_500_963_132_708_477_322_407_536_021_120_113_879_871_393_357_658_789_768_814_416_622_492_847_430_639_474_124_377_767_893_424_865_485_276_302_219_601_246_094_119_453_082_952_085_005_768_838_150_682_342_462_881_473_913_110_540_827_237_163_350_510_684_586_298_239_947_245_938_479_716_304_835_356_329_624_224_137_215n", true, ValueOf_256_F_HexDigits)]
    [Arguments("0.000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_01", true, "1e-323")]
    [Arguments("0.000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_001", true, "0")]
    [Arguments(".000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_01", true, "1e-323")]
    [Arguments(".000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_001", true, "0")]
    [Arguments("1.8_364_758_544_493_064_720e19", true, "1.8364758544493064e+19")]
    [Arguments("1.8_364_758_544_493_064_720e+19", true, "1.8364758544493064e+19")]
    [Arguments("1_8_364_758_544_493_064_720_000e-3", true, "1.8364758544493064e+19")]
    [Arguments("1_8_364_758_544_493_064_720_000.0e-3", true, "1.8364758544493064e+19")]
    [Arguments("1.8364758544493064720e1_9", true, "1.8364758544493064e+19")]
    [Arguments("1.1235582092889474E+307", true, ValueOf_255_F_HexDigits)]
    [Arguments("1.797_693_134_862_315_9e308", true, "Infinity")]
    [Arguments("1.797_693_134_862_315_9e+308", true, "Infinity")]
    [Arguments("1e-324", true, "0")]
    [Arguments("0.01e-322", true, "0")]
    [Arguments(".01e-322", true, "0")]

    [Arguments("018364758544493064720", false, "1.8364758544493064e+19")]
    [Arguments("018364758544493064720n", false, "<Invalid or unexpected token>")]
    [Arguments("018364758544493064720n", true, "<Invalid or unexpected token>")]
    [Arguments("036811502618202616336", false, "3.6811502618202616E+19")]
    [Arguments("011235582092889474423308157442431404585112356118389416079589380072358292237843810195794279832650471001320007117491962084853674360550901038905802964414967132773610493339054092829768888725077880882465817684505312860552384417646403930092119569408801702322709406917786643639996702871154982269052209770601514008575", false, ValueOf_255_F_HexDigits)]
    [Arguments("0179769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137215", false, "Infinity")]
    public void CanReadNumber(string input, bool strict, string expectedValue)
    {
        var tokenizer = new Tokenizer(input);
        var tokenizerContext = new TokenizerContext(strict);

        var isBigInt = input.AsSpan().Last() == 'n';

        if (!(expectedValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && expectedValue.EndsWith(">", StringComparison.OrdinalIgnoreCase)))
        {
            var token = tokenizer.GetToken(tokenizerContext);

            Assert.Equal(isBigInt ? TokenKind.BigIntLiteral : TokenKind.NumericLiteral, token.Kind);

            expectedValue = expectedValue.Replace("_", "");

            object expectedValueObj = !isBigInt
                ? double.Parse(expectedValue, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture)
                : BigInteger.Parse(expectedValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

            Assert.Equal(expectedValueObj, token.Value);

            Unsafe.SkipInit(out BigInteger parsedBigIntegerValue);
            Unsafe.SkipInit(out double parsedDoubleValue);
            Assert.True(isBigInt
                ? Tokenizer.TryParseBigInt(input.SliceBetween(token.Start, token.End), allowSeparator: true, out parsedBigIntegerValue)
                : Tokenizer.TryParseNumber(input.SliceBetween(token.Start, token.End), allowSeparator: true, strict, out parsedDoubleValue));
            Assert.Equal(expectedValueObj, isBigInt ? (object)parsedBigIntegerValue : parsedDoubleValue);
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.GetToken(tokenizerContext));

            var expectedMessage = expectedValue.Substring(1, expectedValue.Length - 2);
            Assert.Equal(expectedMessage, ex.Error.Description);

            Assert.False(isBigInt
                ? Tokenizer.TryParseBigInt(input, allowSeparator: true, out _)
                : Tokenizer.TryParseNumber(input, allowSeparator: true, strict: true, out _));
        }
    }

    [Test]
    [Arguments(@"""", false, "<Invalid or unexpected token>")]
    [Arguments(@"'", false, "<Invalid or unexpected token>")]
    [Arguments(@"""a", false, "<Invalid or unexpected token>")]
    [Arguments(@"'a", false, "<Invalid or unexpected token>")]
    [Arguments(@"""'", false, "<Invalid or unexpected token>")]
    [Arguments(@"'""", false, "<Invalid or unexpected token>")]
    [Arguments(@"""""", true, "")]
    [Arguments(@"''", true, "")]
    // Common escape sequences
    [Arguments(@"'\t\r\n\v\f\b'", true, "\t\r\n\v\f\b")]
    // Identity escape sequences
    [Arguments(@"'\cA'", true, "cA")]
    // New line escape sequences
    [Arguments("'\r", false, "<Invalid or unexpected token>")]
    [Arguments("'\r'", false, "<Invalid or unexpected token>")]
    [Arguments("'\n'", false, "<Invalid or unexpected token>")]
    [Arguments("'\r\n'", false, "<Invalid or unexpected token>")]
    [Arguments("'\u2028\u2029'", true, "\u2028\u2029")]
    [Arguments("'\\\r'", true, "")]
    [Arguments("'\\\n'", true, "")]
    [Arguments("'\\\r\\\n'", true, "")]
    [Arguments("'\\\u2028\\\u2029'", true, "")]
    [Arguments("'a\\\rb'", true, "ab")]
    [Arguments("'a\\\nb'", true, "ab")]
    [Arguments("'a\\\r\\\nb'", true, "ab")]
    [Arguments("'a\\\u2028\\\u2029b'", true, "ab")]
    // Octal escape sequences
    [Arguments(@"'\0'", false, "\0")]
    [Arguments(@"'\0'", true, "\0")]
    [Arguments(@"'\00'", false, "\0")]
    [Arguments(@"'\00'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [Arguments(@"'\000'", false, "\0")]
    [Arguments(@"'\000'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [Arguments(@"'\0000'", false, "\00")]
    [Arguments(@"'\7'", false, "\x07")]
    [Arguments(@"'\7'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [Arguments(@"'\07'", false, "\x07")]
    [Arguments(@"'\07'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [Arguments(@"'\007'", false, "\x07")]
    [Arguments(@"'\0007'", false, "\07")]
    [Arguments(@"'\8'", false, "8")]
    [Arguments(@"'\8'", true, "<\\8 and \\9 are not allowed in strict mode>")]
    [Arguments(@"'\08'", false, "\08")]
    [Arguments(@"'\08'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [Arguments(@"'\008'", false, "\08")]
    [Arguments(@"'\0008'", false, "\08")]
    [Arguments(@"'\9'", false, "9")]
    [Arguments(@"'\9'", true, "<\\8 and \\9 are not allowed in strict mode>")]
    [Arguments(@"'\09'", false, "\09")]
    [Arguments(@"'\09'", true, "<Octal escape sequences are not allowed in strict mode>")]
    [Arguments(@"'\77'", false, "\x3F")]
    [Arguments(@"'\077'", false, "\x3F")]
    [Arguments(@"'\0077'", false, "\x07\x37")]
    [Arguments(@"'\377'", false, "\xFF")]
    [Arguments(@"'\0377'", false, "\x1F\x37")]
    [Arguments(@"'\400'", false, "\x20\x30")]
    [Arguments(@"'\040'", false, "\x20")]
    // Hexadecimal escape sequences (2 digits)
    [Arguments(@"'\x", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\x'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\x0'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\xA'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\xf'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\x0$'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\x00", false, "<Invalid or unexpected token>")]
    [Arguments(@"'\x00'", true, "\0")]
    [Arguments(@"'\x09'", true, "\x09")]
    [Arguments(@"'\x0A'", true, "\x0A")]
    [Arguments(@"'\x0F'", true, "\x0F")]
    [Arguments(@"'\x0G'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\x0a'", true, "\x0A")]
    [Arguments(@"'\x0f'", true, "\x0F")]
    [Arguments(@"'\x0g'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\xff'", true, "\xFF")]
    [Arguments(@"'\xfg'", false, "<Invalid hexadecimal escape sequence>")]
    [Arguments(@"'\x3f0'", true, "?0")]
    [Arguments(@"'\x00\x09\x0d\x0a\x0b\x0C\x08'", true, "\0\t\r\n\v\f\b")]
    // Hexadecimal escape sequences (4 digits)
    [Arguments(@"'\u", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u0'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\uA'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\uf'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u00'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u0A'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u0f'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u000'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u00A'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u00f'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u000$'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u0000", false, "<Invalid or unexpected token>")]
    [Arguments(@"'\u0000'", true, "\0")]
    [Arguments(@"'\u0009'", true, "\x09")]
    [Arguments(@"'\u000A'", true, "\x0A")]
    [Arguments(@"'\u000F'", true, "\x0F")]
    [Arguments(@"'\u000G'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u000a'", true, "\x0A")]
    [Arguments(@"'\u000f'", true, "\x0F")]
    [Arguments(@"'\u000g'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\uffff'", true, "\xFFFF")]
    [Arguments(@"'\ufffg'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u003f0'", true, "?0")]
    [Arguments(@"'\u0000\u0009\u000d\u000a\u000b\u000C\u0008'", true, "\0\t\r\n\v\f\b")]
    // Hexadecimal escape sequences (Unicode code points)
    [Arguments(@"'\u{", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{0", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{0'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{}'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{ }'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{-1}'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{-0}'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{0}", false, "<Invalid or unexpected token>")]
    [Arguments(@"'\u{10FFFF}'", true, "\udbff\udfff")]
    [Arguments(@"'\u{110000}'", false, "<Undefined Unicode code-point>")]
    [Arguments(@"'\u{80000000}'", false, "<Undefined Unicode code-point>")]
    [Arguments(@"'\u{1.0}'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{1e10}'", true, "\u1E10")]
    [Arguments(@"'\u{.1e10}'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{a}'", true, "\x0A")]
    [Arguments(@"'\u{f}'", true, "\x0F")]
    [Arguments(@"'\u{g}'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{FFFF}'", true, "\xFFFF")]
    [Arguments(@"'\u{FFFG}'", false, "<Invalid Unicode escape sequence>")]
    [Arguments(@"'\u{1F4A9}'", true, "💩")]
    [Arguments(@"'\u{D83D}\uDCA9'", true, "💩")]
    [Arguments(@"'\uD83D\u{DCA9}'", true, "💩")]
    [Arguments(@"'\u{0}\u{9}\u{d}\u{a}\u{b}\u{C}\u{00000000000000008}'", true, "\0\t\r\n\v\f\b")]
    public void CanReadString(string input, bool strict, string expectedValue)
    {
        var tokenizer = new Tokenizer(input);
        var tokenizerContext = new TokenizerContext(strict);

        if (!(expectedValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && expectedValue.EndsWith(">", StringComparison.OrdinalIgnoreCase)))
        {
            var token = tokenizer.GetToken(tokenizerContext);

            Assert.Equal(TokenKind.StringLiteral, token.Kind);
            Assert.Equal(expectedValue, token.Value);

            Assert.True(Tokenizer.TryParseString(input.SliceBetween(token.Start, token.End), strict, out var parsedStringValue));
            Assert.Equal(expectedValue, parsedStringValue);
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => tokenizer.GetToken(tokenizerContext));

            var expectedMessage = expectedValue.Substring(1, expectedValue.Length - 2);
            Assert.Equal(expectedMessage, ex.Error.Description);
        }
    }

    [Test]
    [Arguments(@"`", false, "<Unexpected end of input>", null)]
    [Arguments(@"``", true, "", null)]
    [Arguments(@"`$", false, "<Unexpected end of input>", null)]
    [Arguments(@"`${", false, "<Unexpected end of input>", null)]
    [Arguments(@"`${x}", false, "<Unexpected end of input>", null)]
    [Arguments(@"`${x}`", true, "", null)]
    // Common escape sequences
    [Arguments(@"`\t\r\n\v\f\b`", true, "\t\r\n\v\f\b", "\\t\\r\\n\\v\\f\\b")]
    // Identity escape sequences
    [Arguments(@"`\cA`", true, "cA", "\\cA")]
    // New line escape sequences
    [Arguments("`\r", false, "<Unexpected end of input>", null)]
    [Arguments("`\r`", true, "\n", null)]
    [Arguments("`\n`", true, "\n", null)]
    [Arguments("`\r\n`", true, "\n", null)]
    [Arguments("`\u2028\u2029`", true, "\u2028\u2029", null)]
    [Arguments("`\\\r`", true, "", "\\\n")]
    [Arguments("`\\\n`", true, "", "\\\n")]
    [Arguments("`\\\r\\\n`", true, "", "\\\n\\\n")]
    [Arguments("`\\\u2028\\\u2029`", true, "", "\\\u2028\\\u2029")]
    [Arguments("`a\\\rb`", true, "ab", "a\\\nb")]
    [Arguments("`a\\\nb`", true, "ab", "a\\\nb")]
    [Arguments("`a\\\r\\\nb`", true, "ab", "a\\\n\\\nb")]
    [Arguments("`a\\\u2028\\\u2029b`", true, "ab", "a\\\u2028\\\u2029b")]
    // Octal escape sequences
    [Arguments(@"`\0`", false, "\0", "\\0")]
    [Arguments(@"`\0`", true, "\0", "\\0")]
    [Arguments(@"`\00`", false, "<Octal escape sequences are not allowed in template strings>", null)]
    [Arguments(@"`\00`", true, "<Octal escape sequences are not allowed in template strings>", null)]
    [Arguments(@"`\8`", false, "<\\8 and \\9 are not allowed in template strings>", null)]
    [Arguments(@"`\08`", false, "<Octal escape sequences are not allowed in template strings>", null)]
    [Arguments(@"`\9`", false, "<\\8 and \\9 are not allowed in template strings>", null)]
    [Arguments(@"`\09`", false, "<Octal escape sequences are not allowed in template strings>", null)]
    // Hexadecimal escape sequences (2 digits)
    [Arguments(@"`\x", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\x`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\x0`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\xA`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\xf`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\x0$`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\x00", false, "<Unexpected end of input>", null)]
    [Arguments(@"`\x00`", true, "\0", "\\x00")]
    [Arguments(@"`\x09`", true, "\x09", "\\x09")]
    [Arguments(@"`\x0A`", true, "\x0A", "\\x0A")]
    [Arguments(@"`\x0F`", true, "\x0F", "\\x0F")]
    [Arguments(@"`\x0G`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\x0a`", true, "\x0A", "\\x0a")]
    [Arguments(@"`\x0f`", true, "\x0F", "\\x0f")]
    [Arguments(@"`\x0g`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\xff`", true, "\xFF", "\\xff")]
    [Arguments(@"`\xfg`", false, "<Invalid hexadecimal escape sequence>", null)]
    [Arguments(@"`\x3f0`", true, "?0", "\\x3f0")]
    [Arguments(@"`\x00\x09\x0d\x0a\x0b\x0C\x08`", true, "\0\t\r\n\v\f\b", "\\x00\\x09\\x0d\\x0a\\x0b\\x0C\\x08")]
    // Hexadecimal escape sequences (4 digits)
    [Arguments(@"`\u", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u0`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\uA`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\uf`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u00`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u0A`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u0f`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u000`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u00A`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u00f`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u000$`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u0000", false, "<Unexpected end of input>", null)]
    [Arguments(@"`\u0000`", true, "\0", "\\u0000")]
    [Arguments(@"`\u0009`", true, "\x09", "\\u0009")]
    [Arguments(@"`\u000A`", true, "\x0A", "\\u000A")]
    [Arguments(@"`\u000F`", true, "\x0F", "\\u000F")]
    [Arguments(@"`\u000G`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u000a`", true, "\x0A", "\\u000a")]
    [Arguments(@"`\u000f`", true, "\x0F", "\\u000f")]
    [Arguments(@"`\u000g`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\uffff`", true, "\xFFFF", "\\uffff")]
    [Arguments(@"`\ufffg`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u003f0`", true, "?0", "\\u003f0")]
    [Arguments(@"`\u0000\u0009\u000d\u000a\u000b\u000C\u0008`", true, "\0\t\r\n\v\f\b", "\\u0000\\u0009\\u000d\\u000a\\u000b\\u000C\\u0008")]
    // Hexadecimal escape sequences (Unicode code points)
    [Arguments(@"`\u{", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{0", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{0`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{}`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{ }`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{-1}`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{-0}`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{0}", false, "<Unexpected end of input>", null)]
    [Arguments(@"`\u{10FFFF}`", true, "\udbff\udfff", "\\u{10FFFF}")]
    [Arguments(@"`\u{110000}`", false, "<Undefined Unicode code-point>", null)]
    [Arguments(@"`\u{80000000}`", false, "<Undefined Unicode code-point>", null)]
    [Arguments(@"`\u{1.0}`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{1e10}`", true, "\u1E10", "\\u{1e10}")]
    [Arguments(@"`\u{.1e10}`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{a}`", true, "\x0A", "\\u{a}")]
    [Arguments(@"`\u{f}`", true, "\x0F", "\\u{f}")]
    [Arguments(@"`\u{g}`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{FFFF}`", true, "\xFFFF", "\\u{FFFF}")]
    [Arguments(@"`\u{FFFG}`", false, "<Invalid Unicode escape sequence>", null)]
    [Arguments(@"`\u{1F4A9}`", true, "💩", "\\u{1F4A9}")]
    [Arguments(@"`\u{D83D}\uDCA9`", true, "💩", "\\u{D83D}\\uDCA9")]
    [Arguments(@"`\uD83D\u{DCA9}`", true, "💩", "\\uD83D\\u{DCA9}")]
    [Arguments(@"`\u{0}\u{9}\u{d}\u{a}\u{b}\u{C}\u{00000000000000008}`", true, "\0\t\r\n\v\f\b", "\\u{0}\\u{9}\\u{d}\\u{a}\\u{b}\\u{C}\\u{00000000000000008}")]
    public void CanReadTemplate(string input, bool strict, string expectedCookedValue, string? expectedRawValue)
    {
        var tokens = new List<Token>();

        if (!(expectedCookedValue.StartsWith("<", StringComparison.OrdinalIgnoreCase) && expectedCookedValue.EndsWith(">", StringComparison.OrdinalIgnoreCase)))
        {
            var parser = new Parser(new ParserOptions { OnToken = (in t) => tokens!.Add(t) });
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
                var parser = new Parser(new ParserOptions { OnToken = (in t) => tokens!.Add(t) });
                parser.ParseExpression(input, strict: strict);
            });

            var expectedMessage = expectedCookedValue.Substring(1, expectedCookedValue.Length - 2);
            Assert.Equal(expectedMessage, ex.Error.Description);
        }
    }
}
