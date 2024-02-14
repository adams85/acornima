using System;
using System.Text.RegularExpressions;
using Acornima.Ast;
using Xunit;

namespace Acornima.Tests;

public partial class RegExpTests
{
    [Theory]
    [InlineData("/ab?c/u", RegExpParseMode.Skip, false, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.Skip, true, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.Validate, false, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.Validate, true, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToInterpreted, false, false, false)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToInterpreted, true, false, false)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToCompiled, false, false, true)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToCompiled, true, false, true)]
    [InlineData("/ab|?c/u", RegExpParseMode.Skip, false, false, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.Skip, true, false, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.Validate, false, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.Validate, true, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToInterpreted, false, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToInterpreted, true, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToCompiled, false, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToCompiled, true, true, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Skip, false, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Skip, true, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Validate, false, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Validate, true, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToInterpreted, false, true, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToInterpreted, true, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToCompiled, false, true, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToCompiled, true, false, null)]
    public void ShouldRespectParserOptions(string expression, RegExpParseMode parseMode, bool tolerant, bool expectError, bool? expectCompiled)
    {
        var matchTimeout = TimeSpan.FromMilliseconds(1234);

        var parser = new Parser(new ParserOptions { RegExpParseMode = parseMode, RegexTimeout = matchTimeout, Tolerant = tolerant });

        if (!expectError)
        {
            var expr = parser.ParseExpression(expression);

            Assert.IsType<RegExpLiteral>(expr);

            var regex = expr.As<RegExpLiteral>().Value;
            if (expectCompiled is not null)
            {
                Assert.NotNull(regex);
                Assert.Equal(expectCompiled.Value, regex.Options.HasFlag(RegexOptions.Compiled));
                Assert.Equal(matchTimeout, regex.MatchTimeout);
            }
            else
            {
                Assert.Null(regex);
            }
        }
        else
        {
            Assert.ThrowsAny<ParseErrorException>(() => parser.ParseExpression(expression));
        }
    }

    [InlineData("ab?c", "u", false, false, false, false)]
    [InlineData("ab?c", "u", false, true, false, false)]
    [InlineData("ab?c", "u", true, false, false, true)]
    [InlineData("ab?c", "u", true, true, false, true)]
    [InlineData("ab|?c", "u", false, false, true, null)]
    [InlineData("ab|?c", "u", false, true, true, null)]
    [InlineData("ab|?c", "u", true, false, true, null)]
    [InlineData("ab|?c", "u", true, true, true, null)]
    [InlineData("\\1a(b?)c", "u", false, false, false, null)]
    [InlineData("\\1a(b?)c", "u", false, true, true, null)]
    [InlineData("\\1a(b?)c", "u", true, false, false, null)]
    [InlineData("\\1a(b?)c", "u", true, true, true, null)]
    [Theory]
    public void AdaptRegExpShouldRespectParameters(string pattern, string flags, bool compiled, bool throwIfNotAdaptable, bool expectError, bool? expectCompiled)
    {
        var matchTimeout = TimeSpan.FromMilliseconds(1234);

        if (!expectError)
        {
            var regex = Tokenizer.AdaptRegExp(pattern, flags, compiled, matchTimeout, throwIfNotAdaptable).Regex;
            if (expectCompiled is not null)
            {
                Assert.NotNull(regex);
                Assert.Equal(expectCompiled.Value, regex.Options.HasFlag(RegexOptions.Compiled));
                Assert.Equal(matchTimeout, regex.MatchTimeout);
            }
            else
            {
                Assert.Null(regex);
            }
        }
        else
        {
            Assert.ThrowsAny<ParseErrorException>(() => Tokenizer.AdaptRegExp(pattern, flags, compiled, matchTimeout, throwIfNotAdaptable));
        }
    }

    [InlineData("ab?c", "u", true)]
    [InlineData("ab|?c", "u", false)]
    [InlineData("\\1a(b?)c", "u", true)]
    [Theory]
    public void ValidateRegExpShouldWork(string pattern, string flags, bool expectedResult)
    {
        Assert.Equal(expectedResult, Tokenizer.ValidateRegExp(pattern, flags, out var error));
        if (expectedResult)
        {
            Assert.Null(error);
        }
        else
        {
            Assert.NotNull(error);
        }
    }

    [InlineData("(?<a>x)|(?<a>y)", "u", "((?<a>x))|((?<a>y))")]
    [InlineData("((?<a>x))|(?<a>y)", "u", "(((?<a>x)))|((?<a>y))")]
    [InlineData("(?:(?<a>x))|(?<a>y)", "u", "(?:((?<a>x)))|((?<a>y))")]
    [InlineData("(?<!(?<a>x))|(?<a>y)", "u", "(?<!((?<a>x)))|((?<a>y))")]
    [InlineData("(?<a>x)|((?<a>y))", "u", "((?<a>x))|(((?<a>y)))")]
    [InlineData("(?<a>x)|(?:(?<a>y))", "u", "((?<a>x))|(?:((?<a>y)))")]
    [InlineData("(?<a>x)|(?!(?<a>y))", "u", "((?<a>x))|(?!((?<a>y)))")]
    [InlineData("(?<a>x)|(?<a>y)|(?<a>z)", "u", "((?<a>x))|((?<a>y))|((?<a>z))")]
    [InlineData("((?<a>x)|(?<a>y))|(?<a>z)", "u", "(((?<a>x))|((?<a>y)))|((?<a>z))")]
    [InlineData("(?<a>x)|((?<a>y)|(?<a>z))", "u", "((?<a>x))|(((?<a>y))|((?<a>z)))")]
    [InlineData("(?<a>x)|(((?<a>y)))|(?<a>z)", "u", "((?<a>x))|((((?<a>y))))|((?<a>z))")]
    [Theory]
    public void ShouldAllowDuplicateGroupNamesInAlternates(string pattern, string flags, string expectedAdaptedPattern)
    {
        // TODO: Generate these tests when Duplicate named capturing groups (https://github.com/tc39/proposal-duplicate-named-capturing-groups) gets implemented in V8.

        var parser = new Tokenizer.RegExpParser(pattern, flags, new TokenizerOptions
        {
            EcmaVersion = EcmaVersion.Experimental,
            RegExpParseMode = RegExpParseMode.AdaptToInterpreted,
            Tolerant = false
        });
        var actualAdaptedPattern = parser.ParseCore(out _, out _);

        Assert.Equal(expectedAdaptedPattern, actualAdaptedPattern);
    }
}
