using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Acornima.Ast;
using Acornima.Tests.Helpers;
using Xunit;

namespace Acornima.Tests;

#pragma warning disable CS0618 // Type or member is obsolete

public partial class RegExpTests : IClassFixture<RegExpTests.SharedContextFixture>
{
    private readonly SharedContextFixture _fixture;

    public RegExpTests(SharedContextFixture fixture)
    {
        _fixture = fixture;
    }

    public static IEnumerable<object[]> TestCases(string relativePath)
    {
        var fixturesPath = Path.Combine(ParserTests.GetFixturesPath(), relativePath);
        var testCasesFilePath = Path.Combine(fixturesPath, "testcases.txt");

        if (!File.Exists(testCasesFilePath))
        {
            yield break;
        }

        using var reader = new StreamReader(testCasesFilePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split(new[] { '\t' }, StringSplitOptions.None);
            if (parts.Length >= 5)
            {
                Array.Resize(ref parts, 6);
            }

            yield return parts;
        }
    }

    [Theory]
    [MemberData(nameof(TestCases), "Fixtures.RegExp")]
    public void ExecuteTestCase(string pattern, string flags, string expectedAdaptedPattern, string testString, string expectedMatchesJson, string hints)
    {
        // When upgrading .NET runtime version, it's expected that some of the tests may fail because the regexp rewriting logic
        // uses some Unicode-related APIs provided by .NET and the underlying Unicode datasets may be updated between .NET versions.
        // So, in the case of failing tests, try to re-generate the test cases first.
        // To re-generate test cases, execute `dotnet run --project Fixtures.RegExp\Generator -c Release`
        // (Make sure that Node.js 24+ is installed.)

        static string DecodeStringIfEscaped(string value) => JavaScriptString.IsStringLiteral(value)
            ? JavaScriptString.Decode(value)
            : value;

        pattern = DecodeStringIfEscaped(pattern);
        flags = DecodeStringIfEscaped(flags);
        expectedAdaptedPattern = DecodeStringIfEscaped(expectedAdaptedPattern);
        testString = DecodeStringIfEscaped(testString);
        var hintArray = !string.IsNullOrEmpty(hints)
            ? hints.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        var skipValidationTest = hintArray.Contains("!skip-validation");

        //var skipConversionTest = hintArray.Contains("!skip")
        //#if !NET10_0_OR_GREATER
        //        || hintArray.Contains("!skip-before-net10.0")
        //#endif
        //#if NETFRAMEWORK
        //        || hintArray.Contains("!skip-on-netframework")
        //#endif
        //    ;

        // Both Newtonsoft.Json and System.Text.Json mess up lone surrogates,
        // so we need to parse the JSON containing the matches "manually"...
        var (expectedMatches, syntaxError) = RegExpMatch.MatchesFrom(JavaScriptString.ParseAsExpression(expectedMatchesJson));

        var regExpValidator = _fixture.RegExpValidator;
        regExpValidator.Reset(pattern, patternStartIndex: 0, flags, flagsStartIndex: 0);

        //var regExpConverter = expectedMatches is not null ? _fixture.RegExpConverterTolerant : _fixture.RegExpConverterNonTolerant;
        //regExpConverter.Reset(pattern, patternStartIndex: 0, flags, flagsStartIndex: 0);

        if (expectedMatches is not null)
        {
            if (!skipValidationTest)
            {
                regExpValidator.Validate();
            }

            //if (!skipConversionTest)
            //{
            //    var parseResult = regExpConverter.Parse();
            //    if (expectedAdaptedPattern != ")inconvertible(")
            //    {
            //        Assert.True(parseResult.Success);
            //        Assert.NotNull(parseResult.Regex);

            //        var actualAdaptedPattern = parseResult.Regex.ToString();
            //        Assert.Equal(expectedAdaptedPattern, actualAdaptedPattern);

            //        var actualMatchEnumerable = parseResult.Regex.Matches(testString).Cast<Match>();

            //        // In unicode mode, we can't prevent empty matches within surrogate pairs currently,
            //        // so we need to remove such matches from the match collection to make assertions pass.
            //        var actualMatches = flags.IndexOf('u') >= 0
            //            ? actualMatchEnumerable
            //                .Cast<Match>()
            //                .Where(m => m.Length != 0
            //                    || m.Index == 0 || m.Index == testString.Length
            //                    || !(char.IsHighSurrogate(testString[m.Index - 1]) && char.IsLowSurrogate(testString[m.Index])))
            //                .ToArray()
            //            : actualMatchEnumerable.ToArray();

            //        Assert.Equal(expectedMatches.Length, actualMatches.Length);

            //        for (var i = 0; i < actualMatches.Length; i++)
            //        {
            //            var actualMatch = actualMatches[i];
            //            var expectedMatch = expectedMatches[i];

            //            Assert.Equal(expectedMatch.Index, actualMatch.Index);
            //            Assert.Equal(expectedMatch.Captures.Length, parseResult.ActualRegexGroupCount);
            //            Assert.True(expectedMatch.Captures.Length <= actualMatch.Groups.Count);

            //            var ignoreGroupCaptures = hintArray.Contains("!ignore-group-captures");
            //            var captureCount = !ignoreGroupCaptures ? expectedMatch.Captures.Length : 1;

            //            for (var j = 0; j < captureCount; j++)
            //            {
            //                var actualGroup = actualMatch.Groups[j];
            //                var expectedCapture = expectedMatch.Captures[j];

            //                #if NET6_0_OR_GREATER
            //                var actualGroupName = actualGroup.Name;
            //                #else
            //                var actualGroupName = parseResult.Regex.GetGroupNames()[j];
            //                #endif
            //                Assert.True(int.TryParse(actualGroupName, NumberStyles.None, CultureInfo.InvariantCulture, out var actualGroupIndex));
            //                Assert.Equal(j, actualGroupIndex);

            //                if (expectedCapture is not null)
            //                {
            //                    Assert.True(actualGroup.Success);
            //                    Assert.Equal(expectedCapture, actualGroup.Value);
            //                }
            //                else if (!hintArray.Contains("!ignore-undefined-captures"))
            //                {
            //                    Assert.False(actualGroup.Success);
            //                }
            //            }

            //            if (!ignoreGroupCaptures && expectedMatch.Groups is not null)
            //            {
            //                foreach (var kvp in expectedMatch.Groups)
            //                {
            //                    var actualGroup = actualMatch.Groups[kvp.Key];
            //                    if (!actualGroup.Success)
            //                    {
            //                        actualGroup = actualMatch.Groups[Tokenizer.RegExpParser.EncodeGroupName(kvp.Key)];
            //                    }

            //                    Assert.True(actualGroup.Success);
            //                    Assert.Equal(kvp.Value, actualGroup.Value);
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Assert.False(parseResult.Success);
            //        Assert.Null(parseResult.Regex);
            //        Assert.NotNull(parseResult.ConversionError);
            //    }
            //}
        }
        else
        {
            ParseErrorException ex;

            if (!skipValidationTest)
            {
                ex = Assert.Throws<SyntaxErrorException>(() => regExpValidator.Validate());

                if (!hintArray.Contains("!ignore-error-message"))
                {
                    Assert.Equal($"Invalid regular expression: /{pattern}/{flags}: {syntaxError}", ex.Error?.Description);
                }
            }

            //if (!skipConversionTest)
            //{
            //    ex = Assert.ThrowsAny<ParseErrorException>(() => regExpConverter.Parse());

            //    if (expectedAdaptedPattern != ")inconvertible(")
            //    {
            //        Assert.IsType<SyntaxErrorException>(ex);
            //        if (!hintArray.Contains("!ignore-error-message"))
            //        {
            //            Assert.Equal($"Invalid regular expression: /{pattern}/{flags}: {syntaxError}", ex.Error?.Description);
            //        }
            //    }
            //    else
            //    {
            //        Assert.IsType<RegExpConversionErrorException>(ex);
            //        Assert.StartsWith("Cannot convert regular expression", ex.Error?.Description, StringComparison.Ordinal);
            //    }
            //}
        }
    }

