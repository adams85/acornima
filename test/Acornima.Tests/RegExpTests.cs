using System;
using System.Collections.Generic;
using Acornima.Ast;
using Xunit;

namespace Acornima.Tests;

#pragma warning disable CS0618 // Type or member is obsolete

public partial class RegExpTests
{
    [Theory]
    [InlineData("/ab?c/u", false, false)]
    [InlineData("/ab?c/u", true, false)]
    [InlineData("/ab|?c/u", false, true)]
    [InlineData("/ab|?c/u", true, true)]
    [InlineData("/\\1a(b?)c/u", false, false)]
    [InlineData("/\\1a(b?)c/u", true, false)]
    public void ShouldRespectParserOptions(string expression, bool tolerant, bool expectError)
    {
        var parser = new Parser(new ParserOptions { Tolerant = tolerant });

        if (!expectError)
        {
            var expr = parser.ParseExpression(expression);

            Assert.IsType<RegExpLiteral>(expr);
        }
        else
        {
            Assert.ThrowsAny<ParseErrorException>(() => parser.ParseExpression(expression));
        }
    }

    [Theory]
    [InlineData("\\", "", nameof(SyntaxErrorMessages.RegExpEscapeAtEndOfPattern))]
    [InlineData("\\\\", "", null)]
    [InlineData("\\\\\\", "", nameof(SyntaxErrorMessages.RegExpEscapeAtEndOfPattern))]
    [InlineData("a\\", "", nameof(SyntaxErrorMessages.RegExpEscapeAtEndOfPattern))]
    [InlineData("ab?c", "u", null)]
    [InlineData("ab|?c", "u", nameof(SyntaxErrorMessages.RegExpNothingToRepeat))]
    [InlineData("\\1a(b?)c", "u", null)]
    [InlineData("ab?c", "v", null)]
    [InlineData("ab|?c", "v", nameof(SyntaxErrorMessages.RegExpNothingToRepeat))]
    [InlineData("\\1a(b?)c", "v", null)]
    public void ValidateRegExpShouldWork(string pattern, string flags, string? expectedErrorCode)
    {
        Assert.Equal(expectedErrorCode is null, Tokenizer.ValidateRegExp(pattern, flags, out var error));
        if (expectedErrorCode is null)
        {
            Assert.Null(error);
        }
        else
        {
            Assert.NotNull(error);
            Assert.Equal(expectedErrorCode, error.Code);
        }
    }

