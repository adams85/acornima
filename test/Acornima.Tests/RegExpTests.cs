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

        var parser = CreateRegExpParser(pattern, flags, new TokenizerOptions
        {
            ExperimentalESFeatures = ExperimentalESFeatures.RegExpDuplicateNamedCapturingGroups,
            RegExpParseMode = RegExpParseMode.AdaptToInterpreted,
            Tolerant = false
        });
        var actualAdaptedPattern = parser.ParseCore(out _, out _, out _);

        Assert.Equal(expectedAdaptedPattern, actualAdaptedPattern);
    }

    // Conversion tests for modifiers involving multiline/dotAll that produce patterns with
    // literal newline characters (\n, \r, \u2028, \u2029) — these can't be expressed in InlineData.
    [Fact]
    public void ShouldConvertRegExpModifiers_MultilineAndDotAll()
    {
        var nl = "[\n\r\u2028\u2029]";
        var noNl = "[^\n\r\u2028\u2029]";

        // multiline modifier: ^/$ get rewritten to lookaround inside the group, plain outside
        AssertConversion("(?m:^a$)", "", $"(?:(?<={nl}|^)a(?={nl}|$))");
        AssertConversion("^(?m:^a$)$", "", $"^(?:(?<={nl}|^)a(?={nl}|$))$");

        // dotAll modifier: . gets rewritten to [\s\S] inside the group, [^\n\r\u2028\u2029] outside
        AssertConversion("(?s:.).", "", $"(?:[\\s\\S]){noNl}");
        AssertConversion(".(?s:.).", "", $"{noNl}(?:[\\s\\S]){noNl}");

        // remove dotAll inside the group when globally enabled
        AssertConversion("(?-s:.).", "s", $"(?:{noNl})[\\s\\S]");

        // combined add modifiers
        AssertConversion("(?im:^a)", "", $"(?i:(?<={nl}|^)a)");

        // add and remove modifiers
        AssertConversion("(?m-i:^a)", "i", $"(?-i:(?<={nl}|^)a)");

        // empty remove (e.g., (?s-:...)) is valid
        AssertConversion("(?s-:.).", "", $"(?:[\\s\\S]){noNl}");

        // nested modifier groups: dotAll scope restored on exit
        AssertConversion("(?s:(?-s:.).).", "", $"(?:(?:{noNl})[\\s\\S]){noNl}");

        // nested modifier groups: multiline scope restored on exit
        AssertConversion("(?m:(?-m:^)^)", "", $"(?:(?:^)(?<={nl}|^))");

        // deeply nested modifier combination
        AssertConversion("(?i:(?s:(?m:^.a)))", "", $"(?i:(?:(?:(?<={nl}|^)[\\s\\S]a)))");

        // sequential modifier groups
        AssertConversion("(?i:a)(?s:.)", "", $"(?i:a)(?:[\\s\\S])");

        // remove multiline inside globally-enabled multiline
        AssertConversion("(?-m:^a)", "m", "(?:^a)");

        static void AssertConversion(string pattern, string flags, string expected)
        {
            var parser = CreateRegExpParser(pattern, flags, new TokenizerOptions
            {
                ExperimentalESFeatures = ExperimentalESFeatures.RegExpModifiers,
                RegExpParseMode = RegExpParseMode.AdaptToInterpreted,
                Tolerant = false
            });
            var actual = parser.ParseCore(out _, out _, out _);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void ShouldNotAffectNonCapturingGroupsWhenModifiersEnabled()
    {
        var parser = CreateRegExpParser("(?:a)", "", new TokenizerOptions
        {
            ExperimentalESFeatures = ExperimentalESFeatures.RegExpModifiers,
            RegExpParseMode = RegExpParseMode.AdaptToInterpreted,
            Tolerant = false
        });

        var result = parser.Parse();
        Assert.True(result.Success);
        Assert.Equal("(?:a)", result.Regex!.ToString());
    }

    // === RegExp Modifiers: Matching behavior tests ===
    // Verify that the converted regex actually matches correctly.

    // ignoreCase modifier
    [InlineData("(?i:abc)", "", "ABC", true)]
    [InlineData("(?i:abc)", "", "abc", true)]
    [InlineData("a(?i:b)c", "", "aBc", true)]
    [InlineData("a(?i:b)c", "", "ABc", false)]   // 'a' is case-sensitive outside group
    [InlineData("a(?i:b)c", "", "aBC", false)]   // 'c' is case-sensitive outside group
    [InlineData("(?-i:abc)", "i", "ABC", false)]  // remove ignoreCase
    [InlineData("(?-i:abc)", "i", "abc", true)]
    // dotAll modifier
    [InlineData("(?s:.)", "", "\n", true)]         // dot matches newline inside group
    [InlineData("(?s:.)a", "", "\na", true)]
    [InlineData("a(?s:.)b", "", "a\nb", true)]
    [InlineData("a.b", "", "a\nb", false)]         // dot outside doesn't match newline
    [InlineData("(?-s:.)", "s", "\n", false)]      // remove dotAll
    [InlineData("(?-s:.)", "s", "a", true)]
    // multiline modifier
    [InlineData("(?m:^a)", "", "b\na", true)]      // ^ matches after newline inside group
    [InlineData("(?m:a$)", "", "a\nb", true)]      // $ matches before newline inside group
    [InlineData("^a", "", "b\na", false)]           // ^ outside doesn't match after newline
    // empty remove syntax
    [InlineData("(?s-:.).", "", "\na", true)]
    [InlineData("(?m-:^a)", "", "b\na", true)]
    // nested modifiers
    [InlineData("(?s:(?-s:.).).", "", "a\na", true)]   // outer . matches \n, inner . doesn't
    [InlineData("(?s:(?-s:.).).", "", "\n\na", false)]  // inner . doesn't match \n
    // alternation inside modifier group
    [InlineData("(?i:a|B)", "", "a", true)]
    [InlineData("(?i:a|B)", "", "b", true)]
    [InlineData("(?i:a|B)", "", "A", true)]
    [InlineData("(?i:a|B)", "", "B", true)]
    // quantified modifier group
    [InlineData("(?i:a)+", "", "aAaA", true)]
    [InlineData("(?i:a)*", "", "", true)]
    // empty modifier group content
    [InlineData("(?i:)", "", "", true)]
    // character class inside modifier group
    [InlineData("(?i:[a-z])", "", "A", true)]
    [InlineData("(?i:[a-z])", "", "Z", true)]
    [Theory]
    public void ShouldMatchRegExpModifiers(string pattern, string flags, string input, bool expectedMatch)
    {
        var parser = CreateRegExpParser(pattern, flags, new TokenizerOptions
        {
            ExperimentalESFeatures = ExperimentalESFeatures.RegExpModifiers,
            RegExpParseMode = RegExpParseMode.AdaptToInterpreted,
            Tolerant = false
        });
        var parseResult = parser.Parse();
        Assert.True(parseResult.Success);
        Assert.NotNull(parseResult.Regex);

        Assert.Equal(expectedMatch, parseResult.Regex.IsMatch(input));
    }

    // === RegExp Modifiers: Feature gating test ===
    // Verify that modifier syntax is rejected when the feature is not enabled.

    [Fact]
    public void ShouldRejectModifierSyntaxWhenFeatureDisabled()
    {
        var parser = CreateRegExpParser("(?i:abc)", "", new TokenizerOptions
        {
            ExperimentalESFeatures = ExperimentalESFeatures.None,
            RegExpParseMode = RegExpParseMode.Validate,
            Tolerant = false
        });

        Assert.Throws<SyntaxErrorException>(() => parser.Parse());
    }

    [Fact]
    public void ShouldRejectModifierSyntaxWhenFeatureEnabledButTargetingPreES2018()
    {
        var parser = CreateRegExpParser("(?i:abc)", "", new TokenizerOptions
        {
            EcmaVersion = EcmaVersion.ES2017,
            ExperimentalESFeatures = ExperimentalESFeatures.RegExpModifiers,
            RegExpParseMode = RegExpParseMode.Validate,
            Tolerant = false
        });

        Assert.Throws<SyntaxErrorException>(() => parser.Parse());
    }

    [Theory]
    [InlineData(@"(?:x)", false, false)]
    [InlineData(@"(?![^\\x28]*\\x29)", false, false)]
    [InlineData(@"(?<!(Saturday|Sunday))", false, false)]
    [InlineData(@"(?:x)", true, true)]
#if NET9_0_OR_GREATER
    [InlineData(@"(?![^\\x28]*\\x29)", true, true)]
    [InlineData(@"(?<!(Saturday|Sunday))", true, true)]
#elif NET7_0_OR_GREATER
    [InlineData(@"(?![^\\x28]*\\x29)", true, false)]
    [InlineData(@"(?<!(Saturday|Sunday))", true, false)]
#else
    [InlineData(@"(?![^\\x28]*\\x29)", true, true)]
    [InlineData(@"(?<!(Saturday|Sunday))", true, true)]
#endif
    public void ShouldNotCompileNegativeLookaroundOnNET7OrLater(string pattern, bool compileRegex, bool expectedIsCompiled)
    {
        var parser = CreateRegExpParser(pattern, string.Empty, new TokenizerOptions
        {
            RegExpParseMode = compileRegex ? RegExpParseMode.AdaptToCompiled : RegExpParseMode.AdaptToInterpreted,
            Tolerant = false
        });
        var parseResult = parser.Parse();
        Assert.True(parseResult.Success);
        Assert.NotNull(parseResult.Regex);
        Assert.Equal(expectedIsCompiled, (parseResult.Regex.Options & RegexOptions.Compiled) != 0);
    }

    private static Tokenizer.RegExpParser CreateRegExpParser(string pattern, string flags, TokenizerOptions tokenizerOptions)
    {
        var tokenizer = new Tokenizer(string.Empty, tokenizerOptions);
        var regExpParser = tokenizer._regExpParser ??= new Tokenizer.RegExpParser(tokenizer);
        regExpParser.Reset(pattern, patternStartIndex: 0, flags, flagsStartIndex: 0);
        return regExpParser;
    }
}