    private sealed record class RegExpMatch(string[] Captures, int Index, Dictionary<string, string>? Groups)
    {
        public static (RegExpMatch[]?, string?) MatchesFrom(Expression expression)
        {
            // This parser logic must align with the shape returned by generate-matches.js.

            if (expression is StringLiteral literal)
            {
                return (null, literal.Value);
            }

            return (expression.As<ArrayExpression>().Elements
                .Select(el => MatchFrom(el!.As<ObjectExpression>()))
                .ToArray(), null);
        }

        public static RegExpMatch MatchFrom(ObjectExpression expression)
        {
            string[]? captures = null;
            int? index = null;
            Dictionary<string, string>? groups = null;

            foreach (var property in expression.Properties.Cast<Property>())
            {
                switch (property.Key.As<StringLiteral>().Value)
                {
                    case "captures":
                        captures = property.Value.As<ArrayExpression>().Elements
                            .Select(el => (string)el!.As<Literal>().Value!)
                            .ToArray();
                        break;
                    case "index":
                        index = checked((int)property.Value.As<NumericLiteral>().Value);
                        break;
                    case "groups":
                        groups = property.Value.As<ObjectExpression>().Properties
                            .Cast<Property>()
                            .ToDictionary(p => p.Key.As<StringLiteral>().Value, p => (string)p.Value.As<Literal>().Value!);
                        break;
                }
            }

            return new RegExpMatch(
                captures ?? throw new FormatException(),
                index ?? throw new FormatException(),
                groups);
        }
    }

    public sealed class SharedContextFixture : IDisposable
    {
        private readonly Tokenizer _tokenizerForRegExpValidator;
        //private readonly Tokenizer _tokenizerForNonTolerantRegExpConverter;
        //private readonly Tokenizer _tokenizerForTolerantRegExpConverter;

        public SharedContextFixture()
        {
            _tokenizerForRegExpValidator = new Tokenizer(string.Empty);
            //_tokenizerForNonTolerantRegExpConverter = new Tokenizer(string.Empty, new TokenizerOptions { RegExpParseMode = RegExpParseMode.AdaptToInterpreted, Tolerant = false });
            //_tokenizerForTolerantRegExpConverter = new Tokenizer(string.Empty, new TokenizerOptions { RegExpParseMode = RegExpParseMode.AdaptToInterpreted, Tolerant = true });
        }

        public void Dispose()
        {
            _tokenizerForRegExpValidator.ReleaseLargeBuffersForRegExpParser();
            //_tokenizerForNonTolerantRegExpConverter.ReleaseLargeBuffersForRegExpParser();
            //_tokenizerForTolerantRegExpConverter.ReleaseLargeBuffersForRegExpParser();
        }

        internal Tokenizer.RegExpParser RegExpValidator => _tokenizerForRegExpValidator._regExpParser ??= new(_tokenizerForRegExpValidator);
        //internal Tokenizer.RegExpParser RegExpConverterNonTolerant => _tokenizerForNonTolerantRegExpConverter._regExpParser ??= new(_tokenizerForNonTolerantRegExpConverter);
        //internal Tokenizer.RegExpParser RegExpConverterTolerant => _tokenizerForTolerantRegExpConverter._regExpParser ??= new(_tokenizerForTolerantRegExpConverter);
    }
}