    [Theory]
    [InlineData("\\p{L}", "", null)]
    [InlineData("\\p{L}", "u", nameof(SyntaxErrorMessages.RegExpInvalidEscape))]
    [InlineData("[\\p{L}]", "", null)]
    [InlineData("[\\p{L}]", "u", nameof(SyntaxErrorMessages.RegExpInvalidEscape))]
    public void ShouldRejectUnicodeCharacterClassEscapesBeforeES2018(string pattern, string flags, string? expectedErrorCode)
    {
        var parser = CreateRegExpParser(pattern, flags, new TokenizerOptions { EcmaVersion = EcmaVersion.ES2017 });

        if (expectedErrorCode is null)
        {
            parser.Validate();
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parser.Validate());
            Assert.Equal(expectedErrorCode, ex.Error.Code);
        }
    }

    [Theory]
    [InlineData("(?<a>)\\k<a>", "", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("(?<a>)\\k<a>", "u", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("\\k<a>(?<a>)", "", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("\\k<a>(?<a>)", "u", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("(?<a>)[\\k<a>]", "", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("(?<a>)[\\k<a>]", "u", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("[\\k<a>](?<a>)", "", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("[\\k<a>](?<a>)", "u", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("\\2(?<a>)", "", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    [InlineData("\\2(?<a>)", "u", nameof(SyntaxErrorMessages.RegExpInvalidGroup))]
    public void ShouldRejectNamedBackreferencesBeforeES2018(string pattern, string flags, string expectedErrorCode)
    {
        var parser = CreateRegExpParser(pattern, flags, new TokenizerOptions { EcmaVersion = EcmaVersion.ES2017 });

        var ex = Assert.Throws<SyntaxErrorException>(() => parser.Validate());
        Assert.Equal(expectedErrorCode, ex.Error.Code);
    }

    [Theory]
    [InlineData("\\1\\k<a\\u{61}>(?<a\\u{61}>)", "", nameof(SyntaxErrorMessages.RegExpInvalidCaptureGroupName))]
    [InlineData("\\1\\k<a\\u{61}>(?<a\\u{61}>)", "u", null)]
    public void ShouldRejectAstralUnicodeEscapeInNamedBackreferencesBeforeES2020(string pattern, string flags, string? expectedErrorCode)
    {
        var parser = CreateRegExpParser(pattern, flags, new TokenizerOptions { EcmaVersion = EcmaVersion.ES2019 });

        if (expectedErrorCode is null)
        {
            parser.Validate();
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parser.Validate());
            Assert.Equal(expectedErrorCode, ex.Error.Code);
        }
    }

    [Theory]
    [InlineData("(?<a>x)|(?<a>y)", "u")]
    [InlineData("((?<a>x))|(?<a>y)", "u")]
    [InlineData("(?:(?<a>x))|(?<a>y)", "u")]
    [InlineData("(?<!(?<a>x))|(?<a>y)", "u")]
    [InlineData("(?<a>x)|((?<a>y))", "u")]
    [InlineData("(?<a>x)|(?:(?<a>y))", "u")]
    [InlineData("(?<a>x)|(?!(?<a>y))", "u")]
    [InlineData("(?<a>x)|(?<a>y)|(?<a>z)", "u")]
    [InlineData("((?<a>x)|(?<a>y))|(?<a>z)", "u")]
    [InlineData("(?<a>x)|((?<a>y)|(?<a>z))", "u")]
    [InlineData("(?<a>x)|(((?<a>y)))|(?<a>z)", "u")]
    public void ShouldRejectDuplicateGroupNamesInAlternatesBeforeES2025(string pattern, string flags)
    {
        var parser = CreateRegExpParser(pattern, flags, new TokenizerOptions { EcmaVersion = EcmaVersion.ES2024 });

        Assert.Throws<SyntaxErrorException>(() => parser.Validate());
    }

    // === RegExp Modifiers: Feature gating test ===
    // Verify that modifier syntax is rejected when the feature is not enabled.

    [Fact]
    public void ShouldRejectModifierSyntaxWhenFeatureDisabled()
    {
        var parser = CreateRegExpParser("(?i:abc)", "", new TokenizerOptions
        {
            EcmaVersion = EcmaVersion.ES2024,
            ExperimentalESFeatures = ExperimentalESFeatures.None,
        });

        Assert.Throws<SyntaxErrorException>(() => parser.Validate());
    }

    [Fact]
    public void ShouldRejectModifierSyntaxWhenFeatureEnabledButTargetingPreES2018()
    {
        var parser = CreateRegExpParser("(?i:abc)", "", new TokenizerOptions
        {
            EcmaVersion = EcmaVersion.ES2017,
            ExperimentalESFeatures = ExperimentalESFeatures.RegExpModifiers,
        });

        Assert.Throws<SyntaxErrorException>(() => parser.Validate());
    }

    private static Tokenizer.RegExpParser CreateRegExpParser(string pattern, string flags, TokenizerOptions tokenizerOptions)
    {
        var tokenizer = new Tokenizer(string.Empty, tokenizerOptions);
        var regExpParser = tokenizer._regExpParser ??= new Tokenizer.RegExpParser(tokenizer);
        regExpParser.Reset(pattern, patternStartIndex: 0, flags, flagsStartIndex: 0);
        return regExpParser;
    }

    [Fact]
    public void FlagV_IsMutuallyExclusiveWithUFlag()
    {
        var parser = new Parser();
        Assert.ThrowsAny<SyntaxErrorException>(() => parser.ParseExpression("/abc/uv"));
    }

    [Fact]
    public void FlagV_IsSyntaxErrorBeforeES2024()
    {
        var parser = new Parser(new ParserOptions
        {
            EcmaVersion = EcmaVersion.ES2023
        });
        Assert.ThrowsAny<SyntaxErrorException>(() => parser.ParseExpression("/abc/v"));
    }

    [Fact]
    public void FlagV_ThrowsCatchableExceptionOnTooDeepRecursion_WhenParsing()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = $"/{new string('[', depth)}{new string(']', depth)}/v";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void FlagV_ThrowsCatchableExceptionOnTooDeepRecursion_WhenTokenizing()
    {
        const int depth = 100_000;
        var input = $"/{new string('[', depth)}{new string(']', depth)}/v";
        var tokenizer = new Tokenizer(input);
        Assert.Throws<InsufficientExecutionStackException>(() => tokenizer.Next());
    }

    [Fact]
    public void CanSkipInvalidRegExp()
    {
        var input = $"s\n  .match(u ? /[]]/u : /[]]/)";
        const string sourceFile = "main.js";

        var capturedContexts = new List<(string, string, Range, SourceLocation)>();
        var tokens = new List<Token>();

        var tokenizer = new Tokenizer(input, SourceType.Script, sourceFile, new TokenizerOptions
        {
            Tolerant = false,
            OnRegExp = (in ctx) =>
            {
                capturedContexts.Add((ctx.Pattern, ctx.Flags, ctx.RangeRef(), ctx.LocationRef()));
                return default;
            }
        });

        Token token;

        do { tokens.Add(token = tokenizer.GetToken()); }
        while (token.Kind != TokenKind.EOF);

        tokens.RemoveAll(token => token.Kind != TokenKind.RegExpLiteral);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(2, capturedContexts.Count);

        var capturedContext = capturedContexts[0];
        token = tokens[0];
        Assert.Equal("[]]", capturedContext.Item1);
        Assert.Equal("u", capturedContext.Item2);
        Assert.NotNull(token.RegExpValue);
        Assert.Equal(capturedContext.Item1, token.RegExpValue.Value.Pattern);
        Assert.Equal(capturedContext.Item2, token.RegExpValue.Value.Flags);

        capturedContext = capturedContexts[1];
        token = tokens[1];
        Assert.Equal("[]]", capturedContext.Item1);
        Assert.Equal("", capturedContext.Item2);
        Assert.NotNull(token.RegExpValue);
        Assert.Equal(capturedContext.Item1, token.RegExpValue.Value.Pattern);
        Assert.Equal(capturedContext.Item2, token.RegExpValue.Value.Flags);
    }

    // TODO
    //    [Theory]
    //    [InlineData(@"[]", "", null, 0)]
    //    [InlineData(@"[]", "x", nameof(SyntaxErrorMessages.InvalidRegExpFlags), 4)]
    //    [InlineData(@"[]", "uv", nameof(SyntaxErrorMessages.InvalidRegExpFlags), 4)]
    //    [InlineData(@"[]]", "su", nameof(SyntaxErrorMessages.RegExpLoneQuantifierBrackets), 3)]
    //    [InlineData(@"[\p{sc=Greek}]", "u", "RegExpConversionFailed", 2)]
    //    public void CanHookIntoRegExpParsing(string pattern, string flags, string? expectedErrorCode, int expectedErrorIndex)
    //    {
    //        var input = $"s\n  .match(/{pattern}/{flags})";
    //        const string sourceFile = "main.js";

    //        var capturedContexts = new List<(string, string, Range, SourceLocation)>();
    //        var tokens = new List<Token>();

    //        var tokenizer = new Tokenizer(input, SourceType.Script, sourceFile, new TokenizerOptions
    //        {
    //            Tolerant = true,
    //            OnRegExp = (in ctx) =>
    //            {
    //                capturedContexts.Add((ctx.Pattern, ctx.Flags, ctx.Range, ctx.Location));
    //                try
    //                {
    //                    var result = Tokenizer.AdaptRegExp(ctx.Pattern, ctx.Flags);
    //                    if (result.ConversionError is null)
    //                    {
    //                        return RegExpParseResult.ForSuccess(result.Regex, result.AdditionalData);
    //                    }
    //                    else
    //                    {
    //                        var index = ctx.Range.Start + 1 + result.ConversionError.Index;
    //                        var parseError = ctx.ReportRecoverableError(index,
    //                            "Conversion failed.", RegExpConversionError.s_factory, "RegExpConversionFailed");
    //                        return RegExpParseResult.ForFailure(parseError);
    //                    }
    //                }
    //                catch (SyntaxErrorException ex)
    //                {
    //                    var index = ctx.Range.Start + 1
    //                        + (ex.Error.Code == nameof(SyntaxErrorMessages.InvalidRegExpFlags) ? ctx.Pattern.Length + 1 : 0)
    //                        + ex.Error.Index;
    //                    ctx.ReportSyntaxError(index, ex.Error.Description, ex.Error.Code);
    //                    throw ex; // unreachable, just to keep the compiler happy
    //                }
    //            }
    //        });

    //        Token token;
    //        ParseError? parseError;
    //        var expectSyntaxError = expectedErrorCode is not (null or "RegExpConversionFailed");

    //        try
    //        {
    //            do { tokens.Add(token = tokenizer.GetToken()); }
    //            while (token.Kind != TokenKind.EOF);

    //            Assert.False(expectSyntaxError);

    //            token = Assert.Single(tokens, token => token.Kind == TokenKind.RegExpLiteral);
    //            Assert.NotNull(token.RegExpParseResult);
    //            if (expectedErrorCode is null)
    //            {
    //                parseError = null;
    //                Assert.Null(token.RegExpParseResult.Value.ConversionError);
    //                Assert.IsType<Regex>(token.RegExpParseResult.Value.ConversionResult);
    //                Assert.NotNull(token.RegExpParseResult.Value.Regex);
    //                Assert.NotNull(token.RegExpParseResult.Value.AdditionalData);
    //            }
    //            else
    //            {
    //                parseError = token.RegExpParseResult.Value.ConversionError;
    //                Assert.NotNull(parseError);
    //                Assert.Null(token.RegExpParseResult.Value.ConversionResult);
    //                Assert.Null(token.RegExpParseResult.Value.Regex);
    //                Assert.Null(token.RegExpParseResult.Value.AdditionalData);
    //            }
    //        }
    //        catch (SyntaxErrorException ex)
    //        {
    //            Assert.True(expectSyntaxError);

    //            token = default;
    //            parseError = ex.Error;
    //        }

    //        var capturedContext = Assert.Single(capturedContexts);
    //        Assert.Equal(pattern, capturedContext.Item1);
    //        Assert.Equal(flags, capturedContext.Item2);
    //        var regExpLength = 1 + pattern.Length + 1 + flags.Length;
    //        Assert.Equal(new Range(11, 11 + regExpLength), capturedContext.Item3);
    //#if !NET462
    //        Assert.Equal($"/{pattern}/{flags}", input[capturedContext.Item3.ToSystemRange()]);
    //#endif
    //        Assert.Equal(new SourceLocation(new Position(2, 9), new Position(2, 9 + regExpLength), sourceFile), capturedContext.Item4);

    //        if (token.Kind != TokenKind.Unknown)
    //        {
    //            Assert.NotNull(token.RegExpValue);
    //            Assert.Equal(capturedContext.Item1, token.RegExpValue.Value.Pattern);
    //            Assert.Equal(capturedContext.Item2, token.RegExpValue.Value.Flags);
    //            Assert.Equal(capturedContext.Item3, token.Range);
    //            Assert.Equal(capturedContext.Item4, token.Location);
    //            Assert.NotNull(token.RegExpParseResult);
    //        }

    //        if (parseError is not null)
    //        {
    //            Assert.Equal(11 + expectedErrorIndex, parseError.Index);
    //            Assert.Equal(new Position(2, 9 + expectedErrorIndex), parseError.Position);
    //        }
    //    }
}
